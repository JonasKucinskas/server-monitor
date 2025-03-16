import axios from "axios";

const apiClient = axios.create({
  baseURL: "http://localhost:9000/api",
  timeout: 100000000, 
});

export default {
  getMetrics(systemName) {
    return apiClient.get("/metrics", {
      params: { systemName } 
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