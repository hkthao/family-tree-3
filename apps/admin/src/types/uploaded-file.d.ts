// apps/admin/src/types/uploaded-file.d.ts

/**
 * Interface đại diện cho thông tin chi tiết của một tệp đã tải lên.
 */
export interface IUploadedFile {
  url: string;
  display_url: string;
  width?: number;
  height?: number;
  size?: number;
  id: string;
  name: string;
  filename: string;
  extension: string;
  content_type: string;
}
