# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
npm run dev        # Start Vite dev server (http://localhost:5173)
npm run build      # Type-check (vue-tsc) then build for production
npm run preview    # Preview the production build locally
```

No test or lint scripts are configured yet (Vitest planned).

## Project Overview

**SzabiApp** is a leave/holiday management system (szabadságkezelő alkalmazás) with three user roles:
- **Employee (Alkalmazott):** Submit leave requests, view own leave balance
- **Manager (Vezető):** Approve/reject team's pending requests
- **Admin:** Manage users, set leave allowances, manage holidays/schedules, view statistics

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | Vue 3 (Composition API, `<script setup>`) + TypeScript |
| Build | Vite 8 + `@tailwindcss/vite` |
| State | Pinia (`src/stores/`) |
| Routing | Vue Router 5 (`src/router/index.ts`) |
| Styling | Tailwind CSS v4 (CSS-first config in `src/style.css`) |
| HTTP | Axios (`src/services/api.ts`) |
| Backend | .NET 10 Web API at `http://localhost:5062` |
| Auth | JWT (stored in `localStorage`) |

## Architecture

### Directory structure
```
src/
  types/index.ts          — All TypeScript interfaces and enums matching backend DTOs
  services/               — Thin Axios wrappers per backend controller
    api.ts                — Axios instance with JWT interceptor + 401 redirect
    auth.service.ts       — /api/auth/*
    leaves.service.ts     — /api/leaverequests/*
    users.service.ts      — /api/users/*
    holidays.service.ts   — /api/holidays/*
    allowances.service.ts — /api/leaveallowances/*
    workSchedules.service.ts — /api/workschedules/*
  stores/
    auth.store.ts         — JWT decode, user session, role guards (isAdmin, isManager, etc.)
    toast.store.ts        — Global toast notification queue
  router/index.ts         — Route definitions with beforeEach auth + role guards
  components/
    ui/                   — Design-system primitives (AppButton, AppCard, AppInput,
                            AppSelect, AppBadge, AppModal, AppSpinner, AppToastStack)
    layout/               — AppLayout (shell), AppSidebar, AppTopBar, AppBottomNav
  views/
    auth/LoginView.vue
    dashboard/DashboardView.vue
    leaves/MyLeavesView.vue, NewLeaveView.vue
    manager/ReviewView.vue
    admin/UsersView.vue, HolidaysView.vue, AllowancesView.vue
```

### Auth flow
1. `LoginView` calls `auth.login()` → Axios POST → JWT returned
2. `auth.store` decodes JWT (no library, manual base64), stores token + user in `localStorage`
3. Axios request interceptor attaches `Authorization: Bearer <token>` to every request
4. 401 response → interceptor clears storage and redirects to `/login`
5. `router.beforeEach` enforces `meta.roles` guards per route

### Design system (Tailwind v4)
Custom utility classes defined in `src/style.css` (not a JS config):
- `.glass` — Liquid-glass card (backdrop-blur + semi-transparent bg, dark/light variants)
- `.glass-sm` — Smaller glass surface
- `.neuro` — Neumorphic input field (inset shadows)
- `.btn-gradient` — Indigo→Purple gradient button with hover lift
- `.gradient-mesh` — Radial gradient background blobs

Dark mode is toggled via the `.dark` class on `<html>`. Preference saved to `localStorage('theme')`.

## Backend Integration

- Backend runs on `http://localhost:5062` (configure via `VITE_API_BASE_URL` in `.env.development`)
- CORS is already configured in the backend for `http://localhost:5173`
- JSON property names are **camelCase** (ASP.NET Core default) — TypeScript interfaces use camelCase
- All enums match backend C# enum names exactly: `UserRole`, `LeaveStatus`, `LeaveCategory`

## Environment files

| File | Purpose |
|------|---------|
| `.env.example` | Template (committed) |
| `.env.development` | Dev settings — `VITE_API_BASE_URL`, `VITE_APP_NAME` |
| `.env.development.local` | Local overrides (gitignored) |

## tsconfig notes

- `erasableSyntaxOnly: false` — needed to allow TypeScript enums (overrides `@vue/tsconfig` default)
- `paths: { "@/*": ["./src/*"] }` — `@` alias maps to `src/`
