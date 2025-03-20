<template>
  <div class="row">
    <!-- Header Card -->
    <div class="col-md-12">
      <card type="dashboard-header">
        <div class="d-flex justify-content-between align-items-center">
          <div class="col-md-12">
            <h2 class="card-title">{{ $t("networkServices.header") }}</h2>
            <div class="d-flex align-items-center mt-2">
              <p>{{ $t("networkServices.footer") }}</p>
            </div>
          </div>
        </div>
      </card>
    </div>

    <!-- network services -->
    <div class="col-md-6">
      <card type="task">
        <div class="container-fluid">

          <div class="row p-3 align-items-center service-panel mb-3 clickable-panel">
            <div class="col-md-3">
              <span class="badge bg-success">100%</span>
            </div>
            <div class="col-md-4">
              <h5 class="mb-0">Service Name</h5>
            </div>
            <div class="col-md-5 text-end">
              <span>IP Adress: 192.168.1.154 Port: 8080</span>
            </div>
          </div>

          <div class="row p-3 align-items-center service-panel mb-3 clickable-panel">
            
            <div class="col-md-3">
              <span class="badge bg-success">100%</span>
            </div>
            <div class="col-md-4">
              <h5 class="mb-0">Service Name</h5>
            </div>
            
            <div class="col-md-5 text-end">
              <span>IP Adress: 192.168.1.154 Port: 8080</span>
            </div>
          </div>
          
        </div>
      </card>
    </div>

    <!--System info:-->
    <div class="col-md-6">
      <card type="task">
        <div class="container-fluid">
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
      </card>
    </div>
   
  </div>
</template>

<script>
import { BaseTable } from "@/components";
import BarChart from "@/components/Charts/BarChart";
import * as chartConfigs from "@/components/Charts/config";
import config from "@/config";

import apiService from "@/services/api"; 

export default {
  currentSystem: null,
  components: {
    BarChart,
    BaseTable,
  },                      
  data() {
    return {
      dateRange: { //todo
        startDate: '2025-03-18', 
        endDate: '2025-03-23' 
      },
      pingChart: {
        extraOptions: chartConfigs.pingChartOptions,
        chartData: {
          labels: ["USA", "GER", "AUS", "UK", "RO", "BR"],
          datasets: [
            {
              label: "Countries",
              fill: false,
              borderColor: config.colors.info,
              borderWidth: 2,
              borderDash: [],
              borderDashOffset: 0.0,
              data: [53, 20, 10, 80, 100, 45],
            },
          ],
        },
        gradientColors: config.colors.primaryGradient,
        gradientStops: [1, 0.4, 0],
      },
    };
  },
  methods: {
    
  },
  async mounted() {
    this.currentSystem = this.$store.getters.currentSystem;
    const data = await apiService.getNetworkServices("localhost");
    const data2 = await apiService.getNetworkServicePings(1, null);

    console.log(data2);
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
