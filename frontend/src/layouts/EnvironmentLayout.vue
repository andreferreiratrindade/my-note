<template>
  <q-layout view="lHh Lpr lFf">
    <q-header elevated>
      <q-toolbar>
        <q-btn flat dense round icon="menu" aria-label="Menu" @click="toggleLeftDrawer" />

        <q-toolbar-title> Logged </q-toolbar-title>
        <button @click="logout()">Logout</button>
        <div>{{emailUser}}</div>
      </q-toolbar>
    </q-header>

    <q-drawer v-model="leftDrawerOpen" show-if-above bordered>
        <ListDocumentComponent />
    </q-drawer>

    <q-page-container>
      <DocumentEnvironment />
    </q-page-container>
  </q-layout>
</template>

<script setup lang="ts">

import { userManager, signOutRedirect } from 'src/services/OidcClientService';

import { onMounted, ref } from 'vue';
import ListDocumentComponent from 'src/components/ListDocumentComponent.vue';
import DocumentEnvironment from 'pages/DocumentEnvironment.vue';

let emailUser : string = '';

const leftDrawerOpen = ref(false);

function toggleLeftDrawer() {
  leftDrawerOpen.value = !leftDrawerOpen.value;
}

function logout() {
  signOutRedirect();
}

onMounted(async () => {
  const fetchedUser = await userManager.getUser();
  console.log('Fetched user:', fetchedUser);
  if (fetchedUser) {
    emailUser = fetchedUser.profile.email || 'No Email';
    console.log('User email:', emailUser);
  }
});


</script>
