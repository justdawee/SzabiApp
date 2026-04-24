/**
 * OpenHolidays API — https://openholidaysapi.org
 * Free, no authentication required.
 */

const BASE = 'https://openholidaysapi.org'

interface OpenHolidayName {
  language: string
  text: string
}

interface OpenHoliday {
  id: string
  startDate: string
  endDate:   string
  name:      OpenHolidayName[]
  nationwide: boolean
}

export interface ImportableHoliday {
  /** Resolved Hungarian name */
  name:      string
  /** YYYY-MM-DD — only single-day or first day of a multi-day holiday */
  date:      string
  endDate:   string
  multiDay:  boolean
}

export async function fetchHungarianHolidays(year: number): Promise<ImportableHoliday[]> {
  const params = new URLSearchParams({
    countryIsoCode: 'HU',
    languageIsoCode: 'HU',
    validFrom: `${year}-01-01`,
    validTo:   `${year}-12-31`,
  })

  const res = await fetch(`${BASE}/PublicHolidays?${params}`, {
    headers: { Accept: 'application/json' },
  })

  if (!res.ok) throw new Error(`OpenHolidays API error: ${res.status}`)

  const data: OpenHoliday[] = await res.json()

  // Expand multi-day holidays into individual days so each day maps to one HolidayDto
  const result: ImportableHoliday[] = []

  for (const h of data) {
    if (!h.nationwide) continue

    const hu = h.name.find((n) => n.language === 'HU') ?? h.name[0]
    const label = hu?.text ?? 'Ismeretlen ünnep'

    const start = new Date(h.startDate)
    const end   = new Date(h.endDate)
    const multiDay = h.startDate !== h.endDate

    // Emit one entry per day in the range
    const cur = new Date(start)
    while (cur <= end) {
      result.push({
        name:     multiDay ? `${label} (${cur.toLocaleDateString('hu-HU', { month: 'long', day: 'numeric' })})` : label,
        date:     toDateStr(cur),
        endDate:  h.endDate,
        multiDay,
      })
      cur.setDate(cur.getDate() + 1)
    }
  }

  return result.sort((a, b) => a.date.localeCompare(b.date))
}

function toDateStr(d: Date) {
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}
