// Перевірка чи є токен
const token = localStorage.getItem('token');
if (!token) {
    window.location.href = '/Auth/Index';
}

// Відображення даних користувача
const userName = localStorage.getItem('userName');
const userEmail = localStorage.getItem('userEmail');

if (userName) {
    document.getElementById('user-name').textContent = userName;
}
if (userEmail) {
    document.getElementById('user-email').textContent = userEmail;
}

// Вихід
document.querySelector('.btn-ghost').addEventListener('click', (e) => {
    e.preventDefault();
    localStorage.removeItem('token');
    localStorage.removeItem('userName');
    localStorage.removeItem('userEmail');
    window.location.href = '/Auth/Index';
});