<template>
  <div>

    <card type="dashboard-header">
      <div class="d-flex justify-content-between align-items-center">
        <div class="col-10">
          <h2 class="card-title">{{ this.systemInfo.name }}</h2>
          <div class="d-flex align-items-center mt-2">
            <p class="mr-3">
              <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-globe h-4 w-4"><circle cx="12" cy="12" r="10"></circle><path d="M12 2a14.5 14.5 0 0 0 0 20 14.5 14.5 0 0 0 0-20"></path><path d="M2 12h20"></path></svg>
              {{ this.apiData[0]?.serverId }}
            </p>
            <p class="mr-3">
              <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-clock-arrow-up h-4 w-4"><path d="M13.228 21.925A10 10 0 1 1 21.994 12.338"></path><path d="M12 6v6l1.562.781"></path><path d="m14 18 4-4 4 4"></path><path d="M18 22v-8"></path></svg>              
              {{ this.uptime }}
            </p>
            <p class="mr-3"><svg viewBox="0 0 256 256" width="24" height="24" class="h-4 w-4"><path fill="currentColor" d="M231 217a12 12 0 0 1-16-2c-2-1-35-44-35-127a52 52 0 1 0-104 0c0 83-33 126-35 127a12 12 0 0 1-18-14c0-1 29-39 29-113a76 76 0 1 1 152 0c0 74 29 112 29 113a12 12 0 0 1-2 16m-127-97a16 16 0 1 0-16-16 16 16 0 0 0 16 16m64-16a16 16 0 1 0-16 16 16 16 0 0 0 16-16m-73 51 28 12a12 12 0 0 0 10 0l28-12a12 12 0 0 0-10-22l-23 10-23-10a12 12 0 0 0-10 22m33 29a57 57 0 0 0-39 15 12 12 0 0 0 17 18 33 33 0 0 1 44 0 12 12 0 1 0 17-18 57 57 0 0 0-39-15"></path></svg>
              {{ this.kernel }}
            </p>
            <p class="mr-3">
              <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-cpu h-4 w-4"><rect width="16" height="16" x="4" y="4" rx="2"></rect><rect width="6" height="6" x="9" y="9" rx="1"></rect><path d="M15 2v2"></path><path d="M15 20v2"></path><path d="M2 15h2"></path><path d="M2 9h2"></path><path d="M20 15h2"></path><path d="M20 9h2"></path><path d="M9 2v2"></path><path d="M9 20v2"></path></svg>              
              {{ this.apiData[0]?.cpuName }}
            </p>
          </div>
        </div>

        <div class="col-2 d-flex align-items-center justify-content-end">
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

        <!--
        <div class="col-5 d-flex align-items-center justify-content-end">
          <base-combo-box
            title="Select an option"
            :options="['Option 1', 'Option 2', 'Option 3']"
            :titleClasses="'btn btn-primary'"
            :menuClasses="'dropdown-menu'"
            :selectedOption="selectedOption"
            @select="handleComboBoxSelect"
          >
          <template v-slot:title="{ isOpen }">
            <div class="icon-and-text">
              <span class="text">{{ selectedOption || 'Select an option' }}</span>
              <span class="icon">
                <svg v-if="isOpen" xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-chevron-up h-4 w-4">
                  <path d="M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8"></path>
                  <path d="M3 3v5h5"></path>
                  <path d="M12 7v5l4 2"></path>
                </svg>
                <svg v-else xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-chevron-down h-4 w-4">
                  <path d="M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8"></path>
                  <path d="M3 3v5h5"></path>
                  <path d="M12 7v5l4 2"></path>
                </svg>
              </span>
            </div>
          </template>
          </base-combo-box>
        </div>
        -->
      </div>
    </card>

    
    <div class="row">
      <div class="col-12">
        <card type="chart" ref="bigChartCard">
          <template slot="header">
            <div class="row">
              <div class="col-sm-6">
                <h2 class="card-title">{{ $t("dashboard.cpuUsageHeader") }}</h2>
                <h5 class="card-category"> {{ $t("dashboard.cpuUsageFooter") }} </h5>
              </div>
              <div class="col-sm-6">
                <div class="btn-group btn-group-toggle float-right" data-toggle="buttons">
                  <label
                    v-for="(option, index) in cpuChartCategories"
                    :key="option"
                    class="btn btn-sm btn-primary btn-simple"
                    :class="{ active: cpuChart.activeIndex === index }"
                    :id="index"
                  >
                    <input
                      type="radio"
                      @click="initCpuChart(index)"
                      name="options"
                      autocomplete="off"
                      :checked="cpuChart.activeIndex === index"
                    />
                    {{ option }}
                  </label>
                </div>
              </div>
            </div>
          </template>
          <div class="chart-area">
            <line-chart
              style="height: 100%"
              ref="cpuChart"
              chart-id="cpu-chart"
              :chart-data="cpuChart.chartData"
              :gradient-colors="cpuChart.gradientColors"
              :gradient-stops="cpuChart.gradientStops"
              :extra-options="cpuChart.extraOptions"
            />
          </div>
        </card>
      </div>
    </div>

    <div class="row">
      <div class="col-12">
        <card type="chart" ref="bigChartCard">
          <template slot="header">
            <div class="row">
              <div class="col-sm-6 text-left">
                <h2 class="card-title">{{ $t("dashboard.networkUsageHeader") }}</h2>
                <h5 class="card-category">{{ $t("dashboard.networkUsageFooter") }}</h5>
              </div>
              <div class="col-sm-6">
                <div class="btn-group btn-group-toggle float-right" data-toggle="buttons">
                  <label
                    v-for="(option, index) in networkChartCategories"
                    :key="option"
                    class="btn btn-sm btn-primary btn-simple"
                    :class="{ active: networkChart.activeIndex === index }"
                    :id="index"
                  >
                    <input
                      type="radio"
                      @click="initNetworkChart(index)"
                      name="options"
                      autocomplete="off"
                      :checked="networkChart.activeIndex === index"
                    />
                    {{ option }}
                  </label>
                </div>
                
                <div v-if="networkChart.activeIndex === 1" class="btn-group btn-group-toggle float-right" data-toggle="buttons">
                  <!--0-download 1-upload--->
                  <label
                    v-for="(option, index) in networkChartExtraCategories"
                    :key="option"
                    class="btn btn-sm btn-primary btn-simple"
                    :class="{ active: networkChart.uploadIndex === index }"
                    :id="index"
                  >
                    <input
                      type="radio"
                      @click="initNetworkChartAllInterfaces(index)"
                      name="options"
                      autocomplete="off"
                      :checked="networkChart.uploadIndex === index"
                    />
                    {{ option }}
                  </label>
                </div>
              </div>

            </div>
          </template>
          <div class="chart-area">
            <line-chart
              style="height: 100%"
              chart-id="network-chart"
              :chart-data="networkChart.chartData"
              :gradient-stops="networkChart.gradientStops"
              :extra-options="networkChart.extraOptions"
            />
          </div>
        </card>
      </div>
    </div>

    <div class="row">
      <div class="col-lg-4">
        <card type="chart">
          <template slot="header">
            <div class="row">
              <div class="col-sm-6 text-left">
                <h2 class="card-title">{{ $t("dashboard.ramUsageHeader") }}</h2>
                <h5 class="card-category">{{ $t("dashboard.ramUsageFooter") }}</h5>
              </div>
            </div>
          </template>
          <div class="chart-area">
            <line-chart
              style="height: 100%"
              chart-id="ram-chart"
              :chart-data="ramChart.chartData"
              :gradient-stops="ramChart.gradientStops"
              :extra-options="ramChart.extraOptions"
            />
          </div>
        </card>
      </div>

      <div class="col-lg-4">
        <card type="chart">
          <template slot="header">
            <div class="row">
              <div class="col-sm-6 text-left">
                <h2 class="card-title">{{ $t("dashboard.diskUsageHeader") }}</h2>
                <h5 class="card-category">{{ $t("dashboard.diskUsageFooter") }}</h5>
              </div>
            </div>
          </template>
          <div class="chart-area">
            <line-chart
              style="height: 100%"
              chart-id="ram-chart"
              :chart-data="diskChart.chartData"
              :gradient-stops="diskChart.gradientStops"
              :extra-options="diskChart.extraOptions"
            />
          </div>
        </card>
      </div>

     

      <div class="col-lg-4">
        <card type="chart">
          <template slot="header">
            <div class="row">
              <div class="col-sm-6 text-left">
                <h2 class="card-title">{{ $t("dashboard.swapUsageHeader") }}</h2>
                <h5 class="card-category">{{ $t("dashboard.swapUsageFooter") }}</h5>
              </div>
            </div>
          </template>
          <div class="chart-area">
            <line-chart
              style="height: 100%"
              chart-id="swap-chart"
              :chart-data="swapChart.chartData"
              :gradient-stops="swapChart.gradientStops"
              :extra-options="swapChart.extraOptions"
            />
          </div>
        </card>
      </div>
      
    </div>

    <div class="row">
      <div class="col-12">
        <card type="chart" ref="bigChartCard">
          <template slot="header">
            <div class="row">
              <div class="col-sm-6 text-left">
                <h2 class="card-title">{{ $t("dashboard.sensorUsageHeader") }}</h2>
                <h5 class="card-category"> {{ $t("dashboard.sensorUsageFooter") }} </h5>
              </div>
            </div>
          </template>
          <div class="chart-area">
            <line-chart
              style="height: 100%"
              ref="sensorChart"
              chart-id="sensor-chart"
              :chart-data="sensorChart.chartData"
              :gradient-colors="sensorChart.gradientColors"
              :gradient-stops="sensorChart.gradientStops"
              :extra-options="sensorChart.extraOptions"
            />
          </div>
        </card>
      </div>
    </div>
  </div>
