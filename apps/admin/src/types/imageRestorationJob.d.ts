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

export interface CreateImageRestorationJobCommand {
  originalImageUrl: string;
  familyId: string;
}

export interface UpdateImageRestorationJobCommand {
  jobId: string;
  id?: string; // Alias for jobId to satisfy ICrudService
  status?: 'Processing' | 'Completed' | 'Failed';
  errorMessage?: string;
  restoredImageUrl?: string;
}