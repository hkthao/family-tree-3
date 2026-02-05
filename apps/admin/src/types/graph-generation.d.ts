export interface GraphGenerationJobDto {
  jobId: string;
  familyId: string;
  rootMemberId: string;
  status: string;
  outputFilePath?: string;
  errorMessage?: string;
  requestedAt: string;
  completedAt?: string;
}
