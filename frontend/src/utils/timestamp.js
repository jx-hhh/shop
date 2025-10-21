/**
 * 将秒级时间戳转换为日期时间字符串
 * @param {number} timestamp - 秒级时间戳
 * @param {string} format - 格式（默认：'YYYY-MM-DD HH:mm:ss'）
 * @returns {string} 格式化的日期时间字符串
 */
export function formatTimestamp(timestamp, format = 'YYYY-MM-DD HH:mm:ss') {
  if (!timestamp) return ''

  const date = new Date(timestamp * 1000)

  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  const hours = String(date.getHours()).padStart(2, '0')
  const minutes = String(date.getMinutes()).padStart(2, '0')
  const seconds = String(date.getSeconds()).padStart(2, '0')

  return format
    .replace('YYYY', year)
    .replace('MM', month)
    .replace('DD', day)
    .replace('HH', hours)
    .replace('mm', minutes)
    .replace('ss', seconds)
}

/**
 * 获取当前秒级时间戳
 * @returns {number} 当前秒级时间戳
 */
export function getCurrentTimestamp() {
  return Math.floor(Date.now() / 1000)
}

/**
 * Date 转换为秒级时间戳
 * @param {Date} date - Date 对象
 * @returns {number} 秒级时间戳
 */
export function dateToTimestamp(date) {
  return Math.floor(date.getTime() / 1000)
}
