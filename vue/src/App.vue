<template>
  <div>
    <notifications></notifications>
    <router-view :key="$route.fullPath"></router-view>
  </div>
</template>

<script>
export default {
  computed: {
    noSidebar() {
      return this.$route.name === 'systems';
    }
  },
  methods: {
    toggleNavOpen() {
      let root = document.getElementsByTagName("html")[0];
      root.classList.toggle("nav-open");
    },
  },
  mounted() {
    this.$watch("$sidebar.showSidebar", this.toggleNavOpen);
  },
};
</script>

<style lang="scss">

@import "../node_modules/@syncfusion/ej2-base/styles/bootstrap5.css";
@import "../node_modules/@syncfusion/ej2-icons/styles/bootstrap5.css";
@import "../node_modules/@syncfusion/ej2-inputs/styles/bootstrap5.css";
@import "../node_modules/@syncfusion/ej2-popups/styles/bootstrap5.css";
@import "../node_modules/@syncfusion/ej2-buttons/styles/bootstrap5.css";
@import "../node_modules/@syncfusion/ej2-splitbuttons/styles/bootstrap5.css";
@import "../node_modules/@syncfusion/ej2-navigations/styles/bootstrap5.css";
@import "../node_modules/@syncfusion/ej2-layouts/styles/bootstrap5.css";
@import "../node_modules/@syncfusion/ej2-grids/styles/bootstrap5.css";
@import "../node_modules/@syncfusion/ej2-vue-filemanager/styles/bootstrap5.css";

</style>
