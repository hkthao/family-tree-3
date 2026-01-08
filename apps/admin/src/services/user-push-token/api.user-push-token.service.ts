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

import type { Paginated, ListOptions, FilterOptions } from '@/types'; // Import Paginated and ListOptions
import { buildQueryString } from '@/utils/url'; // Assuming buildQueryString utility exists

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

  getUserPushTokensByUserId(
    userId: string,
    options: ListOptions,
  ): Promise<Result<Paginated<UserPushTokenDto>, ApiError>> {
    const queryString = buildQueryString({
      page: options.page,
      itemsPerPage: options.itemsPerPage,
      sortBy: options.sortBy?.map(s => `${s.key}:${s.order}`).join(','),
    });
    return this.api.get<Paginated<UserPushTokenDto>>(
      `${this.baseUrl}/user/${userId}?${queryString}`,
    );
  }

  searchUserPushTokens(
    userId: string | null | undefined,
    options: ListOptions,
    filters: FilterOptions,
  ): Promise<Result<Paginated<UserPushTokenDto>, ApiError>> {
    const params: { [key: string]: any } = {
      page: options.page,
      itemsPerPage: options.itemsPerPage,
      sortBy: options.sortBy?.map(s => `${s.key}:${s.order}`).join(','),
      SearchQuery: filters.search,
    };
    if (userId !== null && userId !== undefined) {
      params.UserId = userId;
    }
    const queryString = buildQueryString(params);
    return this.api.get<Paginated<UserPushTokenDto>>(
      `${this.baseUrl}/search?${queryString}`,
    );
  }
}

