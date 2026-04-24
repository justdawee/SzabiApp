<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { allowancesService } from '@/services/allowances.service'
import { usersService }      from '@/services/users.service'
import { useToastStore }     from '@/stores/toast.store'
import type { LeaveAllowanceDto, UserDto } from '@/types'
import { LeaveCategory, UserRole } from '@/types'
import AppButton  from '@/components/ui/AppButton.vue'
import AppModal   from '@/components/ui/AppModal.vue'
import AppSelect  from '@/components/ui/AppSelect.vue'
import AppSpinner from '@/components/ui/AppSpinner.vue'
import AppBadge   from '@/components/ui/AppBadge.vue'

const toast      = useToastStore()
const allowances = ref<LeaveAllowanceDto[]>([])
const users      = ref<UserDto[]>([])
const loading    = ref(true)
const filterYear = ref(new Date().getFullYear())

// ─── Hungarian Labour Law (Mt.) ──────────────────────────────────────────────
// Mt. 117. § — base annual leave: 20 working days
// Mt. 119. § — age-based additions (calculated per user from birthDate)
// Mt. 126. § — sick leave: 15 working days (paid at 70%)

/** Returns the total annual leave days for a user based on their age in the given year (Mt. 119. §) */
function annualLeaveDaysForAge(birthDate: string, year: number): number {
  // Age reached during the year (not exact birthday — Mt. grants the right for the whole year)
  const age = year - parseInt(birthDate.substring(0, 4))
  if (age < 25) return 20
  if (age < 28) return 21
  if (age < 31) return 22
  if (age < 33) return 23
  if (age < 35) return 24
  if (age < 37) return 25
  if (age < 39) return 26
  if (age < 41) return 27
  if (age < 43) return 28
  if (age < 45) return 29
  return 30
}

const HU_LAW_DEFAULTS = [
  {
    category:    LeaveCategory.Annual,
    label:       'Éves szabadság',
    defaultDays: 20,
    lawRef:      'Mt. 117. §',
    note:        'Alap 20 nap, életkor szerint +1–10 nap jár (Mt. 119. §)',
    color:       'indigo',
  },
  {
    category:    LeaveCategory.Sick,
    label:       'Betegszabadság',
    defaultDays: 15,
    lawRef:      'Mt. 126. §',
    note:        '15 munkanap évente, 70%-os béren',
    color:       'amber',
  },
]

// ─── Bulk generation ──────────────────────────────────────────────────────────

const showBulk     = ref(false)
const bulkYear     = ref(new Date().getFullYear())
const bulkSaving   = ref(false)
const bulkSkipExisting = ref(true)

// Editable days per category for bulk generation
const bulkDays = ref<Record<string, number>>(
  Object.fromEntries(HU_LAW_DEFAULTS.map((d) => [d.category, d.defaultDays]))
)

const activeUsers = computed(() => users.value.filter((u) => u.isActive && u.role === UserRole.Employee))

const existingKeysForBulkYear = computed(() =>
  new Set(
    allowances.value
      .filter((a) => a.year === bulkYear.value)
      .map((a) => `${a.userId}:${a.category}`)
  )
)

const bulkPreview = computed(() => {
  let count = 0
  for (const u of activeUsers.value) {
    for (const d of HU_LAW_DEFAULTS) {
      const key = `${u.id}:${d.category}`
      if (!bulkSkipExisting.value || !existingKeysForBulkYear.value.has(key)) count++
    }
  }
  return count
})

function daysForUserCategory(u: UserDto, category: LeaveCategory): number {
  if (category === LeaveCategory.Annual && u.birthDate) {
    return annualLeaveDaysForAge(u.birthDate, bulkYear.value)
  }
  return bulkDays.value[category]
}

async function runBulkGenerate() {
  bulkSaving.value = true
  let ok = 0; let skip = 0; let fail = 0

  for (const u of activeUsers.value) {
    for (const d of HU_LAW_DEFAULTS) {
      const key = `${u.id}:${d.category}`
      if (bulkSkipExisting.value && existingKeysForBulkYear.value.has(key)) { skip++; continue }
      try {
        const created = await allowancesService.create({
          userId:    u.id,
          year:      bulkYear.value,
          category:  d.category,
          totalDays: daysForUserCategory(u, d.category),
        })
        allowances.value.push(created)
        ok++
      } catch { fail++ }
    }
  }

  bulkSaving.value = false
  showBulk.value   = false

  if (ok)   toast.success(`${ok} keret sikeresen generálva.`)
  if (skip) toast.success(`${skip} keret már létezett, kihagyva.`)
  if (fail) toast.error(`${fail} keret generálása sikertelen.`)
}

