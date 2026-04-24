<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { holidaysService }                      from '@/services/holidays.service'
import { fetchHungarianHolidays }               from '@/services/openholidays.service'
import type { ImportableHoliday }               from '@/services/openholidays.service'
import { useToastStore }                        from '@/stores/toast.store'
import type { HolidayDto }                      from '@/types'
import AppButton  from '@/components/ui/AppButton.vue'
import AppModal   from '@/components/ui/AppModal.vue'
import AppInput   from '@/components/ui/AppInput.vue'
import AppSpinner from '@/components/ui/AppSpinner.vue'

const toast    = useToastStore()
const holidays = ref<HolidayDto[]>([])
const loading  = ref(true)

// ─── Manual add ───────────────────────────────────────────────────────────────

const showAdd = ref(false)
const saving  = ref(false)
const form    = ref({ name: '', date: '', isRecurringYearly: false })

const deleteTarget = ref<HolidayDto | null>(null)
const deleting     = ref(false)

// ─── API import ───────────────────────────────────────────────────────────────

const showImport   = ref(false)
const importYear   = ref(new Date().getFullYear())
const importLoading = ref(false)
const importItems  = ref<(ImportableHoliday & { selected: boolean })[]>([])
const importSaving = ref(false)

const existingDates = computed(() => new Set(holidays.value.map((h) => h.date.split('T')[0])))

const importYears = computed(() => {
  const y = new Date().getFullYear()
  return [y - 1, y, y + 1]
})

const allSelected  = computed(() => importItems.value.length > 0 && importItems.value.every((i) => i.selected))
const someSelected = computed(() => importItems.value.some((i) => i.selected))
const dupItems     = computed(() => importItems.value.filter((i) =>  existingDates.value.has(i.date)))

async function loadApiHolidays() {
  importLoading.value = true
  importItems.value   = []
  try {
    const fetched = await fetchHungarianHolidays(importYear.value)
    importItems.value = fetched.map((h) => ({
      ...h,
      // Pre-select only items not already in DB
      selected: !existingDates.value.has(h.date),
    }))
  } catch {
    toast.error('Nem sikerült elérni az OpenHolidays API-t.')
  } finally {
    importLoading.value = false
  }
}

function toggleAll() {
  const next = !allSelected.value
  importItems.value.forEach((i) => { i.selected = next })
}

async function runImport() {
  const toImport = importItems.value.filter((i) => i.selected && !existingDates.value.has(i.date))
  if (!toImport.length) return

  importSaving.value = true
  let ok = 0
  let fail = 0

  for (const item of toImport) {
    try {
      const created = await holidaysService.create({
        name:              item.name,
        date:              item.date,
        isRecurringYearly: false,
      })
      holidays.value.push(created)
      ok++
    } catch {
      fail++
    }
  }

  holidays.value.sort((a, b) => a.date.localeCompare(b.date))
  importSaving.value = false
  showImport.value   = false
  importItems.value  = []

  if (ok)   toast.success(`${ok} ünnepnap sikeresen importálva.`)
  if (fail) toast.error(`${fail} ünnepnap importálása sikertelen.`)
}

function openImportModal() {
  showImport.value   = true
  importItems.value  = []
  importYear.value   = new Date().getFullYear()
}

// ─── Manual add ───────────────────────────────────────────────────────────────

async function addHoliday() {
  if (!form.value.name || !form.value.date) return
  saving.value = true
  try {
    const created = await holidaysService.create(form.value)
    holidays.value.push(created)
    holidays.value.sort((a, b) => a.date.localeCompare(b.date))
    toast.success('Ünnepnap hozzáadva.')
    showAdd.value = false
    form.value    = { name: '', date: '', isRecurringYearly: false }
  } catch {
    toast.error('Nem sikerült menteni.')
  } finally {
    saving.value = false
  }
}

// ─── Delete ───────────────────────────────────────────────────────────────────

async function confirmDelete() {
  if (!deleteTarget.value) return
  deleting.value = true
  try {
    await holidaysService.delete(deleteTarget.value.id)
    holidays.value  = holidays.value.filter((h) => h.id !== deleteTarget.value!.id)
    toast.success('Ünnepnap törölve.')
    deleteTarget.value = null
  } catch {
    toast.error('Nem sikerült törölni.')
  } finally {
    deleting.value = false
  }
}

function formatDate(d: string) {
  return new Date(d).toLocaleDateString('hu-HU', { year: 'numeric', month: 'long', day: 'numeric' })
}

// ─── Init ─────────────────────────────────────────────────────────────────────

