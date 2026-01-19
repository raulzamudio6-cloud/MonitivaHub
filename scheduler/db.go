package main

import (
	"context"
	"database/sql"
	"fmt"
	"strings"
	"time"

	_ "github.com/denisenkom/go-mssqldb"
)

type DBStorage struct {
	db          *sql.DB
	ctx         context.Context
	cancel      context.CancelFunc
	techTimeout time.Duration
}

func NewDBStorage(cfg *ServiceConfig) (*DBStorage, error) {
	dbs := &DBStorage{
		db:          nil,
		techTimeout: time.Duration(cfg.SQL.TechTimeout) * time.Second,
	}

	sbConn := strings.Builder{}
	sbConn.WriteString(cfg.SQL.URL)
	sbConn.WriteString(";User Id=")
	sbConn.WriteString(cfg.SQL.Login)
	sbConn.WriteString(";Password=")
	sbConn.WriteString(cfg.SQL.Pass)

	//logger.Debug(fmt.Sprintf("DBCOnnStr (%s) dbs.techTimeout (%d)", sbConn.String(),dbs.techTimeout))

	var err error
	dbs.db, err = sql.Open("sqlserver", sbConn.String())
	if err != nil {
		fmt.Printf("connection open error: %s\r\n", err)
		return nil, err
	}
	dbs.db.SetMaxOpenConns(25)
	dbs.db.SetMaxIdleConns(25)
	dbs.db.SetConnMaxLifetime(5 * time.Minute)

	dbs.ctx, dbs.cancel = context.WithCancel(context.Background())
	ctx, cancel := context.WithTimeout(dbs.ctx, dbs.techTimeout)

	defer cancel()

	err = dbs.db.PingContext(ctx)
	if err != nil {
		fmt.Printf("databasse PingContext error: %s\r\n", err)
		return nil, err
	}

	return dbs, nil
}

func (p *DBStorage) Close() {
	p.cancel()
	if p.db != nil {
		p.db.Close()
	}
}

func (p *DBStorage) ExecCommand(ctx context.Context, command string) error {
	_, err := p.db.ExecContext(ctx, command)
	if err != nil {
		return err
	}
	return nil
}

func (p *DBStorage) LoadTasks(taskId int64) (map[int64]*Task, error) {
	const (
		qryGetCount = "select count(*) from integrations.MbSchedulerTasks where [Enabled] = 1"
		qryGetTasks = `select 
[MbSchedulerID], [MbSchedulerName], [MbSchedulerCommand],
datediff_big(second, '1970-01-01', dateadd(hour, datediff(hour, getdate(), getutcdate()),
   convert(datetime, substring([ActiveStartDate_YYYYMMDD], 1, 4) + '-' + substring([ActiveStartDate_YYYYMMDD], 5, 2) + '-' + substring([ActiveStartDate_YYYYMMDD], 7, 2) +
      ' ' + substring([ActiveStartTime_HHMMSS], 1, 2) + ':' + substring([ActiveStartTime_HHMMSS], 3, 2) + ':' + substring([ActiveStartTime_HHMMSS], 5, 2), 120))),
datediff_big(second, '1970-01-01', dateadd(hour, datediff(hour, getdate(), getutcdate()),
   convert(datetime, substring([ActiveEndDate_YYYYMMDD], 1, 4) + '-' + substring([ActiveEndDate_YYYYMMDD], 5, 2) + '-' + substring([ActiveEndDate_YYYYMMDD], 7, 2) +
      ' ' + substring([ActiveEndTime_HHMMSS], 1, 2) + ':' + substring([ActiveEndTime_HHMMSS], 3, 2) + ':' + substring([ActiveEndTime_HHMMSS], 5, 2), 120))),
[IntervalInSecondsBetweenRuns], [FrequencyType]
from integrations.MbSchedulerTasks 
where [Enabled] = 1`
	)
	ctx, cancel := context.WithTimeout(p.ctx, p.techTimeout)
	defer cancel()

	var nilMap map[int64]*Task
	var res map[int64]*Task
	// всё ли с подключением хорошо?
	err := p.db.PingContext(ctx)
	if err != nil {
		return nilMap, err
	}
	// получим кол-во тасков
	var cnt int64
	err = p.db.QueryRowContext(ctx, qryGetCount).Scan(&cnt)
	if err != nil {
		return nilMap, err
	}
	// тут загрузим таски в мапу
	res = make(map[int64]*Task, 1)
	if cnt > 0 {
		var (
			id         int64
			name       string
			command    string
			startAt    int64
			endAt      int64
			interval   int64
			periodType string
		)
		rows, err := p.db.QueryContext(ctx, qryGetTasks)
		if err != nil {
			return nilMap, err
		}
		defer rows.Close()
		// кверь отработала - загрузим таски в мапу
		res = make(map[int64]*Task, cnt)
		for rows.Next() {
			err = rows.Scan(&id, &name, &command, &startAt, &endAt, &interval, &periodType)
			if err != nil {
				break
			}
			// тут создадим таск и положим в мапу
			if taskId == 0 || taskId == id {
				if periodType == "ONCE-A-DAY" {
					// раз в день
					interval = 24 * 60 * 60
				}
				t := NewTask(p, id, name, command, startAt, endAt, interval)
				res[id] = t
			}
		}
		if err != nil {
			return nilMap, err
		}
	}
	return res, nil
}
