// apps/admin/src/types/public-dashboard.d.ts

export interface PublicDashboardDto {
  totalPublicFamilies: number;
  totalPublicMembers: number;
  totalPublicRelationships: number;
  totalPublicGenerations: number;
  publicMaleRatio: number;
  publicFemaleRatio: number;
  publicLivingMembersCount: number;
  publicDeceasedMembersCount: number;
  publicAverageAge: number;
  publicMembersPerGeneration: { [key: number]: number };
  totalPublicEvents: number;
}
