const token = localStorage.getItem('token');
const userName = localStorage.getItem('userName');
let currentRecipeId = null;
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
                <div class="recipes-empty-icon">❤️</div>
                <div class="recipes-empty-text">Обраних рецептів ще немає.</div>
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
                <p class="recipe-card-author">${recipe.authorName || 'Автор'}</p>
            </div>
        </div>
    `).join('');
}

async function loadFavorites(search = '') {
    const userId = getUserIdFromToken();
    const response = await fetch(`/api/Favorite/${userId}`, {
        headers: { 'Authorization': `Bearer ${token}` }
    });

    if (!response.ok) return;

    let recipes = await response.json();

    if (search) {
        recipes = recipes.filter(r =>
            r.title.toLowerCase().includes(search.toLowerCase())
        );
    }

    renderRecipes(recipes);
}

async function openRecipe(id) {
    const response = await fetch(`/api/Recipe/${id}`, {
        headers: { 'Authorization': `Bearer ${token}` }
    });

    if (!response.ok) return;

    const recipe = await response.json();
    currentRecipeId = id;
    await loadNotes(id);

    document.getElementById('modal-title').textContent = recipe.title;
    document.getElementById('modal-desc').textContent = recipe.description;
    document.getElementById('modal-instructions').innerHTML = recipe.instructions;

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
        currentIngredients = ingredients;

        document.getElementById('modal-ingredients').innerHTML = ingredients
            .map(ing => `
                <li>
                    <span>${ing.productName}</span>
                    <span>${ing.quantity} ${ing.unit}</span>
                </li>
            `).join('');

        const select = document.getElementById('scale-ingredient');
        select.innerHTML = '<option value="">Оберіть інгредієнт</option>' +
            ingredients.map((ing, index) => `
                <option value="${index}">${ing.productName} (${ing.quantity} ${ing.unit})</option>
            `).join('');

        document.getElementById('scaled-ingredients').style.display = 'none';
    }

    document.getElementById('recipe-modal').classList.add('active');
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
async function loadNotes(recipeId) {
    const response = await fetch(`/api/Note/recipe/${recipeId}`, {
        headers: { 'Authorization': `Bearer ${token}` }
    });

    if (!response.ok) return;

    const notes = await response.json();
    document.getElementById('notes-list').innerHTML = notes.map(note => `
        <div class="note-card">
            <p class="note-content">${note.content}</p>
            <p class="note-date">${new Date(note.createdAt).toLocaleDateString('uk-UA')}</p>
        </div>
    `).join('');
}

async function addNote() {
    const content = document.getElementById('note-content').value.trim();
    if (!content) {
        alert('Будь ласка, введіть текст нотатки');
        return;
    }

    if (!currentRecipeId) {
        alert('Помилка: рецепт не вибран');
        return;
    }

    try {
        const response = await fetch('/api/Note', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({
                recipeId: currentRecipeId,
                content: content
            })
        });

        if (response.ok) {
            document.getElementById('note-content').value = '';
            await loadNotes(currentRecipeId);
        } else {
            const errorData = await response.text();
            console.error('Помилка при додаванні нотатки:', response.status, errorData);
            alert('Помилка при додаванні нотатки: ' + response.status);
        }
    } catch (error) {
        console.error('Помилка мережі:', error);
        alert('Помилка: ' + error.message);
    }
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
    loadFavorites(e.target.value.trim());
});

loadFavorites();