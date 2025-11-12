import { RelationshipType } from '@/types/relationship.d';

export function getRelationshipTypeStringName(type: RelationshipType | undefined | null): string {
  if (type === undefined || type === null) {
    return 'Unknown';
  }
  switch (type) {
    case RelationshipType.Father:
      return 'Father';
    case RelationshipType.Mother:
      return 'Mother';
    case RelationshipType.Husband:
      return 'Husband';
    case RelationshipType.Wife:
      return 'Wife';
    case RelationshipType.Child:
      return 'Child';
    default:
      return 'Unknown';
  }
}
