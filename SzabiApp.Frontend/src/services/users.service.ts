import api from './api'
import type { UserDto, CreateUserDto, UpdateUserDto, ChangePasswordDto } from '@/types'

export const usersService = {
  getAll: () =>
    api.get<UserDto[]>('/api/users').then((r) => r.data),

  getMe: () =>
    api.get<UserDto>('/api/users/me').then((r) => r.data),

  getById: (id: string) =>
    api.get<UserDto>(`/api/users/${id}`).then((r) => r.data),

  create: (dto: CreateUserDto) =>
    api.post<UserDto>('/api/auth/register', dto).then((r) => r.data),

  update: (id: string, dto: UpdateUserDto) =>
    api.put<UserDto>(`/api/users/${id}`, dto).then((r) => r.data),

  delete: (id: string) =>
    api.delete(`/api/users/${id}`),

  changePassword: (dto: ChangePasswordDto) =>
    api.post('/api/users/me/password', dto),
}
