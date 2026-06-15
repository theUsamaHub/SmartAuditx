/* ============================================
   SmartAuditX Admin Panel - Sidebar Controller
   Handles: Toggle, Collapse, Mobile Off-Canvas, Active State
   ============================================ */

document.addEventListener('DOMContentLoaded', function() {
    const sidebar = document.getElementById('adminSidebar');
    const sidebarToggle = document.querySelector('[data-sidebar-toggle]') || document.getElementById('adminMobileToggle');
    const sidebarBackdrop = document.querySelector('.sidebar-backdrop');
    const body = document.body;

    if (!sidebar || !sidebarToggle) return;

    // ============================================
    // DESKTOP: Sidebar Collapse/Expand
    // ============================================
    let isCollapsed = false;

    // Restore collapsed state from localStorage
    const savedState = localStorage.getItem('admin-sidebar-collapsed');
    if (savedState === 'true' && window.innerWidth >= 1024) {
        sidebar.classList.add('collapsed');
        isCollapsed = true;
        if (sidebarToggle.hasAttribute('data-sidebar-toggle')) {
            sidebarToggle.classList.add('active');
        }
    }

    sidebarToggle.addEventListener('click', function() {
        if (window.innerWidth >= 1024) {
            // Desktop: Toggle collapse
            isCollapsed = !isCollapsed;
            sidebar.classList.toggle('collapsed', isCollapsed);
            if (sidebarToggle.hasAttribute('data-sidebar-toggle')) {
                sidebarToggle.classList.toggle('active', isCollapsed);
            }
            localStorage.setItem('admin-sidebar-collapsed', isCollapsed);
        } else {
            // Mobile/Tablet: Toggle off-canvas
            sidebar.classList.toggle('mobile-open');
            if (sidebarBackdrop) {
                sidebarBackdrop.classList.toggle('active');
            }
            body.style.overflow = sidebar.classList.contains('mobile-open') ? 'hidden' : '';
        }
    });

    // ============================================
    // MOBILE/TABLET: Close on backdrop click
    // ============================================
    if (sidebarBackdrop) {
        sidebarBackdrop.addEventListener('click', function() {
            sidebar.classList.remove('mobile-open');
            sidebarBackdrop.classList.remove('active');
            body.style.overflow = '';
        });
    }

    // ============================================
    // MOBILE/TABLET: Close sidebar when link clicked
    // ============================================
    const navLinks = sidebar.querySelectorAll('.nav-link, .admin-sidebar-link');
    navLinks.forEach(link => {
        link.addEventListener('click', function() {
            if (window.innerWidth < 1024) {
                sidebar.classList.remove('mobile-open');
                if (sidebarBackdrop) {
                    sidebarBackdrop.classList.remove('active');
                }
                body.style.overflow = '';
            }
        });
    });

    // ============================================
    // RESPONSIVE: Handle window resize
    // ============================================
    let resizeTimer;
    window.addEventListener('resize', function() {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function() {
            if (window.innerWidth >= 1024) {
                // Reset mobile states when going to desktop
                sidebar.classList.remove('mobile-open');
                if (sidebarBackdrop) {
                    sidebarBackdrop.classList.remove('active');
                }
                body.style.overflow = '';
            } else {
                // Reset collapsed state when going to mobile
                sidebar.classList.remove('collapsed');
                if (sidebarToggle.hasAttribute('data-sidebar-toggle')) {
                    sidebarToggle.classList.remove('active');
                }
            }
        }, 250);
    });

    // ============================================
    // ACTIVE LINK: Highlight based on current URL
    // ============================================
    const currentPath = window.location.pathname.toLowerCase();
    navLinks.forEach(link => {
        const href = link.getAttribute('href');
        if (href && href !== '#' && href !== 'javascript:void(0);') {
            const linkPath = href.toLowerCase();
            
            // Remove all active states first
            link.classList.remove('active');
            link.removeAttribute('aria-current');

            // Check for exact match or parent path match
            if (currentPath === linkPath || (currentPath.startsWith(linkPath) && linkPath !== '/admin')) {
                link.classList.add('active');
                link.setAttribute('aria-current', 'page');
            } else if (currentPath === '/admin' && linkPath.includes('/admin')) {
                link.classList.add('active');
                link.setAttribute('aria-current', 'page');
            }
        }
    });

    // ============================================
    // KEYBOARD: Close sidebar on Escape key
    // ============================================
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape' && sidebar.classList.contains('mobile-open')) {
            sidebar.classList.remove('mobile-open');
            if (sidebarBackdrop) {
                sidebarBackdrop.classList.remove('active');
            }
            body.style.overflow = '';
        }
    });
});
