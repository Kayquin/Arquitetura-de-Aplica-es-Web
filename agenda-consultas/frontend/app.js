// Referencias dos elementos da UI.
const apiBaseInput = document.getElementById("apiBase");
const saveApiBaseButton = document.getElementById("saveApiBase");
const authStatus = document.getElementById("authStatus");
const statusMessage = document.getElementById("statusMessage");
const pacienteList = document.getElementById("pacienteList");
const consultaList = document.getElementById("consultaList");
const usuarioList = document.getElementById("usuarioList");
const pacienteSelecionado = document.getElementById("pacienteSelecionado");
const navButtons = document.querySelectorAll(".nav button");
const navAuth = document.getElementById("navAuth");
const navPacientes = document.getElementById("navPacientes");
const navConsultas = document.getElementById("navConsultas");
const navUsuarios = document.getElementById("navUsuarios");
const navConfig = document.getElementById("navConfig");
const pacienteCpfInput = document.getElementById("pacienteCpf");
const pacienteTelefoneInput = document.getElementById("pacienteTelefone");
const pacienteEmailInput = document.getElementById("pacienteEmail");
const pacientePasswordInput = document.getElementById("pacientePassword");
const pacienteUsuarioGroup = document.getElementById("pacienteUsuarioGroup");
const pacienteUsuarioSelect = document.getElementById("pacienteUsuario");
const pacienteConsultaForm = document.getElementById("pacienteConsultaForm");
const pacienteConsultaSelecionado = document.getElementById("pacienteConsultaSelecionado");
const pacienteConsultaDataInput = document.getElementById("pacienteConsultaData");
const pacienteConsultaHorarioSelect = document.getElementById("pacienteConsultaHorario");
const pacienteConsultaEspecialidadeInput = document.getElementById("pacienteConsultaEspecialidade");
const pacienteForm = document.getElementById("pacienteForm");
const consultaForm = document.getElementById("consultaForm");
const pacienteAdminActions = document.getElementById("pacienteAdminActions");
const consultaAdminActions = document.getElementById("consultaAdminActions");
const usuarioAdminActions = document.getElementById("usuarioAdminActions");
const openPacienteModalButton = document.getElementById("openPacienteModal");
const openConsultaModalButton = document.getElementById("openConsultaModal");
const openRoleModalButton = document.getElementById("openRoleModal");
const pacienteModal = document.getElementById("pacienteModal");
const consultaModal = document.getElementById("consultaModal");
const roleModal = document.getElementById("roleModal");
const roleForm = document.getElementById("roleForm");
const roleEmailInput = document.getElementById("roleEmail");
const roleValueInput = document.getElementById("roleValue");
const consultaDataInput = document.getElementById("consultaData");
const consultaHorarioSelect = document.getElementById("consultaHorario");
const consultaEspecialidadeInput = document.getElementById("consultaEspecialidade");
const consultaStatusInput = document.getElementById("consultaStatus");
const authForm = document.getElementById("authForm");
const authFormTitle = document.getElementById("authFormTitle");
const authStepHint = document.getElementById("authStepHint");
const authModeInfo = document.getElementById("authModeInfo");
const authEmailInput = document.getElementById("authEmail");
const authPasswordInput = document.getElementById("authPassword");
const authSubmitButton = document.getElementById("authSubmit");
const modeLoginButton = document.getElementById("modeLogin");
const modeRegisterButton = document.getElementById("modeRegister");
const legacyRegisterForm = document.getElementById("registerForm");
const legacyLoginForm = document.getElementById("loginForm");

// Estado da aplicacao em memoria.
let apiBase = apiBaseInput.value.trim();
let token = localStorage.getItem("jwtToken") || "";
let selectedPaciente = null;
let currentRole = "";
let currentEmail = "";
let pacientesById = new Map();
let authMode = "login";
let activeModalId = "";
let statusTimerId = null;
let authRegisterFields = null;
let authNomeInput = null;
let authCpfInput = null;
let authTelefoneInput = null;
const pacienteConsultaParent = pacienteConsultaForm ? pacienteConsultaForm.parentElement : null;

// Atualiza mensagem de status no rodape.
function setStatus(message, isError = false, autoClearMs = 5000) {
  if (statusTimerId) {
    clearTimeout(statusTimerId);
    statusTimerId = null;
  }

  statusMessage.textContent = message;
  statusMessage.style.color = isError ? "#b91c1c" : "#2563eb";

  if (!message || autoClearMs <= 0) {
    return;
  }

  statusTimerId = setTimeout(() => {
    statusMessage.textContent = "";
    statusMessage.style.color = "";
    statusTimerId = null;
  }, autoClearMs);
}

// Mostra quem esta logado no card de status.
function updateAuthStatus() {
  if (!token) {
    authStatus.textContent = "Sem login";
    return;
  }

  const roleText = currentRole ? ` (${currentRole})` : "";
  const emailText = currentEmail || "Logado";
  authStatus.textContent = `${emailText}${roleText}`;
}

