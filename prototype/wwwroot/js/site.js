$(document).ready(function () {
    const ANIM_MS = 400;
    function isMobile() {
        return $(window).width() <= 768;
    }
    function applyLayout() {
        if (isMobile()) {
            $('#sidebar').removeClass('open').addClass('active');
            $('#content').css('margin-left', '0');
            $('#sidebarBackdrop').removeClass('show');
            $('body').removeClass('no-scroll');
        } else {
            $('#sidebar').removeClass('active open');
            $('#content').css('margin-left', 'var(--sidebar-width)');
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
            $('#content').css('margin-left', '0');
        } else {
            $('#sidebar').toggleClass('active');
            if ($('#sidebar').hasClass('active')) {
                $('#content').css('margin-left', '0');
            } else {
                $('#content').css('margin-left', 'var(--sidebar-width)');
            }
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
        if (isCollapseToggle) {
            return;
        }
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
});
