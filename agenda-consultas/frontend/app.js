// ─────────────────────────────────────────────────
// Config & estado global
// ─────────────────────────────────────────────────
let API_BASE = localStorage.getItem("apiBase") || "http://localhost:5000";
let API_VERSION = localStorage.getItem("apiVersion") || "v2";
let token = localStorage.getItem("token") || "";
let currentRole = localStorage.getItem("role") || "";
let selectedPacienteId = "";
let selectedPacienteNome = "";

function apiPath(path) {
  if (!path.startsWith("/api/")) return path;

  const normalized = path.replace(/^\/api\/+/, "/api/");

  // Se já vier versionada, mantém.
  if (/^\/api\/v\d+\//i.test(normalized)) {
    return normalized;
  }

  return normalized.replace("/api/", `/api/${API_VERSION}/`);
}

// ─────────────────────────────────────────────────
// Refs de elementos da UI
// ─────────────────────────────────────────────────
const apiBaseInput          = document.getElementById("apiBase");
const saveApiBaseButton     = document.getElementById("saveApiBase");
const authStatus            = document.getElementById("authStatus");
const authUserInfo          = document.getElementById("authUserInfo");
const statusMessage         = document.getElementById("statusMessage");
const pacienteList          = document.getElementById("pacienteList");
const consultaList          = document.getElementById("consultaList");
const usuarioList           = document.getElementById("usuarioList");
const pacienteSelecionado   = document.getElementById("pacienteSelecionado");
const navButtons            = document.querySelectorAll(".nav button");
const navAuth               = document.getElementById("navAuth");
const navPacientes          = document.getElementById("navPacientes");
const navConsultas          = document.getElementById("navConsultas");
const navUsuarios           = document.getElementById("navUsuarios");
const navConfig             = document.getElementById("navConfig");
const pacienteCpfInput      = document.getElementById("pacienteCpf");
const pacienteTelefoneInput = document.getElementById("pacienteTelefone");
const pacienteEmailInput    = document.getElementById("pacienteEmail");
const pacientePasswordInput = document.getElementById("pacientePassword");
const pacienteUsuarioGroup  = document.getElementById("pacienteUsuarioGroup");
const pacienteUsuarioSelect = document.getElementById("pacienteUsuario");
const pacienteConsultaForm  = document.getElementById("pacienteConsultaForm");
const pacienteConsultaSelecionado = document.getElementById("pacienteConsultaSelecionado");
const pacienteConsultaDataInput   = document.getElementById("pacienteConsultaData");
const pacienteConsultaHorarioSelect = document.getElementById("pacienteConsultaHorario");
const pacienteConsultaEspecialidadeInput = document.getElementById("pacienteConsultaEspecialidade");
const toastContainer        = document.getElementById("toastContainer");

// ─────────────────────────────────────────────────
// Sistema de Toast (notificacoes flutuantes)
// ─────────────────────────────────────────────────
function showToast(message, type = "success") {
  const icons = { success: "✅", error: "❌", info: "ℹ️" };
  const toast = document.createElement("div");
  toast.className = `toast toast-${type}`;
  toast.innerHTML = `<span class="toast-icon">${icons[type] || "ℹ️"}</span><span>${message}</span>`;
  toastContainer.appendChild(toast);

  // Remove o toast apos 4 segundos com animacao de saida.
  setTimeout(() => {
    toast.classList.add("toast-exit");
    toast.addEventListener("animationend", () => toast.remove(), { once: true });
  }, 4000);
}

// Mantém o statusMessage no rodape para compatibilidade.
function setStatus(msg) {
  statusMessage.textContent = msg;
}

