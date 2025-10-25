<template>
  <section class="auth-container abs-center" @keyup.enter="onEnter">
    <h2 class="title">{{ titleText }}</h2>
    <div class="mode-indicator">
      <span>{{ modeText }}</span>
    </div>

    <!-- LOGIN -->
    <form
      v-if="mode === 'login'"
      @submit.prevent="login"
      autocomplete="on"
      novalidate
    >
      <div class="input-group">
        <div class="input-wrapper">
          <span class="input-icon" aria-hidden="true">
            <svg viewBox="0 0 24 24">
              <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
              <circle cx="12" cy="7" r="4" />
            </svg>
          </span>
          <input
            v-model.trim="loginForm.username"
            type="text"
            inputmode="text"
            placeholder="Имя пользователя"
            autocomplete="username"
            required
          />
        </div>
      </div>

      <div class="input-group">
        <div class="input-wrapper">
          <span class="input-icon" aria-hidden="true">
            <svg viewBox="0 0 24 24">
              <rect x="3" y="11" width="18" height="11" rx="2" ry="2" />
              <path d="M7 11V7a5 5 0 0 1 10 0v4" />
            </svg>
          </span>
          <input
            v-model="loginForm.password"
            type="password"
            placeholder="Пароль"
            autocomplete="current-password"
            required
          />
        </div>
      </div>

      <button
        type="button"
        class="forgot-password"
        @click="switchMode('forgot')"
      >
        Забыли пароль?
      </button>

      <div class="buttons">
        <button type="submit" class="btn-primary">Войти</button>
        <button
          type="button"
          class="btn-secondary"
          @click="switchMode('register')"
        >
          Перейти к регистрации
        </button>
      </div>
    </form>

    <!-- REGISTER -->
    <form v-else-if="mode === 'register'" @submit.prevent="register" novalidate>
      <div class="input-group">
        <div class="input-wrapper">
          <span class="input-icon" aria-hidden="true">
            <svg viewBox="0 0 24 24">
              <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
              <circle cx="12" cy="7" r="4" />
            </svg>
          </span>
          <input
            v-model.trim="registerForm.username"
            type="text"
            placeholder="Имя пользователя"
            required
          />
        </div>
      </div>

      <div class="input-group">
        <div class="input-wrapper">
          <span class="input-icon" aria-hidden="true">
            <svg viewBox="0 0 24 24">
              <rect x="3" y="11" width="18" height="11" rx="2" ry="2" />
              <path d="M7 11V7a5 5 0 0 1 10 0v4" />
            </svg>
          </span>
          <input
            v-model="registerForm.password"
            type="password"
            placeholder="Пароль"
            required
          />
        </div>
      </div>

      <div class="input-group">
        <div class="input-wrapper">
          <span class="input-icon" aria-hidden="true">
            <svg viewBox="0 0 24 24">
              <path
                d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"
              />
              <polyline points="22,6 12,13 2,6" />
            </svg>
          </span>
          <input
            v-model.trim="registerForm.email"
            type="email"
            placeholder="Email"
            autocomplete="email"
            required
          />
        </div>
      </div>

      <div class="buttons">
        <button type="submit" class="btn-primary">Зарегистрироваться</button>
        <button
          type="button"
          class="btn-secondary"
          @click="switchMode('login')"
        >
          Назад к входу
        </button>
      </div>
    </form>

    <!-- FORGOT -->
    <form v-else @submit.prevent="sendResetCode" novalidate>
      <div class="input-group">
        <div class="input-wrapper">
          <span class="input-icon" aria-hidden="true">
            <svg viewBox="0 0 24 24">
              <path
                d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"
              />
              <polyline points="22,6 12,13 2,6" />
            </svg>
          </span>
          <input
            v-model.trim="forgotForm.email"
            type="email"
            placeholder="Email"
            autocomplete="email"
            required
            :disabled="auth.forgotStep !== 1"
          />
        </div>
      </div>
      <div class="input-group">
        <div v-if="auth.forgotStep !== 1" class="input-wrapper">
          <span class="input-icon" aria-hidden="true">
            <svg viewBox="0 0 24 24">
              <path
                d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"
              />
              <polyline points="22,6 12,13 2,6" />
            </svg>
          </span>
          <input
            v-model.trim="forgotForm.code"
            type="password"
            placeholder="Код подтверждения"
            required
          />
        </div>
      </div>
      <div class="input-group">
        <div v-if="auth.forgotStep !== 1" class="input-wrapper">
          <span class="input-icon" aria-hidden="true">
            <svg viewBox="0 0 24 24">
              <path
                d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"
              />
              <polyline points="22,6 12,13 2,6" />
            </svg>
          </span>
          <input
            v-model.trim="forgotForm.newPassword"
            type="password"
            placeholder="Новый пароль"
            required
          />
        </div>
      </div>

      <div class="buttons">
        <button type="submit" class="btn-primary">
          {{ auth.forgotStep !== 1 ? "Подтвердить" : "Отправить код" }}
        </button>
        <button
          type="button"
          class="btn-secondary"
          @click="switchMode('login')"
        >
          Назад
        </button>
      </div>
    </form>
  </section>
