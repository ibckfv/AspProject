document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('apartment-form');
    const errorDiv = document.getElementById('form-error');
    const successDiv = document.getElementById('form-success');
    const authWarning = document.getElementById('auth-warning');
    
    const token = localStorage.getItem('token');
    if (!token && authWarning) {
        authWarning.style.display = 'block';
    }
    
    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        errorDiv.style.display = 'none';
        successDiv.style.display = 'none';
        
        const token = localStorage.getItem('token');
        if (!token) {
            if (confirm('Для добавления объявления необходимо войти в систему. Перейти на страницу входа?')) {
                window.location.href = '/home/login?returnUrl=/apartments/add';
            }
            return;
        }
        
        const title = document.getElementById('title').value.trim();
        const description = document.getElementById('description').value.trim();
        const pricePerDayStr = document.getElementById('price-per-day').value;
        const cityIdStr = document.getElementById('city-id').value;
        const roomsStr = document.getElementById('rooms').value;
        const totalAreaStr = document.getElementById('total-area').value;
        const floorStr = document.getElementById('floor').value;
        const viewType = document.getElementById('view-type').value.trim();
        const renovation = document.getElementById('renovation').value.trim();
        const lighting = document.getElementById('lighting').value.trim();
        
        if (!title || !pricePerDayStr) {
            showError('Заполните обязательные поля: название и цена');
            return;
        }
        
        const pricePerDay = parseInt(pricePerDayStr);
        if (isNaN(pricePerDay) || pricePerDay <= 0) {
            showError('Цена должна быть положительным числом');
            return;
        }
        
        const furnished = document.querySelector('input[name="furnished"]:checked') !== null;
        const internet = document.querySelector('input[name="internet"]:checked') !== null;
        const parking = document.querySelector('input[name="parking"]:checked') !== null;
        const petsAllowed = document.querySelector('input[name="petsAllowed"]:checked') !== null;
        
        const data = {
            title: title,
            description: description || null,
            pricePerDay: pricePerDay,
            cityId: cityIdStr ? parseInt(cityIdStr) : null,
            rooms: roomsStr ? parseInt(roomsStr) : null,
            totalArea: totalAreaStr ? parseInt(totalAreaStr) : null,
            floor: floorStr ? parseInt(floorStr) : null,
            viewType: viewType || null,
            renovation: renovation || null,
            lighting: lighting || null,
            furnished: furnished,
            internet: internet,
            parking: parking,
            petsAllowed: petsAllowed
        };
        
        try {
            const response = await fetch('/api/apartments', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + token
                },
                body: JSON.stringify(data)
            });
            
            const result = await response.json();
            
            if (response.ok && result.ok) {
                successDiv.textContent = 'Объявление успешно создано! Ожидает модерации.';
                successDiv.style.display = 'block';
                form.reset();
                setTimeout(() => {
                    window.location.href = '/apartments/my';
                }, 2000);
            } else {
                showError(result.error || 'Ошибка создания объявления');
            }
        } catch (error) {
            showError('Ошибка соединения');
            console.error(error);
        }
    });
    
    function showError(message) {
        errorDiv.textContent = message;
        errorDiv.style.display = 'block';
    }
});
