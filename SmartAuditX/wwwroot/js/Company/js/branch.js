"use strict";

(function () {
  var config = window.branchConfig || {};
  var branchModal;
  var deleteModal;
  var editingBranchId = null;
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
    var tokenInput = document.querySelector("#branchForm input[name='__RequestVerificationToken']");
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
    var alert = document.getElementById("branchAlert");
    if (!alert) {
      return;
    }

    alert.className = "alert alert-" + (type || "success");
    alert.textContent = message;
    alert.classList.remove("d-none");
  }

  function hidePageAlert() {
    var alert = document.getElementById("branchAlert");
    if (alert) {
      alert.classList.add("d-none");
    }
  }

  function showFormAlert(message) {
    var alert = document.getElementById("branchFormAlert");
    if (!alert) {
      return;
    }

    alert.textContent = message;
    alert.classList.remove("d-none");
  }

  function hideFormAlert() {
    var alert = document.getElementById("branchFormAlert");
    if (alert) {
      alert.classList.add("d-none");
      alert.textContent = "";
    }
  }

  function getFilterValues() {
    var statusFilter = document.getElementById("filterBranchStatus");
    var searchInput = document.getElementById("searchBranches");

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

  function renderBranches(branches) {
    var tbody = document.getElementById("branchesTableBody");
    var emptyState = document.getElementById("branchesEmptyState");
    var table = document.getElementById("branchesTable");

    if (!tbody) {
      return;
    }

    tbody.innerHTML = "";

    if (!branches || branches.length === 0) {
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

    branches.forEach(function (branch) {
      var row = document.createElement("tr");
      var location = [branch.physicalAddress, branch.phoneNumber].filter(Boolean).join(" / ") || "—";
      var typeBadge = branch.isHeadOffice
        ? "<span class=\"badge branch-type-headoffice\">Head Office</span>"
        : "<span class=\"badge branch-type-branch\">Branch</span>";

      row.innerHTML =
        "<td><span class=\"badge branch-code-badge\">" + escapeHtml(branch.branchCode) + "</span></td>" +
        "<td>" + escapeHtml(branch.branchName) + "</td>" +
        "<td><span class=\"branch-location\" title=\"" + escapeHtml(location) + "\">" + escapeHtml(location) + "</span></td>" +
        "<td>" + typeBadge + "</td>" +
        "<td>" + escapeHtml(String(branch.departmentCount)) + "</td>" +
        "<td>" + escapeHtml(String(branch.employeeCount)) + "</td>" +
        "<td>" +
          (branch.isActive
            ? "<span class=\"badge branch-status-active\">Active</span>"
            : "<span class=\"badge branch-status-inactive\">Inactive</span>") +
        "</td>" +
        "<td class=\"text-end\">" +
          "<div class=\"branch-actions\">" +
            "<button type=\"button\" class=\"btn btn-sm btn-outline-secondary btn-toggle-active\" data-id=\"" + branch.branchId + "\">" +
              (branch.isActive ? "Deactivate" : "Activate") +
            "</button>" +
            "<button type=\"button\" class=\"btn btn-sm btn-outline-primary btn-edit-branch\" data-id=\"" + branch.branchId + "\">Edit</button>" +
            "<button type=\"button\" class=\"btn btn-sm btn-outline-danger btn-delete-branch\" data-id=\"" + branch.branchId + "\" data-name=\"" + escapeHtml(branch.branchName) + "\">Delete</button>" +
          "</div>" +
        "</td>";

      tbody.appendChild(row);
    });
  }

  function loadBranches() {
    var tbody = document.getElementById("branchesTableBody");
    var emptyState = document.getElementById("branchesEmptyState");
    var table = document.getElementById("branchesTable");

    if (tbody) {
      tbody.innerHTML = "<tr><td colspan=\"8\" class=\"text-center text-muted py-4\">Loading branches...</td></tr>";
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
          showPageAlert(result.message || "Unable to load branches.", "danger");
          return;
        }

        renderBranches(result.data);
      })
      .catch(function () {
        showPageAlert("Unable to load branches.", "danger");
      });
  }

  function resetBranchForm() {
    var form = document.getElementById("branchForm");
    if (!form) {
      return;
    }

    form.reset();
    form.classList.remove("was-validated");
    hideFormAlert();

    document.getElementById("BranchId").value = "";
    document.getElementById("IsActive").checked = true;
    document.getElementById("IsHeadOffice").checked = false;
    document.getElementById("branchModalLabel").textContent = "Add Branch";
    document.getElementById("branchFormSubmit").textContent = "Save Branch";

    editingBranchId = null;
  }

  function openCreateModal() {
    resetBranchForm();
    branchModal.show();
  }

  function populateForm(branch) {
    document.getElementById("BranchId").value = branch.branchId || "";
    document.getElementById("BranchCode").value = branch.branchCode || "";
    document.getElementById("BranchName").value = branch.branchName || "";
    document.getElementById("Email").value = branch.email || "";
    document.getElementById("PhoneNumber").value = branch.phoneNumber || "";
    document.getElementById("PhysicalAddress").value = branch.physicalAddress || "";
    document.getElementById("IsHeadOffice").checked = !!branch.isHeadOffice;
    document.getElementById("IsActive").checked = !!branch.isActive;
  }

  function openEditModal(branchId) {
    fetch(config.getUrl + "?id=" + encodeURIComponent(branchId), {
      headers: {
        Accept: "application/json"
      }
    })
      .then(function (response) {
        return response.json();
      })
      .then(function (result) {
        if (!result.success || !result.data) {
          showPageAlert(result.message || "Unable to load branch.", "danger");
          return;
        }

        resetBranchForm();
        editingBranchId = branchId;
        populateForm(result.data);

        document.getElementById("branchModalLabel").textContent = "Edit Branch";
        document.getElementById("branchFormSubmit").textContent = "Update Branch";
        branchModal.show();
      })
      .catch(function () {
        showPageAlert("Unable to load branch.", "danger");
      });
  }

  function collectFormData() {
    var form = document.getElementById("branchForm");
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

  function submitBranchForm(event) {
    event.preventDefault();
    hideFormAlert();
    hidePageAlert();

    var form = document.getElementById("branchForm");
    if (!form.checkValidity()) {
      form.classList.add("was-validated");
      return;
    }

    var url = editingBranchId
      ? config.editUrl + "?id=" + encodeURIComponent(editingBranchId)
      : config.createUrl;

    var submitButton = document.getElementById("branchFormSubmit");
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
            showFormAlert(result.payload.message || "Unable to save branch.");
          }
          return;
        }

        branchModal.hide();
        showPageAlert(result.payload.message || "Branch saved successfully.", "success");
        loadBranches();
      })
      .catch(function () {
        showFormAlert("Unable to save branch.");
      })
      .finally(function () {
        submitButton.disabled = false;
      });
  }

  function deleteBranch(branchId) {
    var formData = new FormData();
    formData.append("__RequestVerificationToken", getAntiForgeryToken());

    fetch(config.deleteUrl + "?id=" + encodeURIComponent(branchId), {
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
          showPageAlert(result.message || "Unable to delete branch.", "danger");
          return;
        }

        deleteModal.hide();
        showPageAlert(result.message || "Branch deleted successfully.", "success");
        loadBranches();
      })
      .catch(function () {
        showPageAlert("Unable to delete branch.", "danger");
      });
  }

  function toggleActive(branchId) {
    var formData = new FormData();
    formData.append("__RequestVerificationToken", getAntiForgeryToken());

    fetch(config.toggleActiveUrl + "?id=" + encodeURIComponent(branchId), {
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
          showPageAlert(result.message || "Unable to update branch status.", "danger");
          return;
        }

        showPageAlert(result.message || "Branch status updated.", "success");
        loadBranches();
      })
      .catch(function () {
        showPageAlert("Unable to update branch status.", "danger");
      });
  }

  function bindEvents() {
    var addButton = document.getElementById("btnAddBranch");
    var form = document.getElementById("branchForm");
    var statusFilter = document.getElementById("filterBranchStatus");
    var searchInput = document.getElementById("searchBranches");
    var confirmDeleteButton = document.getElementById("confirmDeleteBranch");
    var tableBody = document.getElementById("branchesTableBody");
    var branchModalElement = document.getElementById("branchModal");
    var deleteModalElement = document.getElementById("deleteBranchModal");
    var codeInput = document.getElementById("BranchCode");

    if (branchModalElement) {
      branchModal = new bootstrap.Modal(branchModalElement);
      branchModalElement.addEventListener("hidden.bs.modal", resetBranchForm);
    }

    if (deleteModalElement) {
      deleteModal = new bootstrap.Modal(deleteModalElement);
    }

    if (addButton) {
      addButton.addEventListener("click", openCreateModal);
    }

    if (form) {
      form.addEventListener("submit", submitBranchForm);
    }

    if (codeInput) {
      codeInput.addEventListener("input", function () {
        codeInput.value = codeInput.value.toUpperCase();
      });
    }

    if (statusFilter) {
      statusFilter.addEventListener("change", loadBranches);
    }

    if (searchInput) {
      searchInput.addEventListener("input", function () {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(loadBranches, 300);
      });
    }

    if (confirmDeleteButton) {
      confirmDeleteButton.addEventListener("click", function () {
        if (pendingDeleteId) {
          deleteBranch(pendingDeleteId);
        }
      });
    }

    if (tableBody) {
      tableBody.addEventListener("click", function (event) {
        var editButton = event.target.closest(".btn-edit-branch");
        var deleteButton = event.target.closest(".btn-delete-branch");
        var toggleButton = event.target.closest(".btn-toggle-active");

        if (editButton) {
          openEditModal(editButton.getAttribute("data-id"));
          return;
        }

        if (deleteButton) {
          pendingDeleteId = deleteButton.getAttribute("data-id");
          document.getElementById("deleteBranchName").textContent =
            deleteButton.getAttribute("data-name") || "this branch";
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
    loadBranches();
  });
})();
