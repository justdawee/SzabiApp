<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { leavesService }   from '@/services/leaves.service'
import { holidaysService } from '@/services/holidays.service'
import { useToastStore }   from '@/stores/toast.store'
import { LeaveCategory, LeaveStatus } from '@/types'
import type { CalendarEvent, CalendarRange } from '@/types'
import AppCalendar from '@/components/ui/AppCalendar.vue'
import AppButton   from '@/components/ui/AppButton.vue'
import AppSelect   from '@/components/ui/AppSelect.vue'
import { extractApiError } from '@/utils/apiError'

const router  = useRouter()
const toast   = useToastStore()

// ─── State ────────────────────────────────────────────────────────────────────

const range    = ref<CalendarRange>({ start: null, end: null })
const category = ref<LeaveCategory>(LeaveCategory.Annual)
const note     = ref('')
const loading  = ref(false)
const events   = ref<CalendarEvent[]>([])

// ─── Load calendar events (my existing leaves + holidays) ─────────────────────

onMounted(async () => {
  try {
    const [leaves, holidays] = await Promise.all([
      leavesService.getMy(),
      holidaysService.getAll(),
    ])

    const leaveEvents: CalendarEvent[] = leaves
      .filter((l) => l.status !== LeaveStatus.Denied && l.status !== LeaveStatus.Cancelled)
      .map((l) => ({
        date:    l.startDate.split('T')[0],
        endDate: l.endDate.split('T')[0],
        type:    l.status === LeaveStatus.Approved
                   ? (l.category === LeaveCategory.Sick ? 'sick' : 'approved')
                   : 'pending',
        label: `${categoryLabel(l.category)} (${statusLabel(l.status)})`,
      }))

    const holidayEvents: CalendarEvent[] = holidays.map((h) => ({
      date:  h.date.split('T')[0],
      type:  'holiday',
      label: h.name,
    }))

    events.value = [...holidayEvents, ...leaveEvents]
  } catch {
    // Calendar works without events if the API is unavailable
  }
})

// ─── Computed ─────────────────────────────────────────────────────────────────

const today = new Date().toISOString().split('T')[0]

const dayCount = computed(() => {
  if (!range.value.start || !range.value.end) return 0
  const diff = new Date(range.value.end).getTime() - new Date(range.value.start).getTime()
  return Math.round(diff / 86_400_000) + 1
})

const formattedRange = computed(() => {
  if (!range.value.start) return null
  const fmt = (d: string) => new Date(d).toLocaleDateString('hu-HU', { month: 'long', day: 'numeric' })
  if (!range.value.end || range.value.end === range.value.start) return fmt(range.value.start)
  return `${fmt(range.value.start)} – ${fmt(range.value.end)}`
})

const categoryOptions = [
  { value: LeaveCategory.Annual,    label: 'Éves szabadság' },
  { value: LeaveCategory.Sick,      label: 'Betegszabadság' },
  { value: LeaveCategory.Unpaid,    label: 'Fizetés nélküli' },
  { value: LeaveCategory.Paternity, label: 'Apasági szabadság' },
  { value: LeaveCategory.Maternity, label: 'Anyasági szabadság' },
  { value: LeaveCategory.Other,     label: 'Egyéb' },
]

// ─── Helpers ──────────────────────────────────────────────────────────────────

function categoryLabel(c: string) {
  return categoryOptions.find((o) => o.value === c)?.label ?? c
}
function statusLabel(s: string) {
  return { Pending: 'Függőben', Approved: 'Jóváhagyva', Denied: 'Elutasítva', Cancelled: 'Visszavonva' }[s] ?? s
}

// ─── Submit ───────────────────────────────────────────────────────────────────

