import api from './api'
import type { HolidayDto, CreateHolidayDto } from '@/types'

export const holidaysService = {
  getAll: () =>
    api.get<HolidayDto[]>('/api/holidays').then((r) => r.data),

  create: (dto: CreateHolidayDto) =>
    api.post<HolidayDto>('/api/holidays', dto).then((r) => r.data),

  delete: (id: string) =>
    api.delete(`/api/holidays/${id}`),
}