// ─────────────────────────────────────────────────
// Dialogo de confirmacao (Promise-based)
// ─────────────────────────────────────────────────
function confirmAction(message, title = "Confirmar acao") {
  return new Promise((resolve) => {
    const modal   = document.getElementById("confirmModal");
    const titleEl = document.getElementById("confirmTitle");
    const msgEl   = document.getElementById("confirmMessage");
    const okBtn   = document.getElementById("confirmOk");
    const cancelBtn = document.getElementById("confirmCancel");

    titleEl.textContent = title;
    msgEl.textContent   = message;
    modal.hidden = false;

    function cleanup(result) {
      modal.hidden = true;
      okBtn.removeEventListener("click", onOk);
      cancelBtn.removeEventListener("click", onCancel);
      resolve(result);
    }

    function onOk()     { cleanup(true);  }
    function onCancel() { cleanup(false); }

    okBtn.addEventListener("click", onOk);
    cancelBtn.addEventListener("click", onCancel);
  });
}

// ─────────────────────────────────────────────────
// Loading states
// ─────────────────────────────────────────────────
function showLoading(id) {
  const el = document.getElementById(id);
  if (el) el.hidden = false;
}

function hideLoading(id) {
  const el = document.getElementById(id);
  if (el) el.hidden = true;
}

// ─────────────────────────────────────────────────
// Helpers de API
// ─────────────────────────────────────────────────
async function apiFetch(path, options = {}) {
  const headers = { "Content-Type": "application/json", ...(options.headers || {}) };
  if (token) headers["Authorization"] = `Bearer ${token}`;

  const res = await fetch(`${API_BASE}${apiPath(path)}`, { ...options, headers });

  if (res.status === 204) return null;

  const text = await res.text();
  let data;
  try { data = text ? JSON.parse(text) : null; } catch { data = null; }

  if (!res.ok) {
    const msg = data?.message || `Erro ${res.status}`;
    throw new Error(msg);
  }

  return data;
}

// ─────────────────────────────────────────────────
// Gestao de modais
// ─────────────────────────────────────────────────
function openModal(id) {
  const modal = document.getElementById(id);
  if (modal) modal.hidden = false;
}

function closeModal(id) {
  const modal = document.getElementById(id);
  if (modal) modal.hidden = true;
}

// Fecha modal ao clicar no botao com data-close-modal.
document.addEventListener("click", (e) => {
  const target = e.target.closest("[data-close-modal]");
  if (target) closeModal(target.dataset.closeModal);
});

// ─────────────────────────────────────────────────
// Navegacao entre views
// ─────────────────────────────────────────────────
navButtons.forEach((btn) => {
  btn.addEventListener("click", () => {
    if (btn.disabled) return;
    navButtons.forEach((b) => b.classList.remove("active"));
    btn.classList.add("active");
    document.querySelectorAll(".view").forEach((v) => v.classList.remove("active"));
    const view = document.getElementById(`view-${btn.dataset.view}`);
    if (view) view.classList.add("active");

    // Carrega dados apenas com sessao ativa.
    if (!token) return;
    if (btn.dataset.view === "pacientes") loadPacientes();
    if (btn.dataset.view === "consultas") {
      if (currentRole === "admin") {
        pacienteSelecionado.textContent = "Todas as consultas";
        pacienteConsultaForm.hidden = true;
        loadConsultas("");
      } else if (selectedPacienteId) {
        pacienteSelecionado.textContent = `Consultas de: ${selectedPacienteNome}`;
        pacienteConsultaForm.hidden = false;
        pacienteConsultaSelecionado.textContent = `Paciente: ${selectedPacienteNome}`;
        loadConsultas(selectedPacienteId);
      } else {
        consultaList.innerHTML = '<li style="color:var(--muted);padding:12px;text-align:center;font-size:14px;">Selecione um paciente na aba Pacientes.</li>';
        pacienteConsultaForm.hidden = true;
      }
    }
    if (btn.dataset.view === "usuarios" && currentRole === "admin") loadUsuarios();
  });
});

// ─────────────────────────────────────────────────
// Autenticacao
// ─────────────────────────────────────────────────
const authForm     = document.getElementById("authForm");
const authEmail    = document.getElementById("authEmail");
const authPassword = document.getElementById("authPassword");
const authSubmit   = document.getElementById("authSubmit");
const modeLogin    = document.getElementById("modeLogin");
const modeRegister = document.getElementById("modeRegister");
const authStepHint = document.getElementById("authStepHint");
const authModeInfo = document.getElementById("authModeInfo");
let authMode = "login";

