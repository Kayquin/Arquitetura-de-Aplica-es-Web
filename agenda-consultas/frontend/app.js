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
const roleForm = document.getElementById("roleForm");
const roleEmailInput = document.getElementById("roleEmail");
const roleValueInput = document.getElementById("roleValue");
const consultaDataInput = document.getElementById("consultaData");
const consultaHorarioSelect = document.getElementById("consultaHorario");

let apiBase = apiBaseInput.value.trim();
let token = localStorage.getItem("jwtToken") || "";
let selectedPaciente = null;
let currentRole = "";
let currentEmail = "";
let pacientesById = new Map();

function setStatus(message, isError = false) {
  statusMessage.textContent = message;
  statusMessage.style.color = isError ? "#b91c1c" : "#2563eb";
}

function updateAuthStatus() {
  if (!token) {
    authStatus.textContent = "Sem login";
    return;
  }

  const roleText = currentRole ? ` (${currentRole})` : "";
  const emailText = currentEmail || "Logado";
  authStatus.textContent = `${emailText}${roleText}`;
}

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
  }
  if (view === "consultas") {
    if (!selectedPaciente && currentRole === "admin") {
      pacienteSelecionado.textContent = "Todas as consultas";
    }
    loadConsultas();
    loadHorarios();
  }
  if (view === "usuarios") {
    loadUsuarios();
  }
}

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
}

function applyAccessControl() {
  const isLogged = Boolean(token);
  const isAdmin = currentRole === "admin";

  navPacientes.hidden = !isLogged;
  navConsultas.hidden = !isLogged;
  navUsuarios.hidden = !isAdmin;
  navConfig.hidden = !isAdmin;

  navPacientes.disabled = !isLogged;
  navConsultas.disabled = !isLogged || (!selectedPaciente && !isAdmin);

  document.getElementById("view-pacientes").hidden = !isLogged;
  document.getElementById("view-consultas").hidden = !isLogged;
  document.getElementById("view-usuarios").hidden = !isAdmin;
  document.getElementById("view-config").hidden = !isAdmin;

  if (!isLogged) {
    setView("auth");
  } else if (document.getElementById("view-auth").classList.contains("active")) {
    setView("pacientes");
  }
}

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

function formatHorarioLabel(isoDate) {
  if (!isoDate || isoDate.length < 16) {
    return "";
  }
  return isoDate.slice(11, 16);
}

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

function renderHorarios(slots) {
  consultaHorarioSelect.innerHTML = "";
  if (!slots || slots.length === 0) {
    const option = document.createElement("option");
    option.value = "";
    option.textContent = "Sem horarios para o dia";
    option.disabled = true;
    option.selected = true;
    consultaHorarioSelect.appendChild(option);
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
    consultaHorarioSelect.appendChild(option);
  });
}

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

async function loadPacientes() {
  if (!token) {
    setStatus("Faca login para ver pacientes", true);
    return;
  }
  try {
    const pacientes = await apiFetch("/api/pacientes");
    pacientesById = new Map(pacientes.map(paciente => [paciente.id, paciente]));
    pacienteList.innerHTML = "";
    pacientes.forEach(paciente => {
      const li = document.createElement("li");
      li.innerHTML = `
        <strong>${paciente.nome}</strong>
        <span>${paciente.email}</span>
        <span>${paciente.telefone}</span>
        <div class="actions">
          <button data-id="${paciente.id}">Ver consultas</button>
        </div>
      `;
      li.querySelector("button").addEventListener("click", () => {
        selectPaciente(paciente);
      });
      pacienteList.appendChild(li);
    });
  } catch (error) {
    setStatus(error.message, true);
  }
}

async function loadConsultas() {
  if (!token) {
    setStatus("Faca login para ver consultas", true);
    return;
  }

  try {
    const isAdmin = currentRole === "admin";
    if (!selectedPaciente && isAdmin && pacientesById.size === 0) {
      const pacientes = await apiFetch("/api/pacientes");
      pacientesById = new Map(pacientes.map(paciente => [paciente.id, paciente]));
    }
    const endpoint = selectedPaciente
      ? `/api/consultas/paciente/${selectedPaciente.id}`
      : isAdmin
        ? "/api/consultas"
        : null;
    if (!endpoint) {
      return;
    }

    const consultas = await apiFetch(endpoint);
    consultaList.innerHTML = "";
    consultas.forEach(consulta => {
      const li = document.createElement("li");
      const dataFormatada = formatConsultaDate(consulta);
      const pacienteNome = !selectedPaciente
        ? (pacientesById.get(consulta.pacienteId)?.nome || consulta.pacienteId)
        : "";
      li.innerHTML = `
        <strong>${consulta.especialidade}</strong>
        <span>${dataFormatada}</span>
        <span>Status: ${consulta.status}</span>
        ${selectedPaciente ? "" : `<span>Paciente: ${pacienteNome}</span>`}
        <div class="actions">
          <button data-id="${consulta.id}">Excluir</button>
        </div>
      `;
      li.querySelector("button").addEventListener("click", async () => {
        try {
          await apiFetch(`/api/consultas/${consulta.id}`, { method: "DELETE" });
          setStatus("Consulta removida");
          await loadConsultas();
        } catch (error) {
          setStatus(error.message, true);
        }
      });
      consultaList.appendChild(li);
    });
  } catch (error) {
    setStatus(error.message, true);
  }
}

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

