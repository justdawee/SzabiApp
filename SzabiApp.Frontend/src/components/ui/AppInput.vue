<script setup lang="ts">
defineProps<{
  label?:       string
  placeholder?: string
  type?:        string
  error?:       string
  hint?:        string
  required?:    boolean
}>()

const model = defineModel<string>()
</script>

<template>
  <div class="flex flex-col gap-1.5">
    <label v-if="label" class="text-sm font-medium text-slate-700 dark:text-slate-300">
      {{ label }}<span v-if="required" class="text-rose-500 ml-0.5">*</span>
    </label>
    <input
      v-model="model"
      :type="type ?? 'text'"
      :placeholder="placeholder"
      :required="required"
      :class="[
        'neuro w-full rounded-xl px-4 py-2.5 text-sm text-slate-800 dark:text-slate-100',
        'placeholder:text-slate-400 dark:placeholder:text-slate-600',
        'outline-none transition-all duration-200',
        'focus:ring-2 focus:ring-indigo-500/40 focus:border-indigo-400',
        error && 'ring-2 ring-rose-400/40 border-rose-400',
      ]"
    />
    <p v-if="error" class="text-xs text-rose-500">{{ error }}</p>
    <p v-else-if="hint" class="text-xs text-slate-400">{{ hint }}</p>
  </div>
</template>
