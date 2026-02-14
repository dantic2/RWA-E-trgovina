const API_BASE_URL = '/api';

const loginForm = document.getElementById('login-form');
const errorMessage = document.getElementById('error-message');
const usernameInput = document.getElementById('username');
const passwordInput = document.getElementById('password');

if (localStorage.getItem('jwt_token')) {
    window.location.href = '/logs.html';
}

loginForm.addEventListener('submit', handleLogin);

async function handleLogin(e) {
    e.preventDefault(); // Zaustavi default submit (page reload)

    hideMessage(errorMessage);

    const username = usernameInput.value.trim();
    const password = passwordInput.value;

    if (!username || !password) {
        showMessage(errorMessage, 'Please enter username and password');
        return;
    }

    try {
        // AJAX POST request na /api/Auth/Login
        const response = await fetch(`${API_BASE_URL}/Auth/Login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                username: username,
                password: password
            })
        });

        console.log(response);

        const data = await response.json();

        console.log(data);

        if (!response.ok) {
            throw new Error(data || 'Login failed');
        }

        localStorage.setItem('jwt_token', data.token);
        localStorage.setItem('username', data.username);
        localStorage.setItem('role', data.role);

        window.location.href = '/logs.html';

    } catch (error) {
        showMessage(errorMessage, 'Invalid username or password');
    }
}
function showMessage(element, message) {
    element.textContent = message;
    element.style.display = 'block';
}

function hideMessage(element) {
    element.style.display = 'none';
}