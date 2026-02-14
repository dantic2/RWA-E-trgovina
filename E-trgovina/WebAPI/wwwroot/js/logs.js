const API_BASE_URL = '/api';

const userInfo = document.getElementById('user-info');
const logoutBtn = document.getElementById('logout-btn');
const refreshBtn = document.getElementById('refresh-btn');
const logCountSelect = document.getElementById('log-count');
const logsContainer = document.getElementById('logs-container');
const loadingDiv = document.getElementById('loading');
const errorMessage = document.getElementById('error-message');

const token = localStorage.getItem('jwt_token');
const username = localStorage.getItem('username');
const role = localStorage.getItem('role');

if (!token) {
    window.location.href = '/login.html';
}

if (username && role) {
    userInfo.textContent = `Logged in as: ${username} (${role})`;
}

logoutBtn.addEventListener('click', handleLogout);
refreshBtn.addEventListener('click', loadLogs);

loadLogs();

async function loadLogs() {
    console.log('loadLogs() called'); 

    hideMessage(errorMessage);
    showLoading();

    const logCount = logCountSelect.value;
    console.log('Selected log count:', logCount); 

    try {
        const url = `${API_BASE_URL}/Log?last=${logCount}`;
        console.log('Fetching:', url); 

        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });

        console.log('Response status:', response.status);

        if (response.status === 401 || response.status === 403) {
            alert('Session expired. Please login again.');
            handleLogout();
            return;
        }

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const logs = await response.json();
        console.log('Logs received:', logs.length); 

        displayLogs(logs);

    } catch (error) {
        console.error('Error loading logs:', error); 
        showMessage(errorMessage, `Error: ${error.message}`);
        logsContainer.innerHTML = '<p style="padding: 20px;">Failed to load logs</p>';
    } finally {
        hideLoading();
    }
}

function displayLogs(logs) {
    if (!logs || logs.length === 0) {
        logsContainer.innerHTML = '<p style="padding:  20px;">No logs found</p>';
        return;
    }

    let html = '<ul class="log-list">';

    logs.forEach(log => {
        const timestamp = new Date(log.timestamp).toLocaleString();
        const levelText = getLevelText(log.level);

        html += `
            <li class="log-item">
                <div class="log-header">
                    <span class="log-level log-level-${log.level}">${levelText}</span>
                    <span class="log-timestamp">${timestamp}</span>
                </div>
                <div class="log-message">${escapeHtml(log.message)}</div>
                ${log.details ? `<div class="log-details">${escapeHtml(log.details)}</div>` : ''}
            </li>
        `;
    });

    html += '</ul>';

    logsContainer.innerHTML = html;
}

function getLevelText(level) {
    const levels = {
        1: 'Debug',
        2: 'Info',
        3: 'Warning',
        4: 'Error',
        5: 'Critical'
    };
    return levels[level] || `Level ${level}`;
}

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function handleLogout() {
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('username');
    localStorage.removeItem('role');
    window.location.href = '/login.html';
}

function showLoading() {
    loadingDiv.style.display = 'block';
    logsContainer.style.display = 'none';
}

function hideLoading() {
    loadingDiv.style.display = 'none';
    logsContainer.style.display = 'block';
}

function showMessage(element, message) {
    element.textContent = message;
    element.style.display = 'block';
}

function hideMessage(element) {
    element.style.display = 'none';
}