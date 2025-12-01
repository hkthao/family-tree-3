import type { Result } from '@/types';
import type { ApiError } from '@/plugins/axios';

export interface IN8nService {
    /**
     * Lấy JWT token để xác thực với các webhook n8n.
     * Token sẽ được làm mới tự động nếu gần hết hạn.
     * @param subject Chủ đề (subject) của JWT, thường là ID người dùng hoặc ID webhook.
     * @returns Promise chứa Result của chuỗi token hoặc ApiError.
     */
    getWebhookJwt(subject: string): Promise<Result<string, ApiError>>;
}
