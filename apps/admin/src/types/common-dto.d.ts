// apps/admin/src/types/common-dto.d.ts

export interface BaseAuditableDto {
  created: string; // DateTime in C# maps to string in TypeScript
  createdBy?: string;
  lastModified?: string;
  lastModifiedBy?: string;
}
