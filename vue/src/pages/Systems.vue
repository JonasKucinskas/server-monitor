<template>
  <div class="row">

    <div class="col-12">
      <card type="header">
        <div class="col-sm-6">
          <h2 class="card-title">{{ $t("systems.header") }}</h2>
          <h4 class="card-title">{{ $t("systems.footer") }}</h4>
        </div>
      </card>

      <div class="row">
        <div class="col-sm-12">
          <card type="tasks">
            <template slot="header">
            </template>
            <div class="table-full-width table-responsive">
              <template>
                <base-table
                  :columns="['Name', 'IP', 'PORT']"
                  :data="systems"
                  type="hover"
                  thead-classes="text-primary"
                >
                  <template slot-scope="{ row }">
                    <tr @click="goToDashboard(row)">
                      <td>{{ row.name }}</td>
                      <td>{{ row.ip }}</td>
                      <td>{{ row.port }}</td>
                    </tr>
                  </template>
                </base-table>
              </template>
            </div>
          </card>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { BaseTable } from "@/components";
import apiService from "@/services/api"; 

export default {
  components: {
    BaseTable
  },
  
  methods: {
    goToDashboard(system) {
      this.$router.push({ name: 'dashboard', params: { systemName: system.name } });
    },
    formatApiData()
    {
      this.systems = this.apiData.map(system => ({
        name: system.name,
        ip: system.ip,
        port: system.port,
      }));
    }
  },
  async mounted() {
    try {
      this.apiData = await apiService.getSystems(0);
    } catch (error) {
      console.error("API Error:", error);
    } 
    this.formatApiData();
  },
  data() {
    return {
      apiData: [], 
      systems: []
    };
  }
};
</script>
<style></style>
