<template>
    <div class="WAL position-relative" :style="style">
        <q-layout view="lHh Lpr lFf" class="WAL__layout shadow-10" container>
            <q-page-container style="background: #f0f0f0f2;">
                <EditNote :noteId="noteId" v-if="noteId"
                        :key="keyEditComponent" @toggleLeftDrawer="toggleLeftDrawer"
                        @noteDeleted="NoteDeletedHandler"/>
                 <q-page class="q-pa-lg col col-12" v-if="!noteId">
                    <q-btn round flat icon="keyboard_arrow_left" class="WAL__drawer-open q-mr-sm"
                    @click="toggleLeftDrawer" />
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

                    <q-scroll-area style="height: calc(100% - 100px)">
                        <ListDocumentComponent  @noteSelected="NoteSelectedHandler"
                        ref="listDocumentRef" :noteId="noteId" :key="keyNotesListComponent"/>
                    </q-scroll-area>
                </q-drawer>
                <q-page-sticky position="bottom-right" :offset="[18, 18]">
                    <q-btn fab icon="add" color="primary" @click="newDocument"/>
                </q-page-sticky>

            </q-page-container>


        </q-layout>
    </div>
</template>

<script setup lang="ts">
import { useQuasar, Notify } from 'quasar'
import { ref, computed } from 'vue'

import { userManager, signOutRedirect } from 'src/services/OidcClientService';
import { api } from 'src/boot/axios';

import { onMounted } from 'vue';
import ListDocumentComponent from 'src/components/ListDocumentComponent.vue';
import EditNote from 'src/pages/Notes/EditNote.vue';
import { useRoute } from 'vue-router';
import { useRouter } from 'vue-router';
import { AxiosError } from 'axios';

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
const keyEditComponent = ref(0);
const keyNotesListComponent = ref(0);
function NoteSelectedHandler(selectedNoteId: string) {
    noteId.value = selectedNoteId;
    keyEditComponent.value++;
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

        Notify.create({
          message: (err instanceof AxiosError ? err.response?.data : String(err)) || 'An unknown error occurred',
          type: 'negative',
          timeout:7_000
        })
        console.error('Error saving note:', err);
    }
}

async function NoteDeletedHandler() {
    console.log('Note deleted, navigating to environment');
    noteId.value = '';
    keyNotesListComponent.value++;
    await listDocumentRef.value.getNotes();

    router.push({ path: `/environment` }).catch((err) => console.error('Router error:', err));

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


html
  width: 100%
  height: 100%

body
  width: 100%
  height: 100%
  background: linear-gradient(-45deg, #ee7752, #e73c7e, #23a6d5, #23d5ab)
  background-size: 400% 400%
  animation: gradient 15s ease infinite

@keyframes gradient
  0%
    background-position: 0% 50%

  50%
    background-position: 100% 50%

  100%
    background-position: 0% 50%

</style>
