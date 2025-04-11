<template>
  <div class="container mt-5 d-flex justify-content-center">
    <div class="card shadow p-4" style="max-width: 400px; width: 100%;">
      <h2 class="text-center mb-4">Edit User Details</h2>
      <form @submit.prevent="handleSubmit">
        <div class="mb-3">
          <input
            v-model="user.name"
            class="form-control"
            placeholder="Full Name"
            required
          />
        </div>
        <div class="mb-3">
          <input1000
            v-model="user.email"
            type="email"
            class="form-control"
            placeholder="Email"
            required
          />
        </div>
        <div class="mb-3">
          <input
            v-model="user.username"
            class="form-control"
            placeholder="Username"
            required
          />
        </div>
        <div class="mb-3">
          <input
            v-model="user.phone"
            class="form-control"
            placeholder="Phone Number"
          />
        </div>
        <div class="mb-3">
          <input
            v-model="user.address"
            class="form-control"
            placeholder="Address"
          />
        </div>
        <button type="submit" class="btn btn-primary w-100">Save Changes</button>
      </form>
      <p v-if="error" class="text-danger text-center mt-3">{{ error }}</p>
      <div class="text-center mt-4">
        <button class="btn btn-outline-secondary w-100" @click="goToProfile">
          Back to Profile
        </button>
      </div>
    </div>
  </div>
</template>
<script>
import EditProfileForm from "./Profile/EditProfileForm";
import UserCard from "./Profile/UserCard";
export default {
  components: {
    EditProfileForm,
    UserCard,
  },
  data() {
    return {
      model: {
        company: "Creative Code Inc.",
        email: "mike@email.com",
        username: "michael23",
        firstName: "Mike",
        lastName: "Andrew",
        address: "Bld Mihail Kogalniceanu, nr. 8 Bl 1, Sc 1, Ap 09",
        city: "Melbourne",
        country: "Australia",
        about:
          "Lamborghini Mercy, Your chick she so thirsty, I'm in that two seat Lambo.",
      },
      user: {
        fullName: "Mike Andrew",
        title: "Ceo/Co-Founder",
        description: `Do not be scared of the truth because we need to restart the human foundation in truth And I love you like Kanye loves Kanye I love Rick Owens’ bed design but the back is...`,
      },
    };
  },
};
</script>
<style></style>
