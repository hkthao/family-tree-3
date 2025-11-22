// apps/mobile/family_tree_rn/src/types/userProfile.d.ts

export interface UserProfileDto {
  id: string; // Guid in C# maps to string in TypeScript
  externalId: string;
  userId: string; // Guid in C# maps to string in TypeScript
  email: string;
  name: string;
  avatar?: string;
  roles: string[];
  firstName?: string;
  lastName?: string;
  phone?: string;
}
