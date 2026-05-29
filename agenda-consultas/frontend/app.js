const apiBaseInput = document.getElementById("apiBase");
const saveApiBaseButton = document.getElementById("saveApiBase");
const authStatus = document.getElementById("authStatus");
const statusMessage = document.getElementById("statusMessage");
const pacienteList = document.getElementById("pacienteList");
const consultaList = document.getElementById("consultaList");
const pacienteSelecionado = document.getElementById("pacienteSelecionado");
const navButtons = document.querySelectorAll(".nav button");
const navConsultas = document.getElementById("navConsultas");
const pacienteCpfInput = document.getElementById("pacienteCpf");
const pacienteTelefoneInput = document.getElementById("pacienteTelefone");

let apiBase = apiBaseInput.value.trim();
let token = localStorage.getItem("jwtToken") || "";
let selectedPaciente = null;

function setStatus(message, isError = false) {
  statusMessage.textContent = message;
  statusMessage.style.color = isError ? "#b91c1c" : "#2563eb";
}

function updateAuthStatus() {
  authStatus.textContent = token ? "Logado" : "Sem login";
}

function setView(view) {
  document.querySelectorAll(".view").forEach(section => {
    section.classList.toggle("active", section.id === `view-${view}`);
  });
  navButtons.forEach(btn => {
    btn.classList.toggle("active", btn.dataset.view === view);
  });
}

function onlyDigits(value, maxLength) {
  const digits = value.replace(/\D/g, "");
  return typeof maxLength === "number" ? digits.slice(0, maxLength) : digits;
}

function formatCpf(value) {
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
  const digits = onlyDigits(value, 11);
  const ddd = digits.slice(0, 2);
  const part1 = digits.length > 10 ? digits.slice(2, 7) : digits.slice(2, 6);
  const part2 = digits.length > 10 ? digits.slice(7, 11) : digits.slice(6, 10);
  if (!ddd) return digits;
  if (!part1) return `(${ddd}`;
  if (!part2) return `(${ddd}) ${part1}`;
  return `(${ddd}) ${part1}-${part2}`;
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
  const headers = options.headers || {};
  if (options.body && !headers["Content-Type"]) {
    headers["Content-Type"] = "application/json";
  }
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  const response = await fetch(`${apiBase}${path}`, { ...options, headers });
  if (!response.ok) {
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
    throw new Error(message);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

async function loadPacientes() {
  try {
    const pacientes = await apiFetch("/api/pacientes");
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
  if (!selectedPaciente) {
    return;
  }

  try {
    const consultas = await apiFetch(`/api/consultas/paciente/${selectedPaciente.id}`);
    consultaList.innerHTML = "";
    consultas.forEach(consulta => {
      const li = document.createElement("li");
      const dataFormatada = new Date(consulta.data).toLocaleString();
      li.innerHTML = `
        <strong>${consulta.especialidade}</strong>
        <span>${dataFormatada}</span>
        <span>Status: ${consulta.status}</span>
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

function selectPaciente(paciente) {
  selectedPaciente = paciente;
  pacienteSelecionado.textContent = `Paciente: ${paciente.nome}`;
  navConsultas.disabled = false;
  setView("consultas");
  loadConsultas();
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
  btn.addEventListener("click", () => setView(btn.dataset.view));
});

const registerForm = document.getElementById("registerForm");
registerForm.addEventListener("submit", async event => {
  event.preventDefault();
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
    updateAuthStatus();
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
    updateAuthStatus();
    setStatus("Login realizado");
  } catch (error) {
    setStatus(error.message, true);
  }
});

const logoutButton = document.getElementById("logout");
logoutButton.addEventListener("click", () => {
  token = "";
  localStorage.removeItem("jwtToken");
  updateAuthStatus();
  setStatus("Logout realizado");
});

const pacienteForm = document.getElementById("pacienteForm");
pacienteForm.addEventListener("submit", async event => {
  event.preventDefault();
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
  const body = {
    pacienteId: selectedPaciente.id,
    data: new Date(document.getElementById("consultaData").value).toISOString(),
    especialidade: document.getElementById("consultaEspecialidade").value.trim(),
    status: document.getElementById("consultaStatus").value.trim()
  };
  try {
    await apiFetch("/api/consultas", {
      method: "POST",
      body: JSON.stringify(body)
    });
    consultaForm.reset();
    setStatus("Consulta criada");
    await loadConsultas();
  } catch (error) {
    setStatus(error.message, true);
  }
});

updateAuthStatus();
loadPacientes();
