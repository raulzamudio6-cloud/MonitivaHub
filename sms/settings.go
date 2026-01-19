package main

import (
	"time"

	"gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/rabbitworker"
)

type SenderConfig struct {
	Name     string            `json:"name"`
	Enabled  bool              `json:"enabled"`
	Priority int               `json:"priority"`
	Params   map[string]string `json:"params"`
}

type ServiceConfig struct {
    Debug int
	// Имя данного сервиса-воркера
	WorkerName   string
	WorkersCount int
	// AMQP
	AMQP rabbitworker.Settings
	// Proxy
	Proxy struct {
		URL      string
		User     string
		Password string
	}
	// Request timeout (seconds)
	RequestTimeout time.Duration
	// Senders
	Senders []SenderConfig
}
