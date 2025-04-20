<template>
  <div class="wrapper">
    <side-bar>
      <template slot="links">
        <sidebar-link
          :to="`/systems/${this.$route.params.systemName}/dashboard`"
          :name="$t('sidebar.dashboard')"
          icon="tim-icons icon-chart-pie-36"
        />
        <sidebar-link
          :to="`/systems/${this.$route.params.systemName}/services`"
          :name="$t('sidebar.services')"
          icon="tim-icons icon-puzzle-10"
        />
        <sidebar-link
          :to="`/systems/${this.$route.params.systemName}/networkServices`"
          :name="$t('sidebar.networkServices')"
          icon="tim-icons icon-compass-05"
        />
        <sidebar-link
          :to="`/systems/${this.$route.params.systemName}/filemanager`"
          :name="$t('sidebar.filemanager')"
          icon="tim-icons icon-bullet-list-67"
        />
        <sidebar-link
          :to="`/systems/${this.$route.params.systemName}/terminal`"
          :name="$t('sidebar.terminal')"
          icon="tim-icons icon-calendar-60"
        />
        <sidebar-link
          :to="`/systems/${this.$route.params.systemName}/notifications`"
          :name="$t('sidebar.notifications')"
          icon="tim-icons icon-bell-55"
        />
       
      </template>
    </side-bar>
    <div class="main-panel">
      <top-navbar></top-navbar>
      <dashboard-content @click.native="toggleSidebar"> </dashboard-content>
    </div>
  </div>
</template>
<style lang="scss"></style>
<script>
import TopNavbar from "./TopNavbar.vue";
import DashboardContent from "./Content.vue";
import MobileMenu from "./MobileMenu";

export default {
  components: {
    TopNavbar,
    DashboardContent,
  },
  methods: {
    toggleSidebar() {
      if (this.$sidebar.showSidebar) {
        this.$sidebar.displaySidebar(false);
      }
    },
  },
};
</script>
