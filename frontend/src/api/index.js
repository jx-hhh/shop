import request from '@/utils/request'

// 用户注册
export function register(data) {
  return request({
    url: '/auth/register',
    method: 'post',
    data
  })
}

// 用户登录
export function login(data) {
  return request({
    url: '/auth/login',
    method: 'post',
    data
  })
}

// 刷新令牌
export function refreshToken(refreshToken) {
  return request({
    url: '/auth/refresh',
    method: 'post',
    data: { refreshToken }
  })
}

// 获取图书列表（搜索）
export function searchBooks(params) {
  return request({
    url: '/books/search',
    method: 'get',
    params
  })
}

// 获取图书详情
export function getBookById(id) {
  return request({
    url: `/books/${id}`,
    method: 'get'
  })
}

// 创建图书（管理员）
export function createBook(data) {
  return request({
    url: '/books',
    method: 'post',
    data
  })
}

// 更新图书（管理员）
export function updateBook(id, data) {
  return request({
    url: `/books/${id}`,
    method: 'put',
    data
  })
}

// 删除图书（管理员）
export function deleteBook(id) {
  return request({
    url: `/books/${id}`,
    method: 'delete'
  })
}

// 获取我的购物车
export function getMyBasket() {
  return request({
    url: '/basket',
    method: 'get'
  })
}

// 添加商品到购物车
export function addToBasket(data) {
  return request({
    url: '/basket/items',
    method: 'post',
    data
  })
}

// 更新购物车商品数量
export function updateBasketItem(itemId, data) {
  return request({
    url: `/basket/items/${itemId}`,
    method: 'put',
    data
  })
}

// 从购物车移除商品
export function removeBasketItem(itemId) {
  return request({
    url: `/basket/items/${itemId}`,
    method: 'delete'
  })
}

// 获取我的订单列表
export function getMyOrders(params) {
  return request({
    url: '/orders/my-orders',
    method: 'get',
    params
  })
}

// 获取订单详情
export function getOrderById(id) {
  return request({
    url: `/orders/${id}`,
    method: 'get'
  })
}

// 创建订单
export function createOrder(data) {
  return request({
    url: '/orders',
    method: 'post',
    data
  })
}

// 更新订单状态（管理员）
export function updateOrderStatus(id, data) {
  return request({
    url: `/orders/${id}/status`,
    method: 'put',
    data
  })
}
