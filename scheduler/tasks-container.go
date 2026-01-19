package main

import (
	"encoding/json"
	"strings"
	"github.com/streadway/amqp"
	"fmt"
	"sync"
	"time"
)

type TasksContainer struct {
	dbs   *DBStorage      // коннект к БД
	tasks map[int64]*Task // массив тасков для обслуживания
	//wg  sync.WaitGroup
	mu       sync.RWMutex
	critical struct {
		reloading      bool // задача в данный момент выполняется на сервере ?
	}
	cfg *ServiceConfig
}

func (p *TasksContainer)  SetReloading(value bool) {
	p.mu.Lock()
	p.critical.reloading = value
	p.mu.Unlock()
}

func (p *TasksContainer)  IsReloading() bool {
	p.mu.RLock()
	defer p.mu.RUnlock()
	return p.critical.reloading

}


func NewTasksContainer(cfg *ServiceConfig) (*TasksContainer, error) {
	// создаем коннект к БД
	dbs, err := NewDBStorage(cfg)
	if err != nil {
		return nil, err
	}
	// создаем контейнер
	tc := &TasksContainer{
		dbs: dbs,
	}
	tc.cfg = cfg

	tc.tasks, err = tc.dbs.LoadTasks(0)
	if err != nil {
		return nil, err
	}
	logger.Info(fmt.Sprintf("number of active tasks :%d", len(tc.tasks)))

	return tc, nil
}

func (p *TasksContainer) Shutdown() {
	// стопаем все таски
	var wg sync.WaitGroup
	for k := range p.tasks {
		if p.tasks[k].Enabled() {
			wg.Add(1)
			go p.tasks[k].Shutdown(&wg)
		}
		wg.Wait()
	}
	p.dbs.Close()
}

func (p *TasksContainer) Serve() {
	// запускаем жизненный цикл всех тасков
	var wg sync.WaitGroup
	for k := range p.tasks {
		wg.Add(1)
		go p.tasks[k].Serve(&wg)
	}
	wg.Wait()
}


func (p *TasksContainer) reloadTask(taskId int64) {
	var err error
	
	for p.IsReloading() {
		logger.Debug("wait reloadTask...")
		<-time.After(5 * time.Second)
	}

	defer p.SetReloading(false)
	p.SetReloading(true)
	
	descr := "all tasks"
	if taskId != 0 {
		descr = fmt.Sprintf("task S#%d", taskId)
	}
	logger.Debug("Stopping "+descr+" before reloading...")
	var wg sync.WaitGroup
	for k := range p.tasks {
		if taskId == 0 || taskId == k {
			if p.tasks[k].Enabled() {
				wg.Add(1)
				go p.tasks[k].Shutdown(&wg)
			}
		}
	}
	wg.Wait()
	
	logger.Debug("Load tasks...")
	if taskId == 0 {
		p.tasks, err = p.dbs.LoadTasks(taskId)	
		if err != nil {
			logger.Error(fmt.Sprintf("Load tasks: %w", err))
			return
		}
		p.Serve()		
	} else {
		oneTaskMap, err2 := p.dbs.LoadTasks(taskId)	
		if err2 != nil {
			logger.Error(fmt.Sprintf("Load tasks: %w", err2))
			return
		}
		task, ok := oneTaskMap[taskId]
		if ok {
			p.tasks[taskId] = task
			wg.Add(1)
			go task.Serve(&wg) 
		} else {
			delete(p.tasks,taskId)
		}
	}
	logger.Debug("Reloading tasks done.")

}

func (p *TasksContainer) processTask(workerId int, task *amqp.Delivery) {
	// получим ID таска
	task.Ack(false)
	if task.Headers["MbTaskID"] == nil {
		task.Nack(false, false)
		logger.Error("MbTaskID header not set or empty")
		return
	}
	taskID := task.Headers["MbTaskID"].(int64)
	logger.Debug(fmt.Sprintf("%d new task: %d", workerId, taskID))
	
	tp := &taskParams{}
	jd := json.NewDecoder(strings.NewReader(task.Headers["MbTaskParams"].(string)))
	err := jd.Decode(tp)
	if err != nil {
		//task.Nack(false, false)
		logger.Error(fmt.Sprintf("MbTaskParams decode error: %w", err))
		return
	}
	
	logger.Error(fmt.Sprintf("MbTaskParams: %w", tp))
	
	if tp.Action == "ping" {
		logger.Info("pong")
		for k, v := range p.tasks { 
			logger.Info(fmt.Sprintf("task %d: %s\n", k, v))
		}
		
	} else if tp.Action == "reload" {
		p.reloadTask(tp.SchedfulerTaskId)

	} else {
		logger.Error(fmt.Sprintf("unknown action: %s", tp.Action))
	}
}
