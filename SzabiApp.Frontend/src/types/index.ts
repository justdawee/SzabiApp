// ─── Calendar ─────────────────────────────────────────────────────────────────

export type CalendarEventType = 'holiday' | 'approved' | 'pending' | 'denied' | 'cancelled' | 'sick' | 'other'

export interface CalendarEvent {
  /** ISO date string YYYY-MM-DD  (start date for range events) */
  date: string
  /** ISO date string YYYY-MM-DD  (end date for range events, inclusive) */
  endDate?: string
  type: CalendarEventType
  label: string
}

export interface CalendarRange {
  start: string | null
  end:   string | null
}

// ─── Enums ────────────────────────────────────────────────────────────────────

export enum UserRole {
  Employee = 'Employee',
  Manager  = 'Manager',
  Admin    = 'Admin',
}

export enum LeaveStatus {
  Pending   = 'Pending',
  Approved  = 'Approved',
  Denied    = 'Denied',
  Cancelled = 'Cancelled',
}

export enum LeaveCategory {
  Annual    = 'Annual',
  Sick      = 'Sick',
  Unpaid    = 'Unpaid',
  Paternity = 'Paternity',
  Maternity = 'Maternity',
  Other     = 'Other',
}

// ─── Auth DTOs ────────────────────────────────────────────────────────────────

export interface LoginRequestDto {
  email: string
  password: string
}

export interface RegisterRequestDto {
  firstName: string
  lastName: string
  email: string
  password: string
  birthDate: string
  role: UserRole
  managerId?: string
  workScheduleId?: string
}

export interface AuthResponseDto {
  token: string
  email: string
  fullName: string
  role: string
}

// ─── User DTOs ────────────────────────────────────────────────────────────────

export interface UserDto {
  id: string
  firstName: string
  lastName: string
  email: string
  role: UserRole
  isActive: boolean
  birthDate: string
  managerId?: string
  managerFullName?: string
  workScheduleId?: string
  workScheduleName?: string
}

export interface CreateUserDto {
  firstName: string
  lastName: string
  email: string
  password: string
  role: UserRole
  managerId?: string
  workScheduleId?: string
}

export interface UpdateUserDto {
  firstName?: string
  lastName?: string
  email?: string
  role?: UserRole
  isActive?: boolean
  birthDate?: string
  managerId?: string
  workScheduleId?: string
}

export interface ChangePasswordDto {
  currentPassword: string
  newPassword: string
}

// ─── Leave Request DTOs ───────────────────────────────────────────────────────

export interface CreateLeaveRequestDto {
  category: LeaveCategory
  startDate: string
  endDate: string
  requestNote?: string
}

export interface LeaveRequestDto {
  id: string
  userId: string
  userFullName: string
  category: LeaveCategory
  startDate: string
  endDate: string
  status: LeaveStatus
  requestNote?: string
  reviewNote?: string
  reviewedById?: string
  reviewedByFullName?: string
  reviewedAt?: string
  createdAt: string
}

export interface ReviewLeaveRequestDto {
  decision: LeaveStatus
  reviewNote?: string
}

// ─── Holiday DTOs ─────────────────────────────────────────────────────────────

export interface HolidayDto {
  id: string
  name: string
  date: string
  isRecurringYearly: boolean
}

export interface CreateHolidayDto {
  name: string
  date: string
  isRecurringYearly: boolean
}

// ─── Work Schedule DTOs ───────────────────────────────────────────────────────

export interface WorkScheduleDto {
  id: string
  name: string
  workDaysPerWeek: number
  dailyWorkHours: number
}

export interface CreateWorkScheduleDto {
  name: string
  workDaysPerWeek: number
  dailyWorkHours: number
}

export interface UpdateWorkScheduleDto {
  name?: string
  workDaysPerWeek?: number
  dailyWorkHours?: number
}

// ─── Leave Allowance DTOs ─────────────────────────────────────────────────────

export interface LeaveAllowanceDto {
  id: string
  userId: string
  userFullName: string
  year: number
  category: LeaveCategory
  totalDays: number
  usedDays: number
  remainingDays: number
}

export interface CreateLeaveAllowanceDto {
  userId: string
  year: number
  category: LeaveCategory
  totalDays: number
}

export interface UpdateLeaveAllowanceDto {
  year?: number
  category?: LeaveCategory
  totalDays?: number
  usedDays?: number
}

// ─── JWT payload ─────────────────────────────────────────────────────────────

export interface JwtPayload {
  sub: string
  email: string
  role: string
  jti: string
  exp: number
  iss: string
  aud: string
}

// ─── Auth state ───────────────────────────────────────────────────────────────

export interface AuthUser {
  id: string
  email: string
  fullName: string
  role: UserRole
}
