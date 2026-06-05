"use strict";

(function () {
  var config = window.branchDepartmentConfig || {};
  var branchDepartmentModal;
  var deleteModal;
  var pendingDeleteId = null;
  var branchesData = [];
  var departmentsData = [];

  function onReady(callback) {
    if (document.readyState === "loading") {
      document.addEventListener("DOMContentLoaded", callback);
      return;
    }

    callback();
  }

  function getAntiForgeryToken() {
    var tokenInput = document.querySelector("#branchDepartmentForm input[name='__RequestVerificationToken']");
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

  function formatDate(dateString) {
    if (!dateString) {
      return "—";
    }

    var date = new Date(dateString);
    if (isNaN(date.getTime())) {
      return "—";
    }

    return date.toLocaleDateString(undefined, {
      year: "numeric",
      month: "short",
      day: "numeric"
    });
  }

  function showPageAlert(message, type) {
    var alert = document.getElementById("branchDepartmentAlert");
    if (!alert) {
      return;
    }

    alert.className = "alert alert-" + (type || "success");
    alert.textContent = message;
    alert.classList.remove("d-none");
  }

  function hidePageAlert() {
    var alert = document.getElementById("branchDepartmentAlert");
    if (alert) {
      alert.classList.add("d-none");
    }
  }

  function showFormAlert(message) {
    var alert = document.getElementById("branchDepartmentFormAlert");
    if (!alert) {
      return;
    }

    alert.textContent = message;
    alert.classList.remove("d-none");
  }

  function hideFormAlert() {
    var alert = document.getElementById("branchDepartmentFormAlert");
    if (alert) {
      alert.classList.add("d-none");
      alert.textContent = "";
    }
  }

  function populateDropdown(selectElement, items, valueField, textField, defaultText) {
    if (!selectElement) {
      return;
    }

    var currentValue = selectElement.value;
    selectElement.innerHTML = '<option value="">' + (defaultText || "Select...") + "</option>";

    items.forEach(function (item) {
      var option = document.createElement("option");
      option.value = item[valueField];
      option.textContent = item[textField];
      selectElement.appendChild(option);
    });

    selectElement.value = currentValue;
  }

  function populateFilterDropdown(selectElement, items, valueField, textField, defaultText) {
    if (!selectElement) {
      return;
    }

    var currentValue = selectElement.value;
    selectElement.innerHTML = '<option value="">' + (defaultText || "All") + "</option>";

    items.forEach(function (item) {
      var option = document.createElement("option");
      option.value = item[valueField];
      option.textContent = item[textField];
      selectElement.appendChild(option);
    });

    selectElement.value = currentValue;
  }

  function loadDropdowns() {
    fetch(config.branchesUrl, {
      headers: { Accept: "application/json" }
    })
      .then(function (response) { return response.json(); })
      .then(function (result) {
        if (result.success) {
          branchesData = result.data;
          populateFilterDropdown(
            document.getElementById("filterBranch"),
            branchesData,
            "branchId",
            "branchName",
            "All Branches"
          );
          populateDropdown(
            document.getElementById("BranchId"),
            branchesData,
            "branchId",
            "branchName",
            "Select a branch..."
          );
        }
      })
      .catch(function () { /* silent fail */ });

    fetch(config.departmentsUrl, {
      headers: { Accept: "application/json" }
    })
      .then(function (response) { return response.json(); })
      .then(function (result) {
        if (result.success) {
          departmentsData = result.data;
          populateFilterDropdown(
            document.getElementById("filterDepartment"),
            departmentsData,
            "departmentId",
            "name",
            "All Departments"
          );
          populateDropdown(
            document.getElementById("DepartmentId"),
            departmentsData,
            "departmentId",
            "name",
            "Select a department..."
          );
        }
      })
      .catch(function () { /* silent fail */ });
  }

  function getFilterValues() {
    var branchFilter = document.getElementById("filterBranch");
    var departmentFilter = document.getElementById("filterDepartment");

    return {
      branchId: branchFilter ? branchFilter.value : "",
      departmentId: departmentFilter ? departmentFilter.value : ""
    };
  }

  function buildListUrl() {
    var filters = getFilterValues();
    var params = new URLSearchParams();

    if (filters.branchId) {
      params.set("branchId", filters.branchId);
    }

    if (filters.departmentId) {
      params.set("departmentId", filters.departmentId);
    }

    var query = params.toString();
    return query ? config.listUrl + "?" + query : config.listUrl;
  }

  function renderBranchDepartments(links) {
    var tbody = document.getElementById("branchDepartmentsTableBody");
    var emptyState = document.getElementById("branchDepartmentsEmptyState");
    var table = document.getElementById("branchDepartmentsTable");

    if (!tbody) {
      return;
    }

    tbody.innerHTML = "";

    if (!links || links.length === 0) {
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

    links.forEach(function (link) {
      var row = document.createElement("tr");
      row.innerHTML =
        "<td>" +
          "<span class=\"badge branch-code-badge me-1\">" + escapeHtml(link.branchCode) + "</span>" +
          escapeHtml(link.branchName) +
        "</td>" +
        "<td>" +
          "<span class=\"badge department-code-badge me-1\">" + escapeHtml(link.departmentCode) + "</span>" +
          escapeHtml(link.departmentName) +
        "</td>" +
        "<td>" + escapeHtml(formatDate(link.createdAt)) + "</td>" +
        "<td class=\"text-end\">" +
          "<div class=\"branch-department-actions\">" +
            "<button type=\"button\" class=\"btn btn-sm btn-outline-danger btn-delete-branch-department\" data-id=\"" + link.branchDepartmentId + "\" data-branch=\"" + escapeHtml(link.branchName) + "\" data-department=\"" + escapeHtml(link.departmentName) + "\">Unlink</button>" +
          "</div>" +
        "</td>";

      tbody.appendChild(row);
    });
  }

  function loadBranchDepartments() {
    var tbody = document.getElementById("branchDepartmentsTableBody");
    var emptyState = document.getElementById("branchDepartmentsEmptyState");
    var table = document.getElementById("branchDepartmentsTable");

    if (tbody) {
      tbody.innerHTML = "<tr><td colspan=\"4\" class=\"text-center text-muted py-4\">Loading assignments...</td></tr>";
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
          showPageAlert(result.message || "Unable to load assignments.", "danger");
          return;
        }

        renderBranchDepartments(result.data);
      })
      .catch(function () {
        showPageAlert("Unable to load assignments.", "danger");
      });
  }

  function resetBranchDepartmentForm() {
    var form = document.getElementById("branchDepartmentForm");
    if (!form) {
      return;
    }

    form.reset();
    form.classList.remove("was-validated");
    hideFormAlert();

    document.getElementById("BranchDepartmentId").value = "";
    document.getElementById("branchDepartmentModalLabel").textContent = "Link Department to Branch";
    document.getElementById("branchDepartmentFormSubmit").textContent = "Save Link";
  }

  function openCreateModal() {
    resetBranchDepartmentForm();
    branchDepartmentModal.show();
  }

  function collectFormData() {
    var form = document.getElementById("branchDepartmentForm");
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

  function submitBranchDepartmentForm(event) {
    event.preventDefault();
    hideFormAlert();
    hidePageAlert();

    var form = document.getElementById("branchDepartmentForm");
    if (!form.checkValidity()) {
      form.classList.add("was-validated");
      return;
    }

    var submitButton = document.getElementById("branchDepartmentFormSubmit");
    submitButton.disabled = true;

    fetch(config.createUrl, {
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
            showFormAlert(result.payload.message || "Unable to save link.");
          }
          return;
        }

        branchDepartmentModal.hide();
        showPageAlert(result.payload.message || "Department linked successfully.", "success");
        loadBranchDepartments();
      })
      .catch(function () {
        showFormAlert("Unable to save link.");
      })
      .finally(function () {
        submitButton.disabled = false;
      });
  }

  function deleteBranchDepartment(branchDepartmentId) {
    var formData = new FormData();
    formData.append("__RequestVerificationToken", getAntiForgeryToken());

    fetch(config.deleteUrl + "?id=" + encodeURIComponent(branchDepartmentId), {
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
          showPageAlert(result.message || "Unable to unlink department.", "danger");
          return;
        }

        deleteModal.hide();
        showPageAlert(result.message || "Department unlinked successfully.", "success");
        loadBranchDepartments();
      })
      .catch(function () {
        showPageAlert("Unable to unlink department.", "danger");
      });
  }

  function bindEvents() {
    var addButton = document.getElementById("btnAddBranchDepartment");
    var form = document.getElementById("branchDepartmentForm");
    var branchFilter = document.getElementById("filterBranch");
    var departmentFilter = document.getElementById("filterDepartment");
    var confirmDeleteButton = document.getElementById("confirmDeleteBranchDepartment");
    var tableBody = document.getElementById("branchDepartmentsTableBody");
    var modalElement = document.getElementById("branchDepartmentModal");
    var deleteModalElement = document.getElementById("deleteBranchDepartmentModal");

    if (modalElement) {
      branchDepartmentModal = new bootstrap.Modal(modalElement);
      modalElement.addEventListener("hidden.bs.modal", resetBranchDepartmentForm);
    }

    if (deleteModalElement) {
      deleteModal = new bootstrap.Modal(deleteModalElement);
    }

    if (addButton) {
      addButton.addEventListener("click", openCreateModal);
    }

    if (form) {
      form.addEventListener("submit", submitBranchDepartmentForm);
    }

    if (branchFilter) {
      branchFilter.addEventListener("change", loadBranchDepartments);
    }

    if (departmentFilter) {
      departmentFilter.addEventListener("change", loadBranchDepartments);
    }

    if (confirmDeleteButton) {
      confirmDeleteButton.addEventListener("click", function () {
        if (pendingDeleteId) {
          deleteBranchDepartment(pendingDeleteId);
        }
      });
    }

    if (tableBody) {
      tableBody.addEventListener("click", function (event) {
        var deleteButton = event.target.closest(".btn-delete-branch-department");

        if (deleteButton) {
          pendingDeleteId = deleteButton.getAttribute("data-id");
          document.getElementById("deleteBranchDepartmentName").textContent =
            deleteButton.getAttribute("data-department") || "this department";
          document.getElementById("deleteBranchDepartmentBranch").textContent =
            deleteButton.getAttribute("data-branch") || "this branch";
          deleteModal.show();
        }
      });
    }
  }

  onReady(function () {
    bindEvents();
    loadDropdowns();
    loadBranchDepartments();
  });
})();
