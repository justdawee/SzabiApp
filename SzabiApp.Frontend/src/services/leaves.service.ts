import api from './api'
import type {
  LeaveRequestDto,
  CreateLeaveRequestDto,
  ReviewLeaveRequestDto,
} from '@/types'

export const leavesService = {
  // All requests (Manager / Admin)
  getAll: () =>
    api.get<LeaveRequestDto[]>('/api/leaverequests').then((r) => r.data),

  // Current user's requests
  getMy: () =>
    api.get<LeaveRequestDto[]>('/api/leaverequests/my').then((r) => r.data),

  getById: (id: string) =>
    api.get<LeaveRequestDto>(`/api/leaverequests/${id}`).then((r) => r.data),

  create: (dto: CreateLeaveRequestDto) =>
    api.post<LeaveRequestDto>('/api/leaverequests', dto).then((r) => r.data),

  review: (id: string, dto: ReviewLeaveRequestDto) =>
    api.patch<LeaveRequestDto>(`/api/leaverequests/${id}/review`, dto).then((r) => r.data),

  cancel: (id: string) =>
    api.patch<LeaveRequestDto>(`/api/leaverequests/${id}/cancel`).then((r) => r.data),
}
