import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMutation } from '@tanstack/vue-query';
import { useRelationshipDetectionStore } from '@/stores/relationshipDetection.store';
import type { RelationshipDetectionResult } from '@/types/relationshipDetection.d';

export function useRelationshipDetector() {
  const { t } = useI18n();
  const relationshipDetectionStore = useRelationshipDetectionStore();

  const selectedFamilyId = ref<string | undefined>(undefined);
  const selectedMemberAId = ref<string | undefined>(undefined);
  const selectedMemberBId = ref<string | undefined>(undefined);

  // Initialize with null to indicate no detection has been attempted yet
  const result = ref<RelationshipDetectionResult | null>(null);
  const error = ref<string | null>(null);

  // Reset members and results when family changes
  watch(selectedFamilyId, () => {
    selectedMemberAId.value = undefined;
    selectedMemberBId.value = undefined;
    result.value = null;
    error.value = null;
  });

  const { mutate, isPending } = useMutation({
    mutationFn: async (payload: { familyId: string; memberAId: string; memberBId: string }) => {
      // Direct call to the store action
      return await relationshipDetectionStore.detectRelationship(
        payload.familyId,
        payload.memberAId,
        payload.memberBId
      );
    },
    onSuccess: (data: RelationshipDetectionResult | null) => {
      // Handle the case where the API returns null or an 'unknown' description
      if (data && (data.description === 'unknown' || data.description === t('relationshipDetection.noRelationshipFound'))) {
        error.value = t('relationshipDetection.noRelationshipFound');
        result.value = null; // Clear result if unknown or no relationship found
      } else if (data) {
        result.value = data;
        error.value = null; // Clear previous errors
      } else {
        // If data is null/undefined and not explicitly handled above
        error.value = t('relationshipDetection.noRelationshipFound');
        result.value = null;
      }
    },
    onError: (err: any) => {
      error.value = err.message || t('relationshipDetection.genericError');
      result.value = null; // Clear any previous result on error
    },
  });

  const detectRelationship = () => {
    if (!selectedFamilyId.value || !selectedMemberAId.value || !selectedMemberBId.value) {
      error.value = t('relationshipDetection.selectFamilyAndMembersError');
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
    selectedFamilyId,
    selectedMemberAId,
    selectedMemberBId,
    result,
    loading: isPending, // isLoading from useMutation
    error,
    detectRelationship,
  };
}
