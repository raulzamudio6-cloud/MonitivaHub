package main

import (
	"fmt"
	"io/ioutil"
	"net/http"
	"os"
	"os/signal"
	"strings"
	"syscall"

	"github.com/streadway/amqp"
	"gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/rabbitworker"
	"gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/stdlogger"
	"go.uber.org/zap"
)

var (
	logger         *zap.Logger = stdlogger.Logger
	rw             *rabbitworker.Worker
	cfg            *ServiceConfig
	listenChannels map[string]string
)

func main() {
	defer logger.Sync()

	cfg := &ServiceConfig{}

	// читаем конфиг
	err := loadConfig(cfg)
	if err != nil {
		logger.Error(fmt.Sprintf("loading service config... error: %s", err.Error()))
		return
	}
	logger.Info("loading service config... OK")

	listenChannels = make(map[string]string)
	for _, lc := range strings.Split(cfg.ListenChannels, ",") {
		l_c := strings.Split(lc, "=")
		if len(l_c) == 1 {
			if strings.Trim(l_c[0], " ") != "" {
				listenChannels[strings.Trim(l_c[0], " ")] = strings.Trim(l_c[0], " ")
			}
		}
		if len(l_c) == 2 {
			listenChannels[strings.Trim(l_c[0], " ")] = strings.Trim(l_c[1], " ")
		}
	}
	if len(listenChannels) == 0 {
		logger.Error(fmt.Sprintf("No channels to listen, specify some in  LISTEN_CHANNELS env var"))
		return
	}

	logger.Info("Starting webhooks ")

	// Запуск слушателя RabbitMQ
	rw, err = rabbitworker.NewWorker("webhooks")
	if err != nil {
		logger.Error(fmt.Sprintf("starting AMQP worker... error: %s", err.Error()))
		return
	}
	defer rw.Close()

	// start http listener
	for url, channel := range listenChannels {
		logger.Info(fmt.Sprintf("Mapped %s to %s", "/"+url, channel))
		http.HandleFunc("/"+url, processRequestWU)

	}

	go http.ListenAndServe(":8004", nil)
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

}

func processRequestWU(w http.ResponseWriter, req *http.Request) {

	if req.Method == "POST" {

		buf, err := ioutil.ReadAll(req.Body)
		if err != nil {
			m := fmt.Sprintf("reading POST body error: %s", err.Error())
			logger.Warn(m)
			fmt.Fprintln(w, m)
			return
		}

		i := len(buf)
		suff := ""
		if i > 256 {
			i = 256
			suff = "..."
		}
		s := string(buf[:i])

		lastPath := req.URL.Path
		k := strings.LastIndex(req.URL.Path, "/")
		if k != -1 {
			lastPath = lastPath[k+1:]
		}
		channel, urlMapped := listenChannels[lastPath]

		if urlMapped {
			logger.Sugar().Infof("publishing to %s bytes=%d, req=%s %s", channel, len(buf), s, suff)

			rw.SendTask(channel, 0, `{action:"http_request"}`, buf)
			//fmt.Printf("respond1 val=%s que=s\r\n", cfg)
			//if cfg.AMQP.OUT.QName != "" {
			rw.Respond(0, 0, channel, buf)
			//}
		}
	}

}
