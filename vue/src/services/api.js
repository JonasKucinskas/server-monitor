import axios from "axios";

const apiClient = axios.create({
  baseURL: "http://localhost:9000/api",
  timeout: 10000000000000, 
});

apiClient.interceptors.request.use(config => {
  const token = localStorage.getItem('jwt_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
}, error => {
  return Promise.reject(error);
});

apiClient.interceptors.response.use(
  response => response,
  error => {
    if (error.response && error.response.status === 401) {
      localStorage.removeItem('jwt_token'); 
      window.location.href = "/login"; 
    }
    return Promise.reject(error);
  }
);

export default {
  deleteNotification(id) {

    return apiClient.delete("/notifications", {
      params: { 
        notificationId: id
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
  deleteNotifications(ruleid) {

    return apiClient.delete("/notifications", {
      params: { 
        ruleId: ruleid
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
  getLatestNotification(ruleid) {

    return apiClient.get("/notifications/latest", {
      params: { 
        ruleId: ruleid
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
  getNotifications(ruleid) {

    return apiClient.get("/notifications", {
      params: { 
        ruleId: ruleid
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
  getNotificationProcesses(notificationid) {

    return apiClient.get("/notifications/processes", {
      params: { 
        notificationId: notificationid
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
  postNotificationRule(rule, user, system) {

    const cleanRule = {
      id: 0,//ignored
      userId: user.id,
      systemIp: system.ip,
      resource: rule.resource,
      usage: rule.usage,
      timestamp: "2025-03-23T14:30:00+00:00"//ignored
    }
    return apiClient.post("/notifications/rules", cleanRule) 
    .then(response => {
      return response.data; 
    })
    .catch(error => {
      console.error("Error executing command:", error);
      throw error; 
    });
  },
  deleteNotificationRule(id) {

    return apiClient.delete("/notifications/rules", {
      params: { 
        ruleId: id 
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
  getNotificationRules(systemip, userid) {

    return apiClient.get("/notifications/rules", {
      params: { 
        systemIp: systemip, 
        userid: userid 
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
  login(username, password) {

    const auth = {
      Username: username,
      Password: password,
    };

    return apiClient.post("/auth/login", auth)
      .then(response => {
        localStorage.setItem('jwt_token', response.data.token);
        return response.data;
      })
      .catch(error => {
        console.error("Error executing command:", error);
        throw error; 
      });
  },
  register(username, password) {

    const user = {
      id: 0,//ignored
      username: username,
      password: password,
      creationDate: "2025-03-23T14:30:00+00:00"//ignored
    }

    return apiClient.post("/auth/register", user)
      .then(response => {
        return response.data;
      });
  },
  logout() {
    localStorage.removeItem('jwt_token');
    window.location.href = "/login"; 
  },
  getToken() {
    return localStorage.getItem('jwt_token');
  },
  deleteSystem(id)
  {
    return apiClient.delete("/systems", {
      params: { 
        id,
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
  updateSystem(formData, system)
  {
    const cleanSystem = {
      id: system.id,
      ip: formData.ip,
      port: formData.port,
      name: formData.name,
      ownerId: system.ownerId,
      updateInterval: formData.updateInterval,
      cretionDate: system.creationDate
    };

    return apiClient.put("/systems", cleanSystem)
    .then(response => {
      return response.data; 
    })
    .catch(error => {
      console.error("Error executing command:", error);
      throw error; 
    });
  },
  updateUser(oldpass, newpass)
  {
    const details = {
      OldPassword: oldpass,
      newPassword: newpass,
    };

    return apiClient.post("/auth/change-password", details)
    .then(response => {
      return response.data; 
    })
    .catch(error => {
      console.error("Error executing command:", error);
      throw error; 
    });
  },
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