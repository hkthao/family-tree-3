// apps/admin/src/types/apiError.d.ts

export interface ApiError {
  name: string;
  message: string;
  statusCode?: number;
  details?: any;
}