<template>
  <q-layout view="lHh Lpr lFf">
    <q-header elevated>
      <q-toolbar>
        <q-toolbar-title> Meu Caderno </q-toolbar-title>
        <q-btn color="primary" label="Login with Cognito" @click="loginWithCognito" />

      </q-toolbar>
    </q-header>

    <q-page-container>
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
