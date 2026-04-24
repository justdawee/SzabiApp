<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import { UserRole } from '@/types'

const auth  = useAuthStore()
const route = useRoute()

interface NavItem { to: string; label: string; icon: string; roles?: UserRole[] }

const navItems = computed<NavItem[]>(() => [
  { to: '/dashboard', label: 'Áttekintés',      icon: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6' },
  { to: '/leaves',    label: 'Szabadságaim',    icon: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z' },
  { to: '/review',    label: 'Kérelmek',        icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4',
    roles: [UserRole.Manager, UserRole.Admin] },
  { to: '/admin/users',      label: 'Felhasználók', icon: 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z',
    roles: [UserRole.Admin] },
  { to: '/admin/allowances', label: 'Keretek',      icon: 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z',
    roles: [UserRole.Admin] },
  { to: '/admin/holidays',   label: 'Ünnepnapok',   icon: 'M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z',
    roles: [UserRole.Admin] },
])

const visible = computed(() =>
  navItems.value.filter((item) =>
    !item.roles || (auth.user && item.roles.includes(auth.user.role))
  )
)

function isActive(to: string) {
  return route.path === to || (to !== '/dashboard' && route.path.startsWith(to))
}

function initials(name: string) {
  return name.split(' ').map((n) => n[0]).slice(0, 2).join('').toUpperCase()
}
</script>

<template>
  <aside class="w-64 flex flex-col min-h-screen glass border-r border-black/5 dark:border-white/5 shrink-0">
    <!-- Logo -->
    <div class="px-6 pt-7 pb-5 border-b border-black/5 dark:border-white/5">
      <div class="flex items-center gap-3">
        <div class="w-9 h-9 rounded-xl btn-gradient flex items-center justify-center shrink-0">
          <svg class="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
          </svg>
        </div>
        <span class="text-xl font-bold tracking-tight text-slate-800 dark:text-white">SzabiApp</span>
      </div>
    </div>

    <!-- Navigation -->
    <nav class="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
      <RouterLink
        v-for="item in visible"
        :key="item.to"
        :to="item.to"
        :class="[
          'flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-all duration-200 group',
          isActive(item.to)
            ? 'bg-gradient-to-r from-indigo-500/15 to-purple-500/10 text-indigo-600 dark:text-indigo-400 shadow-sm'
            : 'text-slate-600 dark:text-slate-400 hover:bg-black/5 dark:hover:bg-white/5',
        ]"
      >
        <svg
          :class="['w-5 h-5 shrink-0', isActive(item.to) ? 'text-indigo-500' : 'text-slate-400 group-hover:text-slate-600 dark:group-hover:text-slate-300']"
          fill="none" stroke="currentColor" viewBox="0 0 24 24"
        >
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.8" :d="item.icon" />
        </svg>
        {{ item.label }}
        <!-- Active indicator -->
        <span v-if="isActive(item.to)" class="ml-auto w-1.5 h-1.5 rounded-full bg-indigo-500" />
      </RouterLink>
    </nav>

    <!-- User info + logout -->
    <div class="px-3 pb-5 pt-3 border-t border-black/5 dark:border-white/5 space-y-1">
      <div class="flex items-center gap-3 px-3 py-2.5 rounded-xl glass-sm">
        <div class="w-8 h-8 rounded-lg btn-gradient flex items-center justify-center text-white text-xs font-bold shrink-0">
          {{ auth.user ? initials(auth.user.fullName) : '?' }}
        </div>
        <div class="flex-1 min-w-0">
          <p class="text-sm font-medium text-slate-800 dark:text-slate-100 truncate">{{ auth.user?.fullName }}</p>
          <p class="text-xs text-slate-400 truncate">{{ auth.user?.role }}</p>
        </div>
      </div>
      <button
        class="flex w-full items-center gap-3 px-3 py-2.5 rounded-xl text-sm text-slate-500 hover:text-rose-500 hover:bg-rose-50 dark:hover:bg-rose-500/10 transition-all duration-200 group"
        @click="auth.logout(); $router.push('/login')"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.8"
            d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"/>
        </svg>
        Kijelentkezés
      </button>
    </div>
  </aside>
</template>
