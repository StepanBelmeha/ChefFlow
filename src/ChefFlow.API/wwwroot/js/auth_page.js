function switchPanel(panel) {
  document.querySelectorAll('.form-panel').forEach(p => p.classList.remove('active'));
  document.getElementById('panel-' + panel).classList.add('active');
}

function togglePass(id, btn) {
  const inp = document.getElementById(id);
  if (inp.type === 'password') {
    inp.type = 'text';
    btn.textContent = '🙈';
  } else {
    inp.type = 'password';
    btn.textContent = '👁';
  }
}
// Валідація
function validateLogin(email, password) {
    if (!email.includes('@')) {
        alert('Email має містити символ @');
        return false;
    }
    if (password.length < 6) {
        alert('Пароль має бути не менше 6 символів');
        return false;
    }
    return true;
}

function validateRegister(name, email, password) {
    if (!/^[a-zA-Zа-яА-ЯіІїЇєЄ']+$/.test(name)) {
        alert('Ім\'я має містити лише букви');
        return false;
    }
    if (!email.includes('@')) {
        alert('Email має містити символ @');
        return false;
    }
    if (password.length < 6) {
        alert('Пароль має бути не менше 6 символів');
        return false;
    }
    return true;
}


document.querySelector('#panel-login .form-submit').addEventListener('click', async () => {
    const email = document.querySelector('#panel-login input[type="email"]').value;
    const password = document.getElementById('login-pass').value;

    if (!validateLogin(email, password)) return;

    const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
    });

    if (response.ok) {
        const data = await response.json();
        localStorage.setItem('token', data.token);
        window.location.href = '/Cabinet/Index';
    } else {
        alert('Невірний email або пароль');
    }
});


document.querySelector('#panel-register .form-submit').addEventListener('click', async () => {
    const name = document.querySelector('#panel-register input[type="text"]').value;
    const email = document.querySelector('#panel-register input[type="email"]').value;
    const password = document.getElementById('reg-pass').value;

    if (!validateRegister(name, email, password)) return;

    const response = await fetch('/api/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, email, password })
    });

    if (response.ok) {
      window.location.href = '/Cabinet/Index';
    } else {
        const error = await response.text();
        alert(error);
    }
});