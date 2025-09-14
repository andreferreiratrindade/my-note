<template>
    <div>
        <q-toolbar class="bg-grey-2">
         <div style="font-size: medium;">
            My Notes
        </div>
        </q-toolbar>

        <q-list >
            <q-item v-for="doc in documents"
                    :key="doc.noteId"
                    clickable @click="selectDocument(doc)"
                    v-ripple :active="(noteIdSelected===doc.noteId)"
                    active-class="note-selected ">
                <q-item-section >
                    <q-item-label>{{ doc.title }}</q-item-label>
                </q-item-section>
                <q-item-section side>
                    <q-icon name="keyboard_arrow_right" :style="{ color: (noteIdSelected === doc.noteId ? 'white' : 'inherit') }"/>
                </q-item-section>
            </q-item>
        </q-list>
    </div>
</template>

<script setup lang="ts">


import { api } from 'src/boot/axios';
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';


const router = useRouter();
const isProcessing = ref(true);
interface Document {
    noteId: string;
    title: string;
}

const props = defineProps({
    noteId: {
        type: String,
        required: false,
    },
});

const noteIdSelected = ref('');


const documents = ref<Document[]>([]);
const emit = defineEmits(['noteSelected']);
defineExpose({ getNotes });

function selectDocument(doc: Document) {

    router.push({ path: `/editNote/${doc.noteId}` }).catch((err) => console.error('Router error:', err));

    emitNoteSelected(doc.noteId);
}

function emitNoteSelected(noteId: string) {
    noteIdSelected.value = noteId
    emit('noteSelected', noteId);
}



async function getNotes() {
    try {
        isProcessing.value = true;
        const res = await api.get<Document[]>('/notes');
        documents.value = res.data;
    } catch (err) {
        console.error('Error fetching documents:', err);
    } finally {
        isProcessing.value = false;
    }
}


onMounted(async () => {
    await getNotes();
    noteIdSelected.value = props.noteId|| ""
});


</script>

<style lang="sass">
.note-selected
  color: white
  background: #23d5ab
</style>
