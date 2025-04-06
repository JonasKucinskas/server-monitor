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
            <label for="publicKey">Public Key</label>
            <input v-model="formData.publicKey" type="text" class="form-control" id="publicKey" :class="{'is-invalid': formErrors.publicKey}" required />
            <div v-if="formErrors.publicKey" class="invalid-feedback">{{ formErrors.publicKey }}</div>
          </div>


        </form>
      </Modal>
      

    </div>
  </div>
</template>

<script>
import { BaseTable } from "@/components";
import apiService from "@/services/api"; 
import Modal from "@/components/Modal";

export default {
  components: {
    BaseTable,
    Modal
  },
  
  methods: {
    async handleSubmit() {
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
      if (isValid) {

        //if (this.isAddingNetworkService || this.isCloningNetworkService)
        //{
        //  await apiService.insertNetworkService(this.formData, this.systemInfo.name);
        //  this.networkServices = await apiService.getNetworkServices(this.systemInfo.name);
        //  
        //  this.stopAutoUpdate();
        //  
        //  if (this.selectedService == null)
        //  {
        //    this.selectedService = this.networkServices[0];
        //    this.startAutoUpdate();
        //  }
//
        //  this.updateRangeValues(this.dateRange);
        //  this.startAutoUpdate();
        //}
        //else if (this.isEditingNetworkService)
        //{
        //  await apiService.updateNetworkService(this.selectedService, this.formData);
        //  this.networkServices = await apiService.getNetworkServices(this.systemInfo.name);
        //  this.selectedService = this.networkServices.find(s => s.id === this.selectedService.id) || this.networkServices[0];
//
        //}
        //this.closeModal();
      }
    },
    closeModal() {
      this.showModal = false;
    },
    addSystem(){
      this.isAddingSystem = true;
      this.showModal = true;
      this.formData = [];
    },
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
      showModal: false,
      formErrors: {},
      formData: {
        name: '',
        ip: '',
        port: '',
        publicKey: ''
      },
      apiData: [], 
      systems: []
    };
  }
};
</script>
<style></style>
