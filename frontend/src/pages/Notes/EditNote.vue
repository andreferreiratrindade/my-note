<template>
    <q-page class="q-pa-lg col-12">
        <q-input v-model="note.title" label="Title" outlined />
        <q-input v-model="note.content" type="textarea" label="Content" outlined />

        <div class="q-mt-sm text-caption">
            <span v-if="isSaving">💾 Saving...</span>
            <span v-else-if="lastError" class="text-negative">⚠️ {{ lastError }}</span>
            <span v-else>✅ All changes saved</span>
        </div>
    </q-page>
</template>

<script lang="ts">
import { defineComponent, reactive, ref, watch, onMounted, onUnmounted } from 'vue';
import { api } from 'src/boot/axios';

interface Note {
    noteId: string;
    title: string;
    content: string;
}

export default defineComponent({
    name: 'EditNote',

    async setup() {
        const note = reactive<Note>({
            noteId: '',
            title: '',
            content: '',
        });

        let oldNote = { ...note }; // shallow copy
        let saveTimeout: number | undefined;
        const isSaving = ref(false);
        const lastError = ref<string | null>(null);
        let isSavingInProgress = false;
        onUnmounted(() => {
            if (saveTimeout) clearTimeout(saveTimeout);
            document.removeEventListener('visibilitychange', handleVisibilityChange);
            window.removeEventListener('beforeunload', handleBeforeUnload);
        });

        // --- Load note ---
        onMounted(() => {
            document.addEventListener('visibilitychange', handleVisibilityChange);
            window.addEventListener('beforeunload', handleBeforeUnload);
        });

        watch(
            () => ({ ...note }),
            () => {
                if (saveTimeout) clearTimeout(saveTimeout);
                saveTimeout = window.setTimeout(() => {
                    void saveNote();
                }, 30_000);
            },
            { deep: true },
        );

        await getNote();

        // --- Resilient Save ---
        const saveNote = async () => {
            if (oldNote.title === note.title && oldNote.content === note.content) return;

            if (isSavingInProgress) return; // avoid concurrent saves
            isSavingInProgress = true;
            isSaving.value = true;
            lastError.value = null;

            try {
                console.log('Auto-saving note...', note);
                await api.put(`/notes/${note.noteId}`, note);

                oldNote = { ...note }; // update snapshot
                console.log('Note saved!');
            } catch (err: unknown) {
                console.error('Error saving note:', err);
                if (err instanceof Error) {
                    lastError.value = err.message;
                } else {
                    lastError.value = 'Failed to save';
                }
                // simple retry after 5s
                setTimeout(() => {
                    void saveNote();
                }, 30_000);
            } finally {
                isSaving.value = false;
                isSavingInProgress = false;
            }
        };

        // --- Save when user leaves tab ---
        const handleVisibilityChange = () => {
            if (document.hidden) void saveNote();
        };

        const handleBeforeUnload = () => {
            void saveNote();
        };



        async function getNote() {
            const noteId = window.location.pathname.split('/').pop();
            if (!noteId) {
                lastError.value = 'Invalid note ID';
                return;
            }
            try {
                const res = await api.get<Note>(`/notes/${noteId}`);
                Object.assign(note, res.data);
                oldNote = { ...note }; // initialize snapshot
            } catch (err) {
                console.error('Error loading note:', err);
                lastError.value = 'Failed to load note';
            }
        }

        return { note, isSaving, lastError };
    },
});
</script>
