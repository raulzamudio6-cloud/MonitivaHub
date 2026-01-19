package main

import (
	"fmt"
	"net/http"
	"net/url"
	"time"
)

func NewHTTPClient(cfg *ServiceConfig) (*http.Client, error) {
	// без прокси ?
	fmt.Printf("NewHTTPClient RequestTimeout: %s\r\n",cfg.RequestTimeout)
	if cfg.Proxy.URL == "" {
		return &http.Client{
			Timeout: cfg.RequestTimeout,
		}, nil
	}
	// проверяем нужна ли авторизация
	proxyStr := cfg.Proxy.URL
	if cfg.Proxy.User != "" && cfg.Proxy.Password != "" {
		u, err := url.Parse(proxyStr)
		if err != nil {
			return nil, err
		}
		//   "http://" + user + ":" + password + "@" + ip + ":" + port
		proxyStr = fmt.Sprintf("%s://%s:%s@%s",
			u.Scheme,
			cfg.Proxy.User,
			cfg.Proxy.Password,
			u.Host)
	}
	proxyURL, err := url.Parse(proxyStr)
	if err != nil {
		return nil, err
	}
	transport := &http.Transport{
		Proxy: http.ProxyURL(proxyURL),
	}
	return &http.Client{
		Transport: transport,
		Timeout:   cfg.RequestTimeout * time.Second,
	}, nil
}
