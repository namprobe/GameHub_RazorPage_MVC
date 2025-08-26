// Global Toast System
class ToastSystem {
    constructor() {
        this.container = document.getElementById('toastContainer');
        this.toastCount = 0;
    }

    show(message, type = 'info', duration = 5000) {
        const toastId = `toast-${++this.toastCount}`;
        const icon = this.getIcon(type);
        const title = this.getTitle(type);

        const toast = document.createElement('div');
        toast.className = `toast ${type}`;
        toast.id = toastId;
        toast.innerHTML = `
            <div class="toast-header">
                <i class="${icon} me-2"></i>
                <strong class="me-auto">${title}</strong>
                <button type="button" class="btn-close" onclick="toastSystem.hide('${toastId}')"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        `;

        this.container.appendChild(toast);

        // Auto hide after duration
        setTimeout(() => {
            this.hide(toastId);
        }, duration);

        return toastId;
    }

    hide(toastId) {
        const toast = document.getElementById(toastId);
        if (toast) {
            toast.classList.add('fade-out');
            setTimeout(() => {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
            }, 300);
        }
    }

    success(message, duration = 5000) {
        return this.show(message, 'success', duration);
    }

    error(message, duration = 7000) {
        return this.show(message, 'error', duration);
    }

    warning(message, duration = 6000) {
        return this.show(message, 'warning', duration);
    }

    info(message, duration = 5000) {
        return this.show(message, 'info', duration);
    }

    getIcon(type) {
        const icons = {
            success: 'fas fa-check-circle text-success',
            error: 'fas fa-exclamation-circle text-danger',
            warning: 'fas fa-exclamation-triangle text-warning',
            info: 'fas fa-info-circle text-info'
        };
        return icons[type] || icons.info;
    }

    getTitle(type) {
        const titles = {
            success: 'Success',
            error: 'Error',
            warning: 'Warning',
            info: 'Information'
        };
        return titles[type] || 'Information';
    }
}

// Initialize global toast system
const toastSystem = new ToastSystem();

// Make it available globally
window.toastSystem = toastSystem;
