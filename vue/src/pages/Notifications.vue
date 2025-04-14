<template>
  <div>
    <div class="row">
      <div class="col-md-4">
        <card type="dashboard-header" class="p-2">
          <div class="d-flex justify-content-between align-items-center px-2">
            <div>
              <h2 class="card-title mb-1">{{ $t("notifications.rulesHeader") }}</h2>
              <p class="mb-0">{{ $t("notifications.rulesFooter") }}</p>
            </div>
            <div class="d-flex align-items-center">
              <button class="btn btn-sm btn-primary btn-simple active" @click="addNotification()">
                Create New
              </button>
            </div>
          </div>
        </card>
      </div>

      <div class="col-md-8">
        <card type="dashboard-header" class="p-2">
          <div class="d-flex justify-content-between align-items-center px-2">
            <div class="col-10">
                <h2 class="card-title mb-1">{{ $t("notifications.header") }}</h2>
                <p class="mb-0">{{ $t("notifications.footer") }}</p>
            </div>
            <div v-if="this.notifications.length > 0" class="d-flex align-items-center">
              <button class="btn btn-sm btn-danger btn-simple active" @click="showDeleteConfirmation('notifications')">
                Clear All
              </button>
            </div>
          </div>
        </card>
      </div>
    </div>


    <div class="row">
      <div class="col-md-4">
        <card class=".full-height">
          <div class="container-fluid w-100 h-100 d-flex flex-column justify-content-center">
            <template v-if="notificationRules.length > 0">
              <div
                v-for="(rule, index) in notificationRules"
                :key="rule.id"
                class="row p-3 align-items-center system-panel mb-3 clickable-panel"
                @click="onNotificationRuleClick(rule)"
              >
                <div class="col d-flex justify-content-between">
                  <div class="col text-center">
                    <p><strong>Resource</strong> {{ rule.resource.toUpperCase() }}</p>
                  </div>
                  <div class="col text-center">
                    <p><strong>Condition</strong> More than</p>
                  </div>
                  <div class="col text-center">
                    <p><strong>Threshold</strong> {{ rule.usage }}%</p>
                  </div>
                </div>
                
                <div class="d-flex flex-column align-items-end gap-2">
                  <button class="btn btn-sm btn-primary btn-simple active" @click.stop="showDeleteConfirmation('rule', rule.id)">
                    <i class="tim-icons icon-simple-remove"></i> Delete
                  </button>
                </div>
              </div>
            </template>
            <template v-else>
              <div class="d-flex flex-column justify-content-center align-items-center h-100">
                <h3>No Notification Rules</h3>
              </div>
            </template>
          </div>
        </card>
      </div>

      <div class="col-md-8">
        <card class=".full-height">
          <div class="container-fluid w-100 h-100 d-flex flex-column justify-content-center">
            <template v-if="notifications.length > 0">
              <div v-for="(notification, index) in notifications" :key="index" class="row p-3 align-items-center system-panel mb-3 clickable-panel" @click="showProcesses(notification)">

                <div class="col text-center">
                  <p>{{ notification.resource.toUpperCase() }}</p>
                </div>
                
                <div class="col d-flex align-items-center justify-content-center">
                  <div>
                    <p>{{ notification.usage }}%</p>
                  </div>
                  <div class="progress" style="width: 100px; margin-left: 10px;">
                    <div
                      class="progress-bar"
                      role="progressbar"
                      :style="{ width: notification.usage + '%' }"
                      :class="{
                        'bg-success': notification.usage < 40,
                        'bg-warning': notification.usage >= 40 && notification.usage < 70,
                        'bg-danger': notification.usage >= 70
                      }"
                      aria-valuenow="notification.usage"
                      aria-valuemin="0"
                      aria-valuemax="100"
                    ></div>
                  </div>
                </div>

               
  
                <div class="col text-center">
                  <p>{{ formatDate(notification.timestamp) }}</p>
                </div>
                
                <div class="col text-center">
                  <button class="btn btn-sm btn-danger btn-simple active" @click="clearNotification(notification.id)">
                    <i class="tim-icons icon-simple-remove"></i> Dismiss
                  </button>
                </div>

                <div v-if="selectedNotification && selectedNotification.id === notification.id" class="w-100 mt-3">
                  <table class="table table-bordered table-striped">
                    <thead>
                      <tr>
                        <th>PID</th>
                        <th>User</th>
                        <th>Time</th>
                        <th>Name</th>
                        <th>CPU (%)</th>
                        <th>RAM (%)</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="process in processes.slice(0, 8)" :key="process.pid">
                        <td>{{ process.pid }}</td>
                        <td>{{ process.process_user }}</td>
                        <td>{{ process.process_time }}</td>
                        <td>{{ process.name }}</td>
                        <td>{{ process.cpu_usage }}</td>
                        <td>{{ process.ram_usage }}</td>
                      </tr>
                    </tbody>
                  </table>
                </div>




              </div>
            </template>
            <template v-else>
              <div class="d-flex flex-column justify-content-center align-items-center h-100">
                <h3>No Notifications</h3>
              </div>
            </template>
          </div>
        </card>
      </div>

      <Modal
        :show="showModal"
        @close="closeModal"
        type="notice"
        :centered="true"
        gradient="primary"
        modalClasses="custom-modal-class"
        :animationDuration="300"
      >
        <template v-slot:header>
          <h5 class="modal-title">Enter Notification Details</h5>
        </template>

        <template v-slot:footer>
          <button class="btn btn-secondary" @click="closeModal">Cancel</button>
          <button class="btn btn-primary" @click="handleSubmit">Submit</button>
        </template>

        <form @submit.prevent="handleSubmit">
          <div class="form-group row">

            <div class="col-md-4">
              <label for="component">Component</label>
              <select
                v-model="formData.component"
                class="form-control"
                id="component"
                :class="{ 'is-invalid': formErrors.component }"
                required
              >
                <option disabled value="">Select a component</option>
                <option value="cpu">CPU</option>
                <option value="ram">RAM</option>
                <option value="disk">Disk</option>
                <option value="swap">Swap</option>
                <option value="sensors">Sensors</option>
                <option value="battery">Battery</option>
              </select>
              <div v-if="formErrors.component" class="invalid-feedback">{{ formErrors.component }}</div>
            </div>

            <div class="col-md-4">
              <label for="operator">Operator</label>
              <select
                v-model="formData.operator"
                class="form-control"
                id="operator"
                :class="{ 'is-invalid': formErrors.operator }"
                required
              >
                <option disabled value="">Select operator</option>
                <option value="greater">More than</option>
                <option value="less">Less than</option>
              </select>
              <div v-if="formErrors.operator" class="invalid-feedback">{{ formErrors.operator }}</div>
            </div>

            <div class="col-md-4">
              <label for="threshold">Usage %</label>
              <input
                v-model="formData.usage"
                type="number"
                class="form-control"
                id="usage"
                :class="{ 'is-invalid': formErrors.usage }"
                required
              />
              <div v-if="formErrors.usage" class="invalid-feedback">{{ formErrors.usage }}</div>
            </div>
          </div>
        </form>
      </Modal>


      <Modal
        :show="showDeleteConfirmationModal"
        @close="closeDeleteConfirmationModal"
        type="notice"
        :centered="true"
        gradient="danger"
        modalClasses="custom-modal-class"
        :animationDuration="300"
      >
        <template v-slot:header>
          <h5 class="modal-title">Confirm Deletion</h5>
        </template>

        <template v-slot:footer>
          <button class="btn btn-secondary" @click="closeDeleteConfirmationModal">Cancel</button>
          <button class="btn btn-danger" @click="handleDeleteConfirmation()">Delete</button>
        </template>

        <p>
          {{ deletionContext.type === 'notifications' 
              ? 'Are you sure you want to clear all notifications?' 
              : 'Are you sure you want to delete this notification rule?' }}
        </p>
      </Modal>

    </div>
  </div>
