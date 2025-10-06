import { RelationshipType } from '@/types';
import i18n from '@/plugins/i18n';

export const RELATIONSHIP_TYPE_OPTIONS = [
  { title: i18n.global.t('relationship.type.father'), value: RelationshipType.Father },
  { title: i18n.global.t('relationship.type.mother'), value: RelationshipType.Mother },
  { title: i18n.global.t('relationship.type.wife'), value: RelationshipType.Wife },
  { title: i18n.global.t('relationship.type.husband'), value: RelationshipType.Husband },
];

export function getRelationshipTypeTitle(type: RelationshipType | undefined): string {
  if (type === undefined) {
    return i18n.global.t('common.unknown');
  }
  const option = RELATIONSHIP_TYPE_OPTIONS.find(opt => opt.value === type);
  return option ? option.title : i18n.global.t('common.unknown');
}
