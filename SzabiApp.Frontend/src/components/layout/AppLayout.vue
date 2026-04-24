<script setup lang="ts">
import { RouterView } from 'vue-router'
import AppSidebar from './AppSidebar.vue'
import AppTopBar from './AppTopBar.vue'
import AppBottomNav from './AppBottomNav.vue'
</script>

<template>
  <div class="gradient-mesh min-h-screen flex">
    <!-- Sidebar (desktop) -->
    <AppSidebar class="hidden lg:flex" />

    <!-- Main area -->
    <div class="flex-1 flex flex-col min-w-0">
      <AppTopBar />

      <main class="flex-1 p-4 md:p-6 pb-24 lg:pb-6 overflow-auto">
        <RouterView v-slot="{ Component }">
          <Transition name="page" mode="out-in">
            <component :is="Component" />
          </Transition>
        </RouterView>
      </main>
    </div>

    <!-- Bottom nav (mobile) -->
    <AppBottomNav class="lg:hidden" />
  </div>
</template>

<style scoped>
.page-enter-active, .page-leave-active { transition: all 0.2s ease; }
.page-enter-from { opacity: 0; transform: translateY(8px); }
.page-leave-to   { opacity: 0; transform: translateY(-4px); }
</style>
