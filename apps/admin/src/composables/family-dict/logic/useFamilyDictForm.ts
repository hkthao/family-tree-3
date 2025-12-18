import { reactive, toRefs, toRef, computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyDict } from '@/types';
import { FamilyDictType, FamilyDictLineage } from '@/types';
import { useFamilyDictRules } from '@/validations/family-dict.validation';
import { useAuth } from '@/composables';

interface FamilyDictFormProps {
  readOnly?: boolean;
  initialFamilyDictData?: FamilyDict;
}

export function useFamilyDictForm(props: FamilyDictFormProps) {
  const { t } = useI18n();
  const { isAdmin, isFamilyManager } = useAuth();
  const formRef = ref<any>(null);

  const isFormReadOnly = computed(() => {
    return props.readOnly || !(isAdmin.value || isFamilyManager.value);
  });

  const familyDictTypes = computed(() => [
    { title: t('familyDict.type.blood'), value: FamilyDictType.Blood },
    { title: t('familyDict.type.marriage'), value: FamilyDictType.Marriage },
    { title: t('familyDict.type.adoption'), value: FamilyDictType.Adoption },
    { title: t('familyDict.type.inLaw'), value: FamilyDictType.InLaw },
    { title: t('familyDict.type.other'), value: FamilyDictType.Other },
  ]);

  const familyDictLineages = computed(() => [
    { title: t('familyDict.lineage.noi'), value: FamilyDictLineage.Noi },
    { title: t('familyDict.lineage.ngoai'), value: FamilyDictLineage.Ngoai },
    { title: t('familyDict.lineage.noiNgoai'), value: FamilyDictLineage.NoiNgoai },
    { title: t('familyDict.lineage.other'), value: FamilyDictLineage.Other },
  ]);

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

  const state = reactive({
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

  const rules = useFamilyDictRules(state);

  const validate = async () => {
    const { valid } = await formRef.value.validate();
    return valid;
  };

  const getFormData = () => {
    return formData;
  };

  return {
    formRef,
    isFormReadOnly,
    familyDictTypes,
    familyDictLineages,
    formData,
    rules,
    validate,
    getFormData,
  };
}
