package main

import (
	"context"
	"fmt"
	"strings"
	"sync"
	"time"
)

type Task struct {
	dbs *DBStorage
	// для выполнения комманд
	cmdCtx    context.Context
	cmdCancel context.CancelFunc
	CloseCh   chan int
	// общие поля (для инициализации)
	id       int64
	name     string
	command  string
	startAt  int64
	endAt    int64
	interval int64

	mu       sync.RWMutex
	critical struct {
		nextRunTime int64
		active      bool // задача в данный момент выполняется на сервере ?
		enabled     bool // нужно ли выполнять ф-цию Serve (цикл вызова)
	}
}

func NewTask(dbs *DBStorage,
	id int64, name, command string, startAt, endAt, interval int64) *Task {

	t := &Task{
		dbs:      dbs,
		id:       id,
		name:     name,
		command:  command,
		startAt:  startAt,
		endAt:    endAt,
		interval: interval,
		CloseCh:  make(chan int),
	}
	// контекст
	t.cmdCtx, t.cmdCancel = context.WithCancel(context.Background())

	// сбросим время следующего запуска
	t.mu.Lock()
	t.critical.nextRunTime = 0
	t.critical.active = false
	t.critical.enabled = true // изначально задача включена для выполнения
	t.mu.Unlock()

	return t
}

// общая процедура обработки жизненного цикла таска
func (p *Task) Serve(wg *sync.WaitGroup) {
	logger.Debug(fmt.Sprintf("Serve scheduled task S#%d (%s)", p.id, p.name))
	defer func() {
		p.SetActive(false)
		p.Disable()
	}()
	wg.Done()
	for {
		// если выключено - то сразу на выход
		if !p.Enabled() {
			logger.Info(fmt.Sprintf("task S#%d disabled, not run", p.id))
			return
		}
		// считаем время следующего запуска
		p.CalcNextRunTime()
		// и таймаут до наступления события
		timeout := p.NextRunTime() - time.Now().Unix()

		logger.Debug(fmt.Sprintf("Scheduled task S#%d (%s) in %d seconds", p.id, p.name, timeout))

		// если таймаут кривой - то тоже на выход (возможно таск вышел за границы диаппазона "жизни")
		if timeout < 0 {
			p.Disable()
			return
		}
		// всё ок - запускаем
		select {
		case <-time.After(time.Duration(timeout) * time.Second):
			logger.Info(fmt.Sprintf("task S#%d wake up", p.id))
		case <-p.CloseCh:
			logger.Info(fmt.Sprintf("task S#%d closed", p.id))
		}

		if !p.Enabled() {
			logger.Info(fmt.Sprintf("task S#%d disabled, not run", p.id))
			return
		}

		// запуск команды
		cmd := fmt.Sprintf("%s", p.command)
		logger.Debug(fmt.Sprintf("Run %s", strings.Trim(cmd, " \n\r\t")))
		p.SetActive(true)

		var err error
		err = nil
		<-time.After(5 * time.Second)
		err = p.dbs.ExecCommand(p.cmdCtx, p.command)
		p.SetActive(false)
		logger.Debug(fmt.Sprintf("Task S#%d done %s", p.id, strings.Trim(cmd, " \n\r\t")))
		if err != nil {
			logger.Error(fmt.Sprintf("task S#%d run error: %w", p.id, err))
		}
	}
}

func (p *Task) NextRunTime() int64 {
	p.mu.RLock()
	defer p.mu.RUnlock()
	return p.critical.nextRunTime
}

// получим время следующего запуска таска
func (p *Task) CalcNextRunTime() {
	p.mu.Lock()
	// получим текущее время
	tn := time.Now().Unix()
	// время запуска еще не было назначено?
	if p.critical.nextRunTime == 0 {
		// текущее время входит в диаппазон "жизни" таска
		logger.Debug(fmt.Sprintf("calc first, Task S#%d 0- nextRunTime: %d   startAt: %d  endAt:%d", p.id, p.critical.nextRunTime, p.startAt, p.endAt))

		if tn >= p.startAt && tn < p.endAt-p.interval {
			// считаем время следующего запуска
			p.critical.nextRunTime = (tn - p.startAt) / p.interval
			logger.Debug(fmt.Sprintf("calc first, Task S#%d 1- num of intervals: %d ", p.id, p.critical.nextRunTime))
			p.critical.nextRunTime = p.critical.nextRunTime * p.interval
			logger.Debug(fmt.Sprintf("calc first, Task S#%d 2- nextRunTime: %d ", p.id, p.critical.nextRunTime))
			p.critical.nextRunTime += p.startAt
			logger.Debug(fmt.Sprintf("calc first, Task S#%d 3- nextRunTime: %d (%s)", p.id, p.critical.nextRunTime, time.Unix(p.critical.nextRunTime, 0).Format(time.RFC3339)))
			if tn >= p.critical.nextRunTime {
				p.critical.nextRunTime += p.interval
				logger.Debug(fmt.Sprintf("calc first, Task S#%d 4 - nextRunTime: %d (%s)", p.id, p.critical.nextRunTime, time.Unix(p.critical.nextRunTime, 0).Format(time.RFC3339)))
			}
		}
	} else {
		// какоето время запуска уже было
		for {
			// когда таск закончился - неизвестно такчто считаем от времени старта
			p.critical.nextRunTime += p.interval
			logger.Debug(fmt.Sprintf("calc next, Task S#%d 1- nextRunTime: %d (%s)", p.id, p.critical.nextRunTime, time.Unix(p.critical.nextRunTime, 0).Format(time.RFC3339)))
			if p.critical.nextRunTime > tn && p.critical.nextRunTime < p.endAt {
				// время следующего запуска посчиталось верно
				logger.Debug(fmt.Sprintf("calc next, Task S#%d   break: %d ", p.id, p.critical.nextRunTime))
				break
			}
			// выход за границы диаппазона "жизни"
			if p.critical.nextRunTime >= p.endAt {
				logger.Debug(fmt.Sprintf("calc next, Task S#%d   %d > endAt:  ", p.id, p.critical.nextRunTime))
				p.critical.nextRunTime = 0
				break
			}
		}
	}
	p.mu.Unlock()
}

// отключаем цикл вычислений следующего запуска
func (p *Task) Disable() {
	p.mu.Lock()
	p.critical.enabled = false
	p.mu.Unlock()
}

func (p *Task) GetActive() bool {
	p.mu.RLock()
	defer p.mu.RUnlock()
	return p.critical.active

}

func (p *Task) Enabled() bool {
	p.mu.RLock()
	defer p.mu.RUnlock()
	return p.critical.enabled

}

func (p *Task) SetActive(active bool) {
	p.mu.Lock()
	p.critical.active = active
	p.mu.Unlock()

}

// выключим обработку таска с таймаутом
func (p *Task) Shutdown(wg *sync.WaitGroup) {
	defer wg.Done()
	// отключим цикл обработки
	logger.Info(fmt.Sprintf("task S#%d shutting down", p.id))
	close(p.CloseCh)
	logger.Info(fmt.Sprintf("task S#%d channel closed", p.id))
	p.Disable()
	logger.Info(fmt.Sprintf("task S#%d disabled", p.id))
	// подождём таймаут (если задача активна)
	for p.GetActive() {
		logger.Info(fmt.Sprintf("wait task S#%d inactive", p.id))
		<-time.After(1 * time.Second)
	}
}
