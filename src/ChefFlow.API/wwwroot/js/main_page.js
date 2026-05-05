// Перевірка авторизації на головній сторінці
const token = localStorage.getItem('token');
const userName = localStorage.getItem('userName');

if (token && userName) {
    // Замінити кнопки навігації
    const navActions = document.querySelector('.nav-actions');
    navActions.innerHTML = `
        <a href="/Cabinet/Index" class="btn-ghost">👤 ${userName}</a>
        <a href="#" class="btn-primary" id="logout-btn">Вийти</a>
    `;

    document.getElementById('logout-btn').addEventListener('click', (e) => {
        e.preventDefault();
        localStorage.removeItem('token');
        localStorage.removeItem('userName');
        localStorage.removeItem('userEmail');
        window.location.reload();
    });

    // Замінити кнопки в CTA секції
    const ctaActions = document.querySelector('.cta-actions');
    if (ctaActions) {
        ctaActions.innerHTML = `
            <a href="/Cabinet/Index" class="btn-primary">Мій кабінет</a>
        `;
    }
}

// Зберігати останню сторінку
localStorage.setItem('lastPage', window.location.href);