// Alterna entre as views principais.
function setView(view) {
  const target = document.getElementById(`view-${view}`);
  if (!target || target.hidden) {
    return;
  }

  document.querySelectorAll(".view").forEach(section => {
    section.classList.toggle("active", section.id === `view-${view}`);
  });
  navButtons.forEach(btn => {
    btn.classList.toggle("active", btn.dataset.view === view);
  });

  if (view === "pacientes") {
    loadPacientes();
    loadUsuarioOptionsForPaciente();
    loadHorariosPacienteUsuario();
  }
  if (view === "consultas") {
    if (!selectedPaciente && currentRole === "admin") {
      pacienteSelecionado.textContent = "Todas as consultas";
    } else if (currentRole !== "admin") {
      pacienteSelecionado.textContent = "Minhas consultas";
    }
    loadConsultas();
    if (currentRole === "admin") {
      loadHorarios();
    }
  }
  if (view === "usuarios") {
    loadUsuarios();
  }
}

// Faz parse do JWT para extrair claims.
function parseJwt(tokenValue) {
  try {
    const payload = tokenValue.split(".")[1];
    if (!payload) {
      return null;
    }
    const normalized = payload.replace(/-/g, "+").replace(/_/g, "/");
    const padded = normalized.padEnd(normalized.length + (4 - (normalized.length % 4)) % 4, "=");
    const json = atob(padded);
    return JSON.parse(json);
  } catch {
    return null;
  }
}

// Atualiza email/role a partir do token atual.
function syncAuthFromToken() {
  currentRole = "";
  currentEmail = "";
  if (!token) {
    return;
  }

  const payload = parseJwt(token);
  if (!payload) {
    return;
  }

  currentEmail =
    payload.email ||
    payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] ||
    payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ||
    "";

  currentRole =
    payload.role ||
    payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
    "";
  currentRole = currentRole.trim().toLowerCase();
}

// Ajusta visibilidade e permissoes do menu.
function applyAccessControl() {
  const isLogged = Boolean(token);
  const isAdmin = currentRole === "admin";
  const isUsuario = currentRole === "usuario";

  navPacientes.hidden = !isLogged;
  navConsultas.hidden = !isLogged;
  navUsuarios.hidden = !isAdmin;
  navConfig.hidden = !isAdmin;

  navPacientes.disabled = !isLogged;
  navConsultas.disabled = !isLogged || (isAdmin && !selectedPaciente);

  if (pacienteUsuarioGroup) {
    pacienteUsuarioGroup.hidden = !isAdmin;
  }
  if (pacienteAdminActions) {
    pacienteAdminActions.hidden = !isAdmin;
  }
  if (consultaAdminActions) {
    consultaAdminActions.hidden = !isAdmin;
  }
  if (usuarioAdminActions) {
    usuarioAdminActions.hidden = !isAdmin;
  }
  ensurePacienteConsultaMounted(isLogged && isUsuario);
  if (pacienteEmailInput) {
    if (isLogged && !isAdmin) {
      pacienteEmailInput.value = currentEmail;
      pacienteEmailInput.readOnly = true;
    } else {
      pacienteEmailInput.readOnly = false;
    }
  }

  document.getElementById("view-pacientes").hidden = !isLogged;
  document.getElementById("view-consultas").hidden = !isLogged;
  document.getElementById("view-usuarios").hidden = !isAdmin;
  document.getElementById("view-config").hidden = !isAdmin;

  if (!isLogged) {
    closeModal("pacienteModal");
    closeModal("consultaModal");
    closeModal("roleModal");
    setView("auth");
  } else if (document.getElementById("view-auth").classList.contains("active")) {
    setView("pacientes");
  }

  if (!isAdmin) {
    closeModal("pacienteModal");
    closeModal("consultaModal");
    closeModal("roleModal");
  }
}

function openModal(modalId) {
  const modal = document.getElementById(modalId);
  if (!modal) {
    return;
  }
  modal.hidden = false;
  activeModalId = modalId;
}

function closeModal(modalId) {
  const modal = document.getElementById(modalId);
  if (!modal) {
    return;
  }
  modal.hidden = true;
  if (activeModalId === modalId) {
    activeModalId = "";
  }
}

function todayIsoDate() {
  const hoje = new Date();
  const local = new Date(hoje.getTime() - hoje.getTimezoneOffset() * 60000);
  return local.toISOString().slice(0, 10);
}

function ensureConsultaDateValue() {
  if (!consultaDataInput) {
    return;
  }
  if (!consultaDataInput.value) {
    consultaDataInput.value = todayIsoDate();
  }
}

function ensurePacienteConsultaMounted(shouldMount) {
  if (!pacienteConsultaForm || !pacienteConsultaParent) {
    return;
  }

  if (shouldMount) {
    if (!pacienteConsultaForm.isConnected) {
      if (pacienteAdminActions && pacienteAdminActions.parentElement === pacienteConsultaParent) {
        pacienteConsultaParent.insertBefore(pacienteConsultaForm, pacienteAdminActions);
      } else {
        pacienteConsultaParent.appendChild(pacienteConsultaForm);
      }
    }
    return;
  }

  if (pacienteConsultaForm.isConnected) {
    pacienteConsultaForm.remove();
  }
}

