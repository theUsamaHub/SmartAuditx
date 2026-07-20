// ============================================
// SmartAuditX - Audit Template Builder
// AJAX Operations for Template Management
// ============================================

// Global state
let currentSectionId = null;
let currentFieldId = null;
let selectedSectionId = null;

// Modal instances
let addSectionModal;
let editSectionModal;
let addFieldModal;
let editFieldModal;

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Initialize Bootstrap modals
    addSectionModal = new bootstrap.Modal(document.getElementById('addSectionModal'));
    editSectionModal = new bootstrap.Modal(document.getElementById('editSectionModal'));
    addFieldModal = new bootstrap.Modal(document.getElementById('addFieldModal'));
    editFieldModal = new bootstrap.Modal(document.getElementById('editFieldModal'));
    
    // Initialize weightage visibility based on scoring
    if (!isScoringEnabled) {
        document.getElementById('weightageGroup').style.display = 'none';
        document.getElementById('editWeightageGroup').style.display = 'none';
    }
});

// ============================================
// TEMPLATE OPERATIONS
// ============================================

async function publishTemplate(templateId) {
    try {
        const result = await Swal.fire({
            title: 'Publish Template?',
            text: 'This will make the template available for audits. You cannot edit a published template directly.',
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#00d4a4',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Yes, publish it!'
        });

        if (result.isConfirmed) {
            const response = await fetch(`/AuditTemplate/Publish/${templateId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            });

            const data = await response.json();

            if (data.success) {
                Swal.fire({
                    title: 'Published!',
                    text: data.message,
                    icon: 'success',
                    confirmButtonColor: '#00d4a4'
                }).then(() => {
                    location.reload();
                });
            } else {
                Swal.fire({
                    title: 'Error',
                    text: data.message,
                    icon: 'error',
                    confirmButtonColor: '#d45656'
                });
            }
        }
    } catch (error) {
        console.error('Error publishing template:', error);
        Swal.fire({
            title: 'Error',
            text: 'Failed to publish template. Please try again.',
            icon: 'error',
            confirmButtonColor: '#d45656'
        });
    }
}

async function unpublishTemplate(templateId) {
    try {
        const result = await Swal.fire({
            title: 'Unpublish Template?',
            text: 'This will make the template unavailable for new audits. Existing audits using this template will not be affected.',
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#00d4a4',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Yes, unpublish it!'
        });

        if (result.isConfirmed) {
            const response = await fetch(`/AuditTemplate/Unpublish/${templateId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            });

            const data = await response.json();

            if (data.success) {
                Swal.fire({
                    title: 'Unpublished!',
                    text: data.message,
                    icon: 'success',
                    confirmButtonColor: '#00d4a4'
                }).then(() => {
                    location.reload();
                });
            } else {
                Swal.fire({
                    title: 'Error',
                    text: data.message,
                    icon: 'error',
                    confirmButtonColor: '#d45656'
                });
            }
        }
    } catch (error) {
        console.error('Error unpublishing template:', error);
        Swal.fire({
            title: 'Error',
            text: 'Failed to unpublish template. Please try again.',
            icon: 'error',
            confirmButtonColor: '#d45656'
        });
    }
}

// ============================================
// SECTION OPERATIONS
// ============================================

function openAddSectionModal() {
    document.getElementById('sectionTitle').value = '';
    addSectionModal.show();
}

