import { defineStore } from 'pinia';
import i18n from '@/plugins/i18n';
import type { Family, Member } from '@/types'; // Import Family and Member from '@/types'
import type { ApiError } from '@/plugins/axios';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { FamilyPdfExportData, MemberPdfExportData } from '@/types/pdf'; // Import the new PDF DTOs

export const useExportStore = defineStore('export', {
  state: () => ({
    error: null as string | null,
    pdfHtmlContent: null as string | null, // State for storing generated HTML
    exportingPdf: false, // Loading state for PDF generation
  }),
  actions: {
    setPdfHtmlContent(html: string | null) {
      this.pdfHtmlContent = html;
    },

    async transformFamilyToPdfExportData(familyId: string): Promise<FamilyPdfExportData | undefined> {
        const services = (this as any).services; // Access services injected by plugin

        // Fetch full family details, including members and their event counts
        const familyDetailResult = await services.family.getByIdWithDetails(familyId); 

        if (!familyDetailResult.ok || !familyDetailResult.value) {
            console.error('Failed to fetch family details for PDF export:', familyDetailResult.error);
            return undefined;
        }

        const family: Family = familyDetailResult.value;

        const membersPdfExport: MemberPdfExportData[] = family.members.map((member: Member) => ({
            id: member.id,
            fullName: member.fullName,
            gender: member.gender,
            dateOfBirth: member.dateOfBirth ? new Date(member.dateOfBirth).toLocaleDateString('vi-VN') : undefined,
            eventMembersCount: member.eventMembers?.length || 0, // Assuming member object has eventMembers array
        }));

        return {
            id: family.id,
            name: family.name,
            description: family.description,
            members: membersPdfExport,
        };
    },

    async exportFamilyPdf(familyId: string, htmlContent: string) {
      this.exportingPdf = true;
      const { showSnackbar } = useGlobalSnackbar(); // Corrected destructuring
      const services = (this as any).services; // Access services injected by plugin

      try {
        const result = await services.familyData.exportFamilyPdf(familyId, htmlContent);

        if (result.ok) {
          const url = window.URL.createObjectURL(new Blob([result.value]));
          const link = document.createElement('a');
          link.href = url;
          link.setAttribute('download', `family_tree_${familyId}.pdf`); // Filename can be improved if returned by API
          document.body.appendChild(link);
          link.click();
          link.remove();
          window.URL.revokeObjectURL(url);
          showSnackbar(i18n.global.t('family.export_pdf_success'), 'success');
        } else {
          console.error('Error exporting PDF:', result.error);
          showSnackbar(i18n.global.t('family.export_pdf_error'), 'error');
        }
      } catch (error) {
        console.error('Unexpected error exporting PDF:', error);
        showSnackbar(i18n.global.t('family.export_pdf_error'), 'error');
      } finally {
        this.exportingPdf = false;
      }
    },
  },
});