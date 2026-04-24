<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import { useToastStore } from '@/stores/toast.store'
import { allowancesService } from '@/services/allowances.service'
import { leavesService }     from '@/services/leaves.service'
import type { LeaveAllowanceDto, LeaveRequestDto } from '@/types'
import { LeaveStatus, LeaveCategory } from '@/types'
import AppCard    from '@/components/ui/AppCard.vue'
import AppBadge   from '@/components/ui/AppBadge.vue'
import AppSpinner from '@/components/ui/AppSpinner.vue'
import AppButton  from '@/components/ui/AppButton.vue'

const auth   = useAuthStore()
const toast  = useToastStore()
const router = useRouter()

const allowances = ref<LeaveAllowanceDto[]>([])
const requests   = ref<LeaveRequestDto[]>([])
const loading    = ref(true)

onMounted(async () => {
  try {
    const [a, r] = await Promise.all([
      allowancesService.getMy(),
      leavesService.getMy(),
    ])
    allowances.value = a
    requests.value   = r
  } catch {
    toast.error('Nem sikerült betölteni az adatokat.')
  } finally {
    loading.value = false
  }
})

const annual = computed(() => allowances.value.find((a) => a.category === LeaveCategory.Annual))
const pending = computed(() => requests.value.filter((r) => r.status === LeaveStatus.Pending).length)
const recent  = computed(() => [...requests.value].sort((a, b) => b.createdAt.localeCompare(a.createdAt)).slice(0, 5))

function greet() {
  const h = new Date().getHours()
  if (h < 12) return 'Jó reggelt'
  if (h < 18) return 'Jó napot'
  return 'Jó estét'
}

function formatDate(d: string) {
  return new Date(d).toLocaleDateString('hu-HU', { month: 'short', day: 'numeric' })
}

function daysBetween(start: string, end: string) {
  const diff = new Date(end).getTime() - new Date(start).getTime()
  return Math.round(diff / 86_400_000) + 1
}
</script>

