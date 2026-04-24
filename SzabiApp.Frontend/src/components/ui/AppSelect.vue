<script setup lang="ts">
defineProps<{
  label?:    string
  options:   { value: string | number; label: string }[]
  error?:    string
  required?: boolean
}>()

const model = defineModel<string | number>()
</script>

<template>
  <div class="flex flex-col gap-1.5">
    <label v-if="label" class="text-sm font-medium text-slate-700 dark:text-slate-300">
      {{ label }}<span v-if="required" class="text-rose-500 ml-0.5">*</span>
    </label>
    <select
      v-model="model"
      :class="[
        'neuro w-full rounded-xl px-4 py-2.5 text-sm text-slate-800 dark:text-slate-100',
        'outline-none transition-all duration-200 cursor-pointer',
        'focus:ring-2 focus:ring-indigo-500/40',
        error && 'ring-2 ring-rose-400/40',
      ]"
    >
      <option v-for="opt in options" :key="opt.value" :value="opt.value">
        {{ opt.label }}
      </option>
    </select>
    <p v-if="error" class="text-xs text-rose-500">{{ error }}</p>
  </div>
</template>
