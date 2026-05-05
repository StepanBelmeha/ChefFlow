// Зберігай поточну сторінку перед перевіркою
const currentPage = window.location.href;
const token = localStorage.getItem('token');

if (!token) {
    localStorage.setItem('lastPage', currentPage);
    window.location.href = '/Auth/Index';
}