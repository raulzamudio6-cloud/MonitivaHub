package main

import "strings"

type EmailAddress struct {
	Address     string `json:"address"`
	DisplayName string `json:"display_name,omitempty"`
	Encoding    string `json:"encoding,omitempty"`
}

type EMailAttachment struct {
	Name      string `json:"name"`
	MediaType string `json:"media_type"`
	IsInline  bool   `json:"is_inline"`
	ContentID string `json:"content_id"`
	Content   string `json:"content"` // BASE64 String
}

type PayloadData struct {
	// EMailFrom
	From []EmailAddress `json:"from"`
	// EMailTo
	Tos []EmailAddress `json:"tos"`
	// EMailCC
	CCs []EmailAddress `json:"ccs,omitempty"`
	// EMailBCC
	BCCs []EmailAddress `json:"bccs,omitempty"`
	// Subject
	Subject string `json:"subject"`
	// Body
	Body string `json:"body,omitempty"`
	// Priority
	//EMailPriority string `json:"EMailPriority"`
	// EMailIsBodyHtml
	IsHtml bool `json:"is_html"`
	// EMailAttachment (BASE64 string)
	Attachments []EMailAttachment `json:"attachments,omitempty"`
}

type ResponseData struct {
	taskID  int64
	msgQID  int32
	code    int32
	message string
	tos		[]string
}

/*************** EmailAddress ***********************************/
func (p EmailAddress) GetFullAddress() string {
	sb := strings.Builder{}

	if p.DisplayName != "" {
		sb.WriteString(p.DisplayName)
		sb.WriteString("<")
		sb.WriteString(p.Address)
		sb.WriteString(">")
	} else {
		sb.WriteString(p.Address)
	}
	return sb.String()
}