// ─── Single add ───────────────────────────────────────────────────────────────

const showAdd  = ref(false)
const saving   = ref(false)
const form     = ref({ userId: '', year: new Date().getFullYear(), category: LeaveCategory.Annual as LeaveCategory, totalDays: 20 })

const userOptions = computed(() => users.value.map((u) => ({
  value: u.id,
  label: `${u.lastName} ${u.firstName}`,
})))

const categoryOptions = [
  { value: LeaveCategory.Annual,    label: 'Éves szabadság' },
  { value: LeaveCategory.Sick,      label: 'Betegszabadság' },
  { value: LeaveCategory.Unpaid,    label: 'Fizetés nélküli' },
  { value: LeaveCategory.Paternity, label: 'Apasági' },
  { value: LeaveCategory.Maternity, label: 'Anyasági' },
  { value: LeaveCategory.Other,     label: 'Egyéb' },
]

async function addAllowance() {
  if (!form.value.userId) return
  saving.value = true
  try {
    const created = await allowancesService.create(form.value)
    allowances.value.push(created)
    toast.success('Keret hozzáadva.')
    showAdd.value = false
    form.value = { userId: '', year: new Date().getFullYear(), category: LeaveCategory.Annual, totalDays: 20 }
  } catch {
    toast.error('Nem sikerült menteni.')
  } finally {
    saving.value = false
  }
}

// ─── Inline edit ──────────────────────────────────────────────────────────────

const editingId   = ref<string | null>(null)
const editingDays = ref(0)

function startEdit(a: LeaveAllowanceDto) {
  editingId.value   = a.id
  editingDays.value = a.totalDays
}

async function saveEdit(a: LeaveAllowanceDto) {
  if (editingDays.value === a.totalDays) { editingId.value = null; return }
  try {
    const updated = await allowancesService.update(a.id, { totalDays: editingDays.value })
    const idx = allowances.value.findIndex((x) => x.id === a.id)
    if (idx !== -1) allowances.value[idx] = updated
    toast.success('Keret frissítve.')
  } catch {
    toast.error('Nem sikerült frissíteni.')
  } finally {
    editingId.value = null
  }
}

// ─── Delete ───────────────────────────────────────────────────────────────────

const deleteTarget = ref<LeaveAllowanceDto | null>(null)
const deleting     = ref(false)

async function confirmDelete() {
  if (!deleteTarget.value) return
  deleting.value = true
  try {
    await allowancesService.delete(deleteTarget.value.id)
    allowances.value   = allowances.value.filter((a) => a.id !== deleteTarget.value!.id)
    toast.success('Keret törölve.')
    deleteTarget.value = null
  } catch {
    toast.error('Nem sikerült törölni.')
  } finally {
    deleting.value = false
  }
}

// ─── View state ───────────────────────────────────────────────────────────────

const filtered = computed(() =>
  allowances.value
    .filter((a) => a.year === filterYear.value)
    .sort((a, b) => a.userFullName.localeCompare(b.userFullName, 'hu'))
)

const yearOptions = computed(() => {
  const years = new Set(allowances.value.map((a) => a.year))
  years.add(new Date().getFullYear())
  return [...years].sort((a, b) => b - a).map((y) => ({ value: y, label: String(y) }))
})

const bulkYears = computed(() => {
  const y = new Date().getFullYear()
  return [y - 1, y, y + 1]
})

