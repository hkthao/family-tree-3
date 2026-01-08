import type { ApiClientMethods } from '@/plugins/axios';
import type { IFamilyFollowService } from './familyFollow.service.interface';
import type { Result, ApiError } from '@/types';
import type { FamilyFollowDto, UpdateFamilyFollowSettingsCommand, FollowFamilyCommand } from '@/types/familyFollow';

export class ApiFamilyFollowService implements IFamilyFollowService {
  constructor(private apiClient: ApiClientMethods) {}

  /**
   * Lấy trạng thái theo dõi của một gia đình.
   * @param familyId ID của gia đình.
   * @returns Trạng thái theo dõi của gia đình.
   */
  async getFollowStatus(familyId: string): Promise<Result<FamilyFollowDto, ApiError>> {
    try {
      const data: Result<FamilyFollowDto, ApiError> = await this.apiClient.get<FamilyFollowDto>(
        `/family-follows/${familyId}/status`,
      );
      return data;
    } catch (error: any) {
      return {
        ok: false,
        error: {
          name: 'FamilyFollowStatusError',
          message: error.response?.data?.error?.message || 'Không thể lấy trạng thái theo dõi gia đình.',
        },
      };
    }
  }

  /**
   * Theo dõi một gia đình.
   * @param command Dữ liệu lệnh theo dõi gia đình.
   * @returns Kết quả của thao tác theo dõi gia đình.
   */
  async followFamily(command: FollowFamilyCommand): Promise<Result<string, ApiError>> {
    try {
      const data: Result<string, ApiError> = await this.apiClient.post<string>(`/family-follows`, command);
      return data;
    } catch (error: any) {
      return {
        ok: false,
        error: {
          name: 'FollowFamilyError',
          message: error.response?.data?.error?.message || 'Không thể theo dõi gia đình.',
        },
      };
    }
  }

  /**
   * Hủy theo dõi một gia đình.
   * @param familyId ID của gia đình cần hủy theo dõi.
   * @returns Kết quả của thao tác hủy theo dõi gia đình.
   */
  async unfollowFamily(familyId: string): Promise<Result<boolean, ApiError>> {
    try {
      const data: Result<boolean, ApiError> = await this.apiClient.delete<boolean>(`/family-follows/${familyId}`);
      return data;
    } catch (error: any) {
      return {
        ok: false,
        error: {
          name: 'UnfollowFamilyError',
          message: error.response?.data?.error?.message || 'Không thể hủy theo dõi gia đình.',
        },
      };
    }
  }

  /**
   * Cập nhật cài đặt theo dõi gia đình.
   * @param familyId ID của gia đình.
   * @param command Dữ liệu lệnh cập nhật cài đặt.
   * @returns Kết quả của thao tác cập nhật cài đặt.
   */
  async updateFamilyFollowSettings(
    familyId: string,
    command: UpdateFamilyFollowSettingsCommand,
  ): Promise<Result<boolean, ApiError>> {
    try {
      const data: Result<boolean, ApiError> = await this.apiClient.put<boolean>(
        `/family-follows/${familyId}/preferences`,
        command,
      );
      return data;
    } catch (error: any) {
      return {
        ok: false,
        error: {
          name: 'UpdateFamilyFollowSettingsError',
          message: error.response?.data?.error?.message || 'Không thể cập nhật cài đặt theo dõi gia đình.',
        },
      };
    }
  }
}
