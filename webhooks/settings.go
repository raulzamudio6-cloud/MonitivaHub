package main

import (
	"fmt"
	"time"

	"github.com/spf13/viper"
	"gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/rabbitworker"
)

type SenderConfig struct {
	Name     string            `json:"name"`
	Enabled  bool              `json:"enabled"`
	Priority int               `json:"priority"`
	Params   map[string]string `json:"params"`
}

type ServiceConfig struct {
	// Имя данного сервиса-воркера
	WorkerName string
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

	ListenChannels string
}

func loadConfig(cfg *ServiceConfig) error {
	viper.AutomaticEnv()
	viper.SetEnvPrefix("")

	// Worker
	cfg.WorkerName = viper.GetString("WORKER_NAME")
	// Proxy
	cfg.Proxy.URL = viper.GetString("PROXY_SERVER")
	cfg.Proxy.User = viper.GetString("PROXY_USER")
	cfg.Proxy.Password = viper.GetString("PROXY_PASSWORD")
	// Request timeout
	var err error
	cfg.RequestTimeout, err = time.ParseDuration(viper.GetString("REQUEST_TIMEOUT"))
	if err != nil {
		logger.Warn(fmt.Sprintf("Can't parse REQUEST_TIMEOUT: %s", err.Error()))
		cfg.RequestTimeout = time.Duration(10) * time.Second
	}
	logger.Info(fmt.Sprintf("RequestTimeout: %s", cfg.RequestTimeout))

	cfg.ListenChannels = viper.GetString("LISTEN_CHANNELS")
	return nil
}