async function submit() {
  if (!range.value.start || !range.value.end) {
    toast.warning('Kérjük válassz ki egy dátum intervallumot a naptáron.')
    return
  }
  loading.value = true
  try {
    await leavesService.create({
      category:    category.value,
      startDate:   range.value.start,
      endDate:     range.value.end,
      requestNote: note.value || undefined,
    })
    toast.success('Szabadságkérelem sikeresen beadva!')
    router.push('/leaves')
  } catch (e: unknown) {
    toast.error(extractApiError(e, 'Hiba történt a kérelem beadásakor.'))
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="max-w-5xl mx-auto space-y-4">
    <!-- Back button + title -->
    <div class="flex items-center gap-3">
      <AppButton variant="ghost" size="sm" @click="router.back()">
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
        </svg>
        Vissza
      </AppButton>
      <div>
        <h2 class="text-xl font-bold text-slate-800 dark:text-white">Új szabadságkérelem</h2>
        <p class="text-sm text-slate-500 dark:text-slate-400">Válaszd ki a kívánt időszakot a naptáron</p>
      </div>
    </div>

    <!-- Two-column layout: calendar left, form right -->
    <div class="grid grid-cols-1 lg:grid-cols-[1fr_320px] gap-4 items-start">

      <!-- ─── Calendar ─────────────────────────────────────────────────── -->
      <AppCalendar
        v-model="range"
        :events="events"
        :min-date="today"
      />

      <!-- ─── Side panel ────────────────────────────────────────────────── -->
      <div class="glass rounded-2xl p-5 space-y-5 sticky top-20">

        <!-- Selection summary -->
        <div>
          <p class="text-xs font-semibold uppercase tracking-wide text-slate-400 dark:text-slate-500 mb-3">
            Kiválasztott időszak
          </p>

          <Transition name="fade" mode="out-in">
            <div v-if="!range.start" key="empty" class="text-center py-6">
              <div class="w-12 h-12 rounded-2xl bg-indigo-50 dark:bg-indigo-500/10 flex items-center justify-center mx-auto mb-2">
                <svg class="w-6 h-6 text-indigo-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                    d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
                </svg>
              </div>
              <p class="text-sm text-slate-400 dark:text-slate-500">Kattints a naptárra<br/>a kezdő dátum kiválasztásához</p>
            </div>

            <div v-else-if="!range.end" key="selecting" class="text-center py-4">
              <p class="text-sm font-medium text-indigo-600 dark:text-indigo-400">
                Kezdő: {{ new Date(range.start).toLocaleDateString('hu-HU', { month: 'long', day: 'numeric' }) }}
              </p>
              <p class="text-xs text-slate-400 mt-1">Most válaszd ki a záró dátumot</p>
              <div class="mt-3 flex gap-1 justify-center">
                <div class="w-2 h-2 rounded-full bg-indigo-500 animate-bounce" style="animation-delay:0ms"/>
                <div class="w-2 h-2 rounded-full bg-indigo-400 animate-bounce" style="animation-delay:150ms"/>
                <div class="w-2 h-2 rounded-full bg-indigo-300 animate-bounce" style="animation-delay:300ms"/>
              </div>
            </div>

            <div v-else key="selected" class="space-y-3">
              <!-- Date range display -->
              <div class="glass-sm rounded-xl p-3 text-center">
                <p class="text-sm font-semibold text-slate-800 dark:text-slate-100">{{ formattedRange }}</p>
                <div class="mt-2 flex items-center justify-center gap-1">
                  <span class="text-2xl font-bold text-transparent bg-clip-text" style="background-image:linear-gradient(135deg,#6366f1,#a855f7)">
                    {{ dayCount }}
                  </span>
                  <span class="text-sm text-slate-500">nap</span>
                </div>
              </div>
              <!-- Reset button -->
              <button
                class="w-full text-xs text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 transition-colors text-center"
                @click="range = { start: null, end: null }"
              >
                ✕ Kiválasztás törlése
              </button>
            </div>
          </Transition>
        </div>

        <!-- Divider -->
        <div class="border-t border-black/5 dark:border-white/5" />

        <!-- Form fields -->
        <div class="space-y-4">
          <AppSelect
            v-model="category"
            label="Szabadság típusa"
            :options="categoryOptions"
            required
          />

          <div class="flex flex-col gap-1.5">
            <label class="text-sm font-medium text-slate-700 dark:text-slate-300">
              Megjegyzés <span class="text-slate-400 font-normal">(opcionális)</span>
            </label>
            <textarea
              v-model="note"
              rows="3"
              placeholder="Indoklás vagy megjegyzés a vezetőnek..."
              class="neuro w-full rounded-xl px-4 py-2.5 text-sm text-slate-800 dark:text-slate-100
                     placeholder:text-slate-400 dark:placeholder:text-slate-600
                     outline-none focus:ring-2 focus:ring-indigo-500/40 resize-none"
            />
          </div>
        </div>

        <!-- Submit -->
        <AppButton
          variant="primary"
          size="lg"
          :loading="loading"
          :disabled="!range.start || !range.end"
          class="w-full"
          @click="submit"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8"/>
          </svg>
          Kérelem beadása
        </AppButton>

        <!-- Helper text -->
        <p class="text-xs text-center text-slate-400">
          A kérelmet a vezető értesítést kap és jóváhagyja vagy elutasítja.
        </p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.fade-enter-active, .fade-leave-active { transition: all 0.2s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; transform: translateY(4px); }
</style>
