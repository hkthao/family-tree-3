import React, { useMemo } from 'react';
import { StyleSheet, Image } from 'react-native';
import { useTheme, Chip } from 'react-native-paper'; // Import Chip
import { SPACING_SMALL } from '@/constants/dimensions';
import { getAvatarSource } from '@/utils/imageUtils';

interface MemberAvatarChipProps {
  id: string;
  fullName: string;
  avatarUrl?: string | null;
}

const MemberAvatarChip: React.FC<MemberAvatarChipProps> = ({ fullName, avatarUrl }) => {
  const theme = useTheme();

  const styles = useMemo(() => StyleSheet.create({
    avatar: {
      width: 24,
      height: 24,
      borderRadius: 12, // Half of width/height for circular avatar
      backgroundColor: theme.colors.surface, // Fallback background for avatar
    },
    chip: {
      marginRight: SPACING_SMALL,
      marginBottom: SPACING_SMALL,
    }
  }), [theme]);

  const avatarSource = getAvatarSource(avatarUrl);

  return (
    <Chip
      compact={true}
      avatar={<Image source={avatarSource} style={styles.avatar} />}
      onPress={() => console.log('Pressed')} // TODO: Add actual navigation or action
      style={styles.chip}
      textStyle={{ color: theme.colors.onSurfaceVariant, fontSize: 12, fontWeight: '500' }}
    >
      {fullName}
    </Chip>
  );
};

export default MemberAvatarChip;
