/* ============================================
   SmartAuditX Admin Panel - Top Navbar Controller
   Handles: Search shortcut, Theme toggle (future)
   ============================================ */

document.addEventListener('DOMContentLoaded', function() {
    // ============================================
    // SEARCH: Keyboard shortcut (Cmd/Ctrl + K)
    // ============================================
    const searchInput = document.querySelector('.search-input');
    
    if (searchInput) {
        document.addEventListener('keydown', function(e) {
            // Cmd+K (Mac) or Ctrl+K (Windows/Linux)
            if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
                e.preventDefault();
                searchInput.focus();
            }
            
            // Escape to blur search
            if (e.key === 'Escape' && document.activeElement === searchInput) {
                searchInput.blur();
            }
        });
    }

    // ============================================
    // THEME: Theme toggle placeholder (future feature)
    // ============================================
    const themeToggle = document.querySelector('[data-theme-toggle]');
    
    if (themeToggle) {
        themeToggle.addEventListener('click', function() {
            // TODO: Implement dark mode toggle
            console.log('Theme toggle clicked - dark mode coming soon');
        });
    }
});
