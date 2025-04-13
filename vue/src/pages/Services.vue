<template>
  <div class="row">

    <div class="col-md-12">
      <card type="dashboard-header" class="p-2">
        <div class="d-flex justify-content-between align-items-center px-2">
          
          <div class="col-10">
            <h2 class="card-title mb-1">{{ $t("services.header") }}</h2>
            <p class="mb-0">{{ $t("services.footer") }}</p>
          </div>
          
          <div class="col-2 d-flex justify-content-end align-items-center">
            <div class="d-flex flex-column align-items-end gap-2">
              <button 
                class="btn btn-sm btn-primary btn-simple active w-100" 
                @click="toggleAutoUpdate" 
                :class="{'btn-warning': !isPaused, 'btn-success': isPaused}"
              >
                <i :class="isPaused ? 'tim-icons icon-triangle-right-17' : 'tim-icons icon-button-pause'"></i> 
                {{ isPaused ? 'Resume' : 'Pause' }}
              </button>
            </div>
          </div>

        </div>
      </card>
    </div>

    <div class="col-md-12">
      <card type="services">
        <template slot="header">

        </template>
        <div class="table-full-width table-responsive">
          <div v-if="errorMessage" class="alert alert-danger">{{ errorMessage }}</div>

          <base-table
            v-if="tableData.length > 0"
            :columns="['CGroup', 'Tasks', 'CPU', 'Memory', 'Input', 'Output']"
            :data="tableData"
            thead-classes="text-primary"
          >
            <template #columns>
              <th @click="setSortKey('CGroup')">CGroup</th>
              <th @click="setSortKey('Tasks')">Tasks</th>
              <th @click="setSortKey('CPU')">CPU%</th>
              <th @click="setSortKey('Memory')">Memory</th>
              <th @click="setSortKey('Input')">Input</th>
              <th @click="setSortKey('Output')">Output</th>
              <th>Actions</th> 
            </template>
            <template slot-scope="{ row }">
              <td>{{ row.CGroup }}</td>
              <td>{{ row.Tasks }}</td>
              <td>{{ row.CPU }}</td>
              <td>{{ row.Memory }}</td>
              <td>{{ row.Input }}</td>
              <td>{{ row.Output }}</td>
              <td>
                
                <button @click="getServiceLogs(row.CGroup)" class="btn btn-info btn-sm">
                  I
                </button>
              </td>
            </template>
          </base-table>
          <div v-else="loading" class="custom-loader-wrapper">
            <div class="custom-spinner"></div>
            <p class="mt-3">Loading data...</p>
          </div>
        </div>
      </card>
    </div>

    <Modal
      :show="showModal"
      @close="closeModal"
      type=""
      :centered="true"
      gradient="primary"
      modalClasses="custom-modal-class"
      :animationDuration="300"
    >

      <template v-slot:header>
        <h5 class="modal-title">System Logs</h5>
      </template>

      <template v-slot:footer>
        <button class="btn btn-secondary" @click="closeModal">Cancel</button>
      </template>

      <form @submit.prevent="handleSubmit">
        <div class="form-group">
          <pre class="form-control">{{ serviceLogs }}</pre>
        </div>
      </form>
    </Modal>

  </div>
</template>

<script>
import { BaseTable } from "@/components";
import apiService from "@/services/api";
import Modal from "@/components/Modal";
import NotificationTemplate from "./Notifications/NotificationTemplate";


