// apps/admin/src/types/user-push-token.d.ts

export interface UserPushTokenDto {
  id: string;
  userId: string;
  expoPushToken: string;
  platform: string;
  deviceId: string;
  isActive: boolean;
}

export interface CreateUserPushTokenCommand {
  userId: string;
  expoPushToken: string;
  platform: string;
  deviceId: string;
  isActive: boolean;
}

export interface UpdateUserPushTokenCommand {
  id: string;
  userId: string;
  expoPushToken: string;
  platform: string;
  deviceId: string;
  isActive: boolean;
}

