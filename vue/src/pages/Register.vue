<template>
  <div>
    <h2>Register</h2>
    <form @submit.prevent="handleRegister">
      <input v-model="username" placeholder="Username" />
      <input v-model="password" type="password" placeholder="Password" />
      <input v-model="confirmPassword" type="password" placeholder="Confirm Password" />
      <button type="submit">Register</button>
    </form>
    <p v-if="error">{{ error }}</p>
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
  