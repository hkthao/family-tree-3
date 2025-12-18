import { reactive, watch, toRefs } from 'vue';
import { useVuelidate } from '@vuelidate/core';
import type { MemberFace } from '@/types';
import { useMemberFaceFormRules } from '@/validations/memberFace.validation';

interface UseMemberFaceFormOptions {
  initialMemberFaceData?: MemberFace;
  memberId?: string;
}

export function useMemberFaceForm(options: UseMemberFaceFormOptions) {
  const { initialMemberFaceData, memberId } = options;


  const defaultFormData = (): MemberFace => ({
    id: '',
    memberId: memberId || '',
    faceId: '',
    boundingBox: { x: 0, y: 0, width: 0, height: 0 },
    confidence: 0, 
    embedding: [],
    isVectorDbSynced: false,
    thumbnailUrl: '',
    originalImageUrl: '',
    emotion: '',
    emotionConfidence: 0,
    vectorDbId: '',
    memberName: '',
    memberGender: '',
    memberAvatarUrl: '',
    familyName: '',
  });

  const state = reactive<MemberFace>(initialMemberFaceData ? { ...defaultFormData(), ...initialMemberFaceData } : defaultFormData());

  watch(() => memberId, (newMemberId) => {
    if (newMemberId) {
      state.memberId = newMemberId;
    }
  });

  const rules = useMemberFaceFormRules();
  const v$ = useVuelidate(rules, toRefs(state));

  const validate = async () => {
    const result = await v$.value.$validate();
    return result;
  };

  const getFormData = (): MemberFace => {
    // Handle embedding string to array conversion if necessary, as per the original component
    if (typeof state.embedding === 'string') {
      try {
        state.embedding = JSON.parse(state.embedding);
      } catch (e) {
        console.error('Failed to parse embedding string:', e);
        state.embedding = [];
      }
    }
    return { ...state };
  };

  // Watch for initial data changes for external updates to the form
  watch(
    () => initialMemberFaceData,
    (newVal) => {
      if (newVal) {
        Object.assign(state, { ...defaultFormData(), ...newVal });
      } else {
        Object.assign(state, defaultFormData());
      }
    },
    { deep: true }
  );

  // Watch for embedding changes to stringify if it's an array for display in textarea
  watch(() => state.embedding, (newVal) => {
    if (Array.isArray(newVal)) {
      // Check if it's an array of numbers or empty
      if (newVal.length === 0 || typeof newVal[0] === 'number') {
        (state.embedding as any) = JSON.stringify(newVal);
      }
    }
  }, { immediate: true, deep: true });

  return {
    state,
    v$,
    validate,
    getFormData,
  };
}
