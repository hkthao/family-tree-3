import { defineStore } from 'pinia';
import { container } from 'tsyringe';
import { Services } from '@/services/service.factory';
import type { IFamilyMediaService } from '@/services/family-media/family-media.service.interface';
import type { FamilyMedia, MediaLink, FamilyMediaFilter, PaginatedList, ListOptions, RefType } from '@/types';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import { useI18n } from 'vue-i18n';
import { Result } from '@/types/result'; // Import Result

export const useFamilyMediaStore = defineStore('familyMedia', {
  state: () => ({
    list: {
      items: [] as FamilyMedia[],
      totalItems: 0,
      page: 1,
      itemsPerPage: 10,
      sortBy: [] as ListOptions['sortBy'],
      filters: {} as FamilyMediaFilter,
      loading: false,
      error: null as string | null,
    },
    detail: {
      item: null as FamilyMedia | null,
      loading: false,
      error: null as string | null,
    },
    mediaLinks: {
      items: [] as MediaLink[],
      loading: false,
      error: null as string | null,
    },
    // For upload progress
    uploadProgress: 0,
    isUploading: false,
  }),
  getters: {
    familyMediaService(): IFamilyMediaService {
      return container.resolve(Services.FamilyMedia);
    },
    // Add any specific getters here if needed
  },
  actions: {
    setListOptions(options: ListOptions) {
      this.list.page = options.page || 1;
      this.list.itemsPerPage = options.itemsPerPage || 10;
      this.list.sortBy = options.sortBy || [];
      this._loadItems();
    },

    async _loadItems() {
      const { showSnackbar } = useGlobalSnackbar();
      const { t } = useI18n();
      this.list.loading = true;
      this.list.error = null;
      try {
        if (!this.list.filters.familyId) {
            this.list.error = t('familyMedia.errors.familyIdRequired');
            showSnackbar(this.list.error, 'error');
            return;
        }
        const result = await this.familyMediaService.getFamilyMediaList(
          this.list.filters.familyId,
          {
            searchQuery: this.list.filters.searchQuery,
            refId: this.list.filters.refId,
            refType: this.list.filters.refType,
            mediaType: this.list.filters.mediaType,
            page: this.list.page,
            itemsPerPage: this.list.itemsPerPage,
            sortBy: this.list.sortBy ? this.list.sortBy.map(s => `${s.key} ${s.order}`).join(',') : undefined,
          }
        );

        if (result.isSuccess) {
          this.list.items = result.value.items;
          this.list.totalItems = result.value.totalItems;
          this.list.page = result.value.page;
          this.list.itemsPerPage = result.value.itemsPerPage;
        } else {
          this.list.error = result.error?.message || t('familyMedia.errors.loadList');
          showSnackbar(this.list.error, 'error');
        }
      } catch (error: any) {
        this.list.error = error.message || t('familyMedia.errors.loadList');
        showSnackbar(this.list.error, 'error');
      } finally {
        this.list.loading = false;
      }
    },

    async getById(familyId: string, id: string) {
      const { showSnackbar } = useGlobalSnackbar();
      const { t } = useI18n();
      this.detail.loading = true;
      this.detail.error = null;
      try {
        const result = await this.familyMediaService.getFamilyMediaById(familyId, id);
        if (result.isSuccess) {
          this.detail.item = result.value;
        } else {
          this.detail.error = result.error?.message || t('familyMedia.errors.loadDetail');
          showSnackbar(this.detail.error, 'error');
        }
      } catch (error: any) {
        this.detail.error = error.message || t('familyMedia.errors.loadDetail');
        showSnackbar(this.detail.error, 'error');
      } finally {
        this.detail.loading = false;
      }
    },

    async createFamilyMedia(familyId: string, file: File, description?: string) {
      const { showSnackbar } = useGlobalSnackbar();
      const { t } = useI18n();
      this.isUploading = true;
      this.uploadProgress = 0;
      try {
        const result = await this.familyMediaService.createFamilyMedia(familyId, file, description);
        if (result.isSuccess) {
          showSnackbar(t('familyMedia.messages.uploadSuccess'), 'success');
          this._loadItems(); // Refresh list after upload
          return result;
        } else {
          showSnackbar(result.error?.message || t('familyMedia.errors.upload'), 'error');
          return result;
        }
      } catch (error: any) {
        showSnackbar(error.message || t('familyMedia.errors.upload'), 'error');
        return Result.failure(new Error(error.message));
      } finally {
        this.isUploading = false;
        this.uploadProgress = 0;
      }
    },

    async deleteFamilyMedia(familyId: string, id: string) {
      const { showSnackbar } = useGlobalSnackbar();
      const { t } = useI18n();
      this.list.loading = true; // Use list loading for deletion too
      try {
        const result = await this.familyMediaService.deleteFamilyMedia(familyId, id);
        if (result.isSuccess) {
          showSnackbar(t('familyMedia.messages.deleteSuccess'), 'success');
          this._loadItems(); // Reload list after deletion
        } else {
          showSnackbar(result.error?.message || t('familyMedia.errors.delete'), 'error');
        }
      } catch (error: any) {
        showSnackbar(error.message || t('familyMedia.errors.delete'), 'error');
      } finally {
        this.list.loading = false;
      }
    },

    async linkMediaToEntity(familyId: string, familyMediaId: string, refType: RefType, refId: string) {
      const { showSnackbar } = useGlobalSnackbar();
      const { t } = useI18n();
      this.mediaLinks.loading = true;
      try {
        const result = await this.familyMediaService.linkMediaToEntity(familyId, familyMediaId, refType, refId);
        if (result.isSuccess) {
          showSnackbar(t('familyMedia.messages.linkSuccess'), 'success');
          this.getMediaLinksByRefId(familyId, refType, refId); // Refresh links
          return result;
        } else {
          showSnackbar(result.error?.message || t('familyMedia.errors.link'), 'error');
          return result;
        }
      } catch (error: any) {
        showSnackbar(error.message || t('familyMedia.errors.link'), 'error');
        return Result.failure(new Error(error.message));
      } finally {
        this.mediaLinks.loading = false;
      }
    },

    async unlinkMediaFromEntity(familyId: string, familyMediaId: string, refType: RefType, refId: string) {
      const { showSnackbar } = useGlobalSnackbar();
      const { t } = useI18n();
      this.mediaLinks.loading = true;
      try {
        const result = await this.familyMediaService.unlinkMediaFromEntity(familyId, familyMediaId, refType, refId);
        if (result.isSuccess) {
          showSnackbar(t('familyMedia.messages.unlinkSuccess'), 'success');
          this.getMediaLinksByRefId(familyId, refType, refId); // Refresh links
          return result;
        } else {
          showSnackbar(result.error?.message || t('familyMedia.errors.unlink'), 'error');
          return result;
        }
      } catch (error: any) {
        showSnackbar(error.message || t('familyMedia.errors.unlink'), 'error');
        return Result.failure(new Error(error.message));
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
        const result = await this.familyMediaService.getMediaLinksByRefId(familyId, refType, refId);
        if (result.isSuccess) {
          this.mediaLinks.items = result.value;
        } else {
          this.mediaLinks.error = result.error?.message || t('familyMedia.errors.loadLinks');
          showSnackbar(this.mediaLinks.error, 'error');
        }
      } catch (error: any) {
        this.mediaLinks.error = error.message || t('familyMedia.errors.loadLinks');
        showSnackbar(this.mediaLinks.error, 'error');
      } finally {
        this.mediaLinks.loading = false;
      }
    },

    clearDetail() {
      this.detail.item = null;
      this.detail.error = null;
      this.detail.loading = false;
    },
  },
});
