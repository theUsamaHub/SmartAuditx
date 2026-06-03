/**
 * ─────────────────────────────────────────────
 * SECTION: LOGIN FLOW JS
 * ─────────────────────────────────────────────
 * Handles AJAX login submission, loading states, and inline verification panel.
 */

$(document).ready(function () {
    const $loginForm = $('#loginForm');
    const $submitBtn = $('#loginSubmit');
    const $submitText = $submitBtn.find('.btn-text');
    const $spinner = $submitBtn.find('.spinner');
    const $globalError = $('#globalError');
    const $verificationPanel = $('#verificationPanel');
    const $authFormContainer = $('.auth-form-container');

    // Handle AJAX form submission
    $loginForm.on('submit', function (e) {
        e.preventDefault();
        
        // Clear previous errors
        $globalError.hide().text('');
        $('.is-invalid').removeClass('is-invalid');
        
        // Basic client validation
        let isValid = true;
        const $identifier = $('#Input_LoginIdentifier');
        const $password = $('#Input_Password');
        
        if (!$identifier.val().trim()) {
            $identifier.addClass('is-invalid');
            isValid = false;
        }
        if (!$password.val().trim()) {
            $password.addClass('is-invalid');
            isValid = false;
        }

        if (!isValid) return;

        // UI Loading state
        $submitBtn.prop('disabled', true);
        $submitText.text('Signing in...');
        $spinner.show();

        const formData = $(this).serialize();

        $.ajax({
            url: $(this).attr('action'),
            type: 'POST',
            data: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (response) {
                if (response.success) {
                    window.location.href = response.redirectUrl;
                } else if (response.requiresVerification) {
                    // Show inline verification panel
                    $authFormContainer.hide();
                    $verificationPanel.addClass('active');
                    $('#resendVerificationBtn').data('userid', response.userId);
                } else {
                    // Display error
                    $globalError.text(response.message).show();
                    $submitBtn.prop('disabled', false);
                    $submitText.text('Sign in');
                    $spinner.hide();
                }
            },
            error: function () {
                $globalError.text('An error occurred during sign in. Please try again.').show();
                $submitBtn.prop('disabled', false);
                $submitText.text('Sign in');
                $spinner.hide();
            }
        });
    });

    // Handle input focus to clear errors
    $('.form-control').on('input', function() {
        $(this).removeClass('is-invalid');
        $globalError.hide();
    });

    // Handle back to login from verification panel
    $('#backToLoginBtn').on('click', function(e) {
        e.preventDefault();
        $verificationPanel.removeClass('active');
        $authFormContainer.fadeIn();
        $('#Input_Password').val(''); // clear password for security
    });

    // Handle resend verification from inline panel
    $('#resendVerificationBtn').on('click', function(e) {
        e.preventDefault();
        const userId = $(this).data('userid');
        const $btn = $(this);
        const originalText = $btn.text();
        
        $btn.prop('disabled', true).text('Sending...');

        $.ajax({
            url: '/Registration/ResendVerificationAjax',
            type: 'POST',
            data: { userId: userId },
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    SmartAuditX.Common.showToast(response.message, 'success');
                    $btn.text('Sent!');
                    setTimeout(() => {
                        $btn.text(originalText).prop('disabled', false);
                    }, 5000); // 5 sec cooldown
                } else {
                    SmartAuditX.Common.showToast(response.message, 'error');
                    $btn.text(originalText).prop('disabled', false);
                }
            },
            error: function() {
                SmartAuditX.Common.showToast('Failed to resend email.', 'error');
                $btn.text(originalText).prop('disabled', false);
            }
        });
    });
});