function setAuthMode(mode) {
  authMode = mode;
  const isLogin = mode === "login";

  modeLogin.classList.toggle("active", isLogin);
  modeLogin.setAttribute("aria-pressed", String(isLogin));
  modeRegister.classList.toggle("active", !isLogin);
  modeRegister.setAttribute("aria-pressed", String(!isLogin));

  authSubmit.textContent  = isLogin ? "Entrar" : "Criar conta";
  authStepHint.textContent = isLogin
    ? "Etapa 2: informe email e senha para entrar."
    : "Etapa 2: informe email e senha (min 6 chars) para criar conta.";
  authModeInfo.textContent = isLogin
    ? "Modo login: acesso rapido."
    : "Modo registro: cria conta de usuario comum.";
  authForm.dataset.mode = mode;
}

modeLogin.addEventListener("click", () => setAuthMode("login"));
modeRegister.addEventListener("click", () => setAuthMode("register"));
setAuthMode("login");

authForm.addEventListener("submit", async (e) => {
  e.preventDefault();
  try {
    const dto = { email: authEmail.value.trim(), password: authPassword.value };
    const endpoint = authMode === "login" ? "/api/auth/login" : "/api/auth/register";
    const data = await apiFetch(endpoint, { method: "POST", body: JSON.stringify(dto) });

    token = data.token;
    currentRole = data.role;
    localStorage.setItem("token", token);
    localStorage.setItem("role", currentRole);

    updateAuthUI(data);
    showToast(`Bem-vindo! Logado como ${currentRole}.`, "success");
    authPassword.value = "";
  } catch (err) {
    showToast(err.message, "error");
  }
});

function updateAuthUI(data) {
  const email = data?.email || localStorage.getItem("authEmail") || "";
  authStatus.textContent = `Logado como ${data?.role || currentRole}`;
  authUserInfo.textContent = email || "";
  authUserInfo.hidden = !email;

  if (data?.email) localStorage.setItem("authEmail", data.email);

  // Libera views baseado no role.
  navConsultas.disabled = false;
  navPacientes.disabled = false;

  if (currentRole === "admin") {
    navUsuarios.hidden = false;
    navConfig.hidden   = false;
    document.querySelectorAll("[id$='AdminActions']").forEach((el) => (el.hidden = false));
  } else {
    navUsuarios.hidden = true;
    navConfig.hidden   = true;
  }
}

document.getElementById("logout").addEventListener("click", () => {
  token = "";
  currentRole = "";
  selectedPacienteId = "";
  localStorage.removeItem("token");
  localStorage.removeItem("role");
  localStorage.removeItem("authEmail");

  authStatus.textContent = "Sem login";
  authUserInfo.hidden = true;
  navPacientes.disabled  = true;
  navConsultas.disabled  = true;
  navUsuarios.hidden = true;
  navConfig.hidden   = true;
  pacienteList.innerHTML = "";
  consultaList.innerHTML = "";
  usuarioList.innerHTML  = "";

  document.querySelectorAll("[id$='AdminActions']").forEach((el) => (el.hidden = true));
  showToast("Sessao encerrada.", "info");
});

// Restaura sessao salva ao carregar a pagina.
if (token) {
  updateAuthUI({ email: localStorage.getItem("authEmail") || "", role: currentRole });
}

// ─────────────────────────────────────────────────
// Config da API
// ─────────────────────────────────────────────────
apiBaseInput.value = API_BASE;

saveApiBaseButton.addEventListener("click", () => {
  API_BASE = apiBaseInput.value.trim().replace(/\/$/, "");
  localStorage.setItem("apiBase", API_BASE);
  showToast("URL da API salva com sucesso.", "success");
});

