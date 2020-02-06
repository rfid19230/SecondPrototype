/*
 * Данный файл содержит код для микроконтроллера, используемый во втором прототипе
 * системы автоматизированного учета и выдачи материальных ценностей.
 * Алгоритм работы микроконтроллера подробно описан в документе SecondPrototypeDescription.docx.
 */

 /*
  * В данной версии прототипа сервер и микроконтроллер обмениваются данными по Ethernet,
  * поэтому последовательный COM-порт №0 используется ТОЛЬКО для вывода сообщений о
  * работе системы, которые предназначались для контроля правильности ее работы и отладки.
  * Все строки, содержащие слово "Serial" (НО НЕ Serial1), могут быть удалены и
  * будут удалены в финальной версии кода микроконтроллера, которая ориентировочно будет
  * готова во второй половине февраля 2020 года.
  */

// Подключение необходимых заголовочных файлов
#include <SPI.h>
#include <Ethernet.h>

// Список используемых команд
#define COMMAND_CONNECT        '0'
#define COMMAND_DISCONNECT     '1'
#define COMMAND_VALID_PASS     '2'
#define COMMAND_INVALID_PASS   '3'
#define COMMAND_END_SESSION    '4'
#define COMMAND_VALUE_TAKEN    '5'
#define COMMAND_VALUE_RETURNED '6'
#define COMMAND_INVALID_VALUE  '7'

// Определение задержек между попытками подключения к сети и серверу соответственно, мс
#define CONN_NET_DELAY 10000
#define CONN_SRV_DELAY 10000

// Определение пина, к которому подключен герметизированный контакт, а также его состояний для удобства
#define GERCON_PIN  22
#define GERCON_CLOSED 0
#define GERCON_OPENED 1

// Определение пина, с помощью которого через реле управляется замок
#define LOCK_PIN 24

/* 
 *  Содержимое UID используемых меток
 * 0 символ - знак начала передачи (числовой код 2) 
 * 1-2 символы - служебные
 * 3-10 символы - собственно UID
 * 11-12 символы - проверочные
 * 13 символ - знак конца передачи (числовой код 3)
 */
#define UID_LEN 14
#define UID_FROM 3
#define UID_TO 10

// Определение задержки между двумя считываниями
#define READ_RFID_DELAY 2000

// Определение времени на открытие шкафа после успешного прикладывания пропуска
#define TIME_TO_OPEN 10000

// MAC-адрес микроконтроллера
const byte mac[] = {0xDE, 0xAD, 0xBE, 0xEF, 0xFE, 0xED};
// IP-адрес и порт сервера
const byte srv_ip[] = {172, 18, 160, 26};
const int srv_port = 19230;

// Для проверки подключения к сети и серверу в setup
bool conn_net;
bool conn_srv;

// Объект Ethernet-клиента
EthernetClient client;

// Для хранения последней полученной команды
char command;

// Для хранения последнего считанного UID
char uid[UID_LEN];

// Для определения, нужно ли пропустить часть кода в loop (чтобы не использовать continue)
bool next_loop;

// Для подсчета интервалов времени
unsigned long m;
unsigned long curr_m;

// Для хранения последнего считанного сигнала геркона
int gs;

void setup() {
/*
 * Функция установки, вызывается при подаче питания на Arduino и нажатии кнопки RESET. Также может быть вызвана в других функциях. Выполняется 1 раз.
 * Входных парамеров нет
 * Выходных параметров нет
 */
  // инициализация команды и проверочных переменных
  command = COMMAND_CONNECT;
  conn_net = false;
  conn_srv = false;

  // Запуск COM-портов и ожидание его окончания
  Serial.begin(9600);
  Serial1.begin(9600);
  while (!Serial || !Serial1) delay(10);
  clearSerial();
  clearSerial1();

  Serial.println("Setup started");

  // Настройка пинов периферии
  pinMode(GERCON_PIN, INPUT_PULLUP);
  pinMode(LOCK_PIN, OUTPUT);
  pinMode(LED_BUILTIN, OUTPUT);

  // Закрытие замка
  lock();

  // Попытка подключения к сети до успеха
  while (!conn_net) {
    if (Ethernet.begin(mac) == 0) {
      Serial.println("Failed to start Eth");
      delay(CONN_NET_DELAY);
    }
    else {
      conn_net = true;
    }
  }
  Serial.print("Eth started, my IP: ");
  Serial.println(Ethernet.localIP());

  // Попытка подключения к серверу до успеха
  while (!conn_srv) {
    if (client.connect(srv_ip, srv_port)) {
      conn_srv = true;
      client.print("00000000");
      client.stop();
    }
    else {
      Serial.println("Connection to srv: failed");
      delay(CONN_SRV_DELAY);
    }
  }
  Serial.println("Connection to srv: successful");

  Serial.println("Setup done");
}


