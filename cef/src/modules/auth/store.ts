import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useAuthStore = defineStore('auth', () => {
  // Состояния формы
  const login = ref('');
  const password = ref('');
  const email = ref('');
  const code = ref('');
  const newPassword = ref('');

  // Режимы интерфейса
  const isLoginMode = ref(true);
  const forgotPasswordMode = ref(false);
  const forgotStep = ref(1);

  // Аctions

  function toggleMode() {
    isLoginMode.value = !isLoginMode.value;
  }

  function toggleForgotPasswordMode() {
    forgotPasswordMode.value = !forgotPasswordMode.value;
    forgotStep.value = 1;
  }

  function resetForm() {
    login.value = '';
    password.value = '';
    email.value = '';
    code.value = '';
    newPassword.value = '';
    forgotStep.value = 1;
    forgotPasswordMode.value = false;
    isLoginMode.value = true;
  }

  function submit() {
    if (!login.value || !password.value) return;

    if (isLoginMode.value) {
      loginRequest();
    } else {
      if (!email.value) return;
      registerRequest();
    }
  }

  function loginRequest() {
    window.mpTrigger('Ath.Login', login.value, password.value);
  }

  function registerRequest() {
    window.mpTrigger('Auth.Register', email.value, login.value, password.value);
  }

  function sendResetCode() {
    if (!email.value) return;

    window.mpTrigger('Auth.SendResetCode', email.value);
  }

  function resetPasswordRequest() {
    if (!code.value || !newPassword.value) return;

    window.mpTrigger('Auth.ResetPassword', email.value, code.value, newPassword.value);
  }

  function goToResetPasswordStep() {
    ui.hideLoading();
    forgotStep.value = 2;
  }

  function goToLogin() {
    ui.hideLoading();

    code.value = '';
    newPassword.value = '';
    forgotStep.value = 1;
    forgotPasswordMode.value = false;
    isLoginMode.value = true;
  }

  return {
    // Состояния
    login,
    password,
    email,
    code,
    newPassword,
    isLoginMode,
    forgotPasswordMode,
    forgotStep,

    // Методы
    toggleMode,
    toggleForgotPasswordMode,
    resetForm,
    submit,
    sendResetCode,
    resetPasswordRequest,
    goToResetPasswordStep,
    goToLogin
  };
});
