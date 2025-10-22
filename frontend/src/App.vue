<template>
  <div id="app">
    <el-container>
      <el-header height="70px">
        <div class="header-content">
          <div class="logo" @click="$router.push('/')">
            <div class="logo-icon">📚</div>
            <div class="logo-text">
              <div class="logo-title">BookStore</div>
              <div class="logo-subtitle">在线书店</div>
            </div>
          </div>
          <div class="header-actions">
            <el-button @click="$router.push('/')" text class="nav-btn">
              <el-icon><HomeFilled /></el-icon>
              首页
            </el-button>
            <template v-if="authStore.isAuthenticated">
              <el-badge :value="basketCount" :hidden="basketCount === 0" class="basket-badge">
                <el-button @click="$router.push('/basket')" text class="nav-btn">
                  <el-icon><ShoppingCart /></el-icon>
                  购物车
                </el-button>
              </el-badge>
              <el-button @click="$router.push('/orders')" text class="nav-btn">
                <el-icon><List /></el-icon>
                我的订单
              </el-button>
              <el-dropdown>
                <div class="user-dropdown">
                  <el-avatar :size="32" class="user-avatar">
                    <el-icon><User /></el-icon>
                  </el-avatar>
                  <span class="user-name">{{ authStore.username }}</span>
                </div>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item @click="authStore.logout()">
                      <el-icon><SwitchButton /></el-icon>
                      退出登录
                    </el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </template>
            <template v-else>
              <el-button @click="$router.push('/login')" type="primary" round class="login-btn">登录</el-button>
              <el-button @click="$router.push('/register')" round class="register-btn">注册</el-button>
            </template>
          </div>
        </div>
      </el-header>
      <el-main>
        <router-view v-slot="{ Component }">
          <transition name="fade" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </el-main>
      <el-footer height="auto" class="footer">
        <div class="footer-content">
          <p>&copy; 2025 BookStore 在线书店. All rights reserved.</p>
          <p class="footer-links">
            <a href="#">关于我们</a>
            <span class="divider">|</span>
            <a href="#">联系方式</a>
            <span class="divider">|</span>
            <a href="#">隐私政策</a>
          </p>
        </div>
      </el-footer>
    </el-container>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const basketCount = ref(0)

// 这里可以添加获取购物车数量的逻辑
// onMounted(async () => {
//   if (authStore.isAuthenticated) {
//     // 获取购物车数量
//   }
// })
</script>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

#app {
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #333;
}

.el-container {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.el-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
  position: sticky;
  top: 0;
  z-index: 1000;
  display: flex;
  align-items: center;
  padding: 0 40px;
}

.header-content {
  width: 100%;
  max-width: 1400px;
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.logo {
  display: flex;
  align-items: center;
  gap: 15px;
  cursor: pointer;
  transition: transform 0.3s ease;
}

.logo:hover {
  transform: scale(1.05);
}

.logo-icon {
  font-size: 40px;
  filter: drop-shadow(0 2px 4px rgba(0, 0, 0, 0.2));
}

.logo-text {
  display: flex;
  flex-direction: column;
  color: white;
}

.logo-title {
  font-size: 24px;
  font-weight: 700;
  letter-spacing: 1px;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.logo-subtitle {
  font-size: 12px;
  opacity: 0.9;
  letter-spacing: 2px;
}

.header-actions {
  display: flex;
  gap: 10px;
  align-items: center;
}

.nav-btn {
  color: white !important;
  font-size: 15px;
  font-weight: 500;
  padding: 8px 16px;
  transition: all 0.3s ease;
}

.nav-btn:hover {
  background-color: rgba(255, 255, 255, 0.2) !important;
  transform: translateY(-2px);
}

.basket-badge {
  margin: 0;
}

.basket-badge :deep(.el-badge__content) {
  background-color: #f56c6c;
  border: 2px solid white;
}

.user-dropdown {
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
  padding: 5px 15px;
  border-radius: 20px;
  transition: all 0.3s ease;
}

.user-dropdown:hover {
  background-color: rgba(255, 255, 255, 0.2);
}

.user-avatar {
  background: linear-gradient(135deg, #ffd89b 0%, #19547b 100%);
}

.user-name {
  color: white;
  font-weight: 500;
}

.login-btn,
.register-btn {
  font-weight: 500;
  padding: 10px 24px;
  transition: all 0.3s ease;
}

.login-btn {
  background: white !important;
  color: #667eea !important;
  border: none;
}

.login-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(255, 255, 255, 0.3);
}

.register-btn {
  background: transparent !important;
  color: white !important;
  border: 2px solid white !important;
}

.register-btn:hover {
  background: white !important;
  color: #667eea !important;
  transform: translateY(-2px);
}

.el-main {
  flex: 1;
  background: linear-gradient(to bottom, #f8f9fa 0%, #e9ecef 100%);
  min-height: calc(100vh - 140px);
  padding: 0;
}

/* 页面切换动画 */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease, transform 0.3s ease;
}

.fade-enter-from {
  opacity: 0;
  transform: translateY(10px);
}

.fade-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

/* Footer样式 */
.footer {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  text-align: center;
  padding: 30px 20px;
  margin-top: auto;
}

.footer-content p {
  margin: 8px 0;
  font-size: 14px;
}

.footer-links {
  opacity: 0.9;
}

.footer-links a {
  color: white;
  text-decoration: none;
  transition: opacity 0.3s;
}

.footer-links a:hover {
  opacity: 0.7;
  text-decoration: underline;
}

.footer-links .divider {
  margin: 0 10px;
  opacity: 0.5;
}

/* 自定义滚动条 */
::-webkit-scrollbar {
  width: 10px;
}

::-webkit-scrollbar-track {
  background: #f1f1f1;
}

::-webkit-scrollbar-thumb {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 5px;
}

::-webkit-scrollbar-thumb:hover {
  background: linear-gradient(135deg, #764ba2 0%, #667eea 100%);
}
</style>
