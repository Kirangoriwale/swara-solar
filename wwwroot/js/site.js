// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {
    var topbar = document.querySelector(".topbar-mobile");
    if (topbar) {
        var handleTopbarShadow = function () {
            if (window.scrollY > 8) {
                topbar.classList.add("scrolled");
            } else {
                topbar.classList.remove("scrolled");
            }
        };

        handleTopbarShadow();
        window.addEventListener("scroll", handleTopbarShadow, { passive: true });
    }

    var mobileNav = document.getElementById("mobileNav");
    if (mobileNav) {
        mobileNav.addEventListener("click", function (event) {
            var link = event.target.closest("a.nav-link");
            if (!link) {
                return;
            }

            var href = link.getAttribute("href");
            if (!href || href === "#" || href.startsWith("javascript:")) {
                return;
            }

            // Force navigation on mobile menu click to avoid stale offcanvas state.
            window.location.assign(href);
        });
    }
});