function ensureRegisterFieldsMounted() {
  if (!authForm || !authSubmitButton || authRegisterFields) {
    return;
  }

  authRegisterFields = document.createElement("div");
  authRegisterFields.id = "authRegisterFields";
  authRegisterFields.className = "field-group";

  authNomeInput = document.createElement("input");
  authNomeInput.id = "authNome";
  authNomeInput.type = "text";
  authNomeInput.placeholder = "Nome completo";

  authCpfInput = document.createElement("input");
  authCpfInput.id = "authCpf";
  authCpfInput.type = "text";
  authCpfInput.placeholder = "CPF";
  authCpfInput.addEventListener("input", event => {
    event.target.value = formatCpf(event.target.value);
  });

  authTelefoneInput = document.createElement("input");
  authTelefoneInput.id = "authTelefone";
  authTelefoneInput.type = "text";
  authTelefoneInput.placeholder = "Telefone";
  authTelefoneInput.addEventListener("input", event => {
    event.target.value = formatPhone(event.target.value);
  });

  authRegisterFields.appendChild(authNomeInput);
  authRegisterFields.appendChild(authCpfInput);
  authRegisterFields.appendChild(authTelefoneInput);
  authForm.insertBefore(authRegisterFields, authSubmitButton);
}

function removeRegisterFieldsMounted() {
  if (authRegisterFields && authRegisterFields.parentElement) {
    authRegisterFields.parentElement.removeChild(authRegisterFields);
  }
  authRegisterFields = null;
  authNomeInput = null;
  authCpfInput = null;
  authTelefoneInput = null;
}

// Carrega usuarios para facilitar vinculacao ao criar paciente (admin).
async function loadUsuarioOptionsForPaciente() {
  if (!pacienteUsuarioSelect || currentRole !== "admin" || !token) {
    return;
  }

  try {
    const usuarios = await apiFetch("/api/usuarios");
    const previousValue = pacienteUsuarioSelect.value;
    pacienteUsuarioSelect.innerHTML = "";

    const defaultOption = document.createElement("option");
    defaultOption.value = "";
    defaultOption.textContent = "Selecione um usuario cadastrado";
    pacienteUsuarioSelect.appendChild(defaultOption);

    usuarios
      .filter(usuario => usuario.email)
      .forEach(usuario => {
        const option = document.createElement("option");
        option.value = usuario.email.toLowerCase();
        option.textContent = `${usuario.email} (${usuario.role})`;
        if (option.value === previousValue) {
          option.selected = true;
        }
        pacienteUsuarioSelect.appendChild(option);
      });
  } catch (error) {
    setStatus(error.message, true);
  }
}

// Atualiza o formulario unico entre login e cadastro.
function setAuthMode(mode) {
  if (
    !modeLoginButton ||
    !modeRegisterButton ||
    !authFormTitle ||
    !authStepHint ||
    !authSubmitButton ||
    !authPasswordInput
  ) {
    return;
  }

  authMode = mode === "register" ? "register" : "login";
  const isRegister = authMode === "register";
  if (authForm) {
    authForm.dataset.mode = isRegister ? "register" : "login";
  }

  modeLoginButton.classList.toggle("active", !isRegister);
  modeRegisterButton.classList.toggle("active", isRegister);
  modeLoginButton.setAttribute("aria-pressed", String(!isRegister));
  modeRegisterButton.setAttribute("aria-pressed", String(isRegister));
  authFormTitle.textContent = `Etapa 1: ${isRegister ? "criar conta" : "entrar"}`;
  authStepHint.textContent = isRegister
    ? "Etapa 2: informe dados da conta e do paciente."
    : "Etapa 2: informe email e senha para entrar.";
  if (authModeInfo) {
    authModeInfo.textContent = isRegister
      ? "Modo criar conta: cria usuario e paciente automaticamente."
      : "Modo login: acesso rapido.";
  }
  authSubmitButton.textContent = isRegister ? "Criar conta" : "Entrar";
  authPasswordInput.minLength = isRegister ? 6 : 0;

  if (isRegister) {
    ensureRegisterFieldsMounted();
    if (authNomeInput) authNomeInput.required = true;
    if (authCpfInput) authCpfInput.required = true;
    if (authTelefoneInput) authTelefoneInput.required = true;
  } else {
    removeRegisterFieldsMounted();
  }
}

// Helpers de formatacao de entradas.
function onlyDigits(value, maxLength) {
  // Mantem apenas digitos para validar no backend.
  const digits = value.replace(/\D/g, "");
  return typeof maxLength === "number" ? digits.slice(0, maxLength) : digits;
}

function formatCpf(value) {
  // Formata CPF como 000.000.000-00 enquanto digita.
  const digits = onlyDigits(value, 11);
  const part1 = digits.slice(0, 3);
  const part2 = digits.slice(3, 6);
  const part3 = digits.slice(6, 9);
  const part4 = digits.slice(9, 11);
  let formatted = part1;
  if (part2) formatted += `.${part2}`;
  if (part3) formatted += `.${part3}`;
  if (part4) formatted += `-${part4}`;
  return formatted;
}

