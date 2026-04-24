<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { usersService }       from '@/services/users.service'
import { workSchedulesService } from '@/services/workSchedules.service'
import { useToastStore }      from '@/stores/toast.store'
import type { UserDto, WorkScheduleDto, UpdateUserDto } from '@/types'
import { UserRole } from '@/types'
import AppButton  from '@/components/ui/AppButton.vue'
import AppModal   from '@/components/ui/AppModal.vue'
import AppInput   from '@/components/ui/AppInput.vue'
import AppSelect  from '@/components/ui/AppSelect.vue'
import AppSpinner from '@/components/ui/AppSpinner.vue'

const toast = useToastStore()

const users     = ref<UserDto[]>([])
const schedules = ref<WorkScheduleDto[]>([])
const loading   = ref(true)
const search    = ref('')

const editTarget = ref<UserDto | null>(null)
const editForm   = ref<UpdateUserDto>({})
const saving     = ref(false)

const deleteTarget = ref<UserDto | null>(null)
const deleting     = ref(false)

onMounted(async () => {
  try {
    const [u, s] = await Promise.all([usersService.getAll(), workSchedulesService.getAll()])
    users.value     = u
    schedules.value = s
  } catch {
    toast.error('Nem sikerült betölteni az adatokat.')
  } finally {
    loading.value = false
  }
})

const filtered = computed(() => {
  const q = search.value.toLowerCase()
  if (!q) return users.value
  return users.value.filter((u) =>
    u.firstName.toLowerCase().includes(q) ||
    u.lastName.toLowerCase().includes(q)  ||
    u.email.toLowerCase().includes(q)
  )
})

const roleOptions = [
  { value: UserRole.Employee, label: 'Alkalmazott' },
  { value: UserRole.Manager,  label: 'Vezető' },
  { value: UserRole.Admin,    label: 'Admin' },
]

const scheduleOptions = computed(() => [
  { value: '', label: '– Nincs –' },
  ...schedules.value.map((s) => ({ value: s.id, label: s.name })),
])

const managerOptions = computed(() => [
  { value: '', label: '– Nincs –' },
  ...users.value.filter((u) => u.role !== UserRole.Employee).map((u) => ({
    value: u.id,
    label: `${u.lastName} ${u.firstName}`,
  })),
])

function openEdit(u: UserDto) {
  editTarget.value = u
  editForm.value   = {
    firstName:      u.firstName,
    lastName:       u.lastName,
    email:          u.email,
    role:           u.role,
    isActive:       u.isActive,
    birthDate:      u.birthDate,
    managerId:      u.managerId ?? '',
    workScheduleId: u.workScheduleId ?? '',
  }
}

async function saveEdit() {
  if (!editTarget.value) return
  saving.value = true
  try {
    const payload: UpdateUserDto = { ...editForm.value }
    if (!payload.managerId)      delete payload.managerId
    if (!payload.workScheduleId) delete payload.workScheduleId
    const updated = await usersService.update(editTarget.value.id, payload)
    const idx = users.value.findIndex((u) => u.id === updated.id)
    if (idx !== -1) users.value[idx] = updated
    toast.success('Felhasználó mentve.')
    editTarget.value = null
  } catch {
    toast.error('Nem sikerült menteni.')
  } finally {
    saving.value = false
  }
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  deleting.value = true
  try {
    await usersService.delete(deleteTarget.value.id)
    users.value = users.value.filter((u) => u.id !== deleteTarget.value!.id)
    toast.success('Felhasználó törölve.')
    deleteTarget.value = null
  } catch {
    toast.error('Nem sikerült törölni.')
  } finally {
    deleting.value = false
  }
}

const roleLabel: Record<string, string> = {
  Employee: 'Alkalmazott',
  Manager:  'Vezető',
  Admin:    'Admin',
}
</script>

