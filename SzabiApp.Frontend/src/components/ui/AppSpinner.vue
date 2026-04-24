<script setup lang="ts">
defineProps<{ size?: 'sm' | 'md' | 'lg' }>()
</script>

<template>
  <svg
    :class="['animate-spin text-indigo-500', {
      'w-4 h-4': size === 'sm',
      'w-6 h-6': !size || size === 'md',
      'w-10 h-10': size === 'lg',
    }]"
    fill="none"
    viewBox="0 0 24 24"
  >
    <circle class="opacity-20" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
    <path class="opacity-80" fill="currentColor" d="M4 12a8 8 0 018-8v8z" />
  </svg>
</template>
