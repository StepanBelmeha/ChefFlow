| Сценарій | Метод | Маршрут | Статус моноліту |
|----------|-------|---------|-----------------|
| Всі завдання | GET | /api/UserTasks?userId=1 | 200 |
| Одне завдання | GET | /api/UserTasks/{id} | 200 |
| Створення | POST | /api/UserTasks | 201 |
| Створення без токена | POST | /api/UserTasks | 201 (немає авторизації) |
| Оновлення | PUT | /api/UserTasks/{id} | 200 |
| Видалення | DELETE | /api/UserTasks/{id} | 204 |
| Пошук | GET | /api/UserTasks/search | 200 |
| Неіснуюче завдання | GET | /api/UserTasks/99999 | 404 |