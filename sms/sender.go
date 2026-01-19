package main

import "net/http"

type SMSSender interface {
	// инициализация провайдера значениями сеттингов
	Init(map[string]string, int) error
	SendMessage(*http.Client, string, string, bool) error
	GetPriority() int
}
