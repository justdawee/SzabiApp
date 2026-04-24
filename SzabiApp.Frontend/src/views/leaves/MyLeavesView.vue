<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { leavesService }   from '@/services/leaves.service'
import { holidaysService } from '@/services/holidays.service'
import { useToastStore }   from '@/stores/toast.store'
import type { LeaveRequestDto, CalendarEvent, CalendarRange } from '@/types'
import { LeaveStatus, LeaveCategory } from '@/types'
import AppBadge    from '@/components/ui/AppBadge.vue'
import AppButton   from '@/components/ui/AppButton.vue'
import AppSpinner  from '@/components/ui/AppSpinner.vue'
import AppModal    from '@/components/ui/AppModal.vue'
import AppCalendar from '@/components/ui/AppCalendar.vue'

const toast  = useToastStore()
const router = useRouter()

const requests   = ref<LeaveRequestDto[]>([])
const loading    = ref(true)
const cancelling = ref<string | null>(null)
const confirmId  = ref<string | null>(null)
const viewMode   = ref<'list' | 'calendar'>('list')
const events     = ref<CalendarEvent[]>([])

// Calendar range is read-only here (display only, no selection needed)
const dummyRange = ref<CalendarRange>({ start: null, end: null })

onMounted(async () => {
  try {
    const [reqs, holidays] = await Promise.all([
      leavesService.getMy(),
      holidaysService.getAll(),
    ])
    requests.value = reqs

    const leaveEvents: CalendarEvent[] = reqs
      .filter((r) => r.status !== LeaveStatus.Denied && r.status !== LeaveStatus.Cancelled)
      .map((r) => ({
        date:    r.startDate.split('T')[0],
        endDate: r.endDate.split('T')[0],
        type:    r.status === LeaveStatus.Approved
                   ? (r.category === LeaveCategory.Sick ? 'sick' : 'approved')
                   : 'pending',
        label: `${categoryLabel(r.category)} (${statusLabel(r.status)})`,
      }))

    const holidayEvents: CalendarEvent[] = holidays.map((h) => ({
      date:  h.date.split('T')[0],
      type:  'holiday',
      label: h.name,
    }))

    events.value = [...holidayEvents, ...leaveEvents]
  } catch {
    toast.error('Nem sikerült betölteni a kérelmeket.')
  } finally {
    loading.value = false
  }
})

async function cancelRequest() {
  if (!confirmId.value) return
  cancelling.value = confirmId.value
  confirmId.value  = null
  try {
    await leavesService.cancel(cancelling.value)
    const idx = requests.value.findIndex((r) => r.id === cancelling.value)
    if (idx !== -1) requests.value[idx].status = LeaveStatus.Cancelled
    toast.success('Kérelem visszavonva.')
  } catch {
    toast.error('Nem sikerült visszavonni a kérelmet.')
  } finally {
    cancelling.value = null
  }
}

const sorted = computed(() =>
  [...requests.value].sort((a, b) => b.createdAt.localeCompare(a.createdAt))
)

function formatDate(d: string) {
  return new Date(d).toLocaleDateString('hu-HU', { year: 'numeric', month: 'long', day: 'numeric' })
}

function days(start: string, end: string) {
  return Math.round((new Date(end).getTime() - new Date(start).getTime()) / 86_400_000) + 1
}

function categoryLabel(c: string) {
  return { Annual: 'Éves', Sick: 'Beteg', Unpaid: 'Fizetés nélk.', Paternity: 'Apasági', Maternity: 'Anyasági', Other: 'Egyéb' }[c] ?? c
}
function statusLabel(s: string) {
  return { Pending: 'Függőben', Approved: 'Jóváhagyva', Denied: 'Elutasítva', Cancelled: 'Visszavonva' }[s] ?? s
}
</script>

