import axios from "axios";

const apiClient = axios.create({
  baseURL: "http://localhost:9000/api",
  timeout: 100000000, 
});

export default {
  getNetworkServicePings(serviceId, dateRange) {
    let startDate = null;
    let endDate = null;

    if (dateRange)
    {
      startDate = new Date(dateRange.startDate).toISOString();
      endDate = new Date(dateRange.endDate).toISOString();
    }
    
    return apiClient.get("/networkServices/pings", {
      params: { 
        serviceId,
        startDate: startDate ? startDate : null, 
        endDate: endDate ? endDate : null 
      }
    })
    .then(response => {
      return response.data; 
    })
    .catch(error => {
      console.error("Error executing command:", error);
      throw error; 
    });
  },
  getNetworkServices(systemId){
    return apiClient.get("/networkServices", {
      params: { 
        systemId
      }
    })
    .then(response => {
      return response.data; 
    })
    .catch(error => {
      console.error("Error executing command:", error);
      throw error; 
    });
  },
  getMetrics(systemName, dateRange) {

    let startDate = null;
    let endDate = null;

    if (dateRange)
    {
      startDate = new Date(dateRange.startDate).toISOString();
      endDate = new Date(dateRange.endDate).toISOString();
    }
    
    return apiClient.get("/metrics", {
      params: { 
        systemName,
        startDate: startDate ? startDate : null, 
        endDate: endDate ? endDate : null 
      }
    })
    .then(response => {
      return response.data; 
    })
    .catch(error => {
      console.error("Error executing command:", error);
      throw error; 
    });
  },
  execCommand(command, system) {
    const cleanSystem = {
        id: system.id,
        ip: system.ip,
        port: system.port,
        name: system.name,
        ownerId: system.ownerId, 
        creationDate: system.creationDate
    };
    
    return apiClient.post(`/command/execute?Command=${encodeURIComponent(command)}`, cleanSystem)
    .then(response => {
        return response.data;
    })
    .catch(error => {
        console.error("Error executing command:", error);
        throw error;
    });
  },
  getSystems(userId) {
    return apiClient.get("/systems/all", {
      params: { userId } 
    })
    .then(response => {
      return response.data; 
    })
    .catch(error => {
      console.error("Error executing command:", error);
      throw error; 
    });
  },
  getSystem(name) {
    return apiClient.get("/systems/byname", {
      params: { name } 
    })
    .then(response => {
      return response.data; 
    })
    .catch(error => {
      console.error("Error executing command:", error);
      throw error; 
    });
  }
};