# Порівняння базової поведінки до і після винесення TaskService

| Сценарій | Маршрут моноліту | Маршрут сервісу | Статус до | Статус після | Різниця | Пояснення |
|----------|-----------------|-----------------|-----------|--------------|---------|-----------|
| Всі завдання | GET /api/UserTasks?userId=1 | GET /api/v1/tasks | 200 | 200 | userId тепер з токена, не з query | Свідома зміна — безпечніше |
| Одне завдання | GET /api/UserTasks/{id} | GET /api/v1/tasks/{id} | 200 | 200 | Немає |  |
| Створення | POST /api/UserTasks | POST /api/v1/tasks | 201 | 201 | userId тепер з токена | Свідома зміна |
| Створення без токена | POST /api/UserTasks | POST /api/v1/tasks | 201 | 401 | Свідома зміна — додано авторизацію | Моноліт не мав [Authorize], сервіс має |
| Оновлення | PUT /api/UserTasks/{id} | PUT /api/v1/tasks/{id} | 200 | 200 | Немає |  |
| Видалення | DELETE /api/UserTasks/{id} | DELETE /api/v1/tasks/{id} | 204 | 204 | Немає |  |
| Пошук | GET /api/UserTasks/search?userId=1&q= | GET /api/v1/tasks/search?q= | 200 | 200 | userId тепер з токена | Свідома зміна |
| Неіснуюче завдання | GET /api/UserTasks/99999 | GET /api/v1/tasks/99999 | 404 | 404 | Немає |  |