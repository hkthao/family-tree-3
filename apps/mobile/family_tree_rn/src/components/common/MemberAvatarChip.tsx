import React, { useMemo } from 'react';
import { View, Text, StyleSheet, Image } from 'react-native';
import { useTheme } from 'react-native-paper';
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
    chip: {
      flexDirection: 'row',
      alignItems: 'center',
      backgroundColor: theme.colors.surfaceVariant,
      borderRadius: 16, // Half of height for pill shape
      paddingHorizontal: SPACING_SMALL,
      paddingVertical: SPACING_SMALL / 2,
      marginRight: SPACING_SMALL,
      marginBottom: SPACING_SMALL,
    },
    avatar: {
      width: 24,
      height: 24,
      borderRadius: 12, // Half of width/height for circular avatar
      marginRight: SPACING_SMALL / 2,
      backgroundColor: theme.colors.surface, // Fallback background for avatar
    },
    name: {
      color: theme.colors.onSurfaceVariant,
      fontSize: 12,
      fontWeight: '500',
    },
  }), [theme]);

  const avatarSource = getAvatarSource(avatarUrl);

  return (
    <View style={styles.chip}>
      <Image source={avatarSource} style={styles.avatar} />
      <Text style={styles.name}>{fullName}</Text>
    </View>
  );
};

export default MemberAvatarChip;
