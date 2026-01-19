package rabbitworker

import (
	"strings"
	"time"

	"github.com/streadway/amqp"
	"gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/stdlogger"
)

type Worker struct {
	workerName string // наименование сервиса

	conn *amqp.Connection

	CloseCh chan *amqp.Error

	in struct {
		ch   *amqp.Channel
		q    amqp.Queue
		msgs <-chan amqp.Delivery
	}

	out struct {
		ch           *amqp.Channel
		q            amqp.Queue
		exchangeName string
	}
}

func (p *Worker) GetInputMsgChan() <-chan amqp.Delivery {
	return p.in.msgs
}

// Close - освобождаем ресурсы
func (p *Worker) Close() {
	p.closeAll(true)
}

// closeAll - закрыть открытые каналы и коннект (сервисная)
func (p *Worker) closeAll(closeOut bool) {
	if closeOut {
		p.out.ch.Close()
	}
	p.in.ch.Close()
	p.conn.Close()
}

func NewWorker(name string) (*Worker, error) {
	wrk := &Worker{
		workerName: name,
	}

	var err error
	cfg, err := loadConfig()
	if err != nil {
		return nil, err
	}

	// коннектимся
	dialCfg := amqp.Config{
		Properties: amqp.Table{
			"connection_name": name,
		},
	}

	wrk.conn, err = amqp.DialConfig(cfg.URL, dialCfg)
	if err != nil {
		if strings.Contains(err.Error(), "connection") && strings.Contains(err.Error(), "refused") {
			stdlogger.Logger.Info("connection refused, sleep 10 seconds")
			time.Sleep(10 * time.Second)
		}
		return nil, err
	}
	// входной канал
	wrk.in.ch, err = wrk.conn.Channel()
	if err != nil {
		wrk.conn.Close()
		return nil, err
	}

	// listern close event
	wrk.CloseCh = make(chan *amqp.Error)
	wrk.in.ch.NotifyClose(wrk.CloseCh)

	// входная очередь
	wrk.in.q, err = wrk.in.ch.QueueDeclare(
		cfg.IN.QName, // name
		true,         // durable
		false,        // delete when unused
		false,        // exclusive
		false,        // no-wait
		nil,          // arguments
	)
	if err != nil {
		wrk.closeAll(false)
		return nil, err
	}
	// Qos
	err = wrk.in.ch.Qos(
		1,     // prefetch count
		0,     // prefetch size
		false, // global
	)
	if err != nil {
		wrk.closeAll(false)
		return nil, err
	}
	wrk.in.msgs, err = wrk.in.ch.Consume(
		wrk.in.q.Name, // queue
		"",            // consumer
		false,         // auto-ack
		false,         // exclusive
		false,         // no-local
		false,         // no-wait
		nil,           // args
	)
	if err != nil {
		wrk.closeAll(false)
		return nil, err
	}
	// выходной канал
	wrk.out.ch, err = wrk.conn.Channel()
	if err != nil {
		wrk.closeAll(false)
		return nil, err
	}
	if cfg.OUT.QName != "" {
		wrk.out.q, err = wrk.out.ch.QueueDeclare(
			cfg.OUT.QName,
			true,  // durable
			false, // delete when unused
			false, // exclusive
			false, // no-wait
			nil,   // arguments
		)
	}
	if err != nil {
		wrk.closeAll(true)
		return nil, err
	}
	wrk.out.exchangeName = cfg.OUT.Exchange

	stdlogger.Logger.Info("starting AMQP worker... OK")
	return wrk, nil
}

/*
	отправить ответ в выходную очередь
		taskID - ID задачи из MBTasks
		execCode - 0/1 удачно/неудачно
*/
func (p *Worker) Respond(taskID int64, execCode int32, respValue string, body []byte) error {
	// создаем сообщение
	resp := amqp.Publishing{
		DeliveryMode: amqp.Persistent,
		ContentType:  "text/plain",
		Body:         body,
		Headers:      amqp.Table{},
	}

	resp.Headers["MbTaskID"] = taskID
	resp.Headers["MbTaskExecutionErrorCode"] = execCode
	resp.Headers["MbResponseValue"] = respValue
	resp.Headers["MbHookListenerCode"] = p.workerName
	resp.Headers["Worker"] = p.workerName
	// публикуем
	if p.out.q.Name != "" {
		err := p.out.ch.Publish(
			p.out.exchangeName, // exchange
			p.out.q.Name,       // routing key
			false,              // mandatory
			false,
			resp)

		if err != nil {
			return err
		}
	}
	return nil
}

func (p *Worker) SendTask(queueName string, taskID int64, taskParams string, body []byte) error {
	// создаем сообщение
	resp := amqp.Publishing{
		DeliveryMode: amqp.Persistent,
		ContentType:  "text/plain",
		Body:         body,
		Headers:      amqp.Table{},
	}

	resp.Headers["MbTaskID"] = taskID
	resp.Headers["MbTaskParams"] = taskParams
	resp.Headers["Worker"] = p.workerName

	if queueName == "" {
		queueName = p.out.q.Name
	}

	err := p.out.ch.Publish(
		p.out.exchangeName, // exchange
		queueName,          // routing key
		false,              // mandatory
		false,
		resp)

	if err != nil {
		return err
	}
	return nil
}