<template>
  <div class="space-y-6 max-w-4xl mx-auto">
    <!-- Header -->
    <div class="flex items-center justify-between flex-wrap gap-3">
      <div>
        <h2 class="text-xl font-bold text-slate-800 dark:text-white">Szabadságaim</h2>
        <p class="text-sm text-slate-500 dark:text-slate-400 mt-0.5">{{ requests.length }} kérelem összesen</p>
      </div>
      <div class="flex gap-2">
        <!-- View toggle -->
        <div class="glass-sm rounded-xl p-1 flex gap-1">
          <button
            v-for="tab in [{ key: 'list', icon: 'M4 6h16M4 10h16M4 14h16M4 18h16' }, { key: 'calendar', icon: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z' }]"
            :key="tab.key"
            :class="['w-8 h-8 rounded-lg flex items-center justify-center transition-all duration-200',
              viewMode === tab.key
                ? 'bg-white dark:bg-slate-700 shadow-sm text-indigo-600 dark:text-indigo-400'
                : 'text-slate-400 hover:text-slate-600 dark:hover:text-slate-300']"
            @click="viewMode = tab.key as 'list' | 'calendar'"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.8" :d="tab.icon" />
            </svg>
          </button>
        </div>
        <AppButton variant="primary" @click="router.push('/leaves/new')">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
          </svg>
          Új kérelem
        </AppButton>
      </div>
    </div>

    <div v-if="loading" class="flex justify-center py-12">
      <AppSpinner size="lg" />
    </div>

    <template v-else>
      <!-- ── Calendar view ──────────────────────────────────────────────── -->
      <Transition name="fade" mode="out-in">
        <div v-if="viewMode === 'calendar'" key="calendar">
          <AppCalendar
            v-model="dummyRange"
            :events="events"
            :min-date="''"
          />
          <p class="text-xs text-center text-slate-400 mt-3">
            A naptár csak megjelenítési célú – új kérelem beadásához kattints az "Új kérelem" gombra.
          </p>
        </div>

        <!-- ── List view ───────────────────────────────────────────────── -->
        <div v-else key="list" class="space-y-3">
          <div v-if="sorted.length === 0" class="glass rounded-2xl p-12 text-center">
            <svg class="w-12 h-12 mx-auto text-slate-300 dark:text-slate-600 mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
            </svg>
            <p class="text-slate-500 dark:text-slate-400">Még nem adtál be szabadságkérelmet.</p>
            <AppButton variant="primary" size="sm" class="mt-4" @click="router.push('/leaves/new')">Első kérelem beadása</AppButton>
          </div>

          <div
            v-for="r in sorted"
            :key="r.id"
            class="glass rounded-2xl p-5 flex flex-col sm:flex-row sm:items-center gap-4"
          >
            <div class="flex-1 min-w-0 space-y-2">
              <div class="flex items-center gap-2 flex-wrap">
                <AppBadge :value="r.category" type="category" />
                <AppBadge :value="r.status"   type="status" />
                <span class="text-xs text-slate-400">{{ days(r.startDate, r.endDate) }} nap</span>
              </div>
              <p class="text-sm font-medium text-slate-700 dark:text-slate-200">
                {{ formatDate(r.startDate) }}
                <span class="text-slate-400 mx-1">→</span>
                {{ formatDate(r.endDate) }}
              </p>
              <p v-if="r.requestNote" class="text-xs text-slate-500 dark:text-slate-400 truncate">
                "{{ r.requestNote }}"
              </p>
              <p v-if="r.reviewNote" class="text-xs text-slate-500 dark:text-slate-400">
                Visszajelzés: {{ r.reviewNote }}
              </p>
            </div>

            <div class="flex gap-2 shrink-0">
              <AppButton
                v-if="r.status === LeaveStatus.Pending"
                variant="danger"
                size="sm"
                :loading="cancelling === r.id"
                @click="confirmId = r.id"
              >
                Visszavon
              </AppButton>
            </div>
          </div>
        </div>
      </Transition>
    </template>

    <!-- Confirm cancel modal -->
    <AppModal title="Visszavonás megerősítése" :open="!!confirmId" @close="confirmId = null">
      <p class="text-sm text-slate-600 dark:text-slate-300">Biztosan visszavonod ezt a kérelmet? Ez a művelet nem vonható vissza.</p>
      <template #footer>
        <AppButton variant="ghost" @click="confirmId = null">Mégse</AppButton>
        <AppButton variant="danger" @click="cancelRequest">Visszavon</AppButton>
      </template>
    </AppModal>
  </div>
</template>

<style scoped>
.fade-enter-active, .fade-leave-active { transition: all 0.2s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>
