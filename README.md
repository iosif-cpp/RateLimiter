# RateLimiter

Микросервисная система ограничения скорости запросов (Rate Limiting), построенная на архитектуре CQRS с использованием ASP.NET Core, .NET 9, gRPC, MongoDB, Redis и Apache Kafka.

## 📋 Описание

RateLimiter — это распределенная система для управления лимитами запросов к API. Система отслеживает количество запросов пользователей к различным эндпоинтам и автоматически блокирует пользователей, превышающих установленные лимиты.

### Основные возможности

- ✅ **Управление лимитами по маршрутам** — настройка лимитов запросов в минуту для каждого эндпоинта
- ✅ **Автоматическая блокировка** — временная блокировка пользователей при превышении лимита
- ✅ **CQRS архитектура** — разделение операций чтения и записи для масштабируемости
- ✅ **Real-time обновления** — синхронизация лимитов через MongoDB Change Streams
- ✅ **Кэширование** — использование Redis для быстрого доступа к счетчикам запросов
- ✅ **Асинхронная обработка** — обработка событий через Apache Kafka
- ✅ **Валидация данных** — проверка корректности данных через FluentValidation

## 🏗️ Архитектура

Система состоит из четырех основных микросервисов:

```
┌─────────────────────┐
│  UserService        │  ──► Управление пользователями (PostgreSQL)
└─────────────────────┘
         │
         ▼
┌─────────────────────┐
│  Kafka              │  ──► Очередь событий пользователей
└─────────────────────┘
         │
         ▼
┌─────────────────────┐
│  RateLimiter.Reader │  ──► Чтение лимитов, обработка событий
│  - gRPC API         │
│  - Kafka Consumer   │
│  - Redis Cache      │
│  - MongoDB Watch    │
└─────────────────────┘
         ▲
         │
┌─────────────────────┐
│  RateLimiter.Writer │  ──► Управление лимитами (CRUD)
│  - gRPC API         │
│  - MongoDB          │
└─────────────────────┘
```

### Компоненты системы

#### 1. **RateLimiter.Writer**
Сервис для управления лимитами запросов:
- Создание, обновление, удаление лимитов
- Валидация данных через FluentValidation
- Хранение данных в MongoDB
- gRPC API для управления лимитами

#### 2. **RateLimiter.Reader**
Сервис для чтения лимитов и обработки событий:
- Получение всех лимитов через gRPC
- Кэширование лимитов в памяти (ConcurrentDictionary)
- Отслеживание изменений через MongoDB Change Streams
- Обработка событий пользователей из Kafka
- Управление счетчиками запросов в Redis
- Автоматическая блокировка пользователей при превышении лимита

#### 3. **UserService**
Сервис управления пользователями:
- Работа с PostgreSQL через SSH-туннель
- gRPC API для работы с пользователями
- Dapper для работы с базой данных

#### 4. **UserRequestsKafkaGenerator**
Утилита для генерации тестовых данных:
- Генерация событий пользователей
- Отправка событий в Kafka для тестирования системы

## 🛠️ Технологический стек

### Backend
- **ASP.NET Core** — веб-фреймворк для построения микросервисов
- **.NET 9.0** — платформа разработки
- **gRPC** — межсервисная коммуникация
- **FluentValidation** — валидация данных
- **Riok.Mapperly** — генерация маппингов

### Базы данных
- **MongoDB** — основное хранилище лимитов
- **PostgreSQL** — база данных пользователей
- **Redis** — кэширование счетчиков запросов

### Инфраструктура
- **Apache Kafka** — очередь сообщений для асинхронной обработки
- **Docker Compose** — оркестрация контейнеров
- **SSH Tunnel** — безопасное подключение к PostgreSQL

## 📦 Структура проекта

