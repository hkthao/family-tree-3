// apps/admin/src/utils/avatar.utils.ts

import { Gender } from '@/types'; // Assuming Gender enum is defined here or similar type

const FEMALE_AVATAR_PLACEHOLDER = '/female_avatar.png'; // Path to your default female avatar
const MALE_AVATAR_PLACEHOLDER = '/male_avatar.png'; // Path to your default male avatar
const GENERIC_AVATAR_PLACEHOLDER = 'https://via.placeholder.com/150'; // Generic placeholder
const FAMILY_AVATAR_PLACEHOLDER = '/family_avatar.png'; // Default family avatar placeholder

export function getAvatarUrl(avatarUrl: string | null | undefined, gender: string | null | undefined): string {
  if (avatarUrl) {
    return avatarUrl;
  }

  if (gender === Gender.Female) {
    return FEMALE_AVATAR_PLACEHOLDER;
  }

  if (gender === Gender.Male) {
    return MALE_AVATAR_PLACEHOLDER;
  }

  return GENERIC_AVATAR_PLACEHOLDER;
}

export function getFamilyAvatarUrl(avatarUrl: string | null | undefined): string {
  if (avatarUrl) {
    return avatarUrl;
  }
  return FAMILY_AVATAR_PLACEHOLDER;
}
