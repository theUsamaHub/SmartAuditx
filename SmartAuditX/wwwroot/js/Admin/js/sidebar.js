document.addEventListener('DOMContentLoaded', function() {
    const sidebar = document.getElementById('adminSidebar');
    const mobileToggle = document.getElementById('adminMobileToggle');
    const body = document.body;
    
    // Create overlay
    const overlay = document.createElement('div');
    overlay.className = 'admin-sidebar-overlay';
    body.appendChild(overlay);

    // Mobile toggle functionality
    if (mobileToggle && sidebar) {
        mobileToggle.addEventListener('click', function() {
            sidebar.classList.add('mobile-open');
            overlay.classList.add('show');
            body.style.overflow = 'hidden'; // Prevent background scrolling
        });
    }

    // Close on overlay click
    overlay.addEventListener('click', function() {
        sidebar.classList.remove('mobile-open');
        overlay.classList.remove('show');
        body.style.overflow = ''; // Restore scrolling
    });

    // Close sidebar when a link is clicked on mobile
    const links = sidebar.querySelectorAll('.admin-sidebar-link');
    links.forEach(link => {
        link.addEventListener('click', function() {
            if (window.innerWidth <= 991) {
                sidebar.classList.remove('mobile-open');
                overlay.classList.remove('show');
                body.style.overflow = '';
            }
        });
    });

    // Handle window resize
    window.addEventListener('resize', function() {
        if (window.innerWidth > 991) {
            sidebar.classList.remove('mobile-open');
            overlay.classList.remove('show');
            body.style.overflow = '';
        }
    });

    // Active link highlighting based on current URL path
    const currentPath = window.location.pathname.toLowerCase();
    links.forEach(link => {
        const href = link.getAttribute('href');
        if (href && href !== '#' && href !== 'javascript:void(0);') {
            const linkPath = href.toLowerCase();
            // Basic exact match or starts with (for nested routes)
            if (currentPath === linkPath || (currentPath.startsWith(linkPath) && linkPath !== '/admin')) {
                link.classList.add('active');
            } else if (currentPath === '/admin' && linkPath === '/admin') {
                link.classList.add('active');
            } else {
                link.classList.remove('active');
            }
        }
    });
});
