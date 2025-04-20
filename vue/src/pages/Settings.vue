<template>
  <div class="container mt-5 d-flex justify-content-center">
    <div class="card shadow p-4" style="max-width: 400px; width: 100%;">
      <h2 class="text-center mb-4">Edit User Details</h2>
      <form @submit.prevent="handleSubmit">
        <div class="mb-3">
          <input
            v-model="user.oldpasswd"
            class="form-control"
            placeholder="Old Password"
            type="password"
          />
        </div>
        <div class="mb-3">
          <input
            v-model="user.newpasswd"
            class="form-control"
            placeholder="New Password"
            type="password"
          />
        </div>
        <div class="mb-3">
          <input
            v-model="user.newpasswdRepeated"
            class="form-control"
            placeholder="Repeat New Password"
            type="password"
          />
        </div>
        <button type="submit" class="btn btn-primary w-100">Save Changes</button>
      </form>
      <p v-if="error" class="text-danger text-center mt-3">{{ error }}</p>
      <div class="text-center mt-4">
        <button class="btn btn-outline-secondary w-100" @click="goToSystems">
          Back
        </button>
      </div>
    </div>
  </div>
</template>

<script>
import EditProfileForm from "./Profile/EditProfileForm";
import UserCard from "./Profile/UserCard";
import apiService from "@/services/api";
import NotificationTemplate from "./Notifications/NotificationTemplate";

export default {
  components: {
    EditProfileForm,
    UserCard,
  },

  data() {
    return {
      user: {
        oldpasswd: "",
        newpasswd: "",
        newpasswdRepeated: ""
      },
      error: "",
    };
  },
  methods: {
    async handleSubmit() {
      this.error = ""; 

      if (this.user.newpasswd !== this.user.newpasswdRepeated) {
        this.error = "New passwords do not match!";
        return;
      }

      if (this.user.oldpasswd === this.user.newpasswd) {
        this.error = "Old password and new password cannot be the same!";
        return;
      }

      if (!this.user.oldpasswd) {
        this.error = "Old password is required!";
        return;
      }
      try{
        await apiService.updateUser(this.user.oldpasswd, this.user.newpasswd);
        this.notifyVue('Details Updated!');
      }
      catch{
        this.notifyVue('Incorrect password');
      }
      
    },
    goToSystems() {
      this.$router.push({ name: "systems" });
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
  }
};
</script>

<style></style>