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
//import { api } from 'boot/axios';
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

const documents = ref<Document[]>([]);

const selectDocument = (doc: Document) => {
  console.log('Selected doc:', doc);
  // 👉 navigate to document details if you want
};

const formatDefaultTitle = () => {
  const now = new Date();
  return `# ${now.getFullYear()} ${now.toLocaleString('en-US', {
    month: 'long',
  })} ${String(now.getDate()).padStart(2, '0')} - ${String(now.getHours()).padStart(
    2,
    '0',
  )}:${String(now.getMinutes()).padStart(2, '0')}`;
};

async function newDocument() {
  const newNote: NewNote = {
    title: formatDefaultTitle(),
    content: '',
  };

  try {
    const res = await api.post('/notes', newNote);
    const noteId = res.data.noteId;
    await router.push({ path: `/editNote/${noteId}` });
  } catch (err: unknown) {
    console.error('Error saving note:', err);
  }
}

onMounted(async () => {
  try {
    const res = await api.get<Document[]>('/notes');
    documents.value = res.data;
  } catch (err) {
    console.error('Error fetching documents:', err);
  }
});
</script>
