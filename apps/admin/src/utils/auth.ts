// apps/admin/src/utils/auth.ts

import type { MenuItem } from '@/data/menuItems';

/**
 * Kiểm tra xem người dùng có quyền truy cập vào một mục menu cụ thể hay không.
 * @param menuItem Mục menu cần kiểm tra.
 * @param userRoles Mảng các vai trò của người dùng hiện tại.
 * @returns `true` nếu người dùng có quyền truy cập, `false` nếu ngược lại.
 */
export function hasPermissionToMenuItem(menuItem: MenuItem, userRoles: string[]): boolean {
  // Nếu mục menu không yêu cầu vai trò cụ thể, thì mọi người đều có quyền truy cập.
  if (!menuItem.roles || menuItem.roles.length === 0) {
    return true;
  }

  // Kiểm tra xem người dùng có ít nhất một trong các vai trò được yêu cầu bởi mục menu hay không.
  return menuItem.roles.some(requiredRole => userRoles.includes(requiredRole));
}
