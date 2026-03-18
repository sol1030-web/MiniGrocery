// Global toast available immediately (before DOM ready)
window.showToast = function (message, type) {
    const bg = type === 'success' ? 'bg-success' : type === 'error' ? 'bg-danger' : 'bg-info';
    const icon = type === 'success' ? 'fa-check-circle' : type === 'error' ? 'fa-exclamation-circle' : 'fa-info-circle';
    let container = document.getElementById('toastContainer');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toastContainer';
        container.className = 'toast-container position-fixed top-0 end-0 p-3';
        container.style.zIndex = '1080';
        document.body.appendChild(container);
    }
    const toastEl = document.createElement('div');
    toastEl.className = 'toast align-items-center text-white ' + bg;
    toastEl.setAttribute('role', 'alert');
    toastEl.setAttribute('aria-live', 'assertive');
    toastEl.setAttribute('aria-atomic', 'true');
    toastEl.innerHTML = '<div class="d-flex"><div class="toast-body"><i class="fas ' + icon + ' me-2"></i>' + message + '</div><button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button></div>';
    container.appendChild(toastEl);
    const toast = new bootstrap.Toast(toastEl, { delay: 3000 });
    toast.show();
};

$(document).ready(function () {
    const ANIM_MS = 400;
    function isMobile() { return $(window).width() < 992; }
    (function setupConfirm(){
        const modalEl = document.getElementById('confirmModal');
        if (!modalEl) return;
        const modal = new bootstrap.Modal(modalEl);
        const msgEl = document.getElementById('confirmMessage');
        const okBtn = document.getElementById('confirmOk');
        let pendingAction = null;
        document.body.addEventListener('click', function(e){
            const target = e.target.closest('[data-confirm]');
            if (!target) return;
            e.preventDefault();
            const msg = target.getAttribute('data-confirm') || 'Are you sure?';
            msgEl.textContent = msg;
            pendingAction = target;
            modal.show();
        });
        okBtn.addEventListener('click', function(){
            modal.hide();
            if (!pendingAction) return;
            if (pendingAction.tagName === 'BUTTON' && pendingAction.type === 'submit') {
                const form = pendingAction.closest('form');
                if (form) form.submit();
            } else if (pendingAction.tagName === 'A') {
                const href = pendingAction.getAttribute('href');
                if (href) window.location.href = href;
            }
            pendingAction = null;
        });
    })();
    function applyLayout() {
        if (isMobile()) {
            $('#sidebar').removeClass('open').addClass('active');
            $('#content').css('margin-left', '0');
            $('#sidebarBackdrop').removeClass('show');
            $('body').removeClass('no-scroll');
        } else {
            $('#sidebar').removeClass('active open');
            $('#content').css('margin-left', '');
            $('#sidebarBackdrop').removeClass('show');
            $('body').removeClass('no-scroll');
        }
    }
    $('#sidebarCollapse').on('click', function () {
        if (isMobile()) {
            if ($('#sidebar').hasClass('open')) {
                $('#sidebar').removeClass('open').addClass('active');
                setTimeout(function () {
                    $('#sidebarBackdrop').removeClass('show');
                    $('body').removeClass('no-scroll');
                }, ANIM_MS);
            } else {
                $('#sidebar').removeClass('active').addClass('open');
                $('#sidebarBackdrop').addClass('show');
                $('body').addClass('no-scroll');
            }
            $('#content').css('margin-left', '');
        } else {
            return;
        }
    });
    $('#sidebarBackdrop').on('click', function () {
        $('#sidebar').removeClass('open').addClass('active');
        setTimeout(() => $(this).removeClass('show'), ANIM_MS);
        $('#content').css('margin-left', '0');
        setTimeout(() => $('body').removeClass('no-scroll'), ANIM_MS);
    });
    $('#sidebar').on('click', 'a', function (e) {
        const href = $(this).attr('href') || '';
        const isCollapseToggle = $(this).attr('data-bs-toggle') === 'collapse' || href.startsWith('#');
        if (isCollapseToggle) { return; }
        if (isMobile() && $('#sidebar').hasClass('open')) {
            $('#sidebar').removeClass('open').addClass('active');
            setTimeout(() => {
                $('#sidebarBackdrop').removeClass('show');
                $('body').removeClass('no-scroll');
            }, ANIM_MS);
        }
    });
    $(document).on('keydown', function (e) {
        if (e.key === 'Escape' && isMobile() && $('#sidebar').hasClass('open')) {
            $('#sidebar').removeClass('open').addClass('active');
            setTimeout(() => $('#sidebarBackdrop').removeClass('show'), ANIM_MS);
            $('#content').css('margin-left', '0');
            setTimeout(() => $('body').removeClass('no-scroll'), ANIM_MS);
        }
    });
    applyLayout();
    $(window).resize(applyLayout);

    const loginFooter = $('.footer-links');
    if (loginFooter.length) {
        loginFooter.empty();
        loginFooter.append(
            $('<p>').addClass('text-muted small mb-1').text('Need access? Contact the System Administrator.'),
            $('<p>').addClass('text-muted small mb-0').text('Accounts are created and assigned roles by authorized staff.')
        );
    }
});
