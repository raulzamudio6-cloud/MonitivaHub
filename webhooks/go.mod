module gitlab.canopus.ru/macrobank_5_7/core_api/go-services/mb_webhooks

go 1.17

replace gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/stdlogger => ../lib/pkg/stdlogger

replace gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/rabbitworker => ../lib/pkg/rabbitworker

require (
	github.com/spf13/viper v1.9.0
	gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/rabbitworker v0.0.0-00010101000000-000000000000
	gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/stdlogger v0.0.0-00010101000000-000000000000
	go.uber.org/zap v1.19.1
)

require github.com/streadway/amqp v1.0.0
