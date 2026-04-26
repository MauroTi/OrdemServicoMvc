type DashboardFatia = {
  rotulo: string;
  valor: number;
  cor: string;
};

type ClientesDashboard = {
  totalClientes: number;
  comEmail: number;
  comTelefone: number;
  cadastroCompleto: number;
  semContato: number;
  distribuicaoContato: DashboardFatia[];
};

document.addEventListener("DOMContentLoaded", () => {
  const app = document.querySelector<HTMLElement>("#clientes-dashboard-app");

  if (!app) {
    return;
  }

  const endpoint = app.dataset.endpoint;

  if (!endpoint) {
    renderStatus(app, "Endpoint do dashboard nao configurado.");
    return;
  }

  carregarDashboard(app, endpoint).catch(() => {
    renderStatus(app, "Nao foi possivel carregar os indicadores agora.");
  });
});

async function carregarDashboard(app: HTMLElement, endpoint: string): Promise<void> {
  renderStatus(app, "Consultando API e preparando o grafico...");

  const response = await fetch(endpoint, {
    headers: {
      Accept: "application/json"
    }
  });

  if (!response.ok) {
    throw new Error(`Falha ao carregar dashboard: ${response.status}`);
  }

  const dados = (await response.json()) as ClientesDashboard;
  renderDashboard(app, dados);
}

function renderStatus(app: HTMLElement, message: string): void {
  app.innerHTML = `<div class="dashboard-status-card">${escapeHtml(message)}</div>`;
}

function renderDashboard(app: HTMLElement, dados: ClientesDashboard): void {
  const total = dados.totalClientes;
  const fatias = dados.distribuicaoContato ?? [];

  if (total === 0 || fatias.length === 0) {
    app.innerHTML = `
      <div class="dashboard-shell">
        <div class="dashboard-empty">
          Nenhum cliente disponivel para montar o grafico.
        </div>
      </div>`;
    return;
  }

  const gradiente = buildConicGradient(fatias, total);
  const legenda = fatias
    .map((fatia) => {
      const percentual = calculatePercent(fatia.valor, total);
      return `
        <div class="dashboard-legend-item">
          <div class="dashboard-legend-label">
            <span class="dashboard-legend-swatch" style="background:${escapeAttribute(fatia.cor)}"></span>
            <span>${escapeHtml(fatia.rotulo)}</span>
          </div>
          <div class="dashboard-legend-meta">
            <span class="dashboard-legend-value">${fatia.valor}</span>
            <span class="dashboard-legend-percent">${percentual}%</span>
          </div>
        </div>`;
    })
    .join("");

  app.innerHTML = `
    <div class="dashboard-shell">
      <div class="dashboard-grid">
        ${renderCard("Total de clientes", dados.totalClientes)}
        ${renderCard("Com e-mail", dados.comEmail)}
        ${renderCard("Com telefone", dados.comTelefone)}
        ${renderCard("Cadastros completos", dados.cadastroCompleto)}
      </div>

      <div class="dashboard-card">
        <div class="dashboard-chart-layout">
          <div class="dashboard-pie-panel">
            <div class="dashboard-pie-chart" style="background:${gradiente}" aria-label="Grafico de pizza da distribuicao de contatos"></div>
            <p class="dashboard-pie-caption">
              O grafico resume como os dados de contato estao distribuidos na base atual.
            </p>
          </div>

          <div class="dashboard-legend">
            ${legenda}
          </div>
        </div>
      </div>
    </div>`;
}

function renderCard(label: string, value: number): string {
  return `
    <article class="dashboard-card">
      <span class="dashboard-card-label">${escapeHtml(label)}</span>
      <strong class="dashboard-card-value">${value}</strong>
    </article>`;
}

function buildConicGradient(fatias: DashboardFatia[], total: number): string {
  let acumulado = 0;

  const partes = fatias.map((fatia) => {
    const inicio = acumulado;
    const percentual = total === 0 ? 0 : (fatia.valor / total) * 100;
    acumulado += percentual;
    return `${fatia.cor} ${inicio.toFixed(2)}% ${acumulado.toFixed(2)}%`;
  });

  return `conic-gradient(${partes.join(", ")})`;
}

function calculatePercent(value: number, total: number): string {
  if (total === 0) {
    return "0.0";
  }

  return ((value / total) * 100).toFixed(1);
}

function escapeHtml(value: string): string {
  return value
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#39;");
}

function escapeAttribute(value: string): string {
  return escapeHtml(value);
}
