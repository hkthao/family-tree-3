import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { DetectedFace, Member } from '@/types';
import { useQuery } from '@tanstack/vue-query';
import { useServices } from '@/composables';
import { queryKeys } from '@/constants/queryKeys';

export function useFaceMemberSelectDialog(props: {
  show: boolean;
  selectedFace: DetectedFace | null;
  familyId?: string;
  showRelationPromptField: boolean;
  disableSaveValidation: boolean;
}, emit: {
  (e: 'update:show', value: boolean): void;
  (e: 'label-face', updatedFace: DetectedFace): void;
}) {
  const { t } = useI18n();
  const { member: memberService } = useServices(); // Use the member service

  const selectedMemberId = ref<string | null | undefined>(undefined);
  const internalRelationPrompt = ref<string | undefined>(undefined);

  // Fetch member details using useQuery
  const { data: selectedMemberDetails } = useQuery<Member | undefined, Error>({
    queryKey: queryKeys.members.detail(selectedMemberId.value || ''),
    queryFn: async () => {
      if (!selectedMemberId.value) return Promise.reject(new Error(t('member.memberIdRequired')));
      const result = await memberService.getById(selectedMemberId.value);
      if (result.ok) {
        return result.value;
      } else {
        throw result.error;
      }
    },
    enabled: computed(() => !!selectedMemberId.value),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  watch(() => props.selectedFace, (newFace) => {
    selectedMemberId.value = newFace?.memberId;
    internalRelationPrompt.value = newFace?.relationPrompt;
  }, { immediate: true });

  const faceThumbnailSrc = computed(() => {
    if (props.selectedFace?.thumbnail) {
      return `data:image/jpeg;base64,${props.selectedFace.thumbnail}`;
    }
    return '';
  });

  const handleSave = () => {
    if (props.selectedFace && selectedMemberId.value && selectedMemberDetails.value) {
      const member = selectedMemberDetails.value as Member;
      const updatedFace: DetectedFace = {
        ...props.selectedFace,
        memberId: member.id,
        memberName: member.fullName,
        relationPrompt: internalRelationPrompt.value,
      };
      emit('label-face', updatedFace);
      emit('update:show', false);
    }
  };

  return {
    t,
    selectedMemberId,
    selectedMemberDetails,
    internalRelationPrompt,
    faceThumbnailSrc,
    handleSave,
  };
}