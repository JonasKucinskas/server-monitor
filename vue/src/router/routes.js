import DashboardLayout from "@/layout/dashboard/DashboardLayout.vue";
import SystemsLayout from "@/layout/systems/SampleLayout.vue";

import NotFound from "@/pages/NotFoundPage.vue";

const Dashboard = () => import("@/pages/Dashboard.vue");
const Profile = () => import("@/pages/Profile.vue");
const Notifications = () => import("@/pages/Notifications.vue");
const Icons = () => import("@/pages/Icons.vue");
const Typography = () => import("@/pages/Typography.vue");
const Services = () => import("@/pages/Services.vue");
const Systems = () => import("@/pages/Systems.vue");
const NetworkServices = () => import("@/pages/NetworkServices.vue");

const routes = [
  {
    path: "/systems/:systemName",
    component: DashboardLayout,
    children: [
      { path: "dashboard", name: "dashboard", component: Dashboard },
      { path: "profile", name: "profile", component: Profile },
      { path: "notifications", name: "notifications", component: Notifications },
      { path: "icons", name: "icons", component: Icons },
      { path: "typography", name: "typography", component: Typography },
      { path: "services", name: "services", component: Services },
      { path: "networkServices", name: "networkServices", component: NetworkServices },
      { path: "", redirect: "dashboard" }
    ],
  },
  {
    path: "/",
    redirect: "/systems",  // Default path should go to systems overview
    component: SystemsLayout, 
    children: [ 
      { path: "", name: "systems", component: Systems },
    ],
  },
  { 
    path: "*", 
    component: NotFound 
  }
];

export default routes;