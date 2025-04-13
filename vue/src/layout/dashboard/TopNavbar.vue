<template>
  <nav
    class="navbar navbar-expand-lg navbar-absolute"
    :class="{ 'bg-white': showMenu, 'navbar-transparent': !showMenu }"
  >
    <div class="container-fluid">
      <div class="navbar-wrapper">
        <div
          class="navbar-toggle d-inline"
          :class="{ toggled: $sidebar.showSidebar }"
        >
          <button
            type="button"
            class="navbar-toggler"
            aria-label="Navbar toggle button"
            @click="toggleSidebar"
          >
            <span class="navbar-toggler-bar bar1"></span>
            <span class="navbar-toggler-bar bar2"></span>
            <span class="navbar-toggler-bar bar3"></span>
          </button>
        </div>
        <a class="navbar-brand" href="/systems">Monitor</a>
      </div>
      <button
        class="navbar-toggler"
        type="button"
        @click="toggleMenu"
        data-toggle="collapse"
        data-target="#navigation"
        aria-controls="navigation-index"
        aria-label="Toggle navigation"
      >
        <span class="navbar-toggler-bar navbar-kebab"></span>
        <span class="navbar-toggler-bar navbar-kebab"></span>
        <span class="navbar-toggler-bar navbar-kebab"></span>
      </button>

      <collapse-transition>
        <div class="collapse navbar-collapse show" v-show="showMenu">
          <ul class="navbar-nav ml-auto">
            <base-dropdown
              tag="li"
              :menu-on-right="true"
              title-tag="a"
              class="nav-item"
            >
              <a
                slot="title"
                href="#"
                class="dropdown-toggle nav-link"
                data-toggle="dropdown"
                aria-expanded="true"
              >
              <div v-if="notifications.length > 0" class="notification d-none d-lg-block d-xl-block"></div>
                <i class="tim-icons icon-sound-wave"></i>
                <p class="d-lg-none">New Notifications</p>
              </a>

              <li
                v-for="(notification, index) in notifications"
                :key="index"
                class="nav-link"
              >
                <a
                  :href="`/systems/${system.name}/notifications`"
                  class="nav-item dropdown-item"
                >
                  {{ notification.resource.toUpperCase() }} Exceeded usage: {{ notification.usage }}%
                </a>
              </li>

              <li v-if="notifications.length === 0" class="nav-link">
                <a href="#" class="nav-item dropdown-item text-muted">
                  No new notifications
                </a>
              </li>
            </base-dropdown>
            <base-dropdown
              tag="li"
              :menu-on-right="true"
              title-tag="a"
              class="nav-item"
              menu-classes="dropdown-navbar"
            >
              <a
                slot="title"
                href="#"
                class="dropdown-toggle nav-link"
                data-toggle="dropdown"
                aria-expanded="true"
              >
                <div class="photo">
                  <img src="img/anime3.png" />
                </div>
                <b class="caret d-none d-lg-block d-xl-block"></b>
                <p class="d-lg-none">Log out</p>
              </a>
              <li class="nav-link">
                <a href="#" class="nav-item dropdown-item" @click="settings">Settings</a>
              </li>
              <div class="dropdown-divider"></div>
              <li class="nav-link">
                <a href="#" class="nav-item dropdown-item" @click="logout">Log out</a>
              </li>
            </base-dropdown>
          </ul>
        </div>
      </collapse-transition>
    </div>
  </nav>
</template>
<script>
import { CollapseTransition } from "vue2-transitions";
import Modal from "@/components/Modal";
import { mapState } from 'vuex';
import apiService from "@/services/api"; 

export default {
  components: {
    CollapseTransition,
    Modal,
  },
  computed: {
    routeName() {
      const { name } = this.$route;
      return this.capitalizeFirstLetter(name);
    },
    ...mapState({
      user: state => state.currentUser,
      system: state => state.currentSystem,
    })
  },
  async mounted() {
    const response = await apiService.getNotificationRules(this.system.ip, this.user.id);

    this.notificationRules = response;

    for (let i = 0; i < this.notificationRules.length; i++)
    {
      const notif_response = await apiService.getNotifications(this.notificationRules[i].id);
      this.notifications.push(...notif_response);
      console.log(notif_response);
    }
  },
  data() {
    return {
      activeNotifications: false,
      showMenu: false,
      searchModalVisible: false,
      notifications: [],
    };
  },
  methods: {
    settings() {
      window.location.href = "/settings"; 
    },
    logout() {
      apiService.logout();
    },
    capitalizeFirstLetter(string) {
      return string.charAt(0).toUpperCase() + string.slice(1);
    },
    toggleNotificationDropDown() {
      this.activeNotifications = !this.activeNotifications;
    },
    closeDropDown() {
      this.activeNotifications = false;
    },
    toggleSidebar() {
      this.$sidebar.displaySidebar(!this.$sidebar.showSidebar);
    },
    hideSidebar() {
      this.$sidebar.displaySidebar(false);
    },
    toggleMenu() {
      this.showMenu = !this.showMenu;
    },
  },
};
</script>
<style></style>
