import api from './api'
import type { LeaveAllowanceDto, CreateLeaveAllowanceDto, UpdateLeaveAllowanceDto } from '@/types'

export const allowancesService = {
  getAll: () =>
    api.get<LeaveAllowanceDto[]>('/api/leaveallowances').then((r) => r.data),

  getMy: () =>
    api.get<LeaveAllowanceDto[]>('/api/leaveallowances/my').then((r) => r.data),

  getByUser: (userId: string) =>
    api.get<LeaveAllowanceDto[]>(`/api/leaveallowances/user/${userId}`).then((r) => r.data),

  create: (dto: CreateLeaveAllowanceDto) =>
    api.post<LeaveAllowanceDto>('/api/leaveallowances', dto).then((r) => r.data),

  update: (id: string, dto: UpdateLeaveAllowanceDto) =>
    api.put<LeaveAllowanceDto>(`/api/leaveallowances/${id}`, dto).then((r) => r.data),

  delete: (id: string) =>
    api.delete(`/api/leaveallowances/${id}`),
}
