import { defineStore } from 'pinia';
import type { FamilyExportDto } from '@/types/family';
import i18n from '@/plugins/i18n';

export const useFamilyDataStore = defineStore('familyData', {
  state: () => ({
    exporting: false,
    importing: false,
    error: null as string | null,
  }),
  actions: {
    async exportFamilyData(familyId: string) {
      this.exporting = true;
      this.error = null;
      try {
        const result = await this.services.familyData.exportFamilyData(familyId);

        if (result.ok) {
          const familyExportDto: FamilyExportDto = result.value;
          const filename = `family_data_${familyExportDto.name.replace(/\s/g, '_')}_${new Date().toISOString().split('T')[0]}.json`;
          const jsonStr = JSON.stringify(familyExportDto, null, 2);
          const blob = new Blob([jsonStr], { type: 'application/json' });
          const url = URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = filename;
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          URL.revokeObjectURL(url);
          return true;
        } else {
          this.error = result.error?.message || i18n.global.t('family.errors.export');
          return false;
        }
      } catch (error: any) {
        this.error = error.message || i18n.global.t('family.errors.export');
        return false;
      } finally {
        this.exporting = false;
      }
    },

    async importFamilyData(familyId: string, familyData: FamilyExportDto, clearExistingData: boolean = true) {
      this.importing = true;
      this.error = null;
      try {
        const result = await this.services.familyData.importFamilyData(familyId, familyData, clearExistingData);

        if (result.ok) {
          return result.value; // Return new family ID
        } else {
          this.error = result.error?.message || i18n.global.t('family.errors.import');
          return null;
        }
      } catch (error: any) {
        this.error = error.message || i18n.global.t('family.errors.import');
        return null;
      } finally {
        this.importing = false;
      }
    },
  },
});
