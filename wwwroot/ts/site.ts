document.addEventListener("DOMContentLoaded", () => {
  const currentPage = document.body.dataset.page;

  if (currentPage === "clientes-index") {
    const searchInput = document.querySelector<HTMLInputElement>("input[name='termoBusca']");
    searchInput?.addEventListener("focus", () => {
      searchInput.select();
    });
  }
});
