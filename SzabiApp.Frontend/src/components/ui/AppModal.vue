<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import AppButton from './AppButton.vue'

const props = defineProps<{
  title: string
  open:  boolean
}>()

const emit = defineEmits<{ close: [] }>()

function onKey(e: KeyboardEvent) {
  if (e.key === 'Escape' && props.open) emit('close')
}
onMounted(() => document.addEventListener('keydown', onKey))
onUnmounted(() => document.removeEventListener('keydown', onKey))
</script>

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="open"
        class="fixed inset-0 z-50 flex items-center justify-center p-4"
        @click.self="emit('close')"
      >
        <!-- Backdrop -->
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm dark:bg-black/50" />

        <!-- Panel -->
        <div class="glass relative w-full max-w-md rounded-2xl z-10 overflow-hidden">
          <div class="flex items-center justify-between px-6 py-4 border-b border-black/5 dark:border-white/5">
            <h3 class="text-base font-semibold text-slate-800 dark:text-slate-100">{{ title }}</h3>
            <AppButton variant="ghost" size="sm" @click="emit('close')">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </AppButton>
          </div>
          <div class="p-6">
            <slot />
          </div>
          <div v-if="$slots.footer" class="px-6 pb-6 flex justify-end gap-3">
            <slot name="footer" />
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal-enter-active, .modal-leave-active { transition: all 0.2s ease; }
.modal-enter-from, .modal-leave-to { opacity: 0; }
.modal-enter-from .glass, .modal-leave-to .glass { transform: scale(0.96) translateY(8px); }
</style>
