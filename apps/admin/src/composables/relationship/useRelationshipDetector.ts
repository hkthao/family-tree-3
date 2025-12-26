import { ref, watch } from 'vue';
import { useMutation } from '@tanstack/vue-query';
import type { RelationshipDetectionResult } from '@/types/relationshipDetection.d';
import { useServices } from '@/plugins/services.plugin';

export function useRelationshipDetector(initialFamilyId?: string, t?: (key: string) => string) {
  const i18n_t = t || ((key: string) => key);
  const { relationship } = useServices();

  const selectedFamilyId = ref<string | undefined>(initialFamilyId || undefined);
  const selectedMemberAId = ref<string | undefined>(undefined);
  const selectedMemberBId = ref<string | undefined>(undefined);

  // Initialize with null to indicate no detection has been attempted yet
  const result = ref<RelationshipDetectionResult | null>(null);
  const error = ref<string | null>(null);

  // Reset members and results when family changes
  const handleFamilyIdChange = () => {
    selectedMemberAId.value = undefined;
    selectedMemberBId.value = undefined;
    result.value = null;
    error.value = null;
  };

  watch(selectedFamilyId, handleFamilyIdChange);

  const { mutate, isPending } = useMutation({
    mutationFn: async (payload: { familyId: string; memberAId: string; memberBId: string }) => {
      const response = await relationship.detectRelationship(
        payload.familyId,
        payload.memberAId,
        payload.memberBId
      );

      if (response && response.ok) {
        return response.value;
      } else if (response && !response.ok) {
        // Here we throw an error so onError can catch it
        throw new Error(response.error?.message || i18n_t('relationshipDetection.genericError'));
      } else {
        throw new Error(i18n_t('relationshipDetection.genericError'));
      }
    },
    onSuccess: (data: RelationshipDetectionResult | null) => {
      // Handle the case where the API returns null or an 'unknown' description
      if (data && (data.description === 'unknown' || data.description === i18n_t('relationshipDetection.noRelationshipFound'))) {
        error.value = i18n_t('relationshipDetection.noRelationshipFound');
        result.value = null; // Clear result if unknown or no relationship found
      } else if (data) {
        result.value = data;
        error.value = null; // Clear previous errors
      } else {
        // If data is null/undefined and not explicitly handled above
        error.value = i18n_t('relationshipDetection.noRelationshipFound');
        result.value = null;
      }
    },
    onError: (err: any) => {
      error.value = err.message || i18n_t('relationshipDetection.genericError');
      result.value = null; // Clear any previous result on error
    },
  });

  const detectRelationship = () => {
    if (!selectedFamilyId.value || !selectedMemberAId.value || !selectedMemberBId.value) {
      error.value = i18n_t('relationshipDetection.selectFamilyAndMembersError');
      return;
    }

    // Clear previous results and errors before new detection attempt
    result.value = null;
    error.value = null;

    mutate({
      familyId: selectedFamilyId.value,
      memberAId: selectedMemberAId.value,
      memberBId: selectedMemberBId.value,
    });
  };

  return {
    state: {
      selectedFamilyId,
      selectedMemberAId,
      selectedMemberBId,
      result,
      loading: isPending,
      error,
    },
    actions: {
      detectRelationship,
    },
  };
}