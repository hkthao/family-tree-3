import React, { useState, useRef } from 'react';
import { View, StyleSheet, FlatList, Dimensions, Animated, ViewToken } from 'react-native';
import { Button, useTheme } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useRouter } from 'expo-router';
import OnboardingSlide from '@/components/onboarding/OnboardingSlide';
import { SPACING_MEDIUM, SPACING_LARGE } from '@/constants/dimensions';
import AsyncStorage from '@react-native-async-storage/async-storage';

const { width } = Dimensions.get('window');

const slides = [
  {
    id: '1',
    image: require('@/assets/images/onboarding-1.png'), // Placeholder image
    title: 'onboarding.slide1.title',
    description: 'onboarding.slide1.description',
  },
  {
    id: '2',
    image: require('@/assets/images/onboarding-2.png'), // Placeholder image
    title: 'onboarding.slide2.title',
    description: 'onboarding.slide2.description',
  },
  {
    id: '3',
    image: require('@/assets/images/onboarding-3.png'), // Placeholder image
    title: 'onboarding.slide3.title',
    description: 'onboarding.slide3.description',
  },
  {
    id: '4',
    image: require('@/assets/images/onboarding-4.png'), // Placeholder image
    title: 'onboarding.slide4.title',
    description: 'onboarding.slide4.description',
  },
  {
    id: '5',
    image: require('@/assets/images/onboarding-5.png'), // Placeholder image
    title: 'onboarding.slide5.title',
    description: 'onboarding.slide5.description',
  },
];

export default function OnboardingScreen() {
  const { t } = useTranslation();
  const theme = useTheme();
  const router = useRouter();
  const [currentIndex, setCurrentIndex] = useState(0);
  const scrollX = useRef(new Animated.Value(0)).current;
  const slidesRef = useRef<FlatList | null>(null);

  const viewableItemsChanged = useRef(({ viewableItems }: { viewableItems: ViewToken[] }) => {
    if (viewableItems.length > 0 && typeof viewableItems[0].index === 'number') {
      setCurrentIndex(viewableItems[0].index);
    }
  }).current;

  const viewConfig = useRef({ viewAreaCoveragePercentThreshold: 50 }).current;

  const scrollToPrevious = () => {
    if (slidesRef.current && currentIndex > 0) {
      slidesRef.current.scrollToIndex({ index: currentIndex - 1 });
    }
  };

  const scrollToNext = () => {
    if (slidesRef.current && currentIndex < slides.length - 1) {
      slidesRef.current.scrollToIndex({ index: currentIndex + 1 });
    } else {
      skipOnboarding();
    }
  };

  const skipOnboarding = async () => {

    await AsyncStorage.setItem('hasOnboarded', 'true');
    router.replace('/feature-under-development');
  };

  const styles = StyleSheet.create({
    container: {
      flex: 1,
      justifyContent: 'center',
      alignItems: 'center',
      backgroundColor: theme.colors.background,
    },
    pagination: {
      flexDirection: 'row',
      height: 64,
      justifyContent: 'center',
      alignItems: 'center',
    },
    dot: {
      height: 10,
      borderRadius: 5,
      backgroundColor: theme.colors.primary,
      marginHorizontal: 8,
    },
    buttonContainer: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      width: '100%',
      paddingHorizontal: SPACING_MEDIUM,
      paddingBottom: SPACING_LARGE,
    },
    skipButton: {
      backgroundColor: theme.colors.surfaceVariant,
    },
    nextButton: {
      backgroundColor: theme.colors.primary,
      borderRadius: theme.roundness,
    },
    buttonLabel: {
      color: theme.colors.onPrimary,
    },
  });

  const renderPagination = () => {
    return (
      <View style={styles.pagination}>
        {slides.map((_, i) => {
          const inputRange = [(i - 1) * width, i * width, (i + 1) * width];
          const dotWidth = scrollX.interpolate({
            inputRange,
            outputRange: [10, 20, 10],
            extrapolate: 'clamp',
          });
          const opacity = scrollX.interpolate({
            inputRange,
            outputRange: [0.3, 1, 0.3],
            extrapolate: 'clamp',
          });
          return (
            <Animated.View
              style={[styles.dot, { width: dotWidth, opacity }]}
              key={i.toString()}
            />
          );
        })}
      </View>
    );
  };

  return (
    <View style={styles.container}>

      <FlatList
        data={slides}
        renderItem={({ item }) => <OnboardingSlide item={item} />}
        horizontal
        pagingEnabled
        showsHorizontalScrollIndicator={false}
        bounces={false}
        keyExtractor={(item) => item.id}
        onScroll={Animated.event(
          [{ nativeEvent: { contentOffset: { x: scrollX } } }],
          { useNativeDriver: false }
        )}
        onViewableItemsChanged={viewableItemsChanged}
        viewabilityConfig={viewConfig}
        scrollEventThrottle={32}
        ref={slidesRef}
      />
      {renderPagination()}
      <View style={styles.buttonContainer}>
        {/* Left Button: Back or empty space */}
        {currentIndex > 0 ? (
          <Button mode="text" onPress={scrollToPrevious}>
            {t('onboarding.back')}
          </Button>
        ) : (
          <View /> // Empty view to take up space if no back button
        )}

        {/* Right Buttons: Skip/Next or Get Started */}
        <View style={{ flexDirection: 'row' }}>
          {currentIndex < slides.length - 1 ? (
            <>
              <Button mode="text" onPress={skipOnboarding} style={{ marginRight: SPACING_MEDIUM, borderRadius: theme.roundness }}>
                {t('onboarding.skip')}
              </Button>
              <Button style={{ borderRadius: theme.roundness }} mode="contained" onPress={scrollToNext}>
                {t('onboarding.next')}
              </Button>
            </>
          ) : (
            <Button mode="contained" onPress={skipOnboarding} labelStyle={styles.buttonLabel} style={{ borderRadius: theme.roundness }}>
              {t('onboarding.getStarted')}
            </Button>
          )}
        </View>
      </View>
    </View>
  );
}
