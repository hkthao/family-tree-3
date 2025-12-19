import { describe, it, expect } from 'vitest';
import { checkUserHasRole, checkUserHasFamilyRole } from '@/composables/auth/auth.logic';
import { FamilyRole } from '@/types/enums';
import type { UserProfile } from '@/types';
import type { IFamilyAccess } from '@/types/family.d';

describe('auth.logic', () => {
  describe('checkUserHasRole', () => {
    const adminUser: UserProfile = { id: '1', email: 'admin@example.com', name: 'Admin User', roles: ['Admin', 'User'] };
    const regularUser: UserProfile = { id: '2', email: 'user@example.com', name: 'Regular User', roles: ['User'] };
    const noRoleUser: UserProfile = { id: '3', email: 'norole@example.com', name: 'No Role User', roles: [] };
    const nullUser: UserProfile | null = null;
    const undefinedRolesUser: UserProfile = { id: '4', email: 'undefined@example.com', name: 'Undefined Roles User' };

    it('should return true if user has the specified single role', () => {
      expect(checkUserHasRole(adminUser, 'Admin')).toBe(true);
      expect(checkUserHasRole(regularUser, 'User')).toBe(true);
    });

    it('should return false if user does not have the specified single role', () => {
      expect(checkUserHasRole(adminUser, 'Editor')).toBe(false);
      expect(checkUserHasRole(regularUser, 'Admin')).toBe(false);
    });

    it('should return true if user has at least one of the specified multiple roles', () => {
      expect(checkUserHasRole(adminUser, ['Admin', 'Editor'])).toBe(true);
      expect(checkUserHasRole(regularUser, ['Guest', 'User'])).toBe(true);
    });

    it('should return false if user does not have any of the specified multiple roles', () => {
      expect(checkUserHasRole(adminUser, ['Guest', 'Editor'])).toBe(false);
      expect(checkUserHasRole(regularUser, ['Guest', 'Editor'])).toBe(false);
    });

    it('should return false if user has no roles', () => {
      expect(checkUserHasRole(noRoleUser, 'Admin')).toBe(false);
      expect(checkUserHasRole(noRoleUser, ['Admin', 'User'])).toBe(false);
    });

    it('should return false if user object is null', () => {
      expect(checkUserHasRole(nullUser, 'Admin')).toBe(false);
    });

    it('should return false if user roles are undefined', () => {
      expect(checkUserHasRole(undefinedRolesUser, 'Admin')).toBe(false);
    });
  });

  describe('checkUserHasFamilyRole', () => {
    const familyAccess: IFamilyAccess[] = [
      { familyId: 'family1', role: FamilyRole.Manager },
      { familyId: 'family2', role: FamilyRole.Member },
    ];

    it('should return true if user has the specified family role', () => {
      expect(checkUserHasFamilyRole(familyAccess, 'family1', FamilyRole.Manager)).toBe(true);
    });

    it('should return false if user does not have the specified family role', () => {
      expect(checkUserHasFamilyRole(familyAccess, 'family1', FamilyRole.Member)).toBe(false);
    });

    it('should return false if user does not have access to the family', () => {
      expect(checkUserHasFamilyRole(familyAccess, 'family3', FamilyRole.Manager)).toBe(false);
    });

    it('should return false if family access list is empty', () => {
      expect(checkUserHasFamilyRole([], 'family1', FamilyRole.Manager)).toBe(false);
    });

    it('should return false if family access list is null/undefined', () => {
      // @ts-expect-error: Intentionally testing with null/undefined for robustness
      expect(checkUserHasFamilyRole(null, 'family1', FamilyRole.Manager)).toBe(false);
      // @ts-expect-error: Intentionally testing with null/undefined for robustness
      expect(checkUserHasFamilyRole(undefined, 'family1', FamilyRole.Manager)).toBe(false);
    });
  });
});
