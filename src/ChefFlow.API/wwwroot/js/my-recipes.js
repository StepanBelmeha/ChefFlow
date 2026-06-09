const token = localStorage.getItem('token');
const userName = localStorage.getItem('userName');
let currentRecipeId = null;
let currentRecipePublished = false;
let currentIngredients = [];
if (userName) {
    document.getElementById('user-btn').textContent = `👤 ${userName}`;
}

function getUserIdFromToken() {
    if (!token) return null;
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload['sub']
        || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
}

function renderRecipes(recipes) {
    const grid = document.getElementById('recipes-grid');

    if (recipes.length === 0) {
        grid.innerHTML = `
            <div class="recipes-empty">
                <div class="recipes-empty-icon">📖</div>
                <div class="recipes-empty-text">Рецептів ще немає. Створи перший!</div>
            </div>`;
        return;
    }

    grid.innerHTML = recipes.map(recipe => `
        <div class="recipe-card" onclick="openRecipe(${recipe.id})">
            <div class="recipe-card-media">
                ${recipe.media
                    ? `<img src="${recipe.media}" alt="${recipe.title}"/>`
                    : `<span class="recipe-card-media-placeholder">🍽️</span>`
                }
            </div>
            <div class="recipe-card-body">
                <h3 class="recipe-card-title">${recipe.title}</h3>
                <div class="recipe-card-footer">
                    <p class="recipe-card-author">${recipe.authorName || userName || 'Автор'}</p>
                    ${recipe.isPublished
                        ? `<span class="status-published">✓ Опубліковано</span>`
                        : `<span class="status-draft">Чернетка</span>`
                    }
                </div>
            </div>
        </div>
    `).join('');
}

async function loadRecipes(search = '') {
    const userId = getUserIdFromToken();
    const url = search
        ? `/api/Recipe/user/${userId}?search=${encodeURIComponent(search)}`
        : `/api/Recipe/user/${userId}`;

    const response = await fetch(url, {
        headers: { 'Authorization': `Bearer ${token}` }
    });

    if (!response.ok) return;
    const recipes = await response.json();
    renderRecipes(recipes);
}

async function openRecipe(id) {
    const response = await fetch(`/api/Recipe/${id}`, {
        headers: { 'Authorization': `Bearer ${token}` }
    });

    if (!response.ok) return;

    const recipe = await response.json();
    currentRecipeId = id;
    currentRecipePublished = recipe.isPublished;

    document.getElementById('modal-title').textContent = recipe.title;
    document.getElementById('modal-desc').textContent = recipe.description;
    document.getElementById('modal-instructions').innerHTML = recipe.instructions;

    // Статус публікації
    const statusEl = document.getElementById('modal-status');
    const publishBtn = document.getElementById('publish-recipe-btn');

    if (recipe.isPublished) {
        statusEl.textContent = '✓ Опубліковано';
        statusEl.className = 'recipe-modal-status status-published';
        publishBtn.style.display = 'none';
    } else {
        statusEl.textContent = 'Чернетка';
        statusEl.className = 'recipe-modal-status status-draft';
        publishBtn.style.display = 'block';
    }

    const mediaImg = document.getElementById('modal-media');
    const mediaPlaceholder = document.getElementById('modal-media-placeholder');

    if (recipe.media) {
        mediaImg.src = recipe.media;
        mediaImg.style.display = 'block';
        mediaPlaceholder.style.display = 'none';
    } else {
        mediaImg.style.display = 'none';
        mediaPlaceholder.style.display = 'block';
    }

    const ingResponse = await fetch(`/api/Recipe/${id}/ingredients`, {
        headers: { 'Authorization': `Bearer ${token}` }
    });
    if (ingResponse.ok) {
    const ingredients = await ingResponse.json();
    currentIngredients = ingredients; // зберігаємо для масштабування

    document.getElementById('modal-ingredients').innerHTML = ingredients
        .map(ing => `
            <li>
                <span>${ing.productName}</span>
                <span>${ing.quantity} ${ing.unit}</span>
            </li>
        `).join('');

    // Заповнити select інгредієнтами
    const select = document.getElementById('scale-ingredient');
    select.innerHTML = '<option value="">Оберіть інгредієнт</option>' +
        ingredients.map((ing, index) => `
            <option value="${index}">${ing.productName} (${ing.quantity} ${ing.unit})</option>
        `).join('');

    // Сховати попередні результати
    document.getElementById('scaled-ingredients').style.display = 'none';
    }


    document.getElementById('recipe-modal').classList.add('active');
}

async function publishRecipe() {
    if (!currentRecipeId) return;

    const response = await fetch(`/api/Recipe/${currentRecipeId}/publish`, {
        method: 'PATCH',
        headers: { 'Authorization': `Bearer ${token}` }
    });

    if (response.ok) {
        document.getElementById('modal-status').textContent = '✓ Опубліковано';
        document.getElementById('modal-status').className = 'recipe-modal-status status-published';
        document.getElementById('publish-recipe-btn').style.display = 'none';
        loadRecipes();
    } else {
        alert('Помилка при публікації');
    }
}

function scaleRecipe() {
    const selectEl = document.getElementById('scale-ingredient');
    const newValue = parseFloat(document.getElementById('scale-value').value);
    const selectedIndex = parseInt(selectEl.value);

    if (isNaN(selectedIndex) || selectEl.value === '') {
        alert('Оберіть інгредієнт');
        return;
    }
    if (!newValue || newValue <= 0) {
        alert('Введіть коректну кількість');
        return;
    }

    const baseIngredient = currentIngredients[selectedIndex];
    const ratio = newValue / baseIngredient.quantity;

    const scaled = currentIngredients.map(ing => ({
        productName: ing.productName,
        quantity: Math.round(ing.quantity * ratio * 100) / 100,
        unit: ing.unit
    }));

    const scaledList = document.getElementById('scaled-ingredients');
    scaledList.style.display = 'flex';
    scaledList.innerHTML = scaled.map(ing => `
        <li>
            <span>${ing.productName}</span>
            <span>${ing.quantity} ${ing.unit}</span>
        </li>
    `).join('');
}

document.getElementById('modal-close').addEventListener('click', () => {
    document.getElementById('recipe-modal').classList.remove('active');
});

document.getElementById('recipe-modal').addEventListener('click', (e) => {
    if (e.target === document.getElementById('recipe-modal')) {
        document.getElementById('recipe-modal').classList.remove('active');
    }
});

document.getElementById('search-input').addEventListener('input', (e) => {
    loadRecipes(e.target.value.trim());
});

loadRecipes();