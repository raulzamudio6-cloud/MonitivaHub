package smssenders

import (
	"bytes"
	"encoding/json"
	"fmt"
	"net/http"
)

type VertexSender struct {
	parser paramsParser

	name     string
	priority int

	url        string
	token      string
	senderName string
}

func NewVertexSender(priority int) *VertexSender {
	res := &VertexSender{
		name:     "vertex",
		priority: priority,
	}
	return res
}

func (p *VertexSender) GetPriority() int {
	return p.priority
}

func (p *VertexSender) Init(params map[string]string, priority int) error {
	var err error
	// URL
	p.url, err = p.parser.Parse(p.name, params, "URL")
	if err != nil {
		return err
	}
	// token
	p.token, err = p.parser.Parse(p.name, params, "token")
	if err != nil {
		return err
	}
	// senderName
	p.senderName, err = p.parser.Parse(p.name, params, "SenderName")
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
func (p *VertexSender) SendMessage(client *http.Client, to string, message string, whatsapp bool) error {
	// подготовим реквест
	msgBuff, err := json.Marshal(map[string]string{"from": p.senderName, "to": to, "message": message})
	if err != nil {
		return err
	}
	req, err := http.NewRequest("POST", p.url, bytes.NewBuffer(msgBuff))
	if err != nil {
		return err
	}
	req.Header.Add("Content-Type", "application/json; charset=utf-8")
	req.Header.Add("X-VertexSMS-Token", p.token)
	req.ContentLength = int64(len(msgBuff))
	// отправка
	resp, err := client.Do(req)
	if err != nil {
		return err
	}
	defer resp.Body.Close()
	// парсим результат
	var res map[string]interface{}

	json.NewDecoder(resp.Body).Decode(&res)

	fmt.Println(resp.StatusCode)

	fmt.Println(res)

	return nil
}
