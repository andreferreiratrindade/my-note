<template>
  <q-layout view="lHh Lpr lFf">

    <q-page-container>
            <q-page class="flex flex-center">
    <div>Processing login...</div>

      </q-page>
      <router-view />
    </q-page-container>
  </q-layout>
</template>


<script setup lang="ts">
import { userManager } from "src/services/OidcClientService";
import { onMounted } from "vue";
import { useRouter } from "vue-router";

const router = useRouter();

onMounted(async () => {
  const user = await userManager.signinCallback();
  if (user) {
    await router.push("/environment");
  } else {
    await router.push("/login");
  }
});
</script>