</template>

<script setup lang="ts">
import { computed, reactive, ref } from "vue";

import { useAuthStore } from "@/modules/auth";

type Mode = "login" | "register" | "forgot";

interface LoginForm {
  username: string;
  password: string;
}
interface RegisterForm {
  username: string;
  password: string;
  email: string;
}
interface ForgotForm {
  email: string;
  code: string;
  newPassword: string;
}

const auth = useAuthStore();

const mode = ref<Mode>("login");

const loginForm = reactive<LoginForm>({ username: "", password: "" });
const registerForm = reactive<RegisterForm>({
  username: "",
  password: "",
  email: "",
});
const forgotForm = reactive<ForgotForm>({
  email: "",
  code: "",
  newPassword: "",
});

const titleText = computed(() => {
  switch (mode.value) {
    case "login":
      return "Вход";
    case "register":
      return "Регистрация";
    case "forgot":
      return "Восстановление";
  }
});
const modeText = computed(() => {
  switch (mode.value) {
    case "login":
      return "Авторизация";
    case "register":
      return "Создание аккаунта";
    case "forgot":
      return "Сброс пароля";
  }
});

function switchMode(next: Mode) {
  mode.value = next;
}

function onEnter() {
  if (mode.value === "login") login();
  else if (mode.value === "register") register();
  else sendResetCode();
}

async function login() {
  if (!loginForm.username || !loginForm.password) {
    return;
  }

  auth.login = loginForm.username;
  auth.password = loginForm.password;
  auth.isLoginMode = true;
  auth.submit();
}

async function register() {
  if (!registerForm.username || !registerForm.password || !registerForm.email) {
    return;
  }

  auth.login = registerForm.username;
  auth.password = registerForm.password;
  auth.email = registerForm.email;
  auth.isLoginMode = false;
  auth.submit();
  switchMode("login");
}

async function sendResetCode() {
  if (!forgotForm.email) {
    return;
  }
  if (auth.forgotStep == 2) {
    auth.email = forgotForm.email;
    auth.code = forgotForm.code;
    auth.newPassword = forgotForm.newPassword;
    auth.resetPasswordRequest();
    return;
  }
  auth.forgotStep = 2;
  auth.email = forgotForm.email;
  auth.forgotPasswordMode = true;
  auth.sendResetCode();
}
</script>

<style scoped>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

.auth-container {
  width: 440px;
  max-width: 92%;
  padding: 40px 36px 36px;
  margin: 0 auto;
  color: #e8f3ff;
  background: linear-gradient(
      135deg,
      rgba(10, 14, 26, 0.95),
      rgba(5, 8, 15, 0.92)
    ),
    radial-gradient(
      600px 320px at 85% 0%,
      rgba(0, 255, 224, 0.08),
      transparent 70%
    );
  border: 1px solid rgba(0, 255, 224, 0.2);
  box-shadow: 0 0 40px rgba(0, 255, 224, 0.3), 0 0 80px rgba(0, 122, 255, 0.2),
    inset 0 0 0 1px rgba(255, 255, 255, 0.05), 0 25px 60px rgba(0, 0, 0, 0.7);
  overflow: hidden;
  animation: breathe 4s ease-in-out infinite;
  z-index: 1;
}

