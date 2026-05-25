const token = localStorage.getItem('token');
let editingTaskId = null;

// Отримати userId з токена
function getUserIdFromToken() {
    if (!token) return null;
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload['sub'] || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
}
loadTasks();
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
        await loadTasks(); 
    } else {
        alert('Помилка при створенні завдання');
    }
});
// --- Завантаження та рендер завдань ---

async function loadTasks() {
    const userId = getUserIdFromToken();
    if (!userId) return;

    const response = await fetch(`/api/UserTasks?userId=${userId}`, {
        headers: { 'Authorization': `Bearer ${token}` }
    });

    if (!response.ok) {
        console.error('Помилка завантаження завдань');
        return;
    }

    const tasks = await response.json();
    renderTasks(tasks);
}

function renderTasks(tasks) {
    const list = document.querySelector('.tasks-list');
    list.innerHTML = ''; // очищаємо старі картки

    if (tasks.length === 0) {
        list.innerHTML = '<p class="no-tasks">Завдань поки немає</p>';
        return;
    }

    tasks.forEach(task => {
        const card = createTaskCard(task);
        list.appendChild(card);
    });
}

function createTaskCard(task) {
    const priorityMap = {
        'Низький':  { cls: 'low' },
        'Середній': { cls: 'medium' },
        'Високий':  { cls: 'high'},
    };

    const p = priorityMap[task.priority] ?? { cls: 'low' };

    const deadline = task.deadline
        ? new Date(task.deadline).toLocaleDateString('uk-UA')
        : '—';

    const article = document.createElement('article');
    article.className = 'task-card';
    article.dataset.id = task.id;

    article.innerHTML = `
        <div class="task-card-header">
            <h3 class="task-title">${escapeHtml(task.title)}</h3>
            <div class="task-actions">
                <button class="task-btn btn-edit">
                    <img src="/images/edit_icon.svg" alt="Edit">
                </button>
                <button class="task-btn delete btn-delete">
                    <img src="/images/delete_icon.svg" alt="Delete">
                </button>
            </div>
        </div>
        <p class="task-desc">${escapeHtml(task.description ?? '')}</p>
        <div class="task-footer">
            <span class="task-date">
                <img src="/images/calendar_icon.svg" alt="Calendar"> ${deadline}
            </span>
            <span class="task-priority ${p.cls}">${task.priority}</span>
        </div>
    `;

    // Видалення
    article.querySelector('.btn-delete').addEventListener('click', () => deleteTask(task.id));
    // Редагування
    article.querySelector('.btn-edit').addEventListener('click', () => openEditModal(task));
    return article;

   
}

async function deleteTask(id) {
    if (!confirm('Видалити завдання?')) return;

    const response = await fetch(`/api/UserTasks/${id}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
    });

    if (response.ok) {
        document.querySelector(`.task-card[data-id="${id}"]`)?.remove();
    } else {
        alert('Помилка при видаленні');
    }
}

// XSS захист
function escapeHtml(str) {
    return str.replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
}




function openEditModal(task) {
    editingTaskId = task.id;
    document.getElementById('edit-task-title').value = task.title;
    document.getElementById('edit-task-desc').value = task.description ?? '';
    document.getElementById('edit-task-priority').value = task.priority;

    // Дедлайн треба у форматі yyyy-MM-dd для input[type=date]
    if (task.deadline) {
        document.getElementById('edit-task-deadline').value =
            new Date(task.deadline).toISOString().split('T')[0];
    } else {
        document.getElementById('edit-task-deadline').value = '';
    }

    document.getElementById('edit-modal-overlay').classList.add('active');
}


document.getElementById('edit-modal-cancel').addEventListener('click', () => {
    editingTaskId = null;
    document.getElementById('edit-modal-overlay').classList.remove('active');
});

document.getElementById('edit-modal-overlay').addEventListener('click', (e) => {
    if (e.target === document.getElementById('edit-modal-overlay')) {
        document.getElementById('edit-modal-overlay').classList.remove('active');
    }
});
document.getElementById('edit-modal-save').addEventListener('click', async () => {
    if (!editingTaskId) {
        alert('Не вибране завдання для редагування');
        return;
    }

    const title = document.getElementById('edit-task-title').value;
    const description = document.getElementById('edit-task-desc').value;
    const deadline = document.getElementById('edit-task-deadline').value;
    const priority = document.getElementById('edit-task-priority').value;
    const response = await fetch(`/api/UserTasks/${editingTaskId}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
            title,
            description,
            deadline: deadline || new Date().toISOString(),
            priority
        })
    });

    if (response.ok) {
        editingTaskId = null;
        document.getElementById('edit-modal-overlay').classList.remove('active');
        await loadTasks();
    } else {
        alert('Помилка при збереженні змін');
    }
});

let lastSearchQuery = '';

document.querySelector('.search-input').addEventListener('input', async (e) => {
    const q = e.target.value.trim();
    const userId = getUserIdFromToken();
    if (!userId) return;

    if (q === '') {
        lastSearchQuery = '';
        await loadTasks();
        return;
    }

    lastSearchQuery = q;
    const response = await fetch(`/api/UserTasks/search?userId=${userId}&q=${encodeURIComponent(q)}`, {
        headers: { 'Authorization': `Bearer ${token}` }
    });

    if (response.ok) {
        const tasks = await response.json();
        if (q === lastSearchQuery) {
            renderTasks(tasks);
        }
    }
});