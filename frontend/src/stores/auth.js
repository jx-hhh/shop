import { defineStore } from 'pinia'
import { login as loginApi, register as registerApi } from '@/api'
import router from '@/router'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: JSON.parse(localStorage.getItem('user') || 'null'),
    token: localStorage.getItem('access_token') || '',
    refreshToken: localStorage.getItem('refresh_token') || ''
  }),

  getters: {
    isAuthenticated: (state) => !!state.token,
    username: (state) => state.user?.username || ''
  },

  actions: {
    async login(credentials) {
      try {
        const response = await loginApi(credentials)
        if (response.success) {
          const { accessToken, refreshToken, user } = response.data

          this.token = accessToken
          this.refreshToken = refreshToken
          this.user = user

          localStorage.setItem('access_token', accessToken)
          localStorage.setItem('refresh_token', refreshToken)
          localStorage.setItem('user', JSON.stringify(user))

          router.push('/')
        }
      } catch (error) {
        console.error('Login failed:', error)
        throw error
      }
    },

    async register(userData) {
      try {
        const response = await registerApi(userData)
        if (response.success) {
          const { accessToken, refreshToken, user } = response.data

          this.token = accessToken
          this.refreshToken = refreshToken
          this.user = user

          localStorage.setItem('access_token', accessToken)
          localStorage.setItem('refresh_token', refreshToken)
          localStorage.setItem('user', JSON.stringify(user))

          router.push('/')
        }
      } catch (error) {
        console.error('Register failed:', error)
        throw error
      }
    },

    logout() {
      this.token = ''
      this.refreshToken = ''
      this.user = null

      localStorage.removeItem('access_token')
      localStorage.removeItem('refresh_token')
      localStorage.removeItem('user')

      router.push('/login')
    }
  }
})
