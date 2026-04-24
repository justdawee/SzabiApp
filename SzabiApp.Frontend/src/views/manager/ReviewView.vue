<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { leavesService } from '@/services/leaves.service'
import { useToastStore } from '@/stores/toast.store'
import type { LeaveRequestDto } from '@/types'
import { LeaveStatus } from '@/types'
import AppBadge   from '@/components/ui/AppBadge.vue'
import AppButton  from '@/components/ui/AppButton.vue'
import AppModal   from '@/components/ui/AppModal.vue'
import AppSpinner from '@/components/ui/AppSpinner.vue'

const toast = useToastStore()

const requests     = ref<LeaveRequestDto[]>([])
const loading      = ref(true)
const reviewTarget = ref<LeaveRequestDto | null>(null)
const reviewNote   = ref('')
const submitting   = ref(false)
const activeTab    = ref<'pending' | 'all'>('pending')

onMounted(async () => {
  try {
    requests.value = await leavesService.getAll()
  } catch {
    toast.error('Nem sikerült betölteni a kérelmeket.')
  } finally {
    loading.value = false
  }
})

const pending = computed(() => requests.value.filter((r) => r.status === LeaveStatus.Pending))
const displayed = computed(() => activeTab.value === 'pending' ? pending.value : requests.value)

async function doReview(decision: LeaveStatus) {
  if (!reviewTarget.value) return
  submitting.value = true
  try {
    const updated = await leavesService.review(reviewTarget.value.id, {
      decision,
      reviewNote: reviewNote.value || undefined,
    })
    const idx = requests.value.findIndex((r) => r.id === updated.id)
    if (idx !== -1) requests.value[idx] = updated
    toast.success(decision === LeaveStatus.Approved ? 'Kérelem jóváhagyva.' : 'Kérelem elutasítva.')
    reviewTarget.value = null
    reviewNote.value   = ''
  } catch {
    toast.error('Hiba történt a döntés mentésekor.')
  } finally {
    submitting.value = false
  }
}

function formatDate(d: string) {
  return new Date(d).toLocaleDateString('hu-HU', { month: 'short', day: 'numeric' })
}

function days(start: string, end: string) {
  return Math.round((new Date(end).getTime() - new Date(start).getTime()) / 86_400_000) + 1
}
</script>

<template>
  <div class="space-y-6 max-w-5xl mx-auto">
    <!-- Header -->
    <div class="flex items-center justify-between flex-wrap gap-3">
      <div>
        <h2 class="text-xl font-bold text-slate-800 dark:text-white">Kérelmek áttekintése</h2>
        <p class="text-sm text-slate-500 dark:text-slate-400 mt-0.5">
          <span class="text-amber-500 font-medium">{{ pending.length }}</span> függőben lévő kérelem
        </p>
      </div>
      <!-- Tabs -->
      <div class="glass-sm rounded-xl p-1 flex gap-1">
        <button
          v-for="tab in [{ key: 'pending', label: 'Függőben' }, { key: 'all', label: 'Összes' }]"
          :key="tab.key"
          :class="[
            'px-4 py-1.5 rounded-lg text-sm font-medium transition-all duration-200',
            activeTab === tab.key
              ? 'bg-white dark:bg-slate-700 text-slate-800 dark:text-slate-100 shadow-sm'
              : 'text-slate-500 hover:text-slate-700 dark:hover:text-slate-300',
          ]"
          @click="activeTab = tab.key as 'pending' | 'all'"
        >
          {{ tab.label }}
          <span v-if="tab.key === 'pending' && pending.length" class="ml-1.5 bg-amber-500 text-white text-xs rounded-full px-1.5">
            {{ pending.length }}
          </span>
        </button>
      </div>
    </div>

    <div v-if="loading" class="flex justify-center py-12"><AppSpinner size="lg" /></div>

    <template v-else>
      <div v-if="displayed.length === 0" class="glass rounded-2xl p-12 text-center">
        <svg class="w-12 h-12 mx-auto text-slate-300 dark:text-slate-600 mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
            d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/>
        </svg>
        <p class="text-slate-500 dark:text-slate-400">Nincs {{ activeTab === 'pending' ? 'függőben lévő' : '' }} kérelem.</p>
      </div>

      <div v-else class="space-y-3">
        <div
          v-for="r in displayed"
          :key="r.id"
          class="glass rounded-2xl p-5 flex flex-col sm:flex-row sm:items-start gap-4"
          :class="r.status === LeaveStatus.Pending && 'ring-1 ring-amber-300/40 dark:ring-amber-500/20'"
        >
          <div class="flex-1 min-w-0 space-y-2">
            <div class="flex items-center gap-2 flex-wrap">
              <span class="text-sm font-medium text-slate-800 dark:text-slate-100">{{ r.userFullName }}</span>
              <AppBadge :value="r.category" type="category" />
              <AppBadge :value="r.status"   type="status" />
            </div>
            <p class="text-sm text-slate-600 dark:text-slate-300">
              {{ formatDate(r.startDate) }} – {{ formatDate(r.endDate) }}
              <span class="text-slate-400 ml-1">({{ days(r.startDate, r.endDate) }} nap)</span>
            </p>
            <p v-if="r.requestNote" class="text-xs text-slate-500 dark:text-slate-400">
              "{{ r.requestNote }}"
            </p>
            <p v-if="r.reviewNote" class="text-xs text-slate-500 dark:text-slate-400">
              Döntés megjegyzés: {{ r.reviewNote }}
            </p>
          </div>
          <div v-if="r.status === LeaveStatus.Pending" class="flex gap-2 shrink-0">
            <AppButton variant="secondary" size="sm" @click="reviewTarget = r; reviewNote = ''">
              Döntés
            </AppButton>
          </div>
        </div>
      </div>
    </template>

    <!-- Review modal -->
    <AppModal title="Döntés a kérelemről" :open="!!reviewTarget" @close="reviewTarget = null">
      <div v-if="reviewTarget" class="space-y-4">
        <div class="glass-sm rounded-xl p-4 space-y-1.5">
          <p class="text-sm font-medium text-slate-800 dark:text-slate-100">{{ reviewTarget.userFullName }}</p>
          <p class="text-sm text-slate-500">{{ formatDate(reviewTarget.startDate) }} – {{ formatDate(reviewTarget.endDate) }} ({{ days(reviewTarget.startDate, reviewTarget.endDate) }} nap)</p>
          <AppBadge :value="reviewTarget.category" type="category" />
          <p v-if="reviewTarget.requestNote" class="text-xs text-slate-500 pt-1">"{{ reviewTarget.requestNote }}"</p>
        </div>
        <div class="flex flex-col gap-1.5">
          <label class="text-sm font-medium text-slate-700 dark:text-slate-300">Megjegyzés <span class="text-slate-400 font-normal">(opcionális)</span></label>
          <textarea
            v-model="reviewNote"
            rows="3"
            placeholder="Indoklás vagy megjegyzés a kérelmezőnek..."
            class="neuro w-full rounded-xl px-4 py-2.5 text-sm text-slate-800 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-600 outline-none focus:ring-2 focus:ring-indigo-500/40 resize-none"
          />
        </div>
        <div class="flex gap-3">
          <AppButton variant="danger" :loading="submitting" class="flex-1"
            @click="doReview(LeaveStatus.Denied)">
            Elutasít
          </AppButton>
          <AppButton variant="primary" :loading="submitting" class="flex-1"
            @click="doReview(LeaveStatus.Approved)">
            Jóváhagy
          </AppButton>
        </div>
      </div>
    </AppModal>
  </div>
</template>
