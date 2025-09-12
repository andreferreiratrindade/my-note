import { defineBoot } from '#q-app/wrappers';
import axios, { type AxiosInstance } from 'axios';
import  config  from 'src/config/config';
import { userManager } from 'src/services/OidcClientService';
declare module 'vue' {
  interface ComponentCustomProperties {
    $axios: AxiosInstance;
    $api: AxiosInstance;
  }
}

const api = axios.create({ baseURL: config.API_BASE_URL.toString() || '' });

export default defineBoot(({ app }) => {

 // Add Authorization header if token exists
  api.interceptors.request.use(async (config) => {
    //const token = localStorage.getItem('access_token');

    const user = await userManager.getUser();
       // debugger
      if (user) {
        config.headers = config.headers || {};
        config.headers.Authorization = `Bearer ${user.access_token}`;
      }
    ;
    return config;
  });

  // Make Axios available globally
  app.config.globalProperties.$axios = axios;
  app.config.globalProperties.$api = api;
});

export { api };
