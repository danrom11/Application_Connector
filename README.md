# Application Connector
## ENG
**Application Connector** - A library that is capable of transferring data from one DotNet application to another DotNet application by means of memory formation.
## Usage examples:
Given application `OneApp`, it should pass data / variable value to` TwoApp`.
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
