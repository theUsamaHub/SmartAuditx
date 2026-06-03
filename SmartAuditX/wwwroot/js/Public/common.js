/**
 * ─────────────────────────────────────────────
 * SECTION: COMMON PUBLIC JS UTILITIES
 * ─────────────────────────────────────────────
 * Provides toast notifications and standard AJAX helpers using jQuery.
 */

window.SmartAuditX = window.SmartAuditX || {};

SmartAuditX.Common = (function ($) {
    'use strict';

    /**
     * Shows a toast notification.
     * @param {string} message - The message to display.
     * @param {string} type - 'success' or 'error'.
     */
    function showToast(message, type = 'success') {
        const containerId = 'toast-container';
        let $container = $('#' + containerId);
        
        if ($container.length === 0) {
            $container = $('<div id="' + containerId + '" class="toast-container"></div>');
            $('body').append($container);
        }

        const iconHtml = type === 'success' 
            ? '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path><polyline points="22 4 12 14.01 9 11.01"></polyline></svg>'
            : '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="8" x2="12" y2="12"></line><line x1="12" y1="16" x2="12.01" y2="16"></line></svg>';

        const $toast = $(`
            <div class="toast ${type}">
                ${iconHtml}
                <span>${message}</span>
            </div>
        `);

        $container.append($toast);

        // Trigger reflow for animation
        $toast[0].offsetHeight;
        $toast.addClass('show');

        setTimeout(() => {
            $toast.removeClass('show');
            setTimeout(() => $toast.remove(), 300);
        }, 4000);
    }

    /**
     * Formats validation errors from server JSON.
     * @param {object} errors - Validation errors dictionary.
     * @returns {string} Formatted HTML string.
     */
    function formatErrors(errors) {
        if (!errors) return 'An unknown error occurred.';
        if (typeof errors === 'string') return errors;
        
        let html = '<ul class="m-0 pl-4">';
        for (const key in errors) {
            if (errors.hasOwnProperty(key)) {
                const messages = errors[key];
                if (Array.isArray(messages)) {
                    messages.forEach(msg => { html += `<li>${msg}</li>`; });
                } else {
                    html += `<li>${messages}</li>`;
                }
            }
        }
        html += '</ul>';
        return html;
    }

    return {
        showToast: showToast,
        formatErrors: formatErrors
    };

})(jQuery);

$(document).ready(function() {
    // Password toggle functionality
    $(document).on('click', '.password-toggle-btn', function(e) {
        e.preventDefault();
        const $btn = $(this);
        const $input = $btn.siblings('input');
        
        if ($input.attr('type') === 'password') {
            $input.attr('type', 'text');
            $btn.html('<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="icon-eye-off"><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path><line x1="1" y1="1" x2="23" y2="23"></line></svg>');
        } else {
            $input.attr('type', 'password');
            $btn.html('<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="icon-eye"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle></svg>');
        }
    });
});
