import { type ComposerTranslation } from 'vue-i18n';
import { FamilyDictType, FamilyDictLineage } from '@/types';

export function getFamilyDictTypeOptions(t: ComposerTranslation) {
  return [
    { title: t('familyDict.type.blood'), value: FamilyDictType.Blood },
    { title: t('familyDict.type.marriage'), value: FamilyDictType.Marriage },
    { title: t('familyDict.type.adoption'), value: FamilyDictType.Adoption },
    { title: t('familyDict.type.inLaw'), value: FamilyDictType.InLaw },
    { title: t('familyDict.type.other'), value: FamilyDictType.Other },
  ];
}

export function getFamilyDictLineageOptions(t: ComposerTranslation) {
  return [
    { title: t('familyDict.lineage.noi'), value: FamilyDictLineage.Noi },
    { title: t('familyDict.lineage.ngoai'), value: FamilyDictLineage.Ngoai },
    { title: t('familyDict.lineage.noiNgoai'), value: FamilyDictLineage.NoiNgoai },
    { title: t('familyDict.lineage.other'), value: FamilyDictLineage.Other },
  ];
}

export function getFamilyDictRegionOptions(t: ComposerTranslation) {
  return [
    { title: t('familyDict.form.namesByRegion.north'), value: 'north' },
    { title: t('familyDict.form.namesByRegion.central'), value: 'central' },
    { title: t('familyDict.form.namesByRegion.south'), value: 'south' },
  ];
}

export function getFamilyDictTypeTitle(t: ComposerTranslation, type: FamilyDictType) {
  switch (type) {
    case FamilyDictType.Blood: return t('familyDict.type.blood');
    case FamilyDictType.Marriage: return t('familyDict.type.marriage');
    case FamilyDictType.Adoption: return t('familyDict.type.adoption');
    case FamilyDictType.InLaw: return t('familyDict.type.inLaw');
    case FamilyDictType.Other: return t('familyDict.type.other');
    default: return t('common.unknown');
  }
}

export function getFamilyDictLineageTitle(t: ComposerTranslation, lineage: FamilyDictLineage) {
  switch (lineage) {
    case FamilyDictLineage.Noi: return t('familyDict.lineage.noi');
    case FamilyDictLineage.Ngoai: return t('familyDict.lineage.ngoai');
    case FamilyDictLineage.NoiNgoai: return t('familyDict.lineage.noiNgoai');
    case FamilyDictLineage.Other: return t('familyDict.lineage.other');
    default: return t('common.unknown');
  }
}