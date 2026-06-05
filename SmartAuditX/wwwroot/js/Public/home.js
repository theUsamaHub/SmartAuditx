/**
 * ─────────────────────────────────────────────
 * SECTION: HOME PAGE JS
 * ─────────────────────────────────────────────
 * Interactions for the public home page.
 */

$(document).ready(function () {
    // FAQ Accordion
    $('.faq-question').on('click', function () {
        var $item = $(this).closest('.faq-item');
        var $allItems = $('.faq-item');

        if ($item.hasClass('active')) {
            $item.removeClass('active');
        } else {
            $allItems.removeClass('active');
            $item.addClass('active');
        }
    });
});
