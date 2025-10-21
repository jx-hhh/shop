<template>
  <div class="order-detail-container">
    <el-card v-loading="loading">
      <template #header>
        <div class="header-content">
          <h2>订单详情</h2>
          <el-button @click="$router.back()" icon="ArrowLeft">返回</el-button>
        </div>
      </template>

      <div v-if="order">
        <el-descriptions title="订单信息" :column="2" border>
          <el-descriptions-item label="订单号">{{ order.orderNumber }}</el-descriptions-item>
          <el-descriptions-item label="订单状态">
            <el-tag :type="getStatusType(order.status)">
              {{ getStatusText(order.status) }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="下单时间">
            {{ formatTimestamp(order.createdAt) }}
          </el-descriptions-item>
          <el-descriptions-item label="更新时间">
            {{ order.updatedAt ? formatTimestamp(order.updatedAt) : '-' }}
          </el-descriptions-item>
          <el-descriptions-item label="收货地址" :span="2">
            {{ order.shippingAddress }}
          </el-descriptions-item>
        </el-descriptions>

        <el-divider />

        <h3>商品清单</h3>
        <el-table :data="order.items" style="width: 100%; margin-top: 15px">
          <el-table-column prop="bookTitle" label="书名" />
          <el-table-column prop="bookAuthor" label="作者" width="200" />
          <el-table-column prop="price" label="单价" width="120">
            <template #default="scope">
              ¥{{ scope.row.price.toFixed(2) }}
            </template>
          </el-table-column>
          <el-table-column prop="quantity" label="数量" width="100" />
          <el-table-column label="小计" width="120">
            <template #default="scope">
              ¥{{ scope.row.subtotal.toFixed(2) }}
            </template>
          </el-table-column>
        </el-table>

        <div class="order-summary">
          <el-statistic title="订单总额" :value="order.totalAmount" :precision="2" prefix="¥" />
        </div>
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { getOrderById } from '@/api'
import { ElMessage } from 'element-plus'
import { formatTimestamp } from '@/utils/timestamp'

const route = useRoute()
const loading = ref(false)
const order = ref(null)

const loadOrder = async () => {
  loading.value = true
  try {
    const response = await getOrderById(route.params.id)
    if (response.success) {
      order.value = response.data
    }
  } catch (error) {
    ElMessage.error('加载订单详情失败')
  } finally {
    loading.value = false
  }
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
  loadOrder()
})
</script>

<style scoped>
.order-detail-container {
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

.order-summary {
  margin-top: 30px;
  text-align: right;
  padding: 20px;
  background-color: #f5f7fa;
  border-radius: 4px;
}
</style>
