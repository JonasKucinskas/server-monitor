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
          icon="tim-icons icon-atom"
        />
        <sidebar-link
          :to="`/systems/${this.$route.params.systemName}/icons`"
          :name="$t('sidebar.icons')"
          icon="tim-icons icon-atom"
        />
        <sidebar-link
          :to="`/systems/${this.$route.params.systemName}/notifications`"
          :name="$t('sidebar.notifications')"
          icon="tim-icons icon-bell-55"
        />
        <sidebar-link
          :to="`/systems/${this.$route.params.systemName}/profile`"
          :name="$t('sidebar.userProfile')"
          icon="tim-icons icon-single-02"
        />
        <sidebar-link
          :to="`/systems/${this.$route.params.systemName}/typography`"
          :name="$t('sidebar.typography')"
          icon="tim-icons icon-align-center"
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
