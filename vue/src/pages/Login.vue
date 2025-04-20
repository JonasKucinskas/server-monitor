<template>
  <div class="container mt-5 d-flex justify-content-center">
    <div class="card shadow p-4" style="max-width: 400px; width: 100%;">
      <h2 class="text-center mb-4">Login</h2>
      <form @submit.prevent="handleLogin">
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
        <button type="submit" class="btn btn-primary w-100">Login</button>
      </form>
      <p v-if="error" class="text-danger text-center mt-3">{{ error }}</p>
      <div class="text-center mt-4">
        <p>Don't have an account?</p>
        <button class="btn btn-outline-secondary w-100" @click="goToRegister">
          Register
        </button>
      </div>
    </div>
  </div>
</template>

<script>
import apiService from '@/services/api';
import { mapState, mapActions } from 'vuex';
import NotificationTemplate from "./Notifications/NotificationTemplate";

export default {
  data() {
    return {
      username: '',
      password: '',
      error: null
    };
  },
  methods: {
    async handleLogin() {
      
      try
      {
        const response = await apiService.login(this.username, this.password)
        await this.$store.dispatch('fetchUserDetails', response.user);
        this.$router.push('/'); 
      }
      catch
      {
        this.notifyVue('Incorrect password');
      }
    
    },
    notifyVue(message) {
      this.$notify({
        component: NotificationTemplate,
        message: message,
        icon: "ticon",
        icon: "tim-icons icon-bell-55",
        horizontalAlign: 'center',
        verticalAlign: 'top',
        type: "danger",
        timeout: 3000,
      });
    },
    goToRegister() {
      this.$router.push('/register');
    },
    ...mapActions(['fetchUserDetails']),
  }
};
</script>