.title {
  text-align: center;
  font-weight: 900;
  font-size: 32px;
  margin-bottom: 32px;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  background: linear-gradient(135deg, #00ffe0 0%, #007aff 50%, #00ffe0 100%);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
  background-size: 200% 100%;
  animation: borderRun 6s linear infinite;
  filter: drop-shadow(0 0 20px rgba(0, 255, 224, 0.5));
  position: relative;
}
.title::after {
  content: "";
  position: absolute;
  bottom: -12px;
  left: 50%;
  transform: translateX(-50%);
  width: 60px;
  height: 3px;
  background: linear-gradient(90deg, transparent, #00ffe0, transparent);
  box-shadow: 0 0 10px rgba(0, 255, 224, 0.8);
}

.mode-indicator {
  text-align: center;
  margin-bottom: 24px;
  font-size: 13px;
  color: #5a6f88;
  animation: fadeInUp 0.6s ease-out both 0.05s;
}
.mode-indicator span {
  color: #00ffe0;
  font-weight: 700;
}

.input-group {
  margin-bottom: 20px;
  animation: fadeInUp 0.6s ease-out both;
}
.input-wrapper {
  position: relative;
}
input {
  width: 100%;
  padding: 16px 20px 16px 50px;
  background: rgba(10, 14, 26, 0.6);
  border: 1px solid rgba(0, 255, 224, 0.2);
  border-radius: 14px;
  color: #e8f3ff;
  font-size: 15px;
  font-weight: 500;
  transition: all 0.3s ease;
}
input:focus {
  outline: none;
  border-color: #00ffe0;
  background: rgba(10, 14, 26, 0.8);
  box-shadow: 0 0 0 3px rgba(0, 255, 224, 0.1), 0 0 20px rgba(0, 255, 224, 0.2),
    inset 0 0 20px rgba(0, 255, 224, 0.05);
}
input::placeholder {
  color: #5a6f88;
}
.input-icon {
  position: absolute;
  left: 18px;
  top: 50%;
  transform: translateY(-50%);
  color: #00ffe0;
  pointer-events: none;
  width: 20px;
  height: 20px;
}
.input-icon svg {
  width: 100%;
  height: 100%;
  fill: none;
  stroke: currentColor;
  stroke-width: 2;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.buttons {
  display: grid;
  gap: 14px;
  margin-top: 28px;
  animation: fadeInUp 0.6s ease-out both 0.4s;
}
button {
  width: 100%;
  padding: 16px 24px;
  font-size: 15px;
  font-weight: 900;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  border-radius: 14px;
  cursor: pointer;
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
  border: none;
}
button::before {
  content: "";
  position: absolute;
  inset: 0;
  background: linear-gradient(
    110deg,
    transparent 30%,
    rgba(255, 255, 255, 0.3) 50%,
    transparent 70%
  );
  transform: translateX(-100%);
  transition: transform 0.6s ease;
}
button:hover::before {
  transform: translateX(100%);
}

.btn-primary {
  background: linear-gradient(
    135deg,
    rgba(0, 255, 224, 0.3),
    rgba(0, 122, 255, 0.3)
  );
  border: 2px solid #00ffe0;
  color: #00ffe0;
  box-shadow: 0 0 20px rgba(0, 255, 224, 0.4), 0 8px 24px rgba(0, 0, 0, 0.5),
    inset 0 1px 0 rgba(255, 255, 255, 0.1);
  text-shadow: 0 0 10px rgba(0, 255, 224, 0.5);
}
.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 0 30px rgba(0, 255, 224, 0.6), 0 12px 32px rgba(0, 0, 0, 0.6),
    inset 0 1px 0 rgba(255, 255, 255, 0.2);
}
.btn-primary:active {
  transform: translateY(0);
}

.btn-secondary {
  background: linear-gradient(
    135deg,
    rgba(255, 255, 255, 0.08),
    rgba(255, 255, 255, 0.04)
  );
  border: 1px solid rgba(255, 255, 255, 0.1);
  color: #e8f3ff;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.4);
}
.btn-secondary:hover {
  background: linear-gradient(
    135deg,
    rgba(255, 255, 255, 0.12),
    rgba(255, 255, 255, 0.06)
  );
  border-color: rgba(0, 255, 224, 0.3);
  transform: translateY(-2px);
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.5);
}

.forgot-password {
  background: none;
  border: none;
  width: 100%;
  text-align: right;
  color: #00ffe0;
  text-decoration: none;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  padding: 8px 0;
  margin-top: 8px;
  transition: all 0.3s ease;
  text-shadow: 0 0 10px rgba(0, 255, 224, 0.3);
  animation: fadeInUp 0.6s ease-out both 0.35s;
}
.forgot-password:hover {
  color: #00d4ff;
  text-shadow: 0 0 15px rgba(0, 255, 224, 0.6);
}

.loading {
  pointer-events: none;
  opacity: 0.6;
}
.loading::after {
  content: "";
  position: absolute;
  inset: 0;
  border-radius: 14px;
  border: 3px solid transparent;
  border-top-color: #00ffe0;
  animation: spin 1s linear infinite;
}
</style>
