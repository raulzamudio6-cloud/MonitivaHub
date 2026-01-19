package main

import (
	"encoding/json"
	"fmt"
	"io/ioutil"
	"net/http"
	"os"
	"os/signal"
	"sync"
	"syscall"

	"github.com/streadway/amqp"
	"gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/rabbitworker"
	"gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/stdlogger"
	"go.uber.org/zap"
)

var (
	logger      *zap.Logger = stdlogger.Logger
	rw          *rabbitworker.Worker
	quotes      map[string]string = map[string]string{}
	requests    map[string]int    = map[string]int{}
	mux         sync.Mutex
	jobSequence int = 1
)

type QuoteRequest struct {
	SessionId    string  `json:"sessionId"`
	JobSeq       int     `json:"jobSeq"`
	SellCurrency string  `json:"sellCurrency"`
	BuyCurrency  string  `json:"buyCurrency"`
	Amount       float64 `json:"amount"`
	IsSellAmount bool    `json:"isSellAmount"`
}

type Quote struct {
	SellCurrency            string       `json:"sellCurrency"`
	SellAmount              float64      `json:"sellAmount"`
	BuyCurrency             string       `json:"buyCurrency"`
	BuyAmount               float64      `json:"buyAmount"`
	Rate                    float64      `json:"rate"`
	RateInverted            float64      `json:"rateInverted"`
	IsDirectRate            bool         `json:"isDirectRate"`
	ExpirationIntervalInSec int          `json:"expirationIntervalInSec"`
	CreatedOn               string       `json:"createdOn"`
	MarkupPercent           string       `json:"markupPercent"`
	RateRef                 string       `json:"rateRef"`
	OriginalRequest         QuoteRequest `json:"original-request"`
}

func main() {
	defer logger.Sync()

	logger.Info("Starting rates_rest_api")

	cfg := &ServiceConfig{}

	// читаем конфиг
	err := loadConfig(cfg)
	if err != nil {
		logger.Error(fmt.Sprintf("loading service config... error: %s", err.Error()))
		return
	}
	logger.Info("loading service config... OK")

	// Запуск слушателя RabbitMQ
	rw, err = rabbitworker.NewWorker("rates_api")
	if err != nil {
		logger.Error(fmt.Sprintf("starting AMQP worker... error: %s", err.Error()))
		return
	}
	defer rw.Close()

	// start http listener
	http.HandleFunc("/rates_api", processRequest)

	go http.ListenAndServe(":8090", nil)
	logger.Info("ListenAndServe... OK")

	go func() {
		for m := range rw.GetInputMsgChan() {
			m.Ack(false)
			processMsg(&m)
		}
	}()

	// канал для прерывания работы
	shutdown := make(chan os.Signal, 1)
	signal.Notify(shutdown, os.Interrupt, syscall.SIGINT, syscall.SIGTERM)

	select {
	case <-shutdown:
	case <-rw.CloseCh:
		logger.Debug("AMQP Closed")
	}
}

func processMsg(msg *amqp.Delivery) {
	mux.Lock()
	defer mux.Unlock()

	body := string(msg.Body)

	// fmt.Printf("body:" + body)

	quo := &Quote{}

	err := json.Unmarshal(msg.Body, quo)
	if err != nil {
		logger.Error(fmt.Sprintf("body decode error: %s", err.Error()))
		return
	}

	//fmt.Printf("%+v", quo)

	jobId := GenJobId(&quo.OriginalRequest)
	logger.Debug(fmt.Sprintf("body for jobId: %s  total:%d body: %s", jobId, len(quotes), body))

	quotes[jobId] = body
}

func processRequest(w http.ResponseWriter, req *http.Request) {

	w.Header().Set("Access-Control-Allow-Origin", "*")
	w.Header().Set("Access-Control-Allow-Methods", "GET,HEAD,OPTIONS,POST,PUT")
	w.Header().Set("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, content-type, Accept, Authorization")

	if req.Method == "GET" {

		jobId := req.URL.Query().Get("jobId")

		mux.Lock()
		defer mux.Unlock()

		if val, ok := quotes[jobId]; ok {
			fmt.Fprintln(w, val)
			delete(quotes, jobId)
			logger.Debug(fmt.Sprintf("pop jobId: %s  total:%d", jobId, len(quotes)))
		} else {
			fmt.Fprintln(w, "")
		}

	} else if req.Method == "POST" {

		buf, err := ioutil.ReadAll(req.Body)
		if err != nil {
			m := fmt.Sprintf("reading POST body error: %s", err.Error())
			logger.Warn(m)
			fmt.Fprintln(w, m)
		} else {
			qr := &QuoteRequest{}

			logger.Debug(`rate request: ` + string(buf))

			err := json.Unmarshal(buf, qr)
			if err != nil {
				m := fmt.Sprintf("body decode error: %s", err.Error())
				logger.Warn(m)
				fmt.Fprintln(w, m)
				return
			}

			qr.JobSeq = jobSequence
			jobSequence++

			jobId := GenJobId(qr)

			requests[jobId] = 0
			w.WriteHeader(http.StatusAccepted)
			fmt.Fprintf(w, `{"jobId":"%s","eta":1000}`, jobId)

			buf, err := json.Marshal(qr)
			if err != nil {
				logger.Error(fmt.Sprintf("Can't format request: %s", err.Error()))
				return
			}

			logger.Debug(fmt.Sprintf("req for job: %s", jobId))
			rw.SendTask("", 0, `{action:"rateQuote"}`, buf)
		}
	} else if req.Method == "OPTIONS" {
	}

}

func GenJobId(rq *QuoteRequest) string {
	i := rq.JobSeq % 10
	return fmt.Sprintf("%s_%d", rq.SessionId[i+1:i+5], rq.JobSeq)
}
