import api from './api'
import type { LoginRequestDto, RegisterRequestDto, AuthResponseDto, UserDto } from '@/types'

export const authService = {
  login: (dto: LoginRequestDto) =>
    api.post<AuthResponseDto>('/api/auth/login', dto).then((r) => r.data),

  register: (dto: RegisterRequestDto) =>
    api.post<UserDto>('/api/auth/register', dto).then((r) => r.data),
}
