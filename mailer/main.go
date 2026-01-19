package main

import (
	"fmt"
	"os"
	"os/signal"
	"syscall"

	"gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/rabbitworker"
	"gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/stdlogger"
	"go.uber.org/zap"
)

var (
	logger *zap.Logger = stdlogger.Logger
)

func main() {
	defer logger.Sync()

	logger.Info("init mail sender... OK")

	// получим настройки

	cfg, err := loadConfig()
	if err != nil {
		logger.Error(fmt.Sprintf("loading service config... error: %s", err.Error()))
		return
	}

	// Создадим отправщика мейлов
	mp, err := NewMailProvider(cfg)
	if err != nil {
		logger.Error(fmt.Sprintf("init mail sender... error: %s", err.Error()))
		return
	}

	// Запуск слушателя RabbitMQ
	rw, err := rabbitworker.NewWorker(cfg.WorkerName)
	if err != nil {
		logger.Error(fmt.Sprintf("starting AMQP worker... error: %s", err.Error()))
		return
	}
	defer rw.Close()

	// обработчик задач
	th, err := NewTaskHandler(rw, mp)
	if err != nil {
		logger.Error(fmt.Sprintf("starting Task handler... error: %s", err.Error()))
		return
	}
	defer th.Shutdown()
	logger.Info("starting Task handler... OK")

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
