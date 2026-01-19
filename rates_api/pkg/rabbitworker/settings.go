package rabbitworker

import (
	"github.com/spf13/viper"
)

type Settings struct {
	URL string
	// откуда берем сообщения для обработки
	IN struct {
		QName string
	}
	// куда пишем ответы
	OUT struct {
		Exchange string
		QName    string
	}
}

func loadConfig() (*Settings, error) {
	cfg := &Settings{}

	viper.SetDefault("RABBIT_PORT", 5672)
	rHost := viper.GetString("RABBIT_HOST")
	rPort := viper.GetString("RABBIT_PORT")
	rUser := viper.GetString("RABBIT_USER")
	rPass := viper.GetString("RABBIT_PASSWORD")

	cfg.URL = "amqp://" + rUser + ":" + rPass + "@" + rHost + ":" + rPort + "/"

	cfg.IN.QName = viper.GetString("AMQP_IN_QUEUE")

	cfg.OUT.QName = viper.GetString("AMQP_OUT_QUEUE")
	cfg.OUT.Exchange = viper.GetString("AMQP_OUT_EXCH")

	return cfg, nil

}
