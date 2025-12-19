import type { UserProfile } from '@/types/user.d';
import type { IFamilyAccess } from '@/types/family.d';
import { FamilyRole } from '@/types/enums';

/**
 * Checks if a user has any of the specified roles.
 * @param userProfile The user's profile object.
 * @param roles The role(s) to check against.
 * @returns True if the user has at least one of the specified roles, false otherwise.
 */
export const checkUserHasRole = (userProfile: UserProfile | null, roles: string | string[]): boolean => {
  if (!userProfile?.roles) {
    return false;
  }
  const rolesToCheck = Array.isArray(roles) ? roles : [roles];
  return rolesToCheck.some(role => userProfile.roles?.includes(role));
};

/**
 * Checks if a user has a specific role within a family.
 * @param userFamilyAccess The list of family access objects for the user.
 * @param familyId The ID of the family to check.
 * @param role The family role to check (e.g., Manager).
 * @returns True if the user has the specified role in the given family, false otherwise.
 */
export const checkUserHasFamilyRole = (
  userFamilyAccess: IFamilyAccess[],
  familyId: string,
  role: FamilyRole
): boolean => {
  if (!userFamilyAccess || userFamilyAccess.length === 0) {
    return false;
  }
  return userFamilyAccess.some(
    access => access.familyId === familyId && access.role === role
  );
};
