import { ImageSourcePropType } from 'react-native';

const defaultFamilyAvatar = require('@/assets/images/familyAvatar.png');

export const getAvatarSource = (avatarUrl?: string | null): ImageSourcePropType => {
  if (avatarUrl) {
    return { uri: avatarUrl };
  }
  return defaultFamilyAvatar;
};
