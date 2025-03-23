<template>
  <div class="row">
    <!-- Header Card -->
    <div class="col-md-12">
      <card type="dashboard-header" class="p-2">
        <div class="d-flex justify-content-between align-items-center px-2">
          <div>
            <h2 class="card-title mb-1">{{ $t("networkServices.header") }}</h2>
            <p class="mb-0">{{ $t("networkServices.footer") }}</p>
          </div>

          <div class="d-flex align-items-center">

            <button class="btn btn-sm btn-primary btn-simple active" @click="addService()">
              Add Service
            </button>

            <div class="mr-2">
              <date-range-picker 
                v-model="dateRange"
                :single-date-picker="false"
                :ranges="false"
                :minDate="this.minDate" 
                :maxDate="this.maxDate"
                :showDropdowns="false"
                :time-picker="false"
                :auto-apply="true"
                :opens="'left'"
                :locale-data="localeData"
                @update="updateRangeValues"
              >
                <template v-slot:input="picker">
                  {{ picker.startDate | date }} - {{ picker.endDate | date }}
                </template>
              </date-range-picker>
            </div>
          </div>
        </div>
      </card>
    </div>

    <div class="col-md-6">
      <card type="task">
        <div class="container-fluid">
          <div v-for="service in this.networkServices" :key="service.id" class="row p-3 align-items-center service-panel mb-3 clickable-panel" @click="handleClick(service.id)">
            <div class="col-md-3">
              <span class="badge">up</span>
            </div>
            <div class="col-md-4">
              <h5 class="mb-0">{{ service.name }}</h5>
            </div>
            <div class="col-md-5 text-end">
              <span>IP Address: {{ service.ip }} Port: {{ service.port }}</span>
            </div>
          </div>
        </div>
      </card>
    </div>

   
    <div class="col-md-6">
      <div class="row">

        <div class="col-md-12 mb-3">
          <card type="task">
            <div class="d-flex justify-content-between align-items-start">
              
              <div class="col-10">
                <h5 class="card-category">{{ $t("networkServices.infoChartHeader") }}</h5>
                
                <div class="d-flex flex-column">
                  <div>
                    <h3 class="card-title d-flex align-items-center" v-if="selectedService">
                      <i class="tim-icons icon-settings text-info me-2"></i>
                      <a :href="'http://' + selectedService.ip + ':' + selectedService.port" target="_blank" class="text-decoration-underline">  
                        {{ selectedService.name }}
                      </a>
                    </h3>
                    <p v-if="getLastPingTime()" class="text-muted mt-2">
                      Last pinged: {{ getLastPingTime() }}
                    </p>
                  </div>
                </div>
              </div>

              <div class="col-2 d-flex justify-content-end align-items-center">
                <div class="d-flex flex-column align-items-end gap-2">
                  <button class="btn btn-sm btn-primary btn-simple active w-100" @click="editService()">
                    <i class="tim-icons icon-settings-gear-63"></i> Edit
                  </button>
                  <button class="btn btn-sm btn-primary btn-simple active w-100" @click="cloneService()">
                    <i class="tim-icons icon-refresh-01"></i> Clone
                  </button>
                  <button class="btn btn-sm btn-primary btn-simple active w-100">
                    <i class="tim-icons icon-simple-remove"></i> Delete
                  </button>
                </div>
              </div>
            </div>
          </card>
        </div>

        <!--info card -->
        <div class="col-md-12 mb-3">
          <card type="task">
            <div class="container-fluid h-100 d-flex align-items-center justify-content-center">
              <div class="row w-100 text-center">
                <div class="col-md-3">
                  <h5>Last Response Time</h5>
                  <p>{{ this.infoCardData.latestResponseTime }} ms</p>
                </div>
                <div class="col-md-3">
                  <h5>Avg Response Time</h5>
                  <p>{{ this.infoCardData.avgResponseTimeRange }} ms</p>
                </div>
                <div class="col-md-3">
                  <h5>24h Uptime</h5>
                  <p>{{ this.infoCardData.uptimePercentage24h }}%</p>
                </div>
                <div class="col-md-3">
                  <h5>Date Range Uptime</h5>
                  <p>{{ this.infoCardData.uptimePercentageRange }}%</p>
                </div>
              </div>
            </div>
          </card>
        </div>

        <!-- Chart Card -->
        <div class="col-md-12">
          <card type="chart" ref="pingChart">
            <template slot="header">
              <h4 class="card-title">{{ $t("networkServices.chartHeader") }}</h4>
            </template>
            <div class="chart-area">s
              <bar-chart
                style="height: 100%"
                chart-id="blue-bar-chart"
                :chart-data="pingChart.chartData"
                :gradient-stops="pingChart.gradientStops"
                :extra-options="pingChart.extraOptions"
              >
              </bar-chart>
            </div>
          </card>
        </div>
      </div>
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
      <!-- Header Slot -->
      <template v-slot:header>
        <h5 class="modal-title">Enter Service Details</h5>
      </template>

      <!-- Footer Slot -->
      <template v-slot:footer>
        <button class="btn btn-secondary" @click="closeModal">Cancel</button>
        <button class="btn btn-primary" @click="handleSubmit">Submit</button>
      </template>

      <!-- Form Content -->
      <form @submit.prevent="handleSubmit">
        <div class="form-group">
          <label for="name">Name</label>
          <input v-model="formData.name" type="text" class="form-control" id="name" :class="{'is-invalid': formErrors.name}" required />
          <div v-if="formErrors.name" class="invalid-feedback">{{ formErrors.name }}</div>
        </div>

        <div class="form-group">
          <label for="ip">IP Address</label>
          <input v-if="this.systemInfo" v-model="formData.ip" type="text" class="form-control" id="ip" :class="{'is-invalid': formErrors.ip}" required />
          <div v-if="formErrors.ip" class="invalid-feedback">{{ formErrors.ip }}</div>
        </div>

        <div class="form-group">
          <label for="port">Port</label>
          <input v-model.number="formData.port" type="number" class="form-control" id="port" :class="{'is-invalid': formErrors.port}" required />
          <div v-if="formErrors.port" class="invalid-feedback">{{ formErrors.port }}</div>
        </div>

        <div class="form-group">
          <label for="interval">Interval</label>
          <input v-model.number="formData.interval" type="number" class="form-control" id="interval" :class="{'is-invalid': formErrors.interval}" required />
          <div v-if="formErrors.interval" class="invalid-feedback">{{ formErrors.interval }}</div>
        </div>

        <div class="form-group">
          <label for="timeout">Timeout</label>
          <input v-model.number="formData.timeout" type="number" class="form-control" id="timeout" :class="{'is-invalid': formErrors.timeout}" required />
          <div v-if="formErrors.timeout" class="invalid-feedback">{{ formErrors.timeout }}</div>
        </div>

        <div class="form-group">
          <label for="expected_status">Expected Status</label>
          <input v-model.number="formData.expected_status" type="number" class="form-control" id="expected_status" :class="{'is-invalid': formErrors.expected_status}" required />
          <div v-if="formErrors.expected_status" class="invalid-feedback">{{ formErrors.expected_status }}</div>
        </div>
      </form>
    </Modal>

  </div>