function selectPaciente(paciente) {
  selectedPaciente = paciente;
  pacienteSelecionado.textContent = `Paciente: ${paciente.nome}`;
  navConsultas.disabled = false;
  setView("consultas");
  loadConsultas();
  loadHorarios();
}

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

saveApiBaseButton.addEventListener("click", () => {
  apiBase = apiBaseInput.value.trim();
  setStatus("API base atualizada");
});

pacienteCpfInput.addEventListener("input", event => {
  event.target.value = formatCpf(event.target.value);
});

pacienteTelefoneInput.addEventListener("input", event => {
  event.target.value = formatPhone(event.target.value);
});

navButtons.forEach(btn => {
  btn.addEventListener("click", () => {
    if (btn.hidden || btn.disabled) {
      return;
    }
    setView(btn.dataset.view);
  });
});

const registerForm = document.getElementById("registerForm");
registerForm.addEventListener("submit", async event => {
  event.preventDefault();
  // Normaliza email/role antes de enviar.
  const body = {
    email: document.getElementById("regEmail").value.trim().toLowerCase(),
    password: document.getElementById("regPassword").value,
    role: document.getElementById("regRole").value.trim().toLowerCase()
  };
  try {
    const response = await apiFetch("/api/auth/register", {
      method: "POST",
      body: JSON.stringify(body)
    });
    token = response.token;
    localStorage.setItem("jwtToken", token);
    syncAuthFromToken();
    updateAuthStatus();
    applyAccessControl();
    setView("pacientes");
    setStatus("Conta criada e logado");
  } catch (error) {
    setStatus(error.message, true);
  }
});

const loginForm = document.getElementById("loginForm");
loginForm.addEventListener("submit", async event => {
  event.preventDefault();
  const body = {
    email: document.getElementById("loginEmail").value.trim().toLowerCase(),
    password: document.getElementById("loginPassword").value
  };
  try {
    const response = await apiFetch("/api/auth/login", {
      method: "POST",
      body: JSON.stringify(body)
    });
    token = response.token;
    localStorage.setItem("jwtToken", token);
    syncAuthFromToken();
    updateAuthStatus();
    applyAccessControl();
    setView("pacientes");
    setStatus("Login realizado");
  } catch (error) {
    setStatus(error.message, true);
  }
});

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

const pacienteForm = document.getElementById("pacienteForm");
pacienteForm.addEventListener("submit", async event => {
  event.preventDefault();
  // Envia apenas digitos para CPF/telefone.
  const body = {
    nome: document.getElementById("pacienteNome").value.trim(),
    cpf: onlyDigits(document.getElementById("pacienteCpf").value, 11),
    telefone: onlyDigits(document.getElementById("pacienteTelefone").value, 11),
    email: document.getElementById("pacienteEmail").value.trim().toLowerCase()
  };
  try {
    await apiFetch("/api/pacientes", {
      method: "POST",
      body: JSON.stringify(body)
    });
    pacienteForm.reset();
    setStatus("Paciente criado");
    await loadPacientes();
  } catch (error) {
    setStatus(error.message, true);
  }
});

const consultaForm = document.getElementById("consultaForm");
consultaForm.addEventListener("submit", async event => {
  event.preventDefault();
  if (!selectedPaciente) {
    setStatus("Selecione um paciente primeiro", true);
    return;
  }
  const selectedOption = consultaHorarioSelect.selectedOptions[0];
  if (!selectedOption || selectedOption.disabled || !selectedOption.value) {
    setStatus("Selecione um horario disponivel", true);
    return;
  }
  const body = {
    pacienteId: selectedPaciente.id,
    data: selectedOption.value,
    especialidade: document.getElementById("consultaEspecialidade").value.trim(),
    status: document.getElementById("consultaStatus").value.trim()
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
    await loadConsultas();
    await loadHorarios();
  } catch (error) {
    setStatus(error.message, true);
  }
});

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
    await loadUsuarios();
  } catch (error) {
    setStatus(error.message, true);
  }
});

syncAuthFromToken();
updateAuthStatus();
applyAccessControl();

if (consultaDataInput) {
  const hoje = new Date();
  const local = new Date(hoje.getTime() - hoje.getTimezoneOffset() * 60000);
  consultaDataInput.value = local.toISOString().slice(0, 10);
  consultaDataInput.addEventListener("change", () => {
    loadHorarios();
  });
}
