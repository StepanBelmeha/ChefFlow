const token = localStorage.getItem('token');

// Отримати userId з токена
function getUserIdFromToken() {
    if (!token) return null;
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload['sub'] || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
}

// Відкрити модальне вікно
document.getElementById('add-task-btn').addEventListener('click', () => {
    document.getElementById('modal-overlay').classList.add('active');
});

// Закрити модальне вікно
document.getElementById('modal-cancel').addEventListener('click', () => {
    document.getElementById('modal-overlay').classList.remove('active');
});

document.getElementById('modal-overlay').addEventListener('click', (e) => {
    if (e.target === document.getElementById('modal-overlay')) {
        document.getElementById('modal-overlay').classList.remove('active');
    }
});

// Створити завдання
document.querySelector('.modal .btn-primary').addEventListener('click', async () => {
    const userId = getUserIdFromToken();
    const title = document.getElementById('task-title').value;
    const description = document.getElementById('task-desc').value;
    const deadline = document.getElementById('task-deadline').value;
    const priority = document.getElementById('task-priority').value;

    if (!title) {
        alert('Введіть назву завдання');
        return;
    }

    const response = await fetch('/api/UserTasks', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
            userId: parseInt(userId),
            title,
            description,
            deadline: deadline || new Date().toISOString(),
            priority
        })
    });

    if (response.ok) {
        document.getElementById('modal-overlay').classList.remove('active');
        alert('Завдання створено!');
    } else {
        alert('Помилка при створенні завдання');
    }
});