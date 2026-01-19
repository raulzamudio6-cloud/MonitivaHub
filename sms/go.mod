module gitlab.canopus.ru/macrobank_5_7/core_api/go-services/mb_sms

go 1.17

replace gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/stdlogger => ./pkg/stdlogger

replace gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/rabbitworker => ./pkg/rabbitworker

replace gitlab.canopus.ru/macrobank_5_7/core_api/go-services/internal/sms/smssenders => ./senders

require (
	github.com/spf13/viper v1.9.0
	github.com/streadway/amqp v1.0.0
	gitlab.canopus.ru/macrobank_5_7/core_api/go-services/internal/sms/smssenders v0.0.0-00010101000000-000000000000
	gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/rabbitworker v0.0.0-00010101000000-000000000000
	gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/stdlogger v0.0.0-00010101000000-000000000000
	go.uber.org/zap v1.19.1
)

require (
	github.com/fsnotify/fsnotify v1.5.1 // indirect
	github.com/golang/mock v1.6.0 // indirect
	github.com/hashicorp/hcl v1.0.0 // indirect
	github.com/magiconair/properties v1.8.5 // indirect
	github.com/mitchellh/mapstructure v1.4.2 // indirect
	github.com/pelletier/go-toml v1.9.4 // indirect
	github.com/pkg/errors v0.9.1 // indirect
	github.com/spf13/afero v1.6.0 // indirect
	github.com/spf13/cast v1.4.1 // indirect
	github.com/spf13/jwalterweatherman v1.1.0 // indirect
	github.com/spf13/pflag v1.0.5 // indirect
	github.com/subosito/gotenv v1.2.0 // indirect
	github.com/twilio/twilio-go v1.15.0 // indirect
	go.uber.org/atomic v1.9.0 // indirect
	go.uber.org/multierr v1.7.0 // indirect
	golang.org/x/sys v0.5.0 // indirect
	golang.org/x/text v0.13.0 // indirect
	gopkg.in/ini.v1 v1.64.0 // indirect
	gopkg.in/yaml.v2 v2.4.0 // indirect
)
