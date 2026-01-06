import type { ApiClientMethods } from '@/plugins/axios';
import type { ApiError } from '@/types/apiError';
import { ApiCrudService } from '../common/api.crud.service';
import type { IUserPushTokenService } from './user-push-token.service.interface';
import type {
  CreateUserPushTokenCommand,
  UpdateUserPushTokenCommand,
  UserPushTokenDto,
  Result,
} from '@/types';

export class ApiUserPushTokenService
  extends ApiCrudService<
    UserPushTokenDto,
    CreateUserPushTokenCommand,
    UpdateUserPushTokenCommand
  >
  implements IUserPushTokenService
{
  constructor(apiClient: ApiClientMethods) {
    super(apiClient, 'user-push-tokens'); // Base path for user push token API
  }

  getUserPushTokensByUserId(userId: string): Promise<Result<UserPushTokenDto[], ApiError>> {
    return this.api.get<UserPushTokenDto[]>(`${this.baseUrl}/user/${userId}`);
  }
}
