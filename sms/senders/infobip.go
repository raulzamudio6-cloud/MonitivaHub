package smssenders

import (
	"bytes"
	"encoding/json"
	"errors"
	"fmt"
	"io"
	"net/http"
)

type InfoBipSender struct {
	parser paramsParser

	name     string
	priority int

	apiUrl     string
	token      string
	senderName string
}

func NewInfoBipSender(priority int) *InfoBipSender {
	res := &InfoBipSender{
		name:     "infobip",
		priority: priority,
	}
	return res
}

func (sender *InfoBipSender) GetPriority() int {
	return sender.priority
}

func (sender *InfoBipSender) Init(params map[string]string, priority int) error {
	// FIXME unused and duplicated param "priority"

	var err error

	sender.apiUrl, err = sender.parser.Parse(sender.name, params, "URL")
	if err != nil {
		return err
	}

	sender.token, err = sender.parser.Parse(sender.name, params, "api_secret")
	if err != nil {
		return err
	}

	sender.senderName, err = sender.parser.Parse(sender.name, params, "SenderName")
	if err != nil {
		return err
	}

	return nil
}

func (sender *InfoBipSender) SendMessage(client *http.Client, to string, message string, whatsapp bool) error {
	/*
		Docs: https://www.infobip.com/docs/api/channels/sms/outbound-sms/send-sms-messages
	*/

	if whatsapp {
		return errors.New("whatsapp is not supported yet")
	}

	messageObj := map[string]interface{}{
		"sender": sender.senderName,
		"destinations": []map[string]string{
			{"to": to},
		},
		"content": map[string]string{
			"text": message,
		},
	}

	payload := map[string]interface{}{
		"messages": []map[string]interface{}{messageObj},
	}

	jsonData, error := json.Marshal(payload)
	if error != nil {
		return error
	}

	payloadReader := bytes.NewReader(jsonData)

	request, error := http.NewRequest("POST", sender.apiUrl, payloadReader)

	if error != nil {
		return error
	}

	request.Header.Add("Authorization", "App "+sender.token)
	request.Header.Add("Content-Type", "application/json")
	request.Header.Add("Accept", "application/json")

	response, error := client.Do(request)
	if error != nil {
		return error
	}
	defer response.Body.Close()

	body, error := io.ReadAll(response.Body)
	if error != nil {
		fmt.Println("Response reading error: ", error, "response: ", body)
		return error
	}

	return nil
}
