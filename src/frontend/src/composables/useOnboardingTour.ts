import { onMounted } from 'vue';
import { driver } from 'driver.js';
import 'driver.js/dist/driver.css';
import { useI18n } from 'vue-i18n';

const TOUR_FLAG = 'dashboardTourCompleted';

export function useOnboardingTour() {
  const { t } = useI18n();

  onMounted(() => {
    const tourCompleted = localStorage.getItem(TOUR_FLAG);

    if (tourCompleted === 'true') {
      return;
    }

    const driverObj = driver({
      showProgress: true,
      steps: [
        { element: '#dashboard-link', popover: { title: t('onboarding.tourSteps.dashboard.title'), description: t('onboarding.tourSteps.dashboard.description') } },
        { element: '#family-auto-complete', popover: { title: t('onboarding.tourSteps.familyAutoComplete.title'), description: t('onboarding.tourSteps.familyAutoComplete.description') } },
        { element: '#genealogy-chart', popover: { title: t('onboarding.tourSteps.genealogyChart.title'), description: t('onboarding.tourSteps.genealogyChart.description') } },
        { element: '#dashboard-event-calendar', popover: { title: t('onboarding.tourSteps.eventCalendar.title'), description: t('onboarding.tourSteps.eventCalendar.description') } },
        { element: '#dashboard-recent-activity', popover: { title: t('onboarding.tourSteps.recentActivity.title'), description: t('onboarding.tourSteps.recentActivity.description') } },
      ],
      onDestroyed: () => {
        localStorage.setItem(TOUR_FLAG, 'true');
      },
    });

    setTimeout(() => driverObj.drive(), 500);
  });
}
