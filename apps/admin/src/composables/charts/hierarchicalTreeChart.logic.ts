import type { MemberDto, Relationship } from '@/types';
import { Gender, RelationshipType } from '@/types';
import { getAvatarUrl } from '@/utils/avatar.utils'; // NEW

// Define the type for the data used in the family chart cards
interface CardDataPayload {
  id: string;
  data: {
    avatar?: string;
    gender: 'M' | 'F'; // Strictly M or F
    fullName: string; // Should always be present
    birthYear?: string | number;
    deathYear?: string | number;
    main?: boolean; // Explicitly boolean
    [key: string]: string | number | boolean | undefined; // Allow other keys, including boolean for main
  };
  rels: {
    spouses: string[];
    children: string[];
    father?: string;
    mother?: string;
  };
}

/**
 * Transforms raw Member and Relationship data into a format suitable for the family-chart (f3) library.
 * This function is pure and does not interact with the DOM or external libraries directly.
 * @param members An array of Member objects.
 * @param relationships An array of Relationship objects.
 * @param rootId The ID of the root member for the tree, if any.
 * @returns An object containing the filtered members and transformed data.
 */
export function transformFamilyData(
  members: MemberDto[],
  relationships: Relationship[],
  rootId: string | null
): { members: MemberDto[]; transformedData: CardDataPayload[] } { // Changed filteredMembers to members
  const processedMembers: MemberDto[] = members;
  const processedRelationships: Relationship[] = relationships.filter(rel => rel.sourceMemberId !== rel.targetMemberId);

  const personMap = new Map<string, CardDataPayload>();

  // 1. Initialize all members in a map for quick access
  processedMembers.forEach((person) => {
    personMap.set(String(person.id), {
      id: String(person.id),
      data: {
        fullName: person.fullName || `${person.firstName || ''} ${person.lastName || ''}`.trim(),
        birthYear: person.dateOfBirth ? new Date(person.dateOfBirth).getFullYear() : '',
        deathYear: person.dateOfDeath ? new Date(person.dateOfDeath).getFullYear() : '',
        avatar: getAvatarUrl(person.avatarUrl, person.gender),
        gender: person.gender === Gender.Male ? 'M' : 'F', // Map Unknown to Female by default
      },
      rels: {
        spouses: [],
        children: [],
      },
    });
  });

  // 2. Process relationships to build the tree structure
  processedRelationships.forEach((rel) => {
    const sourcePerson = personMap.get(String(rel.sourceMemberId));
    const targetPerson = personMap.get(String(rel.targetMemberId));

    if (!sourcePerson || !targetPerson) {
      // This case should be rare if filteredRelationships are correctly generated
      // console.warn('Could not find person for relationship after filtering:', rel);
      return;
    }
    // Frontend workaround: Ensure a person is not their own parent/child/spouse in rels
    if (sourcePerson.id === targetPerson.id) {
        console.warn(`Skipping self-referencing relationship for member ${sourcePerson.id}`);
        return;
    }

    switch (rel.type) {
      case RelationshipType.Wife:
      case RelationshipType.Husband:
        // Add spouse if not already added to avoid duplicates
        if (!sourcePerson.rels.spouses.includes(targetPerson.id)) {
          sourcePerson.rels.spouses.push(targetPerson.id);
        }
        if (!targetPerson.rels.spouses.includes(sourcePerson.id)) {
          targetPerson.rels.spouses.push(sourcePerson.id);
        }
        break;

      case RelationshipType.Father:
        targetPerson.rels.father = sourcePerson.id;
        if (!sourcePerson.rels.children.includes(targetPerson.id)) {
          sourcePerson.rels.children.push(targetPerson.id);
        }
        break;

      case RelationshipType.Mother:
        targetPerson.rels.mother = sourcePerson.id;
        if (!sourcePerson.rels.children.includes(targetPerson.id)) {
          sourcePerson.rels.children.push(targetPerson.id);
        }
        break;
      // Add other relationship types if they influence tree structure
    }
  });

  // 3. Mark the root member and return the array of transformed data
  const transformedData = Array.from(personMap.values()).map(person => {
    if (rootId && person.id === rootId) {
      return { ...person, data: { ...person.data, main: true } };
    }
    return person;
  });

  return { members: processedMembers, transformedData }; // Changed filteredMembers to members
}

/**
 * Determines the initial main ID for the family tree chart.
 * @param currentMembers All members currently available.
 * @param transformedData The data transformed for the f3 chart.
 * @param providedRootId The root ID provided as a prop.
 * @returns The ID of the main person to set for the chart, or undefined if none can be determined.
 */
export function determineMainChartId(
  members: MemberDto[], // Changed filteredMembers to members
  transformedData: CardDataPayload[],
  providedRootId: string | null
): string | undefined {
  let mainIdToSet: string | undefined;

  if (providedRootId) {
    // Check if the provided rootId exists in the transformed data
    const foundRootMember = transformedData.find(d => d.id === providedRootId);
    if (foundRootMember) {
      mainIdToSet = providedRootId;
    }
  }

  if (!mainIdToSet) {
    // Fallback to existing logic if providedRootId is not found or not provided
    const rootMember = members.find((m: MemberDto) => m.isRoot);
    if (rootMember) {
      mainIdToSet = rootMember.id;
    } else if (transformedData.length > 0) {
      mainIdToSet = transformedData[0].id; // Fallback to the first available member
    }
  }
  return mainIdToSet;
}
