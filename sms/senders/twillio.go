package smssenders

import (
	"fmt"
	"net/http"
	"github.com/twilio/twilio-go"
	api "github.com/twilio/twilio-go/rest/api/v2010"
)

type TwilioSender struct {
	parser paramsParser

	name     string
	url   string
	priority int


	// for sms
	sid   string
	token string
	// for whatsapp
	messagingServiceSid string
	contentSid          string
}

func NewTwilioSender(priority int) *TwilioSender {
	res := &TwilioSender{
		name:     "twilio",
		priority: priority,
	}
	return res
}

func (p *TwilioSender) GetPriority() int {
	return p.priority
}

func (p *TwilioSender) Init(params map[string]string, priority int) error {
	var err error
	p.token, err = p.parser.Parse(p.name, params, "token")
	if err != nil {
		return err
	}
	p.sid, err = p.parser.Parse(p.name, params, "sid")
	if err != nil {
		return err
	}
	p.messagingServiceSid, err = p.parser.Parse(p.name, params, "messagingServiceSid")
	if err != nil {
		return err
	}
	p.contentSid, err = p.parser.Parse(p.name, params, "contentSid")
	if err != nil {
		return err
	}

	return nil
}


func (p *TwilioSender) SendMessage(client *http.Client, to string, message string, whatsapp bool) error {
	clientParams := twilio.ClientParams{
		Username:   p.sid,
		Password:   p.token,
		AccountSid: p.sid,
		Client:     nil,
	}

	twilioClient := twilio.NewRestClientWithParams(clientParams)
	params := &api.CreateMessageParams{}

	if whatsapp {
		params.SetContentSid(p.contentSid)
		params.SetTo("whatsapp:" + to)
		params.SetFrom(p.messagingServiceSid)
		params.SetContentVariables("{\"1\":\"" + message + "\"}")
	} else {
		params.SetMessagingServiceSid(p.messagingServiceSid)
		params.SetTo(to)
		params.SetBody(message)
	}

	resp, err := twilioClient.Api.CreateMessage(params)
	if err != nil {
		return err
	} else {
		if resp.Sid != nil {
			fmt.Println(*resp.Sid)
		} else {
			fmt.Println(resp.Sid)
		}
	}

	return nil
}
