<template>
  <div class="home-container">
    <el-card class="search-card">
      <el-form :inline="true" :model="searchForm">
        <el-form-item label="关键词">
          <el-input v-model="searchForm.keyword" placeholder="搜索书名、作者、ISBN" clearable />
        </el-form-item>
        <el-form-item label="分类">
          <el-select v-model="searchForm.category" placeholder="请选择分类" clearable>
            <el-option label="编程" value="Programming" />
            <el-option label="计算机科学" value="Computer Science" />
            <el-option label="软件架构" value="Software Architecture" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch" :loading="loading">搜索</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <div class="books-grid" v-loading="loading">
      <el-card v-for="book in books" :key="book.id" class="book-card" shadow="hover">
        <img :src="book.imageUrl || '/placeholder.jpg'" alt="" class="book-image" />
        <div class="book-info">
          <h3>{{ book.title }}</h3>
          <p class="book-author">作者：{{ book.author }}</p>
          <p class="book-price">¥{{ book.price.toFixed(2) }}</p>
          <p class="book-stock">库存：{{ book.stock }}</p>
          <el-button type="primary" @click="addToBasket(book)" :disabled="book.stock === 0">
            <el-icon><ShoppingCart /></el-icon>
            加入购物车
          </el-button>
        </div>
      </el-card>
    </div>

    <el-pagination
      v-if="total > 0"
      v-model:current-page="searchForm.pageNumber"
      v-model:page-size="searchForm.pageSize"
      :total="total"
      layout="total, prev, pager, next"
      @current-change="handlePageChange"
      style="margin-top: 20px; text-align: center"
    />
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

onMounted(() => {
  loadBooks()
})
</script>

<style scoped>
.home-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 20px;
}

.search-card {
  margin-bottom: 20px;
}

.books-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  gap: 20px;
  min-height: 400px;
}

.book-card {
  transition: transform 0.2s;
}

.book-card:hover {
  transform: translateY(-5px);
}

.book-image {
  width: 100%;
  height: 300px;
  object-fit: cover;
  border-radius: 4px;
}

.book-info {
  padding: 10px 0;
}

.book-info h3 {
  margin: 10px 0;
  font-size: 16px;
  height: 40px;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

.book-author {
  color: #666;
  font-size: 14px;
  margin: 5px 0;
}

.book-price {
  color: #f56c6c;
  font-size: 20px;
  font-weight: bold;
  margin: 10px 0;
}

.book-stock {
  color: #909399;
  font-size: 12px;
  margin: 5px 0;
}
</style>
