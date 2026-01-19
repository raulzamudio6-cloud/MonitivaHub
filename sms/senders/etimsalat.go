package smssenders

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"strings"
	"time"

	"gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/stdlogger"
	"go.uber.org/zap"
)

var (
	logger *zap.Logger = stdlogger.Logger
)

type EtimsalatSender struct {
	parser paramsParser

	name     string
	priority int

	url string

	api_key    string
	api_secret string

	senderName string

	token         string
	tokenTime     time.Time
	tokenDuration time.Duration
}

func NewEtimsalatSender(priority int) *EtimsalatSender {
	res := &EtimsalatSender{
		name:     "etimsalat",
		priority: priority,
	}
	return res
}

func (p *EtimsalatSender) GetPriority() int {
	return p.priority
}

func (p *EtimsalatSender) Init(params map[string]string, priority int) error {
	var err error
	// URL
	p.url, err = p.parser.Parse(p.name, params, "URL")
	if err != nil {
		return err
	}
	p.api_key, err = p.parser.Parse(p.name, params, "api_key")
	if err != nil {
		return err
	}
	p.api_secret, err = p.parser.Parse(p.name, params, "api_secret")
	if err != nil {
		return err
	}
	p.senderName, err = p.parser.Parse(p.name, params, "SenderName")
	if err != nil {
		return err
	}
	token_timeout, err := p.parser.Parse(p.name, params, "token_timeout")
	if err != nil {
		return err
	}
	p.tokenDuration, err = time.ParseDuration(token_timeout)
	if err != nil {
		logger.Warn(fmt.Sprintf("Can't parse REQUEST_TIMEOUT: %s", err.Error()))
		p.tokenDuration = time.Duration(10) * time.Second
	}

	p.tokenTime = time.Now().Add(-24 * time.Hour)

	return nil
}

func (p *EtimsalatSender) login(client *http.Client) error {
	// конвертируем message в utf-16
	// enc := unicode.UTF16(unicode.LittleEndian, unicode.UseBOM).NewEncoder()
	// utf16msg, err := enc.String(message)
	// if err != nil {
	// 	return err
	// }

	// подготовим реквест
	msgBuff, err := json.Marshal(map[string]string{"username": p.api_key, "password": p.api_secret})
	if err != nil {
		return err
	}
	fmt.Println(string(msgBuff))
	req, err := http.NewRequest("POST", p.url+"/login/user", bytes.NewBuffer(msgBuff))
	if err != nil {
		return err
	}
	req.Header.Add("Content-Type", "application/json; charset=utf-8")
	req.ContentLength = int64(len(msgBuff))
	// отправка
	resp, err := client.Do(req)
	if err != nil {
		return err
	}

	defer resp.Body.Close()
	// парсим результат
	var res map[string]interface{}
	bytes, err := io.ReadAll(resp.Body)
	//fmt.Println(resp.StatusCode)
	//fmt.Println(string(bytes))
	json.Unmarshal(bytes, &res)
	//fmt.Println(res)
	logger.Debug(fmt.Sprintf("login resp: %s", string(bytes)))
	//fmt.Println(res["token"])

	p.token = fmt.Sprintf("%s", res["token"])

	p.tokenTime = time.Now()

	return nil
}

/*
	отправка сообщения
		sender  - имя отправителя
		to      - номер телефона получателя
		message - содержимое сообщения
*/
func (p *EtimsalatSender) SendMessage(client *http.Client, to string, message string, whatsapp bool) error {

	if "" == p.token || time.Now().After(p.tokenTime.Add(p.tokenDuration)) {
		p.login(client)
	}

	if "" == p.token {
		return fmt.Errorf("token is empty")
	}
	// конвертируем message в utf-16
	// enc := unicode.UTF16(unicode.LittleEndian, unicode.UseBOM).NewEncoder()
	// utf16msg, err := enc.String(message)
	// if err != nil {
	// 	return err
	// }

	to = strings.Replace(to, "+", "", -1)

	// подготовим реквест
	msgBuff, err := json.Marshal(map[string]string{
		"desc":         "d",
		"campaignName": "cn",
		"msgCategory":  "4.5",
		"senderAddr":   p.senderName,
		"dr":           "0",
		"clientTxnId":  "123",
		"recipient":    to,
		"msg":          message})
	if err != nil {
		return err
	}

	fmt.Println("req:" + string(msgBuff))

	req, err := http.NewRequest("POST", p.url, bytes.NewBuffer(msgBuff))
	if err != nil {
		return err
	}
	req.Header.Add("Content-Type", "application/json; charset=utf-8")
	req.Header.Add("Authorization", "bearer "+p.token)

	
	

	req.ContentLength = int64(len(msgBuff))
	// отправка
	resp, err := client.Do(req)
	if err != nil {
		return err
	}
	defer resp.Body.Close()
	// парсим результат
	var res map[string]interface{}
	bytes, err := io.ReadAll(resp.Body)
	if err != nil {
		return err
	}
	//fmt.Println(resp.StatusCode)
	logger.Debug(fmt.Sprintf("sms send resp: %s", string(bytes)))
	err = json.Unmarshal(bytes, &res)
	if err != nil {
		return err
	}

	logger.Debug(fmt.Sprintf("jobId:%s", res["jobId"]))

	if res["jobId"] == "" || res["jobId"] == nil {
		logger.Error("no jobId")
		return fmt.Errorf("error:%s", string(bytes))
	}

	return nil
}
