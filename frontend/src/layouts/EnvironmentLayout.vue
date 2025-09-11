<template>
    <div class="WAL position-relative bg-grey-4" :style="style">
        <q-layout view="lHh Lpr lFf" class="WAL__layout shadow-3" container>
            <q-page-container class="bg-grey-2">
                <EditNote :noteId="noteId" v-if="noteId" :key="count" @toggleLeftDrawer="toggleLeftDrawer"/>
                 <q-page class="q-pa-lg col col-12" v-if="!noteId">
                    <div class="q-gutter-md flex items-end justify-center" style="padding-top: 50%;">
                             <q-btn color="primary"  class="col-12"    icon="note_add" label="Novo" align="center" @click="newDocument"/>
                    </div>
                </q-page>
                <q-drawer v-model="leftDrawerOpen" show-if-above bordered :breakpoint="690">
                    <q-toolbar class="bg-grey-3">

                        <span class="q-subtitle-1">
                            {{emailUser}}
                        </span>

                        <q-space />
                            <q-btn round flat icon="more_vert">
                            <q-menu auto-close :offset="[110, 8]">
                                <q-list style="min-width: 150px">

                                    <q-item clickable>
                                        <q-item-section @click="logout">Logout</q-item-section>
                                    </q-item>
                                </q-list>
                            </q-menu>
                        </q-btn>
                        <q-btn round flat icon="close" class="WAL__drawer-close" @click="toggleLeftDrawer" />

                    </q-toolbar>

                    <q-toolbar class="bg-grey-2 flex items-end justify-center">

                        <q-btn color="primary" class="col-12"  flat  icon="note_add" label="Novo" align="center" @click="newDocument"/>

                    </q-toolbar>

                    <q-scroll-area style="height: calc(100% - 100px)">
                        <ListDocumentComponent :noteId="noteId" @noteSelected="NoteSelectedHandler" ref="listDocumentRef"/>
                    </q-scroll-area>
                </q-drawer>

            </q-page-container>


        </q-layout>
    </div>
</template>

<script setup lang="ts">
import { useQuasar } from 'quasar'
import { ref, computed } from 'vue'

import { userManager, signOutRedirect } from 'src/services/OidcClientService';
import { api } from 'src/boot/axios';

import { onMounted } from 'vue';
import ListDocumentComponent from 'src/components/ListDocumentComponent.vue';
import EditNote from 'src/pages/Notes/EditNote.vue';
import { useRoute } from 'vue-router';
import { useRouter } from 'vue-router';

const router = useRouter();

interface NewNote {
    title: string;
    content: string;
}
const listDocumentRef = ref();
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


if (route.params.noteId) {
    noteId.value = route.params.noteId as string;
}

const formatDefaultTitle = () => {
    const now = new Date();
    return `# ${String(now.getDate()).padStart(2, '0')} ${now.toLocaleString('pt-BR', { month: 'long' })} ${now.getFullYear()} - ${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}:${String(now.getSeconds()).padStart(2, '0')}`;
};
async function newDocument() {
    const newNote: NewNote = {
        title: formatDefaultTitle(),
        content: '',
    };

    try {
        const res = await api.post('/notes', newNote);
        await listDocumentRef.value.getNotes();
        const noteId = res.data.noteId;

        router.push({ path: `/editNote/${noteId}` }).catch((err) => console.error('Router error:', err));

        NoteSelectedHandler(noteId);
    } catch (err: unknown) {
        console.error('Error saving note:', err);
    }
}


const $q = useQuasar()


const style = computed(() => ({
    height: $q.screen.height + 'px'
}))



</script>

<style lang="sass">
.WAL
  width: 100%
  height: 100%
  padding-top: 20px
  padding-bottom: 20px

  &:before
    content: ''
    height: 127px
    position: fixed
    top: 0
    width: 100%
    background-color: #007a96

  &__layout
    margin: 0 auto
    z-index: 4000
    height: 100%
    width: 90%
    max-width: 950px
    border-radius: 5px

  &__field.q-field--outlined .q-field__control:before
    border: none

  .q-drawer--standard
    .WAL__drawer-close
      display: none

@media (max-width: 850px)
  .WAL
    padding: 0
    &__layout
      width: 100%
      border-radius: 0

@media (min-width: 691px)
  .WAL
    &__drawer-open
      display: none

.conversation__summary
  margin-top: 4px

.conversation__more
  margin-top: 0!important
  font-size: 1.4rem
</style>
