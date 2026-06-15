/* ============================================
   SmartAuditX Admin Panel - Layout Initialization
   Global layout setup and utilities
   ============================================ */

document.addEventListener('DOMContentLoaded', function() {
    // ============================================
    // INITIALIZE: Set initial body class for JS-enabled state
    // ============================================
    document.body.classList.add('js-enabled');

    // ============================================
    // TOASTR: Configure global toastr settings
    // ============================================
    if (typeof toastr !== 'undefined') {
        toastr.options = {
            closeButton: true,
            debug: false,
            newestOnTop: true,
            progressBar: true,
            positionClass: 'toast-top-right',
            preventDuplicates: false,
            onclick: null,
            showDuration: '300',
            hideDuration: '1000',
            timeOut: '5000',
            extendedTimeOut: '1000',
            showEasing: 'swing',
            hideEasing: 'linear',
            showMethod: 'fadeIn',
            hideMethod: 'fadeOut'
        };
    }

    console.log('SmartAuditX Admin Panel initialized');
});