void loop() {
/*
 * Функция описывает постоянно выполняемые в бесконечном цикле действия. После завершения вызывается снова.
 * Входных параметров нет
 * Выходных параметров нет
 */

  Serial.println("Loop started");

  // Изначально пропуск части кода loop не нужен
  next_loop = false;

  // Ожидание пропуска
  Serial.println("Waiting for pass");
  if (command == COMMAND_CONNECT || command == COMMAND_INVALID_PASS) {
    // При указанных выше командах ожидание метки пропуска
    while (Serial1.available() < UID_LEN) delay(100);
  }
  else {
    if (command == COMMAND_VALUE_TAKEN || command == COMMAND_VALUE_RETURNED || command == COMMAND_INVALID_VALUE) {
      // При указанных выше командах ожидание метки ценности
      Serial.println("Waiting for value");
      
      gs = digitalRead(GERCON_PIN);
      while ((Serial1.available() < UID_LEN) && (gs == GERCON_OPENED)) {
        delay(100);
        gs = digitalRead(GERCON_PIN);
      }
      
      if (gs == GERCON_CLOSED) {
        // Если при ожидании ценности получен сигнал о закрытии щкафа
        Serial.println("Door was closed");
        // Пропуск части кода в дальнейшем
        next_loop = true;
        // Закрытие замка
        lock();
        
        if (client.connect(srv_ip, srv_port)) {
          // Отправка сообщения о завершении работы пользователя с системой
          client.print("FFFFFFFF");
          
          while (!client.available()) delay(10);

          command = client.read();
          Serial.print("Code of received command: ");
          Serial.println(command);
          Serial.print("New command: ");
          command = COMMAND_CONNECT;
          Serial.println(command);
        }
        else {
          // При ошибке соединения переход в setup
          setup();
        }
      }
      
    }
    else {
      if (command == COMMAND_VALID_PASS) {
        // При прикладывании валидного пропуска
        m = millis();
        curr_m = millis();
        gs = digitalRead(GERCON_PIN);
        Serial.println("Waiting for door opening");
        while ((gs  == GERCON_CLOSED) && (curr_m - m < TIME_TO_OPEN)) {
          // Ожидание открытия двери
          delay(100);
          gs = digitalRead(GERCON_PIN);
          curr_m = millis();
        }

        if (gs == GERCON_CLOSED) {
          // Дверь не была открыта 
          Serial.println("Opening timeout");
          next_loop = true;
          // Закрытие двери
          lock();
        
          if (client.connect(srv_ip, srv_port)) {
            // Отправка сообщения о завершении работы пользователя с системой
            client.print("FFFFFFFF");
          
            while (!client.available()) delay(10);

            command = client.read();
            command = COMMAND_CONNECT;
          }
          else {
            // При ошибке подключения переход в setup
            setup();
          }
        }
        else {
          // Дверь была открыта
          Serial.println("Door was opened");
          next_loop = true;
          command = COMMAND_INVALID_VALUE;
        }
        
      }
    }
  }

  if (!next_loop) {
    // Если был считан UID и его нужно считать и отправить

    // Считывание UID
    for (int i = 0; i < UID_LEN; ++i) {
      uid[i] = Serial1.read();
    }
    
    Serial.println("UID was read: ");
    for (int i = UID_FROM; i <= UID_TO; ++i) {
      Serial.print(uid[i]);
    }
    Serial.println();

    // Отправка UID
    if (client.connect(srv_ip, srv_port)) {
      for (int i = UID_FROM; i <= UID_TO; ++i) {
        client.print(uid[i]);
      }

      // Ожидание ответа и его получение
      while (!client.available()) delay(10);
    
      command = client.read();
      Serial.print("COMMAND received");
      Serial.println(command);

      // Открытие двери, если приложен валидный пропуск
      if (command == COMMAND_VALID_PASS) {
        unlock();
      }
    }
    else {
      // Переход в setup при ошибке соединения
      setup();
    }

    // Задержка между считываниями и очистка последовательного порта
    delay(READ_RFID_DELAY);
    clearSerial1();
  }
}

void clearSerial() {
/*
 * Функция для очистки входного буфера последовательного порта 0 
 * Входных параметров нет
 * Выходных параметров нет
 */
 
  while (Serial.available()) {
    Serial.read();
  }
}

void clearSerial1() {
/*
 * Функция для очистки входного буфера последовательного порта 1
 * Входных параметров нет
 * Выходных параметров нет
 */
  while (Serial1.available()) {
    Serial1.read();
  }
}

void unlock() {
/*
 * Функция, предназначенная для открытия замка
 * Входных параметров нет
 * Выходных параметров нет
 */

  Serial.println("Lock unlocked");
  digitalWrite(LOCK_PIN, HIGH);
  ledOff();
}

void lock() {
/*
 * Функция, предназначенная для закрытия замка
 * Входных параметров нет
 * Выходных параметров нет
 */
  
  Serial.println("Lock locked");
  digitalWrite(LOCK_PIN, LOW);
  ledOn();
}

void ledOn() {
/*
 * Функция, предназначенная для включения индикации
 * Входных параметров нет
 * Выходных параметров нет
*/

  digitalWrite(LED_BUILTIN, HIGH);
}

void ledOff() {
/*
 * Функция, предназначенная для выключения индикации
 * Входных параметров нет
 * Выходных параметров нет
*/

  digitalWrite(LED_BUILTIN, LOW);
}
