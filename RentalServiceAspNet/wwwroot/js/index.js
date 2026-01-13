function getAuthHeaders() {
    const token = localStorage.getItem('token');
    const headers = {
        'Content-Type': 'application/json'
    };
    
    if (token) {
        headers['Authorization'] = 'Bearer ' + token;
    }
    
    return headers;
}

async function fetchWithAuth(url, options = {}) {
    const defaultHeaders = getAuthHeaders();
    const mergedHeaders = { ...defaultHeaders, ...(options.headers || {}) };
    
    const response = await fetch(url, {
        ...options,
        headers: mergedHeaders
    });
    
    if (response.status === 401) {
        localStorage.removeItem('token');
        if (window.location.pathname !== '/home/login') {
            window.location.href = '/home/login';
        }
        return null;
    }
    
    return response;
}

window.logout = function() {
    localStorage.removeItem('token');
    document.cookie = 'token=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
    updateNavigation();
    window.location.href = '/';
};

window.updateNavigation = function() {
    const token = localStorage.getItem('token');
    const authLinks = document.getElementById('auth-links');
    const guestLinks = document.getElementById('guest-links');
    
    if (token) {
        if (authLinks) authLinks.style.display = 'flex';
        if (guestLinks) guestLinks.style.display = 'none';
    } else {
        if (authLinks) authLinks.style.display = 'none';
        if (guestLinks) guestLinks.style.display = 'flex';
    }
};

document.addEventListener('DOMContentLoaded', function() {
    updateNavigation();
    
    const originalFetch = window.fetch;
    window.fetch = function(url, options = {}) {
        const token = localStorage.getItem('token');
        if (token && (!options.headers || !options.headers['Authorization'])) {
            options = options || {};
            options.headers = options.headers || {};
            if (typeof options.headers === 'object' && !Array.isArray(options.headers)) {
                options.headers['Authorization'] = 'Bearer ' + token;
            }
        }
        return originalFetch(url, options);
    };
});
