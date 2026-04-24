<script setup lang="ts">
import { computed } from 'vue'
import { LeaveStatus, LeaveCategory } from '@/types'

const props = defineProps<{
  value: string
  type?: 'status' | 'category'
}>()

const classes = computed(() => {
  if (props.type === 'status') {
    return {
      [LeaveStatus.Pending]:   'bg-amber-100  text-amber-700  dark:bg-amber-500/15  dark:text-amber-300',
      [LeaveStatus.Approved]:  'bg-emerald-100 text-emerald-700 dark:bg-emerald-500/15 dark:text-emerald-300',
      [LeaveStatus.Denied]:    'bg-rose-100   text-rose-700   dark:bg-rose-500/15   dark:text-rose-300',
      [LeaveStatus.Cancelled]: 'bg-slate-100  text-slate-500  dark:bg-slate-500/15  dark:text-slate-400',
    }[props.value] ?? 'bg-slate-100 text-slate-500'
  }
  return {
    [LeaveCategory.Annual]:    'bg-indigo-100  text-indigo-700 dark:bg-indigo-500/15 dark:text-indigo-300',
    [LeaveCategory.Sick]:      'bg-orange-100  text-orange-700 dark:bg-orange-500/15 dark:text-orange-300',
    [LeaveCategory.Unpaid]:    'bg-slate-100   text-slate-600  dark:bg-slate-500/15  dark:text-slate-400',
    [LeaveCategory.Paternity]: 'bg-sky-100     text-sky-700    dark:bg-sky-500/15    dark:text-sky-300',
    [LeaveCategory.Maternity]: 'bg-pink-100    text-pink-700   dark:bg-pink-500/15   dark:text-pink-300',
    [LeaveCategory.Other]:     'bg-teal-100    text-teal-700   dark:bg-teal-500/15   dark:text-teal-300',
  }[props.value] ?? 'bg-slate-100 text-slate-500'
})

const label = computed(() => {
  const labels: Record<string, string> = {
    Pending: 'Függőben', Approved: 'Jóváhagyva', Denied: 'Elutasítva', Cancelled: 'Visszavonva',
    Annual: 'Éves', Sick: 'Betegszabadság', Unpaid: 'Fizetés nélküli',
    Paternity: 'Apasági', Maternity: 'Anyasági', Other: 'Egyéb',
  }
  return labels[props.value] ?? props.value
})
</script>

<template>
  <span :class="['inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium', classes]">
    {{ label }}
  </span>
</template>
