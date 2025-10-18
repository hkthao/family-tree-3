import { defineStore } from 'pinia';
import type { Member } from '@/types';
import i18n from '@/plugins/i18n';

interface FaceMemberState {
  managedMembers: Member[];
  loadingManagedMembers: boolean;
  errorManagedMembers: string | null;
}

export const useFaceMemberStore = defineStore('faceMember', {
  state: (): FaceMemberState => ({
    managedMembers: [],
    loadingManagedMembers: false,
    errorManagedMembers: null,
  }),

  actions: {
    async loadManagedMembers(): Promise<void> {
      this.loadingManagedMembers = true;
      this.errorManagedMembers = null;
      try {
        const result = await this.services.faceMember.getManagedMembers();
        if (result.ok) {
          this.managedMembers = result.value;
        } else {
          this.errorManagedMembers = result.error?.message || i18n.global.t('face.labeling.errors.loadManagedMembersFailed');
        }
      } catch (err: any) {
        this.errorManagedMembers = err.message || i18n.global.t('face.labeling.errors.unexpectedError');
      } finally {
        this.loadingManagedMembers = false;
      }
    },
  },
});