</template>
<script>
import LineChart from "@/components/Charts/LineChart";
import BaseComboBox from "@/components/BaseComboBox.vue";
import BaseInput from "../components/Inputs/BaseInput.vue";
import * as chartConfigs from "@/components/Charts/config";
import config from "@/config";
import apiService from "@/services/api";
import { mapState, mapActions } from 'vuex';
import DateRangePicker from 'vue2-daterange-picker'
import 'vue2-daterange-picker/dist/vue2-daterange-picker.css'


export default {
  components: {
    LineChart,
    BaseComboBox,
    BaseInput,
    DateRangePicker
  },
  data() {
    return {
      minDate: "",
      maxDate: "",
      dateRange: { //todo
        startDate: '2025-03-18', 
        endDate: '2025-03-19' 
      },
      localeData: {
        format: 'YYYY-MM-DD',
        separator: ' to ',
        applyLabel: 'Apply',
        cancelLabel: 'Cancel',
      },
      systemInfo: [],
      apiData: [], 
      uptime: "",
      kernel: "",
      selectedOption: null,
      cpuChart: {
        allData: [],
        activeIndex: 0,
        chartData: {
          datasets: [{}],
          labels: [],
        },
        extraOptions: chartConfigs.cpuChartOptions,
        gradientColors: config.colors.primaryGradient,
        gradientStops: [1, 0.4, 0],
        categories: [],
      },
      networkChart: {
        allData: [],
        activeIndex: 0,
        uploadIndex: 0,//download by default
        chartData: {
          datasets: [{}],
          labels: [],
        },
        extraOptions: chartConfigs.networkChartOptions,
        gradientColors: config.colors.primaryGradient,
        gradientStops: [1, 0.4, 0],
        categories: [],
      },
      sensorChart: {
        allData: [],
        chartData: {
          datasets: [{}],
          labels: [],
        },
        extraOptions: chartConfigs.sensorChartOptions,
        gradientColors: config.colors.primaryGradient,
        gradientStops: [1, 0.4, 0],
        categories: [],
      },
      ramChart: {
        extraOptions: chartConfigs.ramChartOptions,
        chartData: {
          labels: [],
          datasets: [
            {
              label: "My First dataset",
              fill: true,
              borderColor: config.colors.danger,
              borderWidth: 2,
              borderDash: [],
              borderDashOffset: 0.0,
              pointBackgroundColor: config.colors.danger,
              pointBorderColor: "rgba(255,255,255,0)",
              pointHoverBackgroundColor: config.colors.danger,
              pointBorderWidth: 20,
              pointHoverRadius: 4,
              pointHoverBorderWidth: 15,
              pointRadius: 4,
              data: [],
            },
          ],
        },
        gradientColors: [
          "rgba(66,134,121,0.15)",
          "rgba(66,134,121,0.0)",
          "rgba(66,134,121,0)",
        ],
        gradientStops: [1, 0.4, 0],
      },
      diskChart: {
        extraOptions: chartConfigs.diskChartOptions,
        chartData: {
          labels: [],
          datasets: [
            {
              label: "My First dataset",
              fill: true,
              borderColor: config.colors.danger,
              borderWidth: 2,
              borderDash: [],
              borderDashOffset: 0.0,
              pointBackgroundColor: config.colors.danger,
              pointBorderColor: "rgba(255,255,255,0)",
              pointHoverBackgroundColor: config.colors.danger,
              pointBorderWidth: 20,
              pointHoverRadius: 4,
              pointHoverBorderWidth: 15,
              pointRadius: 4,
              data: [],
            },
          ],
        },
        gradientColors: [
          "rgba(66,134,121,0.15)",
          "rgba(66,134,121,0.0)",
          "rgba(66,134,121,0)",
        ],
        gradientStops: [1, 0.4, 0],
      },
      
      swapChart: {
        extraOptions: chartConfigs.ramChartOptions,
        chartData: {
          labels: [],
          datasets: [
            {
              label: "My First dataset",
              fill: true,
              borderColor: config.colors.danger,
              borderWidth: 2,
              borderDash: [],
              borderDashOffset: 0.0,
              pointBackgroundColor: config.colors.danger,
              pointBorderColor: "rgba(255,255,255,0)",
              pointHoverBackgroundColor: config.colors.danger,
              pointBorderWidth: 20,
              pointHoverRadius: 4,
              pointHoverBorderWidth: 15,
              pointRadius: 4,
              data: [],
            },
          ],
        },
        gradientColors: [
          "rgba(66,134,121,0.15)",
          "rgba(66,134,121,0.0)",
          "rgba(66,134,121,0)",
        ],
        gradientStops: [1, 0.4, 0],
      },
    };
  },
  computed: {
    cpuChartCategories() {
      return this.$t("dashboard.cpuChartCategories");
    },
    networkChartCategories() {
      return this.$t("dashboard.networkChartCategories");
    },
    networkChartExtraCategories()
    {
      return this.$t("dashboard.networkChartExtraCategories");
    },
  },
  methods: {
    async updateRangeValues(newRange)
    {
      console.log('Updated date range:', newRange);
      try {
        this.apiData = await apiService.getMetrics(this.systemInfo.name, newRange);
        this.initCpuChart(0);
        this.initSensorChart();
        this.initRamChart();
        this.initNetworkChart(0);
        this.initDiskChart();
        this.initSwapChart();
      } catch (error) {
        console.error("API Error:", error);
      } 
    },
    handleComboBoxSelect(option) {
      this.selectedOption = option; 
    },
    initCpuChart(index) {
      let chartData = {
        datasets: [],
        labels: [],
      };

      const colors = [
        'rgba(255, 99, 132, 1)', // red
        'rgba(54, 162, 235, 1)', // blue
        'rgba(255, 206, 86, 1)', // yellow
        'rgba(75, 192, 192, 1)', // green
        'rgba(153, 102, 255, 1)', // purple
        'rgba(255, 159, 64, 1)'  // orange
      ];


      const generateDataset = (coreIndex, color) => ({
        fill: true,
        borderColor: color,
        borderWidth: 2,
        borderDash: [],
        borderDashOffset: 0.0,
        pointBackgroundColor: color,
        pointBorderColor: "rgba(255,255,255,0)",
        pointHoverBackgroundColor: color,
        pointBorderWidth: 20,
        pointHoverRadius: 4,
        pointHoverBorderWidth: 15,
        pointRadius: 4,
        label: index === 0 ? `CPU` : `Core ${coreIndex + 1}`, 
        data: this.apiData.map(metrics => {return parseFloat(metrics.cpuCores[coreIndex].total.toFixed(2))})
      });

      //defualt chart optoins to
      this.cpuChart.extraOptions = chartConfigs.cpuChartOptions;

      if (index === 1) {//all cores
        // 0th is whole usage, next individual cores
        for (let i = 1; i < this.apiData[0].cpuCores.length; i++){
          let color = colors[i % colors.length];
          chartData.datasets.push(generateDataset(i, color));
        }
      }
      else if (index === 2)
      {
        this.cpuChart.extraOptions = chartConfigs.cpuFreqChartOptions;

        chartData.datasets.push(
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
          label: "Frequency: ",
          data: this.apiData.map(metrics => parseFloat(metrics.cpuFreq.toFixed(2)))
        }
        );
      }
      else//main cpu
      {
        chartData.datasets.push(generateDataset(0, config.colors.primary));
      }

      this.$refs.cpuChart.updateGradients(chartData);

      chartData.labels = this.apiData.map(metrics => {
        const date = new Date(metrics.time);
        const month = (date.getMonth() + 1).toString().padStart(2, '0'); 
        const day = date.getDate().toString().padStart(2, '0');
        return month + '-' + day;
      });

      this.cpuChart.chartData = chartData;  
      this.cpuChart.activeIndex = index;
    },
    initNetworkChart(index) 
    {
      const colors = [
        'rgba(255, 99, 132, 1)', // red
        'rgba(54, 162, 235, 1)', // blue
        'rgba(255, 206, 86, 1)', // yellow
        'rgba(75, 192, 192, 1)', // green
        'rgba(153, 102, 255, 1)', // purple
        'rgba(255, 159, 64, 1)',
        'rgba(255, 159, 64, 1)',
        'rgba(255, 159, 64, 1)',
        'rgba(255, 159, 64, 1)',
        'rgba(255, 159, 64, 1)'
      ];

      let chartData = {
        labels: [],
        datasets: [
          {
            fill: true,
            borderColor: colors[5],
            borderWidth: 2,
            borderDash: [],
            borderDashOffset: 0.0,
            pointBackgroundColor: colors[5],
            pointBorderColor: "rgba(255,255,255,0)",
            pointHoverBackgroundColor: colors[5],
            pointBorderWidth: 20,
            pointHoverRadius: 4,
            pointHoverBorderWidth: 15,
            pointRadius: 4,
            label: "Upload",
            data: this.apiData.map(metrics => metrics.networkMetrics.reduce((sum, nm) => sum + (nm.upload || 0), 0) / Math.pow(1024, 2))
          },
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
            label: "Download",
            data: this.apiData.map(metrics => metrics.networkMetrics.reduce((sum, nm) => sum + (nm.download || 0), 0) / Math.pow(1024, 2))
          }
        ],
      };

      const generateDataset = (ifaceIndex, color) => ({
        fill: true,
        borderColor: color,
        borderWidth: 2,
        borderDash: [],
        borderDashOffset: 0.0,
        pointBackgroundColor: color,
        pointBorderColor: "rgba(255,255,255,0)",
        pointHoverBackgroundColor: color,
        pointBorderWidth: 20,
        pointHoverRadius: 4,
        pointHoverBorderWidth: 15,
        pointRadius: 4,
        label: this.apiData[0]?.networkMetrics[ifaceIndex]?.name || `Interface ${ifaceIndex}`,
        data: this.apiData.map(metrics => {
          return this.networkChart.activeIndex === 1
            ? (metrics.networkMetrics[ifaceIndex]?.upload || 0) / Math.pow(1024, 2) // Upload
            : (metrics.networkMetrics[ifaceIndex]?.download || 0) / Math.pow(1024, 2); // Download
        })
      });


      if (index !== 0) 
      {
        chartData.datasets = [];
        for (let i = 0; i < this.apiData[0].networkMetrics.length; i++)
        {
          let color = colors[i % colors.length];
          chartData.datasets.push(generateDataset(i, color));
        }
      }

      chartData.labels = this.apiData.map(metrics => {
        const date = new Date(metrics.time);
        const month = (date.getMonth() + 1).toString().padStart(2, '0'); 
        const day = date.getDate().toString().padStart(2, '0');
        return month + '-' + day;
      });

      this.networkChart.chartData = chartData;
      this.networkChart.activeIndex = index;
    },
    initNetworkChartAllInterfaces(index) 
    {
      const colors = [
        'rgba(255, 99, 132, 1)', // red
        'rgba(54, 162, 235, 1)', // blue
        'rgba(255, 206, 86, 1)', // yellow
        'rgba(75, 192, 192, 1)', // green
        'rgba(153, 102, 255, 1)', // purple
        'rgba(255, 159, 64, 1)',
        'rgba(255, 159, 64, 1)',
        'rgba(255, 159, 64, 1)',
        'rgba(255, 159, 64, 1)',
        'rgba(255, 159, 64, 1)'
      ];

      let chartData = {
        labels: [],
        datasets: [],
      };

      const generateDataset = (ifaceIndex, color) => ({
        fill: true,
        borderColor: color,
        borderWidth: 2,
        borderDash: [],
        borderDashOffset: 0.0,
        pointBackgroundColor: color,
        pointBorderColor: "rgba(255,255,255,0)",
        pointHoverBackgroundColor: color,
        pointBorderWidth: 20,
        pointHoverRadius: 4,
        pointHoverBorderWidth: 15,
        pointRadius: 4,
        label: this.apiData[0]?.networkMetrics[ifaceIndex]?.name || `Interface ${ifaceIndex}`,
        data: this.apiData.map(metrics => {
          return this.networkChart.uploadIndex === 0
            ? (metrics.networkMetrics[ifaceIndex]?.upload || 0) / Math.pow(1024, 2) // Upload
            : (metrics.networkMetrics[ifaceIndex]?.download || 0) / Math.pow(1024, 2); // Download
        })
      });

      chartData.datasets = [];
      for (let i = 0; i < this.apiData[0].networkMetrics.length; i++)
      {
        let color = colors[i % colors.length];
        chartData.datasets.push(generateDataset(i, color));
      }
      
      chartData.labels = this.apiData.map(metrics => {
        const date = new Date(metrics.time);
        const month = (date.getMonth() + 1).toString().padStart(2, '0'); 
        const day = date.getDate().toString().padStart(2, '0');
        return month + '-' + day;
      });

      this.networkChart.chartData = chartData;
      this.networkChart.uploadIndex = index
    },
    initSensorChart() {
      let chartData = {
        datasets: [],
        labels: [],
      };

      const colors = [
        'rgba(255, 99, 132, 1)', // red
        'rgba(54, 162, 235, 1)', // blue
        'rgba(255, 206, 86, 1)', // yellow
        'rgba(75, 192, 192, 1)', // green
        'rgba(153, 102, 255, 1)', // purple
        'rgba(255, 159, 64, 1)'  // orange
      ];

      const generateDataset = (sensorIndex, color) => ({
        fill: true,
        borderColor: color,
        borderWidth: 2,
        borderDash: [],
        borderDashOffset: 0.0,
        pointBackgroundColor: color,
        pointBorderColor: "rgba(255,255,255,0)",
        pointHoverBackgroundColor: color,
        pointBorderWidth: 20,
        pointHoverRadius: 4,
        pointHoverBorderWidth: 15,
        pointRadius: 4,
        label: this.apiData[0].sensorList[sensorIndex].name,
        data: this.apiData.map(metrics => metrics.sensorList[sensorIndex].value)
      });
      
      // 0th is whole usage, next individual cores
      for (let i = 0; i < this.apiData[0].sensorList.length; i++)
      {
        let color = colors[i % colors.length];
        chartData.datasets.push(generateDataset(i, color));
      }

      this.$refs.sensorChart.updateGradients(chartData);

      chartData.labels = this.apiData.map(metrics => {
        const date = new Date(metrics.time);
        const month = (date.getMonth() + 1).toString().padStart(2, '0'); 
        const day = date.getDate().toString().padStart(2, '0');
        return month + '-' + day;
      });

      this.sensorChart.chartData = chartData;  
    },
    initDiskChart() 
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
            data: this.apiData.map(metrics => metrics.diskUsedSpace / Math.pow(1024, 3)), 
          },
        ],
      };

      chartData.labels = this.apiData.map(metrics => {
        const date = new Date(metrics.time);
        const month = (date.getMonth() + 1).toString().padStart(2, '0'); 
        const day = date.getDate().toString().padStart(2, '0');
        return month + '-' + day;
      });
      this.diskChart.chartData = chartData;
    },
    
    initRamChart() 
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
            data: this.apiData.map(metrics => metrics.ramMemUsed / Math.pow(1024, 2)), 
          },
        ],
      };

      //this.ramChart.extraOptions.scales.yAxis.ticks.suggestedMax = this.apiData.metrics[0].ramMemAvailable;

      chartData.labels = this.apiData.map(metrics => {
        const date = new Date(metrics.time);
        const month = (date.getMonth() + 1).toString().padStart(2, '0'); 
        const day = date.getDate().toString().padStart(2, '0');
        return month + '-' + day;
      });
      
      this.ramChart.chartData = chartData;
    },
    initSwapChart() 
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
            data: this.apiData.map(metrics => metrics.ramSwapUsed / Math.pow(1024, 2)), 
          },
        ],
      };

      chartData.labels = this.apiData.map(metrics => {
        const date = new Date(metrics.time);
        const month = (date.getMonth() + 1).toString().padStart(2, '0'); 
        const day = date.getDate().toString().padStart(2, '0');
        return month + '-' + day;
      });
      
      this.swapChart.chartData = chartData;
    },
    ...mapActions(['fetchSystemDetails']),
  },
  async mounted() {
    
    this.systemInfo = await apiService.getSystem(this.$route.params.systemName);
    await this.$store.dispatch('fetchSystemDetails', this.systemInfo);//store sysdetails
    //console.log(this.systemInfo.creationDate);

    const date = new Date(this.systemInfo.creationDate);
    this.minDate = `${(date.getMonth() + 1).toString().padStart(2, '0')}/${date.getDate().toString().padStart(2, '0')}/${date.getFullYear()}`;

    const today = new Date();
    this.maxDate = `${(today.getMonth() + 1).toString().padStart(2, '0')}/${today.getDate().toString().padStart(2, '0')}/${today.getFullYear()}`;

    const uptimeResult = await apiService.execCommand("uptime -p", this.systemInfo);
    this.uptime = uptimeResult.output.replace("up ", '');
    const kernelResult = await apiService.execCommand("uname -r", this.systemInfo);
    this.kernel = kernelResult.output;

    //  const kernel = apiService.execCommand("uname -r", this.systemInfo);
    //fetch metrics objects
    try {
      this.apiData = await apiService.getMetrics(this.systemInfo.name, null);
    } catch (error) {
      console.error("API Error:", error);
    } 

    this.i18n = this.$i18n;

    this.initCpuChart(0);
    this.initSensorChart();
    this.initRamChart();
    this.initNetworkChart(0);
    this.initDiskChart();
    this.initSwapChart();
  },
};
</script>
<style></style>