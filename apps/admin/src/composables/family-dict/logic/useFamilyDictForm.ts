import { reactive, toRef, ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyDict } from '@/types';
import { FamilyDictType, FamilyDictLineage } from '@/types';
import { useFamilyDictRules } from '@/validations/family-dict.validation';
import { useAuth } from '@/composables';
import { getFamilyDictTypeOptions, getFamilyDictLineageOptions } from '@/composables/utils/familyDictOptions';

interface FamilyDictFormProps {
  readOnly?: boolean;
  initialFamilyDictData?: FamilyDict;
}

export function useFamilyDictForm(props: FamilyDictFormProps) {
  const { t } = useI18n();
  const { state: authState } = useAuth(); // Renamed to authState
  const formRef = ref<any>(null);

  const isFormReadOnly = computed(() => {
    return props.readOnly || !(authState.isAdmin.value || authState.isFamilyManager.value);
  });

  const familyDictTypes = getFamilyDictTypeOptions(t);
  const familyDictLineages = getFamilyDictLineageOptions(t);

  const formData = reactive<Omit<FamilyDict, 'id'> | FamilyDict>(
    props.initialFamilyDictData
      ? {
        ...props.initialFamilyDictData,
      }
      : {
        name: '',
        type: FamilyDictType.Blood,
        description: '',
        lineage: FamilyDictLineage.Noi,
        specialRelation: false,
        namesByRegion: { north: '', central: '', south: '' },
      },
  );

  const formLocalState = reactive({ // Renamed to formLocalState
    name: toRef(formData, 'name'),
    type: toRef(formData, 'type'),
    description: toRef(formData, 'description'),
    lineage: toRef(formData, 'lineage'),
    namesByRegion: reactive({
      north: toRef(formData.namesByRegion, 'north'),
      central: toRef(formData.namesByRegion, 'central'),
      south: toRef(formData.namesByRegion, 'south'),
    }),
  });

  const rules = useFamilyDictRules(formLocalState); // Passed formLocalState

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
      isFormReadOnly,
      familyDictTypes,
      familyDictLineages,
      formData,
      rules,
    },
    actions: {
      validate,
      getFormData,
    },
  };
}
