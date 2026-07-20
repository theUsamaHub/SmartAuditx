/**
 * Auditor Conduct JS
 * Handles audit execution: starting, saving progress, submitting responses
 */

document.addEventListener('DOMContentLoaded', function () {
    // Initialize rating buttons
    initRatingButtons();

    // Initialize signature pads if any
    initSignaturePads();
});

function initRatingButtons() {
    document.querySelectorAll('.rating-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            const fieldId = this.dataset.fieldId;
            const value = parseInt(this.dataset.value);
            const container = this.closest('.rating-container');

            // Update active state
            container.querySelectorAll('.rating-btn').forEach(b => b.classList.remove('active'));
            this.classList.add('active');

            // Update hidden input
            const hiddenInput = container.querySelector('.response-number');
            if (hiddenInput) {
                hiddenInput.value = value;
            }
        });
    });
}

function initSignaturePads() {
    document.querySelectorAll('.signature-canvas').forEach(canvas => {
        const ctx = canvas.getContext('2d');
        let isDrawing = false;
        let lastX = 0;
        let lastY = 0;

        // Set drawing style
        ctx.strokeStyle = '#000';
        ctx.lineWidth = 2;
        ctx.lineCap = 'round';

        canvas.addEventListener('mousedown', (e) => {
            isDrawing = true;
            [lastX, lastY] = [e.offsetX, e.offsetY];
        });

        canvas.addEventListener('mousemove', (e) => {
            if (!isDrawing) return;
            ctx.beginPath();
            ctx.moveTo(lastX, lastY);
            ctx.lineTo(e.offsetX, e.offsetY);
            ctx.stroke();
            [lastX, lastY] = [e.offsetX, e.offsetY];
        });

        canvas.addEventListener('mouseup', () => {
            isDrawing = false;
            // Save signature data to hidden input
            const fieldId = canvas.id.replace('signaturePad_', '');
            const hiddenInput = document.querySelector(`.signature-data[data-field-id="${fieldId}"]`);
            if (hiddenInput) {
                hiddenInput.value = canvas.toDataURL();
            }
        });

        canvas.addEventListener('mouseleave', () => {
            isDrawing = false;
        });
    });
}

function clearSignature(fieldId) {
    const canvas = document.getElementById(`signaturePad_${fieldId}`);
    if (canvas) {
        const ctx = canvas.getContext('2d');
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        const hiddenInput = document.querySelector(`.signature-data[data-field-id="${fieldId}"]`);
        if (hiddenInput) {
            hiddenInput.value = '';
        }
    }
}

function collectResponses() {
    const responses = [];
    const formGroups = document.querySelectorAll('.form-group[data-field-id]');

    formGroups.forEach(group => {
        const fieldId = parseInt(group.dataset.fieldId);
        const fieldType = parseInt(group.dataset.fieldType);

        const response = {
            auditTemplateFieldId: fieldId,
            isSkipped: false
        };

        switch (fieldType) {
            case 1: // Boolean
                const checkedRadio = group.querySelector(`input[name="field_${fieldId}"]:checked`);
                response.responseBoolean = checkedRadio ? checkedRadio.value === 'true' : null;
                if (!checkedRadio) response.isSkipped = true;
                break;

            case 2: // Text
                const textArea = group.querySelector('.response-text');
                response.responseText = textArea ? textArea.value : null;
                if (!response.responseText) response.isSkipped = true;
                break;

            case 3: // Number
                const numberInput = group.querySelector('.response-number');
                response.responseNumber = numberInput && numberInput.value ? parseFloat(numberInput.value) : null;
                if (!response.responseNumber && response.responseNumber !== 0) response.isSkipped = true;
                break;

            case 7: // Rating
                const ratingInput = group.querySelector('.response-number');
                response.responseNumber = ratingInput && ratingInput.value ? parseFloat(ratingInput.value) : null;
                if (!response.responseNumber && response.responseNumber !== 0) response.isSkipped = true;
                break;

            case 6: // Selection
                const selectInput = group.querySelector('.response-option');
                response.selectedOptionId = selectInput && selectInput.value ? parseInt(selectInput.value) : null;
                if (!response.selectedOptionId) response.isSkipped = true;
                break;

            case 8: // Date
                const dateInput = group.querySelector('.response-date');
                response.responseDate = dateInput && dateInput.value ? dateInput.value : null;
                if (!response.responseDate) response.isSkipped = true;
                break;

            case 9: // Signature
                const sigInput = group.querySelector('.signature-data');
                response.responseText = sigInput ? sigInput.value : null;
                if (!response.responseText) response.isSkipped = true;
                break;
        }

        // Collect notes
        const notesInput = group.querySelector('.response-notes');
        response.notes = notesInput ? notesInput.value : null;

        responses.push(response);
    });

    return responses;
}

async function startAudit() {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    try {
        const response = await fetch(`/Auditor/Start/${auditId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            }
        });

        const data = await response.json();

        if (data.success) {
            Swal.fire({
                title: 'Audit Started',
                text: 'You can now fill in the responses.',
                icon: 'success',
                confirmButtonColor: '#00d4a4'
            }).then(() => {
                location.reload();
            });
        } else {
            Swal.fire('Error', data.message || 'Failed to start audit.', 'error');
        }
    } catch (error) {
        console.error('Error starting audit:', error);
        Swal.fire('Error', 'Failed to start audit.', 'error');
    }
}

async function saveProgress() {
    const responses = collectResponses();
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    try {
        const response = await fetch(`/Auditor/SaveProgress/${auditId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(responses)
        });

        const data = await response.json();

        if (data.success) {
            Swal.fire({
                title: 'Saved',
                text: 'Your progress has been saved.',
                icon: 'success',
                confirmButtonColor: '#00d4a4'
            });
        } else {
            Swal.fire('Error', data.message || 'Failed to save progress.', 'error');
        }
    } catch (error) {
        console.error('Error saving progress:', error);
        Swal.fire('Error', 'Failed to save progress.', 'error');
    }
}