onMounted(async () => {
  try {
    const [a, u] = await Promise.all([allowancesService.getAll(), usersService.getAll()])
    allowances.value = a
    users.value      = u
  } catch {
    toast.error('Nem sikerült betölteni az adatokat.')
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="space-y-6 max-w-5xl mx-auto">

    <!-- ─── Header ──────────────────────────────────────────────────────────── -->
    <div class="flex items-center justify-between flex-wrap gap-3">
      <div>
        <h2 class="text-xl font-bold text-slate-800 dark:text-white">Szabadság keretek</h2>
        <p class="text-sm text-slate-500 mt-0.5">{{ filtered.length }} keret a {{ filterYear }}. évre</p>
      </div>
      <div class="flex gap-2">
        <AppButton variant="secondary" @click="showBulk = true">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M13 10V3L4 14h7v7l9-11h-7z"/>
          </svg>
          Tömeges generálás
        </AppButton>
        <AppButton variant="primary" @click="showAdd = true">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
          </svg>
          Egyéni hozzáadás
        </AppButton>
      </div>
    </div>

    <!-- ─── Law info banner ─────────────────────────────────────────────────── -->
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
      <div
        v-for="d in HU_LAW_DEFAULTS" :key="d.category"
        class="glass-sm rounded-xl px-4 py-3 flex items-start gap-3"
      >
        <div :class="['w-8 h-8 rounded-lg flex items-center justify-center shrink-0 mt-0.5',
          d.color === 'indigo' ? 'bg-indigo-100 dark:bg-indigo-500/15' : 'bg-amber-100 dark:bg-amber-500/15']">
          <svg :class="['w-4 h-4', d.color === 'indigo' ? 'text-indigo-600 dark:text-indigo-400' : 'text-amber-600 dark:text-amber-400']"
            fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
          </svg>
        </div>
        <div>
          <div class="flex items-center gap-2">
            <p class="text-sm font-semibold text-slate-800 dark:text-slate-100">{{ d.label }}</p>
            <span class="text-[10px] font-mono text-slate-400 dark:text-slate-500 bg-slate-100 dark:bg-slate-800 px-1.5 py-0.5 rounded">
              {{ d.lawRef }}
            </span>
          </div>
          <p class="text-xs text-slate-500 dark:text-slate-400 mt-0.5">{{ d.note }}</p>
        </div>
        <span class="ml-auto text-xl font-bold text-slate-700 dark:text-slate-200 shrink-0">{{ d.defaultDays }}</span>
      </div>
    </div>

    <!-- ─── Year filter + table ─────────────────────────────────────────────── -->
    <div v-if="loading" class="flex justify-center py-12"><AppSpinner size="lg" /></div>

    <template v-else>
      <!-- Year selector -->
      <div class="flex items-center gap-3">
        <span class="text-sm text-slate-500">Év:</span>
        <div class="flex gap-1.5">
          <button
            v-for="y in yearOptions" :key="y.value"
            :class="[
              'px-3 py-1.5 rounded-lg text-sm font-medium transition-all',
              filterYear === y.value
                ? 'btn-gradient text-white shadow-sm'
                : 'glass-sm text-slate-600 dark:text-slate-400 hover:text-slate-800 dark:hover:text-slate-200',
            ]"
            @click="filterYear = y.value"
          >
            {{ y.label }}
          </button>
        </div>
      </div>

      <div v-if="filtered.length === 0" class="glass rounded-2xl p-12 text-center">
        <p class="text-sm text-slate-500 dark:text-slate-400">
          Nincs keret a {{ filterYear }}. évre.
        </p>
        <p class="text-xs text-slate-400 mt-1">
          Használd a „Tömeges generálás" gombot az összes alkalmazott keretének létrehozásához.
        </p>
      </div>

      <div v-else class="glass rounded-2xl overflow-hidden">
        <div class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead>
              <tr class="border-b border-black/5 dark:border-white/5">
                <th class="text-left px-5 py-3.5 font-semibold text-slate-600 dark:text-slate-300">Felhasználó</th>
                <th class="text-left px-5 py-3.5 font-semibold text-slate-600 dark:text-slate-300">Típus</th>
                <th class="text-left px-5 py-3.5 font-semibold text-slate-600 dark:text-slate-300">Összesen</th>
                <th class="text-left px-5 py-3.5 font-semibold text-slate-600 dark:text-slate-300 hidden md:table-cell">Felhasznált</th>
                <th class="text-left px-5 py-3.5 font-semibold text-slate-600 dark:text-slate-300">Maradt</th>
                <th class="px-5 py-3.5 w-16" />
              </tr>
            </thead>
            <tbody class="divide-y divide-black/5 dark:divide-white/5">
              <tr v-for="a in filtered" :key="a.id" class="hover:bg-black/2 dark:hover:bg-white/2 transition-colors">
                <td class="px-5 py-3.5 font-medium text-slate-800 dark:text-slate-100">{{ a.userFullName }}</td>
                <td class="px-5 py-3.5"><AppBadge :value="a.category" type="category" /></td>

                <!-- Inline editable totalDays -->
                <td class="px-5 py-3.5">
                  <div v-if="editingId === a.id" class="flex items-center gap-1.5">
                    <input
                      v-model.number="editingDays"
                      type="number"
                      min="1" max="365"
                      class="w-16 neuro rounded-lg px-2 py-1 text-sm text-slate-800 dark:text-slate-100 outline-none focus:ring-2 focus:ring-indigo-500/40"
                      @keyup.enter="saveEdit(a)"
                      @keyup.escape="editingId = null"
                    />
                    <button class="text-emerald-500 hover:text-emerald-600 transition-colors" @click="saveEdit(a)">
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>
                      </svg>
                    </button>
                    <button class="text-slate-400 hover:text-slate-600 transition-colors" @click="editingId = null">
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
                      </svg>
                    </button>
                  </div>
                  <button
                    v-else
                    class="flex items-center gap-1 group text-slate-600 dark:text-slate-300 hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors"
                    :title="'Kattints a szerkesztéshez'"
                    @click="startEdit(a)"
                  >
                    {{ a.totalDays }} nap
                    <svg class="w-3 h-3 opacity-0 group-hover:opacity-100 transition-opacity" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                        d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z"/>
                    </svg>
                  </button>
                </td>

                <td class="px-5 py-3.5 text-slate-600 dark:text-slate-300 hidden md:table-cell">{{ a.usedDays }} nap</td>
                <td class="px-5 py-3.5">
                  <span :class="['font-medium', a.remainingDays <= 3 ? 'text-rose-500' : a.remainingDays <= 7 ? 'text-amber-500' : 'text-emerald-500']">
                    {{ a.remainingDays }} nap
                  </span>
                </td>
                <td class="px-5 py-3.5">
                  <AppButton variant="ghost" size="sm" @click="deleteTarget = a">
                    <svg class="w-4 h-4 text-rose-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                        d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
                    </svg>
                  </AppButton>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </template>

    <!-- ─── Bulk generation modal ────────────────────────────────────────────── -->
    <AppModal title="Tömeges keret generálás" :open="showBulk" @close="showBulk = false">
      <div class="space-y-5">

        <!-- Info -->
        <div class="flex gap-3 rounded-xl bg-indigo-50 dark:bg-indigo-500/10 border border-indigo-100 dark:border-indigo-500/20 px-4 py-3">
          <svg class="w-5 h-5 text-indigo-500 shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
          </svg>
          <p class="text-xs text-indigo-700 dark:text-indigo-300 leading-relaxed">
            Minden <strong>aktív alkalmazottnak</strong> egyszerre hoz létre éves és betegszabadsági keretet.
            Az éves szabadság <strong>egyéni életkor alapján</strong> kerül kiszámításra (Mt. 119. §).
            A betegszabadság egységesen 15 nap (Mt. 126. §).
          </p>
        </div>

        <!-- Year -->
        <div>
          <p class="text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">Év</p>
          <div class="flex gap-2">
            <button
              v-for="y in bulkYears" :key="y"
              :class="[
                'flex-1 py-2 rounded-xl text-sm font-medium border-2 transition-all',
                bulkYear === y
                  ? 'border-indigo-400 bg-indigo-50 dark:bg-indigo-500/10 text-indigo-700 dark:text-indigo-300'
                  : 'border-transparent glass-sm text-slate-600 dark:text-slate-400 hover:border-slate-200 dark:hover:border-slate-600',
              ]"
              @click="bulkYear = y"
            >
              {{ y }}
            </button>
          </div>
        </div>

        <!-- Days config per category -->
        <div>
          <p class="text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">Napok száma típusonként</p>
          <div class="space-y-2">
            <div
              v-for="d in HU_LAW_DEFAULTS" :key="d.category"
              class="glass-sm rounded-xl px-4 py-3 flex items-center gap-3"
            >
              <div class="flex-1">
                <p class="text-sm font-medium text-slate-800 dark:text-slate-100">{{ d.label }}</p>
                <p class="text-xs text-slate-400">
                  {{ d.lawRef }}
                  <template v-if="d.category === 'Annual'">
                    — életkor alapján: 20–30 nap (Mt. 119. §)
                  </template>
                  <template v-else>
                    — alapérték: {{ d.defaultDays }} nap
                  </template>
                </p>
              </div>
              <!-- Annual is auto-calculated per user, only sick is configurable -->
              <template v-if="d.category === 'Annual'">
                <span class="text-xs text-indigo-600 dark:text-indigo-400 font-medium bg-indigo-50 dark:bg-indigo-500/10 px-2.5 py-1 rounded-lg">
                  Egyéni
                </span>
              </template>
              <template v-else>
                <div class="flex items-center gap-2">
                  <button
                    class="w-7 h-7 rounded-lg glass-sm flex items-center justify-center text-slate-500 hover:text-indigo-600 transition-colors"
                    @click="bulkDays[d.category] = Math.max(1, bulkDays[d.category] - 1)"
                  >
                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 12H4"/>
                    </svg>
                  </button>
                  <input
                    v-model.number="bulkDays[d.category]"
                    type="number" min="1" max="365"
                    class="w-14 neuro rounded-lg px-2 py-1 text-sm text-center text-slate-800 dark:text-slate-100 outline-none focus:ring-2 focus:ring-indigo-500/40"
                  />
                  <button
                    class="w-7 h-7 rounded-lg glass-sm flex items-center justify-center text-slate-500 hover:text-indigo-600 transition-colors"
                    @click="bulkDays[d.category] = Math.min(365, bulkDays[d.category] + 1)"
                  >
                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
                    </svg>
                  </button>
                </div>
              </template>
            </div>
          </div>
        </div>

        <!-- Skip existing toggle -->
        <label class="flex items-center gap-3 cursor-pointer select-none glass-sm rounded-xl px-4 py-3">
          <div class="relative">
            <input v-model="bulkSkipExisting" type="checkbox" class="sr-only" />
            <div :class="['w-9 h-5 rounded-full transition-colors', bulkSkipExisting ? 'bg-indigo-500' : 'bg-slate-300 dark:bg-slate-600']" />
            <div :class="['absolute top-0.5 left-0.5 w-4 h-4 bg-white rounded-full shadow transition-transform', bulkSkipExisting ? 'translate-x-4' : '']" />
          </div>
          <div>
            <p class="text-sm font-medium text-slate-700 dark:text-slate-300">Meglévőket kihagyja</p>
            <p class="text-xs text-slate-400">Ha ki van kapcsolva, felülírja a már meglévő kereteket</p>
          </div>
        </label>

        <!-- Preview -->
        <div class="glass-sm rounded-xl px-4 py-3 flex items-center justify-between">
          <div>
            <p class="text-sm text-slate-600 dark:text-slate-300">
              <strong class="text-slate-800 dark:text-slate-100">{{ activeUsers.length }}</strong> aktív alkalmazott ×
              <strong class="text-slate-800 dark:text-slate-100">{{ HU_LAW_DEFAULTS.length }}</strong> kategória
            </p>
            <p class="text-xs text-slate-400 mt-0.5">Létrehozandó keretek: {{ bulkPreview }} db</p>
          </div>
          <span class="text-3xl font-bold text-transparent bg-clip-text" style="background-image:linear-gradient(135deg,#6366f1,#a855f7)">
            {{ bulkPreview }}
          </span>
        </div>
      </div>

      <template #footer>
        <AppButton variant="ghost" @click="showBulk = false">Mégse</AppButton>
        <AppButton
          variant="primary"
          :loading="bulkSaving"
          :disabled="bulkPreview === 0"
          @click="runBulkGenerate"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z"/>
          </svg>
          Generálás ({{ bulkPreview }})
        </AppButton>
      </template>
    </AppModal>

    <!-- ─── Single add modal ─────────────────────────────────────────────────── -->
    <AppModal title="Egyéni keret hozzáadása" :open="showAdd" @close="showAdd = false">
      <div class="space-y-4">
        <AppSelect v-model="form.userId" label="Felhasználó" :options="userOptions" required />
        <AppSelect v-model="form.category" label="Típus" :options="categoryOptions" required />
        <div class="grid grid-cols-2 gap-3">
          <div class="flex flex-col gap-1.5">
            <label class="text-sm font-medium text-slate-700 dark:text-slate-300">Év</label>
            <input
              v-model.number="form.year"
              type="number" min="2020" max="2099"
              class="neuro rounded-xl px-4 py-2.5 text-sm text-slate-800 dark:text-slate-100 outline-none focus:ring-2 focus:ring-indigo-500/40"
            />
          </div>
          <div class="flex flex-col gap-1.5">
            <label class="text-sm font-medium text-slate-700 dark:text-slate-300">Napok száma</label>
            <input
              v-model.number="form.totalDays"
              type="number" min="1" max="365"
              class="neuro rounded-xl px-4 py-2.5 text-sm text-slate-800 dark:text-slate-100 outline-none focus:ring-2 focus:ring-indigo-500/40"
            />
          </div>
        </div>
      </div>
      <template #footer>
        <AppButton variant="ghost" @click="showAdd = false">Mégse</AppButton>
        <AppButton variant="primary" :loading="saving" @click="addAllowance">Hozzáadás</AppButton>
      </template>
    </AppModal>

    <!-- ─── Delete confirm ───────────────────────────────────────────────────── -->
    <AppModal title="Keret törlése" :open="!!deleteTarget" @close="deleteTarget = null">
      <p class="text-sm text-slate-600 dark:text-slate-300">
        Biztosan törlöd <strong>{{ deleteTarget?.userFullName }}</strong> –
        <strong>{{ deleteTarget?.category }}</strong> keretét?
      </p>
      <template #footer>
        <AppButton variant="ghost" @click="deleteTarget = null">Mégse</AppButton>
        <AppButton variant="danger" :loading="deleting" @click="confirmDelete">Törlés</AppButton>
      </template>
    </AppModal>
  </div>
</template>