function formatPhone(value) {
  // Formata telefone como (DD) 0000-0000 ou (DD) 00000-0000.
  const digits = onlyDigits(value, 11);
  const ddd = digits.slice(0, 2);
  const part1 = digits.length > 10 ? digits.slice(2, 7) : digits.slice(2, 6);
  const part2 = digits.length > 10 ? digits.slice(7, 11) : digits.slice(6, 10);
  if (!ddd) return digits;
  if (!part1) return `(${ddd}`;
  if (!part2) return `(${ddd}) ${part1}`;
  return `(${ddd}) ${part1}-${part2}`;
}

// Exibe apenas HH:mm do ISO.
function formatHorarioLabel(isoDate) {
  if (!isoDate || isoDate.length < 16) {
    return "";
  }
  return isoDate.slice(11, 16);
}

// Formata data exibida na lista de consultas.
function formatConsultaDate(consulta) {
  const valor = consulta.dataBrasil || consulta.data;
  if (!valor) {
    return "";
  }
  const data = new Date(valor);
  if (Number.isNaN(data.getTime())) {
    return valor;
  }
  return new Intl.DateTimeFormat("pt-BR", {
    dateStyle: "short",
    timeStyle: "short",
    timeZone: "America/Sao_Paulo"
  }).format(data);
}

// Renderiza select de horarios padronizados.
function renderHorarios(slots) {
  renderHorariosInSelect(consultaHorarioSelect, slots);
}

// Renderiza horarios em qualquer select de consulta.
function renderHorariosInSelect(selectEl, slots) {
  if (!selectEl) {
    return;
  }

  selectEl.innerHTML = "";
  if (!slots || slots.length === 0) {
    const option = document.createElement("option");
    option.value = "";
    option.textContent = "Sem horarios para o dia";
    option.disabled = true;
    option.selected = true;
    selectEl.appendChild(option);
    return;
  }

  let selecionado = false;
  slots.forEach(slot => {
    const option = document.createElement("option");
    option.value = slot.data;
    option.textContent = formatHorarioLabel(slot.data);
    if (!slot.disponivel) {
      option.disabled = true;
      option.textContent += " (ocupado)";
    }
    if (!selecionado && slot.disponivel) {
      option.selected = true;
      selecionado = true;
    }
    selectEl.appendChild(option);
  });
}

// Normaliza mensagens de erro da API.
function extractErrorMessage(data) {
  if (!data || typeof data !== "object") {
    return null;
  }
  if (typeof data.message === "string" && data.message.trim()) {
    return data.message;
  }
  if (typeof data.title === "string" && data.title.trim()) {
    return data.title;
  }
  if (data.errors && typeof data.errors === "object") {
    const messages = Object.values(data.errors)
      .flat()
      .filter(Boolean);
    if (messages.length) {
      return messages.join(" | ");
    }
  }
  return null;
}

