<script setup lang="ts">
import { ref, computed } from 'vue'
import type { CalendarEvent, CalendarRange } from '@/types'

// ─── Props & Emits ────────────────────────────────────────────────────────────

const props = withDefaults(defineProps<{
  modelValue: CalendarRange
  events?:    CalendarEvent[]
  /** Empty string = no minimum (display-only mode). Omit = defaults to today. */
  minDate?:   string
  maxDate?:   string
}>(), {
  events: () => [],
})

const emit = defineEmits<{
  'update:modelValue': [CalendarRange]
}>()

// ─── Helpers ──────────────────────────────────────────────────────────────────

function toDateStr(d: Date) {
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

function addDays(dateStr: string, n: number) {
  const d = new Date(dateStr)
  d.setDate(d.getDate() + n)
  return toDateStr(d)
}

// ─── View state ───────────────────────────────────────────────────────────────

const today         = toDateStr(new Date())
const displayYear   = ref(new Date().getFullYear())
const displayMonth  = ref(new Date().getMonth())        // 0-indexed
const hoverDate     = ref<string | null>(null)

const HU_DAYS   = ['H', 'K', 'Sz', 'Cs', 'P', 'Sz', 'V']
const HU_MONTHS = ['Január','Február','Március','Április','Május','Június',
                   'Július','Augusztus','Szeptember','Október','November','December']

// ─── Calendar grid (always 6 rows × 7 cols = 42 cells) ───────────────────────

interface DayCell {
  date:           Date
  dateStr:        string
  isCurrentMonth: boolean
  isWeekend:      boolean
}

const calendarDays = computed<DayCell[]>(() => {
  const yr = displayYear.value
  const mo = displayMonth.value
  const first = new Date(yr, mo, 1)
  const last  = new Date(yr, mo + 1, 0)

  // Shift: Mon = 0
  let startDow = first.getDay() - 1
  if (startDow < 0) startDow = 6

  const cells: DayCell[] = []

  for (let i = startDow; i > 0; i--) {
    const d = new Date(yr, mo, 1 - i)
    cells.push({ date: d, dateStr: toDateStr(d), isCurrentMonth: false, isWeekend: isWeekend(d) })
  }
  for (let i = 1; i <= last.getDate(); i++) {
    const d = new Date(yr, mo, i)
    cells.push({ date: d, dateStr: toDateStr(d), isCurrentMonth: true, isWeekend: isWeekend(d) })
  }
  const remaining = 42 - cells.length
  for (let i = 1; i <= remaining; i++) {
    const d = new Date(yr, mo + 1, i)
    cells.push({ date: d, dateStr: toDateStr(d), isCurrentMonth: false, isWeekend: isWeekend(d) })
  }
  return cells
})

function isWeekend(d: Date) { return d.getDay() === 0 || d.getDay() === 6 }

// ─── Events lookup (expand ranges to individual days) ─────────────────────────

const eventsByDate = computed(() => {
  const map = new Map<string, CalendarEvent[]>()

  for (const ev of props.events) {
    const end = ev.endDate ?? ev.date
    let cur   = ev.date
    while (cur <= end) {
      if (!map.has(cur)) map.set(cur, [])
      map.get(cur)!.push(ev)
      cur = addDays(cur, 1)
    }
  }
  return map
})

// ─── Selection helpers ────────────────────────────────────────────────────────

const isSelecting = computed(() => !!props.modelValue.start && !props.modelValue.end)

/** Effective end for hover preview */
function effectiveEnd() {
  if (props.modelValue.end) return props.modelValue.end
  if (isSelecting.value && hoverDate.value) {
    const s = props.modelValue.start!
    return hoverDate.value >= s ? hoverDate.value : s
  }
  return props.modelValue.start
}

function rangeStart() {
  const { start } = props.modelValue
  const end = effectiveEnd()
  if (!start || !end) return start
  return start <= end ? start : end
}
function rangeEnd() {
  const { start } = props.modelValue
  const end = effectiveEnd()
  if (!start || !end) return end
  return start <= end ? end : start
}

function isDayStart(ds: string)   { return ds === rangeStart() }
function isDayEnd(ds: string)     { return ds === rangeEnd() && rangeEnd() !== rangeStart() }
function isDaySingle(ds: string)  { return ds === rangeStart() && rangeStart() === rangeEnd() }
function isDayInRange(ds: string) {
  const s = rangeStart(); const e = rangeEnd()
  if (!s || !e) return false
  return ds > s && ds < e
}
function isDisabled(ds: string) {
  // If minDate is explicitly '' → display-only, no restriction
  const min = props.minDate === '' ? null : (props.minDate ?? toDateStr(new Date()))
  if (min && ds < min) return true
  if (props.maxDate && ds > props.maxDate) return true
  return false
}

// ─── Click handler ────────────────────────────────────────────────────────────

function onDayClick(ds: string) {
  if (isDisabled(ds)) return
  const { start, end } = props.modelValue

  if (!start || end) {
    // Fresh start
    emit('update:modelValue', { start: ds, end: null })
  } else if (ds >= start) {
    // Complete range
    emit('update:modelValue', { start, end: ds })
  } else {
    // Clicked before start → reset with new start
    emit('update:modelValue', { start: ds, end: null })
  }
}

// ─── Navigation ───────────────────────────────────────────────────────────────

function prevMonth() {
  if (displayMonth.value === 0) { displayMonth.value = 11; displayYear.value-- }
  else displayMonth.value--
}
function nextMonth() {
  if (displayMonth.value === 11) { displayMonth.value = 0; displayYear.value++ }
  else displayMonth.value++
}

// ─── Event colour map ─────────────────────────────────────────────────────────

const eventBgClass: Record<string, string> = {
  approved:  'bg-emerald-400 dark:bg-emerald-500',
  pending:   'bg-amber-400   dark:bg-amber-500',
  holiday:   'bg-rose-400    dark:bg-rose-500',
  denied:    'bg-slate-400   dark:bg-slate-500',
  cancelled: 'bg-slate-300   dark:bg-slate-600',
  sick:      'bg-orange-400  dark:bg-orange-500',
  other:     'bg-teal-400    dark:bg-teal-500',
}

const eventBgTint: Record<string, string> = {
  approved: 'bg-emerald-50 dark:bg-emerald-500/10',
  pending:  'bg-amber-50   dark:bg-amber-500/10',
  holiday:  'bg-rose-50    dark:bg-rose-500/10',
  sick:     'bg-orange-50  dark:bg-orange-500/10',
}
</script>

<template>
  <div class="glass rounded-2xl overflow-hidden select-none">
    <!-- ── Header: month nav ─────────────────────────────────────────────── -->
    <div class="flex items-center justify-between px-5 py-4 border-b border-black/5 dark:border-white/5">
      <button
        class="w-8 h-8 rounded-xl flex items-center justify-center text-slate-500 hover:text-indigo-600 hover:bg-indigo-50 dark:hover:bg-indigo-500/10 transition-all"
        @click="prevMonth"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
        </svg>
      </button>

      <h3 class="text-sm font-semibold text-slate-800 dark:text-slate-100 tracking-wide">
        {{ HU_MONTHS[displayMonth] }} {{ displayYear }}
      </h3>

      <button
        class="w-8 h-8 rounded-xl flex items-center justify-center text-slate-500 hover:text-indigo-600 hover:bg-indigo-50 dark:hover:bg-indigo-500/10 transition-all"
        @click="nextMonth"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
        </svg>
      </button>
    </div>

    <!-- ── Day headers ───────────────────────────────────────────────────── -->
    <div class="grid grid-cols-7 px-3 pt-3 pb-1">
      <div
        v-for="(d, i) in HU_DAYS"
        :key="d + i"
        :class="['text-center text-[11px] font-semibold tracking-wide pb-1',
          i >= 5 ? 'text-rose-400 dark:text-rose-500' : 'text-slate-400 dark:text-slate-500']"
      >
        {{ d }}
      </div>
    </div>

    <!-- ── Calendar grid ─────────────────────────────────────────────────── -->
    <div class="grid grid-cols-7 px-3 pb-4 gap-y-0.5">
      <div
        v-for="day in calendarDays"
        :key="day.dateStr"
        class="relative flex flex-col items-center isolate"
        @mouseenter="hoverDate = day.dateStr"
        @mouseleave="hoverDate = null"
      >
        <!-- ── Range bar (horizontal fill behind the circle) ── -->
        <div class="absolute top-0 h-8 inset-x-0 flex pointer-events-none z-0">
          <!-- In-range: full bar -->
          <div
            v-if="isDayInRange(day.dateStr)"
            class="flex-1 bg-indigo-50 dark:bg-indigo-500/10"
          />
          <!-- Start: right half only with rounded left cap -->
          <template v-else-if="isDayStart(day.dateStr) && !isDaySingle(day.dateStr)">
            <div class="flex-1" />
            <div class="flex-1 bg-indigo-50 dark:bg-indigo-500/10 rounded-l-full" />
          </template>
          <!-- End: left half only with rounded right cap -->
          <template v-else-if="isDayEnd(day.dateStr)">
            <div class="flex-1 bg-indigo-50 dark:bg-indigo-500/10 rounded-r-full" />
            <div class="flex-1" />
          </template>
        </div>

        <!-- ── Event tint background (behind circle, only for non-selected) ── -->
        <div
          v-if="!isDayStart(day.dateStr) && !isDayEnd(day.dateStr) && !isDaySingle(day.dateStr) && !isDayInRange(day.dateStr)"
          class="absolute inset-1 rounded-full pointer-events-none z-0"
          :class="eventsByDate.get(day.dateStr)?.[0] ? eventBgTint[eventsByDate.get(day.dateStr)![0].type] : ''"
        />

        <!-- ── Day button ── -->
        <button
          :disabled="isDisabled(day.dateStr)"
          :class="[
            'relative z-10 w-8 h-8 flex items-center justify-center rounded-full text-[13px] font-medium transition-all duration-150',
            // Disabled / other month
            isDisabled(day.dateStr) && 'opacity-25 cursor-not-allowed',
            !day.isCurrentMonth && !isDisabled(day.dateStr)
              && !isDayStart(day.dateStr) && !isDayEnd(day.dateStr) && !isDaySingle(day.dateStr) && !isDayInRange(day.dateStr)
              && 'opacity-35',
            // Today ring (not selected)
            day.dateStr === today && !isDayStart(day.dateStr) && !isDayEnd(day.dateStr) && !isDaySingle(day.dateStr)
              && 'ring-2 ring-indigo-400 dark:ring-indigo-500 ring-offset-1 ring-offset-transparent',
            // Selected start / end / single
            (isDayStart(day.dateStr) || isDayEnd(day.dateStr) || isDaySingle(day.dateStr))
              && 'btn-gradient text-white shadow-md shadow-indigo-500/30',
            // In range
            isDayInRange(day.dateStr)
              && 'text-indigo-700 dark:text-indigo-200',
            // Weekend colour
            day.isWeekend && !isDayStart(day.dateStr) && !isDayEnd(day.dateStr) && !isDaySingle(day.dateStr) && !isDayInRange(day.dateStr)
              && !isDisabled(day.dateStr) && 'text-rose-400 dark:text-rose-400',
            // Normal hoverable
            !isDisabled(day.dateStr) && !isDayStart(day.dateStr) && !isDayEnd(day.dateStr) && !isDaySingle(day.dateStr)
              && 'hover:bg-indigo-100 dark:hover:bg-indigo-500/15 cursor-pointer',
            // Default text
            !isDayStart(day.dateStr) && !isDayEnd(day.dateStr) && !isDaySingle(day.dateStr) && !isDayInRange(day.dateStr) && !day.isWeekend
              && 'text-slate-700 dark:text-slate-200',
          ]"
          @click="onDayClick(day.dateStr)"
        >
          {{ day.date.getDate() }}
        </button>

        <!-- ── Event dots ── -->
        <div class="absolute bottom-1.5 flex gap-0.5 h-1 items-center justify-center z-20 pointer-events-none">
          <template v-for="(ev, i) in (eventsByDate.get(day.dateStr) ?? []).slice(0, 3)" :key="i">
            <span
              :class="['w-1 h-1 rounded-full shadow-sm', eventBgClass[ev.type] ?? 'bg-slate-400']"
              :title="ev.label"
            />
          </template>
        </div>
      </div>
    </div>

    <!-- ── Legend ─────────────────────────────────────────────────────────── -->
    <div class="border-t border-black/5 dark:border-white/5 px-5 py-3 flex flex-wrap gap-x-4 gap-y-1.5">
      <span class="flex items-center gap-1.5 text-[11px] text-slate-500 dark:text-slate-400">
        <span class="w-2 h-2 rounded-full bg-indigo-500" />Kiválasztott
      </span>
      <span class="flex items-center gap-1.5 text-[11px] text-slate-500 dark:text-slate-400">
        <span class="w-2 h-2 rounded-full bg-emerald-400" />Jóváhagyott
      </span>
      <span class="flex items-center gap-1.5 text-[11px] text-slate-500 dark:text-slate-400">
        <span class="w-2 h-2 rounded-full bg-amber-400" />Függőben
      </span>
      <span class="flex items-center gap-1.5 text-[11px] text-slate-500 dark:text-slate-400">
        <span class="w-2 h-2 rounded-full bg-rose-400" />Ünnepnap
      </span>
      <span class="flex items-center gap-1.5 text-[11px] text-slate-500 dark:text-slate-400">
        <span class="w-2 h-2 rounded-full bg-orange-400" />Betegszabadság
      </span>
    </div>
  </div>
</template>
