import { defineStore } from 'pinia'
import { ref } from 'vue'

export type ToastType = 'success' | 'error' | 'warning' | 'info'

export interface Toast {
  id: number
  message: string
  type: ToastType
}

export const useToastStore = defineStore('toast', () => {
  const toasts = ref<Toast[]>([])
  let nextId = 0

  function push(message: string, type: ToastType = 'info', duration = 4000) {
    const id = ++nextId
    toasts.value.push({ id, message, type })
    setTimeout(() => remove(id), duration)
  }

  function remove(id: number) {
    toasts.value = toasts.value.filter((t) => t.id !== id)
  }

  const success = (msg: string) => push(msg, 'success')
  const error   = (msg: string) => push(msg, 'error', 5000)
  const warning = (msg: string) => push(msg, 'warning')
  const info    = (msg: string) => push(msg, 'info')

  return { toasts, push, remove, success, error, warning, info }
})
