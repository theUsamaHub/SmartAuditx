/**
 * ─────────────────────────────────────────────
 * SECTION: REGISTRATION FLOW JS
 * ─────────────────────────────────────────────
 * Handles real-time validation and password strength for AccountInfo.
 * Handles drag-and-drop file upload for CompanyInfo.
 */

$(document).ready(function () {

    // ─────────────────────────────────────────────
    // Account Info Validation
    // ─────────────────────────────────────────────
    
    let debounceTimer;
    
    function validateField(fieldName, value) {
        return new Promise((resolve) => {
            $.ajax({
                url: '/Registration/ValidateFieldAjax',
                type: 'POST',
                data: { field: fieldName, value: value },
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function(response) {
                    resolve(response);
                },
                error: function() {
                    resolve({ success: false, message: 'Validation failed' });
                }
            });
        });
    }

    function setupValidation($input, fieldName) {
        if ($input.length === 0) return;
        
          const $feedback = $input.siblings('.invalid-feedback');
        //const fieldName =
        //    $input.attr('name');

        //const $feedback =
        //    $('[data-valmsg-for="' + fieldName + '"]');
        
        $input.on('keyup', function() {
            clearTimeout(debounceTimer);
            $input.removeClass('is-invalid');
            $feedback.text('');
            
            const value = $(this).val().trim();
            if (!value) return;

            debounceTimer = setTimeout(async () => {
                const response = await validateField(fieldName, value);
                if (!response.success) {
                    $input.addClass('is-invalid');
                    $feedback.text(response.message);
                }
            }, 600); // 600ms debounce
        });
    }

    setupValidation($('#Username'), 'Username');
    setupValidation($('#Email'), 'Email');
    setupValidation($('#PhoneNumber'), 'PhoneNumber');

    // ─────────────────────────────────────────────
    // Password Strength Meter
    // ─────────────────────────────────────────────
    const $password = $('#Password');
    const $strengthContainer = $('.password-strength-container');
    const $strengthText = $('.strength-text');

    if ($password.length > 0) {
        $password.on('input', function() {
            const val = $(this).val();
            $strengthContainer.removeClass('strength-weak strength-fair strength-good strength-strong');
            
            if (!val) {
                $strengthText.text('Strength');
                return;
            }

            let strength = 0;
            if (val.length >= 8) strength++;
            if (val.match(/[a-z]+/)) strength++;
            if (val.match(/[A-Z]+/)) strength++;
            if (val.match(/[0-9]+/)) strength++;
            if (val.match(/[$@#&!]+/)) strength++;

            if (strength <= 2) {
                $strengthContainer.addClass('strength-weak');
                $strengthText.text('Weak');
            } else if (strength === 3) {
                $strengthContainer.addClass('strength-fair');
                $strengthText.text('Fair');
            } else if (strength === 4) {
                $strengthContainer.addClass('strength-good');
                $strengthText.text('Good');
            } else {
                $strengthContainer.addClass('strength-strong');
                $strengthText.text('Strong');
            }
        });
    }

    // ─────────────────────────────────────────────
    // Company Info Drag & Drop
    // ─────────────────────────────────────────────
    const $uploadArea = $('#uploadArea');
    const $fileInput = $('#LogoFile');
    const $uploadText = $('.upload-text');

    if ($uploadArea.length > 0) {
        $uploadArea.on('dragover', function(e) {
            e.preventDefault();
            $(this).addClass('dragover');
        });

        $uploadArea.on('dragleave', function(e) {
            e.preventDefault();
            $(this).removeClass('dragover');
        });

        $uploadArea.on('drop', function(e) {
            e.preventDefault();
            $(this).removeClass('dragover');
            
            const files = e.originalEvent.dataTransfer.files;
            if (files.length > 0) {
                $fileInput[0].files = files;
                updateFileText(files[0].name);
            }
        });

        $uploadArea.on('click', function() {
            $fileInput.click();
        });

        $fileInput.on('change', function() {
            if (this.files && this.files.length > 0) {
                updateFileText(this.files[0].name);
            }
        });

        function updateFileText(name) {
            $uploadText.text(name);
            $uploadArea.find('.upload-hint').text('Click or drag to change');
        }
    }
});
