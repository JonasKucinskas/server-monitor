<template>
  <div>
    <h2>Login</h2>
    <form @submit.prevent="handleLogin">
      <input v-model="username" placeholder="Username" />
      <input v-model="password" type="password" placeholder="Password" />
      <button type="submit">Login</button>
    </form>
    <p v-if="error">{{ error }}</p>
    <div>
      <p>Don't have an account?</p>
      <button @click="goToRegister">Register</button>
    </div>
  </div>
</template>

<script>
import apiService from '@/services/api';
import { mapState, mapActions } from 'vuex';

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
      
      try{
        const response = await apiService.login(this.username, this.password)
        await this.$store.dispatch('fetchUserDetails', response.user);
        this.$router.push('/'); 
      }
      catch{

      }
    },
    goToRegister() {
      this.$router.push('/register');
    },
    ...mapActions(['fetchUserDetails']),
  }
};
</script>