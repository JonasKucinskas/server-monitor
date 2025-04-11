<template>
  <div class="container mt-5 d-flex justify-content-center">
    <div class="card shadow p-4" style="max-width: 400px; width: 100%;">
      <h2 class="text-center mb-4">Register</h2>
      <form @submit.prevent="handleRegister">
        <div class="mb-3">
          <input
            v-model="username"
            class="form-control"
            placeholder="Username"
            required
          />
        </div>
        <div class="mb-3">
          <input
            v-model="password"
            type="password"
            class="form-control"
            placeholder="Password"
            required
          />
        </div>
        <div class="mb-3">
          <input
            v-model="confirmPassword"
            type="password"
            class="form-control"
            placeholder="Confirm Password"
            required
          />
        </div>
        <button type="submit" class="btn btn-success w-100">Register</button>
      </form>
      <p v-if="error" class="text-danger text-center mt-3">{{ error }}</p>
      <div class="text-center mt-4">
        <p>Already have an account?</p>
        <button class="btn btn-outline-primary w-100" @click="goToLogin">
          Login
        </button>
      </div>
    </div>
  </div>
</template>

<script>
import apiService from '@/services/api';

export default {
  data() {
    return {
      username: '',
      password: '',
      confirmPassword: '',
      error: null
    };
  },
  methods: {
    goToLogin() {
      this.$router.push('/login');
    },
    handleRegister() {
      if (this.password !== this.confirmPassword) {
        this.error = 'Passwords do not match';
        return;
      }

      apiService.register(this.username, this.password)
        .then(() => {
          this.$router.push('/login'); 
        })
        .catch(() => {
          this.error = 'Registration failed';
        });
    }
  }
};
</script>
  