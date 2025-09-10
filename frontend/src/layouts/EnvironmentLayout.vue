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
        <ListDocumentComponent :noteId="noteId" @noteSelected="NoteSelectedHandler" />
    </q-drawer>

    <q-page-container>
        <EditNote :noteId="noteId" v-if="noteId" :key="count"/>
    </q-page-container>
  </q-layout>
</template>

<script setup lang="ts">

import { userManager, signOutRedirect } from 'src/services/OidcClientService';

import { onMounted, ref } from 'vue';
import ListDocumentComponent from 'src/components/ListDocumentComponent.vue';
import EditNote from 'src/pages/Notes/EditNote.vue';
import { useRoute } from 'vue-router';

const route = useRoute();
const emailUser = ref('Loading...');
const noteId = ref(''); // Example noteId, replace with actual logic to get noteId
const leftDrawerOpen = ref(false);
let count = 0;
function NoteSelectedHandler(selectedNoteId: string) {
  noteId.value = selectedNoteId;
  count++;
}

function toggleLeftDrawer() {
  leftDrawerOpen.value = !leftDrawerOpen.value;
}

function logout() {
  signOutRedirect();
}

onMounted(async () => {
  const fetchedUser = await userManager.getUser();

  if (fetchedUser) {
    emailUser.value = fetchedUser.profile.email || 'No Email';
  }

});

debugger
  if (route.params.noteId) {
    noteId.value = route.params.noteId as string;
  }
</script>
