export interface BaseAuditableEntity {
  id: string;
  created?: Date;
  createdBy?: string;
  lastModified?: Date;
  lastModifiedBy?: string;
  isDeleted?: boolean;
  deletedBy?: string;
  deletedDate?: Date;
}
