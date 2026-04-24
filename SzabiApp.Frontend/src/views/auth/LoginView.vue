<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore }  from '@/stores/auth.store'
import { useToastStore } from '@/stores/toast.store'
import { authService }   from '@/services/auth.service'
import { UserRole }      from '@/types'
import AppButton from '@/components/ui/AppButton.vue'
import AppInput  from '@/components/ui/AppInput.vue'
import { extractApiError } from '@/utils/apiError'

const router = useRouter()
const route  = useRoute()
const auth   = useAuthStore()
const toast  = useToastStore()

// ─── Tab ──────────────────────────────────────────────────────────────────────
const tab = ref<'login' | 'register'>('login')
function switchTab(t: 'login' | 'register') {
  tab.value    = t
  error.value  = ''
}

// ─── Shared ───────────────────────────────────────────────────────────────────
const loading = ref(false)
const error   = ref('')

function toggleTheme() {
  const isDark = document.documentElement.classList.toggle('dark')
  localStorage.setItem('theme', isDark ? 'dark' : 'light')
}

// ─── Login ────────────────────────────────────────────────────────────────────
const loginEmail    = ref('')
const loginPassword = ref('')

async function submitLogin() {
  error.value   = ''
  loading.value = true
  try {
    await auth.login({ email: loginEmail.value, password: loginPassword.value })
    router.push((route.query.redirect as string) ?? '/dashboard')
  } catch (e: unknown) {
    error.value = extractApiError(e, 'Hibás email cím vagy jelszó.')
  } finally {
    loading.value = false
  }
}

// ─── Register ─────────────────────────────────────────────────────────────────
const reg = ref({
  firstName:       '',
  lastName:        '',
  email:           '',
  birthDate:       '',
  password:        '',
  confirmPassword: '',
  role:            UserRole.Employee as UserRole,
})

const passwordMismatch = computed(
  () => reg.value.confirmPassword.length > 0 && reg.value.password !== reg.value.confirmPassword
)

const roles: { value: UserRole; label: string; desc: string; icon: string; color: string }[] = [
  {
    value: UserRole.Employee,
    label: 'Alkalmazott',
    desc:  'Szabadságot kérelmez, nyomon követi egyenlegét',
    icon:  'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z',
    color: 'indigo',
  },
  {
    value: UserRole.Manager,
    label: 'Vezető',
    desc:  'Jóváhagyja vagy elutasítja a beosztottak kérelmeit',
    icon:  'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z',
    color: 'violet',
  },
  {
    value: UserRole.Admin,
    label: 'Admin',
    desc:  'Teljes hozzáférés, felhasználók és keretek kezelése',
    icon:  'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z M15 12a3 3 0 11-6 0 3 3 0 016 0z',
    color: 'purple',
  },
]

const roleColorMap: Record<string, string> = {
  indigo: 'border-indigo-400  bg-indigo-50/80  dark:border-indigo-500  dark:bg-indigo-500/10  shadow-indigo-100  dark:shadow-indigo-500/10',
  violet: 'border-violet-400  bg-violet-50/80  dark:border-violet-500  dark:bg-violet-500/10  shadow-violet-100  dark:shadow-violet-500/10',
  purple: 'border-purple-400  bg-purple-50/80  dark:border-purple-500  dark:bg-purple-500/10  shadow-purple-100  dark:shadow-purple-500/10',
}
const roleIconMap: Record<string, string> = {
  indigo: 'text-indigo-600 dark:text-indigo-400',
  violet: 'text-violet-600 dark:text-violet-400',
  purple: 'text-purple-600 dark:text-purple-400',
}

