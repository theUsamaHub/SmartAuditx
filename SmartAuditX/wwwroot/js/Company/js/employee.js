"use strict";

(function () {
  var config = window.employeeConfig || {};
  var employeeModal;
  var detailsModal;
  var deleteModal;
  var createSystemUserModal;
  var editingEmployeeId = null;
  var pendingDeleteId = null;
  var viewingEmployeeId = null;
  var creatingSystemUserForEmployeeId = null;
  var searchTimer;
  var systemPhoneInputInstance;
  var systemUserPhoneInputInstance;

  function onReady(callback) {
    if (document.readyState === "loading") {
      document.addEventListener("DOMContentLoaded", callback);
      return;
    }
    callback();
  }

  function getAntiForgeryToken(formId) {
    var selector = formId ? "#" + formId + " input[name='__RequestVerificationToken']" : "input[name='__RequestVerificationToken']";
    var tokenInput = document.querySelector(selector);
    return tokenInput ? tokenInput.value : "";
  }

  function escapeHtml(value) {
    return String(value || "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function showPageAlert(message, type) {
    var alert = document.getElementById("employeeAlert");
    if (!alert) return;

    alert.className = "alert alert-" + (type || "success");
    alert.textContent = message;
    alert.classList.remove("d-none");

    setTimeout(function () {
      alert.classList.add("d-none");
    }, 5000);
  }

  function hidePageAlert() {
    var alert = document.getElementById("employeeAlert");
    if (alert) alert.classList.add("d-none");
  }

  function showFormAlert(message) {
    var alert = document.getElementById("employeeFormAlert");
    if (!alert) return;

    alert.textContent = message;
    alert.classList.remove("d-none");
  }

  function hideFormAlert() {
    var alert = document.getElementById("employeeFormAlert");
    if (alert) {
      alert.classList.add("d-none");
      alert.textContent = "";
    }
  }

  function initSystemPhoneInput() {
    var phoneField = document.getElementById("SystemPhoneNumberInput");
    if (!phoneField || !window.intlTelInput) {
      return;
    }

    if (systemPhoneInputInstance) {
      systemPhoneInputInstance.destroy();
    }

    systemPhoneInputInstance = window.intlTelInput(phoneField, {
      initialCountry: "pk",
      preferredCountries: ["pk", "ae", "sa", "us", "gb"],
      separateDialCode: true,
      strictMode: true,
      utilsScript: "https://cdn.jsdelivr.net/npm/intl-tel-input@25.3.1/build/js/utils.js"
    });
  }

  function syncSystemPhoneFields() {
    var phoneField = document.getElementById("SystemPhoneNumberInput");
    var dialCodeInput = document.getElementById("SystemPhoneDialCode");
    var phoneNumberInput = document.getElementById("SystemPhoneNumber");

    if (!systemPhoneInputInstance || !dialCodeInput || !phoneNumberInput || !phoneField) {
      return false;
    }

    var countryData = systemPhoneInputInstance.getSelectedCountryData();
    var digits = phoneField.value.replace(/\D/g, "");

    if (!digits) {
      phoneField.classList.add("is-invalid");
      return false;
    }

    dialCodeInput.value = "+" + countryData.dialCode;
    phoneNumberInput.value = systemPhoneInputInstance.getNumber();
    phoneField.classList.remove("is-invalid");
    return true;
  }

  function getFilterValues() {
    return {
      branchId: document.getElementById("filterBranch")?.value || "",
      departmentId: document.getElementById("filterDepartment")?.value || "",
      designationId: document.getElementById("filterDesignation")?.value || "",
      isActive: document.getElementById("filterStatus")?.value || "",
      search: document.getElementById("searchEmployees")?.value.trim() || ""
    };
  }

  function buildListUrl() {
    var filters = getFilterValues();
    var params = new URLSearchParams();

    if (filters.branchId) params.set("branchId", filters.branchId);
    if (filters.departmentId) params.set("departmentId", filters.departmentId);
    if (filters.designationId) params.set("designationId", filters.designationId);
    if (filters.isActive) params.set("isActive", filters.isActive);
    if (filters.search) params.set("search", filters.search);

    var query = params.toString();
    return query ? config.listUrl + "?" + query : config.listUrl;
  }

  function renderEmployees(employees) {
    var tbody = document.getElementById("employeesTableBody");
    var emptyState = document.getElementById("employeesEmptyState");
    var table = document.getElementById("employeesTable");

    if (!tbody) return;

    tbody.innerHTML = "";

    if (!employees || employees.length === 0) {
      if (table) table.classList.add("d-none");
      if (emptyState) emptyState.classList.remove("d-none");
      return;
    }

    if (table) table.classList.remove("d-none");
    if (emptyState) emptyState.classList.add("d-none");

    employees.forEach(function (emp) {
      var row = document.createElement("tr");
      var fullName = escapeHtml(emp.fullName);
      var email = escapeHtml(emp.email || "—");
      var phone = escapeHtml(emp.phone || "—");
      var branch = escapeHtml(emp.branchName || "—");
      var dept = escapeHtml(emp.departmentName || "—");
      var desig = escapeHtml(emp.designationName || "—");

      var statusBadge = emp.isActive
        ? '<span class="badge employee-status-active">Active</span>'
        : '<span class="badge employee-status-inactive">Inactive</span>';

      row.innerHTML =
        '<td><span class="badge employee-code-badge">' + escapeHtml(emp.employeeCode) + '</span></td>' +
        '<td><strong>' + fullName + '</strong>' + (emp.isSystemUser ? ' <i class="bi bi-shield-check text-primary" title="System User"></i>' : '') + '</td>' +
        '<td>' + email + '</td>' +
        '<td>' + phone + '</td>' +
        '<td>' + branch + '</td>' +
        '<td>' + dept + '</td>' +
        '<td>' + desig + '</td>' +
        '<td>' + statusBadge + '</td>' +
        '<td class="text-end">' +
          '<div class="employee-actions">' +
            '<button type="button" class="btn btn-sm btn-outline-info btn-view-employee" data-id="' + emp.employeeId + '" title="View"><i class="bi bi-eye"></i></button>' +
            '<button type="button" class="btn btn-sm btn-outline-primary btn-edit-employee" data-id="' + emp.employeeId + '" title="Edit"><i class="bi bi-pencil"></i></button>' +
            (emp.isSystemUser ? '<button type="button" class="btn btn-sm btn-outline-warning btn-remove-system-user" data-id="' + emp.employeeId + '" data-name="' + fullName + '" title="Remove System User"><i class="bi bi-person-dash"></i></button>' : '') +
            '<button type="button" class="btn btn-sm btn-outline-secondary btn-toggle-active" data-id="' + emp.employeeId + '" title="' + (emp.isActive ? "Deactivate" : "Activate") + '">' +
              (emp.isActive ? '<i class="bi bi-pause-circle"></i>' : '<i class="bi bi-play-circle"></i>') +
            '</button>' +
            '<button type="button" class="btn btn-sm btn-outline-danger btn-delete-employee" data-id="' + emp.employeeId + '" data-name="' + fullName + '" title="Delete"><i class="bi bi-trash"></i></button>' +
          '</div>' +
        '</td>';

      tbody.appendChild(row);
    });
  }

  function loadEmployees() {
    var tbody = document.getElementById("employeesTableBody");
    var emptyState = document.getElementById("employeesEmptyState");
    var table = document.getElementById("employeesTable");

    if (tbody) {
      tbody.innerHTML = '<tr><td colspan="9" class="text-center text-muted py-4">Loading employees...</td></tr>';
    }

    if (emptyState) emptyState.classList.add("d-none");
    if (table) table.classList.remove("d-none");

    fetch(buildListUrl(), {
      headers: { Accept: "application/json" }
    })
      .then(function (response) { return response.json(); })
      .then(function (result) {
        if (!result.success) {
          showPageAlert(result.message || "Unable to load employees.", "danger");
          return;
        }
        renderEmployees(result.data);
      })
      .catch(function () {
        showPageAlert("Unable to load employees.", "danger");
      });
  }

  function resetEmployeeForm() {
    var form = document.getElementById("employeeForm");
    if (!form) return;

    form.reset();
    form.classList.remove("was-validated");
    hideFormAlert();

    document.getElementById("EmployeeId").value = "";
    document.getElementById("IsActive").checked = true;
    document.getElementById("employeeModalLabel").textContent = "Add Employee";
    document.getElementById("employeeFormSubmit").textContent = "Save Employee";

    editingEmployeeId = null;

    // Reset phone input
    if (systemPhoneInputInstance) {
      systemPhoneInputInstance.setCountry("pk");
    }
  }

  function openCreateModal() {
    resetEmployeeForm();
    loadDropdowns();
    initSystemPhoneInput();
    employeeModal.show();
  }

  function populateForm(emp) {
    document.getElementById("EmployeeId").value = emp.employeeId || "";
    document.getElementById("EmployeeCode").value = emp.employeeCode || "";
    document.getElementById("FirstName").value = emp.firstName || "";
    document.getElementById("LastName").value = emp.lastName || "";
    document.getElementById("Gender").value = emp.gender || "";
    document.getElementById("DateOfBirth").value = emp.dateOfBirth ? emp.dateOfBirth.split('T')[0] : "";
    document.getElementById("CNICOrNationalId").value = emp.cnicOrNationalId || "";
    document.getElementById("PersonalEmail").value = emp.personalEmail || "";
    // Set phone field
    var phoneInput = document.getElementById("SystemPhoneNumberInput");
    if (phoneInput) {
      phoneInput.value = emp.personalPhone || "";
    }
    document.getElementById("BranchId").value = emp.branchId || "";
    document.getElementById("DepartmentId").value = emp.departmentId || "";
    document.getElementById("DesignationId").value = emp.designationId || "";
    document.getElementById("JoiningDate").value = emp.joiningDate ? emp.joiningDate.split('T')[0] : "";
    document.getElementById("IsActive").checked = !!emp.isActive;
  }

  function openEditModal(employeeId) {
    fetch(config.getUrl + "?id=" + encodeURIComponent(employeeId), {
      headers: { Accept: "application/json" }
    })
      .then(function (response) { return response.json(); })
      .then(function (result) {
        if (!result.success || !result.data) {
          showPageAlert(result.message || "Unable to load employee.", "danger");
          return;
        }

        resetEmployeeForm();
        editingEmployeeId = employeeId;
        loadDropdowns(function () {
          populateForm(result.data);
          document.getElementById("employeeModalLabel").textContent = "Edit Employee";
          document.getElementById("employeeFormSubmit").textContent = "Update Employee";
          employeeModal.show();
        });
      })
      .catch(function () {
        showPageAlert("Unable to load employee.", "danger");
      });
  }

  function collectFormData() {
    var form = document.getElementById("employeeForm");
    return new FormData(form);
  }

  function handleValidationErrors(errors) {
    if (!errors) {
      showFormAlert("Validation failed. Please review the form.");
      return;
    }

    var messages = [];
    Object.keys(errors).forEach(function (key) {
      errors[key].forEach(function (message) {
        messages.push(message);
      });
    });

    showFormAlert(messages.join(" "));
  }

  function submitEmployeeForm(event) {
    event.preventDefault();
    hideFormAlert();
    hidePageAlert();

    console.log("=== EMPLOYEE FORM SUBMIT ===");

    var form = document.getElementById("employeeForm");
    console.log("Form valid:", form.checkValidity());

    if (!form.checkValidity()) {
      form.classList.add("was-validated");
      return;
    }

    // Validate phone manually
    var phoneInput = document.getElementById("SystemPhoneNumberInput");
    console.log("Phone input found:", !!phoneInput);
    console.log("Phone input value:", phoneInput ? phoneInput.value : "N/A");
    console.log("systemPhoneInputInstance:", !!systemPhoneInputInstance);

    if (!phoneInput || !phoneInput.value.trim()) {
      console.log("ERROR: Employee phone input is empty");
      if (phoneInput) phoneInput.classList.add("is-invalid");
      showFormAlert("Phone number is required.");
      return;
    }
    phoneInput.classList.remove("is-invalid");

    // Sync phone fields before submission
    var phoneValue = "";

    if (systemPhoneInputInstance) {
      var countryData = systemPhoneInputInstance.getSelectedCountryData();
      document.getElementById("SystemPhoneDialCode").value = "+" + countryData.dialCode;

      // Try getNumber() first, fall back to raw input value
      phoneValue = systemPhoneInputInstance.getNumber();
      console.log("Employee getNumber() returned:", phoneValue);

      if (!phoneValue || phoneValue.trim() === "") {
        console.log("Employee getNumber() empty, using raw input value");
        phoneValue = phoneInput.value;
      }
    } else {
      // Fallback: use the raw input value
      document.getElementById("SystemPhoneDialCode").value = "+92";
      phoneValue = phoneInput.value;
    }

    document.getElementById("SystemPhoneNumber").value = phoneValue;

    console.log("SystemPhoneDialCode:", document.getElementById("SystemPhoneDialCode").value);
    console.log("SystemPhoneNumber:", document.getElementById("SystemPhoneNumber").value);

    var url = editingEmployeeId
      ? config.editUrl + "?id=" + encodeURIComponent(editingEmployeeId)
      : config.createUrl;

    var submitButton = document.getElementById("employeeFormSubmit");
    submitButton.disabled = true;

    fetch(url, {
      method: "POST",
      headers: { RequestVerificationToken: getAntiForgeryToken() },
      body: collectFormData()
    })
      .then(function (response) {
        return response.json().then(function (payload) {
          return { ok: response.ok, payload: payload };
        });
      })
      .then(function (result) {
        if (!result.ok || !result.payload.success) {
          if (result.payload.errors) {
            handleValidationErrors(result.payload.errors);
          } else {
            showFormAlert(result.payload.message || "Unable to save employee.");
          }
          return;
        }

        employeeModal.hide();
        showPageAlert(result.payload.message || "Employee saved successfully.", "success");
        loadEmployees();
      })
      .catch(function () {
        showFormAlert("Unable to save employee.");
      })
      .finally(function () {
        submitButton.disabled = false;
      });
  }

  function deleteEmployee(employeeId) {
    var formData = new FormData();
    formData.append("__RequestVerificationToken", getAntiForgeryToken());

    fetch(config.deleteUrl + "?id=" + encodeURIComponent(employeeId), {
      method: "POST",
      headers: { RequestVerificationToken: getAntiForgeryToken() },
      body: formData
    })
      .then(function (response) { return response.json(); })
      .then(function (result) {
        if (!result.success) {
          showPageAlert(result.message || "Unable to delete employee.", "danger");
          return;
        }

        deleteModal.hide();
        showPageAlert(result.message || "Employee deleted successfully.", "success");
        loadEmployees();
      })
      .catch(function () {
        showPageAlert("Unable to delete employee.", "danger");
      });
  }

  function toggleActive(employeeId) {
    var formData = new FormData();
    formData.append("__RequestVerificationToken", getAntiForgeryToken());

    fetch(config.toggleActiveUrl + "?id=" + encodeURIComponent(employeeId), {
      method: "POST",
      headers: { RequestVerificationToken: getAntiForgeryToken() },
      body: formData
    })
      .then(function (response) { return response.json(); })
      .then(function (result) {
        if (!result.success) {
          showPageAlert(result.message || "Unable to update employee status.", "danger");
          return;
        }

        showPageAlert(result.message || "Employee status updated.", "success");
        loadEmployees();
      })
      .catch(function () {
        showPageAlert("Unable to update employee status.", "danger");
      });
  }

  function openCreateSystemUserModal(employeeId, employeeName) {
    creatingSystemUserForEmployeeId = employeeId;
    document.getElementById("SystemUserEmployeeId").value = employeeId;
    document.getElementById("systemUserEmployeeName").textContent = employeeName;
    document.getElementById("createSystemUserForm").reset();
    document.getElementById("systemUserFormAlert").classList.add("d-none");

    // Load roles for the dropdown
    fetch(config.getRolesUrl, { headers: { Accept: "application/json" } })
      .then(function (response) { return response.json(); })
      .then(function (result) {
        if (!result.success) return;
        var select = document.getElementById("SystemUserRole");
        select.innerHTML = '<option value="">Select Role</option>';
        result.data.forEach(function (role) {
          select.innerHTML += '<option value="' + escapeHtml(role.name) + '">' + escapeHtml(role.name) + (role.description ? ' - ' + escapeHtml(role.description) : '') + '</option>';
        });
      });

    // Initialize phone input
    setTimeout(function() {
      var phoneField = document.getElementById("SystemUserPhone");
      if (phoneField && window.intlTelInput) {
        if (systemUserPhoneInputInstance) systemUserPhoneInputInstance.destroy();
        systemUserPhoneInputInstance = window.intlTelInput(phoneField, {
          initialCountry: "pk",
          preferredCountries: ["pk", "ae", "sa", "us", "gb"],
          separateDialCode: true,
          strictMode: true,
          utilsScript: "https://cdn.jsdelivr.net/npm/intl-tel-input@25.3.1/build/js/utils.js"
        });
      }
    }, 100);

    createSystemUserModal.show();
  }

  function submitCreateSystemUser(event) {
    event.preventDefault();
    var alertEl = document.getElementById("systemUserFormAlert");
    alertEl.classList.add("d-none");

    console.log("=== CREATE SYSTEM USER SUBMIT ===");

    var form = document.getElementById("createSystemUserForm");
    console.log("Form found:", !!form);
    console.log("Form valid:", form.checkValidity());

    if (!form.checkValidity()) {
      form.classList.add("was-validated");
      return;
    }

    // Check phone input
    var phoneInput = document.getElementById("SystemUserPhone");
    console.log("Phone input found:", !!phoneInput);
    console.log("Phone input value:", phoneInput ? phoneInput.value : "N/A");
    console.log("Phone input trimmed:", phoneInput ? phoneInput.value.trim() : "N/A");
    console.log("systemUserPhoneInputInstance:", !!systemUserPhoneInputInstance);
    console.log("intlTelInput available:", !!window.intlTelInput);

    if (!phoneInput || !phoneInput.value.trim()) {
      console.log("ERROR: Phone input is empty");
      if (phoneInput) phoneInput.classList.add("is-invalid");
      alertEl.textContent = "Phone number is required.";
      alertEl.classList.remove("d-none");
      return;
    }
    phoneInput.classList.remove("is-invalid");

    // Validate password match
    var password = document.getElementById("SystemUserPassword").value;
    var confirmPassword = document.getElementById("SystemUserConfirmPassword").value;
    console.log("Password length:", password.length);
    console.log("Passwords match:", password === confirmPassword);

    if (password !== confirmPassword) {
      alertEl.textContent = "Password and confirmation password do not match.";
      alertEl.classList.remove("d-none");
      return;
    }

    // Sync phone fields
    console.log("Syncing phone fields...");
    var phoneValue = "";

    if (systemUserPhoneInputInstance) {
      console.log("Using intl-tel-input instance");
      var countryData = systemUserPhoneInputInstance.getSelectedCountryData();
      console.log("Country data:", countryData);
      document.getElementById("SystemUserPhoneDialCode").value = "+" + countryData.dialCode;

      // Try getNumber() first, fall back to raw input value
      phoneValue = systemUserPhoneInputInstance.getNumber();
      console.log("getNumber() returned:", phoneValue);

      if (!phoneValue || phoneValue.trim() === "") {
        console.log("getNumber() empty, using raw input value");
        phoneValue = phoneInput.value;
      }
    } else {
      console.log("Using fallback phone sync");
      document.getElementById("SystemUserPhoneDialCode").value = "+92";
      phoneValue = phoneInput.value;
    }

    document.getElementById("SystemUserPhoneNumber").value = phoneValue;

    console.log("SystemUserPhoneDialCode:", document.getElementById("SystemUserPhoneDialCode").value);
    console.log("SystemUserPhoneNumber:", document.getElementById("SystemUserPhoneNumber").value);

    var submitButton = document.getElementById("createSystemUserSubmit");
    submitButton.disabled = true;

    var formData = new FormData(form);

    // Log all FormData entries
    console.log("=== FORM DATA ===");
    for (var pair of formData.entries()) {
      console.log(pair[0] + ": " + pair[1]);
    }

    var token = getAntiForgeryToken("createSystemUserForm");
    console.log("Anti-forgery token:", token ? "present (" + token.length + " chars)" : "MISSING");

    var url = config.createSystemUserUrl + "?employeeId=" + encodeURIComponent(creatingSystemUserForEmployeeId);
    console.log("Request URL:", url);

    fetch(url, {
      method: "POST",
      headers: { "RequestVerificationToken": token },
      body: formData
    })
      .then(function (response) {
        console.log("Response status:", response.status);
        console.log("Response headers:", response.headers.get("content-type"));
        return response.text().then(function (text) {
          console.log("Response body:", text);
          try {
            return JSON.parse(text);
          } catch (e) {
            console.log("Failed to parse JSON:", e);
            return { success: false, message: "Invalid server response: " + text.substring(0, 200) };
          }
        });
      })
      .then(function (result) {
        console.log("=== SERVER RESPONSE ===");
        console.log("Success:", result.success);
        console.log("Message:", result.message);
        console.log("Errors:", JSON.stringify(result.errors));

        if (!result.success) {
          // Show specific validation errors if available
          if (result.errors) {
            var messages = [];
            Object.keys(result.errors).forEach(function (key) {
              result.errors[key].forEach(function (msg) {
                messages.push(msg);
              });
            });
            alertEl.textContent = messages.join(" ");
          } else {
            alertEl.textContent = result.message || "Failed to create system user.";
          }
          alertEl.classList.remove("d-none");
          return;
        }

        createSystemUserModal.hide();
        showPageAlert(result.message || "System user account created successfully.", "success");
        loadEmployees();
      })
      .catch(function () {
        alertEl.textContent = "An error occurred. Please try again.";
        alertEl.classList.remove("d-none");
      })
      .finally(function () {
        submitButton.disabled = false;
      });
  }

  function viewEmployeeDetails(employeeId) {
    viewingEmployeeId = employeeId;
    var content = document.getElementById("employeeDetailsContent");
    content.innerHTML = '<div class="text-center text-muted py-4">Loading...</div>';
    detailsModal.show();

    // Load employee basic info (reuse Get endpoint)
    fetch(config.getUrl + "?id=" + encodeURIComponent(employeeId), {
      headers: { Accept: "application/json" }
    })
      .then(function (response) { return response.json(); })
      .then(function (result) {
        if (!result.success || !result.data) {
          content.innerHTML = '<div class="text-center text-danger py-4">Unable to load employee details.</div>';
          return;
        }

        var emp = result.data;
        var html = '<div class="row g-4">' +
          '<div class="col-md-6">' +
            '<h5 class="h6 mb-3"><i class="bi bi-person-badge me-2"></i>Basic Information</h5>' +
            '<table class="table table-sm">' +
              '<tr><td class="text-muted" style="width:40%">Employee Code</td><td><strong>' + escapeHtml(emp.employeeCode) + '</strong></td></tr>' +
              '<tr><td class="text-muted">Full Name</td><td>' + escapeHtml(emp.firstName + " " + (emp.lastName || "")) + '</td></tr>' +
              '<tr><td class="text-muted">Gender</td><td>' + escapeHtml(emp.gender) + '</td></tr>' +
              '<tr><td class="text-muted">Date of Birth</td><td>' + (emp.dateOfBirth ? new Date(emp.dateOfBirth).toLocaleDateString() : "—") + '</td></tr>' +
              '<tr><td class="text-muted">CNIC/National ID</td><td>' + escapeHtml(emp.cnicOrNationalId || "—") + '</td></tr>' +
              '<tr><td class="text-muted">Personal Email</td><td>' + escapeHtml(emp.personalEmail || "—") + '</td></tr>' +
              '<tr><td class="text-muted">Personal Phone</td><td>' + escapeHtml(emp.personalPhone || "—") + '</td></tr>' +
            '</table>' +
          '</div>' +
          '<div class="col-md-6">' +
            '<h5 class="h6 mb-3"><i class="bi bi-building me-2"></i>Organization</h5>' +
            '<table class="table table-sm">' +
              '<tr><td class="text-muted" style="width:40%">Branch</td><td>' + (emp.branchId ? "Loaded" : "—") + '</td></tr>' +
              '<tr><td class="text-muted">Department</td><td>' + (emp.departmentId ? "Loaded" : "—") + '</td></tr>' +
              '<tr><td class="text-muted">Designation</td><td>' + (emp.designationId ? "Loaded" : "—") + '</td></tr>' +
              '<tr><td class="text-muted">Joining Date</td><td>' + (emp.joiningDate ? new Date(emp.joiningDate).toLocaleDateString() : "—") + '</td></tr>' +
              '<tr><td class="text-muted">System User</td><td>' +
                (emp.isSystemUser
                  ? '<span class="badge bg-primary">Yes</span>'
                  : '<span class="badge bg-secondary">No</span> <button type="button" class="btn btn-sm btn-outline-primary ms-2" onclick="window.employeeopenCreateSystemUserModal(' + emp.employeeId + ', \'' + escapeHtml(emp.firstName + ' ' + (emp.lastName || '')) + '\')"><i class="bi bi-plus-lg me-1"></i>Create Account</button>')
              + '</td></tr>' +
              '<tr><td class="text-muted">Status</td><td>' + (emp.isActive ? '<span class="badge bg-success">Active</span>' : '<span class="badge bg-danger">Inactive</span>') + '</td></tr>' +
            '</table>' +
          '</div>' +
        '</div>';

        content.innerHTML = html;

        // Load documents
        loadEmployeeDocuments(employeeId);
      })
      .catch(function () {
        content.innerHTML = '<div class="text-center text-danger py-4">Unable to load employee details.</div>';
      });
  }

  function loadEmployeeDocuments(employeeId) {
    fetch(config.documentListUrl + "?employeeId=" + encodeURIComponent(employeeId), {
      headers: { Accept: "application/json" }
    })
      .then(function (response) { return response.json(); })
      .then(function (result) {
        if (!result.success) return;

        var docs = result.data || [];
        var content = document.getElementById("employeeDetailsContent");
        var docHtml = '<hr class="my-4">' +
          '<h5 class="h6 mb-3"><i class="bi bi-folder2 me-2"></i>Documents</h5>' +
          '<button type="button" class="btn btn-sm btn-primary mb-3" id="btnAddDocument">' +
            '<i class="bi bi-plus-lg me-1"></i>Upload Document' +
          '</button>' +
          '<div id="documentUploadForm" class="d-none mb-3 p-3 border rounded bg-light">' +
            '<form id="uploadForm" enctype="multipart/form-data">' +
              '<div class="row g-2">' +
                '<div class="col-md-4">' +
                  '<select id="docTypeId" class="form-select form-select-sm" required>' +
                    '<option value="">Select Type</option>' +
                  '</select>' +
                '</div>' +
                '<div class="col-md-4">' +
                  '<input type="file" id="docFile" class="form-control form-control-sm" accept=".pdf,.jpg,.jpeg,.png,.doc,.docx,.xls,.xlsx" required />' +
                '</div>' +
                '<div class="col-md-2">' +
                  '<button type="submit" class="btn btn-sm btn-success w-100">Upload</button>' +
                '</div>' +
              '</div>' +
            '</form>' +
          '</div>';

        if (docs.length === 0) {
          docHtml += '<p class="text-muted">No documents uploaded yet.</p>';
        } else {
          docHtml += '<div class="table-responsive">' +
            '<table class="table table-sm">' +
              '<thead><tr><th>Type</th><th>File</th><th>Uploaded</th><th>Status</th><th>Actions</th></tr></thead>' +
              '<tbody>';

          docs.forEach(function (doc) {
            var fileName = escapeHtml(doc.fileName || "Unknown");
            var docType = escapeHtml(doc.documentTypeName);
            var uploadDate = new Date(doc.uploadedAt).toLocaleDateString();
            var verifiedBadge = doc.isVerified
              ? '<span class="badge bg-success">Verified</span>'
              : '<span class="badge bg-warning">Pending</span>';

            docHtml += '<tr>' +
              '<td>' + docType + '</td>' +
              '<td><a href="' + escapeHtml(doc.fileUrl) + '" target="_blank">' + fileName + '</a></td>' +
              '<td>' + uploadDate + '</td>' +
              '<td>' + verifiedBadge + '</td>' +
              '<td>' +
                '<button type="button" class="btn btn-sm btn-outline-secondary btn-toggle-verified" data-id="' + doc.employeeDocumentId + '" title="Toggle Verification">' +
                  '<i class="bi bi-check-circle"></i>' +
                '</button>' +
                '<button type="button" class="btn btn-sm btn-outline-danger btn-delete-doc" data-id="' + doc.employeeDocumentId + '" title="Delete">' +
                  '<i class="bi bi-trash"></i>' +
                '</button>' +
              '</td>' +
            '</tr>';
          });

          docHtml += '</tbody></table></div>';
        }

        content.innerHTML += docHtml;

        // Bind document events
        document.getElementById("btnAddDocument")?.addEventListener("click", function () {
          document.getElementById("documentUploadForm").classList.toggle("d-none");
          loadDocumentTypes();
        });

        document.getElementById("uploadForm")?.addEventListener("submit", function (e) {
          e.preventDefault();
          uploadDocument(employeeId);
        });

        document.querySelectorAll(".btn-toggle-verified").forEach(function (btn) {
          btn.addEventListener("click", function () {
            toggleDocumentVerified(this.getAttribute("data-id"));
          });
        });

        document.querySelectorAll(".btn-delete-doc").forEach(function (btn) {
          btn.addEventListener("click", function () {
            if (confirm("Delete this document?")) {
              deleteDocument(this.getAttribute("data-id"));
            }
          });
        });
      });
  }

  function loadDocumentTypes() {
    fetch(config.documentTypesUrl, {
      headers: { Accept: "application/json" }
    })
      .then(function (response) { return response.json(); })
      .then(function (result) {
        if (!result.success) return;

        var select = document.getElementById("docTypeId");
        if (!select) return;

        select.innerHTML = '<option value="">Select Type</option>';
        result.data.forEach(function (type) {
          select.innerHTML += '<option value="' + type.employeeDocumentTypeId + '">' + escapeHtml(type.name) + '</option>';
        });
      });
  }

  function uploadDocument(employeeId) {
    var formData = new FormData();
    var docTypeId = document.getElementById("docTypeId").value;
    var fileInput = document.getElementById("docFile");

    if (!docTypeId || !fileInput.files[0]) {
      alert("Please select document type and file.");
      return;
    }

    formData.append("EmployeeDocumentTypeId", docTypeId);
    formData.append("File", fileInput.files[0]);
    formData.append("__RequestVerificationToken", getAntiForgeryToken());

    fetch(config.documentUploadUrl + "?employeeId=" + encodeURIComponent(employeeId), {
      method: "POST",
      headers: { RequestVerificationToken: getAntiForgeryToken() },
      body: formData
    })
      .then(function (response) { return response.json(); })
      .then(function (result) {
        if (!result.success) {
          alert(result.message || "Upload failed.");
          return;
        }

        alert("Document uploaded successfully.");
        document.getElementById("documentUploadForm").classList.add("d-none");
        loadEmployeeDocuments(employeeId);
      })
      .catch(function () {
        alert("Upload failed.");
      });
  }

  function deleteDocument(docId) {
    var formData = new FormData();
    formData.append("__RequestVerificationToken", getAntiForgeryToken());

    fetch(config.documentDeleteUrl + "?id=" + encodeURIComponent(docId), {
      method: "POST",
      headers: { RequestVerificationToken: getAntiForgeryToken() },
      body: formData
    })
      .then(function (response) { return response.json(); })
      .then(function (result) {
        if (!result.success) {
          alert(result.message || "Delete failed.");
          return;
        }

        alert(result.message || "Document deleted.");
        if (viewingEmployeeId) loadEmployeeDocuments(viewingEmployeeId);
      })
      .catch(function () {
        alert("Delete failed.");
      });
  }

  function toggleDocumentVerified(docId) {
    var formData = new FormData();
    formData.append("__RequestVerificationToken", getAntiForgeryToken());

    fetch(config.documentToggleVerifiedUrl + "?id=" + encodeURIComponent(docId), {
      method: "POST",
      headers: { RequestVerificationToken: getAntiForgeryToken() },
      body: formData
    })
      .then(function (response) { return response.json(); })
      .then(function (result) {
        if (!result.success) {
          alert(result.message || "Update failed.");
          return;
        }

        alert(result.message);
        if (viewingEmployeeId) loadEmployeeDocuments(viewingEmployeeId);
      })
      .catch(function () {
        alert("Update failed.");
      });
  }

  function loadDropdowns(callback) {
    var promises = [];

    // Load Branches
    promises.push(
      fetch('/Branch/List', { headers: { Accept: "application/json" } })
        .then(function (response) { return response.json(); })
        .then(function (result) {
          if (!result.success) return;
          var select = document.getElementById("BranchId");
          var filterSelect = document.getElementById("filterBranch");
          if (select) {
            select.innerHTML = '<option value="">Select Branch</option>';
            result.data.forEach(function (item) {
              select.innerHTML += '<option value="' + item.branchId + '">' + escapeHtml(item.branchName) + '</option>';
            });
          }
          if (filterSelect) {
            filterSelect.innerHTML = '<option value="">All Branches</option>';
            result.data.forEach(function (item) {
              filterSelect.innerHTML += '<option value="' + item.branchId + '">' + escapeHtml(item.branchName) + '</option>';
            });
          }
        })
    );

    // Load Departments
    promises.push(
      fetch('/Department/List', { headers: { Accept: "application/json" } })
        .then(function (response) { return response.json(); })
        .then(function (result) {
          if (!result.success) return;
          var select = document.getElementById("DepartmentId");
          var filterSelect = document.getElementById("filterDepartment");
          if (select) {
            select.innerHTML = '<option value="">Select Department</option>';
              result.data.forEach(function (item) {
                  select.innerHTML += '<option value="' + item.departmentId + '">' + escapeHtml(item.name) + '</option>';
            });
          }
          if (filterSelect) {
            filterSelect.innerHTML = '<option value="">All Departments</option>';
            result.data.forEach(function (item) {
              filterSelect.innerHTML += '<option value="' + item.departmentId + '">' + escapeHtml(item.name) + '</option>';
            });
          }
        })
    );

    // Load Designations
      // Load Designations
      promises.push(
          fetch('/Designation/List', { headers: { Accept: "application/json" } })
              .then(function (response) {
                  return response.json();
              })
              .then(function (result) {

                  console.log("Full Result:", result);
                  console.log("Designation Data:", result.data);

                  if (!result.success) return;

                  var select = document.getElementById("DesignationId");
                  var filterSelect = document.getElementById("filterDesignation");

                  if (select) {
                      select.innerHTML = '<option value="">Select Designation</option>';

                      result.data.forEach(function (item) {

                          console.log("Designation Item:", item);
                          console.log("DesignationId:", item.designationId);
                          console.log("DesignationName:", item.name);

                          select.innerHTML +=
                              '<option value="' +
                              item.designationId +
                              '">' +
                              escapeHtml(item.name) +
                              '</option>';
                      });
                  }

                  if (filterSelect) {
                      filterSelect.innerHTML = '<option value="">All Designations</option>';

                      result.data.forEach(function (item) {
                          filterSelect.innerHTML +=
                              '<option value="' +
                              item.designationId +
                              '">' +
                              escapeHtml(item.name) +
                              '</option>';
                      });
                  }
              })
      );

    // Load Roles
    promises.push(
      fetch(config.getRolesUrl, { headers: { Accept: "application/json" } })
        .then(function (response) { return response.json(); })
        .then(function (result) {
          if (!result.success) return;
          var select = document.getElementById("Role");
          if (select) {
            select.innerHTML = '<option value="">Select Role</option>';
            result.data.forEach(function (role) {
              select.innerHTML += '<option value="' + escapeHtml(role.name) + '">' + escapeHtml(role.name) + (role.description ? ' - ' + escapeHtml(role.description) : '') + '</option>';
            });
          }
        })
    );

    Promise.all(promises).then(function () {
      if (callback) callback();
    });
  }

  function bindEvents() {
    var addButton = document.getElementById("btnAddEmployee");
    var form = document.getElementById("employeeForm");
    var statusFilter = document.getElementById("filterStatus");
    var branchFilter = document.getElementById("filterBranch");
    var deptFilter = document.getElementById("filterDepartment");
    var desigFilter = document.getElementById("filterDesignation");
    var searchInput = document.getElementById("searchEmployees");
    var confirmDeleteButton = document.getElementById("confirmDeleteEmployee");
    var tableBody = document.getElementById("employeesTableBody");
    var employeeModalElement = document.getElementById("employeeModal");
    var detailsModalElement = document.getElementById("employeeDetailsModal");
    var deleteModalElement = document.getElementById("deleteEmployeeModal");
    var createSystemUserModalElement = document.getElementById("createSystemUserModal");
    var createSystemUserForm = document.getElementById("createSystemUserForm");

    if (employeeModalElement) {
      employeeModal = new bootstrap.Modal(employeeModalElement);
      employeeModalElement.addEventListener("hidden.bs.modal", resetEmployeeForm);
    }

    if (detailsModalElement) {
      detailsModal = new bootstrap.Modal(detailsModalElement);
    }

    if (deleteModalElement) {
      deleteModal = new bootstrap.Modal(deleteModalElement);
    }

    if (createSystemUserModalElement) {
      createSystemUserModal = new bootstrap.Modal(createSystemUserModalElement);
    }

    if (addButton) {
      addButton.addEventListener("click", openCreateModal);
    }

    if (form) {
      form.addEventListener("submit", submitEmployeeForm);
    }

    if (createSystemUserForm) {
      createSystemUserForm.addEventListener("submit", submitCreateSystemUser);
    }

    if (statusFilter) statusFilter.addEventListener("change", loadEmployees);
    if (branchFilter) branchFilter.addEventListener("change", loadEmployees);
    if (deptFilter) deptFilter.addEventListener("change", loadEmployees);
    if (desigFilter) desigFilter.addEventListener("change", loadEmployees);

    if (searchInput) {
      searchInput.addEventListener("input", function () {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(loadEmployees, 300);
      });
    }

    if (confirmDeleteButton) {
      confirmDeleteButton.addEventListener("click", function () {
        if (pendingDeleteId) {
          deleteEmployee(pendingDeleteId);
        }
      });
    }

    if (tableBody) {
      tableBody.addEventListener("click", function (event) {
        var viewButton = event.target.closest(".btn-view-employee");
        var editButton = event.target.closest(".btn-edit-employee");
        var deleteButton = event.target.closest(".btn-delete-employee");
        var toggleButton = event.target.closest(".btn-toggle-active");
        var removeSystemUserButton = event.target.closest(".btn-remove-system-user");

        if (viewButton) {
          viewEmployeeDetails(viewButton.getAttribute("data-id"));
          return;
        }

        if (editButton) {
          openEditModal(editButton.getAttribute("data-id"));
          return;
        }

        if (removeSystemUserButton) {
          removeSystemUser(removeSystemUserButton.getAttribute("data-id"), removeSystemUserButton.getAttribute("data-name"));
          return;
        }

        if (deleteButton) {
          pendingDeleteId = deleteButton.getAttribute("data-id");
          document.getElementById("deleteEmployeeName").textContent =
            deleteButton.getAttribute("data-name") || "this employee";
          deleteModal.show();
          return;
        }

        if (toggleButton) {
          toggleActive(toggleButton.getAttribute("data-id"));
          return;
        }
      });
    }
  }

  async function removeSystemUser(employeeId, employeeName) {
    const result = await Swal.fire({
      title: 'Remove System User',
      text: 'This will remove the system user account for ' + employeeName + '. The employee record will remain. Are you sure?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d45656',
      cancelButtonColor: '#5a5a5c',
      confirmButtonText: 'Remove'
    });

    if (result.isConfirmed) {
      try {
        const formData = new FormData();
        formData.append("__RequestVerificationToken", getAntiForgeryToken());

        const response = await fetch('/Employee/RemoveSystemUser?id=' + employeeId, {
          method: 'POST',
          headers: { "RequestVerificationToken": getAntiForgeryToken() },
          body: formData
        });

        const data = await response.json();

        if (data.success) {
          loadEmployees();
          showPageAlert(data.message, "success");
        } else {
          showPageAlert(data.message || "Failed to remove system user.", "danger");
        }
      } catch (error) {
        console.error('Error removing system user:', error);
        showPageAlert("Failed to remove system user.", "danger");
      }
    }
  }

  onReady(function () {
    bindEvents();
    loadDropdowns();
    loadEmployees();

    // Expose functions to window for inline onclick handlers
    window.employeeopenCreateSystemUserModal = openCreateSystemUserModal;
  });
})();
