import { ref, computed, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';

import type { IVoiceProfileService } from '@/services/voice-profile/voice-profile.service.interface';
import type { VoiceProfile } from '@/types';
import { useServices } from '@/plugins/services.plugin';
import type { ApiError } from '@/types/apiError'; // Import ApiError

export function useVoiceProfileImportExport(memberId: Ref<string>) {
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();
  const { voiceProfile: voiceProfileService } = useServices();

  const isExporting = ref(false);
  const isImporting = ref(false);

  const exportVoiceProfiles = async () => {
    isExporting.value = true;
    try {
      const response = await voiceProfileService.exportVoiceProfiles(memberId.value);
      if (response.ok) {
        const dataStr = JSON.stringify(response.value, null, 2);
        const dataUri = 'data:application/json;charset=utf-8,' + encodeURIComponent(dataStr);
        const exportFileDefaultName = `voice_profiles_member_${memberId.value}.json`;

        const linkElement = document.createElement('a');
        linkElement.setAttribute('href', dataUri);
        linkElement.setAttribute('download', exportFileDefaultName);
        linkElement.click();
        showSnackbar(t('voiceProfile.messages.exportSuccess'), 'success');
      } else {
        showSnackbar(response.error.message || t('voiceProfile.messages.exportError'), 'error');
      }
    } catch (error: any) {
      console.error('Error exporting voice profiles:', error);
      showSnackbar(error.message || t('voiceProfile.messages.exportError'), 'error');
    } finally {
      isExporting.value = false;
    }
  };

  const importVoiceProfiles = async (data: VoiceProfile[]) => {
    isImporting.value = true;
    try {
      const response = await voiceProfileService.importVoiceProfiles(memberId.value, data);
      if (response.ok) {
        showSnackbar(t('voiceProfile.messages.importSuccess'), 'success');
      } else {
        showSnackbar(response.error.message || t('voiceProfile.messages.importError'), 'error');
      }
    } catch (error: any) {
      console.error('Error importing voice profiles:', error);
      showSnackbar(error.message || t('voiceProfile.messages.importError'), 'error');
    } finally {
      isImporting.value = false;
    }
  };

  return {
    isExporting,
    isImporting,
    exportVoiceProfiles,
    importVoiceProfiles,
  };
}