<template>
  <div class="space-y-6 max-w-5xl mx-auto">
    <div class="flex items-center justify-between gap-4 flex-wrap">
      <div>
        <h2 class="text-xl font-bold text-slate-800 dark:text-white">Felhasználók</h2>
        <p class="text-sm text-slate-500 mt-0.5">{{ users.length }} regisztrált felhasználó</p>
      </div>
      <div class="neuro rounded-xl flex items-center gap-2 px-3 py-2 w-full sm:w-64">
        <svg class="w-4 h-4 text-slate-400 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
        </svg>
        <input
          v-model="search"
          placeholder="Keresés..."
          class="flex-1 bg-transparent text-sm text-slate-700 dark:text-slate-200 placeholder:text-slate-400 outline-none"
        />
      </div>
    </div>

    <div v-if="loading" class="flex justify-center py-12"><AppSpinner size="lg" /></div>

    <template v-else>
      <div class="glass rounded-2xl overflow-hidden">
        <div class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead>
              <tr class="border-b border-black/5 dark:border-white/5">
                <th class="text-left px-5 py-3.5 font-semibold text-slate-600 dark:text-slate-300">Név</th>
                <th class="text-left px-5 py-3.5 font-semibold text-slate-600 dark:text-slate-300 hidden md:table-cell">Email</th>
                <th class="text-left px-5 py-3.5 font-semibold text-slate-600 dark:text-slate-300">Szerepkör</th>
                <th class="text-left px-5 py-3.5 font-semibold text-slate-600 dark:text-slate-300 hidden lg:table-cell">Státusz</th>
                <th class="px-5 py-3.5 w-24" />
              </tr>
            </thead>
            <tbody class="divide-y divide-black/5 dark:divide-white/5">
              <tr v-if="filtered.length === 0">
                <td colspan="5" class="px-5 py-10 text-center text-slate-400">Nincs találat.</td>
              </tr>
              <tr v-for="u in filtered" :key="u.id" class="hover:bg-black/2 dark:hover:bg-white/2 transition-colors">
                <td class="px-5 py-3.5">
                  <div class="flex items-center gap-3">
                    <div class="w-8 h-8 rounded-lg btn-gradient flex items-center justify-center text-white text-xs font-bold shrink-0">
                      {{ (u.lastName[0] + u.firstName[0]).toUpperCase() }}
                    </div>
                    <span class="font-medium text-slate-800 dark:text-slate-100">{{ u.lastName }} {{ u.firstName }}</span>
                  </div>
                </td>
                <td class="px-5 py-3.5 text-slate-500 hidden md:table-cell">{{ u.email }}</td>
                <td class="px-5 py-3.5">
                  <span :class="[
                    'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium',
                    u.role === UserRole.Admin    && 'bg-purple-100 text-purple-700 dark:bg-purple-500/15 dark:text-purple-300',
                    u.role === UserRole.Manager  && 'bg-indigo-100 text-indigo-700 dark:bg-indigo-500/15 dark:text-indigo-300',
                    u.role === UserRole.Employee && 'bg-slate-100  text-slate-600  dark:bg-slate-500/15  dark:text-slate-400',
                  ]">{{ roleLabel[u.role] }}</span>
                </td>
                <td class="px-5 py-3.5 hidden lg:table-cell">
                  <span :class="['inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium',
                    u.isActive
                      ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-500/15 dark:text-emerald-300'
                      : 'bg-slate-100 text-slate-500 dark:bg-slate-500/15 dark:text-slate-400']">
                    {{ u.isActive ? 'Aktív' : 'Inaktív' }}
                  </span>
                </td>
                <td class="px-5 py-3.5">
                  <div class="flex gap-1.5 justify-end">
                    <AppButton variant="ghost" size="sm" @click="openEdit(u)">
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                          d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/>
                      </svg>
                    </AppButton>
                    <AppButton variant="ghost" size="sm" @click="deleteTarget = u">
                      <svg class="w-4 h-4 text-rose-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                          d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
                      </svg>
                    </AppButton>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </template>

    <!-- Edit modal -->
    <AppModal title="Felhasználó szerkesztése" :open="!!editTarget" @close="editTarget = null">
      <div v-if="editTarget" class="space-y-4">
        <div class="grid grid-cols-2 gap-3">
          <AppInput v-model="(editForm as UpdateUserDto & Record<string,string>).lastName"  label="Vezetéknév" />
          <AppInput v-model="(editForm as UpdateUserDto & Record<string,string>).firstName" label="Keresztnév" />
        </div>
        <AppInput v-model="(editForm as UpdateUserDto & Record<string,string>).email" label="Email" type="email" />
        <AppInput v-model="(editForm as UpdateUserDto & Record<string,string>).birthDate" label="Születési dátum" type="date" />
        <AppSelect v-model="editForm.role" label="Szerepkör" :options="roleOptions" />
        <AppSelect v-model="(editForm as UpdateUserDto & Record<string,string>).managerId" label="Vezető" :options="managerOptions" />
        <AppSelect v-model="(editForm as UpdateUserDto & Record<string,string>).workScheduleId" label="Munkabeosztás" :options="scheduleOptions" />
        <label class="flex items-center gap-2 cursor-pointer select-none">
          <input v-model="editForm.isActive" type="checkbox" class="w-4 h-4 rounded text-indigo-500" />
          <span class="text-sm text-slate-700 dark:text-slate-300">Aktív felhasználó</span>
        </label>
      </div>
      <template #footer>
        <AppButton variant="ghost" @click="editTarget = null">Mégse</AppButton>
        <AppButton variant="primary" :loading="saving" @click="saveEdit">Mentés</AppButton>
      </template>
    </AppModal>

    <!-- Delete confirm -->
    <AppModal title="Felhasználó törlése" :open="!!deleteTarget" @close="deleteTarget = null">
      <p class="text-sm text-slate-600 dark:text-slate-300">
        Biztosan törlöd <strong>{{ deleteTarget?.lastName }} {{ deleteTarget?.firstName }}</strong> felhasználót? Ez nem visszavonható.
      </p>
      <template #footer>
        <AppButton variant="ghost" @click="deleteTarget = null">Mégse</AppButton>
        <AppButton variant="danger" :loading="deleting" @click="confirmDelete">Törlés</AppButton>
      </template>
    </AppModal>
  </div>
</template>
