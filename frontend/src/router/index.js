import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'Home',
      component: () => import('@/views/Home.vue')
    },
    {
      path: '/login',
      name: 'Login',
      component: () => import('@/views/Login.vue')
    },
    {
      path: '/register',
      name: 'Register',
      component: () => import('@/views/Register.vue')
    },
    {
      path: '/books/:id',
      name: 'BookDetail',
      component: () => import('@/views/BookDetail.vue')
    },
    {
      path: '/basket',
      name: 'Basket',
      component: () => import('@/views/Basket.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/orders',
      name: 'Orders',
      component: () => import('@/views/Orders.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/orders/:id',
      name: 'OrderDetail',
      component: () => import('@/views/OrderDetail.vue'),
      meta: { requiresAuth: true }
    }
  ]
})

// 路由守卫
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login')
  } else {
    next()
  }
})

export default router