async function addSection() {
    const title = document.getElementById('sectionTitle').value.trim();

    if (!title) {
        Swal.fire({
            title: 'Validation Error',
            text: 'Section title is required.',
            icon: 'warning',
            confirmButtonColor: '#d45656'
        });
        return;
    }

    try {
        const response = await fetch(`/AuditTemplate/AddSection?templateId=${templateId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify({
                title: title,
                sortOrder: 0
            })
        });

        const data = await response.json();

        if (data.success || data.id) {
            addSectionModal.hide();
            Swal.fire({
                title: 'Success',
                text: 'Section added successfully.',
                icon: 'success',
                confirmButtonColor: '#00d4a4'
            });
            
            // Add the new section to local data and refresh display
            if (data.data) {
                if (!sectionsData) sectionsData = [];
                sectionsData.push(data.data);
                
                // Add section to DOM
                const sectionsContainer = document.getElementById('sections-container');
                const emptyState = sectionsContainer.querySelector('.empty-state');
                if (emptyState) {
                    emptyState.remove();
                }
                
                const sectionHtml = `
                    <div class="section-item" data-section-id="${data.data.id}" data-section-order="${data.data.sortOrder}">
                        <div class="section-header" onclick="selectSection(${data.data.id})">
                            <div class="section-title">
                                <i class="bi bi-folder"></i>
                                <span>${data.data.title}</span>
                            </div>
                            <div class="section-actions">
                                <button type="button" class="btn btn-ghost btn-sm" onclick="event.stopPropagation(); editSection(${data.data.id})" title="Edit">
                                    <i class="bi bi-pencil"></i>
                                </button>
                                <button type="button" class="btn btn-ghost btn-sm" onclick="event.stopPropagation(); deleteSection(${data.data.id})" title="Delete">
                                    <i class="bi bi-trash"></i>
                                </button>
                            </div>
                        </div>
                        <div class="section-fields-count">
                            0 fields
                        </div>
                    </div>
                `;
                sectionsContainer.insertAdjacentHTML('beforeend', sectionHtml);
                
                // Auto-select the new section
                selectSection(data.data.id);
            }
        } else {
            Swal.fire({
                title: 'Error',
                text: data.message || 'Failed to add section.',
                icon: 'error',
                confirmButtonColor: '#d45656'
            });
        }
    } catch (error) {
        console.error('Error adding section:', error);
        Swal.fire({
            title: 'Error',
            text: 'Failed to add section. Please try again.',
            icon: 'error',
            confirmButtonColor: '#d45656'
        });
    }
}

function editSection(sectionId) {
    const sectionItem = document.querySelector(`[data-section-id="${sectionId}"]`);
    const title = sectionItem.querySelector('.section-title span').textContent;
    
    document.getElementById('editSectionId').value = sectionId;
    document.getElementById('editSectionTitle').value = title;
    editSectionModal.show();
}

async function updateSection() {
    const sectionId = parseInt(document.getElementById('editSectionId').value);
    const title = document.getElementById('editSectionTitle').value.trim();

    if (!title) {
        Swal.fire({
            title: 'Validation Error',
            text: 'Section title is required.',
            icon: 'warning',
            confirmButtonColor: '#d45656'
        });
        return;
    }

    try {
        const response = await fetch(`/AuditTemplate/UpdateSection?templateId=${templateId}&sectionId=${sectionId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify({
                title: title,
                sortOrder: 0
            })
        });

        const data = await response.json();

        if (data.success) {
            editSectionModal.hide();
            Swal.fire({
                title: 'Success',
                text: 'Section updated successfully.',
                icon: 'success',
                confirmButtonColor: '#00d4a4'
            });
            
            // Update section in local data and DOM
            const section = sectionsData?.find(s => s.id === sectionId);
            if (section) {
                section.title = title;
                
                // Update section title in DOM
                const sectionItem = document.querySelector(`[data-section-id="${sectionId}"]`);
                if (sectionItem) {
                    const titleSpan = sectionItem.querySelector('.section-title span');
                    if (titleSpan) {
                        titleSpan.textContent = title;
                    }
                }
            }
        } else {
            Swal.fire({
                title: 'Error',
                text: data.message || 'Failed to update section.',
                icon: 'error',
                confirmButtonColor: '#d45656'
            });
        }
    } catch (error) {
        console.error('Error updating section:', error);
        Swal.fire({
            title: 'Error',
            text: 'Failed to update section. Please try again.',
            icon: 'error',
            confirmButtonColor: '#d45656'
        });
    }
}

