// Base CMS script to handle standard CRUD operations
const CMSManager = {
    init: function(config) {
        this.config = config;
        this.table = $(config.tableId).DataTable({
            ajax: {
                url: config.getDataUrl,
                type: 'GET'
            },
            columns: config.columns,
            responsive: true,
            language: {
                emptyTable: "No records found."
            }
        });

        this.bindEvents();
    },

    bindEvents: function() {
        const self = this;

        $(self.config.createBtnId).on('click', function() {
            $(self.config.formId)[0].reset();
            $(self.config.formId).find('[name="Id"]').val(0); // reset ID
            if($(self.config.formId).find('.image-preview').length > 0) {
                $(self.config.formId).find('.image-preview').hide();
            }
            $(self.config.modalId).modal('show');
            $(self.config.modalTitleId).text('Add New Record');
        });

        $(self.config.formId).on('submit', function(e) {
            e.preventDefault();
            self.saveRecord();
        });

        $(self.config.tableId).on('click', '.edit-btn', function() {
            const id = $(this).data('id');
            self.loadRecord(id);
        });

        $(self.config.tableId).on('click', '.delete-btn', function() {
            const id = $(this).data('id');
            self.deleteRecord(id);
        });

        $(self.config.tableId).on('click', '.toggle-btn', function() {
            const id = $(this).data('id');
            self.toggleStatus(id);
        });
        
        // Image preview binder
        $(self.config.formId).find('input[type="file"]').on('change', function() {
            const file = this.files[0];
            if (file) {
                const reader = new FileReader();
                const previewElement = $(self.config.formId).find('.image-preview');
                reader.onload = function(e) {
                    previewElement.attr('src', e.target.result).show();
                }
                reader.readAsDataURL(file);
            }
        });
    },

    saveRecord: function() {
        const self = this;
        const formData = new FormData($(self.config.formId)[0]);
        const idField = $(self.config.formId).find('input[type="hidden"]').first().val();
        const url = (idField && idField != "0") ? self.config.editUrl : self.config.createUrl;

        $.ajax({
            url: url,
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function(response) {
                if (response.success) {
                    $(self.config.modalId).modal('hide');
                    self.table.ajax.reload();
                    toastr.success(response.message);
                } else {
                    toastr.error(response.message);
                }
            },
            error: function() {
                toastr.error('An error occurred while saving the record.');
            }
        });
    },

    loadRecord: function(id) {
        const self = this;
        $.ajax({
            url: self.config.getRecordUrl + '?id=' + id,
            type: 'GET',
            success: function(response) {
                if (response.success) {
                    self.populateForm(response.data);
                    $(self.config.modalId).modal('show');
                    $(self.config.modalTitleId).text('Edit Record');
                } else {
                    toastr.error('Failed to load record.');
                }
            },
            error: function() {
                toastr.error('An error occurred while loading the record.');
            }
        });
    },

    populateForm: function(data) {
        const form = $(this.config.formId);
        form[0].reset();
        
        $.each(data, function(key, value) {
            const input = form.find('[name="' + key + '"]');
            if (input.length) {
                if (input.attr('type') === 'checkbox') {
                    input.prop('checked', value);
                } else if(input.attr('type') !== 'file') {
                    input.val(value);
                }
            }
        });

        // Handle Image preview
        if(data.imageUrl || data.profileImageUrl) {
            const src = data.imageUrl || data.profileImageUrl;
            form.find('.image-preview').attr('src', src).show();
        } else {
            form.find('.image-preview').hide();
        }
    },

    deleteRecord: function(id) {
        const self = this;
        if (confirm('Are you sure you want to delete this record?')) {
            $.ajax({
                url: self.config.deleteUrl,
                type: 'POST',
                data: { id: id },
                success: function(response) {
                    if (response.success) {
                        self.table.ajax.reload();
                        toastr.success(response.message);
                    } else {
                        toastr.error(response.message);
                    }
                },
                error: function() {
                    toastr.error('An error occurred while deleting the record.');
                }
            });
        }
    },

    toggleStatus: function(id) {
        const self = this;
        $.ajax({
            url: self.config.toggleStatusUrl,
            type: 'POST',
            data: { id: id },
            success: function(response) {
                if (response.success) {
                    self.table.ajax.reload(null, false);
                    toastr.success(response.message);
                } else {
                    toastr.error(response.message);
                }
            },
            error: function() {
                toastr.error('An error occurred while updating status.');
            }
        });
    }
};
