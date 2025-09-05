import { defineRouter } from '#q-app/wrappers';
import {
  createRouter,
  createWebHistory,
} from 'vue-router';
import routes from './routes';
import { userManager } from 'src/services/OidcClientService';


/*
 * If not building with SSR mode, you can
 * directly export the Router instantiation;
 *
 * The function below can be async too; either use
 * async/await or return a Promise which resolves
 * with the Router instance.
 */


export default defineRouter(function (/* { store, ssrContext } */) {


  const Router = createRouter({
    scrollBehavior: () => ({ left: 0, top: 0 }),
    routes,

    // Leave this as is and make changes in quasar.conf.js instead!
    // quasar.conf.js -> build -> vueRouterMode
    // quasar.conf.js -> build -> publicPath
    history: createWebHistory(process.env.VUE_ROUTER_BASE),
  });

   Router.beforeEach( async (to, from, next) => {
    const user = await userManager.getUser();

    if (to.meta.requiresAuth && (user == null || user.expired)) {
      next("/login");
    }

    if(to.path === "/login" && user != null && !user.expired) {
      next("/environment");
    }

    next();

  });

  return Router;
});
