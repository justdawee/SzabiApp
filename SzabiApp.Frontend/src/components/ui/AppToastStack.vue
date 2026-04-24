<script setup lang="ts">
import { useToastStore } from '@/stores/toast.store'

const store = useToastStore()

const icons: Record<string, string> = {
  success: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z',
  error:   'M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z',
  warning: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z',
  info:    'M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z',
}

const colorMap: Record<string, string> = {
  success: 'text-emerald-500 dark:text-emerald-400',
  error:   'text-rose-500    dark:text-rose-400',
  warning: 'text-amber-500   dark:text-amber-400',
  info:    'text-indigo-500  dark:text-indigo-400',
}
</script>

<template>
  <div class="fixed bottom-6 right-6 z-[100] flex flex-col gap-3 pointer-events-none">
    <TransitionGroup name="toast">
      <div
        v-for="t in store.toasts"
        :key="t.id"
        class="glass pointer-events-auto rounded-2xl px-4 py-3 flex items-start gap-3 min-w-72 max-w-sm cursor-pointer"
        @click="store.remove(t.id)"
      >
        <svg class="w-5 h-5 mt-0.5 shrink-0" :class="colorMap[t.type]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" :d="icons[t.type]" />
        </svg>
        <p class="text-sm text-slate-700 dark:text-slate-200 leading-relaxed">{{ t.message }}</p>
      </div>
    </TransitionGroup>
  </div>
</template>

<style scoped>
.toast-enter-active, .toast-leave-active { transition: all 0.3s cubic-bezier(0.34,1.56,0.64,1); }
.toast-enter-from { opacity: 0; transform: translateX(40px); }
.toast-leave-to   { opacity: 0; transform: translateX(40px); }
</style>
