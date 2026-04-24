<script setup lang="ts">
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'

const route = useRoute()
const auth  = useAuthStore()

const pageTitles: Record<string, string> = {
  '/dashboard':        'Áttekintés',
  '/leaves':           'Szabadságaim',
  '/leaves/new':       'Új szabadságkérelem',
  '/review':           'Kérelmek áttekintése',
  '/admin/users':      'Felhasználók',
  '/admin/allowances': 'Szabadság keretek',
  '/admin/holidays':   'Ünnepnapok',
}

function toggleTheme() {
  const html = document.documentElement
  const isDark = html.classList.toggle('dark')
  localStorage.setItem('theme', isDark ? 'dark' : 'light')
}
</script>

<template>
  <header class="glass border-b border-black/5 dark:border-white/5 px-4 md:px-6 h-16 flex items-center justify-between gap-4 sticky top-0 z-30">
    <!-- Page title (+ mobile logo) -->
    <div class="flex items-center gap-3">
      <!-- Mobile logo -->
      <div class="lg:hidden flex items-center gap-2">
        <div class="w-8 h-8 rounded-xl btn-gradient flex items-center justify-center">
          <svg class="w-4 h-4 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
          </svg>
        </div>
      </div>
      <h1 class="text-base font-semibold text-slate-800 dark:text-slate-100">
        {{ pageTitles[route.path] ?? 'SzabiApp' }}
      </h1>
    </div>

    <div class="flex items-center gap-2">
      <!-- Dark mode toggle -->
      <button
        class="w-9 h-9 rounded-xl glass-sm flex items-center justify-center text-slate-500 hover:text-indigo-500 transition-colors"
        @click="toggleTheme"
        title="Téma váltás"
      >
        <svg class="w-4.5 h-4.5 hidden dark:block" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
            d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z"/>
        </svg>
        <svg class="w-4.5 h-4.5 block dark:hidden" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
            d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z"/>
        </svg>
      </button>

      <!-- Avatar (mobile) -->
      <div class="lg:hidden w-9 h-9 rounded-xl btn-gradient flex items-center justify-center text-white text-xs font-bold">
        {{ auth.user?.fullName.split(' ').map((n) => n[0]).slice(0, 2).join('').toUpperCase() }}
      </div>
    </div>
  </header>
</template>
