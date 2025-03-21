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
          <button class="btn btn-sm btn-primary btn-simple active" @click="showModal = true">
            Open Modal
          </button>
        </div>
      </card>
    </div>



    <!-- Network Services Panel -->
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

    <modal
      :show="showModal"
      type="notice"
      centered
      modalClasses="custom-modal"
      modalContentClasses="shadow-lg"
      gradient="primary"
      headerClasses="text-dark font-weight-bold"
      bodyClasses="p-4"
      footerClasses="justify-content-center"
    >
      <template v-slot:header>
        <h5 class="modal-title">My Custom Modal</h5>
      </template>
      <p>This is the modal content.</p>
      <template v-slot:footer>
        <button class="btn btn-secondary" @click="showModal = false">Close</button>
      </template>
    </modal>

    <!-- System Info & Chart -->
    <div class="col-md-6">
      <div class="row">
        <!-- System Info Card -->
        <div class="col-md-12 mb-3">
          <card type="task">
            <div class="container-fluid">
              <h5 class="card-category">{{ $t("dashboard.systemInfo") }}</h5>
              <h3 class="card-title">
                <i class="tim-icons icon-settings text-info"></i> System Stats
              </h3>
              <p>{{ $t("dashboard.systemStats") }}</p>
            </div>
          </card>
        </div>

        <!-- Chart Card -->
        <div class="col-md-12">
          <card type="chart">
            <template slot="header">
              <h5 class="card-category">{{ $t("dashboard.dailySales") }}</h5>
              <h3 class="card-title">
                <i class="tim-icons icon-delivery-fast text-info"></i> 3,500€
              </h3>
            </template>
            <div class="chart-area">
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
  currentSystem: null,
  components: {
    BarChart,
    BaseTable,
    Modal,
  },                      
  data() {
    return {
      showModal: false,
      isOpen: true,
      networkServices: null,
      apiDataPings: null,
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
    closeModal() {
      this.$emit('close');
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
    this.apiDataPings = await apiService.getNetworkServicePings(networkServices[0].id, null);
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
</style>
