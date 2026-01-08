import { ApiUserPushTokenService } from '@/services/user-push-token/api.user-push-token.service';
import { apiClient } from '@/plugins/axios';

let userPushTokenService: ApiUserPushTokenService;

export function useUserPushTokenApi() {
  if (!userPushTokenService) {
    userPushTokenService = new ApiUserPushTokenService(apiClient);
  }
  return { userPushTokenService };
}
