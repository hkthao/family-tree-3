import { ref, watch, computed, type Ref, type ComputedRef } from 'vue';
import { useI18n, type Composer } from 'vue-i18n';
import type { DetectedFace, MemberDto } from '@/types';
import { useQuery, type UseQueryOptions, type UseQueryReturnType } from '@tanstack/vue-query';
import { useServices } from '@/plugins/services.plugin';
import { queryKeys } from '@/constants/queryKeys';
import type { IMemberService } from '@/services/member/member.service.interface';

interface UseFaceMemberSelectDialogDeps {
  useI18n: () => Composer;
  useQuery: <TData = unknown, TError = Error>(
    options: UseQueryOptions<TData, TError> | Ref<UseQueryOptions<TData, TError>> | ComputedRef<UseQueryOptions<TData, TError>>,
  ) => UseQueryReturnType<TData, TError>;
  getMemberService: () => IMemberService;
}

const defaultDeps: UseFaceMemberSelectDialogDeps = {
  useI18n,
  useQuery,
  getMemberService: () => useServices().member,
};

export function useFaceMemberSelectDialog(props: {
  show: boolean;
  selectedFace: DetectedFace | null;
  familyId?: string;
  showRelationPromptField: boolean;
  disableSaveValidation: boolean;
}, emit: {
  (e: 'update:show', value: boolean): void;
  (e: 'label-face', updatedFace: DetectedFace): void;
}, deps: UseFaceMemberSelectDialogDeps = defaultDeps) {
  const { useI18n: injectedUseI18n, useQuery: injectedUseQuery, getMemberService } = deps;
  const { t } = injectedUseI18n();
  const memberService = getMemberService();

  const selectedMemberId = ref<string | null | undefined>(undefined);
  const internalRelationPrompt = ref<string | undefined>(undefined);

  // Fetch member details using useQuery
  const { data: selectedMemberDetails } = injectedUseQuery<MemberDto | undefined, Error>({
    queryKey: computed(() => queryKeys.members.detail(selectedMemberId.value || '')),
    queryFn: async () => {
      if (!selectedMemberId.value) return Promise.reject(new Error(t('member.memberIdRequired')));
      const result = await memberService.getById(selectedMemberId.value);
      if (result.ok) {
        return result.value;
      } else {
        throw result.error;
      }
    },
    enabled: computed(() => !!selectedMemberId.value && !!memberService),
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
      const member = selectedMemberDetails.value as MemberDto;
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
    state: {
      t, // Keep t for translation directly if needed in template
      selectedMemberId,
      selectedMemberDetails,
      internalRelationPrompt,
      faceThumbnailSrc,
    },
    actions: {
      handleSave,
    },
  };
}