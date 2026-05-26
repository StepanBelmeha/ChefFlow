const token = localStorage.getItem('token');
const userName = localStorage.getItem('userName');

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
                <p class="recipe-card-author">${userName || 'Автор'}</p>
            </div>
        </div>
    `).join('');
}

async function loadRecipes(search = '') {
    const userId = getUserIdFromToken();

    const response = await fetch(`/api/Favorites/${userId}`, {
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

    // Завантажити інгредієнти
    const ingResponse = await fetch(`/api/Recipe/${id}/ingredients`, {
        headers: { 'Authorization': `Bearer ${token}` }
    });

    if (ingResponse.ok) {
        const ingredients = await ingResponse.json();
        document.getElementById('modal-ingredients').innerHTML = ingredients
            .map(ing => `
                <li>
                    <span>${ing.productName}</span>
                    <span>${ing.quantity} ${ing.unit}</span>
                </li>
            `).join('');
    }

    document.getElementById('recipe-modal').classList.add('active');
}
document.getElementById('modal-close').addEventListener('click', () => {
    document.getElementById('recipe-modal').classList.remove('active');
});

document.getElementById('recipe-modal').addEventListener('click', (e) => {
    if (e.target === document.getElementById('recipe-modal')) {
        document.getElementById('recipe-modal').classList.remove('active');
    }
});

// Пошук
document.getElementById('search-input').addEventListener('input', (e) => {
    loadRecipes(e.target.value.trim());
});

loadRecipes();