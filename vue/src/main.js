import Vue from "vue";
import VueRouter from "vue-router";
import RouterPrefetch from "vue-router-prefetch";
import App from "./App";
// TIP: change to import router from "./router/starterRouter"; to start with a clean layout
import router from "./router/index";

import BlackDashboard from "./plugins/blackDashboard";
import i18n from "./i18n";
import "./registerServiceWorker";
import store from './store';

Vue.use(BlackDashboard);
Vue.use(VueRouter);
Vue.use(RouterPrefetch);
new Vue({
  store,
  router,
  i18n,
  render: (h) => h(App),
}).$mount("#app");

Vue.filter('date', function(value) {
  if (!value) return '';
  return new Date(value).toLocaleDateString('en-GB');  // Adjust the format as needed
});