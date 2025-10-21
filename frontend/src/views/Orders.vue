<template>
  <div class="orders-container">
    <el-card>
      <template #header>
        <h2>我的订单</h2>
      </template>

      <el-form :inline="true" :model="queryForm" class="filter-form">
        <el-form-item label="订单状态">
          <el-select v-model="queryForm.status" placeholder="全部" clearable>
            <el-option label="待处理" value="Pending" />
            <el-option label="处理中" value="Processing" />
            <el-option label="已发货" value="Shipped" />
            <el-option label="已送达" value="Delivered" />
            <el-option label="已取消" value="Cancelled" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleQuery" :loading="loading">查询</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>

      <div v-loading="loading">
        <div v-if="orders.length > 0">
          <el-card
            v-for="order in orders"
            :key="order.id"
            class="order-card"
            shadow="hover"
          >
            <div class="order-header">
              <div>
                <span class="order-number">订单号：{{ order.orderNumber }}</span>
                <el-tag :type="getStatusType(order.status)" style="margin-left: 15px">
                  {{ getStatusText(order.status) }}
                </el-tag>
              </div>
              <span class="order-time">{{ formatTimestamp(order.createdAt) }}</span>
            </div>

            <el-divider />

            <div class="order-items">
              <div v-for="item in order.items" :key="item.id" class="order-item">
                <span>{{ item.bookTitle }}</span>
                <span>x{{ item.quantity }}</span>
                <span>¥{{ item.price.toFixed(2) }}</span>
              </div>
            </div>

            <el-divider />

            <div class="order-footer">
              <div class="order-info">
                <p><strong>收货地址：</strong>{{ order.shippingAddress }}</p>
                <p class="order-total"><strong>订单总额：</strong>¥{{ order.totalAmount.toFixed(2) }}</p>
              </div>
              <div class="order-actions">
                <el-button type="primary" size="small" @click="viewOrder(order.id)">
                  查看详情
                </el-button>
              </div>
            </div>
          </el-card>

          <el-pagination
            v-if="total > 0"
            v-model:current-page="queryForm.pageNumber"
            v-model:page-size="queryForm.pageSize"
            :total="total"
            layout="total, prev, pager, next"
            @current-change="handlePageChange"
            style="margin-top: 20px; text-align: center"
          />
        </div>

        <el-empty v-else description="还没有订单" />
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getMyOrders } from '@/api'
import { ElMessage } from 'element-plus'
import { formatTimestamp } from '@/utils/timestamp'

const router = useRouter()
const loading = ref(false)
const orders = ref([])
const total = ref(0)

const queryForm = reactive({
  status: '',
  pageNumber: 1,
  pageSize: 10
})

const loadOrders = async () => {
  loading.value = true
  try {
    const response = await getMyOrders(queryForm)
    if (response.success) {
      orders.value = response.data.items
      total.value = response.data.totalCount
    }
  } catch (error) {
    ElMessage.error('加载订单列表失败')
  } finally {
    loading.value = false
  }
}

const handleQuery = () => {
  queryForm.pageNumber = 1
  loadOrders()
}

const handleReset = () => {
  queryForm.status = ''
  queryForm.pageNumber = 1
  loadOrders()
}

const handlePageChange = () => {
  loadOrders()
}

const viewOrder = (orderId) => {
  router.push(`/orders/${orderId}`)
}

const getStatusType = (status) => {
  const map = {
    Pending: 'warning',
    Processing: 'info',
    Shipped: 'primary',
    Delivered: 'success',
    Cancelled: 'danger'
  }
  return map[status] || 'info'
}

const getStatusText = (status) => {
  const map = {
    Pending: '待处理',
    Processing: '处理中',
    Shipped: '已发货',
    Delivered: '已送达',
    Cancelled: '已取消'
  }
  return map[status] || status
}

onMounted(() => {
  loadOrders()
})
</script>

<style scoped>
.orders-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.orders-container h2 {
  margin: 0;
}

.filter-form {
  margin-bottom: 20px;
}

.order-card {
  margin-bottom: 20px;
}

.order-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.order-number {
  font-weight: bold;
  font-size: 16px;
}

.order-time {
  color: #909399;
  font-size: 14px;
}

.order-items {
  padding: 10px 0;
}

.order-item {
  display: flex;
  justify-content: space-between;
  padding: 8px 0;
  font-size: 14px;
}

.order-footer {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
}

.order-info p {
  margin: 5px 0;
  font-size: 14px;
}

.order-total {
  color: #f56c6c;
  font-size: 18px;
  font-weight: bold;
}
</style>