// ─────────────────────────────────────────────────
// Utilitarios de data / horario
// ─────────────────────────────────────────────────
function todayISO() {
  return new Date().toISOString().slice(0, 10);
}

function formatDateTime(isoString) {
  if (!isoString) return "—";
  const d = new Date(isoString);
  if (isNaN(d)) return isoString;
  return d.toLocaleString("pt-BR", { dateStyle: "short", timeStyle: "short" });
}

function formatDateOnly(isoString) {
  if (!isoString) return "—";
  const d = new Date(isoString);
  if (isNaN(d)) return isoString;
  return d.toLocaleDateString("pt-BR");
}

// ─────────────────────────────────────────────────
// Status badge HTML
// ─────────────────────────────────────────────────
function statusBadge(status) {
  const s = (status || "agendada").toLowerCase();
  const labels = { agendada: "agendada", concluida: "concluída", cancelada: "cancelada" };
  return `<span class="status-badge status-${s}">${labels[s] || s}</span>`;
}

// ─────────────────────────────────────────────────
// Slots de horario
// ─────────────────────────────────────────────────
async function loadSlots(dateStr, selectEl) {
  if (!dateStr) return;
  try {
    const slots = await apiFetch(`/api/consultas/slots?date=${dateStr}`);
    selectEl.innerHTML = "";

    if (!slots || slots.length === 0) {
      selectEl.innerHTML = '<option value="">Nenhum horario disponivel</option>';
      return;
    }

    slots.forEach((slot) => {
      const opt = document.createElement("option");
      const d = new Date(slot.data);
      const hour = d.toLocaleTimeString("pt-BR", { hour: "2-digit", minute: "2-digit" });
      opt.value = slot.data;
      opt.textContent = slot.disponivel ? hour : `${hour} (ocupado)`;
      if (!slot.disponivel) opt.disabled = true;
      selectEl.appendChild(opt);
    });
  } catch (err) {
    showToast("Erro ao carregar horarios: " + err.message, "error");
  }
}

// ─────────────────────────────────────────────────
// Pacientes
// ─────────────────────────────────────────────────
async function loadPacientes() {
  showLoading("pacienteListLoading");
  pacienteList.innerHTML = "";
  try {
    const pacientes = await apiFetch("/api/pacientes");
    hideLoading("pacienteListLoading");
    renderPacientes(pacientes || []);
  } catch (err) {
    hideLoading("pacienteListLoading");
    showToast("Erro ao carregar pacientes: " + err.message, "error");
  }
}

function renderPacientes(pacientes) {
  pacienteList.innerHTML = "";

  if (pacientes.length === 0) {
    pacienteList.innerHTML = '<li style="color:var(--muted);padding:12px;text-align:center;font-size:14px;">Nenhum paciente cadastrado.</li>';
    return;
  }

  pacientes.forEach((p) => {
    const li = document.createElement("li");
    const isAdmin = currentRole === "admin";

    li.innerHTML = `
      <div class="list-item-header">
        <span class="list-item-title">${p.nome}</span>
      </div>
      <div class="list-item-meta">
        <span>📧 ${p.email}</span>
        <span>📞 ${p.telefone || "—"}</span>
      </div>
      <div class="list-item-actions">
        <button class="btn-ver" type="button" data-id="${p.id}" data-nome="${p.nome}">Ver consultas</button>
        ${isAdmin ? `
          <button class="btn-edit" type="button" data-edit-id="${p.id}" data-edit-nome="${p.nome}" data-edit-cpf="${p.cpf || ""}" data-edit-telefone="${p.telefone || ""}" data-edit-email="${p.email}">Editar</button>
          <button class="danger" type="button" data-delete-id="${p.id}" data-delete-nome="${p.nome}">Remover</button>
        ` : ""}
      </div>
    `;

    // Ver consultas
    li.querySelector(".btn-ver").addEventListener("click", (e) => {
      const id   = e.currentTarget.dataset.id;
      const nome = e.currentTarget.dataset.nome;
      selectedPacienteId   = id;
      selectedPacienteNome = nome;
      navConsultas.click();
    });

    // Editar paciente via PATCH
    if (isAdmin) {
      li.querySelector(".btn-edit").addEventListener("click", (e) => {
        const d = e.currentTarget.dataset;
        document.getElementById("editPacienteId").value        = d.editId;
        document.getElementById("editPacienteNome").value      = "";
        document.getElementById("editPacienteCpf").value       = "";
        document.getElementById("editPacienteTelefone").value  = "";
        document.getElementById("editPacienteEmail").value     = "";
        document.getElementById("editPacienteModalTitle").textContent = `Editar: ${d.editNome}`;
        openModal("editPacienteModal");
      });

      // Remover paciente
      li.querySelector(".danger").addEventListener("click", async (e) => {
        const id   = e.currentTarget.dataset.deleteId;
        const nome = e.currentTarget.dataset.deleteNome;
        const ok = await confirmAction(`Remover o paciente "${nome}"? Esta acao nao pode ser desfeita.`, "Remover paciente");
        if (!ok) return;
        try {
          await apiFetch(`/api/pacientes/${id}`, { method: "DELETE" });
          showToast("Paciente removido.", "success");
          loadPacientes();
        } catch (err) {
          showToast("Erro ao remover: " + err.message, "error");
        }
      });
    }

    pacienteList.appendChild(li);
  });
}