onMounted(async () => {
  try {
    holidays.value = await holidaysService.getAll()
  } catch {
    toast.error('Nem sikerült betölteni az ünnepnapokat.')
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="space-y-6 max-w-3xl mx-auto">
    <!-- Header -->
    <div class="flex items-center justify-between gap-3 flex-wrap">
      <div>
        <h2 class="text-xl font-bold text-slate-800 dark:text-white">Ünnepnapok</h2>
        <p class="text-sm text-slate-500 mt-0.5">{{ holidays.length }} ünnepnap rögzítve</p>
      </div>
      <div class="flex gap-2">
        <AppButton variant="secondary" @click="openImportModal">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12"/>
          </svg>
          Importálás API-ból
        </AppButton>
        <AppButton variant="primary" @click="showAdd = true">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
          </svg>
          Egyéni hozzáadás
        </AppButton>
      </div>
    </div>

    <div v-if="loading" class="flex justify-center py-12"><AppSpinner size="lg" /></div>

    <template v-else>
      <div v-if="holidays.length === 0" class="glass rounded-2xl p-12 text-center">
        <div class="w-12 h-12 rounded-2xl bg-indigo-50 dark:bg-indigo-500/10 flex items-center justify-center mx-auto mb-3">
          <svg class="w-6 h-6 text-indigo-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
              d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z"/>
          </svg>
        </div>
        <p class="text-sm text-slate-500 dark:text-slate-400">Nincsenek rögzített ünnepnapok.</p>
        <p class="text-xs text-slate-400 dark:text-slate-500 mt-1">Importáld az API-ból vagy add hozzá manuálisan.</p>
      </div>

      <div v-else class="glass rounded-2xl overflow-hidden">
        <div class="divide-y divide-black/5 dark:divide-white/5">
          <div
            v-for="h in holidays" :key="h.id"
            class="flex items-center gap-4 px-5 py-4 hover:bg-black/2 dark:hover:bg-white/2 transition-colors"
          >
            <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-indigo-500/15 to-purple-500/15 flex items-center justify-center shrink-0">
              <svg class="w-5 h-5 text-indigo-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z"/>
              </svg>
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium text-slate-800 dark:text-slate-100">{{ h.name }}</p>
              <p class="text-xs text-slate-500">{{ formatDate(h.date) }}</p>
            </div>
            <div class="flex items-center gap-3">
              <span v-if="h.isRecurringYearly" class="text-xs bg-teal-100 text-teal-700 dark:bg-teal-500/15 dark:text-teal-300 px-2 py-0.5 rounded-full">
                Éves
              </span>
              <AppButton variant="ghost" size="sm" @click="deleteTarget = h">
                <svg class="w-4 h-4 text-rose-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                    d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
                </svg>
              </AppButton>
            </div>
          </div>
        </div>
      </div>
    </template>

    <!-- ─── Import from API modal ────────────────────────────────────────────── -->
    <AppModal title="Importálás OpenHolidays API-ból" :open="showImport" @close="showImport = false">
      <div class="space-y-4">

        <!-- Info banner -->
        <div class="flex gap-3 rounded-xl bg-indigo-50 dark:bg-indigo-500/10 border border-indigo-100 dark:border-indigo-500/20 px-4 py-3">
          <svg class="w-5 h-5 text-indigo-500 shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
          </svg>
          <p class="text-xs text-indigo-700 dark:text-indigo-300 leading-relaxed">
            Magyar állami ünnepnapok automatikus betöltése az <strong>openholidaysapi.org</strong> adatbázisából.
            Egyéni dátumokat a „Egyéni hozzáadás" gombbal tudsz felvenni.
          </p>
        </div>

        <!-- Year selector -->
        <div>
          <p class="text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">Év kiválasztása</p>
          <div class="flex gap-2">
            <button
              v-for="y in importYears" :key="y"
              :class="[
                'flex-1 py-2 rounded-xl text-sm font-medium border-2 transition-all',
                importYear === y
                  ? 'border-indigo-400 bg-indigo-50 dark:bg-indigo-500/10 text-indigo-700 dark:text-indigo-300'
                  : 'border-transparent glass-sm text-slate-600 dark:text-slate-400 hover:border-slate-200 dark:hover:border-slate-600',
              ]"
              @click="importYear = y; importItems = []"
            >
              {{ y }}
            </button>
          </div>
        </div>

        <!-- Load button -->
        <AppButton
          variant="secondary"
          class="w-full"
          :loading="importLoading"
          @click="loadApiHolidays"
        >
          <svg v-if="!importLoading" class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
          </svg>
          {{ importItems.length ? 'Újratöltés' : 'Betöltés az API-ból' }}
        </AppButton>

        <!-- Results -->
        <template v-if="importItems.length > 0">
          <!-- Select all -->
          <div class="flex items-center justify-between">
            <label class="flex items-center gap-2 cursor-pointer select-none">
              <input
                type="checkbox"
                :checked="allSelected"
                :indeterminate="someSelected && !allSelected"
                class="w-4 h-4 rounded text-indigo-500"
                @change="toggleAll"
              />
              <span class="text-sm font-medium text-slate-700 dark:text-slate-300">Összes kijelölése</span>
            </label>
            <span class="text-xs text-slate-400">
              {{ importItems.filter(i => i.selected).length }} / {{ importItems.length }} kijelölve
            </span>
          </div>

          <!-- Holiday list -->
          <div class="max-h-64 overflow-y-auto rounded-xl border border-black/5 dark:border-white/5 divide-y divide-black/5 dark:divide-white/5">
            <label
              v-for="(item, idx) in importItems"
              :key="idx"
              :class="[
                'flex items-center gap-3 px-4 py-3 cursor-pointer transition-colors select-none',
                existingDates.has(item.date)
                  ? 'opacity-50 cursor-not-allowed bg-slate-50 dark:bg-slate-800/40'
                  : 'hover:bg-indigo-50/50 dark:hover:bg-indigo-500/5',
              ]"
            >
              <input
                v-model="item.selected"
                type="checkbox"
                :disabled="existingDates.has(item.date)"
                class="w-4 h-4 rounded text-indigo-500 shrink-0"
              />
              <div class="flex-1 min-w-0">
                <p class="text-sm text-slate-800 dark:text-slate-100 truncate">{{ item.name }}</p>
                <p class="text-xs text-slate-400">{{ formatDate(item.date) }}</p>
              </div>
              <span
                v-if="existingDates.has(item.date)"
                class="text-xs bg-slate-100 text-slate-500 dark:bg-slate-700 dark:text-slate-400 px-2 py-0.5 rounded-full shrink-0"
              >
                Már létezik
              </span>
            </label>
          </div>

          <!-- Duplicate notice -->
          <p v-if="dupItems.length > 0" class="text-xs text-slate-400 text-center">
            {{ dupItems.length }} ünnepnap már rögzítve van, ezek nem importálhatók újra.
          </p>
        </template>
      </div>

      <template #footer>
        <AppButton variant="ghost" @click="showImport = false">Mégse</AppButton>
        <AppButton
          variant="primary"
          :loading="importSaving"
          :disabled="!importItems.filter(i => i.selected && !existingDates.has(i.date)).length"
          @click="runImport"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12"/>
          </svg>
          Importálás ({{ importItems.filter(i => i.selected && !existingDates.has(i.date)).length }})
        </AppButton>
      </template>
    </AppModal>

    <!-- ─── Manual add modal ─────────────────────────────────────────────────── -->
    <AppModal title="Egyéni ünnepnap hozzáadása" :open="showAdd" @close="showAdd = false">
      <div class="space-y-4">
        <AppInput v-model="form.name" label="Megnevezés" placeholder="pl. Céges rendezvény" required />
        <AppInput v-model="form.date" label="Dátum" type="date" required />
        <label class="flex items-center gap-2 cursor-pointer select-none">
          <input v-model="form.isRecurringYearly" type="checkbox" class="w-4 h-4 rounded text-indigo-500" />
          <span class="text-sm text-slate-700 dark:text-slate-300">Évente ismétlődő</span>
        </label>
      </div>
      <template #footer>
        <AppButton variant="ghost" @click="showAdd = false">Mégse</AppButton>
        <AppButton variant="primary" :loading="saving" @click="addHoliday">Hozzáadás</AppButton>
      </template>
    </AppModal>

    <!-- ─── Delete confirm ───────────────────────────────────────────────────── -->
    <AppModal title="Ünnepnap törlése" :open="!!deleteTarget" @close="deleteTarget = null">
      <p class="text-sm text-slate-600 dark:text-slate-300">
        Biztosan törlöd: <strong>{{ deleteTarget?.name }}</strong>?
      </p>
      <template #footer>
        <AppButton variant="ghost" @click="deleteTarget = null">Mégse</AppButton>
        <AppButton variant="danger" :loading="deleting" @click="confirmDelete">Törlés</AppButton>
      </template>
    </AppModal>
  </div>
</template>
