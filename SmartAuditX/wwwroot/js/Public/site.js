/**
 * ─────────────────────────────────────────────
 * SECTION: PUBLIC SITE JS
 * ─────────────────────────────────────────────
 * General utilities for the public website.
 */

$(document).ready(function () {
    // Smooth scroll for anchor links
    $('a[href^="#"]').on('click', function (e) {
        var target = $(this.getAttribute('href'));
        if (target.length) {
            e.preventDefault();
            $('html, body').stop().animate({
                scrollTop: target.offset().top - 80
            }, 400);
        }
    });
});
