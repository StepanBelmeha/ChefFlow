document.querySelectorAll('#editor-toolbar [data-cmd]').forEach(btn => {
    btn.addEventListener('click', () => {
        document.execCommand(btn.dataset.cmd, false, null);
        document.getElementById('recipe-steps').focus();
    });
});
 
document.getElementById('undo-btn').addEventListener('click', () => {
    document.execCommand('undo');
});
document.getElementById('redo-btn').addEventListener('click', () => {
    document.execCommand('redo');
});


const mediaUpload = document.getElementById('media-upload');
const mediaInput = document.getElementById('media-input');
const mediaPreview = document.getElementById('media-preview');
const mediaPlaceholder = document.getElementById('media-placeholder');
 
mediaUpload.addEventListener('click', () => mediaInput.click());
 
mediaUpload.addEventListener('dragover', (e) => {
    e.preventDefault();
    mediaUpload.style.borderColor = 'var(--accent)';
});
 
mediaUpload.addEventListener('dragleave', () => {
    mediaUpload.style.borderColor = '';
});
 
mediaUpload.addEventListener('drop', (e) => {
    e.preventDefault();
    mediaUpload.style.borderColor = '';
    const file = e.dataTransfer.files[0];
    if (file) handleMediaFile(file);
});
 
mediaInput.addEventListener('change', () => {
    if (mediaInput.files[0]) handleMediaFile(mediaInput.files[0]);
});

// ── ІНГРЕДІЄНТИ ──
const ingredients = []; // масив інгредієнтів

// Відкрити модальне вікно інгредієнта
document.getElementById('add-ingredient-btn').addEventListener('click', () => {
    document.getElementById('ingredient-modal').classList.add('active');
});

// Закрити модальне вікно
document.getElementById('ing-cancel').addEventListener('click', () => {
    document.getElementById('ingredient-modal').classList.remove('active');
    clearIngredientForm();
});

document.getElementById('ingredient-modal').addEventListener('click', (e) => {
    if (e.target === document.getElementById('ingredient-modal')) {
        document.getElementById('ingredient-modal').classList.remove('active');
        clearIngredientForm();
    }
});

// Зберегти інгредієнт
document.getElementById('ing-save').addEventListener('click', () => {
    const name = document.getElementById('ing-name').value.trim();
    const qty = parseFloat(document.getElementById('ing-qty').value);
    const unit = document.getElementById('ing-unit').value;

    if (!name) {
        alert('Введіть назву інгредієнта');
        return;
    }
    if (!qty || qty <= 0) {
        alert('Введіть кількість');
        return;
    }

    const ingredient = { productName: name, quantity: qty, unit };
    ingredients.push(ingredient);
    renderIngredients();

    document.getElementById('ingredient-modal').classList.remove('active');
    clearIngredientForm();
});

function clearIngredientForm() {
    document.getElementById('ing-name').value = '';
    document.getElementById('ing-qty').value = '';
    document.getElementById('ing-unit').value = 'шт.';
}

function renderIngredients() {
    const list = document.getElementById('ingredients-list');
    list.innerHTML = ingredients.map((ing, index) => `
        <li class="ingredient-item">
            <span class="ing-name">${ing.productName}</span>
            <span class="ing-qty">${ing.quantity} ${ing.unit}</span>
            <button type="button" class="ing-delete" onclick="removeIngredient(${index})">✕</button>
        </li>
    `).join('');
}

function removeIngredient(index) {
    ingredients.splice(index, 1);
    renderIngredients();
}

let uploadedMediaPath = '';

async function handleMediaFile(file) {
    if (!file.type.startsWith('image/') && !file.type.startsWith('video/')) {
        alert('Оберіть зображення або відео');
        return;
    }

    // Показати прев'ю
    const url = URL.createObjectURL(file);
    mediaPreview.src = url;
    mediaPreview.style.display = 'block';
    mediaPlaceholder.style.display = 'none';

    // Завантажити файл на сервер
    const formData = new FormData();
    formData.append('file', file);

    const response = await fetch('/api/Recipe/upload', {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${token}` },
        body: formData
    });

    if (response.ok) {
        const data = await response.json();
        uploadedMediaPath = data.path;
    } else {
        alert('Помилка завантаження файлу');
    }
}


const token = localStorage.getItem('token');

function getUserIdFromToken() {
    if (!token) return null;
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload['sub'] 
        || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
}
document.getElementById('publish-btn').addEventListener('click', async () => {
    const userId = getUserIdFromToken();
    const title = document.getElementById('recipe-name').value;
    const description = document.getElementById('recipe-short-desc').value;
    const instructions = document.getElementById('recipe-steps').innerHTML;
    const mediaUrl = mediaPreview.src || '';

    if (!title) {
        alert('Введіть назву рецепту');
        return;
    }

    const response = await fetch('/api/Recipe', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
            userId: parseInt(userId),
            title,
            description,
            instructions,
            isPublished: true,
            createdAt: new Date().toISOString(),
            media: uploadedMediaPath,
            ingredients
        })
    });

    if (response.ok) {
        alert('Рецепт опубліковано!');
        window.location.href = '/AddRecipe/Index';
    } else {
        alert('Помилка при публікації рецепту');
    }
});