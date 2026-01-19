package smssenders

import (
	"encoding/json"
	"fmt"
	"io/ioutil"
	"log"
	"net/http"
	"net/url"
)

type WebSmsSender struct {
	parser paramsParser

	name     string
	priority int

	url           string
	http_username string
	http_password string
	fromPhone     string
}

type WebSmsResponse struct {
	Error string `json:"error"`
	//ErrNum        int    `json:"err_num"`
	PacketID      string `json:"packet_id"`
	BalanceBefore string `json:"balance_before"`
	SenderIP      string `json:"sender_ip"`
	SummPhone     string `json:"summ_phone"`
	SummParts     string `json:"summ_parts"`
	PacketCost    string `json:"packet_cost"`
	BalanceAfter  string `json:"balance_after"`
	Sms           []struct {
		MessageID    string `json:"message_id"`
		MessagePhone string `json:"message_phone"`
		MessageParts string `json:"message_parts"`
		MessageZone  string `json:"message_zone"`
		MessageCost  string `json:"message_cost"`
	} `json:"sms"`
}

func NewWebSmsSender(priority int) *WebSmsSender {
	res := &WebSmsSender{
		name:     "websms",
		priority: priority,
	}
	return res
}

func (p *WebSmsSender) GetPriority() int {
	return p.priority
}

func (p *WebSmsSender) Init(params map[string]string, priority int) error {
	var err error
	p.url, err = p.parser.Parse(p.name, params, "URL")
	if err != nil {
		return err
	}

	p.http_username, err = p.parser.Parse(p.name, params, "http_username")
	if err != nil {
		return err
	}

	p.http_password, err = p.parser.Parse(p.name, params, "http_password")
	if err != nil {
		return err
	}

	p.fromPhone, err = p.parser.Parse(p.name, params, "fromPhone")
	if err != nil {
		return err
	}
	return nil
}

/*
отправка сообщения

	sender  - имя отправителя
	to      - номер телефона получателя
	message - содержимое сообщения
*/
func (p *WebSmsSender) SendMessage(client *http.Client, to string, message string, whatsapp bool) error {
	// подготовим реквест
	msgBuff, err := json.Marshal(map[string]string{"http_username": p.http_username, "http_password": p.http_password,
		"message": message, "fromPhone": p.fromPhone, "phone_list": to})
	if err != nil {
		return err
	}
	req, err := http.NewRequest("POST", p.url, nil)
	req.URL.RawQuery = url.Values{
		"json": {string(msgBuff)},
	}.Encode()
	fmt.Println(req.Host)
	req.Header.Add("Host", req.Host)
	req.ContentLength = 0
	if err != nil {
		return err
	}
	// отправка
	resp, err := client.Do(req)
	if err != nil {
		return err
	}

	responseData, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		log.Fatal(err)
	}

	defer resp.Body.Close()

	var response WebSmsResponse

	err = json.Unmarshal(responseData, &response)
	if err != nil {
		logger.Error(fmt.Sprintf("Error occured on parsing websms response: %s", string(responseData[:])))
		return err
	}

	if response.Error != "OK" {
		return fmt.Errorf("error=%s", response.Error)
	}

	return nil
}
