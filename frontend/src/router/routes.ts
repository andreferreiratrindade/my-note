import type { RouteRecordRaw } from 'vue-router';

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('layouts/MainLayout.vue'),
    children: [{ path: '', component: () => import('pages/IndexPage.vue') }],
  },
  {
    path: "/login",
    component: () => import("pages/oidc/LoginPage.vue")
  },
  {
    path: "/callback",
    component: () => import("src/pages/oidc/CallBackPage.vue")
  },
  {
    path: '/environment',
    component: () => import('layouts/EnvironmentLayout.vue'), meta: { requiresAuth: true },
    children: [{ path: '/editNote/:noteId', component: () => import('pages/Notes/EditNote.vue') }
    ],
  },

  // Always leave this as last one,
  // but you can also remove it
  {
    path: '/:catchAll(.*)*',
    component: () => import('pages/ErrorNotFound.vue'),
  },
];

export default routes;
