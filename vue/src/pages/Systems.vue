<template>
  <div class="row">

    <div class="col-md-12">
      
      <card type="dashboard-header" class="p-2">
        <div class="d-flex justify-content-between align-items-center px-2">
          <div>
            <h2 class="card-title mb-1">{{ $t("systems.header") }}</h2>
            <p class="mb-0">{{ $t("systems.footer") }}</p>
          </div>
          <div class="d-flex align-items-center">
            <button class="btn btn-sm btn-primary btn-simple active" @click="addSystem()">
              Add System
            </button>
          </div>
        </div>
      </card>

      <card class=".full-height">
        <div class="container-fluid w-100 h-100 d-flex flex-column justify-content-center">
          <template v-if="this.systems.length > 0">
            <div
              v-for="(system, index) in this.systems"
              :key="system.id"
              class="row p-3 align-items-center system-panel mb-3 clickable-panel"
              @click="goToDashboard(system)"
            >
              <div class="col text-center">
                <strong>SYSTEM NAME</strong>
                <div>{{ system.name }}</div>
              </div>

              <div class="col text-center">
                <strong>IP ADRESS</strong>
                <div>{{ system.ip }}</div>
              </div>
              

              <div class="col text-center">
                <strong>PORT</strong>
                <div>{{ system.port }}</div>
              </div>

              <div class="col d-flex flex-column align-items-center">
                <strong>CPU</strong>
                <div class="progress" style="width: 100px;">
                  <div
                    v-if="headers.length > 0"
                    class="progress-bar"
                    role="progressbar"
                    :style="{ width: headers[index]?.cpu + '%' }"
                    :class="{
                      'bg-success': headers[index]?.cpu < 40,
                      'bg-warning': headers[index]?.cpu >= 40 && headers[index]?.cpu < 70,
                      'bg-danger': headers[index]?.cpu >= 70
                    }"
                    aria-valuenow="headers[0].cpu"
                    aria-valuemin="0"
                    aria-valuemax="100"
                  ></div>
                </div>
                <div class="text-center">{{ headers[index]?.cpu }}%</div>
              </div>

              <div class="col d-flex flex-column align-items-center">
                <strong>DISK</strong>
                <div class="progress" style="width: 100px;">
                  <div
                    v-if="headers.length > 0"
                    role="progressbar"
                    :style="{ width: headers[index]?.disk + '%' }"
                    :class="{
                      'progress-bar bg-success': headers[index]?.disk < 40,
                      'progress-bar bg-warning': headers[index]?.disk >= 40 && headers[index]?.disk < 70,
                      'progress-bar bg-danger': headers[index]?.disk >= 70
                    }"
                    aria-valuenow="headers[0].disk"
                    aria-valuemin="0"
                    aria-valuemax="100"
                  ></div>
                </div>
                <div class="text-center">{{ headers[index]?.disk }}%</div>
              </div>

              <div class="col d-flex flex-column align-items-center">
                <strong>RAM</strong>
                <div class="progress" style="width: 100px;">
                  <div
                    v-if="headers.length > 0"
                    role="progressbar"
                    :style="{ width: headers[index]?.ram + '%' }"
                    :class="{
                      'progress-bar bg-success': headers[index]?.ram < 40,
                      'progress-bar bg-warning': headers[index]?.ram >= 40 && headers[index]?.ram < 70,
                      'progress-bar bg-danger': headers[index]?.ram >= 70
                    }"
                    aria-valuenow="headers[0].ram"
                    aria-valuemin="0"
                    aria-valuemax="100"
                  ></div>
                </div>
                <div class="text-center">{{ headers[index]?.ram }}%</div>
              </div>

              <div class="col d-flex flex-column align-items-center">
                <strong>BATTERY</strong>
                <div class="progress" style="width: 100px;">
                  <div
                    v-if="headers.length > 0"
                    role="progressbar"
                    :style="{ width: headers[index]?.battery + '%' }"
                    :class="{
                      'progress-bar bg-danger': headers[index]?.battery < 40,
                      'progress-bar bg-warning': headers[index]?.battery >= 40 && headers[index]?.battery < 70,
                      'progress-bar bg-success': headers[index]?.battery >= 70
                    }"
                    aria-valuenow="headers[0].ram"
                    aria-valuemin="0"
                    aria-valuemax="100"
                  ></div>
                </div>
                <div class="text-center">{{ headers[index]?.battery }}%</div>
              </div>

              <div class="col d-flex justify-content-end align-items-center">
                <div class="d-flex flex-column align-items-end gap-2">
                  <button class="btn btn-sm btn-primary btn-simple active w-100" @click.stop="editSystem(system)">
                    <i class="tim-icons icon-settings-gear-63"></i> Edit
                  </button>
                  <button class="btn btn-sm btn-primary btn-simple active w-100" @click.stop="showDeleteConfirmation(system)">
                    <i class="tim-icons icon-simple-remove"></i> Delete
                  </button>
                </div>
              </div>

            </div>
          </template>
          <template v-else>
            <div class="d-flex flex-column justify-content-center align-items-center h-100">
              <h3>No Systems</h3>
            </div>
          </template>
        </div>
      </card>

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
          <h5 class="modal-title">Enter System Details</h5>
        </template>

        <template v-slot:footer>
            <button class="btn btn-secondary" @click="closeModal">Cancel</button>
            <button class="btn btn-primary" @click="copyCommand">Copy Linux Command</button>
            <button class="btn btn-primary" @click="handleSubmit">Submit</button>
        </template>

        <form @submit.prevent="handleSubmit">
          <div class="form-group">
            <label for="name">Name</label>
            <input v-model="formData.name" type="text" class="form-control" id="name" :class="{'is-invalid': formErrors.name}" required />
            <div v-if="formErrors.name" class="invalid-feedback">{{ formErrors.name }}</div>
          </div>

          <div class="form-group">
            <label for="ip">IP Address</label>
            <input v-model="formData.ip" type="text" class="form-control" id="ip" :class="{'is-invalid': formErrors.ip}" required />
            <div v-if="formErrors.ip" class="invalid-feedback">{{ formErrors.ip }}</div>
          </div>

          <div class="form-group">
            <label for="port">Port</label>
            <input v-model.number="formData.port" type="number" class="form-control" id="port" :class="{'is-invalid': formErrors.port}" required />
            <div v-if="formErrors.port" class="invalid-feedback">{{ formErrors.port }}</div>
          </div>

          <div class="form-group">
            <label for="updateInterval">Update Interval</label>
            <input v-model.number="formData.updateInterval" type="number" class="form-control" id="updateInterval" :class="{'is-invalid': formErrors.updateInterval}" required />
            <div v-if="formErrors.updateInterval" class="invalid-feedback">{{ formErrors.updateInterval }}</div>
          </div>

          <div class="form-group">
            <label for="publicKey">Public Key</label>
            <input v-model="formData.publicKey" type="text" class="form-control" id="publicKey" :class="{'is-invalid': formErrors.publicKey}" required />
            <div v-if="formErrors.publicKey" class="invalid-feedback">{{ formErrors.publicKey }}</div>
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
          <button class="btn btn-danger" @click="confirmDelete">Delete</button>
        </template>

        <p>Are you sure you want to delete this System?</p>
      </Modal>


    </div>
  </div>
