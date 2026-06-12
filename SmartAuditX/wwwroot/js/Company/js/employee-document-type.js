"use strict";

(function () {
  var config = window.employeeDocumentTypeConfig || {};
  var employeeDocumentTypeModal;
  var deleteModal;
  var editingEmployeeDocumentTypeId = null;
  var pendingDeleteId = null;
  var searchTimer;
  var currentPageNumber = 1;
  var currentPageSize = 10;
  var currentSearchTerm = "";
  var currentIsActive = null;
  var currentSortColumn = "";
  var currentSortOrder = "";

  function onReady(callback) {
    if (document.readyState === "loading") {
      document.addEventListener("DOMContentLoaded", callback);
      return;
    }

    callback();
  }

  function getAntiForgeryToken() {
    var tokenInput = document.querySelector("#employeeDocumentTypeForm input[name='__RequestVerificationToken']");
    return tokenInput ? tokenInput.value : "";
  }

  function escapeHtml(value) {
    return String(value || "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function showPageAlert(message, type) {
    var alert = document.getElementById("employeeDocumentTypeAlert");
    if (!alert) {
      return;
    }

    alert.className = "alert alert-" + (type || "success");
    alert.textContent = message;
    alert.classList.remove("d-none");
  }

  function hidePageAlert() {
    var alert = document.getElementById("employeeDocumentTypeAlert");
    if (alert) {
      alert.classList.add("d-none");
    }
  }

  function showFormAlert(message) {
    var alert = document.getElementById("employeeDocumentTypeFormAlert");
    if (!alert) {
      return;
    }

    alert.textContent = message;
    alert.classList.remove("d-none");
  }

  function hideFormAlert() {
    var alert = document.getElementById("employeeDocumentTypeFormAlert");
    if (alert) {
      alert.classList.add("d-none");
      alert.textContent = "";
    }
  }

  function getFilterValues() {
    var statusFilter = document.getElementById("filterEmployeeDocumentTypeStatus");
    var searchInput = document.getElementById("searchEmployeeDocumentTypes");

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
      params.set("searchTerm", filters.search);
    }

    params.set("pageNumber", currentPageNumber);
    params.set("pageSize", currentPageSize);
    if (currentSortColumn) {
      params.set("sortColumn", currentSortColumn);
    }
    if (currentSortOrder) {
      params.set("sortOrder", currentSortOrder);
    }

    var query = params.toString();
    return query ? config.listUrl + "?" + query : config.listUrl;
  }

  function renderEmployeeDocumentTypes(employeeDocumentTypes, totalCount) {
    var tbody = document.getElementById("employeeDocumentTypesTableBody");
    var emptyState = document.getElementById("employeeDocumentTypesEmptyState");
    var table = document.getElementById("employeeDocumentTypesTable");
    var infoElement = document.getElementById("employeeDocumentTypesInfo");
    var paginationElement = document.getElementById("employeeDocumentTypesPagination");

    if (!tbody) {
      return;
    }

    tbody.innerHTML = "";

    if (!employeeDocumentTypes || employeeDocumentTypes.length === 0) {
      if (table) {
        table.classList.add("d-none");
      }

      if (emptyState) {
        emptyState.classList.remove("d-none");
      }

      if (infoElement) {
        infoElement.textContent = "No document types to display.";
      }

      if (paginationElement) {
        paginationElement.innerHTML = "";
      }

      return;
    }

    if (table) {
      table.classList.remove("d-none");
    }

    if (emptyState) {
      emptyState.classList.add("d-none");
    }

    employeeDocumentTypes.forEach(function (docType) {
      var row = document.createElement("tr");
      row.innerHTML =
        "<td>" + escapeHtml(docType.name) + "</td>" +
        "<td><span class=\"employee-document-type-description\" title=\"" + escapeHtml(docType.description || "") + "\">" + escapeHtml(docType.description || "—") + "</span></td>" +
        "<td class=\"text-center\">" +
          (docType.isRequired
            ? "<span class=\"badge bg-success\">Yes</span>"
            : "<span class=\"badge bg-secondary\">No</span>") +
        "</td>" +
        "<td>" +
          (docType.isActive
            ? "<span class=\"badge bg-success\">Active</span>"
            : "<span class=\"badge bg-danger\">Inactive</span>") +
        "</td>" +
        "<td class=\"text-end\">" +
          "<span class=\"text-muted\">" + new Date(docType.createdAt).toLocaleString() + "</span>" +
        "</td>" +
        "<td class=\"text-end\">" +
          "<div class=\"employee-document-type-actions\">" +
            "<button type=\"button\" class=\"btn btn-sm btn-outline-secondary btn-toggle-active\" data-id=\"" + docType.employeeDocumentTypeId + "\">" +
              (docType.isActive ? "Deactivate" : "Activate") +
            "</button>" +
            "<button type=\"button\" class=\"btn btn-sm btn-outline-primary btn-edit-employee-document-type\" data-id=\"" + docType.employeeDocumentTypeId + "\">Edit</button>" +
            "<button type=\"button\" class=\"btn btn-sm btn-outline-danger btn-delete-employee-document-type\" data-id=\"" + docType.employeeDocumentTypeId + "\" data-name=\"" + escapeHtml(docType.name) + "\">Delete</button>" +
          "</div>" +
        "</td>";

      tbody.appendChild(row);
    });

    // Update info
    if (infoElement) {
      var startRecord = (currentPageNumber - 1) * currentPageSize + 1;
      var endRecord = Math.min(startRecord + currentPageSize - 1, totalCount);
      if (totalCount === 0) {
        startRecord = 0;
      }
      infoElement.textContent = "Showing " + startRecord + " to " + endRecord + " of " + totalCount + " document types";
    }

    // Update pagination
    if (paginationElement) {
      paginationElement.innerHTML = generatePagination(totalCount);
    }
  }

  function generatePagination(totalCount) {
    var totalPages = Math.max(1, Math.ceil(totalCount / currentPageSize));
    var pagination = "";

    // Previous button
    if (currentPageNumber > 1) {
      pagination += '<li class="page-item"><a class="page-link" href="#" data-page="' + (currentPageNumber - 1) + '">Previous</a></li>';
    } else {
      pagination += '<li class="page-item disabled"><span class="page-link">Previous</span></li>';
    }

    // Page numbers
    var startPage = Math.max(1, currentPageNumber - 2);
    var endPage = Math.min(totalPages, startPage + 4);
    if (endPage - startPage < 4) {
      startPage = Math.max(1, endPage - 4);
    }

    for (var i = startPage; i <= endPage; i++) {
      if (i === currentPageNumber) {
        pagination += '<li class="page-item active"><span class="page-link">' + i + '</span></li>';
      } else {
        pagination += '<li class="page-item"><a class="page-link" href="#" data-page="' + i + '">' + i + '</a></li>';
      }
    }

    // Next button
    if (currentPageNumber < totalPages) {
      pagination += '<li class="page-item"><a class="page-link" href="#" data-page="' + (currentPageNumber + 1) + '">Next</a></li>';
    } else {
      pagination += '<li class="page-item disabled"><span class="page-link">Next</span></li>';
    }

    return pagination;
  }

  function loadEmployeeDocumentTypes() {
    var tbody = document.getElementById("employeeDocumentTypesTableBody");
    var emptyState = document.getElementById("employeeDocumentTypesEmptyState");
    var table = document.getElementById("employeeDocumentTypesTable");

    if (tbody) {
tbody.innerHTML =
  "<tr><td colspan=\"6\" class=\"text-center text-muted py-4\">Loading document types...</td></tr>";    }
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
          showPageAlert(result.message || "Unable to load document types.", "danger");
          return;
        }

        renderEmployeeDocumentTypes(result.data, result.total);
        currentPageNumber = result.page;
        currentPageSize = result.pageSize;
      })
      .catch(function () {
        showPageAlert("Unable to load document types.", "danger");
      });
  }

  function resetEmployeeDocumentTypeForm() {
    var form = document.getElementById("employeeDocumentTypeForm");
    if (!form) {
      return;
    }

    form.reset();
    form.classList.remove("was-validated");
    hideFormAlert();

    document.getElementById("EmployeeDocumentTypeId").value = "";
    document.getElementById("IsActive").checked = true;
    document.getElementById("IsRequired").checked = false;
    document.getElementById("employeeDocumentTypeModalLabel").textContent = "Add Document Type";
    document.getElementById("employeeDocumentTypeFormSubmit").textContent = "Save Document Type";

    editingEmployeeDocumentTypeId = null;
  }

  function openCreateModal() {
    resetEmployeeDocumentTypeForm();
    employeeDocumentTypeModal.show();
  }

  function populateForm(docType) {
    document.getElementById("EmployeeDocumentTypeId").value = docType.employeeDocumentTypeId || "";
    document.getElementById("Name").value = docType.name || "";
    document.getElementById("Description").value = docType.description || "";
    document.getElementById("IsRequired").checked = !!docType.isRequired;
    document.getElementById("IsActive").checked = !!docType.isActive;
  }

  function openEditModal(docTypeId) {
    fetch(config.getUrl + "?id=" + encodeURIComponent(docTypeId), {
      headers: {
        Accept: "application/json"
      }
    })
      .then(function (response) {
        return response.json();
      })
      .then(function (result) {
        if (!result.success || !result.data) {
          showPageAlert(result.message || "Unable to load document type.", "danger");
          return;
        }

        resetEmployeeDocumentTypeForm();
        editingEmployeeDocumentTypeId = docTypeId;
        populateForm(result.data);

        document.getElementById("employeeDocumentTypeModalLabel").textContent = "Edit Document Type";
        document.getElementById("employeeDocumentTypeFormSubmit").textContent = "Update Document Type";
        employeeDocumentTypeModal.show();
      })
      .catch(function () {
        showPageAlert("Unable to load document type.", "danger");
      });
  }

  function collectFormData() {
    var form = document.getElementById("employeeDocumentTypeForm");
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

  function submitEmployeeDocumentTypeForm(event) {
    event.preventDefault();
    hideFormAlert();
    hidePageAlert();

    var form = document.getElementById("employeeDocumentTypeForm");
    if (!form.checkValidity()) {
      form.classList.add("was-validated");
      return;
    }

    var url = editingEmployeeDocumentTypeId
      ? config.editUrl + "?id=" + encodeURIComponent(editingEmployeeDocumentTypeId)
      : config.createUrl;

    var submitButton = document.getElementById("employeeDocumentTypeFormSubmit");
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
            showFormAlert(result.payload.message || "Unable to save document type.");
          }
          return;
        }

        employeeDocumentTypeModal.hide();
        showPageAlert(result.payload.message || "Document type saved successfully.", "success");
        loadEmployeeDocumentTypes();
      })
      .catch(function () {
        showFormAlert("Unable to save document type.");
      })
      .finally(function () {
        submitButton.disabled = false;
      });
  }

  function deleteEmployeeDocumentType(docTypeId) {
    var formData = new FormData();
    formData.append("__RequestVerificationToken", getAntiForgeryToken());

    fetch(config.deleteUrl + "?id=" + encodeURIComponent(docTypeId), {
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
          showPageAlert(result.message || "Unable to delete document type.", "danger");
          return;
        }

        deleteModal.hide();
        showPageAlert(result.message || "Document type deleted successfully.", "success");
        loadEmployeeDocumentTypes();
      })
      .catch(function () {
        showPageAlert("Unable to delete document type.", "danger");
      });
  }

  function toggleActive(docTypeId) {
    var formData = new FormData();
    formData.append("__RequestVerificationToken", getAntiForgeryToken());

    fetch(config.toggleActiveUrl + "?id=" + encodeURIComponent(docTypeId), {
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
          showPageAlert(result.message || "Unable to update document type status.", "danger");
          return;
        }

        showPageAlert(result.message || "Document type status updated.", "success");
        loadEmployeeDocumentTypes();
      })
      .catch(function () {
        showPageAlert("Unable to update document type status.", "danger");
      });
  }

  function bindEvents() {
    var addButton = document.getElementById("btnAddEmployeeDocumentType");
    var form = document.getElementById("employeeDocumentTypeForm");
    var statusFilter = document.getElementById("filterEmployeeDocumentTypeStatus");
    var searchInput = document.getElementById("searchEmployeeDocumentTypes");
    var confirmDeleteButton = document.getElementById("confirmDeleteEmployeeDocumentType");
    var tableBody = document.getElementById("employeeDocumentTypesTableBody");
    var employeeDocumentTypeModalElement = document.getElementById("employeeDocumentTypeModal");
    var deleteModalElement = document.getElementById("deleteEmployeeDocumentTypeModal");
    var paginationElement = document.getElementById("employeeDocumentTypesPagination");

    if (employeeDocumentTypeModalElement) {
      employeeDocumentTypeModal = new bootstrap.Modal(employeeDocumentTypeModalElement);
      employeeDocumentTypeModalElement.addEventListener("hidden.bs.modal", resetEmployeeDocumentTypeForm);
    }

    if (deleteModalElement) {
      deleteModal = new bootstrap.Modal(deleteModalElement);
    }

    if (addButton) {
      addButton.addEventListener("click", openCreateModal);
    }

    if (form) {
      form.addEventListener("submit", submitEmployeeDocumentTypeForm);
    }

    if (statusFilter) {
      statusFilter.addEventListener("change", function () {
        currentPageNumber = 1; // Reset to first page when filter changes
        loadEmployeeDocumentTypes();
      });
    }

    if (searchInput) {
      searchInput.addEventListener("input", function () {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(function () {
          currentPageNumber = 1; // Reset to first page when search changes
          loadEmployeeDocumentTypes();
        }, 300);
      });
    }

    if (confirmDeleteButton) {
      confirmDeleteButton.addEventListener("click", function () {
        if (pendingDeleteId) {
          deleteEmployeeDocumentType(pendingDeleteId);
        }
      });
    }

    if (tableBody) {
      tableBody.addEventListener("click", function (event) {
        var editButton = event.target.closest(".btn-edit-employee-document-type");
        var deleteButton = event.target.closest(".btn-delete-employee-document-type");
        var toggleButton = event.target.closest(".btn-toggle-active");

        if (editButton) {
          openEditModal(editButton.getAttribute("data-id"));
          return;
        }

        if (deleteButton) {
          pendingDeleteId = deleteButton.getAttribute("data-id");
          document.getElementById("deleteEmployeeDocumentTypeName").textContent =
            deleteButton.getAttribute("data-name") || "this document type";
          deleteModal.show();
          return;
        }

        if (toggleButton) {
          toggleActive(toggleButton.getAttribute("data-id"));
        }
      });
    }

    if (paginationElement) {
      paginationElement.addEventListener("click", function (event) {
        var pageLink = event.target.closest(".page-link");
        if (pageLink) {
          event.preventDefault();
          var page = parseInt(pageLink.getAttribute("data-page"));
          if (!isNaN(page)) {
            currentPageNumber = page;
            loadEmployeeDocumentTypes();
          }
        }
      });
    }
  }

  onReady(function () {
    bindEvents();
    loadEmployeeDocumentTypes();
  });
})();