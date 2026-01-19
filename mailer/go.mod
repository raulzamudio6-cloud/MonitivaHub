module gitlab.canopus.ru/macrobank_5_7/core_api/go-services/mb_mailer

go 1.17

replace gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/stdlogger => ./pkg/stdlogger

replace gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/rabbitworker => ./pkg/rabbitworker

require (
	github.com/sendgrid/sendgrid-go v3.10.3+incompatible
	github.com/spf13/viper v1.9.0
	github.com/streadway/amqp v1.0.0
	github.com/xhit/go-simple-mail/v2 v2.10.0
	gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/rabbitworker v0.0.0-00010101000000-000000000000
	gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/stdlogger v0.0.0-00010101000000-000000000000
	go.uber.org/zap v1.19.1
)

require (
	github.com/fsnotify/fsnotify v1.5.1 // indirect
	github.com/hashicorp/hcl v1.0.0 // indirect
	github.com/magiconair/properties v1.8.5 // indirect
	github.com/mitchellh/mapstructure v1.4.2 // indirect
	github.com/pelletier/go-toml v1.9.4 // indirect
	github.com/sendgrid/rest v2.6.5+incompatible // indirect
	github.com/spf13/afero v1.6.0 // indirect
	github.com/spf13/cast v1.4.1 // indirect
	github.com/spf13/jwalterweatherman v1.1.0 // indirect
	github.com/spf13/pflag v1.0.5 // indirect
	github.com/subosito/gotenv v1.2.0 // indirect
	go.uber.org/atomic v1.7.0 // indirect
	go.uber.org/multierr v1.6.0 // indirect
	golang.org/x/sys v0.0.0-20210823070655-63515b42dcdf // indirect
	golang.org/x/text v0.3.6 // indirect
	gopkg.in/ini.v1 v1.63.2 // indirect
	gopkg.in/yaml.v2 v2.4.0 // indirect
)
