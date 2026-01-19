package main

import (
	"fmt"
	"io/ioutil"
	"path/filepath"
	"strings"
	"time"

	"github.com/spf13/viper"
)

type ServiceConfig struct {
	// Имя данного сервиса-воркера
	WorkerName   string
	WorkersCount int

	Mail struct {
		ProviderType bool

		SMTP struct {
			Address        string
			Port           int
			UseSSL         bool
			Login          string
			Pass           string
			ConnectTimeout time.Duration
			SendTimeout    time.Duration
		}

		SenderAddress     string
		SenderDisplayName string

		SendGrid struct {
			ApiKey string
		}
	}

	Common struct {
		GracefulTimeout int `json:"graceful_timeout"`
	} `json:"common"`
}

func loadConfig() (*ServiceConfig, error) {
	cfg := &ServiceConfig{}

	// 1. Читаем ENV
	viper.AutomaticEnv()
	viper.SetEnvPrefix("")
	viper.SetDefault("WORKERS_COUNT", 1)

	// 2. Загружаем все .env файлы из /application/
	files, readErr := ioutil.ReadDir("/application")
	if readErr == nil {
		for _, f := range files {
			if !f.IsDir() && strings.HasSuffix(f.Name(), ".env") {
				path := filepath.Join("/application", f.Name())
				v := viper.New()
				v.SetConfigFile(path)
				if e := v.MergeInConfig(); e == nil {
					logger.Info(fmt.Sprintf("Loaded config from %s", path))
					for _, key := range v.AllKeys() {
						val := v.Get(key)
						viper.Set(key, val)
					}
				} else {
					logger.Warn(fmt.Sprintf("Failed to load config from %s: %v", path, e))
				}
			}
		}
	} else {
		logger.Warn(fmt.Sprintf("No /application directory for secrets: %v", readErr))
	}

	// Worker
	cfg.WorkerName = viper.GetString("WORKER_NAME")
	cfg.WorkersCount = viper.GetInt("WORKERS_COUNT")

	// Mail provider
	rawProvider := strings.ToLower(strings.TrimSpace(viper.GetString("EMAIL_SERVICE_PROVIDER_TYPE")))
	switch rawProvider {
	case "smtp":
		cfg.Mail.ProviderType = true
	case "sendgrid":
		cfg.Mail.ProviderType = false
	default:
		logger.Warn(fmt.Sprintf("Unknown EMAIL_SERVICE_PROVIDER_TYPE=%q, fallback to sendgrid", rawProvider))
		cfg.Mail.ProviderType = false
	}

	// SMTP config
	cfg.Mail.SMTP.Address = viper.GetString("SMTP_ADDRESS")
	cfg.Mail.SMTP.Port = viper.GetInt("SMTP_ADDRESS_PORT")
	cfg.Mail.SMTP.UseSSL = viper.GetBool("SMTP_USE_SSL")
	cfg.Mail.SMTP.Login = viper.GetString("SMTP_LOGIN")
	cfg.Mail.SMTP.Pass = viper.GetString("SMTP_PASSWORD")

	// SMTP timeouts
	if cfg.Mail.ProviderType {
		rawConnect := viper.GetString("SMTP_CONNECT_TIMEOUT")
		if rawConnect != "" {
			if d, err := time.ParseDuration(rawConnect); err == nil && d > 0 {
				cfg.Mail.SMTP.ConnectTimeout = d
			} else {
				logger.Warn(fmt.Sprintf("SMTP_CONNECT_TIMEOUT=%q is invalid, using default 10s", rawConnect))
				cfg.Mail.SMTP.ConnectTimeout = 10 * time.Second
			}
		} else {
			cfg.Mail.SMTP.ConnectTimeout = 10 * time.Second
		}

		rawSend := viper.GetString("SMTP_SEND_TIMEOUT")
		if rawSend != "" {
			if d, err := time.ParseDuration(rawSend); err == nil && d > 0 {
				cfg.Mail.SMTP.SendTimeout = d
			} else {
				logger.Warn(fmt.Sprintf("SMTP_SEND_TIMEOUT=%q is invalid, using default 10s", rawSend))
				cfg.Mail.SMTP.SendTimeout = 10 * time.Second
			}
		} else {
			cfg.Mail.SMTP.SendTimeout = 10 * time.Second
		}
	}

	// Sender info
	cfg.Mail.SenderAddress = viper.GetString("EMAIL_SENDER_ADDRESS")
	cfg.Mail.SenderDisplayName = viper.GetString("EMAIL_SENDER_ADDRESS_DISPLAY_NAME")

	// SendGrid
	cfg.Mail.SendGrid.ApiKey = viper.GetString("SENDGRID_API_KEY")

	return cfg, nil
}
