"use strict";

(function () {
  var config = window.departmentConfig || {};
  var departmentModal;
  var deleteModal;
  var editingDepartmentId = null;
  var pendingDeleteId = null;
  var searchTimer;

  function onReady(callback) {
    if (document.readyState === "loading") {
      document.addEventListener("DOMContentLoaded", callback);
      return;
    }

    callback();
  }

  function getAntiForgeryToken() {
    var tokenInput = document.querySelector("#departmentForm input[name='__RequestVerificationToken']");
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
    var alert = document.getElementById("departmentAlert");
    if (!alert) {
      return;
    }

    alert.className = "alert alert-" + (type || "success");
    alert.textContent = message;
    alert.classList.remove("d-none");
  }

  function hidePageAlert() {
    var alert = document.getElementById("departmentAlert");
    if (alert) {
      alert.classList.add("d-none");
    }
  }

  function showFormAlert(message) {
    var alert = document.getElementById("departmentFormAlert");
    if (!alert) {
      return;
    }

    alert.textContent = message;
    alert.classList.remove("d-none");
  }

  function hideFormAlert() {
    var alert = document.getElementById("departmentFormAlert");
    if (alert) {
      alert.classList.add("d-none");
      alert.textContent = "";
    }
  }

  function getFilterValues() {
    var statusFilter = document.getElementById("filterDepartmentStatus");
    var searchInput = document.getElementById("searchDepartments");

    return {
      isActive: statusFilter ? statusFilter.value : "",
      search: searchInput ? searchInput.value.trim() : ""
    };
  }

  function buildListUrl() {
    var filters = getFilterValues();
    var params = new URLSearchParams();

    if (filters.isActive !== "") {
      params.set("isActive", filters.isActive);
    }

    if (filters.search) {
      params.set("search", filters.search);
    }

    var query = params.toString();
    return query ? config.listUrl + "?" + query : config.listUrl;
  }

  function renderDepartments(departments) {
    var tbody = document.getElementById("departmentsTableBody");
    var emptyState = document.getElementById("departmentsEmptyState");
    var table = document.getElementById("departmentsTable");

    if (!tbody) {
      return;
    }

    tbody.innerHTML = "";

    if (!departments || departments.length === 0) {
      if (table) {
        table.classList.add("d-none");
      }

      if (emptyState) {
        emptyState.classList.remove("d-none");
      }

      return;
    }

    if (table) {
      table.classList.remove("d-none");
    }

    if (emptyState) {
      emptyState.classList.add("d-none");
    }

    departments.forEach(function (department) {
      var row = document.createElement("tr");
      row.innerHTML =
        "<td><span class=\"badge department-code-badge\">" + escapeHtml(department.code) + "</span></td>" +
        "<td>" + escapeHtml(department.name) + "</td>" +
        "<td><span class=\"department-description\" title=\"" + escapeHtml(department.description || "") + "\">" + escapeHtml(department.description || "—") + "</span></td>" +
        "<td>" + escapeHtml(String(department.branchLinkCount)) + "</td>" +
        "<td>" + escapeHtml(String(department.employeeCount)) + "</td>" +
        "<td>" +
          (department.isActive
            ? "<span class=\"badge department-status-active\">Active</span>"
            : "<span class=\"badge department-status-inactive\">Inactive</span>") +
        "</td>" +
        "<td class=\"text-end\">" +
          "<div class=\"department-actions\">" +
            "<button type=\"button\" class=\"btn btn-sm btn-outline-secondary btn-toggle-active\" data-id=\"" + department.departmentId + "\">" +
              (department.isActive ? "Deactivate" : "Activate") +
            "</button>" +
            "<button type=\"button\" class=\"btn btn-sm btn-outline-primary btn-edit-department\" data-id=\"" + department.departmentId + "\">Edit</button>" +
            "<button type=\"button\" class=\"btn btn-sm btn-outline-danger btn-delete-department\" data-id=\"" + department.departmentId + "\" data-name=\"" + escapeHtml(department.name) + "\">Delete</button>" +
          "</div>" +
        "</td>";

      tbody.appendChild(row);
    });
  }

  function loadDepartments() {
    var tbody = document.getElementById("departmentsTableBody");
    var emptyState = document.getElementById("departmentsEmptyState");
    var table = document.getElementById("departmentsTable");

    if (tbody) {
      tbody.innerHTML = "<tr><td colspan=\"7\" class=\"text-center text-muted py-4\">Loading departments...</td></tr>";
    }

    if (emptyState) {
      emptyState.classList.add("d-none");
    }

    if (table) {
      table.classList.remove("d-none");
    }

    fetch(buildListUrl(), {
      headers: {
        Accept: "application/json"
      }
    })
      .then(function (response) {
        return response.json();
      })
      .then(function (result) {
        if (!result.success) {
          showPageAlert(result.message || "Unable to load departments.", "danger");
          return;
        }

        renderDepartments(result.data);
      })
      .catch(function () {
        showPageAlert("Unable to load departments.", "danger");
      });
  }

  function resetDepartmentForm() {
    var form = document.getElementById("departmentForm");
    if (!form) {
      return;
    }

    form.reset();
    form.classList.remove("was-validated");
    hideFormAlert();

    document.getElementById("DepartmentId").value = "";
    document.getElementById("IsActive").checked = true;
    document.getElementById("departmentModalLabel").textContent = "Add Department";
    document.getElementById("departmentFormSubmit").textContent = "Save Department";

    editingDepartmentId = null;
  }

  function openCreateModal() {
    resetDepartmentForm();
    departmentModal.show();
  }

  function populateForm(department) {
    document.getElementById("DepartmentId").value = department.departmentId || "";
    document.getElementById("Code").value = department.code || "";
    document.getElementById("Name").value = department.name || "";
    document.getElementById("Description").value = department.description || "";
    document.getElementById("IsActive").checked = !!department.isActive;
  }

  function openEditModal(departmentId) {
    fetch(config.getUrl + "?id=" + encodeURIComponent(departmentId), {
      headers: {
        Accept: "application/json"
      }
    })
      .then(function (response) {
        return response.json();
      })
      .then(function (result) {
        if (!result.success || !result.data) {
          showPageAlert(result.message || "Unable to load department.", "danger");
          return;
        }

        resetDepartmentForm();
        editingDepartmentId = departmentId;
        populateForm(result.data);

        document.getElementById("departmentModalLabel").textContent = "Edit Department";
        document.getElementById("departmentFormSubmit").textContent = "Update Department";
        departmentModal.show();
      })
      .catch(function () {
        showPageAlert("Unable to load department.", "danger");
      });
  }

  function collectFormData() {
    var form = document.getElementById("departmentForm");
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

  function submitDepartmentForm(event) {
    event.preventDefault();
    hideFormAlert();
    hidePageAlert();

    var form = document.getElementById("departmentForm");
    if (!form.checkValidity()) {
      form.classList.add("was-validated");
      return;
    }

    var url = editingDepartmentId
      ? config.editUrl + "?id=" + encodeURIComponent(editingDepartmentId)
      : config.createUrl;

    var submitButton = document.getElementById("departmentFormSubmit");
    submitButton.disabled = true;

    fetch(url, {
      method: "POST",
      headers: {
        RequestVerificationToken: getAntiForgeryToken()
      },
      body: collectFormData()
    })
      .then(function (response) {
        return response.json().then(function (payload) {
          return {
            ok: response.ok,
            payload: payload
          };
        });
      })
      .then(function (result) {
        if (!result.ok || !result.payload.success) {
          if (result.payload.errors) {
            handleValidationErrors(result.payload.errors);
          } else {
            showFormAlert(result.payload.message || "Unable to save department.");
          }
          return;
        }

        departmentModal.hide();
        showPageAlert(result.payload.message || "Department saved successfully.", "success");
        loadDepartments();
      })
      .catch(function () {
        showFormAlert("Unable to save department.");
      })
      .finally(function () {
        submitButton.disabled = false;
      });
  }

  function deleteDepartment(departmentId) {
    var formData = new FormData();
    formData.append("__RequestVerificationToken", getAntiForgeryToken());

    fetch(config.deleteUrl + "?id=" + encodeURIComponent(departmentId), {
      method: "POST",
      headers: {
        RequestVerificationToken: getAntiForgeryToken()
      },
      body: formData
    })
      .then(function (response) {
        return response.json();
      })
      .then(function (result) {
        if (!result.success) {
          showPageAlert(result.message || "Unable to delete department.", "danger");
          return;
        }

        deleteModal.hide();
        showPageAlert(result.message || "Department deleted successfully.", "success");
        loadDepartments();
      })
      .catch(function () {
        showPageAlert("Unable to delete department.", "danger");
      });
  }

  function toggleActive(departmentId) {
    var formData = new FormData();
    formData.append("__RequestVerificationToken", getAntiForgeryToken());

    fetch(config.toggleActiveUrl + "?id=" + encodeURIComponent(departmentId), {
      method: "POST",
      headers: {
        RequestVerificationToken: getAntiForgeryToken()
      },
      body: formData
    })
      .then(function (response) {
        return response.json();
      })
      .then(function (result) {
        if (!result.success) {
          showPageAlert(result.message || "Unable to update department status.", "danger");
          return;
        }

        showPageAlert(result.message || "Department status updated.", "success");
        loadDepartments();
      })
      .catch(function () {
        showPageAlert("Unable to update department status.", "danger");
      });
  }

  function bindEvents() {
    var addButton = document.getElementById("btnAddDepartment");
    var form = document.getElementById("departmentForm");
    var statusFilter = document.getElementById("filterDepartmentStatus");
    var searchInput = document.getElementById("searchDepartments");
    var confirmDeleteButton = document.getElementById("confirmDeleteDepartment");
    var tableBody = document.getElementById("departmentsTableBody");
    var departmentModalElement = document.getElementById("departmentModal");
    var deleteModalElement = document.getElementById("deleteDepartmentModal");
    var codeInput = document.getElementById("Code");

    if (departmentModalElement) {
      departmentModal = new bootstrap.Modal(departmentModalElement);
      departmentModalElement.addEventListener("hidden.bs.modal", resetDepartmentForm);
    }

    if (deleteModalElement) {
      deleteModal = new bootstrap.Modal(deleteModalElement);
    }

    if (addButton) {
      addButton.addEventListener("click", openCreateModal);
    }

    if (form) {
      form.addEventListener("submit", submitDepartmentForm);
    }

    if (codeInput) {
      codeInput.addEventListener("input", function () {
        codeInput.value = codeInput.value.toUpperCase();
      });
    }

    if (statusFilter) {
      statusFilter.addEventListener("change", loadDepartments);
    }

    if (searchInput) {
      searchInput.addEventListener("input", function () {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(loadDepartments, 300);
      });
    }

    if (confirmDeleteButton) {
      confirmDeleteButton.addEventListener("click", function () {
        if (pendingDeleteId) {
          deleteDepartment(pendingDeleteId);
        }
      });
    }

    if (tableBody) {
      tableBody.addEventListener("click", function (event) {
        var editButton = event.target.closest(".btn-edit-department");
        var deleteButton = event.target.closest(".btn-delete-department");
        var toggleButton = event.target.closest(".btn-toggle-active");

        if (editButton) {
          openEditModal(editButton.getAttribute("data-id"));
          return;
        }

        if (deleteButton) {
          pendingDeleteId = deleteButton.getAttribute("data-id");
          document.getElementById("deleteDepartmentName").textContent =
            deleteButton.getAttribute("data-name") || "this department";
          deleteModal.show();
          return;
        }

        if (toggleButton) {
          toggleActive(toggleButton.getAttribute("data-id"));
        }
      });
    }
  }

  onReady(function () {
    bindEvents();
    loadDepartments();
  });
})();