// Fetch centralizado com JWT e tratamento de erros.
async function apiFetch(path, options = {}) {
  // Fetch centralizado com JSON e JWT.
  const headers = options.headers || {};
  if (options.body && !headers["Content-Type"]) {
    headers["Content-Type"] = "application/json";
  }
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  const response = await fetch(`${apiBase}${path}`, { ...options, headers });
  if (!response.ok) {
    // Tenta mostrar erros de validacao retornados pela API.
    let message = `Erro ${response.status}`;
    try {
      const contentType = response.headers.get("content-type") || "";
      if (contentType.includes("application/json")) {
        const data = await response.json();
        message = extractErrorMessage(data) || message;
      } else {
        const text = await response.text();
        if (text) {
          message = text;
        }
      }
    } catch {
      // ignore
    }
    if (response.status === 401) {
      token = "";
      localStorage.removeItem("jwtToken");
      syncAuthFromToken();
      updateAuthStatus();
      applyAccessControl();
      setView("auth");
    }
    throw new Error(message);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

// Carrega lista de pacientes e atualiza cache por id.
async function loadPacientes() {
  if (!token) {
    setStatus("Faca login para ver pacientes", true);
    return;
  }
  try {
    pacienteList.innerHTML = "";
    const pacientes = await apiFetch("/api/pacientes");
    const isAdmin = currentRole === "admin";

    if (!isAdmin) {
      selectedPaciente = pacientes.length > 0 ? pacientes[0] : null;
      if (selectedPaciente) {
        pacienteSelecionado.textContent = `Paciente: ${selectedPaciente.nome}`;
        if (pacienteConsultaSelecionado) {
          pacienteConsultaSelecionado.textContent = `Paciente: ${selectedPaciente.nome}`;
        }
        navConsultas.disabled = false;
        loadHorariosPacienteUsuario();
      } else {
        pacienteSelecionado.textContent = "Cadastre seu paciente para marcar consulta";
        if (pacienteConsultaSelecionado) {
          pacienteConsultaSelecionado.textContent = "Cadastre seu paciente para marcar consulta";
        }
        navConsultas.disabled = true;
      }
    }

    pacientesById = new Map(pacientes.map(paciente => [paciente.id, paciente]));
    pacienteList.innerHTML = "";
    pacientes.forEach(paciente => {
      const actionLabel = isAdmin ? "Ver consultas" : "Selecionar para marcar";
      const li = document.createElement("li");
      li.innerHTML = `
        <strong>${paciente.nome}</strong>
        <span>${paciente.email}</span>
        <span>${paciente.telefone}</span>
        <div class="actions">
          <button data-id="${paciente.id}">${actionLabel}</button>
        </div>
      `;
      li.querySelector("button").addEventListener("click", () => {
        selectPaciente(paciente);
      });
      pacienteList.appendChild(li);
    });
  } catch (error) {
    pacienteList.innerHTML = "";
    setStatus(error.message, true);
  }
}

// Carrega consultas do paciente selecionado ou todas (admin).
async function loadConsultas() {
  if (!token) {
    setStatus("Faca login para ver consultas", true);
    return;
  }

  try {
    const isAdmin = currentRole === "admin";
    consultaList.innerHTML = "";
    if (!selectedPaciente && isAdmin && pacientesById.size === 0) {
      const pacientes = await apiFetch("/api/pacientes");
      pacientesById = new Map(pacientes.map(paciente => [paciente.id, paciente]));
    }

    // Usuario comum sempre consulta pelo proprio token (evita pacienteId antigo em memoria).
    const endpoint = isAdmin
      ? (selectedPaciente ? `/api/consultas/paciente/${selectedPaciente.id}` : "/api/consultas")
      : "/api/consultas";

    if (!endpoint) {
      return;
    }

    const consultas = await apiFetch(endpoint);
    consultas.forEach(consulta => {
      const li = document.createElement("li");
      const dataFormatada = formatConsultaDate(consulta);
      const pacienteNome = !selectedPaciente
        ? (pacientesById.get(consulta.pacienteId)?.nome || consulta.pacienteId)
        : "";
      const actionHtml = isAdmin
        ? `<div class="actions"><button data-id="${consulta.id}">Excluir</button></div>`
        : "";
      li.innerHTML = `
        <strong>${consulta.especialidade}</strong>
        <span>${dataFormatada}</span>
        <span>Status: ${consulta.status}</span>
        ${selectedPaciente ? "" : `<span>Paciente: ${pacienteNome}</span>`}
        ${actionHtml}
      `;
      if (isAdmin) {
        li.querySelector("button").addEventListener("click", async () => {
          try {
            await apiFetch(`/api/consultas/${consulta.id}`, { method: "DELETE" });
            setStatus("Consulta removida");
            await loadConsultas();
          } catch (error) {
            setStatus(error.message, true);
          }
        });
      }
      consultaList.appendChild(li);
    });
  } catch (error) {
    consultaList.innerHTML = "";
    setStatus(error.message, true);
  }
}

// Carrega horarios disponiveis do dia.
async function loadHorarios() {
  if (!token) {
    setStatus("Faca login para ver consultas", true);
    return;
  }

  if (!selectedPaciente) {
    return;
  }

  if (!consultaDataInput.value) {
    return;
  }

  try {
    const slots = await apiFetch(`/api/consultas/slots?date=${encodeURIComponent(consultaDataInput.value)}`);
    renderHorarios(slots);
  } catch (error) {
    setStatus(error.message, true);
  }
}

// Seleciona paciente e abre a view de consultas.
function selectPaciente(paciente) {
  selectedPaciente = paciente;
  pacienteSelecionado.textContent = `Paciente: ${paciente.nome}`;
  if (pacienteConsultaSelecionado) {
    pacienteConsultaSelecionado.textContent = `Paciente: ${paciente.nome}`;
  }
  navConsultas.disabled = false;
  if (currentRole === "admin") {
    setView("consultas");
    return;
  }

  loadHorariosPacienteUsuario();
}

// Carrega horarios para o formulario de marcacao na aba de pacientes (usuario).
async function loadHorariosPacienteUsuario() {
  if (!token || currentRole === "admin" || !selectedPaciente || !pacienteConsultaDataInput) {
    return;
  }

  if (!pacienteConsultaDataInput.value) {
    return;
  }

  try {
    const slots = await apiFetch(`/api/consultas/slots?date=${encodeURIComponent(pacienteConsultaDataInput.value)}`);
    renderHorariosInSelect(pacienteConsultaHorarioSelect, slots);
  } catch (error) {
    setStatus(error.message, true);
  }
}

// Carrega usuarios (admin).
async function loadUsuarios() {
  if (!token) {
    setStatus("Faca login para ver usuarios", true);
    return;
  }

  try {
    const usuarios = await apiFetch("/api/usuarios");
    usuarioList.innerHTML = "";
    usuarios.forEach(usuario => {
      const li = document.createElement("li");
      li.innerHTML = `
        <strong>${usuario.email}</strong>
        <span>Role: ${usuario.role}</span>
        <span>ID: ${usuario.id}</span>
        <div class="actions">
          <button class="danger" data-id="${usuario.id}">Excluir</button>
        </div>
      `;
      li.querySelector("button").addEventListener("click", async () => {
        try {
          await apiFetch(`/api/usuarios/${usuario.id}`, { method: "DELETE" });
          setStatus("Usuario removido");
          await loadUsuarios();
        } catch (error) {
          setStatus(error.message, true);
        }
      });
      usuarioList.appendChild(li);
    });
  } catch (error) {
    setStatus(error.message, true);
  }
}

// Acao para salvar a API base.
saveApiBaseButton.addEventListener("click", () => {
  apiBase = apiBaseInput.value.trim();
  setStatus("API base atualizada");
});

if (pacienteCpfInput) {
  pacienteCpfInput.addEventListener("input", event => {
    event.target.value = formatCpf(event.target.value);
  });
}

if (pacienteTelefoneInput) {
  pacienteTelefoneInput.addEventListener("input", event => {
    event.target.value = formatPhone(event.target.value);
  });
}

if (pacienteUsuarioSelect) {
  pacienteUsuarioSelect.addEventListener("change", event => {
    const email = event.target.value;
    if (!pacienteEmailInput) {
      return;
    }

    if (email) {
      pacienteEmailInput.value = email;
      pacienteEmailInput.readOnly = true;
      if (pacientePasswordInput) {
        pacientePasswordInput.value = "";
        pacientePasswordInput.disabled = true;
      }
      return;
    }

    if (currentRole === "admin") {
      pacienteEmailInput.readOnly = false;
      pacienteEmailInput.value = "";
      if (pacientePasswordInput) {
        pacientePasswordInput.disabled = false;
      }
    }
  });
}

if (openPacienteModalButton) {
  openPacienteModalButton.addEventListener("click", async () => {
    if (pacienteForm) {
      pacienteForm.reset();
    }
    if (pacienteEmailInput) {
      pacienteEmailInput.readOnly = false;
      pacienteEmailInput.value = "";
    }
    if (pacientePasswordInput) {
      pacientePasswordInput.disabled = false;
      pacientePasswordInput.value = "";
    }
    await loadUsuarioOptionsForPaciente();
    openModal("pacienteModal");
  });
}

if (openConsultaModalButton) {
  openConsultaModalButton.addEventListener("click", async () => {
    if (!selectedPaciente) {
      setStatus("Selecione um paciente antes de marcar consulta", true);
      return;
    }
    if (consultaForm) {
      consultaForm.reset();
    }
    ensureConsultaDateValue();
    await loadHorarios();
    openModal("consultaModal");
  });
}

if (openRoleModalButton) {
  openRoleModalButton.addEventListener("click", () => openModal("roleModal"));
}

document.querySelectorAll("[data-close-modal]").forEach(button => {
  button.addEventListener("click", () => {
    closeModal(button.getAttribute("data-close-modal"));
  });
});

document.addEventListener("keydown", event => {
  if (event.key === "Escape" && activeModalId) {
    closeModal(activeModalId);
  }
});

// Troca de abas.
navButtons.forEach(btn => {
  btn.addEventListener("click", () => {
    if (btn.hidden || btn.disabled) {
      return;
    }
    setView(btn.dataset.view);
  });
});

if (modeLoginButton) {
  modeLoginButton.addEventListener("click", () => {
    setAuthMode("login");
  });
}

if (modeRegisterButton) {
  modeRegisterButton.addEventListener("click", () => {
    setAuthMode("register");
  });
}

// Login e cadastro no mesmo formulario.
if (authForm && authEmailInput && authPasswordInput) {
  authForm.addEventListener("submit", async event => {
    event.preventDefault();
    const body = {
      email: authEmailInput.value.trim().toLowerCase(),
      password: authPasswordInput.value
    };
    const isRegister = authMode === "register";
    const endpoint = isRegister ? "/api/auth/register" : "/api/auth/login";
    if (isRegister) {
      body.role = "usuario";
      body.nome = authNomeInput ? authNomeInput.value.trim() : "";
      body.cpf = onlyDigits(authCpfInput ? authCpfInput.value : "", 11);
      body.telefone = onlyDigits(authTelefoneInput ? authTelefoneInput.value : "", 11);
    }

    try {
      const response = await apiFetch(endpoint, {
        method: "POST",
        body: JSON.stringify(body)
      });
      token = response.token;
      localStorage.setItem("jwtToken", token);
      // Evita carregar estado da sessao anterior ao trocar usuario.
      selectedPaciente = null;
      pacientesById = new Map();
      pacienteList.innerHTML = "";
      consultaList.innerHTML = "";
      usuarioList.innerHTML = "";
      pacienteSelecionado.textContent = "Selecione um paciente";
      navConsultas.disabled = true;
      syncAuthFromToken();
      updateAuthStatus();
      applyAccessControl();
      setView("pacientes");
      setStatus(isRegister ? "Conta criada e login realizado" : "Login realizado");
    } catch (error) {
      setStatus(error.message, true);
    }
  });
} else {
  // Compatibilidade com layout antigo (formularios separados).
  if (legacyRegisterForm) {
    legacyRegisterForm.addEventListener("submit", async event => {
      event.preventDefault();
      const body = {
        email: document.getElementById("regEmail")?.value.trim().toLowerCase() || "",
        password: document.getElementById("regPassword")?.value || "",
        role: "usuario"
      };
      try {
        const response = await apiFetch("/api/auth/register", {
          method: "POST",
          body: JSON.stringify(body)
        });
        token = response.token;
        localStorage.setItem("jwtToken", token);
        selectedPaciente = null;
        pacientesById = new Map();
        pacienteList.innerHTML = "";
        consultaList.innerHTML = "";
        usuarioList.innerHTML = "";
        pacienteSelecionado.textContent = "Selecione um paciente";
        navConsultas.disabled = true;
        syncAuthFromToken();
        updateAuthStatus();
        applyAccessControl();
        setView("pacientes");
        setStatus("Conta criada e login realizado");
      } catch (error) {
        setStatus(error.message, true);
      }
    });
  }

  if (legacyLoginForm) {
    legacyLoginForm.addEventListener("submit", async event => {
      event.preventDefault();
      const body = {
        email: document.getElementById("loginEmail")?.value.trim().toLowerCase() || "",
        password: document.getElementById("loginPassword")?.value || ""
      };
      try {
        const response = await apiFetch("/api/auth/login", {
          method: "POST",
          body: JSON.stringify(body)
        });
        token = response.token;
        localStorage.setItem("jwtToken", token);
        selectedPaciente = null;
        pacientesById = new Map();
        pacienteList.innerHTML = "";
        consultaList.innerHTML = "";
        usuarioList.innerHTML = "";
        pacienteSelecionado.textContent = "Selecione um paciente";
        navConsultas.disabled = true;
        syncAuthFromToken();
        updateAuthStatus();
        applyAccessControl();
        setView("pacientes");
        setStatus("Login realizado");
      } catch (error) {
        setStatus(error.message, true);
      }
    });
  }
}

// Logout do usuario atual.
const logoutButton = document.getElementById("logout");
logoutButton.addEventListener("click", () => {
  token = "";
  localStorage.removeItem("jwtToken");
  selectedPaciente = null;
  navConsultas.disabled = true;
  syncAuthFromToken();
  updateAuthStatus();
  applyAccessControl();
  setView("auth");
  setStatus("Logout realizado");
});

// Cadastro de paciente.
if (pacienteForm) {
  pacienteForm.noValidate = true;
  pacienteForm.addEventListener("submit", async event => {
    event.preventDefault();
    const isAdmin = currentRole === "admin";
    const selectedUserEmail = pacienteUsuarioSelect?.value.trim().toLowerCase() || "";
    const typedEmail = pacienteEmailInput?.value.trim().toLowerCase() || "";
    const password = pacientePasswordInput?.value || "";
    const submitButton = pacienteForm.querySelector("button[type='submit']");
    let emailToUse = isAdmin ? (selectedUserEmail || typedEmail) : currentEmail;

    if (isAdmin && !emailToUse) {
      setStatus("Informe um email ou selecione um usuario para vincular", true, 7000);
      return;
    }

    const existingUserEmails = new Set(
      Array.from(pacienteUsuarioSelect?.options || [])
        .map(option => (option.value || "").trim().toLowerCase())
        .filter(Boolean)
    );
    const isExistingUser = isAdmin ? existingUserEmails.has(emailToUse) : true;

    const nome = document.getElementById("pacienteNome").value.trim();
    const cpf = onlyDigits(document.getElementById("pacienteCpf").value, 11);
    const telefone = onlyDigits(document.getElementById("pacienteTelefone").value, 11);

    if (!nome) {
      setStatus("Informe o nome do paciente", true, 7000);
      return;
    }
    if (cpf.length !== 11) {
      setStatus("Informe CPF com 11 digitos", true, 7000);
      return;
    }
    if (telefone.length < 10 || telefone.length > 11) {
      setStatus("Informe telefone com 10 ou 11 digitos", true, 7000);
      return;
    }
    if (!emailToUse || !emailToUse.includes("@")) {
      setStatus("Informe um email valido", true, 7000);
      return;
    }

    if (!isExistingUser && password.trim().length < 6) {
      setStatus("Para email novo, informe senha com no minimo 6 caracteres", true, 7000);
      return;
    }

    if (isAdmin && Array.from(pacientesById.values()).some(p => p.email?.trim().toLowerCase() === emailToUse)) {
      setStatus("Esse email ja possui paciente vinculado", true, 7000);
      return;
    }

    // Envia apenas digitos para CPF/telefone.
    const body = {
      nome,
      cpf,
      telefone,
      email: emailToUse,
      password: isAdmin && !isExistingUser ? password : ""
    };
    try {
      if (submitButton) {
        submitButton.disabled = true;
      }
      await apiFetch("/api/pacientes", {
        method: "POST",
        body: JSON.stringify(body)
      });
      pacienteForm.reset();
      if (pacienteEmailInput && !isAdmin) {
        pacienteEmailInput.value = currentEmail;
      }
      if (pacienteEmailInput && isAdmin) {
        pacienteEmailInput.readOnly = false;
      }
      if (isAdmin) {
        await loadUsuarioOptionsForPaciente();
      }
      setStatus("Paciente criado");
      closeModal("pacienteModal");
      await loadPacientes();
    } catch (error) {
      setStatus(error.message, true, 9000);
    } finally {
      if (submitButton) {
        submitButton.disabled = false;
      }
    }
  });
}

// Cadastro de consulta.
if (consultaForm) {
  consultaForm.noValidate = true;
  consultaForm.addEventListener("submit", async event => {
    event.preventDefault();
    const submitButton = consultaForm.querySelector("button[type='submit']");
    if (!selectedPaciente) {
      setStatus("Selecione um paciente primeiro", true, 9000);
      return;
    }
    const selectedOption = consultaHorarioSelect.selectedOptions[0];
    if (!selectedOption || selectedOption.disabled || !selectedOption.value) {
      setStatus("Selecione um horario disponivel", true, 9000);
      return;
    }
    const especialidade = consultaEspecialidadeInput?.value.trim() || "";
    if (!especialidade) {
      setStatus("Informe a especialidade", true, 9000);
      return;
    }

    if (submitButton) {
      submitButton.disabled = true;
    }

    const body = {
      pacienteId: selectedPaciente.id,
      data: selectedOption.value,
      especialidade,
      status: consultaStatusInput?.value.trim() || "agendada"
    };
    try {
      await apiFetch("/api/consultas", {
        method: "POST",
        body: JSON.stringify(body)
      });
      const dataSelecionada = consultaDataInput.value;
      consultaForm.reset();
      if (dataSelecionada) {
        consultaDataInput.value = dataSelecionada;
      }
      setStatus("Consulta criada");
      closeModal("consultaModal");
      await loadConsultas();
      await loadHorarios();
    } catch (error) {
      console.error("Falha ao criar consulta:", error);
      setStatus(`Nao foi possivel criar consulta: ${error.message}`, true, 12000);
    } finally {
      if (submitButton) {
        submitButton.disabled = false;
      }
    }
  });
}

// Marcacao de consulta para usuario na aba de pacientes.
if (pacienteConsultaForm) {
  pacienteConsultaForm.noValidate = true;
  pacienteConsultaForm.addEventListener("submit", async event => {
    event.preventDefault();
    if (currentRole === "admin") {
      return;
    }
    if (!selectedPaciente) {
      setStatus("Selecione um paciente primeiro", true);
      return;
    }
    const selectedOption = pacienteConsultaHorarioSelect?.selectedOptions?.[0];
    if (!selectedOption || selectedOption.disabled || !selectedOption.value) {
      setStatus("Selecione um horario disponivel", true);
      return;
    }

    const especialidade = pacienteConsultaEspecialidadeInput?.value.trim() || "";
    if (!especialidade) {
      setStatus("Informe a especialidade", true);
      return;
    }

    try {
      await apiFetch("/api/consultas", {
        method: "POST",
        body: JSON.stringify({
          pacienteId: selectedPaciente.id,
          data: selectedOption.value,
          especialidade,
          status: "agendada"
        })
      });
      const dataSelecionada = pacienteConsultaDataInput?.value || "";
      pacienteConsultaForm.reset();
      if (pacienteConsultaDataInput && dataSelecionada) {
        pacienteConsultaDataInput.value = dataSelecionada;
      }
      setStatus("Consulta marcada");
      await loadConsultas();
      await loadHorariosPacienteUsuario();
    } catch (error) {
      setStatus(error.message, true);
    }
  });
}

if (pacienteConsultaForm) {
  pacienteConsultaForm.addEventListener("click", () => {
    loadHorariosPacienteUsuario();
  });
}

// Atualizacao de role (admin).
roleForm.addEventListener("submit", async event => {
  event.preventDefault();
  const body = {
    email: roleEmailInput.value.trim().toLowerCase(),
    role: roleValueInput.value.trim().toLowerCase()
  };
  try {
    await apiFetch("/api/auth/role", {
      method: "PUT",
      body: JSON.stringify(body)
    });
    roleForm.reset();
    setStatus("Role atualizado");
    closeModal("roleModal");
    await loadUsuarios();
  } catch (error) {
    setStatus(error.message, true);
  }
});

// Inicializacao da interface com base no token salvo.
syncAuthFromToken();
updateAuthStatus();
applyAccessControl();
setAuthMode("login");

// Define data padrao do agendamento para hoje.
if (consultaDataInput) {
  ensureConsultaDateValue();
  consultaDataInput.addEventListener("change", () => {
    loadHorarios();
  });
}

if (pacienteConsultaDataInput) {
  const hoje = new Date();
  const local = new Date(hoje.getTime() - hoje.getTimezoneOffset() * 60000);
  pacienteConsultaDataInput.value = local.toISOString().slice(0, 10);
  pacienteConsultaDataInput.addEventListener("change", () => {
    loadHorariosPacienteUsuario();
  });
}