async function submitRegister() {
  error.value = ''
  if (!reg.value.firstName || !reg.value.lastName || !reg.value.email || !reg.value.birthDate || !reg.value.password) {
    error.value = 'Kérjük töltsd ki az összes kötelező mezőt.'
    return
  }
  if (reg.value.password !== reg.value.confirmPassword) {
    error.value = 'A két jelszó nem egyezik meg.'
    return
  }
  if (reg.value.password.length < 6) {
    error.value = 'A jelszónak legalább 6 karakter hosszúnak kell lennie.'
    return
  }
  loading.value = true
  try {
    const res = await authService.register({
      firstName: reg.value.firstName,
      lastName:  reg.value.lastName,
      email:     reg.value.email,
      birthDate: reg.value.birthDate,
      password:  reg.value.password,
      role:      reg.value.role,
    })
    toast.success(`Fiókod sikeresen létrejött, ${res.lastName} ${res.firstName}! Kérjük jelentkezz be.`)
    reg.value = { firstName: '', lastName: '', email: '', birthDate: '', password: '', confirmPassword: '', role: reg.value.role }
    loginEmail.value = res.email
    switchTab('login')
  } catch (e: unknown) {
    error.value = extractApiError(e, 'Hiba történt a regisztráció során.')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="gradient-mesh min-h-screen flex items-center justify-center p-4">
    <!-- Decorative blobs -->
    <div class="pointer-events-none fixed inset-0 overflow-hidden" aria-hidden="true">
      <div class="absolute -top-40 -left-40 w-96 h-96 rounded-full bg-indigo-400/20 blur-3xl dark:bg-indigo-500/15" />
      <div class="absolute -bottom-40 -right-40 w-96 h-96 rounded-full bg-purple-400/20 blur-3xl dark:bg-purple-500/15" />
      <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-64 h-64 rounded-full bg-pink-300/10 blur-3xl dark:bg-pink-500/8" />
    </div>

    <div class="w-full max-w-sm relative">
      <!-- Logo -->
      <div class="text-center mb-6">
        <div class="inline-flex items-center justify-center w-14 h-14 rounded-2xl btn-gradient mb-3 shadow-lg shadow-indigo-500/30">
          <svg class="w-7 h-7 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
          </svg>
        </div>
        <h1 class="text-2xl font-bold tracking-tight text-slate-800 dark:text-white">SzabiApp</h1>
        <p class="text-sm text-slate-500 dark:text-slate-400 mt-0.5">Szabadságkezelő rendszer</p>
      </div>

      <!-- Tab switcher -->
      <div class="glass-sm rounded-2xl p-1 flex gap-1 mb-4">
        <button
          v-for="t in [{ key: 'login', label: 'Bejelentkezés' }, { key: 'register', label: 'Regisztráció' }]"
          :key="t.key"
          :class="[
            'flex-1 py-2 rounded-xl text-sm font-medium transition-all duration-200',
            tab === t.key
              ? 'bg-white dark:bg-slate-700 text-slate-800 dark:text-slate-100 shadow-sm'
              : 'text-slate-500 dark:text-slate-400 hover:text-slate-700 dark:hover:text-slate-300',
          ]"
          @click="switchTab(t.key as 'login' | 'register')"
        >
          {{ t.label }}
        </button>
      </div>

      <!-- Card -->
      <div class="glass rounded-2xl overflow-hidden">
        <!-- Error alert -->
        <Transition name="slide-down">
          <div
            v-if="error"
            class="mx-5 mt-5 rounded-xl bg-rose-50 dark:bg-rose-500/10 border border-rose-200 dark:border-rose-500/20 px-4 py-3 text-sm text-rose-600 dark:text-rose-400"
          >
            {{ error }}
          </div>
        </Transition>

        <!-- ─── Login form ──────────────────────────────────────── -->
        <Transition name="tab" mode="out-in">
          <form v-if="tab === 'login'" key="login" class="p-6 space-y-4" @submit.prevent="submitLogin">
            <AppInput
              v-model="loginEmail"
              label="Email cím"
              type="email"
              placeholder="pelda@ceg.hu"
              required
            />
            <AppInput
              v-model="loginPassword"
              label="Jelszó"
              type="password"
              placeholder="••••••••"
              required
            />
            <AppButton type="submit" variant="primary" size="lg" :loading="loading" class="w-full !mt-6">
              Bejelentkezés
            </AppButton>
          </form>

          <!-- ─── Register form ───────────────────────────────── -->
          <form v-else key="register" class="p-6 space-y-4" @submit.prevent="submitRegister">
            <!-- Name row -->
            <div class="grid grid-cols-2 gap-3">
              <AppInput v-model="reg.lastName"  label="Vezetéknév" placeholder="Nagy"      required />
              <AppInput v-model="reg.firstName" label="Keresztnév" placeholder="Bertalan"  required />
            </div>

            <AppInput
              v-model="reg.email"
              label="Email cím"
              type="email"
              placeholder="pelda@ceg.hu"
              required
            />

            <AppInput
              v-model="reg.birthDate"
              label="Születési dátum"
              type="date"
              required
            />

            <AppInput
              v-model="reg.password"
              label="Jelszó"
              type="password"
              placeholder="Min. 6 karakter"
              required
            />

            <AppInput
              v-model="reg.confirmPassword"
              label="Jelszó megerősítése"
              type="password"
              placeholder="••••••••"
              :error="passwordMismatch ? 'A két jelszó nem egyezik.' : ''"
              required
            />

            <!-- Role selector -->
            <div class="space-y-2">
              <p class="text-sm font-medium text-slate-700 dark:text-slate-300">
                Szerepkör <span class="text-rose-500">*</span>
              </p>
              <div class="grid grid-cols-3 gap-2">
                <button
                  v-for="r in roles"
                  :key="r.value"
                  type="button"
                  :class="[
                    'relative flex flex-col items-center gap-1.5 p-3 rounded-xl border-2 transition-all duration-200 cursor-pointer text-center',
                    reg.role === r.value
                      ? ['shadow-lg', roleColorMap[r.color]]
                      : 'border-transparent glass-sm hover:border-slate-200 dark:hover:border-slate-600',
                  ]"
                  @click="reg.role = r.value"
                >
                  <!-- Checkmark -->
                  <Transition name="pop">
                    <div
                      v-if="reg.role === r.value"
                      :class="['absolute top-1.5 right-1.5 w-4 h-4 rounded-full flex items-center justify-center btn-gradient']"
                    >
                      <svg class="w-2.5 h-2.5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 13l4 4L19 7"/>
                      </svg>
                    </div>
                  </Transition>

                  <!-- Icon -->
                  <div :class="['w-8 h-8 rounded-lg flex items-center justify-center', reg.role === r.value ? 'btn-gradient' : 'bg-slate-100 dark:bg-slate-700']">
                    <svg
                      :class="['w-4 h-4', reg.role === r.value ? 'text-white' : 'text-slate-500 dark:text-slate-300']"
                      fill="none" stroke="currentColor" viewBox="0 0 24 24"
                    >
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.8" :d="r.icon" />
                    </svg>
                  </div>

                  <span :class="['text-xs font-semibold', reg.role === r.value ? roleIconMap[r.color] : 'text-slate-600 dark:text-slate-300']">
                    {{ r.label }}
                  </span>
                </button>
              </div>

              <!-- Role description -->
              <Transition name="fade" mode="out-in">
                <p
                  :key="reg.role"
                  class="text-xs text-slate-500 dark:text-slate-400 text-center px-2 min-h-[2.5rem] flex items-center justify-center"
                >
                  {{ roles.find(r => r.value === reg.role)?.desc }}
                </p>
              </Transition>
            </div>

            <AppButton
              type="submit"
              variant="primary"
              size="lg"
              :loading="loading"
              :disabled="passwordMismatch"
              class="w-full !mt-2"
            >
              Fiók létrehozása
            </AppButton>
          </form>
        </Transition>
      </div>

      <!-- Theme toggle -->
      <div class="text-center mt-5">
        <button
          class="text-xs text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 transition-colors"
          @click="toggleTheme"
        >
          ☀ / ☾ &nbsp;Téma váltás
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.tab-enter-active, .tab-leave-active  { transition: all 0.18s ease; }
.tab-enter-from                       { opacity: 0; transform: translateY(6px); }
.tab-leave-to                         { opacity: 0; transform: translateY(-4px); }

.slide-down-enter-active, .slide-down-leave-active { transition: all 0.2s ease; }
.slide-down-enter-from, .slide-down-leave-to       { opacity: 0; transform: translateY(-6px); max-height: 0; }

.pop-enter-active  { transition: all 0.15s cubic-bezier(0.34, 1.56, 0.64, 1); }
.pop-enter-from    { opacity: 0; transform: scale(0.5); }
.pop-leave-active  { transition: all 0.1s ease; }
.pop-leave-to      { opacity: 0; transform: scale(0.5); }

.fade-enter-active, .fade-leave-active { transition: opacity 0.15s ease; }
.fade-enter-from, .fade-leave-to       { opacity: 0; }
</style>
