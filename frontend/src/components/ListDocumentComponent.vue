<template>

        <q-list>
            <q-item-label header> Minhas notas </q-item-label>
        <q-item
          v-for="doc in documents"
          :key="doc.noteId"
          clickable
          @click="selectDocument(doc)"
        >
          <q-item-section>
            <q-item-label>{{ doc.title }}</q-item-label>
          </q-item-section>
        </q-item>
      </q-list>
</template>

<script setup lang="ts">

//import { api } from 'boot/axios';
import { api } from 'src/boot/axios';
import {  onMounted, ref } from 'vue';
interface Document {
  noteId: string;
  title: string
}

const documents = ref<Document[]>([]);


const selectDocument = (doc: Document) => {
  console.log('Selected doc:', doc);
  // 👉 navigate to document details if you want
};

onMounted(async () => {
  try {
    const res = await api.get<Document[]>('/notes');
    debugger
    documents.value = res.data;
  } catch (err) {
    console.error('Error fetching documents:', err);
  }
});

</script>
