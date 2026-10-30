
## Вимоги

- .NET 9 SDK
- MySQL Server 9.x
- MySQL Workbench

## Запуск

### 1. Налаштування бази даних

Відкрий `src/ChefFlow.API/appsettings.json` та вкажи рядок підключення:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=chefflow;user=root;password=твій_пароль"
  }
}
```

Застосуй міграції:

```bash
cd src/ChefFlow.API
dotnet ef database update
```

### 2. Запуск моноліту

```bash
cd src/ChefFlow.API
dotnet run --launch-profile http
```

Моноліт доступний на `http://localhost:5208`
Swagger: `http://localhost:5208/swagger`

### 3. Запуск шлюзу

Відкрий новий термінал:

```bash
cd src/ApiGateway
dotnet run --launch-profile http
```

Шлюз доступний на `http://localhost:5000`

### 4. Запуск TaskService

Відкрий ще один термінал:

```bash
cd src/TaskService
dotnet run --launch-profile http
```

TaskService доступний на `http://localhost:5003`. Шлюз пересилає запити до
`/api/v1/tasks/**` на цей сервіс. TaskService потрібно запустити окремо від шлюзу.

### AuthService і RecipeService

Запусти AuthService, а потім RecipeService в окремих терміналах:

```bash
cd src/AuthService
dotnet run --launch-profile http
```

```bash
cd src/RecipeService
dotnet run --launch-profile http
```

У Development AuthService слухає HTTP на `localhost:5001` і внутрішній gRPC на
`localhost:5005`. RecipeService отримує ім'я автора через gRPC; HTTP-маршрут
`GET /api/v1/users/{userId}` для цього більше не використовується і видалений.
Gateway маршрутизує лише `GET /api/v1/users/me` та `GET /api/v1/users`.

Для локальної розробки обидва сервіси використовують ключ із
`appsettings.Development.json`. Це лише тестовий ключ: у розгортанні задай
однакове значення `Services__InternalApiKey` для обох сервісів, налаштуй
`Services__AuthGrpc` на HTTPS-адресу AuthService та опублікуй gRPC-порт лише у
приватній мережі. Для AuthService задай `Kestrel__Endpoints__Grpc__Url` з
HTTPS-адресою та `Kestrel__Endpoints__Grpc__Protocols=Http2`. Ключ має містити
щонайменше 32 байти. Не використовуй локальний ключ у Production.

## Порти

| Сервіс | Порт |
|--------|------|
| API Gateway (Ocelot) | 5000 |
| AuthService (HTTP) | 5001 |
| AuthService (gRPC, внутрішній) | 5005 |
| RecipeService | 5002 |
| ChefFlow Моноліт | 5208 |
| TaskService | 5003 |
| MySQL | 3306 |

## Маршрути шлюзу

| Upstream (клієнт) | Downstream (моноліт) | Опис |
|-------------------|---------------------|------|
| /api/auth/** | localhost:5208/api/auth/** | Явний маршрут — Auth |
| /api/recipe/** | localhost:5208/api/recipe/** | Явний маршрут — Recipe |
| /api/** | localhost:5208/api/** | Перехоплювач — решта |
| /health | шлюз | Health endpoint шлюзу |

## Приклади запитів через шлюз

```bash
# Health шлюзу
GET http://localhost:5000/health

# Список рецептів
GET http://localhost:5000/api/Recipe

# Авторизація
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}

# Публікація рецепту
PATCH http://localhost:5000/api/Recipe/1/publish
Authorization: Bearer <JWT токен>
```

## Демонстрація

Відкрий `docs/demo.html` у браузері для інтерактивної демонстрації запитів через шлюз.

## Архітектурні рішення (ADR)

- [ADR-0001 — Поділ моноліту на мікросервіси](docs/adr/0001-split-monolith.md)
- [ADR-0002 — Шлюз на Ocelot](docs/adr/0002-ocelot-as-gateway.md)

## Діаграми

- [Діаграма композиції](docs/diagrams/composition.png) — поточна та майбутня архітектура
- [Діаграма послідовностей (зараз)](docs/diagrams/sequence-current.png) — запит через шлюз на моноліт
- [Діаграма послідовностей (майбутнє)](docs/diagrams/sequence-future.png) — запит після винесення сервісів