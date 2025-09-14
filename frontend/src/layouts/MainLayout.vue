
<template>
  <q-layout view="lHh Lpr lFf">
   <q-page-container >

  <q-page class="q-pa-lg flex flex-center">
    <div class="text-center">
      <h1 class="text-h3 q-mb-md">My Notes</h1>
      <p class="text-subtitle1 q-mb-lg">
        A simple and fast way to capture your ideas, with <b>autosave</b> built in.
      </p>

      <div class="q-mb-lg">
        <q-card class="q-pa-md q-mb-md" style="background: rgb(240 240 240 / 49%);">
          <q-card-section >
            <h2 class="text-h6">Features</h2>
            <ul class="text-left q-pl-md">
              <li>Autosaves as you type</li>
              <li>Clean and minimal interface</li>
              <li>Secure and resilient backend</li>
              <li>Accessible anywhere</li>
            </ul>
          </q-card-section>
        </q-card>
      </div>

        <q-btn color="accent" label="Start Writing Notes"  size="lg" @click="loginWithCognito" to="/environment" no-caps/>
    <p class="text-caption q-mt-md">
        v1.0 • Built by <a href="https://www.linkedin.com/in/andr%C3%A9-ferreira-trindade-6883846a/" target="_blank"> André Trindade</a>
      </p>
        <q-btn color="primary" label="Architecture"  to="/architecture" no-caps/>

    </div>
  </q-page>
    </q-page-container>
  </q-layout>
</template>

<script setup lang="ts">
import { userManager } from 'src/services/OidcClientService';

const loginWithCognito = async () => {
   await userManager.signinRedirect();
};

</script>

<style lang="sass">


html
  width: 100%
  height: 100%

body
  width: 100%
  height: 100%
  background: linear-gradient(-45deg, #ee7752, #e73c7e, #23a6d5, #23d5ab)
  background-size: 400% 400%
  animation: gradient 15s ease infinite

@keyframes gradient
  0%
    background-position: 0% 50%

  50%
    background-position: 100% 50%

  100%
    background-position: 0% 50%
</style>
