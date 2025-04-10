import Vue from 'vue';
import Vuex from 'vuex';
import createPersistedState from 'vuex-persistedstate';
Vue.use(Vuex);

export default new Vuex.Store({
  state: {
    currentSystem: null,  
    currentUser: null
  },
  mutations: {
    setCurrentSystem(state, systemData) {
      state.currentSystem = systemData;
    },
    setCurrentUser(state, userData) {
      state.currentUser = userData;
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
    async fetchUserDetails({ commit }, user) {
      try {
        commit('setCurrentUser', user);
      } catch (error) {
        console.error('Error setting user details:', error);
      }
    },
  },
  getters: {
    currentSystem(state) {
      return state.currentSystem;
    },
    currentUser(state) {
      return state.currentUser;
    }
  },
  plugins: [
    createPersistedState({
      storage: window.localStorage,
    }),
  ],
});