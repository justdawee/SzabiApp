import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import { UserRole } from '@/types'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/auth/LoginView.vue'),
      meta: { public: true },
    },
    {
      path: '/',
      component: () => import('@/components/layout/AppLayout.vue'),
      children: [
        {
          path: '',
          redirect: '/dashboard',
        },
        {
          path: 'dashboard',
          name: 'dashboard',
          component: () => import('@/views/dashboard/DashboardView.vue'),
        },
        {
          path: 'leaves',
          name: 'leaves',
          component: () => import('@/views/leaves/MyLeavesView.vue'),
        },
        {
          path: 'leaves/new',
          name: 'leaves-new',
          component: () => import('@/views/leaves/NewLeaveView.vue'),
        },
        // Manager
        {
          path: 'review',
          name: 'review',
          component: () => import('@/views/manager/ReviewView.vue'),
          meta: { roles: [UserRole.Manager, UserRole.Admin] },
        },
        // Admin
        {
          path: 'admin/users',
          name: 'admin-users',
          component: () => import('@/views/admin/UsersView.vue'),
          meta: { roles: [UserRole.Admin] },
        },
        {
          path: 'admin/holidays',
          name: 'admin-holidays',
          component: () => import('@/views/admin/HolidaysView.vue'),
          meta: { roles: [UserRole.Admin] },
        },
        {
          path: 'admin/allowances',
          name: 'admin-allowances',
          component: () => import('@/views/admin/AllowancesView.vue'),
          meta: { roles: [UserRole.Admin] },
        },
      ],
    },
    { path: '/:pathMatch(.*)*', redirect: '/dashboard' },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()

  if (!to.meta.public && !auth.isAuthenticated) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }
  if (to.name === 'login' && auth.isAuthenticated) {
    return { name: 'dashboard' }
  }
  if (to.meta.roles) {
    const allowed = to.meta.roles as UserRole[]
    if (!auth.user || !allowed.includes(auth.user.role)) {
      return { name: 'dashboard' }
    }
  }
})

export default router