async function submitAudit() {
    const responses = collectResponses();

    // Validate required fields
    const missingRequired = [];
    document.querySelectorAll('.form-group[data-field-id]').forEach(group => {
        const fieldId = parseInt(group.dataset.fieldId);
        const isRequired = group.querySelector('.text-danger') !== null; // Check if required marker exists
        if (isRequired) {
            const response = responses.find(r => r.auditTemplateFieldId === fieldId);
            if (response && response.isSkipped) {
                const question = group.querySelector('.form-label')?.textContent?.trim() || `Field ${fieldId}`;
                missingRequired.push(question.replace('*', '').trim());
            }
        }
    });

    if (missingRequired.length > 0) {
        Swal.fire({
            title: 'Missing Required Fields',
            text: `Please fill in: ${missingRequired.join(', ')}`,
            icon: 'warning',
            confirmButtonColor: '#d45656'
        });
        return;
    }

    const result = await Swal.fire({
        title: 'Submit Audit',
        text: 'Are you sure you want to submit this audit? This action cannot be undone.',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#00d4a4',
        cancelButtonColor: '#5a5a5c',
        confirmButtonText: 'Submit'
    });

    if (!result.isConfirmed) return;

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    try {
        const response = await fetch(`/Auditor/Submit/${auditId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(responses)
        });

        const data = await response.json();

        if (data.success) {
            Swal.fire({
                title: 'Audit Submitted',
                text: 'Your audit has been submitted for review.',
                icon: 'success',
                confirmButtonColor: '#00d4a4'
            }).then(() => {
                window.location.href = '/Auditor';
            });
        } else {
            Swal.fire('Error', data.message || 'Failed to submit audit.', 'error');
        }
    } catch (error) {
        console.error('Error submitting audit:', error);
        Swal.fire('Error', 'Failed to submit audit.', 'error');
    }
}

// ============================================
// BARCODE SCAN FUNCTIONS
// ============================================

async function scanBarcode() {
    const barcode = document.getElementById('barcodeInput').value.trim();
    const quantity = parseFloat(document.getElementById('barcodeQuantity').value) || 1;
    const resultDiv = document.getElementById('scanResult');

    if (!barcode) {
        resultDiv.className = 'alert alert-danger mb-3';
        resultDiv.textContent = 'Please enter a barcode value.';
        resultDiv.classList.remove('d-none');
        return;
    }

    try {
        const response = await fetch('/Auditor/ScanBarcode/' + auditId, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({ barcodeValue: barcode, actualQuantity: quantity })
        });

        const result = await response.json();

        if (result.success) {
            resultDiv.className = 'alert alert-success mb-3';
            resultDiv.textContent = result.message + (result.inventoryItem ? ` - ${result.inventoryItem.itemName} (Expected: ${result.inventoryItem.expectedQuantity})` : '');
            resultDiv.classList.remove('d-none');
            document.getElementById('barcodeInput').value = '';
            document.getElementById('barcodeQuantity').value = '1';
            loadScans();
        } else {
            resultDiv.className = 'alert alert-danger mb-3';
            resultDiv.textContent = result.message;
            resultDiv.classList.remove('d-none');
        }
    } catch (error) {
        console.error('Error scanning barcode:', error);
        resultDiv.className = 'alert alert-danger mb-3';
        resultDiv.textContent = 'Error scanning barcode.';
        resultDiv.classList.remove('d-none');
    }
}

async function loadScans() {
    try {
        const response = await fetch('/Auditor/ComparisonReport/' + auditId);
        const data = await response.json();
        if (data.success) {
            renderScansTable(data.data);
        }
    } catch (error) {
        console.error('Error loading scans:', error);
    }
}

function renderScansTable(scans) {
    const tbody = document.getElementById('scansTableBody');
    if (!scans || scans.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">No scans yet</td></tr>';
        return;
    }

    const statusColors = {
        'Matched': 'success',
        'Surplus': 'info',
        'Shortage': 'warning',
        'Missing': 'danger',
        'Unrecognized': 'secondary'
    };

    tbody.innerHTML = scans.map(scan => `
        <tr>
            <td><code>${escapeHtml(scan.barcodeValue)}</code></td>
            <td>${escapeHtml(scan.itemName)}</td>
            <td>${scan.expectedQuantity}</td>
            <td>${scan.actualQuantity}</td>
            <td><span class="badge bg-${statusColors[scan.status] || 'secondary'}">${scan.status}</span></td>
            <td>
                ${scan.status !== 'Missing' ? `<button type="button" class="btn btn-sm btn-outline-danger" onclick="deleteScan(${scan.id || 0})"><i class="bi bi-trash"></i></button>` : ''}
            </td>
        </tr>
    `).join('');
}

async function deleteScan(scanId) {
    if (!scanId) return;
    try {
        await fetch('/Auditor/DeleteScan?scanId=' + scanId + '&auditId=' + auditId, {
            method: 'POST',
            headers: { 'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value }
        });
        loadScans();
    } catch (error) {
        console.error('Error deleting scan:', error);
    }
}

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}
