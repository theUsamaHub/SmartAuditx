"use strict";

(function () {
  var config = window.companyContactConfig || {};
  var contactModal;
  var deleteModal;
  var phoneInput;
  var phoneInputInstance;
  var editingContactId = null;
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
    var tokenInput = document.querySelector("#contactForm input[name='__RequestVerificationToken']");
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
    var alert = document.getElementById("contactAlert");
    if (!alert) {
      return;
    }

    alert.className = "alert alert-" + (type || "success");
    alert.textContent = message;
    alert.classList.remove("d-none");
  }

  function hidePageAlert() {
    var alert = document.getElementById("contactAlert");
    if (alert) {
      alert.classList.add("d-none");
    }
  }

  function showFormAlert(message) {
    var alert = document.getElementById("contactFormAlert");
    if (!alert) {
      return;
    }

    alert.textContent = message;
    alert.classList.remove("d-none");
  }

  function hideFormAlert() {
    var alert = document.getElementById("contactFormAlert");
    if (alert) {
      alert.classList.add("d-none");
      alert.textContent = "";
    }
  }

  function getFilterValues() {
    var typeFilter = document.getElementById("filterContactType");
    var searchInput = document.getElementById("searchContacts");

    return {
      contactType: typeFilter ? typeFilter.value : "",
      search: searchInput ? searchInput.value.trim() : ""
    };
  }

  function buildListUrl() {
    var filters = getFilterValues();
    var params = new URLSearchParams();

    if (filters.contactType) {
      params.set("contactType", filters.contactType);
    }

    if (filters.search) {
      params.set("search", filters.search);
    }

    var query = params.toString();
    return query ? config.listUrl + "?" + query : config.listUrl;
  }

  function renderContacts(contacts) {
    var tbody = document.getElementById("contactsTableBody");
    var emptyState = document.getElementById("contactsEmptyState");
    var table = document.getElementById("contactsTable");

    if (!tbody) {
      return;
    }

    tbody.innerHTML = "";

    if (!contacts || contacts.length === 0) {
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

    contacts.forEach(function (contact) {
      var row = document.createElement("tr");
      row.innerHTML =
        "<td><span class=\"badge contact-type-badge\">" + escapeHtml(contact.contactTypeDisplay) + "</span></td>" +
        "<td>" + escapeHtml(contact.contactName || "—") + "</td>" +
        "<td><a href=\"mailto:" + escapeHtml(contact.email) + "\">" + escapeHtml(contact.email) + "</a></td>" +
        "<td>" + escapeHtml(contact.fullPhone) + "</td>" +
        "<td>" + escapeHtml(contact.physicalAddress || "—") + "</td>" +
        "<td>" +
          (contact.isPrimary
            ? "<span class=\"badge contact-primary-badge\">Primary</span>"
            : "<button type=\"button\" class=\"btn btn-sm btn-outline-success btn-set-primary\" data-id=\"" + contact.companyContactId + "\">Set Primary</button>") +
        "</td>" +
        "<td class=\"text-end\">" +
          "<div class=\"company-contact-actions\">" +
            "<button type=\"button\" class=\"btn btn-sm btn-outline-primary btn-edit-contact\" data-id=\"" + contact.companyContactId + "\">Edit</button>" +
            "<button type=\"button\" class=\"btn btn-sm btn-outline-danger btn-delete-contact\" data-id=\"" + contact.companyContactId + "\" data-name=\"" + escapeHtml(contact.contactName || contact.email) + "\">Delete</button>" +
          "</div>" +
        "</td>";

      tbody.appendChild(row);
    });
  }

  function loadContacts() {
    var tbody = document.getElementById("contactsTableBody");
    var emptyState = document.getElementById("contactsEmptyState");
    var table = document.getElementById("contactsTable");

    if (tbody) {
      tbody.innerHTML = "<tr><td colspan=\"7\" class=\"text-center text-muted py-4\">Loading contacts...</td></tr>";
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
          showPageAlert(result.message || "Unable to load contacts.", "danger");
          return;
        }

        renderContacts(result.data);
      })
      .catch(function () {
        showPageAlert("Unable to load contacts.", "danger");
      });
  }

  function initPhoneInput() {
    var phoneField = document.getElementById("PhoneNumber");
    if (!phoneField || !window.intlTelInput) {
      return;
    }

    if (phoneInputInstance) {
      phoneInputInstance.destroy();
    }

    phoneInputInstance = window.intlTelInput(phoneField, {
      initialCountry: "pk",
      separateDialCode: true,
      utilsScript: "https://cdn.jsdelivr.net/npm/intl-tel-input@25.3.1/build/js/utils.js"
    });

    phoneInput = phoneField;
  }

  function syncPhoneFields() {
    var dialCodeInput = document.getElementById("PhoneDialCode");
    var localPhoneInput = document.getElementById("PhoneNumberLocal");

    if (!phoneInputInstance || !dialCodeInput || !localPhoneInput || !phoneInput) {
      return false;
    }

    var countryData = phoneInputInstance.getSelectedCountryData();
    var digits = phoneInput.value.replace(/\D/g, "");

    if (!digits) {
      phoneInput.classList.add("is-invalid");
      return false;
    }

    var localNumber = digits;
    if (digits.indexOf(countryData.dialCode) === 0) {
      localNumber = digits.substring(countryData.dialCode.length);
    }

    dialCodeInput.value = "+" + countryData.dialCode;
    localPhoneInput.value = localNumber;
    phoneInput.classList.remove("is-invalid");
    return true;
  }

  function resetContactForm() {
    var form = document.getElementById("contactForm");
    if (!form) {
      return;
    }

    form.reset();
    form.classList.remove("was-validated");
    hideFormAlert();

    document.getElementById("CompanyContactId").value = "";
    document.getElementById("contactModalLabel").textContent = "Add Contact";
    document.getElementById("contactFormSubmit").textContent = "Save Contact";

    editingContactId = null;

    if (phoneInputInstance) {
      phoneInputInstance.setCountry("pk");
    }
  }

  function openCreateModal() {
    resetContactForm();
    contactModal.show();
  }

  function populateForm(contact) {
    document.getElementById("CompanyContactId").value = contact.companyContactId || "";
    document.getElementById("ContactType").value = contact.contactType;
    document.getElementById("ContactName").value = contact.contactName || "";
    document.getElementById("Email").value = contact.email || "";
    document.getElementById("FaxNumber").value = contact.faxNumber || "";
    document.getElementById("PhysicalAddress").value = contact.physicalAddress || "";
    document.getElementById("IsPrimary").checked = !!contact.isPrimary;

    if (phoneInputInstance && contact.phoneDialCode && contact.phoneNumber) {
      phoneInputInstance.setNumber(contact.phoneDialCode + contact.phoneNumber);
    }
  }

  function openEditModal(contactId) {
    fetch(config.getUrl + "?id=" + encodeURIComponent(contactId), {
      headers: {
        Accept: "application/json"
      }
    })
      .then(function (response) {
        return response.json();
      })
      .then(function (result) {
        if (!result.success || !result.data) {
          showPageAlert(result.message || "Unable to load contact.", "danger");
          return;
        }

        resetContactForm();
        editingContactId = contactId;
        populateForm(result.data);

        document.getElementById("contactModalLabel").textContent = "Edit Contact";
        document.getElementById("contactFormSubmit").textContent = "Update Contact";
        contactModal.show();
      })
      .catch(function () {
        showPageAlert("Unable to load contact.", "danger");
      });
  }

  function collectFormData() {
    var form = document.getElementById("contactForm");
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

  function submitContactForm(event) {
    event.preventDefault();
    hideFormAlert();
    hidePageAlert();

    var form = document.getElementById("contactForm");
    if (!form.checkValidity() || !syncPhoneFields()) {
      form.classList.add("was-validated");
      return;
    }

    var url = editingContactId
      ? config.editUrl + "?id=" + encodeURIComponent(editingContactId)
      : config.createUrl;

    var submitButton = document.getElementById("contactFormSubmit");
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
            showFormAlert(result.payload.message || "Unable to save contact.");
          }
          return;
        }

        contactModal.hide();
        showPageAlert(result.payload.message || "Contact saved successfully.", "success");
        loadContacts();
      })
      .catch(function () {
        showFormAlert("Unable to save contact.");
      })
      .finally(function () {
        submitButton.disabled = false;
      });
  }

  function deleteContact(contactId) {
    var formData = new FormData();
    formData.append("__RequestVerificationToken", getAntiForgeryToken());

    fetch(config.deleteUrl + "?id=" + encodeURIComponent(contactId), {
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
          showPageAlert(result.message || "Unable to delete contact.", "danger");
          return;
        }

        deleteModal.hide();
        showPageAlert(result.message || "Contact deleted successfully.", "success");
        loadContacts();
      })
      .catch(function () {
        showPageAlert("Unable to delete contact.", "danger");
      });
  }

  function setPrimary(contactId) {
    var formData = new FormData();
    formData.append("__RequestVerificationToken", getAntiForgeryToken());

    fetch(config.setPrimaryUrl + "?id=" + encodeURIComponent(contactId), {
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
          showPageAlert(result.message || "Unable to update primary contact.", "danger");
          return;
        }

        showPageAlert(result.message || "Primary contact updated.", "success");
        loadContacts();
      })
      .catch(function () {
        showPageAlert("Unable to update primary contact.", "danger");
      });
  }

  function bindEvents() {
    var addButton = document.getElementById("btnAddContact");
    var form = document.getElementById("contactForm");
    var typeFilter = document.getElementById("filterContactType");
    var searchInput = document.getElementById("searchContacts");
    var confirmDeleteButton = document.getElementById("confirmDeleteContact");
    var tableBody = document.getElementById("contactsTableBody");
    var contactModalElement = document.getElementById("contactModal");
    var deleteModalElement = document.getElementById("deleteContactModal");

    if (contactModalElement) {
      contactModal = new bootstrap.Modal(contactModalElement);
      contactModalElement.addEventListener("hidden.bs.modal", resetContactForm);
    }

    if (deleteModalElement) {
      deleteModal = new bootstrap.Modal(deleteModalElement);
    }

    if (addButton) {
      addButton.addEventListener("click", openCreateModal);
    }

    if (form) {
      form.addEventListener("submit", submitContactForm);
    }

    if (typeFilter) {
      typeFilter.addEventListener("change", loadContacts);
    }

    if (searchInput) {
      searchInput.addEventListener("input", function () {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(loadContacts, 300);
      });
    }

    if (confirmDeleteButton) {
      confirmDeleteButton.addEventListener("click", function () {
        if (pendingDeleteId) {
          deleteContact(pendingDeleteId);
        }
      });
    }

    if (tableBody) {
      tableBody.addEventListener("click", function (event) {
        var editButton = event.target.closest(".btn-edit-contact");
        var deleteButton = event.target.closest(".btn-delete-contact");
        var primaryButton = event.target.closest(".btn-set-primary");

        if (editButton) {
          openEditModal(editButton.getAttribute("data-id"));
          return;
        }

        if (deleteButton) {
          pendingDeleteId = deleteButton.getAttribute("data-id");
          document.getElementById("deleteContactName").textContent =
            deleteButton.getAttribute("data-name") || "this contact";
          deleteModal.show();
          return;
        }

        if (primaryButton) {
          setPrimary(primaryButton.getAttribute("data-id"));
        }
      });
    }
  }

  onReady(function () {
    initPhoneInput();
    bindEvents();
    loadContacts();
  });
})();
