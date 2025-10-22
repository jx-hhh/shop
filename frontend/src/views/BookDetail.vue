<template>
  <div class="book-detail-container">
    <el-card v-loading="loading">
      <template #header>
        <div class="header-content">
          <h2>图书详情</h2>
          <el-button @click="$router.back()" icon="ArrowLeft">返回</el-button>
        </div>
      </template>

      <div v-if="book" class="book-content">
        <div class="book-image-section">
          <img :src="book.imageUrl || '/placeholder.svg'" :alt="book.title" class="book-detail-image" @error="handleImageError" />
        </div>

        <div class="book-info-section">
          <h1>{{ book.title }}</h1>

          <el-descriptions :column="1" border style="margin: 20px 0">
            <el-descriptions-item label="作者">{{ book.author }}</el-descriptions-item>
            <el-descriptions-item label="ISBN">{{ book.isbn }}</el-descriptions-item>
            <el-descriptions-item label="分类">{{ book.category }}</el-descriptions-item>
            <el-descriptions-item label="库存">{{ book.stock }} 本</el-descriptions-item>
            <el-descriptions-item label="发布时间">
              {{ formatTimestamp(book.createdAt) }}
            </el-descriptions-item>
          </el-descriptions>

          <div class="price-section">
            <span class="price-label">价格：</span>
            <span class="price-value">¥{{ book.price.toFixed(2) }}</span>
          </div>

          <div class="description-section">
            <h3>内容简介</h3>
            <p>{{ book.description }}</p>
          </div>

          <div class="actions-section">
            <el-input-number v-model="quantity" :min="1" :max="book.stock" style="margin-right: 15px" />
            <el-button
              type="primary"
              size="large"
              @click="addToBasket"
              :disabled="book.stock === 0"
              icon="ShoppingCart"
            >
              加入购物车
            </el-button>
          </div>
        </div>
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { getBookById, addToBasket as addToBasketApi } from '@/api'
import { ElMessage } from 'element-plus'
import { formatTimestamp } from '@/utils/timestamp'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const authStore = useAuthStore()
const loading = ref(false)
const book = ref(null)
const quantity = ref(1)

const loadBook = async () => {
  loading.value = true
  try {
    const response = await getBookById(route.params.id)
    if (response.success) {
      book.value = response.data
    }
  } catch (error) {
    ElMessage.error('加载图书详情失败')
  } finally {
    loading.value = false
  }
}

const addToBasket = async () => {
  if (!authStore.isAuthenticated) {
    ElMessage.warning('请先登录')
    return
  }

  try {
    await addToBasketApi({ bookId: book.value.id, quantity: quantity.value })
    ElMessage.success('已加入购物车')
  } catch (error) {
    ElMessage.error('加入购物车失败')
  }
}

const handleImageError = (e) => {
  e.target.src = '/placeholder.svg'
}

onMounted(() => {
  loadBook()
})
</script>

<style scoped>
.book-detail-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header-content h2 {
  margin: 0;
}

.book-content {
  display: grid;
  grid-template-columns: 400px 1fr;
  gap: 40px;
}

.book-detail-image {
  width: 100%;
  border-radius: 8px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
}

.book-info-section h1 {
  margin: 0 0 20px 0;
  font-size: 28px;
}

.price-section {
  margin: 30px 0;
  padding: 20px;
  background-color: #fff5f5;
  border-radius: 8px;
}

.price-label {
  font-size: 18px;
  color: #606266;
}

.price-value {
  font-size: 32px;
  font-weight: bold;
  color: #f56c6c;
  margin-left: 10px;
}

.description-section {
  margin: 30px 0;
}

.description-section h3 {
  margin-bottom: 15px;
}

.description-section p {
  line-height: 1.8;
  color: #606266;
}

.actions-section {
  margin-top: 30px;
  display: flex;
  align-items: center;
}
</style>
