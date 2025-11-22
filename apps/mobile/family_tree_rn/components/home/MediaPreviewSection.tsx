import React, { useState, useMemo } from 'react';
import { StyleSheet, View, Dimensions, TouchableOpacity } from 'react-native';
import { Text, useTheme } from 'react-native-paper';
import { Image } from 'expo-image';
import { TFunction } from 'i18next';
import ImageViewing from 'react-native-image-viewing';
import { SPACING_LARGE } from '@/constants/dimensions'; // Import spacing constants

interface MediaPreviewSectionProps {
  t: TFunction;
  backgroundColor?: string; // Add backgroundColor prop
}

const { width } = Dimensions.get('window');
const SPACING = 10; // Desired spacing between images
const NUM_COLUMNS = 2;
const IMAGE_SIZE = (width - (SPACING * (NUM_COLUMNS + 1))) / NUM_COLUMNS;

const images = [
  require('../../assets/images/onboarding-2.png'),
  require('../../assets/images/onboarding-3.png'),
  require('../../assets/images/onboarding-4.png'),
  require('../../assets/images/onboarding-2.png'), // Reusing an image to maintain array length
];

export function MediaPreviewSection({ t, backgroundColor }: MediaPreviewSectionProps) {
  const [visible, setIsVisible] = useState(false);
  const [currentImageIndex, setImageIndex] = useState(0);
  const theme = useTheme(); // Get theme from PaperProvider

  const onSelectImage = (index: number) => {
    setImageIndex(index);
    setIsVisible(true);
  };

  const styles = useMemo(() => StyleSheet.create({
    container: {
      paddingHorizontal: SPACING, // Apply padding to the container
      paddingVertical: SPACING_LARGE,
    },
    sectionTitle: {
      textAlign: 'center',
      marginBottom: SPACING_LARGE,
      fontWeight: 'bold',
    },
    imageGallery: {
      flexDirection: 'row',
      flexWrap: 'wrap',
      justifyContent: 'space-between', // Distribute space evenly
    },
    imageItem: {
      width: IMAGE_SIZE,
      height: IMAGE_SIZE,
      borderRadius: theme.roundness,
      marginBottom: SPACING, // Use SPACING for vertical margin
    },
  }), [theme]);

  return (
    <>
      <View style={[styles.container, backgroundColor ? { backgroundColor } : {}]}>
        <Text variant="headlineMedium" style={styles.sectionTitle}>
          {t('home.media_preview.title')}
        </Text>
        <View style={styles.imageGallery}>
          {images.map((image, index) => (
            <TouchableOpacity key={index} onPress={() => onSelectImage(index)}>
              <Image
                source={image} // Direct image source
                style={styles.imageItem}
                contentFit="cover"
              />
            </TouchableOpacity>
          ))}
        </View>
      </View>

      <ImageViewing
        images={images}
        imageIndex={currentImageIndex}
        visible={visible}
        onRequestClose={() => setIsVisible(false)}
      />
    </>
  );
}