// Modal: novo paciente
document.getElementById("openPacienteModal").addEventListener("click", async () => {
  document.getElementById("pacienteForm").reset();

  // Admin pode vincular a usuario existente.
  if (currentRole === "admin") {
    pacienteUsuarioGroup.hidden = false;
    try {
      const usuarios = await apiFetch("/api/usuarios");
      pacienteUsuarioSelect.innerHTML = '<option value="">Selecione um usuario cadastrado</option>';
      (usuarios || []).forEach((u) => {
        const opt = document.createElement("option");
        opt.value = u.email;
        opt.textContent = `${u.email} (${u.role})`;
        pacienteUsuarioSelect.appendChild(opt);
      });
    } catch { /* ignora erro de lista de usuarios */ }
  }

  openModal("pacienteModal");
});

// Formulario: criar paciente
document.getElementById("pacienteForm").addEventListener("submit", async (e) => {
  e.preventDefault();

  const emailSelecionado = pacienteUsuarioSelect.value;
  const dto = {
    nome:     pacienteCpfInput.closest("form").querySelector("#pacienteNome").value.trim(),
    cpf:      pacienteCpfInput.value.trim(),
    telefone: pacienteTelefoneInput.value.trim(),
    email:    emailSelecionado || pacienteEmailInput.value.trim(),
    password: pacientePasswordInput.value || undefined,
  };

  try {
    await apiFetch("/api/pacientes", { method: "POST", body: JSON.stringify(dto) });
    closeModal("pacienteModal");
    showToast("Paciente cadastrado com sucesso!", "success");
    loadPacientes();
  } catch (err) {
    showToast("Erro ao criar paciente: " + err.message, "error");
  }
});

// Formulario: editar paciente via PATCH
document.getElementById("editPacienteForm").addEventListener("submit", async (e) => {
  e.preventDefault();
  const id = document.getElementById("editPacienteId").value;

  // Monta dto apenas com campos preenchidos.
  const dto = {};
  const nome     = document.getElementById("editPacienteNome").value.trim();
  const cpf      = document.getElementById("editPacienteCpf").value.trim();
  const telefone = document.getElementById("editPacienteTelefone").value.trim();
  const email    = document.getElementById("editPacienteEmail").value.trim();

  if (nome)     dto.nome     = nome;
  if (cpf)      dto.cpf      = cpf;
  if (telefone) dto.telefone = telefone;
  if (email)    dto.email    = email;

  if (Object.keys(dto).length === 0) {
    showToast("Preencha ao menos um campo para alterar.", "info");
    return;
  }

  try {
    await apiFetch(`/api/pacientes/${id}`, { method: "PATCH", body: JSON.stringify(dto) });
    closeModal("editPacienteModal");
    showToast("Paciente atualizado com sucesso!", "success");
    loadPacientes();
  } catch (err) {
    showToast("Erro ao editar paciente: " + err.message, "error");
  }
});

