package main

type PayloadData struct {
	To       string `json:"to"`
	Message  string `json:"message"`
	Whatsapp bool   `json:"whatsapp"`
}

// ResponseData — структура ответа в канал
type ResponseData struct {
	taskID  int64
	msgQID  int32
	code    int
	message string
	phone   string
}