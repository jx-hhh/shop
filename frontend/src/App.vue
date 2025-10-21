<template>
  <div id="app">
    <el-container>
      <el-header height="60px">
        <div class="header-content">
          <h2 @click="$router.push('/')" style="cursor: pointer">📚 在线书店</h2>
          <div class="header-actions">
            <template v-if="authStore.isAuthenticated">
              <el-button @click="$router.push('/basket')" link>
                <el-icon><ShoppingCart /></el-icon>
                购物车
              </el-button>
              <el-button @click="$router.push('/orders')" link>
                <el-icon><List /></el-icon>
                我的订单
              </el-button>
              <el-dropdown>
                <span class="user-dropdown">
                  <el-icon><User /></el-icon>
                  {{ authStore.username }}
                </span>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item @click="authStore.logout()">
                      退出登录
                    </el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </template>
            <template v-else>
              <el-button @click="$router.push('/login')" type="primary">登录</el-button>
              <el-button @click="$router.push('/register')">注册</el-button>
            </template>
          </div>
        </div>
      </el-header>
      <el-main>
        <router-view />
      </el-main>
    </el-container>
  </div>
</template>

<script setup>
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
</script>

<style scoped>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

.el-header {
  background-color: #409eff;
  color: #fff;
  display: flex;
  align-items: center;
}

.header-content {
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header-content h2 {
  margin: 0;
  color: #fff;
}

.header-actions {
  display: flex;
  gap: 15px;
  align-items: center;
}

.header-actions .el-button {
  color: #fff;
}

.user-dropdown {
  color: #fff;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 5px;
}

.el-main {
  background-color: #f5f5f5;
  min-height: calc(100vh - 60px);
}
</style>
