// apps/admin/src/types/privacyConfiguration.d.ts

export interface PrivacyConfiguration {
  id: string;
  familyId: string;
  publicMemberProperties: string[];
  publicEventProperties: string[];
  publicFamilyProperties: string[];
  publicFamilyLocationProperties: string[];
  publicMemoryItemProperties: string[];
  publicMemberFaceProperties: string[];
  publicFoundFaceProperties: string[];
}
