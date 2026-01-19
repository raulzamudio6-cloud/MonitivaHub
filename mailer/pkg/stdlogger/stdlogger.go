package stdlogger

import (
	"fmt"
	"os"
	"time"

	"go.uber.org/zap"
	"go.uber.org/zap/zapcore"
)

var (
	Logger *zap.Logger
)

func CreateLogger() (*zap.Logger, error) {
	cfg := zap.Config{
		Encoding:         "console",
		Level:            zap.NewAtomicLevelAt(zapcore.DebugLevel),
		OutputPaths:      []string{"stdout"},
		ErrorOutputPaths: []string{"stdout"},
		EncoderConfig: zapcore.EncoderConfig{
			MessageKey: "message",

			LevelKey:    "level",
			EncodeLevel: zapcore.CapitalLevelEncoder,

			TimeKey:    "time",
			EncodeTime: zapcore.TimeEncoderOfLayout("2006-01-02 15:04:05.999999-07:00"),

			CallerKey:    "caller",
			EncodeCaller: zapcore.ShortCallerEncoder,
		},
	}
	var err error
	Logger, err = cfg.Build()
	if err != nil {
		fmt.Printf(`{"level":"ERROR","time":"%s","caller":"app/app.go:38","message":"Can't create logger"}`, time.Now().Format("2006-01-02 15:04:05.999999-07:00"))
		return nil, err
	}
	return Logger, nil
}

// тут будет инициализация логгера
func init() {
	var err error
	Logger, err = CreateLogger()
	if err != nil {
		os.Exit(1)
	}
}
