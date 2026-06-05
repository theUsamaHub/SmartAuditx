"use strict";

(function () {
  var config = window.designationConfig || {};
  var designationModal;
  var deleteModal;
  var editingDesignationId = null;
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
    var tokenInput = document.querySelector("#designationForm input[name='__RequestVerificationToken']");
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
    var alert = document.getElementById("designationAlert");
    if (!alert) {
      return;
    }

    alert.className = "alert alert-" + (type || "success");
    alert.textContent = message;
    alert.classList.remove("d-none");
  }

  function hidePageAlert() {
    var alert = document.getElementById("designationAlert");
    if (alert) {
      alert.classList.add("d-none");
    }
  }

  function showFormAlert(message) {
    var alert = document.getElementById("designationFormAlert");
    if (!alert) {
      return;
    }

    alert.textContent = message;
    alert.classList.remove("d-none");
  }

  function hideFormAlert() {
    var alert = document.getElementById("designationFormAlert");
    if (alert) {
      alert.classList.add("d-none");
      alert.textContent = "";
    }
  }

  function getFilterValues() {
    var statusFilter = document.getElementById("filterDesignationStatus");
    var searchInput = document.getElementById("searchDesignations");

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

  function renderDesignations(designations) {
    var tbody = document.getElementById("designationsTableBody");
    var emptyState = document.getElementById("designationsEmptyState");
    var table = document.getElementById("designationsTable");

    if (!tbody) {
      return;
    }

    tbody.innerHTML = "";

    if (!designations || designations.length === 0) {
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

    designations.forEach(function (designation) {
      var row = document.createElement("tr");
      row.innerHTML =
        "<td><span class=\"badge designation-code-badge\">" + escapeHtml(designation.code) + "</span></td>" +
        "<td>" + escapeHtml(designation.name) + "</td>" +
        "<td><span class=\"designation-description\" title=\"" + escapeHtml(designation.description || "") + "\">" + escapeHtml(designation.description || "—") + "</span></td>" +
        "<td>" + escapeHtml(String(designation.employeeCount)) + "</td>" +
        "<td>" +
          (designation.isActive
            ? "<span class=\"badge designation-status-active\">Active</span>"
            : "<span class=\"badge designation-status-inactive\">Inactive</span>") +
        "</td>" +
        "<td class=\"text-end\">" +
          "<div class=\"designation-actions\">" +
            "<button type=\"button\" class=\"btn btn-sm btn-outline-secondary btn-toggle-active\" data-id=\"" + designation.designationId + "\">" +
              (designation.isActive ? "Deactivate" : "Activate") +
            "</button>" +
            "<button type=\"button\" class=\"btn btn-sm btn-outline-primary btn-edit-designation\" data-id=\"" + designation.designationId + "\">Edit</button>" +
            "<button type=\"button\" class=\"btn btn-sm btn-outline-danger btn-delete-designation\" data-id=\"" + designation.designationId + "\" data-name=\"" + escapeHtml(designation.name) + "\">Delete</button>" +
          "</div>" +
        "</td>";

      tbody.appendChild(row);
    });
  }

  function loadDesignations() {
    var tbody = document.getElementById("designationsTableBody");
    var emptyState = document.getElementById("designationsEmptyState");
    var table = document.getElementById("designationsTable");

    if (tbody) {
      tbody.innerHTML = "<tr><td colspan=\"6\" class=\"text-center text-muted py-4\">Loading designations...</td></tr>";
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
          showPageAlert(result.message || "Unable to load designations.", "danger");
          return;
        }

        renderDesignations(result.data);
      })
      .catch(function () {
        showPageAlert("Unable to load designations.", "danger");
      });
  }

  function resetDesignationForm() {
    var form = document.getElementById("designationForm");
    if (!form) {
      return;
    }

    form.reset();
    form.classList.remove("was-validated");
    hideFormAlert();

    document.getElementById("DesignationId").value = "";
    document.getElementById("IsActive").checked = true;
    document.getElementById("designationModalLabel").textContent = "Add Designation";
    document.getElementById("designationFormSubmit").textContent = "Save Designation";

    editingDesignationId = null;
  }

  function openCreateModal() {
    resetDesignationForm();
    designationModal.show();
  }

  function populateForm(designation) {
    document.getElementById("DesignationId").value = designation.designationId || "";
    document.getElementById("Code").value = designation.code || "";
    document.getElementById("Name").value = designation.name || "";
    document.getElementById("Description").value = designation.description || "";
    document.getElementById("IsActive").checked = !!designation.isActive;
  }

  function openEditModal(designationId) {
    fetch(config.getUrl + "?id=" + encodeURIComponent(designationId), {
      headers: {
        Accept: "application/json"
      }
    })
      .then(function (response) {
        return response.json();
      })
      .then(function (result) {
        if (!result.success || !result.data) {
          showPageAlert(result.message || "Unable to load designation.", "danger");
          return;
        }

        resetDesignationForm();
        editingDesignationId = designationId;
        populateForm(result.data);

        document.getElementById("designationModalLabel").textContent = "Edit Designation";
        document.getElementById("designationFormSubmit").textContent = "Update Designation";
        designationModal.show();
      })
      .catch(function () {
        showPageAlert("Unable to load designation.", "danger");
      });
  }

  function collectFormData() {
    var form = document.getElementById("designationForm");
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

  function submitDesignationForm(event) {
    event.preventDefault();
    hideFormAlert();
    hidePageAlert();

    var form = document.getElementById("designationForm");
    if (!form.checkValidity()) {
      form.classList.add("was-validated");
      return;
    }

    var url = editingDesignationId
      ? config.editUrl + "?id=" + encodeURIComponent(editingDesignationId)
      : config.createUrl;

    var submitButton = document.getElementById("designationFormSubmit");
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
            showFormAlert(result.payload.message || "Unable to save designation.");
          }
          return;
        }

        designationModal.hide();
        showPageAlert(result.payload.message || "Designation saved successfully.", "success");
        loadDesignations();
      })
      .catch(function () {
        showFormAlert("Unable to save designation.");
      })
      .finally(function () {
        submitButton.disabled = false;
      });
  }

  function deleteDesignation(designationId) {
    var formData = new FormData();
    formData.append("__RequestVerificationToken", getAntiForgeryToken());

    fetch(config.deleteUrl + "?id=" + encodeURIComponent(designationId), {
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
          showPageAlert(result.message || "Unable to delete designation.", "danger");
          return;
        }

        deleteModal.hide();
        showPageAlert(result.message || "Designation deleted successfully.", "success");
        loadDesignations();
      })
      .catch(function () {
        showPageAlert("Unable to delete designation.", "danger");
      });
  }

  function toggleActive(designationId) {
    var formData = new FormData();
    formData.append("__RequestVerificationToken", getAntiForgeryToken());

    fetch(config.toggleActiveUrl + "?id=" + encodeURIComponent(designationId), {
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
          showPageAlert(result.message || "Unable to update designation status.", "danger");
          return;
        }

        showPageAlert(result.message || "Designation status updated.", "success");
        loadDesignations();
      })
      .catch(function () {
        showPageAlert("Unable to update designation status.", "danger");
      });
  }

  function bindEvents() {
    var addButton = document.getElementById("btnAddDesignation");
    var form = document.getElementById("designationForm");
    var statusFilter = document.getElementById("filterDesignationStatus");
    var searchInput = document.getElementById("searchDesignations");
    var confirmDeleteButton = document.getElementById("confirmDeleteDesignation");
    var tableBody = document.getElementById("designationsTableBody");
    var designationModalElement = document.getElementById("designationModal");
    var deleteModalElement = document.getElementById("deleteDesignationModal");
    var codeInput = document.getElementById("Code");

    if (designationModalElement) {
      designationModal = new bootstrap.Modal(designationModalElement);
      designationModalElement.addEventListener("hidden.bs.modal", resetDesignationForm);
    }

    if (deleteModalElement) {
      deleteModal = new bootstrap.Modal(deleteModalElement);
    }

    if (addButton) {
      addButton.addEventListener("click", openCreateModal);
    }

    if (form) {
      form.addEventListener("submit", submitDesignationForm);
    }

    if (codeInput) {
      codeInput.addEventListener("input", function () {
        codeInput.value = codeInput.value.toUpperCase();
      });
    }

    if (statusFilter) {
      statusFilter.addEventListener("change", loadDesignations);
    }

    if (searchInput) {
      searchInput.addEventListener("input", function () {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(loadDesignations, 300);
      });
    }

    if (confirmDeleteButton) {
      confirmDeleteButton.addEventListener("click", function () {
        if (pendingDeleteId) {
          deleteDesignation(pendingDeleteId);
        }
      });
    }

    if (tableBody) {
      tableBody.addEventListener("click", function (event) {
        var editButton = event.target.closest(".btn-edit-designation");
        var deleteButton = event.target.closest(".btn-delete-designation");
        var toggleButton = event.target.closest(".btn-toggle-active");

        if (editButton) {
          openEditModal(editButton.getAttribute("data-id"));
          return;
        }

        if (deleteButton) {
          pendingDeleteId = deleteButton.getAttribute("data-id");
          document.getElementById("deleteDesignationName").textContent =
            deleteButton.getAttribute("data-name") || "this designation";
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
    loadDesignations();
  });
})();
