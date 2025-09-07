// Theme Management JavaScript Functions

window.setTheme = (theme) => {
    if (theme === 'dark') {
        document.documentElement.setAttribute('data-theme', 'dark');
    } else {
        document.documentElement.removeAttribute('data-theme');
    }
};

window.updateThemeIcon = (isDark) => {
    const icon = document.getElementById('theme-icon');
    if (icon) {
        icon.className = isDark ? 'fas fa-sun' : 'fas fa-moon';
    }
};

// Global Search Function
window.initGlobalSearch = () => {
    const searchInput = document.getElementById('global-search');
    if (searchInput) {
        searchInput.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                const query = searchInput.value.trim();
                if (query) {
                    performGlobalSearch(query);
                }
            }
        });
    }
};

window.performGlobalSearch = (query) => {
    // Bu fonksiyon Blazor component'lerinden çağrılacak
    console.log('Global search for:', query);
    // Search logic will be implemented in Blazor components
};

// Keyboard Shortcuts
window.initKeyboardShortcuts = () => {
    document.addEventListener('keydown', (e) => {
        // Ctrl + K for global search
        if (e.ctrlKey && e.key === 'k') {
            e.preventDefault();
            const searchInput = document.getElementById('global-search');
            if (searchInput) {
                searchInput.focus();
                searchInput.select();
            }
        }
        
        // Ctrl + D for dark mode toggle
        if (e.ctrlKey && e.key === 'd') {
            e.preventDefault();
            const themeButton = document.querySelector('.theme-toggle button');
            if (themeButton) {
                themeButton.click();
            }
        }
        
        // Escape to close modals
        if (e.key === 'Escape') {
            const modals = document.querySelectorAll('.modal.show');
            modals.forEach(modal => {
                const closeButton = modal.querySelector('[data-bs-dismiss="modal"]');
                if (closeButton) {
                    closeButton.click();
                }
            });
        }
    });
};

// Initialize on page load
document.addEventListener('DOMContentLoaded', () => {
    initKeyboardShortcuts();
    initGlobalSearch();
    
    // Load saved theme
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme === 'dark') {
        setTheme('dark');
        updateThemeIcon(true);
    }
});
