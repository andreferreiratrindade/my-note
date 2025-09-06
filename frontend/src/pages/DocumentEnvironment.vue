
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
import { defineComponent, reactive, ref, watch, onMounted, onUnmounted } from "vue";
import { api } from "src/boot/axios";
    const formatDefaultTitle = () => {
      const now = new Date();
      return `# ${now.getFullYear()} ${now.toLocaleString("en-US", {
        month: "long",
      })} ${String(now.getDate()).padStart(2, "0")} - ${String(
        now.getHours()
      ).padStart(2, "0")}:${String(now.getMinutes()).padStart(2, "0")}`;
    };
interface Note {
  noteId: string;
  title: string;
  content: string;
}

export default defineComponent({
  name: "MyNoteEditor",

  setup() {
    const note = reactive<Note>({
      noteId: "",
      title: formatDefaultTitle(),
      content: "",
    });

    let oldNote = { ...note }; // shallow copy
    let saveTimeout: number | undefined;
    const isSaving = ref(false);
    const lastError = ref<string | null>(null);
    let isSavingInProgress = false;

    // --- Resilient Save ---
    const saveNote = async () => {
      if (
        (oldNote.title === note.title && oldNote.content === note.content)
      ) return;

      if (isSavingInProgress) return; // avoid concurrent saves
      isSavingInProgress = true;
      isSaving.value = true;
      lastError.value = null;

      try {
        console.log("Auto-saving note...", note);

        if (note.noteId) {
          await api.put(`/notes/${note.noteId}`, note);
        } else {
          const res = await api.post("/notes", note);
          note.noteId = res.data.noteId;
        }

        oldNote = { ...note }; // update snapshot
        console.log("Note saved!");
      } catch (err: unknown) {
        console.error("Error saving note:", err);
        if (err instanceof Error) {
          lastError.value = err.message;
        } else {
          lastError.value = "Failed to save";
        }
        // simple retry after 5s
        setTimeout(() => {
          void saveNote();
        }, 5000);
      } finally {
        isSaving.value = false;
        isSavingInProgress = false;
      }
    };

    // --- Auto-save after 30s idle ---
    watch(
      () => ({ ...note }),
      () => {
        if (saveTimeout) clearTimeout(saveTimeout);
        saveTimeout = window.setTimeout(() => {
          void saveNote();
        }, 5_000);
      },
      { deep: true }
    );

    // --- Save when user leaves tab ---
    const handleVisibilityChange = () => {
      if (document.hidden) void saveNote();
    };

    const handleBeforeUnload = () => {
      void saveNote();
    };

    onMounted(() => {
      document.addEventListener("visibilitychange", handleVisibilityChange);
      window.addEventListener("beforeunload", handleBeforeUnload);
    });

    onUnmounted(() => {
      if (saveTimeout) clearTimeout(saveTimeout);
      document.removeEventListener("visibilitychange", handleVisibilityChange);
      window.removeEventListener("beforeunload", handleBeforeUnload);
    });

    return { note, isSaving, lastError };
  },
});
</script>
