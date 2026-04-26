"use strict";
document.addEventListener("DOMContentLoaded", () => {
    const currentPage = document.body.dataset.page;
    if (currentPage === "clientes-index") {
        const searchInput = document.querySelector("input[name='termoBusca']");
        searchInput?.addEventListener("focus", () => {
            searchInput.select();
        });
    }
});
