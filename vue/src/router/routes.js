import DashboardLayout from "@/layout/dashboard/DashboardLayout.vue";
import SystemsLayout from "@/layout/systems/SampleLayout.vue";

import NotFound from "@/pages/NotFoundPage.vue"

const Dashboard = () => import("@/pages/Dashboard.vue");
const Profile = () => import("@/pages/Profile.vue");
const Notifications = () => import("@/pages/Notifications.vue");
const Icons = () => import("@/pages/Icons.vue");
const Typography = () => import("@/pages/Typography.vue");
const Services = () => import("@/pages/Services.vue");
const Systems = () => import("@/pages/Systems.vue");
const NetworkServices = () => import("@/pages/NetworkServices.vue");
const Terminal = () => import("@/pages/Terminal.vue");
const FileManager = () => import("@/pages/FileManager.vue");

const Register = () => import("@/pages/Register.vue");
const Login = () => import("@/pages/Login.vue");

const routes = [
  {

    path: "/systems/:systemName",
    component: DashboardLayout,
    children: [
      { path: "dashboard", name: "dashboard", component: Dashboard, meta: { requiresAuth: true }},
      { path: "profile", name: "profile", component: Profile, meta: { requiresAuth: true } },
      { path: "notifications", name: "notifications", component: Notifications, meta: { requiresAuth: true } },
      { path: "icons", name: "icons", component: Icons, meta: { requiresAuth: true } },
      { path: "typography", name: "typography", component: Typography, meta: { requiresAuth: true } },
      { path: "services", name: "services", component: Services, meta: { requiresAuth: true } },
      { path: "networkServices", name: "networkServices", component: NetworkServices, meta: { requiresAuth: true } },
      { path: "terminal", name: "terminal", component: Terminal, meta: { requiresAuth: true } },
      { path: "filemanager", name: "filemanager", component: FileManager, meta: { requiresAuth: true } },
      
      { path: "", redirect: "dashboard" }
    ],
  },
  {
    path: "/",
    redirect: "/systems",  
    component: SystemsLayout, 
    children: [ 
      { path: "", name: "systems", component: Systems },
    ],
  },
  { 
    path: "*", 
    component: NotFound 
  },
  {
    path: "/login", 
    name: "login",
    component: Login
  },
  {
    path: "/register", 
    name: "register",
    component: Register
  },
];

export default routes;