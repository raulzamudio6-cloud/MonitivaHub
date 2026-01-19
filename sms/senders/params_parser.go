package smssenders

import "fmt"

const (
	parseErrTemplate = "SMS Sender %s: param with name \"%s\" not found"
)

type paramsParser struct {
}

func (p paramsParser) Parse(senderName string, params map[string]string, paramName string) (string, error) {
	s, ok := params[paramName]
	if !ok {
		return "", fmt.Errorf(parseErrTemplate, senderName, paramName)
	}
	return s, nil
}
