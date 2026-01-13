document.addEventListener('DOMContentLoaded', async () => {
    const apartmentId = document.getElementById('apartment-id')?.value;
    
    if (apartmentId) {
        await loadBookedDates(apartmentId);
        setupBookingForm(apartmentId);
    }
});

async function loadBookedDates(apartmentId) {
    const container = document.getElementById('booked-dates-list');
    
    try {
        const response = await fetch(`/api/bookings/apartment/${apartmentId}`);
        if (!response.ok) throw new Error('Ошибка загрузки');
        
        const bookings = await response.json();
        
        if (bookings.length === 0) {
            container.innerHTML = '<p>Нет забронированных дат</p>';
            return;
        }
        
        container.innerHTML = bookings.map(booking => {
            const start = new Date(booking.startDate).toLocaleDateString('ru-RU');
            const end = new Date(booking.endDate).toLocaleDateString('ru-RU');
            return `<div class="booked-date">${start} - ${end}</div>`;
        }).join('');
    } catch (error) {
        container.innerHTML = '<p class="error">Ошибка загрузки дат</p>';
        console.error(error);
    }
}

function setupBookingForm(apartmentId) {
    const form = document.getElementById('booking-form');
    const startDateInput = document.getElementById('start-date');
    const endDateInput = document.getElementById('end-date');
    const summary = document.getElementById('booking-summary');
    const errorDiv = document.getElementById('booking-error');
    
    const today = new Date().toISOString().split('T')[0];
    startDateInput.min = today;
    endDateInput.min = today;
    
    startDateInput.addEventListener('change', () => {
        if (startDateInput.value) {
            endDateInput.min = startDateInput.value;
            calculatePrice();
        }
    });
    
    endDateInput.addEventListener('change', calculatePrice);
    
    async function calculatePrice() {
        const startDate = startDateInput.value;
        const endDate = endDateInput.value;
        
        if (!startDate || !endDate) {
            summary.style.display = 'none';
            return;
        }
        
        try {
            const response = await fetch(`/api/apartments/${apartmentId}`);
            if (!response.ok) throw new Error('Ошибка загрузки');
            
            const apartment = await response.json();
            const start = new Date(startDate);
            const end = new Date(endDate);
            const days = Math.ceil((end - start) / (1000 * 60 * 60 * 24));
            const total = days * apartment.pricePerDay;
            
            document.getElementById('booking-days').textContent = days;
            document.getElementById('booking-total').textContent = total + ' ₽';
            summary.style.display = 'block';
        } catch (error) {
            console.error(error);
        }
    }
    
    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        errorDiv.style.display = 'none';
        
        const token = localStorage.getItem('token');
        if (!token) {
            window.location.href = '/home/login';
            return;
        }
        
        const startDate = startDateInput.value;
        const endDate = endDateInput.value;
        
        if (!startDate || !endDate) {
            showError('Заполните все поля');
            return;
        }
        
        try {
            const response = await fetch('/api/bookings', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + token
                },
                body: JSON.stringify({
                    apartmentId: parseInt(apartmentId),
                    startDate: startDate,
                    endDate: endDate
                })
            });
            
            const data = await response.json();
            
            if (response.ok && data.ok) {
                alert('Бронирование успешно создано!');
                form.reset();
                summary.style.display = 'none';
                await loadBookedDates(apartmentId);
            } else {
                showError(data.error || 'Ошибка создания бронирования');
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
}
