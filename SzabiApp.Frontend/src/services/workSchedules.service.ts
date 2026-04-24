import api from './api'
import type { WorkScheduleDto, CreateWorkScheduleDto, UpdateWorkScheduleDto } from '@/types'

export const workSchedulesService = {
  getAll: () =>
    api.get<WorkScheduleDto[]>('/api/workschedules').then((r) => r.data),

  getById: (id: string) =>
    api.get<WorkScheduleDto>(`/api/workschedules/${id}`).then((r) => r.data),

  create: (dto: CreateWorkScheduleDto) =>
    api.post<WorkScheduleDto>('/api/workschedules', dto).then((r) => r.data),

  update: (id: string, dto: UpdateWorkScheduleDto) =>
    api.put<WorkScheduleDto>(`/api/workschedules/${id}`, dto).then((r) => r.data),

  delete: (id: string) =>
    api.delete(`/api/workschedules/${id}`),
}
