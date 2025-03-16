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
  execCommand(command) {
    return apiClient.post('/command/execute', { command })
      .then(response => {
        return response.data; 
      })
      .catch(error => {
        console.error("Error executing command:", error);
        throw error; 
      });
  },
  getSystems(userId) {
    return apiClient.get("/systems", {
      params: { userId } 
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