<template>
    <div>
        <q-header elevated>
            <q-toolbar class="bg-grey-3 text-black">
                <q-btn round flat icon="keyboard_arrow_left" class="WAL__drawer-open q-mr-sm"
                    @click="toggleLeftDrawer" />
                <div class="q-gutter-sm flex items-center">
                    <span class="q-subtitle-1">
                        {{ note.title }}
                    </span>
                </div>
                <q-space />
                <q-btn icon="delete" color="negative" round flat  @click="deleteNote" />
            </q-toolbar>
        </q-header>
        <q-page class="q-pa-lg col col-12">
            <div class="q-gutter-md">
                <q-input square borderless bg-color="white" input-style="padding-left: 10px !important ;"
                    v-model="note.title" @keydown="markDirty" class="col-12 q-ml-md" :blur="saveNote" />
                <q-editor v-model="note.content" min-height="50vh" autogrow flat :blur="saveNote"
                    @keydown="markDirty" />
            </div>
        </q-page>
    </div>
</template>

<script setup lang="ts">
import { reactive, ref, onMounted, onUnmounted } from 'vue';
const emit = defineEmits(['toggleLeftDrawer', 'noteDeleted']);


// Define the toggleLeftDrawer method
function toggleLeftDrawer() {
    emit('toggleLeftDrawer');
}
import { api } from 'src/boot/axios';
import { watchDebounced } from '@vueuse/core';

interface Note {
    noteId: string;

    title: string;
    content: string;
}

const note = reactive<Note>({
    noteId: '',
    title: '',
    content: '',
});

let oldNote: Note = { ...note };
const isSaving = ref(false);
const lastError = ref<string | null>(null);

const props = defineProps({
    noteId: {
        type: String,
        required: true,
    },
});
// --- Mark dirty (user started typing) ---
function markDirty() {
    isSaving.value = true;
}
// --- API: Load note ---
async function getNote() {
    const noteId = props.noteId;
    if (!noteId) {
        lastError.value = 'Invalid note ID';
        return;
    }

    try {
        const res = await api.get<Note>(`/notes/${noteId}`);
        Object.assign(note, res.data);
        oldNote = { ...note };
    } catch (err) {
        console.error('Error loading note:', err);
        lastError.value = 'Failed to load note';
    }
}

// --- API: Save note ---
async function saveNote() {
    if (oldNote.title === note.title && oldNote.content === note.content) return;

    isSaving.value = true;
    lastError.value = null;

    try {
        await api.put(`/notes/${note.noteId}`, note);
        oldNote = { ...note };
    } catch (err) {
        console.error('Error saving note:', err);
        lastError.value = err instanceof Error ? err.message : 'Failed to save note';

        // retry once after 30s
        setTimeout(() => {
            void saveNote();
        }, 30_000);
    } finally {
        isSaving.value = false;
    }
}

// --- Lifecycle: load & setup listeners ---
function handleVisibilityChange() {
    if (document.hidden) void saveNote();
}

function handleBeforeUnload() {
    void saveNote();
}

watchDebounced(
    () => ({ ...note }),
    () => {
        void saveNote();
    },
    { deep: true, debounce: 3000 }, // 3s debounce feels better than 30s
);

onMounted(async () => {
    await getNote();
    document.addEventListener('visibilitychange', handleVisibilityChange);
    window.addEventListener('beforeunload', handleBeforeUnload);
});

onUnmounted(() => {
    void saveNote();
    document.removeEventListener('visibilitychange', handleVisibilityChange);
    window.removeEventListener('beforeunload', handleBeforeUnload);
});

async function deleteNote() {
    if (!confirm('Are you sure you want to delete this note?')) return;

    await api.delete(`/notes/${note.noteId}`)

     emit('noteDeleted');

}


</script>
