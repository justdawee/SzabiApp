import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authService } from '@/services/auth.service'
import type { AuthUser, LoginRequestDto } from '@/types'
import { UserRole } from '@/types'

function decodeJwt(token: string): Record<string, unknown> {
  try {
    const base64Url = token.split('.')[1]
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
    return JSON.parse(decodeURIComponent(
      atob(base64).split('').map((c) =>
        '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
      ).join(''),
    ))
  } catch {
    return {}
  }
}

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('token'))
  const user  = ref<AuthUser | null>(
    JSON.parse(localStorage.getItem('user') ?? 'null'),
  )

  const isAuthenticated = computed(() => !!token.value)
  const isEmployee = computed(() => user.value?.role === UserRole.Employee)
  const isManager  = computed(() => user.value?.role === UserRole.Manager)
  const isAdmin    = computed(() => user.value?.role === UserRole.Admin)
  const isManagerOrAdmin = computed(() => isManager.value || isAdmin.value)

  async function login(dto: LoginRequestDto) {
    const res = await authService.login(dto)
    // Role and email come from the response body (AuthResponseDto).
    // We only decode the JWT for the user ID (sub claim).
    _setSession(res.token, res.fullName, res.email, res.role as UserRole)
  }

  function logout() {
    token.value = null
    user.value  = null
    localStorage.removeItem('token')
    localStorage.removeItem('user')
  }

  function _setSession(rawToken: string, fullName: string, email?: string, role?: UserRole) {
    const payload = decodeJwt(rawToken)

    // .NET encodes ClaimTypes.Role as the full URI key in the JWT
    const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'

    token.value = rawToken
    user.value  = {
      id:       payload['sub'] as string,
      email:    email ?? payload['email'] as string,
      fullName,
      role:     role ?? (payload[ROLE_CLAIM] as UserRole) ?? (payload['role'] as UserRole) ?? UserRole.Employee,
    }
    localStorage.setItem('token', rawToken)
    localStorage.setItem('user', JSON.stringify(user.value))
  }

  return { token, user, isAuthenticated, isEmployee, isManager, isAdmin, isManagerOrAdmin, login, logout, setSession: _setSession }
})