</template>

<script>
import { BaseTable } from "@/components";
import BarChart from "@/components/Charts/BarChart";
import * as chartConfigs from "@/components/Charts/config";
import config from "@/config";
import apiService from "@/services/api"; 
import Modal from "@/components/Modal";
import DateRangePicker from 'vue2-daterange-picker'
import api from "../services/api";



export default {
  components: {
    BarChart,
    BaseTable,
    Modal,
    DateRangePicker
  },                      
  data() {
    return {
      infoCardData: {
        latestResponseTime: 0,
        avgResponseTimeRange: 0,
        uptimePercentageRange: 0,
        uptimePercentage24h: 0
      },
      localeData: {
        format: 'YYYY-MM-DD',
        separator: ' to ',
        applyLabel: 'Apply',
        cancelLabel: 'Cancel',
      },
      systemInfo: [],
      minDate: null,
      maxDate: null,
      currentSystem: null,
      networkServices: null,
      selectedService: null,
      apiDataPings: [],
      showModal: false,
      formErrors: {},
      formData: {
        name: '',
        ip: '',
        port: '',
        interval: '',
        timeout: '',
        expected_status: '',
      },
      dateRange: { //todo
        startDate: '2025-03-21', 
        endDate: '2025-03-23' 
      },
      pingChart: {
        extraOptions: chartConfigs.pingChartOptions,
        chartData: {
          labels: [],
          datasets: [
            {
              backgroundColor: "#2380f7", // solid color for the bars
              borderColor: "#2380f7", // optional border color
              label: "Ping Response Times",
              fill: false,
              borderColor: config.colors.info,
              borderWidth: 2,
              borderDash: [],
              borderDashOffset: 0.0,
              data: [],
            },
          ],
        },
        gradientColors: config.colors.primaryGradient,
        gradientStops: [1, 0.4, 0],
      },
    };
  },
  methods: {
    handleSubmit() {
      this.formErrors = {};

      

      let isValid = true;
      if (!this.formData.name) {
        this.formErrors.name = 'Name is required.';
        isValid = false;
      }
      if (!this.systemInfo || !this.systemInfo.ip) {
        this.formErrors.ip = 'IP address is required.';
        isValid = false;
      }
      if (!this.formData.port) {
        this.formErrors.port = 'Port is required.';
        isValid = false;
      }
      if (!this.formData.interval) {
        this.formErrors.interval = 'Interval is required.';
        isValid = false;
      }
      if (!this.formData.timeout) {
        this.formErrors.timeout = 'Timeout is required.';
        isValid = false;
      }
      if (!this.formData.expected_status) {
        this.formErrors.expected_status = 'Expected status is required.';
        isValid = false;
      }

      if (isValid) {
        this.submitForm();
        
        this.showModal = false;
      }
    },
    async submitForm() {
      await apiService.insertNetworkService(this.formData, this.systemInfo.name);
      this.networkServices = await apiService.getNetworkServices(this.systemInfo.name);
    },
    getLastPingTime() {
     
      if (this.apiDataPings.length === 0 || !this.apiDataPings.at(-1)) {
        return 'No data'; 
      }

      const isoTimestamp = this.apiDataPings.at(-1).timestamp;

      const date = new Date(isoTimestamp);

      const year = date.getFullYear();
      const month = (date.getMonth() + 1).toString().padStart(2, '0'); 
      const day = date.getDate().toString().padStart(2, '0'); 

      const hours = date.getHours().toString().padStart(2, '0'); 
      const minutes = date.getMinutes().toString().padStart(2, '0');
      const seconds = date.getSeconds().toString().padStart(2, '0'); 

      return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
    },
    getIsUpStatus() {
      
      if (this.apiDataPings.length === 0 || !this.apiDataPings.at(-1)) {
        return false; 
      }
      return this.apiDataPings.at(-1).responseTime !== 0;
    },
    async updateRangeValues(newRange)
    {
      this.dateRange.startDate = newRange.startDate.toISOString();

      const endDate = newRange.endDate;

      endDate.setMinutes(new Date().getMinutes());
      endDate.setHours(new Date().getHours());
      endDate.setSeconds(new Date().getSeconds());

      this.dateRange.endDate = endDate.toISOString();
      
      this.apiDataPings = await apiService.getNetworkServicePings(this.selectedService.id, this.dateRange);
      this.initInfoCard();
      this.initPingChart();
    },
    addService(){
      this.formData = [];
      this.showModal = true;
    },
    editService(){
      this.showModal = true;
      this.formData.name = this.selectedService.name;
      this.formData.port = this.selectedService.port;
      this.formData.interval = this.selectedService.interval;
      this.formData.timeout = this.selectedService.timeout;
      this.formData.expected_status = this.selectedService.timeout;
    },
    cloneService(){
      this.showModal = true;
      this.showModal = true;
      this.formData.name = this.selectedService.name;
      this.formData.port = this.selectedService.port;
      this.formData.interval = this.selectedService.interval;
      this.formData.timeout = this.selectedService.timeout;
      this.formData.expected_status = this.selectedService.timeout;
    },
    initInfoCard() {
      // latest response time
      this.infoCardData.latestResponseTime = this.apiDataPings.length > 0 ? this.apiDataPings.at(-1).responseTime : 0;

      // avg response time date range filtered
      const filteredArrByDateRange = this.apiDataPings.filter(item => {
        return item.timestamp >= this.dateRange.startDate && item.timestamp <= this.dateRange.endDate;
      });

      const filteredArr = filteredArrByDateRange.filter(item => item.responseTime !== 0);
      const sum = filteredArr.reduce((acc, item) => acc + item.responseTime, 0);  
      this.infoCardData.avgResponseTimeRange = filteredArr.length > 0 ? (sum / filteredArr.length).toFixed(2) : 0;

      // uptime percentage range
      this.infoCardData.uptimePercentageRange = filteredArrByDateRange.length > 0 ? ((filteredArr.length / filteredArrByDateRange.length) * 100).toFixed(2) : 0;

      const now = new Date();
      const last24Hours = new Date(now.getTime() - 24 * 60 * 60 * 1000).toISOString();

      const filteredPings24h = this.apiDataPings.filter(ping => {
        return ping.timestamp >= last24Hours;
      });

      const filteredArr24h = filteredPings24h.filter(item => item.responseTime !== 0);
      this.infoCardData.uptimePercentage24h = filteredPings24h.length > 0 ? ((filteredArr24h.length / filteredPings24h.length) * 100).toFixed(2) : 0;
    },
    openModal() {
      this.showModal = true;
    },
    closeModal() {
      this.showModal = false;
    },
    async handleClick(id) {
      this.apiDataPings = await apiService.getNetworkServicePings(id, null);
      this.initPingChart();
    },
    initPingChart()
    {
      let chartData = {
        labels: [],
        datasets: [
          {
            fill: true,
            borderColor: config.colors.primary,
            borderWidth: 2,
            borderDash: [],
            borderDashOffset: 0.0,
            pointBackgroundColor: config.colors.primary,
            pointBorderColor: "rgba(255,255,255,0)",
            pointHoverBackgroundColor: config.colors.primary,
            pointBorderWidth: 20,
            pointHoverRadius: 4,
            pointHoverBorderWidth: 15,
            pointRadius: 4,
            label: "ms: ",
            data: this.apiDataPings.map((pingData) => pingData.responseTime),
          },
        ],
      };

      chartData.datasets[0].categoryPercentage = 1;
      chartData.datasets[0].barPercentage = 1;
      
      chartData.labels = this.apiDataPings.map(pingData => {
        const date = new Date(pingData.timestamp);
        const month = (date.getMonth() + 1).toString().padStart(2, '0'); 
        const day = date.getDate().toString().padStart(2, '0');
        return month + '-' + day;
      });
      
      this.pingChart.chartData = chartData;

    }
  },
  async mounted() 
  {
    this.systemInfo = this.$store.getters.currentSystem;

    this.networkServices = await apiService.getNetworkServices(this.systemInfo.name);
    this.selectedService = this.networkServices[0];

    console.log(this.networkServices);

    const date = new Date(this.selectedService.last_checked);
    this.minDate = `${(date.getMonth() + 1).toString().padStart(2, '0')}/${date.getDate().toString().padStart(2, '0')}/${date.getFullYear()}`;

    // Create Date object from minDate string
    const minDateObj = new Date(`${date.getFullYear()}-${(date.getMonth() + 1).toString().padStart(2, '0')}-${date.getDate().toString().padStart(2, '0')}`);
    this.dateRange.startDate = minDateObj.toISOString();

    const today = new Date();
    this.maxDate = `${(today.getMonth() + 1).toString().padStart(2, '0')}/${today.getDate().toString().padStart(2, '0')}/${today.getFullYear()}`;
    this.dateRange.endDate = today.toISOString();
    
    this.apiDataPings = await apiService.getNetworkServicePings(this.selectedService.id, this.dateRange);
    
    //const maxResponseTime = Math.max(...this.apiDataPings.map(item => item.responseTime));
    //this.apiDataPings.forEach(item => {
    //  if (item.responseTime === 0) {
    //    item.responseTime = -this.selectedService.timeout * 1000;
    //  }
    //});

    this.initInfoCard();
    this.initPingChart();
  },
};
</script>

<style>
.pulsating-icon {
  animation: pulse 1.5s infinite ease-in-out;
}

@keyframes pulse {
  0% {
    transform: scale(1);
    opacity: 1;
  }
  50% {
    transform: scale(1.1);
    opacity: 0.7;
  }
  100% {
    transform: scale(1);
    opacity: 1;
  }
}

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

</style>
