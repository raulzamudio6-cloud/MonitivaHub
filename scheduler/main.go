package main

import (

	"fmt"
	"os"
	"os/signal"
	"syscall"
	"time"
	"go.uber.org/zap"
	"github.com/streadway/amqp"
	"gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/stdlogger"
	"gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/rabbitworker"
)

var (
	logger *zap.Logger = stdlogger.Logger
)

type taskParams struct {
		Action string `json:"action"`
		SchedfulerTaskId     int64 `json:"task"`
}

func main() {
	defer logger.Sync()

	// читаем конфиг
	cfg := &ServiceConfig{}
	err := loadConfig(cfg)
	if err != nil {
		logger.Error(fmt.Sprintf("loading service config... error: %s", err.Error()))
		return
	}

	
	rw, err := rabbitworker.NewWorker(cfg.WorkerName)
	if err != nil {
		logger.Error(fmt.Sprintf("starting AMQP worker... error: %s", err.Error()))
		return
	}
	defer rw.Close()

	
	logger.Info("loading service config... OK")
	logger.Info(fmt.Sprintf("TASK_RELOAD_INTERVAL: %d", cfg.Tasks.ReloadInterval))
	
	if cfg.Tasks.ReloadInterval <= 0 {
		logger.Info("Reloading disabled. TASK_RELOAD_INTERVAL is not set.")
	}

	// создаем контейнер тасков
	tc, err := NewTasksContainer(cfg)
	if err != nil {
		logger.Error(fmt.Sprintf("starting task container... error: %s", err.Error()))
		return
	}
	logger.Info("starting task container ... OK")
	
	workersCount := 1;
	que := make(chan amqp.Delivery, workersCount)

    go func() {
		<-rw.CloseCh
		logger.Debug("Rabbit closed")
	}()

    go func(que chan amqp.Delivery, workerId int) {
			for {
				//logger.Debug(fmt.Sprintf("%d start read que", workerId))
				task, ok := <-que
				if !ok {
					logger.Debug(fmt.Sprintf("Exiting worker %d", workerId))
					return
				}
				// logger.Debug(fmt.Sprintf("%d end read que task %d", workerId, task.Headers["MbTaskID"].(int64)))

				tc.processTask(workerId, &task)
				// logger.Debug(fmt.Sprintf("%d end process task %d", workerId, task.Headers["MbTaskID"].(int64)))
			}
		}(que, 1)

    logger.Debug("Start reading input messages")
	go func() {
		for m := range rw.GetInputMsgChan() {

		// logger.Debug(fmt.Sprintf("write que taskId: %d", m.Headers["MbTaskID"].(int64)))
		//logger.Debug(fmt.Sprintf("write que taskId 2: %d", m.Headers["MbTaskId"].(int64)))
			que <- m
		// logger.Debug(fmt.Sprintf("end write que taskId: %d", m.Headers["MbTaskID"].(int64)))
		}
	}()


	defer func() {
		tc.Shutdown()
		logger.Info("task container stopped")
	}()

	tc.Serve()

	if cfg.Tasks.ReloadInterval > 0 {
		go func() {
			for {
				logger.Info(fmt.Sprintf("Auto reload config after %d seconds", cfg.Tasks.ReloadInterval))
				<-time.After(time.Duration(cfg.Tasks.ReloadInterval) * time.Second)
				tc.reloadTask(0);
			}
		}()
    }
	
	
	// канал для прерывания работы
	shutdown := make(chan os.Signal, 1)
	signal.Notify(shutdown, os.Interrupt, syscall.SIGINT, syscall.SIGTERM)

	<-shutdown
}

