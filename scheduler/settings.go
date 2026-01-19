package main

import (
	"github.com/spf13/viper"
	"gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/rabbitworker"
	"time"
)

type ServiceConfig struct {

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
	
	SQL struct {
		URL         string `json:"url"`
		Login       string `json:"login"`
		Pass        string `json:"pass"`
		TechTimeout int    `json:"tech_timeout"` // таймаут для технических запросов (не вызов тасков) в секундах
	} `json:"sql"`


	Tasks struct {
		ReloadInterval  int `json:"reload_interval"`
	} `json:"tasks"`
}

func loadConfig(cfg *ServiceConfig) error {
	viper.AutomaticEnv()
	viper.SetEnvPrefix("")

	viper.SetDefault("REQUEST_TIMEOUT", time.Duration(10)*time.Second)
	// Worker
	cfg.WorkerName = viper.GetString("WORKER_NAME")
	cfg.WorkersCount = viper.GetInt("WORKERS_COUNT")
	// Proxy
	cfg.Proxy.URL = viper.GetString("PROXY_SERVER")
	cfg.Proxy.User = viper.GetString("PROXY_USER")
	cfg.Proxy.Password = viper.GetString("PROXY_PASSWORD")
	// Request timeout
	cfg.RequestTimeout = viper.GetDuration("REQUEST_TIMEOUT") * time.Second	

	cfg.SQL.URL = viper.GetString("SQLDB_CONNECTION")
	cfg.SQL.Login = viper.GetString("SQLDB_CONNECTION_LOGIN")
	cfg.SQL.Pass = viper.GetString("SQLDB_CONNECTION_PASSWORD")
	cfg.SQL.TechTimeout = viper.GetInt("TECH_TIMEOUT")
	//cfg.Tasks.GracefulTimeout = viper.GetInt("GRACEFUL_TIMEOUT")
	cfg.Tasks.ReloadInterval = viper.GetInt("TASK_RELOAD_INTERVAL")
	
	return nil
}