// ─────────────────────────────────────────────────
// Marcar consulta (formulario de paciente)
// ─────────────────────────────────────────────────
pacienteConsultaDataInput.addEventListener("change", () => {
  loadSlots(pacienteConsultaDataInput.value, pacienteConsultaHorarioSelect);
});

// Inicializa com a data de hoje.
if (token) {
  pacienteConsultaDataInput.value = todayISO();
  pacienteConsultaDataInput.dispatchEvent(new Event("change"));
}

pacienteConsultaForm.addEventListener("submit", async (e) => {
  e.preventDefault();
  if (!selectedPacienteId) {
    showToast("Selecione um paciente primeiro.", "info");
    return;
  }

  const dto = {
    pacienteId:   selectedPacienteId,
    data:         pacienteConsultaHorarioSelect.value,
    especialidade: pacienteConsultaEspecialidadeInput.value.trim(),
    status:       "agendada",
  };

  try {
    await apiFetch("/api/consultas", { method: "POST", body: JSON.stringify(dto) });
    showToast("Consulta agendada com sucesso!", "success");
    pacienteConsultaForm.reset();
    pacienteConsultaDataInput.value = todayISO();
    loadSlots(todayISO(), pacienteConsultaHorarioSelect);
  } catch (err) {
    showToast("Erro ao agendar: " + err.message, "error");
  }
});

// ─────────────────────────────────────────────────
// Consultas
// ─────────────────────────────────────────────────
async function loadConsultas(pacienteId) {
  showLoading("consultaListLoading");
  consultaList.innerHTML = "";
  try {
    const consultas = pacienteId
      ? await apiFetch(`/api/consultas/paciente/${pacienteId}`)
      : await apiFetch("/api/consultas");
    hideLoading("consultaListLoading");
    renderConsultas(consultas || []);
  } catch (err) {
    hideLoading("consultaListLoading");
    showToast("Erro ao carregar consultas: " + err.message, "error");
  }
}

function renderConsultas(consultas) {
  consultaList.innerHTML = "";

  if (consultas.length === 0) {
    consultaList.innerHTML = '<li style="color:var(--muted);padding:12px;text-align:center;font-size:14px;">Nenhuma consulta encontrada.</li>';
    return;
  }

  const isAdmin = currentRole === "admin";

  // Ordena por data decrescente.
  consultas.sort((a, b) => new Date(b.data) - new Date(a.data));

  consultas.forEach((c) => {
    const li = document.createElement("li");
    const dataDisplay = c.dataBrasil ? formatDateTime(c.dataBrasil) : formatDateTime(c.data);

    li.innerHTML = `
      <div class="list-item-header">
        <span class="list-item-title">${c.especialidade}</span>
        ${statusBadge(c.status)}
      </div>
      <div class="list-item-meta">
        <span>🕐 ${dataDisplay}</span>
      </div>
      ${isAdmin ? `
        <div class="list-item-actions">
          <button class="btn-edit" type="button"
            data-edit-id="${c.id}"
            data-edit-especialidade="${c.especialidade}"
            data-edit-status="${c.status}">Editar</button>
          <button class="danger" type="button"
            data-delete-id="${c.id}"
            data-delete-esp="${c.especialidade}">Remover</button>
        </div>
      ` : ""}
    `;

    if (isAdmin) {
      // Editar consulta via PATCH
      li.querySelector(".btn-edit").addEventListener("click", (e) => {
        const d = e.currentTarget.dataset;
        document.getElementById("editConsultaId").value                = d.editId;
        document.getElementById("editConsultaDataOpcional").value      = "";
        document.getElementById("editConsultaEspecialidade").value     = "";
        document.getElementById("editConsultaStatusSelect").value      = "";
        document.getElementById("editConsultaHorarioOpcional").innerHTML =
          '<option value="">-- manter horario atual --</option>';
        openModal("editConsultaModal");
      });

      // Remover consulta
      li.querySelector(".danger").addEventListener("click", async (e) => {
        const id  = e.currentTarget.dataset.deleteId;
        const esp = e.currentTarget.dataset.deleteEsp;
        const ok = await confirmAction(`Remover a consulta de "${esp}"? Esta acao nao pode ser desfeita.`, "Remover consulta");
        if (!ok) return;
        try {
          await apiFetch(`/api/consultas/${id}`, { method: "DELETE" });
          showToast("Consulta removida.", "success");
          loadConsultas(selectedPacienteId);
        } catch (err) {
          showToast("Erro ao remover: " + err.message, "error");
        }
      });
    }

    consultaList.appendChild(li);
  });
}

