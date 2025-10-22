<template>
  <div class="home-container">
    <!-- Hero Banner -->
    <div class="hero-banner">
      <div class="hero-content">
        <h1 class="hero-title">发现好书，享受阅读</h1>
        <p class="hero-subtitle">海量图书任您挑选，品质阅读从这里开始</p>
      </div>
    </div>

    <!-- 搜索区域 -->
    <div class="search-section">
      <el-card class="search-card" shadow="never">
        <el-form :inline="true" :model="searchForm" class="search-form">
          <el-form-item>
            <el-input
              v-model="searchForm.keyword"
              placeholder="搜索书名、作者、ISBN"
              clearable
              size="large"
              class="search-input"
              prefix-icon="Search"
            />
          </el-form-item>
          <el-form-item>
            <el-select
              v-model="searchForm.category"
              placeholder="请选择分类"
              clearable
              size="large"
              class="search-select"
            >
              <el-option label="全部分类" value="" />
              <el-option label="编程" value="Programming" />
              <el-option label="计算机科学" value="Computer Science" />
              <el-option label="软件架构" value="Software Architecture" />
            </el-select>
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="handleSearch" :loading="loading" size="large">
              <el-icon><Search /></el-icon>
              搜索
            </el-button>
            <el-button @click="handleReset" size="large">重置</el-button>
          </el-form-item>
        </el-form>
      </el-card>
    </div>

    <!-- 图书网格 -->
    <div class="books-section">
      <div v-if="loading" class="loading-container">
        <el-icon class="is-loading" :size="60"><Loading /></el-icon>
        <p>加载中...</p>
      </div>

      <div v-else-if="books.length === 0" class="empty-container">
        <el-empty description="暂无图书" />
      </div>

      <div v-else class="books-grid">
        <div v-for="book in books" :key="book.id" class="book-card-wrapper">
          <div class="book-card" @click="$router.push(`/books/${book.id}`)">
            <div class="book-image-container">
              <img
                :src="book.imageUrl || '/placeholder.svg'"
                :alt="book.title"
                class="book-image"
                loading="lazy"
                @error="handleImageError"
              />
              <div class="book-overlay">
                <el-button type="primary" circle size="large">
                  <el-icon><View /></el-icon>
                </el-button>
              </div>
              <div v-if="book.stock === 0" class="out-of-stock-badge">
                售罄
              </div>
              <div v-else-if="book.stock < 10" class="low-stock-badge">
                仅剩{{ book.stock }}本
              </div>
            </div>
            <div class="book-info">
              <h3 class="book-title" :title="book.title">{{ book.title }}</h3>
              <p class="book-author">
                <el-icon><User /></el-icon>
                {{ book.author }}
              </p>
              <div class="book-footer">
                <div class="book-price-section">
                  <span class="book-price">¥{{ book.price.toFixed(2) }}</span>
                </div>
                <el-button
                  type="primary"
                  size="small"
                  @click.stop="addToBasket(book)"
                  :disabled="book.stock === 0"
                  class="add-to-cart-btn"
                >
                  <el-icon><ShoppingCart /></el-icon>
                </el-button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 分页 -->
    <div v-if="total > 0" class="pagination-section">
      <el-pagination
        v-model:current-page="searchForm.pageNumber"
        v-model:page-size="searchForm.pageSize"
        :total="total"
        :page-sizes="[12, 24, 48]"
        layout="total, sizes, prev, pager, next, jumper"
        @current-change="handlePageChange"
        @size-change="handlePageChange"
        background
      />
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { searchBooks, addToBasket as addToBasketApi } from '@/api'
import { ElMessage } from 'element-plus'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const loading = ref(false)
const books = ref([])
const total = ref(0)

const searchForm = reactive({
  keyword: '',
  category: '',
  pageNumber: 1,
  pageSize: 12
})

const loadBooks = async () => {
  loading.value = true
  try {
    const response = await searchBooks(searchForm)
    if (response.success) {
      books.value = response.data.items
      total.value = response.data.totalCount
    }
  } catch (error) {
    ElMessage.error('加载图书列表失败')
  } finally {
    loading.value = false
  }
}

const handleSearch = () => {
  searchForm.pageNumber = 1
  loadBooks()
}

const handleReset = () => {
  searchForm.keyword = ''
  searchForm.category = ''
  searchForm.pageNumber = 1
  loadBooks()
}

const handlePageChange = () => {
  loadBooks()
}

const addToBasket = async (book) => {
  if (!authStore.isAuthenticated) {
    ElMessage.warning('请先登录')
    return
  }

  try {
    await addToBasketApi({ bookId: book.id, quantity: 1 })
    ElMessage.success('已加入购物车')
  } catch (error) {
    ElMessage.error('加入购物车失败')
  }
}

const handleImageError = (e) => {
  e.target.src = '/placeholder.svg'
}

onMounted(() => {
  loadBooks()
})
</script>

<style scoped>
.home-container {
  min-height: 100vh;
}

