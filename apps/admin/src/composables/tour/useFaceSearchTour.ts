import { onMounted } from 'vue';
import { driver } from 'driver.js';
import 'driver.js/dist/driver.css';
import { useI18n } from 'vue-i18n';

const TOUR_FLAG = 'faceSearchTourCompleted';

export function useFaceSearchTour() {
  let tour: any;
  const { t } = useI18n();

  const initializeTour = () => {
    tour = driver({
      showProgress: true,
      steps: [
        {
          element: '#tour-face-upload',
          popover: {
            title: t('tour.faceSearch.upload.title'),
            description: t('tour.faceSearch.upload.description'),
          },
        },
        {
          element: '#tour-face-viewer',
          popover: {
            title: t('tour.faceSearch.viewer.title'),
            description: t('tour.faceSearch.viewer.description'),
          },
        },
        {
          element: '#tour-face-sidebar',
          popover: {
            title: t('tour.faceSearch.sidebar.title'),
            description: t('tour.faceSearch.sidebar.description'),
          },
        },
      ],
      onDestroyed: () => {
        localStorage.setItem(TOUR_FLAG, 'true');
      },
    });
  };

  const startTour = () => {
    const tourCompleted = localStorage.getItem(TOUR_FLAG);
    if (tourCompleted !== 'true') {
      tour.drive();
    }
  };

  onMounted(() => {
    initializeTour();
    // Small delay to ensure the DOM is fully ready
    setTimeout(startTour, 500);
  });

  return {
    startTour,
  };
}
