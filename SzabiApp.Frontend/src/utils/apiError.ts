/**
 * Extracts a human-readable error message from any backend error response.
 *
 * Handles all formats the backend can return:
 *  - { error: "..." }              — Result.Failure()
 *  - { errors: { field: [...] } }  — FluentValidation (ValidationFilter)
 *  - { detail: "..." }             — ASP.NET Core ProblemDetails
 *  - { title: "..." }              — ASP.NET Core ProblemDetails
 */
export function extractApiError(e: unknown, fallback = 'Ismeretlen hiba történt.'): string {
  const data = (e as { response?: { data?: unknown } })?.response?.data

  if (!data || typeof data !== 'object') return fallback

  const d = data as Record<string, unknown>

  // { error: "string" }
  if (typeof d.error === 'string' && d.error) return d.error

  // { errors: { field: ["msg1", ...] } } — FluentValidation
  if (d.errors && typeof d.errors === 'object') {
    const msgs = Object.values(d.errors as Record<string, string[]>)
      .flat()
      .filter(Boolean)
    if (msgs.length) return msgs.join(' ')
  }

  // { detail: "..." } or { title: "..." }
  if (typeof d.detail === 'string' && d.detail) return d.detail
  if (typeof d.title  === 'string' && d.title)  return d.title

  return fallback
}
