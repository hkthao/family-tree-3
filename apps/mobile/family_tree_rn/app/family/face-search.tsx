import React, { useState, useEffect } from 'react';
import { StyleSheet, View, Image, Button, Alert } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Text, useTheme } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import * as ImagePicker from 'expo-image-picker';
import { useCameraPermissions } from 'expo-camera';
import { detectFaces } from '@/api/publicApiClient';
import type { FaceDetectionResponseDto, DetectedFaceDto } from '@/types/public.d';

export default function FamilyFaceSearchScreen() {
  const { t } = useTranslation();
  const theme = useTheme();
  // const currentFamilyId = useFamilyStore((state) => state.currentFamilyId);

  const [image, setImage] = useState<string | null>(null);
  const [imageDimensions, setImageDimensions] = useState<{ width: number; height: number } | null>(null);
  const [detectedFaces, setDetectedFaces] = useState<DetectedFaceDto[]>([]);
  const [loading, setLoading] = useState(false);

  const [cameraPermission, requestCameraPermission] = useCameraPermissions();
  const [mediaLibraryPermission, requestMediaLibraryPermission] = ImagePicker.useMediaLibraryPermissions();

  useEffect(() => {
    (async () => {
      await requestCameraPermission();
      await requestMediaLibraryPermission();
    })();
  }, [requestCameraPermission, requestMediaLibraryPermission]);

  const pickImage = async () => {
    if (!mediaLibraryPermission?.granted) {
      Alert.alert(t('faceSearch.permissionRequired'), t('faceSearch.mediaLibraryPermissionDenied'));
      return;
    }

    let result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ImagePicker.MediaTypeOptions.Images,
      allowsEditing: true,
      aspect: [4, 3],
      quality: 1,
      base64: true,
    });

    if (!result.canceled && result.assets && result.assets.length > 0) {
      const selectedImage = result.assets[0];
      setImage(selectedImage.uri);
      setImageDimensions({ width: selectedImage.width, height: selectedImage.width }); // Use width for both to maintain aspect ratio for bounding box calculations
      setLoading(true);
      setDetectedFaces([]); // Clear previous detections

      try {
        if (selectedImage.base64) {
          const response = await detectFaces({
            imageBytes: selectedImage.base64,
            contentType: selectedImage.mimeType || 'image/jpeg',
            returnCrop: true,
          });
          if (response && response.DetectedFaces) {
            setDetectedFaces(response.DetectedFaces);
          }
        } else {
          Alert.alert(t('common.error'), t('faceSearch.base64Error'));
        }
      } catch (err) {
        console.error('Face detection API error:', err);
        Alert.alert(t('common.error'), t('faceSearch.detectionFailed'));
      } finally {
        setLoading(false);
      }
    }
  };

  const takePhoto = async () => {
    if (!cameraPermission?.granted) {
      Alert.alert(t('faceSearch.permissionRequired'), t('faceSearch.cameraPermissionDenied'));
      return;
    }

    let result = await ImagePicker.launchCameraAsync({
      allowsEditing: true,
      aspect: [4, 3],
      quality: 1,
      base64: true,
    });

    if (!result.canceled && result.assets && result.assets.length > 0) {
      const selectedImage = result.assets[0];
      setImage(selectedImage.uri);
      setImageDimensions({ width: selectedImage.width, height: selectedImage.height });
      setLoading(true);
      setDetectedFaces([]); // Clear previous detections

      try {
        if (selectedImage.base64) {
          const response = await detectFaces({
            imageBytes: selectedImage.base64,
            contentType: selectedImage.mimeType || 'image/jpeg',
            returnCrop: true,
          });
          if (response && response.DetectedFaces) {
            setDetectedFaces(response.DetectedFaces);
          }
        } else {
          Alert.alert(t('common.error'), t('faceSearch.base64Error'));
        }
      } catch (err) {
        console.error('Face detection API error:', err);
        Alert.alert(t('common.error'), t('faceSearch.detectionFailed'));
      } finally {
        setLoading(false);
      }
    }
  };

  const styles = StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: theme.colors.background,
      alignItems: 'center',
      justifyContent: 'center',
      padding: 20,
    },
    imageContainer: {
      marginTop: 20,
      width: '100%', // Take full width
      aspectRatio: 4 / 3, // Maintain aspect ratio
      borderColor: theme.colors.outline,
      borderWidth: 1,
      justifyContent: 'center',
      alignItems: 'center',
      position: 'relative', // For absolute positioning of bounding boxes
    },
    image: {
      width: '100%',
      height: '100%',
      resizeMode: 'contain',
    },
    boundingBox: {
      position: 'absolute',
      borderColor: 'red',
      borderWidth: 2,
    },
    label: {
      position: 'absolute',
      backgroundColor: 'rgba(255, 255, 255, 0.7)',
      paddingHorizontal: 4,
      paddingVertical: 2,
      fontSize: 12,
      color: 'black',
    },
    buttonContainer: {
      flexDirection: 'row',
      marginTop: 20,
      gap: 10,
    },
    text: {
      marginBottom: 10,
    },
  });

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.buttonContainer}>
        <Button title={t('faceSearch.pickImage')} onPress={pickImage} />
        <Button title={t('faceSearch.takePhoto')} onPress={takePhoto} />
      </View>

      {loading && <Text>{t('common.loading')}</Text>}

      {image && imageDimensions && (
        <View style={styles.imageContainer}>
          <Image source={{ uri: image }} style={styles.image} />
          {detectedFaces.map((face: DetectedFaceDto, index: number) => {
            const scaleX = (styles.imageContainer.width as number) / imageDimensions.width;
            const scaleY = (styles.imageContainer.aspectRatio as number * (styles.imageContainer.width as number)) / imageDimensions.height;

            const box = face.BoundingBox;
            const scaledBox = {
              x: box.X * scaleX,
              y: box.Y * scaleY,
              width: box.Width * scaleX,
              height: box.Height * scaleY,
            };

            return (
              <View
                key={index}
                style={[
                  styles.boundingBox,
                  {
                    left: scaledBox.x,
                    top: scaledBox.y,
                    width: scaledBox.width,
                    height: scaledBox.height,
                  },
                ]}
              >
                {face.MemberName && (
                  <Text style={[styles.label, { top: -20 }]}>
                    {face.MemberName}
                  </Text>
                )}
              </View>
            );
          })}
        </View>
      )}

      {detectedFaces.length > 0 && <Text>{t('faceSearch.facesDetected', { count: detectedFaces.length })}</Text>}
    </SafeAreaView>
  );
}
