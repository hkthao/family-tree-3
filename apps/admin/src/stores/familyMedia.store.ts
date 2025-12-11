import { defineStore } from 'pinia';
import type { IFamilyMediaService } from '@/services/family-media/family-media.service.interface';
import type { MediaLink, RefType } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import { useI18n } from 'vue-i18n';
import { err } from '@/types'; // Import Result, ok, err

export const useFamilyMediaStore = defineStore('familyMedia', {
  state: () => ({
    mediaLinks: {
      items: [] as MediaLink[],
      loading: false,
      error: null as string | null,
    },
  }),
  getters: {
    familyMediaService(): IFamilyMediaService {
      return this.services.familyMedia;
    },
  },
  actions: {
    async linkMediaToEntity(familyId: string, familyMediaId: string, refType: RefType, refId: string) {
      const { showSnackbar } = useGlobalSnackbar();
      const { t } = useI18n();
      this.mediaLinks.loading = true;
      try {
        const result = await this.familyMediaService.linkMediaToEntity(familyId, familyMediaId, String(refType), refId);
        if (result.ok) {
          showSnackbar(t('familyMedia.messages.linkSuccess'), 'success');
          this.getMediaLinksByRefId(familyId, refType, refId); // Refresh links
          return result;
        } else {
          showSnackbar(result.error.message || t('familyMedia.errors.link'), 'error');
          return result;
        }
      } catch (error: any) {
        showSnackbar(error.message || t('familyMedia.errors.link'), 'error');
        return err(new Error(error.message));
      } finally {
        this.mediaLinks.loading = false;
      }
    },

    async unlinkMediaFromEntity(familyId: string, familyMediaId: string, refType: RefType, refId: string) {
      const { showSnackbar } = useGlobalSnackbar();
      const { t } = useI18n();
      this.mediaLinks.loading = true;
      try {
        const result = await this.familyMediaService.unlinkMediaFromEntity(familyId, familyMediaId, String(refType), refId);
        if (result.ok) {
          showSnackbar(t('familyMedia.messages.unlinkSuccess'), 'success');
          this.getMediaLinksByRefId(familyId, refType, refId); // Refresh links
          return result;
        } else {
          showSnackbar(result.error.message || t('familyMedia.errors.unlink'), 'error');
          return result;
        }
      } catch (error: any) {
        showSnackbar(error.message || t('familyMedia.errors.unlink'), 'error');
        return err(new Error(error.message));
      } finally {
        this.mediaLinks.loading = false;
      }
    },

    async getMediaLinksByRefId(familyId: string, refType: RefType, refId: string) {
      const { showSnackbar } = useGlobalSnackbar();
      const { t } = useI18n();
      this.mediaLinks.loading = true;
      this.mediaLinks.error = null;
      try {
        const result = await this.familyMediaService.getMediaLinksByRefId(familyId, String(refType), refId);
        if (result.ok) {
          this.mediaLinks.items = result.value;
        } else {
          this.mediaLinks.error = result.error.message || t('familyMedia.errors.loadLinks');
          showSnackbar(this.mediaLinks.error ?? '', 'error');
        }
      } catch (error: any) {
        this.mediaLinks.error = error.message || t('familyMedia.errors.loadLinks');
        showSnackbar(this.mediaLinks.error ?? '', 'error');
      }
      finally {
        this.mediaLinks.loading = false;
      }
    },
  },
});