export default {
  currentSystem: null,
  components: {
    BaseTable,
    Modal
  },
  data() {
    return {
      serviceLogs: [],
      showModal: false,
      isPaused: false,
      tableData: [],
      errorMessage: null,
      sortKey: 'Tasks',
      sortOrder: 'asc',
    };
  },
  methods: {
    notifyVue(message) {
      this.$notify({
        component: NotificationTemplate,
        message: message,
        icon: "ticon",
        icon: "tim-icons icon-bell-55",
        horizontalAlign: 'center',
        verticalAlign: 'top',
        type: "info",
        timeout: 3000,
      });
    },
    async getServiceLogs(serviceName){
      const response = await apiService.execCommand(`systemctl status ${serviceName}`, this.currentSystem);

      console.log(response);

      this.serviceLogs = response.output;
      this.showModal = true;
    },
    closeModal() {
      this.showModal = false;
      this.serviceLogs = [];
    },
    toggleAutoUpdate(){
      if (this.isPaused === true)
      {
        this.isPaused = false;
        this.startAutoUpdate();
        this.notifyVue("Service Updates Resumed!");
      }else{
        this.isPaused = true;
        this.stopAutoUpdate();
        this.notifyVue("Service Updates Stopped!");
      }
    },
    stopAutoUpdate() {
      if (this.intervalId) {
        clearInterval(this.intervalId);
      }
    },
    startAutoUpdate(){
      this.intervalId = setInterval(() => { this.updatePage(); }, 5000);
    },
    async updatePage(){
      this.loadData();
    },
    async loadData() {
      this.errorMessage = null;
      try {
        const response = await apiService.execCommand('systemd-cgtop -n 2 -b | tail -n +39', this.currentSystem);
        
        this.tableData = this.formatData(response.output);
        this.sortTable();  
      } catch (error) {
        console.error("API Error:", error);
        this.errorMessage = "Failed to load data. Please try again later.";
      } 
    },

    formatData(response) {
      const lines = response.split('\n');
      return lines.map(line => {
        const parts = line.trim().split(/\s+/);
        if (parts.length >= 6) {
          return {
            CGroup: parts[0],
            Tasks: parts[1],
            CPU: parts[2],
            Memory: parts[3],
            Input: parts[4],
            Output: parts[5],
          };
        }
        return null; 
      }).filter(row => row !== null); 
    },
    setSortKey(key) {
      if (this.sortKey === key) {
        this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
      } else {
        this.sortKey = key;
        this.sortOrder = 'asc';
      }
      this.sortTable();
    },

    sortTable() {
      this.tableData = [...this.tableData].sort((a, b) => {
        const modifier = this.sortOrder === 'asc' ? 1 : -1;

        if (this.sortKey === 'Memory') {
          const memoryA = this.convertMemoryToMB(a[this.sortKey]);
          const memoryB = this.convertMemoryToMB(b[this.sortKey]);
          return (memoryA - memoryB) * modifier;
        }

        if (typeof a[this.sortKey] === 'number') {
          return (a[this.sortKey] - b[this.sortKey]) * modifier;
        } else {
          return a[this.sortKey].localeCompare(b[this.sortKey]) * modifier;
        }
      });
    },

    convertMemoryToMB(memory) {
      const value = parseFloat(memory.slice(0, -1));  
      const unit = memory.slice(-1).toUpperCase(); 

      if (unit === 'G') {
        return value * 1024; 
      } else if (unit === 'M') {
        return value;  
      } else if (unit === 'K') {
        return value / 1024;  
      }


      return 0; 
    },
  },
  mounted() {
    this.currentSystem = this.$store.getters.currentSystem;
    this.loadData();
    this.startAutoUpdate();
  },
  beforeDestroy() {
    this.stopAutoUpdate(); 
  },
};
</script>

<style scoped>
.btn-sm {
  margin: 0 3px;
}
th {
  cursor: pointer;
}
.modal-dialog {
  max-width: none !important; /* Remove Bootstrap's width limits */
  width: auto !important; /* Allow modal to grow based on content */
  min-width: 60vw; /* Set a minimum width */
  display: flex; /* Ensures it grows with content */
}
.custom-modal-class .modal-content {
  flex-grow: 1; 
  width: auto;
  max-width: 100%;      
}

.custom-modal-class .modal-body {
  white-space: pre-wrap; 
  overflow-x: auto; 
  word-wrap: break-word;
}

.custom-modal-class .form-group {
  flex-grow: 1;  
}

.custom-modal-class .form-control {
  height: auto;
  min-height: 100px; 
  white-space: pre-wrap;   
  word-wrap: break-word;   
  max-height: 500px;      
  overflow-y: auto;
}

.custom-loader-wrapper {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  margin: 5rem 0;
  text-align: center;
  color: #555;
}

.custom-spinner {
  width: 48px;
  height: 48px;
  border: 5px solid rgba(0, 123, 255, 0.2);
  border-top-color: #007bff;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

</style>