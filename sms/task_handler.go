package main

import (
	"encoding/json"
	"errors"
	"fmt"
	"net/http"
	"sort"
	"strings"
	"sync"

	"github.com/streadway/amqp"
	"gitlab.canopus.ru/macrobank_5_7/core_api/go-services/internal/sms/smssenders"
	"gitlab.canopus.ru/macrobank_5_7/core_api/go-services/pkg/rabbitworker"
)

type TaskHandler struct {
	client *http.Client

	rw *rabbitworker.Worker

	// провайдеры отправщиков
	senders []SMSSender

	chResp  chan ResponseData
	CloseCh chan int
}

// USA prefix codes
var usAreaCodes = map[string]bool{
	"201": true, "202": true, "203": true, "205": true, "206": true,
	"207": true, "208": true, "209": true, "210": true, "212": true,
	"213": true, "214": true, "215": true, "216": true, "217": true,
	"218": true, "219": true, "220": true, "224": true, "225": true,
	"228": true, "229": true, "231": true, "234": true, "239": true,
	"240": true, "248": true, "251": true, "252": true, "253": true,
	"254": true, "256": true, "260": true, "262": true, "267": true,
	"269": true, "270": true, "272": true, "276": true, "281": true,
	"301": true, "302": true, "303": true, "304": true, "305": true,
	"307": true, "308": true, "309": true, "310": true, "312": true,
	"313": true, "314": true, "315": true, "316": true, "317": true,
	"318": true, "319": true, "320": true, "321": true, "323": true,
	"325": true, "327": true, "330": true, "331": true, "334": true,
	"336": true, "337": true, "339": true, "341": true, "346": true,
	"347": true, "351": true, "352": true, "360": true, "361": true,
	"364": true, "380": true, "385": true, "386": true, "401": true,
	"402": true, "404": true, "405": true, "406": true, "407": true,
	"408": true, "409": true, "410": true, "412": true, "413": true,
	"414": true, "415": true, "417": true, "419": true, "423": true,
	"424": true, "425": true, "430": true, "432": true, "434": true,
	"435": true, "440": true, "442": true, "443": true, "447": true,
	"458": true, "463": true, "464": true, "469": true, "470": true,
	"475": true, "478": true, "479": true, "480": true, "484": true,
	"501": true, "502": true, "503": true, "504": true, "505": true,
	"507": true, "508": true, "509": true, "510": true, "512": true,
	"513": true, "515": true, "516": true, "517": true, "518": true,
	"520": true, "530": true, "531": true, "534": true, "539": true,
	"540": true, "541": true, "551": true, "559": true, "561": true,
	"562": true, "563": true, "564": true, "567": true, "570": true,
	"571": true, "573": true, "574": true, "575": true, "580": true,
	"585": true, "586": true, "601": true, "602": true, "603": true,
	"605": true, "606": true, "607": true, "608": true, "609": true,
	"610": true, "612": true, "614": true, "615": true, "616": true,
	"617": true, "618": true, "619": true, "620": true, "623": true,
	"626": true, "628": true, "629": true, "630": true, "631": true,
	"636": true, "641": true, "646": true, "650": true, "651": true,
	"657": true, "660": true, "661": true, "662": true, "667": true,
	"669": true, "678": true, "681": true, "682": true, "689": true,
	"701": true, "702": true, "703": true, "704": true, "706": true,
	"707": true, "708": true, "712": true, "713": true, "714": true,
	"715": true, "716": true, "717": true, "718": true, "719": true,
	"720": true, "724": true, "725": true, "727": true, "730": true,
	"731": true, "732": true, "734": true, "737": true, "740": true,
	"743": true, "747": true, "752": true, "754": true, "757": true,
	"760": true, "762": true, "763": true, "765": true, "769": true,
	"770": true, "772": true, "773": true, "774": true, "775": true,
	"779": true, "781": true, "785": true, "786": true, "801": true,
	"802": true, "803": true, "804": true, "805": true, "806": true,
	"808": true, "810": true, "812": true, "813": true, "814": true,
	"815": true, "816": true, "817": true, "818": true, "828": true,
	"830": true, "831": true, "832": true, "835": true, "838": true,
	"839": true, "840": true, "843": true, "845": true, "847": true,
	"848": true, "850": true, "854": true, "856": true, "857": true,
	"858": true, "859": true, "860": true, "862": true, "863": true,
	"864": true, "865": true, "870": true, "872": true, "878": true,
	"901": true, "903": true, "904": true, "906": true, "907": true,
	"908": true, "909": true, "910": true, "912": true, "913": true,
	"914": true, "915": true, "916": true, "917": true, "918": true,
	"919": true, "920": true, "925": true, "927": true, "928": true,
	"929": true, "930": true, "931": true, "934": true, "936": true,
	"937": true, "938": true, "940": true, "941": true, "945": true,
	"947": true, "948": true, "949": true, "951": true, "952": true,
	"954": true, "956": true, "959": true, "970": true, "971": true,
	"972": true, "973": true, "975": true, "978": true, "979": true,
	"980": true, "984": true, "985": true, "986": true, "989": true,
}


