import type { IN8nService } from './n8n.service.interface';
import type { ApiClientMethods, ApiError } from '@/plugins/axios';
import { ok, err, type Result } from '@/types'; // Import ok, err, and Result
import type { GenerateWebhookJwtResponse } from '@/types'; // NEW: Import from types folder

export class ApiN8nService implements IN8nService {
    private currentToken: string | null = null;
    private tokenExpiresAt: Date | null = null;
    private tokenRefreshPromise: Promise<Result<string, ApiError>> | null = null; // To prevent multiple simultaneous refresh calls

    constructor(private apiClient: ApiClientMethods) {}

    /**
     * Lấy JWT token để xác thực với các webhook n8n.
     * Token sẽ được làm mới tự động nếu gần hết hạn.
     * @param subject Chủ đề (subject) của JWT, thường là ID người dùng hoặc ID webhook.
     * @returns Promise chứa Result của chuỗi token hoặc ApiError.
     */
    public async getWebhookJwt(subject: string): Promise<Result<string, ApiError>> {
        // Nếu token còn hiệu lực và chưa sắp hết hạn, trả về token hiện tại
        if (this.currentToken && this.tokenExpiresAt && this.tokenExpiresAt > new Date(Date.now() + 5 * 60 * 1000)) { // Hết hạn trong 5 phút tới
            return ok(this.currentToken);
        }

        // Nếu đã có lời gọi làm mới token đang chờ, trả về lời hứa đó
        if (this.tokenRefreshPromise) {
            return this.tokenRefreshPromise;
        }

        // Nếu không, tạo một lời gọi làm mới token mới
        this.tokenRefreshPromise = this.fetchNewJwt(subject);
        const result = await this.tokenRefreshPromise;
        this.tokenRefreshPromise = null; // Đặt lại lời hứa sau khi hoàn thành

        if (result.ok) {
            this.currentToken = result.value;
            // Giả sử tokenExpiresAt được thiết lập bởi fetchNewJwt
            return ok(this.currentToken);
        } else {
            return err(result.error);
        }
    }

    /**
     * Gọi API backend để lấy JWT token mới.
     * @param subject Chủ đề (subject) của JWT.
     * @returns Promise chứa Result của chuỗi token hoặc ApiError.
     */
    private async fetchNewJwt(subject: string): Promise<Result<string, ApiError>> {
        try {
            const response = await this.apiClient.post<GenerateWebhookJwtResponse>(
                `/n8n/generate-webhook-jwt`,
                { subject, expiresInMinutes: 60 } // Yêu cầu token có hiệu lực trong 60 phút
            );

            if (response.ok) {
                if (response.value) {
                    this.currentToken = response.value.token;
                    this.tokenExpiresAt = new Date(response.value.expiresAt);
                    return ok(response.value.token);
                } else {
                    return err({ name: 'N8nServiceError', message: 'API trả về thành công nhưng không có token.' });
                }
            } else {
                // response.ok is false, so response.error should be defined
                return err(response.error as ApiError);
            }
        } catch (error: any) {
            // Xử lý các lỗi mạng hoặc lỗi không xác định
            return err({ name: 'NetworkError', message: error.message || 'Lỗi mạng khi lấy JWT token.' });
        }
    }
}
