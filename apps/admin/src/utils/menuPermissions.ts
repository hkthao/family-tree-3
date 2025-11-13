/**
 * Checks if a user has permission to access a menu item.
 * @param userRoles - The roles of the current user.
 * @param itemRoles - The roles allowed to access the menu item. If undefined, it's a public item.
 * @returns True if the user can access the item, false otherwise.
 */
export function canAccessMenu(userRoles: string[], itemRoles?: string[]): boolean {
  if (!itemRoles || itemRoles.length === 0) {
    return true; // Public item
  }
  return userRoles.some(userRole => itemRoles.includes(userRole));
}