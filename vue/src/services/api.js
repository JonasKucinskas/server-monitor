import axios from "axios";

const apiClient = axios.create({
  baseURL: "http://localhost:9000/api",
  timeout: 10000000000000, 
});

export default {
  getLatestNetworkServicePing(serviceId)
  {
    return apiClient.get("/networkServices/pings/latest", {
      params: { 
        serviceId,
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
  updateNetworkService(service, serviceData) {
    
    const cleanService = {
      id: service.id,
      systemId: service.systemId,
      name: serviceData.name,
      ip: serviceData.ip,
      port: serviceData.port,
      interval: serviceData.interval, 
      timeout: serviceData.timeout,
      expected_status: serviceData.expected_status,
      last_checked: service.last_checked
    }

    return apiClient.put(`/networkServices/`, cleanService)
      .then(response => {
        return response.data
      })
      .catch(error => {
        console.error("Error executing command:", error);
        throw error; 
      });
  },
  deleteNetworkService(serviceId)
  {
    return apiClient.delete("/networkServices", {
      params: { 
        serviceId,
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
  insertNetworkService(networkService, system_name)
  {
    const cleanService = {
      id: 0,//ignored
      systemId: system_name,
      name: networkService.name,
      ip: networkService.ip,
      port: networkService.port,
      interval: networkService.interval, 
      timeout: networkService.timeout,
      expected_status: networkService.expected_status,
      last_checked: "2025-03-23T14:30:00+00:00"//ignored
    }
  
    console.log(cleanService);

    return apiClient.post(`/networkServices`, cleanService)
    .then(response => {
        return response.data;
    })
    .catch(error => {
        console.error("Error executing command:", error);
        throw error;
    });
  },
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
  getLatestMetrics(systemName) {
    return apiClient.get("/metrics/latest", {
      params: { 
        systemName,
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
    
    const payload = {
      command: command, 
      SystemInfo: cleanSystem 
    };
  
    return apiClient.post(`/command/execute`, payload)
      .then(response => {
        return response.data;
      })
      .catch(error => {
        console.error("Error executing command:", error);
        throw error;
      }
    );
  },
  getPublicKey(){
    return apiClient.get("/key", {
    })
    .then(response => {
      return response.data; 
    })
    .catch(error => {
      console.error("Error executing command:", error);
      throw error; 
    });
  },
  postSystem(system) {

    const cleanSystem = {
      ip: system.ip,
      port: system.port,
      name: system.name,
      updateInterval: system.updateInterval,
      ownerId: 0 
    };

    return apiClient.post("/systems", cleanSystem)
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