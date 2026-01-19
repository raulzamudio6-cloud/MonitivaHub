package smssenders

import (
	"encoding/xml"
	"fmt"
	"io"
	"log"
	"net/http"
	"net/url"
	"strings"
)

type NexmoSender struct {
	parser paramsParser

	name     string
	priority int

	url string

	api_key    string
	api_secret string

	senderName   string
	senderName1 string
}

func NewNexmoSender(priority int) *NexmoSender {
	res := &NexmoSender{
		name:     "nexmo",
		priority: priority,
	}
	return res
}

func (p *NexmoSender) GetPriority() int {
	return p.priority
}

func (p *NexmoSender) Init(params map[string]string, priority int) error {
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

	p.senderName1, err = p.parser.Parse(p.name, params, "SenderName1")
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
func (p *NexmoSender) SendMessage(client *http.Client, to string, message string, whatsapp bool) error {
	// конвертируем message в utf-16
	// enc := unicode.UTF16(unicode.LittleEndian, unicode.UseBOM).NewEncoder()
	// utf16msg, err := enc.String(message)
	// if err != nil {
	// 	return err
	// }

	data := url.Values{}
	data.Set("api_key", p.api_key)
	data.Set("api_secret", p.api_secret)
	data.Set("to", to)
	if strings.HasPrefix(to, "+1") {
		data.Set("from", p.senderName1)
	} else {
		data.Set("from", p.senderName)
	}
	data.Set("text", message)
	data.Set("type", "unicode")

	//fmt.Println(fmt.Sprintf("request: %s?%s", p.url, data.Encode()))

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

	type Message struct {
		Status    string `xml:"status"`
		ErrorText string `xml:"errorText"`
	}

	// buf := new(strings.Builder)
	// n, err := io.Copy(buf, resp.Body)
	// fmt.Println(n)
	// fmt.Println(buf.String())

	// check errors

	d := xml.NewDecoder(resp.Body)
	for {
		tok, err := d.Token()
		if tok == nil || err == io.EOF {
			// EOF means we're done.
			break
		} else if err != nil {
			log.Fatalf("Error decoding token: %s", err)
		}

		switch ty := tok.(type) {
		case xml.StartElement:
			if ty.Name.Local == "message" {
				// If this is a start element named "location", parse this element
				// fully.
				var msg Message
				if err = d.DecodeElement(&msg, &ty); err != nil {
					log.Fatalf("Error decoding item: %s", err)
				}
				if msg.Status != "0" {
					return fmt.Errorf("status=%s msg=%s", msg.Status, msg.ErrorText)
				} else {
					return nil
				}
			}
		default:
		}
	}

	buf := new(strings.Builder)
	n, err := io.Copy(buf, resp.Body)
	return fmt.Errorf("unknown response=%s", buf.String(), n)

	return nil
}