// Modal: nova consulta
document.getElementById("openConsultaModal").addEventListener("click", () => {
  document.getElementById("consultaForm").reset();
  const dataInput    = document.getElementById("consultaData");
  const horarioSelect = document.getElementById("consultaHorario");
  dataInput.value = todayISO();
  loadSlots(todayISO(), horarioSelect);
  openModal("consultaModal");
});

document.getElementById("consultaData").addEventListener("change", (e) => {
  loadSlots(e.target.value, document.getElementById("consultaHorario"));
});

// Formulario: criar consulta
document.getElementById("consultaForm").addEventListener("submit", async (e) => {
  e.preventDefault();
  if (!selectedPacienteId) {
    showToast("Selecione um paciente na aba Pacientes primeiro.", "info");
    return;
  }

  const dto = {
    pacienteId:   selectedPacienteId,
    data:         document.getElementById("consultaHorario").value,
    especialidade: document.getElementById("consultaEspecialidade").value.trim(),
    status:       document.getElementById("consultaStatus").value,
  };

  try {
    await apiFetch("/api/consultas", { method: "POST", body: JSON.stringify(dto) });
    closeModal("consultaModal");
    showToast("Consulta criada com sucesso!", "success");
    loadConsultas(selectedPacienteId);
  } catch (err) {
    showToast("Erro ao criar consulta: " + err.message, "error");
  }
});

// Carrega slots ao mudar data no modal de editar consulta
document.getElementById("editConsultaDataOpcional").addEventListener("change", (e) => {
  const select = document.getElementById("editConsultaHorarioOpcional");
  if (e.target.value) {
    loadSlots(e.target.value, select);
  } else {
    select.innerHTML = '<option value="">-- manter horario atual --</option>';
  }
});

// Formulario: editar consulta via PATCH
document.getElementById("editConsultaForm").addEventListener("submit", async (e) => {
  e.preventDefault();
  const id = document.getElementById("editConsultaId").value;

  // Monta dto apenas com campos preenchidos.
  const dto = {};
  const horario     = document.getElementById("editConsultaHorarioOpcional").value;
  const especialidade = document.getElementById("editConsultaEspecialidade").value.trim();
  const status      = document.getElementById("editConsultaStatusSelect").value;

  if (horario)       dto.data          = horario;
  if (especialidade) dto.especialidade = especialidade;
  if (status)        dto.status        = status;

  if (Object.keys(dto).length === 0) {
    showToast("Preencha ao menos um campo para alterar.", "info");
    return;
  }

  try {
    await apiFetch(`/api/consultas/${id}`, { method: "PATCH", body: JSON.stringify(dto) });
    closeModal("editConsultaModal");
    showToast("Consulta atualizada com sucesso!", "success");
    loadConsultas(selectedPacienteId);
  } catch (err) {
    showToast("Erro ao editar consulta: " + err.message, "error");
  }
});

