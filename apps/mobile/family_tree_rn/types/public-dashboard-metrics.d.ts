// apps/mobile/family_tree_rn/src/types/public-dashboard-metrics.d.ts

// Defined locally as it's a client-side derived metric interface
export interface DashboardMetrics {
  totalMembers: number;
  totalRelationships: number;
  totalGenerations: number;
  averageAge: number;
  livingMembers: number;
  deceasedMembers: number;
  genderDistribution: { name: string; population: number; color: string; legendFontColor: string; legendFontSize: number; }[];
  membersPerGeneration: { [key: number]: number };
  totalEvents: number;
}
