<template>
  <div class="basket-container">
    <el-card>
      <template #header>
        <h2>我的购物车</h2>
      </template>

      <div v-if="loading" style="text-align: center; padding: 50px;">
        <el-icon class="is-loading" :size="50"><Loading /></el-icon>
      </div>

      <div v-else-if="basket && basket.items && basket.items.length > 0">
        <el-table :data="basket.items" style="width: 100%">
          <el-table-column prop="bookTitle" label="书名" width="300" />
          <el-table-column prop="bookAuthor" label="作者" width="200" />
          <el-table-column prop="price" label="单价" width="120">
            <template #default="scope">
              ¥{{ scope.row.price.toFixed(2) }}
            </template>
          </el-table-column>
          <el-table-column prop="quantity" label="数量" width="180">
            <template #default="scope">
              <el-input-number
                v-model="scope.row.quantity"
                :min="1"
                :max="99"
                @change="handleQuantityChange(scope.row)"
              />
            </template>
          </el-table-column>
          <el-table-column label="小计" width="120">
            <template #default="scope">
              ¥{{ (scope.row.price * scope.row.quantity).toFixed(2) }}
            </template>
          </el-table-column>
          <el-table-column label="操作" width="120">
            <template #default="scope">
              <el-button type="danger" size="small" @click="handleRemove(scope.row.id)">
                删除
              </el-button>
            </template>
          </el-table-column>
        </el-table>

        <div class="basket-summary">
          <div class="summary-item">
            <span class="label">商品总数：</span>
            <span class="value">{{ totalItems }} 件</span>
          </div>
          <div class="summary-item total">
            <span class="label">总金额：</span>
            <span class="value">¥{{ totalPrice.toFixed(2) }}</span>
          </div>
          <el-button type="primary" size="large" @click="showCheckoutDialog = true">
            去结算
          </el-button>
        </div>
      </div>

      <el-empty v-else description="购物车是空的" />
    </el-card>

    <!-- 结算对话框 -->
    <el-dialog v-model="showCheckoutDialog" title="确认订单" width="500px">
      <el-form :model="checkoutForm" :rules="checkoutRules" ref="checkoutFormRef" label-width="100px">
        <el-form-item label="收货地址" prop="shippingAddress">
          <el-input
            v-model="checkoutForm.shippingAddress"
            type="textarea"
            :rows="3"
            placeholder="请输入收货地址"
          />
        </el-form-item>
        <el-form-item label="联系电话" prop="contactPhone">
          <el-input v-model="checkoutForm.contactPhone" placeholder="请输入联系电话" />
        </el-form-item>
        <el-form-item>
          <div class="checkout-summary">
            <p>商品总数：{{ totalItems }} 件</p>
            <p class="total-price">应付金额：¥{{ totalPrice.toFixed(2) }}</p>
          </div>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showCheckoutDialog = false">取消</el-button>
        <el-button type="primary" @click="handleCheckout" :loading="checkoutLoading">
          确认下单
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getMyBasket, updateBasketItem, removeBasketItem, createOrder } from '@/api'
import { ElMessage, ElMessageBox } from 'element-plus'

const router = useRouter()
const loading = ref(false)
const checkoutLoading = ref(false)
const showCheckoutDialog = ref(false)
const basket = ref(null)
const checkoutFormRef = ref()

const checkoutForm = reactive({
  shippingAddress: '',
  contactPhone: ''
})

const checkoutRules = {
  shippingAddress: [
    { required: true, message: '请输入收货地址', trigger: 'blur' }
  ],
  contactPhone: [
    { required: true, message: '请输入联系电话', trigger: 'blur' },
    { pattern: /^1[3-9]\d{9}$/, message: '请输入有效的手机号', trigger: 'blur' }
  ]
}

const totalItems = computed(() => {
  if (!basket.value || !basket.value.items) return 0
  return basket.value.items.reduce((sum, item) => sum + item.quantity, 0)
})

const totalPrice = computed(() => {
  if (!basket.value || !basket.value.items) return 0
  return basket.value.items.reduce((sum, item) => sum + item.price * item.quantity, 0)
})

const loadBasket = async () => {
  loading.value = true
  try {
    const response = await getMyBasket()
    if (response.success) {
      basket.value = response.data
    }
  } catch (error) {
    ElMessage.error('加载购物车失败')
  } finally {
    loading.value = false
  }
}

const handleQuantityChange = async (item) => {
  try {
    await updateBasketItem(item.id, {
      basketItemId: item.id,
      quantity: item.quantity
    })
    ElMessage.success('已更新数量')
    loadBasket()
  } catch (error) {
    ElMessage.error('更新数量失败')
  }
}

const handleRemove = async (itemId) => {
  try {
    await ElMessageBox.confirm('确定要删除这件商品吗？', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })

    await removeBasketItem(itemId)
    ElMessage.success('已删除')
    loadBasket()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('删除失败')
    }
  }
}

const handleCheckout = async () => {
  await checkoutFormRef.value.validate(async (valid) => {
    if (!valid) return

    checkoutLoading.value = true
    try {
      const response = await createOrder(checkoutForm)
      if (response.success) {
        ElMessage.success('下单成功')
        showCheckoutDialog.value = false
        router.push('/orders')
      }
    } catch (error) {
      ElMessage.error('下单失败')
    } finally {
      checkoutLoading.value = false
    }
  })
}

onMounted(() => {
  loadBasket()
})
</script>

<style scoped>
.basket-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.basket-container h2 {
  margin: 0;
}

.basket-summary {
  margin-top: 30px;
  text-align: right;
  padding: 20px;
  background-color: #f5f7fa;
  border-radius: 4px;
}

.summary-item {
  margin: 10px 0;
  font-size: 16px;
}

.summary-item.total {
  font-size: 20px;
  font-weight: bold;
  color: #f56c6c;
  margin: 20px 0;
}

.summary-item .label {
  margin-right: 10px;
}

.checkout-summary {
  width: 100%;
  padding: 15px;
  background-color: #f5f7fa;
  border-radius: 4px;
}

.checkout-summary p {
  margin: 8px 0;
}

.checkout-summary .total-price {
  font-size: 18px;
  font-weight: bold;
  color: #f56c6c;
}
</style>
