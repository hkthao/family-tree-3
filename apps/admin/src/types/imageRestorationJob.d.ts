export enum RestorationStatus {
  Processing = 0,
  Completed = 1,
  Failed = 2,
}

export interface ImageRestorationJobDto {
  jobId: string;
  id?: string; // Alias for jobId to satisfy ICrudService
  originalImageUrl: string;
  userId: string;
  familyId: string;
  status: RestorationStatus;
  errorMessage?: string;
  restoredImageUrl?: string;
  created: Date;
  lastModified?: Date;
}

export interface UpdateImageRestorationJobDto {
  jobId: string;
  id?: string; // Alias for jobId to satisfy ICrudService
  originalImageUrl?: string; // Added to allow updating original image URL
  status?: RestorationStatus;
  errorMessage?: string;
  restoredImageUrl?: string;
}