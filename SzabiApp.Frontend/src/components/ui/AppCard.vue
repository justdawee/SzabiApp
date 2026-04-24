<script setup lang="ts">
defineProps<{
  title?: string
  subtitle?: string
  padding?: boolean
}>()
</script>

<template>
  <div class="glass rounded-2xl overflow-hidden">
    <div v-if="title || $slots.header" class="px-6 pt-6 pb-4 border-b border-black/5 dark:border-white/5">
      <slot name="header">
        <h3 class="text-base font-semibold text-slate-800 dark:text-slate-100">{{ title }}</h3>
        <p v-if="subtitle" class="text-sm text-slate-500 dark:text-slate-400 mt-0.5">{{ subtitle }}</p>
      </slot>
    </div>
    <div :class="padding === false ? '' : 'p-6'">
      <slot />
    </div>
  </div>
</template>