// обработчик реквестов, пришедших с rabbit
func NewTaskHandler(cfg *ServiceConfig, client *http.Client, rw *rabbitworker.Worker) (*TaskHandler, error) {
	res := &TaskHandler{
		client:  client,
		rw:      rw,
		chResp:  make(chan ResponseData),
		CloseCh: make(chan int),
	}
	// посмотрим сколько активных отправщиков
	var enabledSenders int = 0
	for i := range cfg.Senders {
		if cfg.Senders[i].Enabled {
			enabledSenders++
		}
	}

	if enabledSenders == 0 {
		return res, fmt.Errorf("there are no enabled senders in config")
	}

	// подготовим слайс
	res.senders = make([]SMSSender, enabledSenders)
	senderIndex := 0
	var s SMSSender
	// создадим обработчики
	for i := range cfg.Senders {
		s = nil
		if cfg.Senders[i].Enabled {
			switch strings.ToLower(cfg.Senders[i].Name) {
			case "vertex":
				s = smssenders.NewVertexSender(cfg.Senders[i].Priority)
			case "clickatell":
				s = smssenders.NewClickatellSender(cfg.Senders[i].Priority)
			case "cardboardfish":
				s = smssenders.NewCardBoardFishSender(cfg.Senders[i].Priority)
			case "nexmo":
				s = smssenders.NewNexmoSender(cfg.Senders[i].Priority)
			case "etimsalat":
				s = smssenders.NewEtimsalatSender(cfg.Senders[i].Priority)
			case "websms":
				s = smssenders.NewWebSmsSender(cfg.Senders[i].Priority)
			case "twilio":
				s = smssenders.NewTwilioSender(cfg.Senders[i].Priority)
			case "infobip":
				s = smssenders.NewInfoBipSender(cfg.Senders[i].Priority)
			default:
				return res, fmt.Errorf("unknown sender in config: %s", cfg.Senders[i].Name)
			}
		} else {
			continue
		}
		// обработчик был инициализирован?
		err := s.Init(cfg.Senders[i].Params, cfg.Senders[i].Priority)
		if err != nil {
			logger.Error(err.Error())
			return res, err
		}
		res.senders[senderIndex] = s
		senderIndex++
	}

	// сортируем получившийся список по приоритету
	sort.Slice(res.senders, func(i, j int) bool { return res.senders[i].GetPriority() < res.senders[j].GetPriority() })

	logger.Info("starting Task handler... OK")
	return res, nil
}

func (p *TaskHandler) processTask(workerId int, task *amqp.Delivery, wg *sync.WaitGroup) {
	//defer wg.Done()

	type taskParams struct {
		MessageQID int32 `json:"message_queue_id"`
	}

	// получим ID таска
	if task.Headers["MbTaskID"] == nil {
		task.Nack(false, false)
		p.returnResp(0, 0, errors.New("MbTaskID header not set or empty"), "")
		return
	}
	taskID := task.Headers["MbTaskID"].(int64)
	tp := &taskParams{}
	jd := json.NewDecoder(strings.NewReader(task.Headers["MbTaskParams"].(string)))
	err := jd.Decode(tp)
	if err != nil {
		task.Nack(false, false)
		p.returnResp(taskID, tp.MessageQID, fmt.Errorf("MbTaskParams decode error: %w", err), "")
		return
	}

	pl := &PayloadData{}

	// парсим в структуру
	err = json.Unmarshal(task.Body, pl)
	if err != nil {
		task.Nack(false, false)
		p.returnResp(taskID, tp.MessageQID, fmt.Errorf("payload unmarshal error: %w", err), "")
		return
	}

	// валидация
	if pl.To == "" {
		task.Nack(false, false)
		p.returnResp(taskID, tp.MessageQID, errors.New("field 'To' not set"), "")
		return
	}
	if pl.Message == "" {
		task.Nack(false, false)
		p.returnResp(taskID, tp.MessageQID, errors.New("field 'Message' not set"), "")
		return
	}
	// пробуем послать
	logger.Debug(fmt.Sprintf("%d new task:%d", workerId, taskID))
	err = p.SendMessage(pl)
	if err != nil {
		task.Nack(false, false)
		p.returnResp(taskID, tp.MessageQID, fmt.Errorf("sent error: %w", err), pl.To)
		return
	}
	task.Ack(false)
	p.returnResp(taskID, tp.MessageQID, nil, pl.To)
}

