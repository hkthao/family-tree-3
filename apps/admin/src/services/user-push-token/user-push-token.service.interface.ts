import type { Result, CreateUserPushTokenCommand, UpdateUserPushTokenCommand, UserPushTokenDto, Paginated, ListOptions } from '@/types';
import type { ICrudService } from '../common/crud.service.interface';
import type { ApiError } from '@/types/apiError';

export interface IUserPushTokenService
  extends ICrudService<
    UserPushTokenDto,
    CreateUserPushTokenCommand,
    UpdateUserPushTokenCommand
  > {
  getUserPushTokensByUserId(
    userId: string,
    options: ListOptions,
  ): Promise<Result<Paginated<UserPushTokenDto>, ApiError>>;
}
