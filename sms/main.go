package main

import (
	"encoding/json"
	"fmt"
	"os"
	"os/signal"
	"syscall"
	"time"

	"github.com/spf13/viper"
	"gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/rabbitworker"
	"gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/stdlogger"
	"go.uber.org/zap"

	"gitlab.canopus.ru/macrobank_5_7/core_api/go-services/internal/sms/smssenders"
	"strings"
)

var (
	logger *zap.Logger = stdlogger.Logger
)

func main() {
	defer logger.Sync()
	// читаем конфиг
	cfg := &ServiceConfig{}
	err := loadConfig(cfg)
	if err != nil {
		logger.Error(fmt.Sprintf("loading service config... error: %s", err.Error()))
		return
	}

	// Создаем http-клиент для запросов
	client, err := NewHTTPClient(cfg)
	if err != nil {
		logger.Error(fmt.Sprintf("creating net client... error: %s", err.Error()))
		return
	}
	logger.Info("creating net client... OK")

	logger.Info(fmt.Sprintf("Debug: %d", cfg.Debug))
	if cfg.Debug == 111 {
		var s SMSSender
		// создадим обработчик
		for i := range cfg.Senders {
			s = nil
			if cfg.Senders[i].Enabled {
				switch strings.ToLower(cfg.Senders[i].Name) {
				case "vertex":
					s = smssenders.NewVertexSender(cfg.Senders[i].Priority)
				case "clickatell":
					s = smssenders.NewClickatellSender(cfg.Senders[i].Priority)
				case "cardboardfish":
					s = smssenders.NewCardBoardFishSender(cfg.Senders[i].Priority)
				case "nexmo":
					s = smssenders.NewNexmoSender(cfg.Senders[i].Priority)
				case "etimsalat":
					s = smssenders.NewEtimsalatSender(cfg.Senders[i].Priority)
				case "websms":
					s = smssenders.NewWebSmsSender(cfg.Senders[i].Priority)
				case "twilio":
					s = smssenders.NewTwilioSender(cfg.Senders[i].Priority)
				case "infobip":
					s = smssenders.NewInfoBipSender(cfg.Senders[i].Priority)
				}
			} else {
				continue
			}
			// обработчик был инициализирован?
			err := s.Init(cfg.Senders[i].Params, cfg.Senders[i].Priority)
			if err != nil {
				logger.Error(fmt.Sprintf("Init sender %s error: %s", cfg.Senders[i].Name, err.Error()))
				return
			}
			break
		}

		err := s.SendMessage(client, "+380956315274", "test msg 1", false)
		if err != nil {
			logger.Error(fmt.Sprintf("Send Msg... error: %s", err.Error()))
			return
		}
		time.Sleep(time.Second * 6)
		err = s.SendMessage(client, "+380956315274", "12333", true)
		if err != nil {
			logger.Error(fmt.Sprintf("Send Msg... error: %s", err.Error()))
			return
		}
		/*time.Sleep(time.Second * 6)
			  err = s.SendMessage(client,"+7971558824809","test msg 1",false)
		      if err != nil {
		      	logger.Error(fmt.Sprintf("Send Msg... error: %s", err.Error()))
		      	return
		      }
			  time.Sleep(time.Second * 6)
			  err = s.SendMessage(client,"+7971558824809","test msg 1",false)
		      if err != nil {
		      	logger.Error(fmt.Sprintf("Send Msg... error: %s", err.Error()))
		      	return
		      }*/

	} else {
		// Запуск слушателя RabbitMQ
		rw, err := rabbitworker.NewWorker(cfg.WorkerName)
		if err != nil {
			logger.Error(fmt.Sprintf("starting AMQP worker... error: %s", err.Error()))
			return
		}
		defer rw.Close()

		// обработчик задач
		th, err := NewTaskHandler(cfg, client, rw)
		if err != nil {
			logger.Error(fmt.Sprintf("starting Task handler... error: %s", err.Error()))
			return
		}
		defer th.Shutdown()

		go th.Serve(cfg.WorkersCount)

		// канал для прерывания работы
		shutdown := make(chan os.Signal, 1)
		signal.Notify(shutdown, os.Interrupt, syscall.SIGINT, syscall.SIGTERM)

		select {
		case <-shutdown:
		case <-th.CloseCh:
			logger.Info("AMQP closed")
		}

		logger.Info("exiting Task handler... OK")
	}
}

func loadConfig(cfg *ServiceConfig) error {
	viper.AutomaticEnv()
	viper.SetEnvPrefix("")
	cfg.Debug = viper.GetInt("DEBUG")
	viper.SetDefault("WORKERS_COUNT", 1)

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
	// Senders (конфигурацию берем из отдельного JSON файла)
	jsonFile, err := os.Open(viper.GetString("SENDERS"))
	if err != nil {
		return fmt.Errorf("open senders config file error: %w", err)
	}
	defer jsonFile.Close()
	// JSON UNMARSHALL
	dec := json.NewDecoder(jsonFile)
	err = dec.Decode(&cfg.Senders)
	if err != nil {
		return fmt.Errorf("unmarshall senders config file error: %w", err)
	}

	logger.Info("loading service config... OK")
	return nil
}
