<template>
<q-editor v-model="contentEditor" min-height="5rem" />

</template>

<script lang="ts">
import {
  defineComponent

} from 'vue';


export default defineComponent({
  name: 'DocumentEditorContentComponent',
  props: {
    content: {
      type: String,
      required: true
      }
    },
    data() {
      return {
        contentEditor: this.content
      };
    }
  });
</script>
