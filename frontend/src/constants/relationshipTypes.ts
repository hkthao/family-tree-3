import { RelationshipType } from '@/types';
import i18n from '@/plugins/i18n';

export const RELATIONSHIP_TYPE_OPTIONS = [
  { title: i18n.global.t('relationship.type.parent'), value: RelationshipType.Parent },
  { title: i18n.global.t('relationship.type.child'), value: RelationshipType.Child },
  { title: i18n.global.t('relationship.type.spouse'), value: RelationshipType.Spouse },
  { title: i18n.global.t('relationship.type.sibling'), value: RelationshipType.Sibling },
  { title: i18n.global.t('relationship.type.other'), value: RelationshipType.Other },
];

export function getRelationshipTypeTitle(type: RelationshipType | undefined): string {
  if (type === undefined) {
    return i18n.global.t('common.unknown');
  }
  const option = RELATIONSHIP_TYPE_OPTIONS.find(opt => opt.value === type);
  return option ? option.title : i18n.global.t('common.unknown');
}
