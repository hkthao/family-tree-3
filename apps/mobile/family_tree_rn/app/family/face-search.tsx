import React, { useState, useEffect } from 'react';
import { StyleSheet, View, Image, Alert } from 'react-native';
import { Text, useTheme, Button } from 'react-native-paper';
import { Svg, Rect, Text as SvgText, G } from 'react-native-svg';
import { useTranslation } from 'react-i18next';
import * as ImagePicker from 'expo-image-picker';
import { useCameraPermissions } from 'expo-camera';
import { detectFaces } from '@/api/publicApiClient';
import type { DetectedFaceDto } from '@/types/public.d';
import { SPACING_MEDIUM } from '@/constants/dimensions';
import { useRouter } from 'expo-router';
export default function FamilyFaceSearchScreen() {
  const { t } = useTranslation();
  const theme = useTheme();
  const router = useRouter();
  // const currentFamilyId = useFamilyStore((state) => state.currentFamilyId);

  const [image, setImage] = useState<string | null>(null);
  const [imageDimensions, setImageDimensions] = useState<{ width: number; height: number } | null>(null);
  const [containerDimensions, setContainerDimensions] = useState<{ width: number; height: number } | null>(null);
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
      setImageDimensions({ width: selectedImage.width, height: selectedImage.height });
      setLoading(true);
      setDetectedFaces([]); // Clear previous detections
      try {
        if (selectedImage.base64) {
          const response = await detectFaces({
            imageBytes: selectedImage.base64,
            contentType: selectedImage.mimeType || 'image/jpeg',
            returnCrop: false,
          });
          console.log(response);
          if (response && response.detectedFaces) {
            setDetectedFaces(response.detectedFaces);
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
            returnCrop: false,
          });
          if (response && response.detectedFaces) {
            setDetectedFaces(response.detectedFaces);
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
      padding: SPACING_MEDIUM,
    },
    imageContainer: {
      width: '100%', // Take full width
      aspectRatio: 4 / 3, // Maintain aspect ratio
      borderColor: theme.colors.outline,
      borderWidth: 0.5,
      borderRadius: theme.roundness,
      position: 'relative', // For absolute positioning of bounding boxes
    },
    image: {
      width: '100%',
      height: '100%',
      resizeMode: 'contain',
    },
    buttonContainer: {
      flexDirection: 'row',
      gap: SPACING_MEDIUM,
      marginTop: SPACING_MEDIUM
    },
  });
  return (
    <View style={styles.container}>
      {image && imageDimensions && (
        <View
          style={styles.imageContainer}
          onLayout={(event) => {
            const { width, height } = event.nativeEvent.layout;
            setContainerDimensions({ width, height });
          }}
        >
          <Image source={{ uri: image }} style={styles.image} />
          {containerDimensions && (
            <Svg
              height={containerDimensions.height}
              width={containerDimensions.width}
              style={StyleSheet.absoluteFill}
            >
              {detectedFaces.map((face: DetectedFaceDto, index: number) => {
                // Calculate actual rendered image dimensions within the container
                const imageAspectRatio = imageDimensions.width / imageDimensions.height;
                const containerAspectRatio = containerDimensions.width / containerDimensions.height;
                let actualImageRenderedWidth = 0;
                let actualImageRenderedHeight = 0;
                let offsetX = 0;
                let offsetY = 0;
                if (imageAspectRatio > containerAspectRatio) {
                  // Image is wider than container, will be letterboxed vertically
                  actualImageRenderedWidth = containerDimensions.width;
                  actualImageRenderedHeight = containerDimensions.width / imageAspectRatio;
                  offsetY = (containerDimensions.height - actualImageRenderedHeight) / 2;
                } else {
                  // Image is taller than container, will be pillarboxed horizontally
                  actualImageRenderedHeight = containerDimensions.height;
                  actualImageRenderedWidth = containerDimensions.height * imageAspectRatio;
                  offsetX = (containerDimensions.width - actualImageRenderedWidth) / 2;
                }
                const scaleX = actualImageRenderedWidth / imageDimensions.width;
                const scaleY = actualImageRenderedHeight / imageDimensions.height;
                const box = face.boundingBox;
                // Validate all numbers before calculation
                const isValid = [
                  box.x, box.y, box.width, box.height,
                  imageDimensions.width, imageDimensions.height,
                  containerDimensions.width, containerDimensions.height,
                  actualImageRenderedWidth, actualImageRenderedHeight,
                  offsetX, offsetY,
                  scaleX, scaleY,
                ].every(val => typeof val === 'number' && !isNaN(val));
                if (!isValid) {
                  console.warn('Invalid bounding box or scaling parameter detected, skipping face rendering.');
                  return null; // Skip rendering this face if data is invalid
                }

                if (!face.memberName) {
                  return null; // Skip rendering if no member name is associated
                }

                const scaledX = box.x * scaleX;
                const scaledY = box.y * scaleY;
                const scaledWidth = box.width * scaleX;
                const scaledHeight = box.height * scaleY;

                const finalX = scaledX + offsetX;
                const finalY = scaledY + offsetY;

                // Round to avoid floating point issues with native rendering
                const roundedScaledBox = {
                  x: parseFloat(finalX.toFixed(2)),
                  y: parseFloat(finalY.toFixed(2)),
                  width: parseFloat(scaledWidth.toFixed(2)),
                  height: parseFloat(scaledHeight.toFixed(2)),
                };

                return (
                  <G
                    key={index}
                    onPress={() => router.push(`/member/${face.memberId}`)}
                  >
                    <Rect
                      x={roundedScaledBox.x}
                      y={roundedScaledBox.y}
                      width={roundedScaledBox.width}
                      height={roundedScaledBox.height}
                      stroke={theme.colors.primary}
                      strokeWidth="2"
                      fill="rgba(0,0,0,0)" // Transparent fill
                    />
                    {/* Background for text */}
                    <Rect
                      x={roundedScaledBox.x + roundedScaledBox.width / 2 - (face.memberName.length * 3)} // Adjust x based on text length
                      y={roundedScaledBox.y - 20} // Position above the box
                      width={face.memberName.length * 6 + 10} // Adjust width based on text length
                      height="20"
                      fill="rgba(0,0,0,0.5)" // Semi-transparent black background
                      rx="3" // Rounded corners
                      ry="3"
                    />
                    <SvgText
                      x={roundedScaledBox.x + roundedScaledBox.width / 2}
                      y={roundedScaledBox.y - 5} // Position above the box
                      fill="white" // Changed back to white
                      fontSize="12"
                      fontWeight="bold"
                      textAnchor="middle" // Center horizontally
                    >
                      {face.memberName}
                    </SvgText>
                  </G>
                );
              })}
            </Svg>
          )}
        </View>
      )}

      {detectedFaces.length > 0 && <Text style={{ marginTop: SPACING_MEDIUM }}>{t('faceSearch.facesDetected', { count: detectedFaces.length })}</Text>}

      <View style={styles.buttonContainer}>
        <Button mode="contained" onPress={pickImage} loading={loading} disabled={loading} icon="image-multiple" style={{ borderRadius: theme.roundness }}>
          {t('faceSearch.pickImage')}
        </Button>
        <Button
          mode="contained" onPress={takePhoto} loading={loading} disabled={loading} icon="camera" style={{ borderRadius: theme.roundness }}>
          {t('faceSearch.takePhoto')}
        </Button>
      </View>
    </View>
  );
}
