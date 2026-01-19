package smssenders

import (
	"encoding/json"
	"fmt"
	"net/http"
	"net/url"

	"golang.org/x/text/encoding/unicode"
)

type CardBoardFishSender struct {
	parser paramsParser

	name     string
	priority int

	url string

	username   string
	password   string
	hdc        string
	senderName string
}

func NewCardBoardFishSender(priority int) *CardBoardFishSender {
	res := &CardBoardFishSender{
		name:     "cardboardfish",
		priority: priority,
	}
	return res
}

func (p *CardBoardFishSender) GetPriority() int {
	return p.priority
}

func (p *CardBoardFishSender) Init(params map[string]string, priority int) error {
	var err error
	// URL
	p.url, err = p.parser.Parse(p.name, params, "URL")
	if err != nil {
		return err
	}
	// hdc
	p.hdc, err = p.parser.Parse(p.name, params, "hdc")
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
func (p *CardBoardFishSender) SendMessage(client *http.Client, to string, message string, whatsapp bool) error {
	// конвертируем message в utf-16
	enc := unicode.UTF16(unicode.LittleEndian, unicode.UseBOM).NewEncoder()
	utf16msg, err := enc.String(message)
	if err != nil {
		return err
	}

	data := url.Values{}
	data.Set("DC", p.hdc)
	data.Set("UN", p.username)
	data.Set("P", p.password)
	data.Set("DA", to)
	data.Set("M", utf16msg)
	data.Set("SA", p.senderName)
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