<template>
  <div class="space-y-6 max-w-5xl mx-auto">
    <!-- Welcome banner -->
    <div class="glass rounded-2xl p-6 md:p-8 relative overflow-hidden">
      <div class="pointer-events-none absolute -right-12 -top-12 w-48 h-48 rounded-full bg-gradient-to-br from-indigo-400/20 to-purple-400/20 blur-2xl" />
      <div class="pointer-events-none absolute -right-4 -bottom-8 w-32 h-32 rounded-full bg-pink-400/10 blur-2xl" />
      <div class="relative">
        <p class="text-sm text-indigo-500 dark:text-indigo-400 font-medium">{{ greet() }},</p>
        <h2 class="text-2xl font-bold text-slate-800 dark:text-white mt-0.5">{{ auth.user?.fullName }} 👋</h2>
        <p class="text-sm text-slate-500 dark:text-slate-400 mt-1">{{ new Date().toLocaleDateString('hu-HU', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }) }}</p>
      </div>
    </div>

    <div v-if="loading" class="flex justify-center py-12">
      <AppSpinner size="lg" />
    </div>

    <template v-else>
      <!-- Stats grid -->
      <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <!-- Annual allowance -->
        <div class="glass rounded-2xl p-5 col-span-2 lg:col-span-1">
          <p class="text-xs text-slate-500 dark:text-slate-400 font-medium uppercase tracking-wide">Éves keret</p>
          <div class="mt-3 flex items-end gap-2">
            <span class="text-4xl font-bold text-slate-800 dark:text-white">{{ annual?.totalDays ?? '–' }}</span>
            <span class="text-slate-400 pb-1">nap</span>
          </div>
          <div v-if="annual" class="mt-3">
            <div class="flex justify-between text-xs text-slate-500 mb-1">
              <span>Felhasznált: {{ annual.usedDays }}</span>
              <span>Maradt: {{ annual.remainingDays }}</span>
            </div>
            <div class="h-2 rounded-full bg-slate-200 dark:bg-slate-700 overflow-hidden">
              <div
                class="h-full rounded-full bg-gradient-to-r from-indigo-500 to-purple-500 transition-all duration-700"
                :style="{ width: `${(annual.usedDays / annual.totalDays) * 100}%` }"
              />
            </div>
          </div>
        </div>

        <!-- Remaining -->
        <div class="glass rounded-2xl p-5">
          <p class="text-xs text-slate-500 dark:text-slate-400 font-medium uppercase tracking-wide">Maradék</p>
          <p class="mt-3 text-4xl font-bold text-emerald-500">{{ annual?.remainingDays ?? '–' }}</p>
          <p class="text-xs text-slate-400 mt-1">nap elérhető</p>
        </div>

        <!-- Pending -->
        <div class="glass rounded-2xl p-5">
          <p class="text-xs text-slate-500 dark:text-slate-400 font-medium uppercase tracking-wide">Függőben</p>
          <p class="mt-3 text-4xl font-bold text-amber-500">{{ pending }}</p>
          <p class="text-xs text-slate-400 mt-1">kérelem</p>
        </div>

        <!-- Quick action -->
        <div class="glass rounded-2xl p-5 flex flex-col justify-between">
          <p class="text-xs text-slate-500 dark:text-slate-400 font-medium uppercase tracking-wide">Gyors műveletek</p>
          <AppButton variant="primary" size="sm" class="mt-3 w-full" @click="router.push('/leaves/new')">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
            </svg>
            Új kérelem
          </AppButton>
        </div>
      </div>

      <!-- All allowances -->
      <AppCard title="Szabadság keretek" :subtitle="`${new Date().getFullYear()}. év`" v-if="allowances.length > 1">
        <div class="space-y-3">
          <div v-for="a in allowances" :key="a.id" class="flex items-center gap-4 p-3 rounded-xl glass-sm">
            <AppBadge :value="a.category" type="category" />
            <div class="flex-1 min-w-0">
              <div class="flex justify-between text-sm mb-1.5">
                <span class="text-slate-600 dark:text-slate-400">{{ a.usedDays }} / {{ a.totalDays }} nap</span>
                <span class="font-medium text-slate-800 dark:text-slate-200">{{ a.remainingDays }} maradt</span>
              </div>
              <div class="h-1.5 rounded-full bg-slate-200 dark:bg-slate-700 overflow-hidden">
                <div
                  class="h-full rounded-full bg-gradient-to-r from-indigo-500 to-purple-500"
                  :style="{ width: `${Math.min((a.usedDays / a.totalDays) * 100, 100)}%` }"
                />
              </div>
            </div>
          </div>
        </div>
      </AppCard>

      <!-- Recent requests -->
      <AppCard title="Legutóbbi kérelmeim">
        <div v-if="recent.length === 0" class="text-center py-8 text-slate-400">
          <svg class="w-10 h-10 mx-auto mb-2 opacity-40" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
          </svg>
          <p class="text-sm">Még nincs kérelmezett szabadság</p>
          <AppButton variant="primary" size="sm" class="mt-3" @click="router.push('/leaves/new')">Első kérelem beadása</AppButton>
        </div>
        <div v-else class="divide-y divide-black/5 dark:divide-white/5">
          <div v-for="r in recent" :key="r.id" class="py-3 px-2 last:pb-0">
            <div class="flex items-center gap-2 flex-wrap">
              <AppBadge :value="r.category" type="category" />
              <span class="text-sm text-slate-600 dark:text-slate-300">
                {{ formatDate(r.startDate) }} – {{ formatDate(r.endDate) }}
                <span class="text-slate-400">({{ daysBetween(r.startDate, r.endDate) }} nap)</span>
              </span>
              <AppBadge :value="r.status" type="status" />
            </div>
            <p v-if="r.requestNote" class="text-xs text-slate-400 mt-0.5 truncate">{{ r.requestNote }}</p>
          </div>
        </div>
        <div v-if="recent.length" class="mt-2 pt-4 border-t border-black/5 dark:border-white/5">
          <AppButton variant="ghost" size="sm" @click="router.push('/leaves')">Összes megtekintése →</AppButton>
        </div>
      </AppCard>
    </template>
  </div>
</template>
