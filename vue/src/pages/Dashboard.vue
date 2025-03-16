<template>
  <div>
    <div class="row">
      <div class="col-12">

        <card type="chart">
          <card type="header">
            <div class="col-sm-6">
              <h2 class="card-title">{{ this.systemInfo.name }}</h2>
            </div>
          </card>

          <card type="body">
            <div class="col-sm-6">
              <h2 class="card-title">{{ $t("Body") }}</h2>
            </div>
          </card>
        </card>
      </div>
    </div>
    
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
import BarChart from "@/components/Charts/BarChart";
import * as chartConfigs from "@/components/Charts/config";
import config from "@/config";
import apiService from "@/services/api";
import { mapState, mapActions } from 'vuex';

export default {
  components: {
    LineChart,
    BarChart,
  },
  data() {
    return {
      systemInfo: [],
      apiData: [], 
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
        label: coreIndex === 0 ? `Core ${coreIndex}` : `Core ${coreIndex - 1}`, 
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
    await this.$store.dispatch('fetchSystemDetails', this.systemInfo);

    //fetch metrics objects
    try {
      this.apiData = await apiService.getMetrics(this.systemInfo.name);
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
<style>
.card-title {
  text-align: left;
}
</style>
