import React, { useState } from 'react';
import { StyleSheet, View, Dimensions, TouchableOpacity } from 'react-native';
import { Text } from 'react-native-paper';
import { Image } from 'expo-image';
import { TFunction } from 'i18next';
import ImageViewing from 'react-native-image-viewing';

interface MediaPreviewSectionProps {
  t: TFunction;
}

const { width } = Dimensions.get('window');
const SPACING = 10; // Desired spacing between images
const NUM_COLUMNS = 2;
const IMAGE_SIZE = (width - (SPACING * (NUM_COLUMNS + 1))) / NUM_COLUMNS;

const images = [
  { uri: 'https://picsum.photos/seed/app1/300/300' },
  { uri: 'https://picsum.photos/seed/app2/300/300' },
  { uri: 'https://picsum.photos/seed/app3/300/300' },
  { uri: 'https://picsum.photos/seed/app4/300/300' },
];

export function MediaPreviewSection({ t }: MediaPreviewSectionProps) {
  const [visible, setIsVisible] = useState(false);
  const [currentImageIndex, setImageIndex] = useState(0);

  const onSelectImage = (index: number) => {
    setImageIndex(index);
    setIsVisible(true);
  };

  return (
    <>
      <View style={styles.container}>
        <Text variant="headlineMedium" style={styles.sectionTitle}>
          {t('home.media_preview.title')}
        </Text>
        <View style={styles.imageGallery}>
          {images.map((image, index) => (
            <TouchableOpacity key={index} onPress={() => onSelectImage(index)}>
              <Image
                source={{ uri: image.uri }}
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

const styles = StyleSheet.create({
  container: {
    paddingHorizontal: SPACING, // Apply padding to the container
    paddingVertical: 20,
    backgroundColor: '#f9f9f9', // Light background for the section
  },
  sectionTitle: {
    textAlign: 'center',
    marginBottom: 20,
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
    borderRadius: 8,
    marginBottom: SPACING, // Use SPACING for vertical margin
  },
});