</template>

<script>
import Modal from "@/components/Modal";
import NotificationTemplate from "./Notifications/NotificationTemplate";
import { mapState } from 'vuex';
import apiService from "@/services/api"; 

export default {
  components: {
    Modal
  },
  computed: {
    ...mapState({
      user: state => state.currentUser,
      system: state => state.currentSystem,
    })
  },
  async mounted() {
    const response = await apiService.getNotificationRules(this.system.ip, this.user.id);

    this.notificationRules = response;

    if (this.notificationRules.length > 0)
    {
      this.onNotificationRuleClick(this.notificationRules[0]);
    }
  },
  
  beforeDestroy() { 
    clearInterval(this.intervalId);
  },
  data() {
    return {
      notificationRules: [],
      notifications: [],
      selectedNotification: null,
      processes: [],
      selectedNotificationRule: null,
      intervalId: null,
      showModal: false,
      showDeleteConfirmationModal: false,
      formErrors: {},
      formData: {
        ramThreshhold: '',
        cpuThreshhold: '',
      },
      deletionContext: {
        type: '',  
        id: null  
      },
    };
  },
  methods: {
    async showProcesses(notification) {
      if (this.selectedNotification && this.selectedNotification.id === notification.id) {
        this.selectedNotification = null;
        this.processes = [];
      } else {
        this.selectedNotification = notification;

        const response = await apiService.getNotificationProcesses(notification.id);
        this.processes = response;
      }
    },
    async handleDeleteConfirmation() {
      if (this.deletionContext.type === 'notifications') 
      {
        await apiService.deleteNotifications(this.selectedNotificationRule.id);
        this.notifications = [];
      } 
      else if (this.deletionContext.type === 'rule') 
      {
        await apiService.deleteNotificationRule(this.deletionContext.id);
        await apiService.deleteNotifications(this.deletionContext.id);
        
        for (let i = 0; i < this.notifications.length; i++) {
          //await apiService.getNotificationProcesses(this.notifications[i].id);;
        }

        this.notificationRules = this.notificationRules.filter(rule => rule.id !== this.deletionContext.id);

        if (this.selectedNotificationRule?.id === this.deletionContext.id)
        {
          this.selectedNotificationRule = null;
          this.notifications = [];
        }

        clearInterval(this.intervalId);

        if (this.notificationRules.length > 0)
        {
          this.onNotificationRuleClick(this.notificationRules[0]);
        }
      }

      this.closeDeleteConfirmationModal();
    },
    async fetchLatestNotification()
    {
      let response = null;
      try{
        response = await apiService.getLatestNotification(this.selectedNotificationRule.id);
      }
      catch{

      }

      if (response === null)
      {
        return;  
      }

      if (this.notifications.length === 0)
      {
        this.notifications.unshift(response);
      }
      else if (response.id !== this.notifications[0].id)
      {
        this.notifications.unshift(response);
      }
    },
    showDeleteConfirmation(type, id = null) {
      this.deletionContext = { type, id };
      this.showDeleteConfirmationModal = true;
    },
    closeDeleteConfirmationModal() {
      this.showDeleteConfirmationModal = false;
    },
    async clearAllNotifications(){
      await apiService.deleteNotifications(this.selectedNotificationRule.id);
      this.notifications = [];
      this.showDeleteConfirmationModal = false;
    },
    async clearNotification(id){
      await apiService.deleteNotification(id);
      this.notifications = this.notifications.filter(notification => notification.id !== id);
    },
    formatDate(timestamp) {
      const date = new Date(timestamp);
      return `${date.toLocaleDateString()} ${date.toLocaleTimeString()}`;
    },
    async onNotificationRuleClick(notificationRule)
    {
      if (this.selectedNotificationRule !== null && this.selectedNotificationRule.id === notificationRule.id)
      {
        return;
      }
      
      clearInterval(this.intervalId);

      this.selectedNotificationRule = notificationRule;
      
      const response = await apiService.getNotifications(notificationRule.id);
      this.notifications = response;
      
      this.intervalId = setInterval(() => {
        this.fetchLatestNotification();
      }, this.system.updateInterval * 1000);

    },
    closeModal() {
      this.showModal = false;
      this.formData = {
        ramThreshhold: '',
        cpuThreshhold: '',
      };
    },
    async handleSubmit() {
      this.formErrors = {};

      let isValid = true;
      if(!this.formData.component) {
        this.formErrors.component = 'This field is required.';
        isValid = false;
      } 
      
      if(!this.formData.operator) {
        this.formErrors.operator = 'This field is required.';
        isValid = false;
      }

      if(!this.formData.usage) {
        this.formErrors.usage = 'This field is required.';
        isValid = false;
      }

      if (isValid) {

        const rule = {
          resource: this.formData.component,
          operator: this.formData.operator,
          usage: this.formData.usage,
        }

        const addedRule = await apiService.postNotificationRule(rule, this.user, this.system);
        this.notificationRules.push(addedRule);

        if (this.selectedNotificationRule === null)
        {
          this.onNotificationRuleClick(this.notificationRules[0]);
        }
        

        this.closeModal();
        this.notifyVue("Notification Created!");
      }
    },
    addNotification(){
      this.showModal = true;
      this.formErrors = {};
      this.formData = {
        ramThreshhold: '',
        cpuThreshhold: '',
      };

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
  },
};
</script>

<style scoped>
body {
  font-family: Arial, sans-serif;
}

.modal .form-control {
  color: black !important;
}

.full-height {
  height: 100vh;
  overflow-y: auto;
}

.table-bordered {
  border: #434355;
}
</style>