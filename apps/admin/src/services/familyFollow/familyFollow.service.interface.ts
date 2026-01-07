import type { Result, ApiError } from '@/types';
import type { FamilyFollowDto, UpdateFamilyFollowSettingsCommand, FollowFamilyCommand } from '@/types/familyFollow.d';

export interface IFamilyFollowService {
  /**
   * Lấy trạng thái theo dõi của một gia đình.
   * @param familyId ID của gia đình.
   * @returns Trạng thái theo dõi của gia đình.
   */
  getFollowStatus(familyId: string): Promise<Result<FamilyFollowDto, ApiError>>;

  /**
   * Theo dõi một gia đình.
   * @param command Dữ liệu lệnh theo dõi gia đình.
   * @returns Kết quả của thao tác theo dõi gia đình.
   */
  followFamily(command: FollowFamilyCommand): Promise<Result<string, ApiError>>;

  /**
   * Hủy theo dõi một gia đình.
   * @param familyId ID của gia đình cần hủy theo dõi.
   * @returns Kết quả của thao tác hủy theo dõi gia đình.
   */
  unfollowFamily(familyId: string): Promise<Result<boolean, ApiError>>;

  /**
   * Cập nhật cài đặt theo dõi gia đình.
   * @param familyId ID của gia đình.
   * @param command Dữ liệu lệnh cập nhật cài đặt.
   * @returns Kết quả của thao tác cập nhật cài đặt.
   */
  updateFamilyFollowSettings(
    familyId: string,
    command: UpdateFamilyFollowSettingsCommand,
  ): Promise<Result<boolean, ApiError>>;
}