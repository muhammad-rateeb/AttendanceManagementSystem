// Attendance Management System - Custom JavaScript

// Document Ready
document.addEventListener('DOMContentLoaded', function () {
    initializeApp();
});

// Initialize Application
function initializeApp() {
    initializeTooltips();
    initializeAlertDismiss();
    initializeFormValidation();
    initializeAttendanceForm();
    initializeConfirmDialogs();
    initializeSearchFilters();
}

// Initialize Bootstrap Tooltips
function initializeTooltips() {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// Auto-dismiss alerts after 5 seconds
function initializeAlertDismiss() {
    var alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            var bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });
}

// Form Validation Enhancement
function initializeFormValidation() {
    var forms = document.querySelectorAll('.needs-validation');
    Array.prototype.slice.call(forms).forEach(function (form) {
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });
}

// Attendance Form Functionality
function initializeAttendanceForm() {
    var attendanceForm = document.getElementById('attendanceForm');
    if (attendanceForm) {
        // Select All / Deselect All functionality
        var selectAllBtn = document.getElementById('selectAllPresent');
        var selectAllAbsent = document.getElementById('selectAllAbsent');

        if (selectAllBtn) {
            selectAllBtn.addEventListener('click', function () {
                document.querySelectorAll('input[value="Present"]').forEach(function (radio) {
                    radio.checked = true;
                });
            });
        }

        if (selectAllAbsent) {
            selectAllAbsent.addEventListener('click', function () {
                document.querySelectorAll('input[value="Absent"]').forEach(function (radio) {
                    radio.checked = true;
                });
            });
        }
    }
}

// Confirmation Dialogs
function initializeConfirmDialogs() {
    var deleteButtons = document.querySelectorAll('[data-confirm]');
    deleteButtons.forEach(function (btn) {
        btn.addEventListener('click', function (e) {
            var message = this.getAttribute('data-confirm') || 'Are you sure?';
            if (!confirm(message)) {
                e.preventDefault();
                return false;
            }
        });
    });
}

// Search/Filter functionality
function initializeSearchFilters() {
    var searchInput = document.getElementById('tableSearch');
    if (searchInput) {
        searchInput.addEventListener('keyup', function () {
            var value = this.value.toLowerCase();
            var table = document.querySelector('table tbody');
            if (table) {
                var rows = table.querySelectorAll('tr');
                rows.forEach(function (row) {
                    var text = row.textContent.toLowerCase();
                    row.style.display = text.indexOf(value) > -1 ? '' : 'none';
                });
            }
        });
    }
}

// Mark All Present Helper
function markAllPresent() {
    document.querySelectorAll('select[name$=".Status"]').forEach(function (select) {
        select.value = 'Present';
    });
}

// Mark All Absent Helper
function markAllAbsent() {
    document.querySelectorAll('select[name$=".Status"]').forEach(function (select) {
        select.value = 'Absent';
    });
}

// Date validation for report filters
function validateDateRange() {
    var startDate = document.getElementById('StartDate');
    var endDate = document.getElementById('EndDate');

    if (startDate && endDate && startDate.value && endDate.value) {
        if (new Date(startDate.value) > new Date(endDate.value)) {
            alert('Start date cannot be after end date');
            return false;
        }
    }
    return true;
}

// Loading state for buttons
function showLoading(button) {
    button.disabled = true;
    button.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Loading...';
}

// Reset loading state
function hideLoading(button, originalText) {
    button.disabled = false;
    button.innerHTML = originalText;
}

// Export to Print
function printReport() {
    window.print();
}

// Toggle password visibility
function togglePassword(inputId) {
    var input = document.getElementById(inputId);
    var icon = document.querySelector('[data-password-toggle="' + inputId + '"]');

    if (input.type === 'password') {
        input.type = 'text';
        if (icon) icon.classList.replace('bi-eye', 'bi-eye-slash');
    } else {
        input.type = 'password';
        if (icon) icon.classList.replace('bi-eye-slash', 'bi-eye');
    }
}

// Format date for display
function formatDate(dateString) {
    var options = { year: 'numeric', month: 'short', day: 'numeric' };
    return new Date(dateString).toLocaleDateString('en-US', options);
}

// Calculate attendance percentage
function calculateAttendancePercentage(present, total) {
    if (total === 0) return 0;
    return Math.round((present / total) * 100);
}

// Show success toast
function showToast(message, type) {
    var toastContainer = document.getElementById('toastContainer');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toastContainer';
        toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        document.body.appendChild(toastContainer);
    }

    var toastHtml = `
        <div class="toast align-items-center text-white bg-${type || 'primary'}" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;

    toastContainer.insertAdjacentHTML('beforeend', toastHtml);
    var toast = toastContainer.lastElementChild;
    var bsToast = new bootstrap.Toast(toast);
    bsToast.show();

    toast.addEventListener('hidden.bs.toast', function () {
        toast.remove();
    });
}

// Debounce function for search
function debounce(func, wait) {
    var timeout;
    return function executedFunction() {
        var context = this;
        var args = arguments;
        var later = function () {
            timeout = null;
            func.apply(context, args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// AJAX helper function
function ajaxRequest(url, method, data, successCallback, errorCallback) {
    var xhr = new XMLHttpRequest();
    xhr.open(method, url, true);
    xhr.setRequestHeader('Content-Type', 'application/json');

    // Get anti-forgery token if available
    var token = document.querySelector('input[name="__RequestVerificationToken"]');
    if (token) {
        xhr.setRequestHeader('RequestVerificationToken', token.value);
    }

    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4) {
            if (xhr.status >= 200 && xhr.status < 300) {
                if (successCallback) {
                    successCallback(JSON.parse(xhr.responseText));
                }
            } else {
                if (errorCallback) {
                    errorCallback(xhr.status, xhr.responseText);
                }
            }
        }
    };

    xhr.send(data ? JSON.stringify(data) : null);
}

// Console log for debugging
console.log('Attendance Management System initialized successfully.');
