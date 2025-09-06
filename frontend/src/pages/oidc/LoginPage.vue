<template>
  <q-layout view="lHh Lpr lFf">
    <q-page-container>
            <q-page class="flex flex-center">
      <q-card class="q-pa-md">
        <q-btn color="primary" label="Login with Cognito" @click="loginWithCognito" />
      </q-card>
    </q-page>
      <router-view />
    </q-page-container>
  </q-layout>
</template>

<script setup lang="ts">
import { userManager } from 'src/services/OidcClientService';

const loginWithCognito = async () => {
   await userManager.signinRedirect();
};
</script>