async function deleteSection(sectionId) {
    try {
        const result = await Swal.fire({
            title: 'Delete Section?',
            text: 'This will delete the section and all its fields. This action cannot be undone.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d45656',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Yes, delete it!'
        });

        if (result.isConfirmed) {
            const response = await fetch(`/AuditTemplate/DeleteSection?templateId=${templateId}&sectionId=${sectionId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            });

            const data = await response.json();

            if (data.success) {
                Swal.fire({
                    title: 'Deleted!',
                    text: 'Section deleted successfully.',
                    icon: 'success',
                    confirmButtonColor: '#00d4a4'
                }).then(() => {
                    location.reload();
                });
            } else {
                Swal.fire({
                    title: 'Error',
                    text: data.message || 'Failed to delete section.',
                    icon: 'error',
                    confirmButtonColor: '#d45656'
                });
            }
        }
    } catch (error) {
        console.error('Error deleting section:', error);
        Swal.fire({
            title: 'Error',
            text: 'Failed to delete section. Please try again.',
            icon: 'error',
            confirmButtonColor: '#d45656'
        });
    }
}

function selectSection(sectionId) {
    selectedSectionId = sectionId;
    
    // Update active state
    document.querySelectorAll('.section-item').forEach(item => {
        item.classList.remove('active');
    });
    document.querySelector(`[data-section-id="${sectionId}"]`).classList.add('active');
    
    // Enable add field button
    document.getElementById('add-field-btn').disabled = false;
    
    // Load fields for this section
    loadFields(sectionId);
}

// ============================================
// FIELD OPERATIONS
// ============================================

function loadFields(sectionId) {
    const fieldsContainer = document.getElementById('fields-container');
    
    // Find the section data
    const section = sectionsData?.find(s => s.id === sectionId);
    
    if (!section) {
        fieldsContainer.innerHTML = `
            <div class="empty-state">
                <i class="bi bi-exclamation-circle"></i>
                <p>Section not found</p>
            </div>
        `;
        return;
    }
    
    if (!section.fields || section.fields.length === 0) {
        fieldsContainer.innerHTML = `
            <div class="empty-state">
                <i class="bi bi-plus-circle"></i>
                <p>No fields yet in this section</p>
                <button type="button" class="btn btn-primary btn-sm" onclick="openAddFieldModal()">
                    Add First Field
                </button>
            </div>
        `;
        return;
    }
    
    // Render fields
    let fieldsHtml = '<div class="fields-list-inner">';
    section.fields.forEach(field => {
        const itemTypeNames = {
            1: 'Boolean',
            2: 'Text',
            3: 'Number',
            4: 'Photo',
            5: 'Barcode',
            6: 'Selection',
            7: 'Rating',
            8: 'Date',
            9: 'Signature'
        };
        const itemTypeName = itemTypeNames[field.itemType] || 'Unknown';
        
        fieldsHtml += `
            <div class="field-item" data-field-id="${field.id}" data-field-order="${field.sortOrder}">
                <div class="field-header" onclick="selectField(${field.id})">
                    <div class="field-info">
                        <span class="field-type-badge">${itemTypeName}</span>
                        <span class="field-question">${field.questionText}</span>
                        ${field.isRequired ? '<span class="field-required-badge">Required</span>' : ''}
                    </div>
                    <div class="field-actions">
                        <button type="button" class="btn btn-ghost btn-sm" onclick="event.stopPropagation(); editField(${field.id})" title="Edit">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button type="button" class="btn btn-ghost btn-sm" onclick="event.stopPropagation(); deleteField(${field.id})" title="Delete">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </div>
                ${field.weightage && isScoringEnabled ? `<div class="field-weightage">Weight: ${field.weightage}</div>` : ''}
            </div>
        `;
    });
    fieldsHtml += '</div>';
    
    fieldsContainer.innerHTML = fieldsHtml;
}

function selectField(fieldId) {
    currentFieldId = fieldId;
    
    // Update active state
    document.querySelectorAll('.field-item').forEach(item => {
        item.classList.remove('active');
    });
    document.querySelector(`[data-field-id="${fieldId}"]`).classList.add('active');
    
    // Load field settings
    loadFieldSettings(fieldId);
}

function loadFieldSettings(fieldId) {
    const settingsContainer = document.getElementById('settings-container');
    
    // Find the field in the sectionsData
    let field = null;
    for (const section of sectionsData) {
        const found = section.fields?.find(f => f.id === fieldId);
        if (found) {
            field = found;
            break;
        }
    }
    
    if (!field) {
        settingsContainer.innerHTML = `
            <div class="empty-state">
                <i class="bi bi-exclamation-circle"></i>
                <p>Field not found</p>
            </div>
        `;
        return;
    }
    
    const itemTypeNames = {
        1: { name: 'Boolean', icon: 'bi-toggle-on' },
        2: { name: 'Text', icon: 'bi-textarea-t' },
        3: { name: 'Number', icon: 'bi-123' },
        4: { name: 'Photo', icon: 'bi-camera' },
        5: { name: 'Barcode', icon: 'bi-upc-scan' },
        6: { name: 'Selection', icon: 'bi-list-ul' }
    };
    
    const itemType = itemTypeNames[field.itemType] || { name: 'Unknown', icon: 'bi-question-circle' };
    
    // Render field settings with better styling
    let settingsHtml = `
        <div class="field-settings-card">
            <div class="field-settings-header">
                <div class="field-type-badge-large">
                    <i class="bi ${itemType.icon}"></i>
                    <span>${itemType.name}</span>
                </div>
                ${field.isRequired ? '<span class="field-required-badge-large">Required</span>' : ''}
            </div>
            
            <div class="field-settings-body">
                <div class="setting-item">
                    <div class="setting-label">
                        <i class="bi bi-chat-text"></i>
                        <span>Question</span>
                    </div>
                    <div class="setting-content">${field.questionText}</div>
                </div>
                
                ${isScoringEnabled ? `
                <div class="setting-item">
                    <div class="setting-label">
                        <i class="bi bi-sliders"></i>
                        <span>Weightage</span>
                    </div>
                    <div class="setting-content">${field.weightage || '1.00'}</div>
                </div>
                ` : ''}
                
                ${field.options && field.options.length > 0 ? `
                <div class="setting-item">
                    <div class="setting-label">
                        <i class="bi bi-list-check"></i>
                        <span>Options</span>
                    </div>
                    <div class="setting-content">
                        <div class="options-list">
                            ${field.options.map(o => `
                                <div class="option-item">
                                    <i class="bi bi-check-circle"></i>
                                    <span>${o.text}</span>
                                </div>
                            `).join('')}
                        </div>
                    </div>
                </div>
                ` : ''}
            </div>
            
            <div class="field-settings-footer">
                <button type="button" class="btn btn-primary btn-sm" onclick="editField(${field.id})">
                    <i class="bi bi-pencil"></i> Edit Field
                </button>
                <button type="button" class="btn btn-danger btn-sm" onclick="deleteField(${field.id})">
                    <i class="bi bi-trash"></i> Delete Field
                </button>
            </div>
        </div>
    `;
    
    settingsContainer.innerHTML = settingsHtml;
}

function openAddFieldModal() {
    if (!selectedSectionId) {
        Swal.fire({
            title: 'No Section Selected',
            text: 'Please select a section first.',
            icon: 'warning',
            confirmButtonColor: '#d45656'
        });
        return;
    }
    
    document.getElementById('fieldQuestionText').value = '';
    document.getElementById('fieldItemType').value = '';
    document.getElementById('fieldIsRequired').checked = false;
    document.getElementById('fieldWeightage').value = '1.00';
    document.getElementById('optionsContainer').innerHTML = `
        <div class="option-item">
            <input type="text" class="form-control option-input" placeholder="Option 1" />
        </div>
    `;
    document.getElementById('optionsGroup').style.display = 'none';
    
    addFieldModal.show();
}

function toggleOptionsField() {
    const itemType = parseInt(document.getElementById('fieldItemType').value);
    const optionsGroup = document.getElementById('optionsGroup');
    const ratingRangeGroup = document.getElementById('ratingRangeGroup');
    
    if (itemType === 6) { // Selection/Dropdown
        optionsGroup.style.display = 'block';
        ratingRangeGroup.style.display = 'none';
    } else if (itemType === 7) { // Rating
        optionsGroup.style.display = 'none';
        ratingRangeGroup.style.display = 'block';
    } else {
        optionsGroup.style.display = 'none';
        ratingRangeGroup.style.display = 'none';
    }
}

function addOptionInput() {
    const container = document.getElementById('optionsContainer');
    const optionCount = container.children.length + 1;
    
    const optionItem = document.createElement('div');
    optionItem.className = 'option-item';
    optionItem.innerHTML = `
        <input type="text" class="form-control option-input" placeholder="Option ${optionCount}" />
        <button type="button" class="btn btn-danger btn-sm" onclick="this.parentElement.remove()">
            <i class="bi bi-trash"></i>
        </button>
    `;
    
    container.appendChild(optionItem);
}

async function addField() {
    const questionText = document.getElementById('fieldQuestionText').value.trim();
    const itemType = parseInt(document.getElementById('fieldItemType').value);
    const isRequired = document.getElementById('fieldIsRequired').checked;
    const weightage = parseFloat(document.getElementById('fieldWeightage').value) || 1.00;

    if (!questionText) {
        Swal.fire({
            title: 'Validation Error',
            text: 'Question text is required.',
            icon: 'warning',
            confirmButtonColor: '#d45656'
        });
        return;
    }

    if (!itemType) {
        Swal.fire({
            title: 'Validation Error',
            text: 'Field type is required.',
            icon: 'warning',
            confirmButtonColor: '#d45656'
        });
        return;
    }

    // Collect options if selection type
    let options = [];
    if (itemType === 6) {
        const optionInputs = document.querySelectorAll('#optionsContainer .option-input');
        optionInputs.forEach(input => {
            if (input.value.trim()) {
                options.push({ text: input.value.trim() });
            }
        });

        if (options.length < 2) {
            Swal.fire({
                title: 'Validation Error',
                text: 'At least 2 options are required for dropdown fields.',
                icon: 'warning',
                confirmButtonColor: '#d45656'
            });
            return;
        }
    }

    try {
        const response = await fetch(`/AuditTemplate/AddField?templateId=${templateId}&sectionId=${selectedSectionId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify({
                questionText: questionText,
                itemType: itemType,
                isRequired: isRequired,
                weightage: weightage,
                options: options
            })
        });

        const data = await response.json();

        if (data.success || data.id) {
            addFieldModal.hide();
            Swal.fire({
                title: 'Success',
                text: 'Field added successfully.',
                icon: 'success',
                confirmButtonColor: '#00d4a4'
            });
            
            // Add the new field to local data and refresh display
            if (data.data) {
                const section = sectionsData?.find(s => s.id === selectedSectionId);
                if (section) {
                    if (!section.fields) section.fields = [];
                    section.fields.push(data.data);
                    loadFields(selectedSectionId);
                    
                    // Update field count in section
                    const sectionItem = document.querySelector(`[data-section-id="${selectedSectionId}"]`);
                    const fieldsCount = sectionItem.querySelector('.section-fields-count');
                    if (fieldsCount) {
                        fieldsCount.textContent = `${section.fields.length} fields`;
                    }
                }
            }
        } else {
            Swal.fire({
                title: 'Error',
                text: data.message || 'Failed to add field.',
                icon: 'error',
                confirmButtonColor: '#d45656'
            });
        }
    } catch (error) {
        console.error('Error adding field:', error);
        Swal.fire({
            title: 'Error',
            text: 'Failed to add field. Please try again.',
            icon: 'error',
            confirmButtonColor: '#d45656'
        });
    }
}

function editField(fieldId) {
    // Find the field in the sectionsData
    let field = null;
    for (const section of sectionsData) {
        const found = section.fields?.find(f => f.id === fieldId);
        if (found) {
            field = found;
            break;
        }
    }
    
    if (!field) {
        Swal.fire({
            title: 'Error',
            text: 'Field not found.',
            icon: 'error',
            confirmButtonColor: '#d45656'
        });
        return;
    }
    
    // Populate the edit modal with field data
    document.getElementById('editFieldId').value = fieldId;
    document.getElementById('editFieldQuestionText').value = field.questionText || '';
    document.getElementById('editFieldItemType').value = field.itemType || '';
    document.getElementById('editFieldIsRequired').checked = field.isRequired || false;
    document.getElementById('editFieldWeightage').value = field.weightage || '1.00';
    
    // Handle options if selection type
    const editOptionsContainer = document.getElementById('editOptionsContainer');
    const editOptionsGroup = document.getElementById('editOptionsGroup');
    
    if (field.itemType === 6 && field.options && field.options.length > 0) {
        editOptionsGroup.style.display = 'block';
        editOptionsContainer.innerHTML = field.options.map((option, index) => `
            <div class="option-item">
                <input type="text" class="form-control option-input" placeholder="Option ${index + 1}" value="${option.text || ''}" />
                <button type="button" class="btn btn-danger btn-sm" onclick="this.parentElement.remove()">
                    <i class="bi bi-trash"></i>
                </button>
            </div>
        `).join('');
    } else {
        editOptionsGroup.style.display = 'none';
        editOptionsContainer.innerHTML = '';
    }
    
    editFieldModal.show();
}

function toggleEditOptionsField() {
    const itemType = parseInt(document.getElementById('editFieldItemType').value);
    const optionsGroup = document.getElementById('editOptionsGroup');
    const ratingRangeGroup = document.getElementById('editRatingRangeGroup');
    
    if (itemType === 6) { // Selection/Dropdown
        optionsGroup.style.display = 'block';
        ratingRangeGroup.style.display = 'none';
    } else if (itemType === 7) { // Rating
        optionsGroup.style.display = 'none';
        ratingRangeGroup.style.display = 'block';
    } else {
        optionsGroup.style.display = 'none';
        ratingRangeGroup.style.display = 'none';
    }
}

function addEditOptionInput() {
    const container = document.getElementById('editOptionsContainer');
    const optionCount = container.children.length + 1;
    
    const optionItem = document.createElement('div');
    optionItem.className = 'option-item';
    optionItem.innerHTML = `
        <input type="text" class="form-control option-input" placeholder="Option ${optionCount}" />
        <button type="button" class="btn btn-danger btn-sm" onclick="this.parentElement.remove()">
            <i class="bi bi-trash"></i>
        </button>
    `;
    
    container.appendChild(optionItem);
}

async function updateField() {
    const fieldId = parseInt(document.getElementById('editFieldId').value);
    const questionText = document.getElementById('editFieldQuestionText').value.trim();
    const itemType = parseInt(document.getElementById('editFieldItemType').value);
    const isRequired = document.getElementById('editFieldIsRequired').checked;
    const weightage = parseFloat(document.getElementById('editFieldWeightage').value) || 1.00;

    if (!questionText) {
        Swal.fire({
            title: 'Validation Error',
            text: 'Question text is required.',
            icon: 'warning',
            confirmButtonColor: '#d45656'
        });
        return;
    }

    if (!itemType) {
        Swal.fire({
            title: 'Validation Error',
            text: 'Field type is required.',
            icon: 'warning',
            confirmButtonColor: '#d45656'
        });
        return;
    }

    // Collect options if selection type
    let options = [];
    if (itemType === 6) {
        const optionInputs = document.querySelectorAll('#editOptionsContainer .option-input');
        optionInputs.forEach(input => {
            if (input.value.trim()) {
                options.push({ text: input.value.trim() });
            }
        });

        if (options.length < 2) {
            Swal.fire({
                title: 'Validation Error',
                text: 'At least 2 options are required for dropdown fields.',
                icon: 'warning',
                confirmButtonColor: '#d45656'
            });
            return;
        }
    }

    try {
        const response = await fetch(`/AuditTemplate/UpdateField?templateId=${templateId}&sectionId=${selectedSectionId}&fieldId=${fieldId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify({
                questionText: questionText,
                itemType: itemType,
                isRequired: isRequired,
                weightage: weightage,
                options: options
            })
        });

        const data = await response.json();

        if (data.success) {
            editFieldModal.hide();
            Swal.fire({
                title: 'Success',
                text: 'Field updated successfully.',
                icon: 'success',
                confirmButtonColor: '#00d4a4'
            }).then(() => {
                location.reload();
            });
        } else {
            Swal.fire({
                title: 'Error',
                text: data.message || 'Failed to update field.',
                icon: 'error',
                confirmButtonColor: '#d45656'
            });
        }
    } catch (error) {
        console.error('Error updating field:', error);
        Swal.fire({
            title: 'Error',
            text: 'Failed to update field. Please try again.',
            icon: 'error',
            confirmButtonColor: '#d45656'
        });
    }
}

async function deleteField(fieldId) {
    try {
        const result = await Swal.fire({
            title: 'Delete Field?',
            text: 'This will delete the field and all its options. This action cannot be undone.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d45656',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Yes, delete it!'
        });

        if (result.isConfirmed) {
            const response = await fetch(`/AuditTemplate/DeleteField?templateId=${templateId}&sectionId=${selectedSectionId}&fieldId=${fieldId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            });

            const data = await response.json();

            if (data.success) {
                Swal.fire({
                    title: 'Deleted!',
                    text: 'Field deleted successfully.',
                    icon: 'success',
                    confirmButtonColor: '#00d4a4'
                }).then(() => {
                    location.reload();
                });
            } else {
                Swal.fire({
                    title: 'Error',
                    text: data.message || 'Failed to delete field.',
                    icon: 'error',
                    confirmButtonColor: '#d45656'
                });
            }
        }
    } catch (error) {
        console.error('Error deleting field:', error);
        Swal.fire({
            title: 'Error',
            text: 'Failed to delete field. Please try again.',
            icon: 'error',
            confirmButtonColor: '#d45656'
        });
    }
}
