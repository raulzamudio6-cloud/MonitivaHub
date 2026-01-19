package main

import (
	"encoding/json"
	"errors"
	"fmt"
	"strings"
	"sync"

	"github.com/streadway/amqp"
	"go.uber.org/zap"
	"gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/rabbitworker"
)

type TaskHandler struct {
	rw *rabbitworker.Worker
	mp *MailProvider

	chResp  chan ResponseData
	CloseCh chan int
}

func NewTaskHandler(rw *rabbitworker.Worker, mp *MailProvider) (*TaskHandler, error) {
	return &TaskHandler{
		rw:      rw,
		mp:      mp,
		chResp:  make(chan ResponseData),
		CloseCh: make(chan int),
	}, nil
}

func (p *TaskHandler) processTask(workerId int, task *amqp.Delivery, wg *sync.WaitGroup) {
	type taskParams struct {
		MessageQID int32 `json:"message_queue_id"`
	}

	// validate MbTaskID
	if task.Headers["MbTaskID"] == nil {
		task.Nack(false, false)
		p.returnResp(0, 0, errors.New("MbTaskID header not set or empty"), nil)
		return
	}
	taskID := task.Headers["MbTaskID"].(int64)

	// decode MbTaskParams
	tp := &taskParams{}
	dec := json.NewDecoder(strings.NewReader(task.Headers["MbTaskParams"].(string)))
	if err := dec.Decode(tp); err != nil {
		task.Nack(false, false)
		p.returnResp(taskID, tp.MessageQID, err, nil)
		return
	}

	// unmarshal payload
	pl := &PayloadData{}
	if err := json.Unmarshal(task.Body, pl); err != nil {
		task.Nack(false, false)
		p.returnResp(taskID, tp.MessageQID, err, nil)
		return
	}

	// validation
	if len(pl.From) == 0 || pl.From[0].Address == "" {
		task.Nack(false, false)
		p.returnResp(taskID, tp.MessageQID, errors.New("invalid 'From'"), extractAddresses(pl.Tos))
		return
	}
	if len(pl.Tos) == 0 || pl.Tos[0].Address == "" {
		task.Nack(false, false)
		p.returnResp(taskID, tp.MessageQID, errors.New("invalid 'To'"), extractAddresses(pl.Tos))
		return
	}
	if pl.Subject == "" {
		task.Nack(false, false)
		p.returnResp(taskID, tp.MessageQID, errors.New("field 'Subject' not set"), extractAddresses(pl.Tos))
		return
	}

	logger.Debug("new task received",
		zap.Int("workerId", workerId),
		zap.Int64("taskID", taskID),
	)

	// try to send message
	if err := p.mp.SendMessage(pl); err != nil {
		task.Nack(false, false)
		p.returnResp(taskID, tp.MessageQID, err, extractAddresses(pl.Tos))
		return
	}

	// success
	task.Ack(false)
	p.returnResp(taskID, tp.MessageQID, nil, extractAddresses(pl.Tos))
}

func (p *TaskHandler) Serve(workersCount int) {
	var wg sync.WaitGroup

	// responses handler
	wg.Add(1)
	go p.handleResponses(&wg)
	wg.Wait()

	que := make(chan amqp.Delivery, workersCount)
	go func() {
		<-p.rw.CloseCh
		logger.Debug("Closing queues")
		close(p.chResp)
		close(que)
		wg.Wait()
		close(p.CloseCh)
	}()

	logger.Debug(fmt.Sprintf("Starting %d workers", workersCount))
	for i := 1; i <= workersCount; i++ {
		go func(que chan amqp.Delivery, workerId int) {
			wg.Add(1)
			for task := range que {
				p.processTask(workerId, &task, &wg)
			}
			wg.Done()
		}(que, i)
	}

	logger.Debug("Start reading input messages")
	for m := range p.rw.GetInputMsgChan() {
		que <- m
	}
}

func (p *TaskHandler) handleResponses(wg *sync.WaitGroup) {
	wg.Done()
	type RespBodyData struct {
		MsgQID  int32  `json:"message_queue_id"`
		Message string `json:"error_message,omitempty"`
	}
	rbd := &RespBodyData{}

	for resp := range p.chResp {
		respValue := "OK"
		if resp.code == 0 {
			rbd.MsgQID = resp.msgQID
			rbd.Message = ""
			logger.Info("SUCCESS",
				zap.Int64("taskID", resp.taskID),
				zap.Int32("msgID", resp.msgQID),
				zap.Strings("to", maskAddresses(resp.tos)),
			)
		} else {
			respValue = "ERR"
			rbd.MsgQID = resp.msgQID
			rbd.Message = resp.message
			logger.Error("ERROR",
				zap.Int64("taskID", resp.taskID),
				zap.Int32("msgID", resp.msgQID),
				//zap.Int("code", int(resp.code)),
				zap.String("message", resp.message),
				zap.Strings("to", maskAddresses(resp.tos)),
			)
		}

		body, err := json.Marshal(rbd)
		if err != nil {
			logger.Error("marshal response failed",
				zap.Int64("taskID", resp.taskID),
				zap.Int32("msgID", resp.msgQID),
				zap.Error(err),
			)
			continue
		}
		if err := p.rw.Respond(resp.taskID, resp.code, respValue, body); err != nil {
			logger.Error("failed to send answer",
				zap.Int64("taskID", resp.taskID),
				zap.Int32("msgID", resp.msgQID),
				zap.Error(err),
			)
		}
	}
}

func (p *TaskHandler) Shutdown() {}

func (p *TaskHandler) returnResp(taskID int64, msgQID int32, err error, tos []string) {
	if err != nil {
		p.chResp <- ResponseData{
			taskID:  taskID,
			msgQID:  msgQID,
			code:    1,
			message: err.Error(),
			tos:     tos,
		}
	} else {
		p.chResp <- ResponseData{
			taskID:  taskID,
			msgQID:  msgQID,
			code:    0,
			message: "OK",
			tos:     tos,
		}
	}
}
