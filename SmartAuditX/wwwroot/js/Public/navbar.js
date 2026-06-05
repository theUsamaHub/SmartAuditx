/**
 * ─────────────────────────────────────────────
 * SECTION: PUBLIC NAVBAR JS
 * ─────────────────────────────────────────────
 * Navbar scroll behavior and mobile interactions.
 */

$(document).ready(function () {
    var $navbar = $('#publicNavbar');

    function handleScroll() {
        if ($(window).scrollTop() > 10) {
            $navbar.addClass('scrolled');
        } else {
            $navbar.removeClass('scrolled');
        }
    }

    handleScroll();
    $(window).on('scroll', handleScroll);

    // Close mobile menu on link click
    $('.public-navbar .nav-link').on('click', function () {
        var $collapse = $('#publicNavCollapse');
        if ($collapse.hasClass('show')) {
            $collapse.collapse('hide');
        }
    });
});
