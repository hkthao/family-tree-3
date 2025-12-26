export interface ImageUploadResultDto {
  id?: string;
  title?: string;
  url?: string;
  deleteUrl?: string;
  mimeType?: string;
  size: number;
  width: number;
  height: number;
}