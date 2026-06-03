/**
 * ─────────────────────────────────────────────
 * SECTION: VERIFICATION JS
 * ─────────────────────────────────────────────
 * Handles AJAX resend verification functionality on status pages.
 */

$(document).ready(function () {
    $('.btn-resend-verification').on('click', function(e) {
        e.preventDefault();
        
        const $btn = $(this);
        const userId = $btn.data('userid');
        
        if (!userId) {
            SmartAuditX.Common.showToast('User ID missing.', 'error');
            return;
        }

        const originalText = $btn.html();
        $btn.prop('disabled', true).html('<span class="spinner spinner-dark"></span> Sending...');

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
                    $btn.html('Sent successfully!');
                    
                    // 60 second cooldown before they can click again
                    setTimeout(() => {
                        $btn.html(originalText).prop('disabled', false);
                    }, 60000); 
                } else {
                    SmartAuditX.Common.showToast(response.message, 'error');
                    $btn.html(originalText).prop('disabled', false);
                }
            },
            error: function() {
                SmartAuditX.Common.showToast('Failed to connect to the server.', 'error');
                $btn.html(originalText).prop('disabled', false);
            }
        });
    });
});
