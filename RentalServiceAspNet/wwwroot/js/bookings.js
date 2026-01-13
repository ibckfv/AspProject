document.addEventListener('DOMContentLoaded', async () => {
    await loadMyBookings();
});

async function loadMyBookings() {
    const container = document.getElementById('bookings-container');
    container.innerHTML = '<p>Загрузка...</p>';
    
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = '/home/login';
        return;
    }
    
    try {
        const response = await fetch('/api/bookings/my', {
            headers: {
                'Authorization': 'Bearer ' + token
            }
        });
        
        if (response.status === 401) {
            localStorage.removeItem('token');
            window.location.href = '/home/login';
            return;
        }
        
        if (!response.ok) throw new Error('Ошибка загрузки');
        
        const bookings = await response.json();
        displayBookings(bookings);
    } catch (error) {
        container.innerHTML = '<p class="error">Ошибка загрузки бронирований</p>';
        console.error(error);
    }
}

function displayBookings(bookings) {
    const container = document.getElementById('bookings-container');
    
    if (bookings.length === 0) {
        container.innerHTML = '<p>У вас нет бронирований</p>';
        return;
    }
    
    container.innerHTML = bookings.map(booking => {
        const startDate = new Date(booking.startDate).toLocaleDateString('ru-RU');
        const endDate = new Date(booking.endDate).toLocaleDateString('ru-RU');
        const createdDate = new Date(booking.createdAt).toLocaleDateString('ru-RU');
        
        return `
            <div class="booking-card">
                <div class="booking-info">
                    <h3>${booking.apartmentTitle || 'Объявление #' + booking.apartmentId}</h3>
                    ${booking.cityName ? `<p class="booking-location">${booking.cityName}</p>` : ''}
                    <div class="booking-dates">
                        <p><strong>Заезд:</strong> ${startDate}</p>
                        <p><strong>Выезд:</strong> ${endDate}</p>
                        <p class="booking-created">Создано: ${createdDate}</p>
                    </div>
                </div>
                <div class="booking-actions">
                    <a href="/apartments/${booking.apartmentId}" class="btn btn-outline">Просмотр</a>
                    <button onclick="cancelBooking(${booking.id})" class="btn btn-danger">Отменить</button>
                </div>
            </div>
        `;
    }).join('');
}

async function cancelBooking(bookingId) {
    if (!confirm('Вы уверены, что хотите отменить бронирование?')) {
        return;
    }
    
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = '/home/login';
        return;
    }
    
    try {
        const response = await fetch(`/api/bookings/${bookingId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': 'Bearer ' + token
            }
        });
        
        const result = await response.json();
        
        if (response.ok && result.ok) {
            alert('Бронирование отменено');
            await loadMyBookings();
        } else {
            alert(result.error || 'Ошибка отмены бронирования');
        }
    } catch (error) {
        alert('Ошибка соединения');
        console.error(error);
    }
}
