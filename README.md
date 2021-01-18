# Application Connector
## ENG
**Application Connector** - A library that is capable of transferring data from one DotNet application to another DotNet application by means of memory formation.
Download library - [Application Connector](https://github.com/danrom11/Application_Connector/releases)

## Methods:

### Send(string `data`, string `memoryName`)
Write-send data to memory application.
#### Parameters:
string `data` - data to be transferred.</br>
string `memoryName` - Variable memory department name.

### Accept(string `memoryName`)
Accept-read data.
#### Parameters:
string `memoryName` - Variable memory department name.

## Usage examples:
Given application `OneApp`, it should pass data / variable value to `TwoApp`.
### Code in `OneApp`

```C#
using Application_Connector;

string param = "start check";

Application_Connector.Connector.Send(param, "pam1");

```

### Code in `TwoApp`
```C#
using Application_Connector;

string result = Application_Connector.Connector.Accept("pam1");

MessageBox.Show(result);

```

## RU
**Application Connector** - Библиотека которая способна передавать данные из одно DotNet приложение в другое DotNet приложение, по средством формирования памяти.  
Скачать библиотеку - [Application Connector](https://github.com/danrom11/Application_Connector/releases)

## Методы:

### Send(string `data`, string `memoryName`)
Запись-отправка данных в приложение памяти.
#### Параметры:
string `data` - данные для передачи.</br>
string `memoryName` - Название отдела переменной памяти.

### Accept(string `memoryName`)
Принять-прочитать данные.
#### Параметры:
string `memoryName` - Название отдела переменной памяти.

## Примеры использования
Дано приложение `OneApp`, оно должно передать данные/значение переменной в `TwoApp`.
### Код в `OneApp`

```C#
using Application_Connector;

string param = "start check";

Application_Connector.Connector.Send(param, "pam1");

```

### Код в `TwoApp`
```C#
using Application_Connector;

string result = Application_Connector.Connector.Accept("pam1");

MessageBox.Show(result);

```
