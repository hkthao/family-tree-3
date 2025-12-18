import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { DetectedFace, Member } from '@/types';

export function useFaceLabelCard(props: {
  face: DetectedFace;
  members: Member[];
}, emit: (event: 'label-face' | 'create-member', ...args: any[]) => void) {
  const { t } = useI18n();

  const selectedMemberId = ref<string | null | undefined>(props.face.memberId);
  const showCreateMemberDialog = ref(false);
  const newMemberName = ref('');

  watch(() => props.face.memberId, (newMemberId) => {
    selectedMemberId.value = newMemberId;
  });

  const handleSaveMapping = () => {
    if (selectedMemberId.value) {
      emit('label-face', props.face.id, selectedMemberId.value);
    }
  };

  const handleCreateNewMember = () => {
    if (newMemberName.value) {
      emit('create-member', props.face.id, newMemberName.value);
      showCreateMemberDialog.value = false;
      newMemberName.value = '';
    }
  };

  return {
    t,
    selectedMemberId,
    showCreateMemberDialog,
    newMemberName,
    handleSaveMapping,
    handleCreateNewMember,
  };
}