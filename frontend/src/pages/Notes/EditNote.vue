<template>
    <q-page class="q-pa-lg col-12">
        <q-input v-model="note.title" label="Title" outlined @keydown="markDirty" />
        <q-input v-model="note.content" type="textarea" label="Content" outlined @keydown="markDirty" />

        <div class="q-mt-sm text-caption">
            <span v-if="isSaving">💾 Saving...</span>
            <span v-else-if="lastError" class="text-negative">⚠️ {{ lastError }}</span>
            <span v-else>✅ All changes saved</span>
        </div>
    </q-page>
</template>

<script setup lang="ts">
import { reactive, ref, onMounted, onUnmounted } from "vue";
import { api } from "src/boot/axios";
import { watchDebounced } from "@vueuse/core";

interface Note {
    noteId: string;

    title: string;
    content: string;
}

const note = reactive<Note>({
    noteId: "",
    title: "",
    content: "",
});

let oldNote: Note = { ...note };
const isSaving = ref(false);
const lastError = ref<string | null>(null);

 const props = defineProps({
      noteId: {
        type: String,
        required: true
      }
    });
// --- Mark dirty (user started typing) ---
function markDirty() {
      isSaving.value = true;
};
// --- API: Load note ---
async function getNote() {
    const noteId = props.noteId;
    if (!noteId) {
        lastError.value = "Invalid note ID";
        return;
    }

    try {
        const res = await api.get<Note>(`/notes/${noteId}`);
        Object.assign(note, res.data);
        oldNote = { ...note };
    } catch (err) {
        console.error("Error loading note:", err);
        lastError.value = "Failed to load note";
    }
};

// --- API: Save note ---
async function saveNote() {
    if (oldNote.title === note.title && oldNote.content === note.content) return;

    isSaving.value = true;
    lastError.value = null;

    try {
        await api.put(`/notes/${note.noteId}`, note);
        oldNote = { ...note };
        console.log("Note saved!");
    } catch (err) {
        console.error("Error saving note:", err);
        lastError.value =
            err instanceof Error ? err.message : "Failed to save note";

        // retry once after 30s
        setTimeout(() => {
            void saveNote();
        }, 30_000);
    } finally {
        isSaving.value = false;
    }
};



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
    { deep: true, debounce: 3000 } // 3s debounce feels better than 30s
);

onMounted(async () => {
    await getNote();
    document.addEventListener("visibilitychange", handleVisibilityChange);
    window.addEventListener("beforeunload", handleBeforeUnload);
});

onUnmounted(() => {
    document.removeEventListener("visibilitychange", handleVisibilityChange);
    window.removeEventListener("beforeunload", handleBeforeUnload);
});


</script>