```
RateLimiter/
├── RateLimiter.Reader/          # Сервис чтения лимитов
│   ├── AppLayer/                # Бизнес-логика
│   ├── Controllers/             # gRPC контроллеры
│   ├── DAL/                     # Доступ к данным (MongoDB)
│   ├── Domain/                  # Доменные сущности
│   ├── Kafka/                   # Kafka Consumer
│   ├── Redis/                   # Redis сервисы
│   └── Protos/                  # gRPC протобуфы
│
├── RateLimiter.Writer/          # Сервис записи лимитов
│   ├── API/                     # gRPC API
│   ├── AppLayer/                # Бизнес-логика и валидация
│   ├── DAL/                     # Доступ к данным (MongoDB)
│   └── Domain/                   # Доменные сущности
│
├── UserService/                 # Сервис пользователей
│   ├── Api/                     # gRPC API
│   ├── AppLayer/                # Бизнес-логика
│   ├── DAL/                     # Доступ к данным (PostgreSQL)
│   ├── Database/                # SQL скрипты
│   └── SshConnection/           # SSH туннель
│
└── UserRequestsKafkaGenerator/  # Генератор тестовых данных
    ├── Common/                  # Общие компоненты
    ├── Models/                  # Модели данных
    └── Services/                # Kafka Producer
```

## 🚀 Быстрый старт

### Предварительные требования

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started) и Docker Compose
- [MongoDB](https://www.mongodb.com/try/download/community) (или через Docker)
- [Redis](https://redis.io/download) (или через Docker)
- [Apache Kafka](https://kafka.apache.org/downloads) (или через Docker)

### Установка и запуск

1. **Клонируйте репозиторий**
   ```bash
   git clone <repository-url>
   cd RateLimiter
   ```

2. **Запустите инфраструктуру через Docker Compose**

   Запустите Kafka и Zookeeper:
   ```bash
   cd UserRequestsKafkaGenerator
   docker-compose up -d
   ```

   Запустите Redis:
   ```bash
   cd RateLimiter.Reader
   docker-compose up -d
   ```

3. **Настройте MongoDB**

   Убедитесь, что MongoDB запущен на `localhost:27017` или измените строку подключения в `appsettings.json`.

4. **Настройте конфигурацию**

   Скопируйте примеры конфигурационных файлов (если необходимо):
   ```bash
   cp UserService/appsettings.Example.json UserService/appsettings.Development.json
   ```
   
   Заполните реальные значения в `appsettings.Development.json` (этот файл не попадет в Git).

5. **Запустите сервисы**

   В отдельных терминалах:
   ```bash
   # Terminal 1: Writer Service
   cd RateLimiter.Writer
   dotnet run

   # Terminal 2: Reader Service
   cd RateLimiter.Reader
   dotnet run

   # Terminal 3: User Service (опционально)
   cd UserService
   dotnet run

   # Terminal 4: Kafka Generator (для тестирования)
   cd UserRequestsKafkaGenerator
   dotnet run
   ```

## ⚙️ Конфигурация

### RateLimiter.Writer
```json
{
  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "RateLimiterDb",
    "RateLimitCollectionName": "rate_limits"
  }
}
```

### RateLimiter.Reader
```json
{
  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "RateLimiterDb",
    "RateLimitCollectionName": "rate_limits"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "TopicName": "user-events",
    "GroupId": "RateLimiterReader",
    "EnableAutoCommit": false,
    "AutoOffsetReset": "Earliest"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

### UserService
См. `UserService/appsettings.Example.json` для примера конфигурации.

## 📡 API

### RateLimiter.Writer gRPC API

- **CreateLimit** — создание нового лимита для маршрута
- **GetLimitByRoute** — получение лимита по маршруту
- **UpdateLimit** — обновление лимита
- **DeleteLimit** — удаление лимита

### RateLimiter.Reader gRPC API

- **GetAllLimits** — получение всех лимитов (снимок из памяти)

## 🔄 Как это работает

1. **Управление лимитами**: Администратор создает/обновляет лимиты через `RateLimiter.Writer`
2. **Синхронизация**: `RateLimiter.Reader` загружает лимиты из MongoDB и отслеживает изменения через Change Streams
3. **Обработка запросов**: События пользователей поступают в Kafka
4. **Проверка лимитов**: `RateLimiter.Reader` обрабатывает события:
   - Проверяет, не заблокирован ли пользователь
   - Инкрементирует счетчик запросов в Redis
   - Сравнивает с лимитом для маршрута
   - Блокирует пользователя на 5 минут при превышении лимита

## 🧪 Тестирование

Для тестирования системы используйте `UserRequestsKafkaGenerator`:

```bash
cd UserRequestsKafkaGenerator
dotnet run
```

Генератор создаст события пользователей и отправит их в Kafka для обработки.

## 📝 Формат событий Kafka

```json
{
  "UserId": 123,
  "Endpoint": "/api/users",
  "Timestamp": "2024-01-01T00:00:00Z"
}
```

