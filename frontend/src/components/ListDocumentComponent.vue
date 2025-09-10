<template>
  <div>
    <button @click="newDocument">New Document</button>
    <q-list>
      <q-item-label header> Minhas notas </q-item-label>
      <q-item v-for="doc in documents" :key="doc.noteId" clickable @click="selectDocument(doc)">
        <q-item-section>
          <q-item-label>{{ doc.title }}</q-item-label>
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

interface Document {
  noteId: string;
  title: string;
}
interface NewNote {
  title: string;
  content: string;
}

const noteIdSelected = ref('');

const documents = ref<Document[]>([]);
const emit = defineEmits(['noteSelected']);

function selectDocument(doc: Document) {
  console.log('Selected doc:', doc);
  router.push({ path: `/editNote/${doc.noteId}` }).catch((err) => console.error('Router error:', err));

  emitNoteSelected(doc.noteId);
}

function emitNoteSelected(noteId: string) {
 noteIdSelected.value = noteId
  emit('noteSelected', noteId);
}

const formatDefaultTitle = () => {
  const now = new Date();
  return `# ${String(now.getDate()).padStart(2, '0')} ${now.toLocaleString('pt-BR', {month: 'long'})} ${now.getFullYear()} - ${String(now.getHours()).padStart(2, '0')} :${String(now.getMinutes()).padStart(2, '0')} :${String(now.getSeconds()).padStart(2, '0')}`;
};

async function newDocument() {
  const newNote: NewNote = {
    title: formatDefaultTitle(),
    content: '',
  };

  try {
    const res = await api.post('/notes', newNote);
    const noteId = res.data.noteId;
    getNotes().catch((err) => console.error('Error refreshing notes:', err));
    router.push({ path: `/editNote/${noteId}` }).catch((err) => console.error('Router error:', err));
    emitNoteSelected(noteId);
  } catch (err: unknown) {
    console.error('Error saving note:', err);
  }
}

async function getNotes() {
  try {
    const res = await api.get<Document[]>('/notes');
    documents.value = res.data;
  } catch (err) {
    console.error('Error fetching documents:', err);
  }
}

onMounted(async () => {
    await getNotes();
});


</script>
