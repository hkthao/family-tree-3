import { driver } from 'driver.js';
import 'driver.js/dist/driver.css';
import { useI18n } from 'vue-i18n';

export function useOnboardingTour() {
  const { t } = useI18n();

  const startTour = () => {
    const driverObj = driver({
      showProgress: true,
      steps: [
        // Các bước này cần có ID HTML tương ứng trên trang Dashboard
        { element: '#dashboard-link', popover: { title: t('onboarding.tourSteps.dashboard.title'), description: t('onboarding.tourSteps.dashboard.description') } },
        { element: '#genealogy-chart', popover: { title: t('onboarding.tourSteps.genealogyChart.title'), description: t('onboarding.tourSteps.genealogyChart.description') } },
      ]
    });
    driverObj.drive();
  };

  return {
    startTour,
  };
}
