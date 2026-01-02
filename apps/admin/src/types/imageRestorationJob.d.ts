export interface ImageRestorationJobDto {
  jobId: string;
  id?: string; // Alias for jobId to satisfy ICrudService
  originalImageUrl: string;
  userId: string;
  familyId: string;
  status: 'Processing' | 'Completed' | 'Failed';
  errorMessage?: string;
  restoredImageUrl?: string;
  created: Date;
  lastModified?: Date;
}

export interface CreateImageRestorationJobDto {
  originalImageUrl: string;
  familyId: string;
}

export interface UpdateImageRestorationJobDto {
  jobId: string;
  id?: string; // Alias for jobId to satisfy ICrudService
  originalImageUrl?: string; // Added to allow updating original image URL
  status?: 'Processing' | 'Completed' | 'Failed';
  errorMessage?: string;
  restoredImageUrl?: string;
}