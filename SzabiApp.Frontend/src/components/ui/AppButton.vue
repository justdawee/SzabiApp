<script setup lang="ts">
withDefaults(defineProps<{
  variant?: 'primary' | 'secondary' | 'ghost' | 'danger'
  size?:    'sm' | 'md' | 'lg'
  loading?: boolean
  disabled?: boolean
  type?:    'button' | 'submit' | 'reset'
}>(), {
  variant: 'primary',
  size:    'md',
  loading: false,
  disabled: false,
  type:    'button',
})
</script>

<template>
  <button
    :type="type"
    :disabled="disabled || loading"
    :class="[
      'inline-flex items-center justify-center gap-2 font-medium rounded-xl transition-all duration-200 cursor-pointer select-none',
      'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-500',
      'disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none',
      size === 'sm' && 'text-sm px-3 py-1.5',
      size === 'md' && 'text-sm px-4 py-2.5',
      size === 'lg' && 'text-base px-6 py-3',
      variant === 'primary'   && 'btn-gradient text-white',
      variant === 'secondary' && 'glass-sm text-slate-700 dark:text-slate-200 hover:bg-white/70 dark:hover:bg-white/10 active:scale-95',
      variant === 'ghost'     && 'text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-white/8 active:scale-95',
      variant === 'danger'    && 'bg-rose-500 hover:bg-rose-600 text-white shadow-lg shadow-rose-500/30 active:scale-95',
    ]"
  >
    <svg v-if="loading" class="animate-spin -ml-1 w-4 h-4" fill="none" viewBox="0 0 24 24">
      <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
      <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8z" />
    </svg>
    <slot />
  </button>
</template>
