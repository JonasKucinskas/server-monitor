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
          <button class="btn btn-sm btn-primary btn-simple active" @click="addService()">
            Add Service
          </button>
        </div>
      </card>
    </div>

    <!-- all services -->
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

   
    <!-- System Info & Chart -->
    <div class="col-md-6">
      <div class="row">
        <!-- System Info Card -->
        <div class="col-md-12 mb-3">
          <card type="task">
            <div class="container-fluid">
              <h5 class="card-category">{{ $t("networkServices.infoChartHeader") }}</h5>
              <h3 class="card-title d-flex align-items-center" v-if="selectedService">
                <i class="tim-icons icon-settings text-info me-2"></i>
                {{ selectedService.name }}
              </h3>
              <h3 class="card-title" v-if="selectedService">
                <a 
                  :href="'http://' + selectedService.ip + ':' + selectedService.port" 
                  target="_blank"
                  class="text-decoration-underline"
                >
                  {{ 'http://' + selectedService.ip + ':' + selectedService.port }}
                </a>
              </h3>
              <div class="d-flex align-items-center gap-2 mt-2">
                <button class="btn btn-sm btn-primary btn-simple active" @click="editService()">
                  <i class="tim-icons icon-settings-gear-63"></i> Edit
                </button>
                <button class="btn btn-sm btn-primary btn-simple active">
                  <i class="tim-icons icon-refresh-01" @click="cloneService()"></i> Clone
                </button>
                <button class="btn btn-sm btn-primary btn-simple active">
                  <i class="tim-icons icon-simple-remove"></i> Delete
                </button>
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
      centered
      gradient="primary"
      modalClasses="custom-modal-class"
      :animationDuration="300"
    >
      <template v-slot:header>
        <h5 class="modal-title">Enter Service Details</h5>
      </template>

      <form @submit.prevent="handleSubmit">
        <div class="form-group">
          <label for="name">Name</label>
          <input v-model="formData.name" type="text" class="form-control" id="name" required />
        </div>

        <div class="form-group">
          <label for="ip">IP Address</label>
          <input v-if="currentSystem" v-model="this.currentSystem.ip" type="text" class="form-control" id="ip" required />
        </div>

        <div class="form-group">
          <label for="port">Port</label>
          <input v-model.number="formData.port" type="number" class="form-control" id="port" required />
        </div>

        <div class="form-group">
          <label for="interval">Interval</label>
          <input v-model.number="formData.interval" type="number" class="form-control" id="interval" required />
        </div>

        <div class="form-group">
          <label for="timeout">Timeout</label>
          <input v-model.number="formData.timeout" type="number" class="form-control" id="timeout" required />
        </div>

        <div class="form-group">
          <label for="expected_status">Expected Status</label>
          <input v-model.number="formData.expected_status" type="number" class="form-control" id="expected_status" required />
        </div>
      </form>

      <template v-slot:footer>
        <button class="btn btn-secondary" @click="closeModal">Cancel</button>
        <button class="btn btn-primary" @click="handleSubmit">Submit</button>
      </template>
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


export default {
  components: {
    BarChart,
    BaseTable,
    Modal,
  },                      
  data() {
    return {
      infoCardData: {
        latestResponseTime: 0,
        avgResponseTimeRange: 0,
        uptimePercentageRange: 0,
        uptimePercentage24h: 0
      },
      currentSystem: null,
      networkServices: null,
      selectedService: null,
      apiDataPings: null,
      showModal: false,
      formData: {
        name: "",
        port: null,
        interval: null,
        timeout: null,
        expected_status: null,
      },
      dateRange: { //todo
        startDate: '2025-03-18', 
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
    addService(){
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
      this.infoCardData.latestResponseTime = this.apiDataPings.at(-1).responseTime;

      // avg response time date range filtered
      const startDate = new Date(this.dateRange.startDate);
      const endDate = new Date(this.dateRange.endDate);

      const filteredArrByDateRange = this.apiDataPings.filter(item => {
        const itemDate = new Date(item.timestamp);
        return itemDate >= startDate && itemDate <= endDate;
      });

      const filteredArr = filteredArrByDateRange.filter(item => item.responseTime !== 0);
      const sum = filteredArr.reduce((acc, item) => acc + item.responseTime, 0);  // Fixed: sum based on responseTime
      this.infoCardData.avgResponseTimeRange = filteredArr.length > 0 ? sum / filteredArr.length : 0;

      // uptime percentage range
      this.infoCardData.uptimePercentageRange = (filteredArr.length / filteredArrByDateRange.length) * 100;

      // uptime percentage 24h
      const now = new Date();
      const twentyFourHoursAgo = now - 24 * 60 * 60 * 1000;
      const filteredDate24h = this.apiDataPings.filter(item => new Date(item.timestamp) >= twentyFourHoursAgo);
      const filteredArr24h = filteredDate24h.filter(item => item.responseTime !== 0);  // Fixed: new variable for 24h filtered array
      this.infoCardData.uptimePercentage24h = (filteredArr24h.length / filteredDate24h.length) * 100;

      this.infoCardData.avgResponseTimeRange = this.infoCardData.avgResponseTimeRange.toFixed(2);
      this.infoCardData.uptimePercentageRange = this.infoCardData.uptimePercentageRange.toFixed(2);
      this.infoCardData.uptimePercentage24h = this.infoCardData.uptimePercentage24h.toFixed(2);
    },
    openModal() {
      this.showModal = true;
    },
    closeModal() {
      this.showModal = false;
    },
    handleSubmit() {
      console.log("Submitted Data:", this.formData);
      this.formData = null;
      this.showModal = false;
    },
    async handleClick(id) {
      console.log('Service panel clicked!');
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
    this.currentSystem = this.$store.getters.currentSystem;

    this.networkServices = await apiService.getNetworkServices("localhost");
    this.selectedService = this.networkServices[0];
    
    this.apiDataPings = await apiService.getNetworkServicePings(this.networkServices[0].id, null);

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
.clickable-panel {
  background-color: #1e1e2e; /* Light background for the panel */
  border: 2px solid rgb(46, 46, 66); /* Border color */
  border-radius: 5px; /* Rounded borders */
  transition: background-color 0.1s ease, border-color 0.1s ease; /* Smooth transition */
  cursor: pointer; /* Make it clickable */
}

.clickable-panel:hover {
  background-color: rgb(46, 46, 66);
  
}

.modal .form-control {
  color: black !important;
}

</style>
