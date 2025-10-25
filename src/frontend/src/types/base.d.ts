export interface BaseEntity {
  id?: string;
  created?: string;
  createdBy?: string;
  lastModified?: string | null;
  lastModifiedBy?: string | null;
}
