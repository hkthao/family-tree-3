export interface FamilyFollowDto {
  id: string;
  userId: string;
  familyId: string;
  isFollowing: boolean;
  notifyDeathAnniversary: boolean;
  notifyBirthday: boolean;
  notifyEvent: boolean;
}

export interface UpdateFamilyFollowSettingsCommand {
  familyId: string;
  notifyDeathAnniversary: boolean;
  notifyBirthday: boolean;
  notifyEvent: boolean;
}

export interface FollowFamilyCommand {
  familyId: string;
  notifyDeathAnniversary?: boolean;
  notifyBirthday?: boolean;
  notifyEvent?: boolean;
}