func (p *TaskHandler) handleResponses(wg *sync.WaitGroup) {
	wg.Done()
	// структура для создания body ответной посылки
	type RespBodyData struct {
		MsgQID  int32  `json:"message_queue_id"`
		Message string `json:"error_message,omitempty"`
	}
	//
	rbd := &RespBodyData{}
	//
	for {
		resp, ok := <-p.chResp
		if !ok {
			return
		}
		// подготовим посылку с ответом
		respValue := "OK"
		if resp.code == 0 {
			rbd.MsgQID = resp.msgQID
			rbd.Message = ""
			logger.Info(fmt.Sprintf("SUCCESS | T#%d| MsgID#%d | To:%s | SUCCESS.", resp.taskID, resp.msgQID, resp.phone))
		} else {
			respValue = "ERR"
			rbd.MsgQID = resp.msgQID
			rbd.Message = resp.message
			logger.Error(fmt.Sprintf("ERROR | T#%d| MsgID#%d | To:%s | code: %d, message: %s.",
				resp.taskID, resp.msgQID, resp.phone, resp.code, resp.message))
		}
		body, err := json.Marshal(rbd)
		if err != nil {
			logger.Error(fmt.Sprintf("| T#%d| MsgID#%d | marshall error: %s",
				resp.taskID, resp.msgQID, err.Error()))
			continue
		}
		err = p.rw.Respond(resp.taskID, int32(resp.code), respValue, body)
		if err != nil {
			logger.Error(fmt.Sprintf("| T#%d| MsgID#%d | can not send an answer", resp.taskID, resp.msgQID))
			continue
		}
	}
}

func (p *TaskHandler) returnResp(taskID int64, msgQID int32, err error, phone string) {
	masked := maskPhone(phone)
	if err != nil {
		p.chResp <- ResponseData{
			taskID:  taskID,
			msgQID:  msgQID,
			code:    1,
			message: err.Error(),
			phone:   masked,
		}
	} else {
		p.chResp <- ResponseData{
			taskID:  taskID,
			msgQID:  msgQID,
			code:    0,
			message: "OK",
			phone:   masked,
		}
	}
}

func (p *TaskHandler) Serve(workersCount int) {
	var wg sync.WaitGroup

	// запустим читатель ответов
	wg.Add(1)
	go p.handleResponses(&wg)
	wg.Wait()

	defer func() {
		wg.Wait()
	}()

	que := make(chan amqp.Delivery, workersCount)
	go func() {
		<-p.rw.CloseCh
		logger.Debug("Closing queues")
		close(p.chResp)
		close(que)
		wg.Wait()
		logger.Debug(fmt.Sprintf("CLose p.Ch"))
		close(p.CloseCh)
	}()

	logger.Debug(fmt.Sprintf("Starting %d workers", workersCount))
	for i := 1; i <= workersCount; i++ {
		go func(que chan amqp.Delivery, workerId int) {
			wg.Add(1)
			for {
				task, ok := <-que
				if !ok {
					logger.Debug(fmt.Sprintf("Exiting worker %d", workerId))
					wg.Done()
					return
				}
				p.processTask(workerId, &task, &wg)
			}
		}(que, i)
	}

	logger.Debug("Start reading input messages")
	for m := range p.rw.GetInputMsgChan() {
		que <- m
	}
}

func (p *TaskHandler) Shutdown() {}

func (p *TaskHandler) SendMessage(pl *PayloadData) error {
	var err error
	normalized := normalizePhone(pl.To)

	for i := range p.senders {
		err = p.senders[i].SendMessage(p.client, normalized, pl.Message, pl.Whatsapp)
		if err != nil {
			logger.Error(err.Error())
			continue
		} else {
			return nil
		}
	}
	return fmt.Errorf("none of %d senders could send the message: %s", len(p.senders), err)
}

// maskPhone скрывает часть номера, оставляя первые 2 и последние 2 символа
func maskPhone(phone string) string {
	l := len(phone)
	if l <= 4 {
		return phone
	}
	return phone[:2] + strings.Repeat("*", l-4) + phone[l-2:]
}

// normalizePhone removes '+' for US numbers, keeps '+' for others
func normalizePhone(phone string) string {
	if strings.HasPrefix(phone, "+1") {
		if isUS(phone) {
			logger.Debug(fmt.Sprintf("Normalizing US number: %s -> %s", phone, phone[1:]))
			return phone[1:]
		}
	}
	return phone
}

func isUS(phone string) bool {
	if len(phone) < 5 {
		return false
	}
	areaCode := phone[2:5]
	return usAreaCodes[areaCode]
}
