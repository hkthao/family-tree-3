import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { DetectedFace, Member } from '@/types';
import { useFaceMemberSelectStore } from '@/stores/faceMemberSelect.store';

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
  const faceMemberSelectStore = useFaceMemberSelectStore();

  const selectedMemberId = ref<string | null | undefined>(undefined);
  const selectedMemberDetails = ref<Member | null>(null);
  const internalRelationPrompt = ref<string | undefined>(undefined);

  watch(() => props.selectedFace, (newFace) => {
    selectedMemberId.value = newFace?.memberId;
    internalRelationPrompt.value = newFace?.relationPrompt;
  }, { immediate: true });

  watch(selectedMemberId, async (newMemberId) => {
    if (newMemberId) {
      await faceMemberSelectStore.getById(newMemberId);
      selectedMemberDetails.value = faceMemberSelectStore.detail.item || null;
    } else {
      selectedMemberDetails.value = null;
    }
  }, { immediate: true });

  const faceThumbnailSrc = computed(() => {
    if (props.selectedFace?.thumbnail) {
      return `data:image/jpeg;base64,${props.selectedFace.thumbnail}`;
    }
    return '';
  });

  const handleSave = () => {
    if (props.selectedFace && selectedMemberId.value && selectedMemberDetails.value) {
      const updatedFace: DetectedFace = {
        ...props.selectedFace,
        memberId: selectedMemberDetails.value.id,
        memberName: selectedMemberDetails.value.fullName,
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