</template>

<script>
import { BaseTable } from "@/components";
import apiService from "@/services/api"; 
import Modal from "@/components/Modal";
import NotificationTemplate from "./Notifications/NotificationTemplate";
import { mapState } from 'vuex';

export default {
  components: {
    BaseTable,
    Modal
  },
  
  methods: {
    async confirmDelete() {
      this.systems = this.systems.filter(system => system.id !== this.systemToDelete.id);
      
      await apiService.deleteSystem(this.systemToDelete.id);
      
      this.notifyVue('System Deleted!');
      this.closeDeleteConfirmationModal();
    },
    editSystem(system){
      this.systemToEdit = system;

      this.isEditingSystem = true;

      this.showModal = true;
      this.formData.name = system.name;
      this.formData.ip = system.ip;
      this.formData.port = system.port;
      this.formData.updateInterval = system.updateInterval;
      this.formData.publicKey = this.publicKey;
    },
    showDeleteConfirmation(system) {
      this.systemToDelete = system;
      this.showDeleteConfirmationModal = true;
    },
    closeDeleteConfirmationModal() {
      this.showDeleteConfirmationModal = false;
    },
    async copyCommand(){
      this.formErrors = {};

      let isValid = true;
      
      if (!this.formData.ip) {
        this.formErrors.ip = 'IP address is required.';
        isValid = false;
      }
      if (!this.formData.port) {
        this.formErrors.port = 'Port is required.';
        isValid = false;
      }
      if (!this.formData.publicKey) {
        this.formErrors.publicKey = 'Key is required.';
        isValid = false;
      }
      if (isValid) {

        const command = `./install-agent.sh -p ${this.formData.port} -k "${this.formData.publicKey}"`; 
        await navigator.clipboard.writeText(command);
        this.$notify({
          component: NotificationTemplate,
          message: "Command copied to clipboard!",
          icon: "ticon",
          icon: "tim-icons icon-bell-55",
          horizontalAlign: 'center',
          verticalAlign: 'top',
          type: "Success",
          timeout: 3000,
        });
      }
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
    async handleSubmit() {
      this.formErrors = {};

      let isValid = true;
      if (!this.formData.name) {
        this.formErrors.name = 'Name is required.';
        isValid = false;
      }
      if (!this.formData.ip) {
        this.formErrors.ip = 'IP address is required.';
        isValid = false;
      }
      if (!this.formData.port) {
        this.formErrors.port = 'Port is required.';
        isValid = false;
      }
      if (!this.formData.updateInterval) {
        this.formErrors.updateInterval = 'Update interval is required.';
        isValid = false;
      }
      if (!this.formData.publicKey) {
        this.formErrors.publicKey = 'Key is required.';
        isValid = false;
      }
      if (isValid) {

        if (this.isAddingSystem)
        {
          await apiService.postSystem(this.formData);
          this.apiData = await apiService.getSystems(0);//user id

          this.formatApiData();


          this.drawHeaders(this.apiData);
          this.isAddingSystem = false;
          this.formData = [];
        }
        else if (this.isEditingSystem)
        {
          await apiService.updateSystem(this.formData, this.systemToEdit);

          this.apiData = await apiService.getSystems(0);//user id

          this.formatApiData();


          this.drawHeaders(this.apiData);
          this.isEditingSystem = false;
          this.formData = [];
        }
        this.closeModal();
      }
    },
    closeModal() {
      this.showModal = false;
    },
    addSystem(){
      this.isAddingSystem = true;
      this.showModal = true;
      this.formData = [];
      this.formData.publicKey = this.publicKey;
    },
    goToDashboard(system) {
      this.$router.push({ name: 'dashboard', params: { systemName: system.name } });
    },
    formatApiData()
    {
      this.systems = this.apiData.map(system => ({
        creationDate: system.creaationDate,
        id: system.id,
        name: system.name,
        ip: system.ip,
        ownerId: system.ownerId,
        port: system.port,
        updateInterval: system.updateInterval
      }));
    },
    async drawHeaders(data){
      this.headers = [];
      for (const item of data) {


        const metric = await apiService.getLatestMetrics(item.name);

        if(metric.cpuName === null)
        {
          return;
        }

        const cpuUsagePercentage = parseFloat((metric.cpuCores[0].total).toFixed(1));
        const diskUsagePercentage = parseFloat((metric.diskUsedSpace * 100 / metric.diskTotalSpace).toFixed(1));
        const ramUsagePercentage = parseFloat((metric.ramMemUsed * 100 / metric.ramMemTotal).toFixed(1));
        const battery = parseFloat(metric.batteryCapacity);
        const batteryStatus = metric.batteryStatus;

        this.headers.push({
          name: item.name,
          cpu: cpuUsagePercentage,  
          disk: diskUsagePercentage, 
          ram: ramUsagePercentage,  
          battery: battery,
          batteryStatus: batteryStatus
        });
      }
    }
  },
  computed: {
    ...mapState({
      user: state => state.currentUser,
    })
  },
  async mounted() {

    const response = await apiService.getPublicKey();
    this.publicKey = response;
    this.apiData = await apiService.getSystems(this.user.id);

    this.formatApiData();
    this.drawHeaders(this.apiData);

  },
  data() {
    return {
      systemToDelete: {},
      systemToEdit: {},
      showDeleteConfirmationModal: false,
      publicKey: "",
      isAddingSystem: false,
      isEditingSystem: false,
      showModal: false,
      formErrors: {},
      headers: [],
      formData: {
        name: '',
        ip: '',
        port: '',
        updateInterval: '',
        publicKey: ''
      },
      apiData: [], 
      systems: []
    };
  }
};
</script>
<style>
.clickable-panel {
  background-color: #1e1e2e;
  border: 2px solid rgb(46, 46, 66);
  border-radius: 5px; 
  transition: background-color 0.1s ease, border-color 0.1s ease;
  cursor: pointer; 
}

.clickable-panel:hover {
  background-color: rgb(46, 46, 66);
  
}

.modal .form-control {
  color: black !important;
}

.full-height {
  height: 100vh;
  overflow-y: auto;
}
</style>
