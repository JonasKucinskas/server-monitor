import Vue from 'vue';
import Vuex from 'vuex';
import createPersistedState from 'vuex-persistedstate';
Vue.use(Vuex);

export default new Vuex.Store({
  state: {
    currentSystem: null,  
  },
  mutations: {
    setCurrentSystem(state, systemData) {
      state.currentSystem = systemData;
    },
  },
  actions: {
    async fetchSystemDetails({ commit }, system) {
      try {
        commit('setCurrentSystem', system);
      } catch (error) {
        console.error('Error setting system details:', error);
      }
    },
  },
  getters: {
    currentSystem(state) {
      return state.currentSystem;
    },
  },
  plugins: [
    createPersistedState({
      storage: window.localStorage,
    }),
  ],
});