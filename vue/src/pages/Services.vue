<template>
  <div class="row">
    <div class="col-md-12">
      <card type="services">
        <template slot="header">
          
          <base-dropdown
            menu-on-right=""
            tag="div"
            title-classes="btn btn-link btn-icon"
            aria-label="Settings menu"
          >
            <i slot="title" class="tim-icons icon-settings-gear-63"></i>
            <a class="dropdown-item" href="#pablo">{{ $t("dashboard.dropdown.action") }}</a>
            <a class="dropdown-item" href="#pablo">{{ $t("dashboard.dropdown.anotherAction") }}</a>
            <a class="dropdown-item" href="#pablo">{{ $t("dashboard.dropdown.somethingElse") }}</a>
          </base-dropdown>
        </template>
        <div class="table-full-width table-responsive">
          <template>
            <base-table
            :columns="['CGroup', 'Tasks', 'CPU', 'Memory', 'Input', 'Output']"
            :data="tableData"
            thead-classes="text-primary"
            >
              <template slot-scope="{ row }">
                <td>{{ row.CGroup }}</td>
                <td>{{ row.Tasks }}</td>
                <td>{{ row.CPU }}</td>
                <td>{{ row.Memory }}</td>
                <td>{{ row.Input }}</td>
                <td>{{ row.Output }}</td>
              </template>
            </base-table>
          </template>
        </div>
      </card>
    </div>
  </div>
</template>

<script>
import { BaseTable } from "@/components";
import apiService from "@/services/api"; 

export default {
  currentSystem: null,
  components: {
    BaseTable,
  },
  data() {
    return {
      tableData: [], 
    };
  },
  methods: {
    async loadData() {
      try {
        console.log(this.currentSystem);
        const response = await apiService.execCommand('systemd-cgtop', this.currentSystem); 
        this.tableData = this.formatData(response.output);
      } catch (error) {
        console.error("API Error:", error);
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
  },
  mounted() {
    this.currentSystem = this.$store.getters.currentSystem;
    this.loadData();
  },
};
</script>

<style></style>