/* Hero Banner */
.hero-banner {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 80px 20px;
  text-align: center;
  color: white;
  position: relative;
  overflow: hidden;
}

.hero-banner::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: url('data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1440 320"><path fill="rgba(255,255,255,0.1)" d="M0,96L48,112C96,128,192,160,288,160C384,160,480,128,576,122.7C672,117,768,139,864,138.7C960,139,1056,117,1152,106.7C1248,96,1344,96,1392,96L1440,96L1440,320L1392,320C1344,320,1248,320,1152,320C1056,320,960,320,864,320C768,320,672,320,576,320C480,320,384,320,288,320C192,320,96,320,48,320L0,320Z"></path></svg>');
  background-size: cover;
  background-position: bottom;
}

.hero-content {
  position: relative;
  z-index: 1;
  max-width: 800px;
  margin: 0 auto;
}

.hero-title {
  font-size: 48px;
  font-weight: 700;
  margin-bottom: 20px;
  text-shadow: 0 2px 10px rgba(0, 0, 0, 0.2);
  animation: fadeInUp 0.8s ease;
}

.hero-subtitle {
  font-size: 20px;
  opacity: 0.95;
  animation: fadeInUp 0.8s ease 0.2s backwards;
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* 搜索区域 */
.search-section {
  max-width: 1400px;
  margin: -30px auto 0;
  padding: 0 20px;
  position: relative;
  z-index: 2;
}

.search-card {
  border-radius: 16px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
  border: none;
}

.search-form {
  display: flex;
  flex-wrap: wrap;
  gap: 15px;
  justify-content: center;
  align-items: center;
}

.search-input {
  min-width: 300px;
}

.search-select {
  min-width: 200px;
}

/* 图书区域 */
.books-section {
  max-width: 1400px;
  margin: 40px auto;
  padding: 0 20px;
  min-height: 500px;
}

.loading-container,
.empty-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 400px;
  color: #909399;
}

.loading-container p {
  margin-top: 20px;
  font-size: 16px;
}

/* 图书网格 */
.books-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 30px;
  animation: fadeIn 0.6s ease;
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

.book-card-wrapper {
  animation: slideUp 0.6s ease backwards;
}

.book-card-wrapper:nth-child(1) { animation-delay: 0.1s; }
.book-card-wrapper:nth-child(2) { animation-delay: 0.15s; }
.book-card-wrapper:nth-child(3) { animation-delay: 0.2s; }
.book-card-wrapper:nth-child(4) { animation-delay: 0.25s; }

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.book-card {
  background: white;
  border-radius: 16px;
  overflow: hidden;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  height: 100%;
  display: flex;
  flex-direction: column;
}

.book-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 12px 24px rgba(102, 126, 234, 0.2);
}

/* 图片容器 */
.book-image-container {
  position: relative;
  width: 100%;
  padding-top: 140%; /* 7:10 比例 */
  overflow: hidden;
  background: linear-gradient(135deg, #f5f7fa 0%, #e9ecef 100%);
}

.book-image {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.4s ease;
}

.book-card:hover .book-image {
  transform: scale(1.05);
}

/* 悬浮覆盖层 */
.book-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(102, 126, 234, 0.9);
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition: opacity 0.3s ease;
}

.book-card:hover .book-overlay {
  opacity: 1;
}

/* 库存标签 */
.out-of-stock-badge,
.low-stock-badge {
  position: absolute;
  top: 15px;
  right: 15px;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  color: white;
  z-index: 1;
}

.out-of-stock-badge {
  background: linear-gradient(135deg, #f56c6c 0%, #e74c3c 100%);
}

.low-stock-badge {
  background: linear-gradient(135deg, #f9a825 0%, #f57f17 100%);
}

/* 图书信息 */
.book-info {
  padding: 20px;
  flex: 1;
  display: flex;
  flex-direction: column;
}

.book-title {
  font-size: 16px;
  font-weight: 600;
  color: #333;
  margin: 0 0 12px 0;
  height: 44px;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  line-height: 1.4;
  transition: color 0.3s;
}

.book-card:hover .book-title {
  color: #667eea;
}

.book-author {
  color: #909399;
  font-size: 14px;
  margin: 0 0 auto 0;
  display: flex;
  align-items: center;
  gap: 6px;
}

.book-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid #ebeef5;
}

.book-price-section {
  display: flex;
  flex-direction: column;
}

.book-price {
  font-size: 24px;
  font-weight: 700;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.add-to-cart-btn {
  border-radius: 50%;
  width: 40px;
  height: 40px;
  padding: 0;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* 分页 */
.pagination-section {
  max-width: 1400px;
  margin: 40px auto;
  padding: 0 20px;
  display: flex;
  justify-content: center;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .hero-title {
    font-size: 32px;
  }

  .hero-subtitle {
    font-size: 16px;
  }

  .search-input,
  .search-select {
    min-width: 100%;
  }

  .books-grid {
    grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
    gap: 20px;
  }

  .book-title {
    font-size: 14px;
  }

  .book-price {
    font-size: 20px;
  }
}
</style>
