import type { Result, CreateUserPushTokenCommand, UpdateUserPushTokenCommand, UserPushTokenDto } from '@/types';
import type { ICrudService } from '../common/crud.service.interface';
import type { ApiError } from '@/types/apiError';

export interface IUserPushTokenService
  extends ICrudService<
    UserPushTokenDto,
    CreateUserPushTokenCommand,
    UpdateUserPushTokenCommand
  > {
  getUserPushTokensByUserId(userId: string): Promise<Result<UserPushTokenDto[], ApiError>>;
}
