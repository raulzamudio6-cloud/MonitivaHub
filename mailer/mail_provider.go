package main

import (
	"fmt"
	"strings"

	"github.com/sendgrid/sendgrid-go"
	"github.com/sendgrid/sendgrid-go/helpers/mail"
	simplemail "github.com/xhit/go-simple-mail/v2"
	"go.uber.org/zap"
)

type MailProvider struct {
	cfg *ServiceConfig
}

func NewMailProvider(cfg *ServiceConfig) (*MailProvider, error) {
	return &MailProvider{cfg: cfg}, nil
}

// sending mail
func (p *MailProvider) SendMessage(msg *PayloadData) error {
	if p.cfg.Mail.ProviderType {
		return p.sendSMTPMessage(msg)
	}
	return p.sendSendGridMessage(msg)
}

// --- Masking e-mails ---
func maskEmail(addr string) string {
	parts := strings.Split(addr, "@")
	if len(parts) != 2 {
		return addr
	}
	local, domain := parts[0], parts[1]
	if len(local) <= 3 {
		return local[:1] + "***@" + domain
	}
	return local[:3] + "***@" + domain
}

func maskAddresses(emails []string) []string {
	res := make([]string, 0, len(emails))
	for _, e := range emails {
		res = append(res, maskEmail(e))
	}
	return res
}

// --- sending via SMTP ---
func (p *MailProvider) sendSMTPMessage(msg *PayloadData) error {
	server := simplemail.NewSMTPClient()

	// SMTP Server config
	server.Host = p.cfg.Mail.SMTP.Address
	server.Port = p.cfg.Mail.SMTP.Port
	server.Username = p.cfg.Mail.SMTP.Login
	server.Password = p.cfg.Mail.SMTP.Pass

	if p.cfg.Mail.SMTP.UseSSL {
		server.Encryption = simplemail.EncryptionSTARTTLS
	} else {
		server.Encryption = simplemail.EncryptionNone
	}
	server.Authentication = simplemail.AuthLogin
	server.KeepAlive = false
	server.ConnectTimeout = p.cfg.Mail.SMTP.ConnectTimeout
	server.SendTimeout = p.cfg.Mail.SMTP.SendTimeout

	// connect to SMTP
	smtpClient, err := server.Connect()
	if err != nil {
		logger.Error("SMTP connect failed",
			zap.String("host", server.Host),
			zap.Int("port", server.Port),
			zap.String("user", server.Username),
			zap.Error(err),
			zap.Strings("to", maskAddresses(extractAddresses(msg.Tos))),
		)
		return err
	}

	// build message
	email := simplemail.NewMSG()
	email.SetFrom(msg.From[0].GetFullAddress())

	for _, to := range msg.Tos {
		if to.Address != "" {
			email.AddTo(to.GetFullAddress())
		}
	}
	for _, cc := range msg.CCs {
		if cc.Address != "" {
			email.AddCc(cc.GetFullAddress())
		}
	}
	for _, bcc := range msg.BCCs {
		if bcc.Address != "" {
			email.AddBcc(bcc.GetFullAddress())
		}
	}

	email.SetSubject(msg.Subject)

	if msg.Body != "" {
		if msg.IsHtml {
			email.SetBody(simplemail.TextHTML, msg.Body)
		} else {
			email.SetBody(simplemail.TextPlain, msg.Body)
		}
	}

	for _, a := range msg.Attachments {
		email.Attach(&simplemail.File{
			B64Data: a.Content,
			Name:    a.Name,
		})
	}

	// send
	if err := email.Send(smtpClient); err != nil {
		logger.Error("SMTP send failed",
			zap.String("user", server.Username),
			zap.String("host", server.Host),
			zap.Int("port", server.Port),
			zap.Error(err),
			zap.Strings("to", maskAddresses(extractAddresses(msg.Tos))),
		)
		return err
	}

	logger.Info("SMTP send success",
		zap.String("host", server.Host),
		zap.Int("port", server.Port),
		zap.Strings("to", maskAddresses(extractAddresses(msg.Tos))),
	)
	return nil
}

// --- sending via SendGrid ---
func (p *MailProvider) sendSendGridMessage(msg *PayloadData) error {
	m := &mail.SGMailV3{}
	m.SetFrom(mail.NewEmail(msg.From[0].DisplayName, msg.From[0].Address))

	// to / cc / bcc
	per := mail.NewPersonalization()
	for _, to := range msg.Tos {
		per.AddTos(mail.NewEmail(to.DisplayName, to.Address))
	}
	for _, cc := range msg.CCs {
		per.AddCCs(mail.NewEmail(cc.DisplayName, cc.Address))
	}
	for _, bcc := range msg.BCCs {
		per.AddBCCs(mail.NewEmail(bcc.DisplayName, bcc.Address))
	}
	m.AddPersonalizations(per)

	m.Subject = msg.Subject

	if msg.Body != "" {
		if msg.IsHtml {
			m.AddContent(mail.NewContent("text/html", msg.Body))
		} else {
			m.AddContent(mail.NewContent("text/plain", msg.Body))
		}
	}

	for _, a := range msg.Attachments {
		m.AddAttachment(&mail.Attachment{
			Content:   a.Content,
			Type:      a.MediaType,
			Name:      a.Name,
			Filename:  a.Name,
			ContentID: a.ContentID,
		})
	}

	// SendGrid API call
	request := sendgrid.GetRequest(p.cfg.Mail.SendGrid.ApiKey, "/v3/mail/send", "https://api.sendgrid.com")
	request.Method = "POST"
	request.Body = mail.GetRequestBody(m)

	response, err := sendgrid.API(request)
	if err != nil {
		logger.Error("SendGrid API error",
			zap.Error(err),
			zap.Strings("to", maskAddresses(extractAddresses(msg.Tos))),
		)
		return err
	}

	if response.StatusCode != 202 {
		err := fmt.Errorf("SendGrid returned status %d: %s", response.StatusCode, response.Body)
		return err
	}
	
	return nil
}

// helper: collect addresses
func extractAddresses(emails []EmailAddress) []string {
	res := make([]string, 0, len(emails))
	for _, e := range emails {
		res = append(res, e.Address)
	}
	return res
}