// ─────────────────────────────────────────────────
// Usuarios
// ─────────────────────────────────────────────────
async function loadUsuarios() {
  showLoading("usuarioListLoading");
  usuarioList.innerHTML = "";
  try {
    const usuarios = await apiFetch("/api/usuarios");
    hideLoading("usuarioListLoading");
    renderUsuarios(usuarios || []);
  } catch (err) {
    hideLoading("usuarioListLoading");
    showToast("Erro ao carregar usuarios: " + err.message, "error");
  }
}

function renderUsuarios(usuarios) {
  usuarioList.innerHTML = "";

  if (usuarios.length === 0) {
    usuarioList.innerHTML = '<li style="color:var(--muted);padding:12px;text-align:center;font-size:14px;">Nenhum usuario cadastrado.</li>';
    return;
  }

  usuarios.forEach((u) => {
    const li = document.createElement("li");
    li.innerHTML = `
      <div class="list-item-header">
        <span class="list-item-title">${u.email}</span>
        <span class="status-badge ${u.role === "admin" ? "status-concluida" : "status-agendada"}">${u.role}</span>
      </div>
      <div class="list-item-actions">
        <button class="danger" type="button" data-delete-id="${u.id}" data-delete-email="${u.email}">Remover</button>
      </div>
    `;

    li.querySelector(".danger").addEventListener("click", async (e) => {
      const id    = e.currentTarget.dataset.deleteId;
      const email = e.currentTarget.dataset.deleteEmail;
      const ok = await confirmAction(`Remover o usuario "${email}"?`, "Remover usuario");
      if (!ok) return;
      try {
        await apiFetch(`/api/usuarios/${id}`, { method: "DELETE" });
        showToast("Usuario removido.", "success");
        loadUsuarios();
      } catch (err) {
        showToast("Erro ao remover: " + err.message, "error");
      }
    });

    usuarioList.appendChild(li);
  });
}

// ─────────────────────────────────────────────────
// Modal: atualizar role
// ─────────────────────────────────────────────────
document.getElementById("openRoleModal").addEventListener("click", () => {
  document.getElementById("roleForm").reset();
  openModal("roleModal");
});

document.getElementById("roleForm").addEventListener("submit", async (e) => {
  e.preventDefault();
  const dto = {
    email: document.getElementById("roleEmail").value.trim(),
    role:  document.getElementById("roleValue").value,
  };

  try {
    await apiFetch("/api/auth/role", { method: "PUT", body: JSON.stringify(dto) });
    closeModal("roleModal");
    showToast(`Role atualizado para "${dto.role}" com sucesso!`, "success");
    loadUsuarios();
  } catch (err) {
    showToast("Erro ao atualizar role: " + err.message, "error");
  }
});

// ─────────────────────────────────────────────────
// Mascara de CPF e telefone
// ─────────────────────────────────────────────────
function maskCpf(e) {
  let v = e.target.value.replace(/\D/g, "").slice(0, 11);
  if (v.length > 9) v = v.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, "$1.$2.$3-$4");
  else if (v.length > 6) v = v.replace(/(\d{3})(\d{3})(\d{3})/, "$1.$2.$3");
  else if (v.length > 3) v = v.replace(/(\d{3})(\d{3})/, "$1.$2");
  e.target.value = v;
}

function maskTelefone(e) {
  let v = e.target.value.replace(/\D/g, "").slice(0, 11);
  if (v.length > 10) v = v.replace(/(\d{2})(\d{5})(\d{4})/, "($1) $2-$3");
  else if (v.length > 6) v = v.replace(/(\d{2})(\d{4})(\d{0,4})/, "($1) $2-$3");
  else if (v.length > 2) v = v.replace(/(\d{2})(\d{0,5})/, "($1) $2");
  e.target.value = v;
}

pacienteCpfInput.addEventListener("input", maskCpf);
pacienteTelefoneInput.addEventListener("input", maskTelefone);

// ─────────────────────────────────────────────────
// Inicializacao
// ─────────────────────────────────────────────────
// Carrega pacientes se ja estiver logado.
if (token) loadPacientes();
