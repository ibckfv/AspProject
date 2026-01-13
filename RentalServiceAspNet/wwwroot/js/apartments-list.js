let currentFilters = {};

document.addEventListener('DOMContentLoaded', async () => {
    await loadApartments();
    
    document.getElementById('filters-form').addEventListener('submit', async (e) => {
        e.preventDefault();
        await applyFilters();
    });
    
    document.getElementById('reset-filters').addEventListener('click', () => {
        document.getElementById('filters-form').reset();
        currentFilters = {};
        loadApartments();
    });
});

async function loadApartments() {
    const container = document.getElementById('apartments-container');
    container.innerHTML = '<p>Загрузка...</p>';
    
    try {
        const token = localStorage.getItem('token');
        const headers = {
            'Content-Type': 'application/json'
        };
        if (token) {
            headers['Authorization'] = 'Bearer ' + token;
        }
        
        const response = await fetch('/api/apartments', { headers });
        if (!response.ok) throw new Error('Ошибка загрузки');
        
        const apartments = await response.json();
        displayApartments(apartments);
    } catch (error) {
        container.innerHTML = '<p class="error">Ошибка загрузки объявлений</p>';
        console.error(error);
    }
}

async function applyFilters() {
    const form = document.getElementById('filters-form');
    const formData = new FormData(form);
    
    currentFilters = {};
    for (const [key, value] of formData.entries()) {
        if (value) {
            if (key === 'furnished' || key === 'parking' || key === 'petsAllowed' || key === 'internet') {
                currentFilters[key] = value === 'true';
            } else {
                currentFilters[key] = value;
            }
        }
    }
    
    await loadFilteredApartments();
}

async function loadFilteredApartments() {
    const container = document.getElementById('apartments-container');
    container.innerHTML = '<p>Загрузка...</p>';
    
    try {
        const token = localStorage.getItem('token');
        const headers = {
            'Content-Type': 'application/json'
        };
        if (token) {
            headers['Authorization'] = 'Bearer ' + token;
        }
        
        const response = await fetch('/api/apartments', { headers });
        if (!response.ok) throw new Error('Ошибка загрузки');
        
        let apartments = await response.json();
        
        if (currentFilters.cityId) {
            apartments = apartments.filter(a => a.cityId == currentFilters.cityId);
        }
        if (currentFilters.rooms) {
            const rooms = parseInt(currentFilters.rooms);
            if (rooms === 4) {
                apartments = apartments.filter(a => a.rooms >= 4);
            } else {
                apartments = apartments.filter(a => a.rooms == rooms);
            }
        }
        if (currentFilters.priceMin) {
            apartments = apartments.filter(a => a.pricePerDay >= parseInt(currentFilters.priceMin));
        }
        if (currentFilters.priceMax) {
            apartments = apartments.filter(a => a.pricePerDay <= parseInt(currentFilters.priceMax));
        }
        if (currentFilters.furnished === true) {
            apartments = apartments.filter(a => a.furnished === true);
        }
        if (currentFilters.parking === true) {
            apartments = apartments.filter(a => a.parking === true);
        }
        if (currentFilters.petsAllowed === true) {
            apartments = apartments.filter(a => a.petsAllowed === true);
        }
        if (currentFilters.internet === true) {
            apartments = apartments.filter(a => a.internet === true);
        }
        
        displayApartments(apartments);
    } catch (error) {
        container.innerHTML = '<p class="error">Ошибка загрузки объявлений</p>';
        console.error(error);
    }
}

function displayApartments(apartments) {
    const container = document.getElementById('apartments-container');
    const countEl = document.getElementById('results-count');
    
    countEl.textContent = `Найдено объявлений: ${apartments.length}`;
    
    if (apartments.length === 0) {
        container.innerHTML = '<p>Объявлений не найдено</p>';
        return;
    }
    
    container.innerHTML = apartments.map(apt => `
        <a href="/apartments/${apt.id}" class="apartment-card">
            ${apt.mainPhoto ? `<div class="card-image"><img src="${apt.mainPhoto}" alt="${apt.title}" /></div>` : ''}
            <div class="card-content">
                <h3>${apt.title || 'Без названия'}</h3>
                <p class="card-description">${apt.description ? apt.description.substring(0, 100) + '...' : ''}</p>
                <p class="card-price">${apt.pricePerDay} ₽/день</p>
                ${apt.cityName ? `<p class="card-location">${apt.cityName}</p>` : ''}
            </div>
        </a>
    `).join('');
}
