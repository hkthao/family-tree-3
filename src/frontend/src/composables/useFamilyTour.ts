import { onMounted } from 'vue';
import { driver } from 'driver.js';
import 'driver.js/dist/driver.css';
import { useI18n } from 'vue-i18n';

const TOUR_FLAG = 'familyTourCompleted';

export function useFamilyTour() {
  const { t } = useI18n();

  onMounted(() => {
    const tourCompleted = localStorage.getItem(TOUR_FLAG);

    if (tourCompleted === 'true') {
      return;
    }

    const driverObj = driver({
      showProgress: true,
      steps: [
        {
          element: '#tour-step-1',
          popover: {
            title: t('tour.familyList.search.title'),
            description: t('tour.familyList.search.description'),
          },
        },
        {
          element: '#tour-step-2 .v-btn[aria-label="Create Family"]',
          popover: {
            title: t('tour.familyList.create.title'),
            description: t('tour.familyList.create.description'),
          },
        },
        {
          element: '#tour-step-2',
          popover: {
            title: t('tour.familyList.list.title'),
            description: t('tour.familyList.list.description'),
          },
        },
        {
          element: '#tour-step-2 tbody tr:first-child a[aria-label="View"]',
          popover: {
            title: t('tour.familyList.view.title'),
            description: t('tour.familyList.view.description'),
          },
        },
        {
          element: '#tour-step-2 tbody tr:first-child .v-btn[aria-label="Delete"]',
          popover: {
            title: t('tour.familyList.delete.title'),
            description: t('tour.familyList.delete.description'),
          },
        },
      ],
      onDestroyed: () => {
        localStorage.setItem(TOUR_FLAG, 'true');
      },
    });

    setTimeout(() => driverObj.drive(), 500);
  });
}
