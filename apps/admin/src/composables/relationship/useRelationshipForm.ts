import { reactive, toRefs, ref, watch } from 'vue';
import type { Relationship } from '@/types';
import { RelationshipType } from '@/types';
import { useRelationshipRules } from '@/validations/relationship.validation';

interface UseRelationshipFormOptions {
  id?: string;
  readOnly?: boolean;
  initialRelationshipData?: Relationship;
}

export function useRelationshipForm(options: UseRelationshipFormOptions) {


  const formRef = ref();

  const formData = reactive<Omit<Relationship, 'id'> | Relationship>(
    options.initialRelationshipData
      ? { ...options.initialRelationshipData }
      : {
        sourceMemberId: '',
        targetMemberId: '',
        type: RelationshipType.Father,
        order: undefined,
        familyId: undefined
      },
  );

  const handleFamilyIdChange = (newFamilyId?: string, oldFamilyId?: string) => {
    if (newFamilyId !== oldFamilyId) {
      formData.sourceMemberId = '';
      formData.targetMemberId = '';
    }
  };

  watch(() => formData.familyId, handleFamilyIdChange);

  const { rules } = useRelationshipRules(toRefs(formData));

  const validate = async () => {
    const { valid } = await formRef.value.validate();
    return valid;
  };

  const getFormData = () => {
    return formData;
  };

  return {
    state: {
      formRef,
      formData,
      validationRules: rules,
    },
    actions: {
      validate,
      getFormData,
    },
  };
}
