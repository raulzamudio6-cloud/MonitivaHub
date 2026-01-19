package smssenders

import (
	"encoding/json"
	"fmt"
	"net/http"
	"net/url"

	"golang.org/x/text/encoding/unicode"
)

type ClickatellSender struct {
	parser paramsParser

	name     string
	priority int

	url string

	apiID      string
	username   string
	password   string
	senderName string
}

func NewClickatellSender(priority int) *ClickatellSender {
	res := &ClickatellSender{
		name:     "clickatell",
		priority: priority,
	}
	return res
}

func (p *ClickatellSender) GetPriority() int {
	return p.priority
}

func (p *ClickatellSender) Init(params map[string]string, priority int) error {
	var err error
	// URL
	p.url, err = p.parser.Parse(p.name, params, "URL")
	if err != nil {
		return err
	}
	// api_id
	p.apiID, err = p.parser.Parse(p.name, params, "APIID")
	if err != nil {
		return err
	}
	// username
	p.username, err = p.parser.Parse(p.name, params, "Username")
	if err != nil {
		return err
	}
	// password
	p.password, err = p.parser.Parse(p.name, params, "Password")
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
func (p *ClickatellSender) SendMessage(client *http.Client, to string, message string, whatsapp bool) error {
	// конвертируем message в utf-16
	enc := unicode.UTF16(unicode.LittleEndian, unicode.UseBOM).NewEncoder()
	utf16msg, err := enc.String(message)
	if err != nil {
		return err
	}
	data := url.Values{}
	data.Set("api_id", p.apiID)
	data.Set("user", p.username)
	data.Set("password", p.password)
	data.Set("to", to)
	data.Set("text", utf16msg)
	data.Set("from", p.senderName)
	data.Set("unicode", "1")
	data.Set("S", "H")

	req, err := http.NewRequest(http.MethodGet, fmt.Sprintf("%s?%s", p.url, data.Encode()), nil)
	if err != nil {
		return err
	}
	req.Header.Add("Content-Type", "application/x-www-form-urlencoded")
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
