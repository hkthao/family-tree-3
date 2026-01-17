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

const MAX_NODES_TO_DISPLAY = 100; // Hardcoded limit for now, can be made configurable
const MAX_RELATIONSHIPS_TO_DISPLAY = 80; // New hardcoded limit as per user's request

/**
 * Traverses the family tree from the rootId and collects a limited number of connected members and relationships.
 * This function ensures that the displayed tree is manageable in size.
 * @param members An array of all available MemberDto objects.
 * @param relationships An array of all available Relationship objects.
 * @param rootId The ID of the central member to start the traversal from.
 * @param maxNodes The maximum number of nodes to include in the subtree.
 * @param maxRelationships The maximum number of relationships to include in the subtree.
 * @returns An object containing the filtered members and relationships.
 */
function getLimitedFamilySubtree(
  members: MemberDto[],
  relationships: Relationship[],
  rootId: string | null,
  maxNodes: number,
  maxRelationships: number // New parameter
): { filteredMembers: MemberDto[]; filteredRelationships: Relationship[] } {
  console.log('[getLimitedFamilySubtree] Start. Total members:', members.length, 'Total relationships:', relationships.length, 'RootId:', rootId, 'MaxNodes:', maxNodes, 'MaxRelationships:', maxRelationships);

  if (!rootId || !members.some(m => m.id === rootId)) {
    console.log('[getLimitedFamilySubtree] Invalid rootId or root member not found. Returning empty.');
    return { filteredMembers: [], filteredRelationships: [] };
  }

  const memberMap = new Map<string, MemberDto>(members.map(m => [m.id, m]));
  const adjacencyList = new Map<string, Set<string>>();

  // Build adjacency list (graph representation) from relationships - O(R)
  relationships.forEach(rel => {
    if (!adjacencyList.has(rel.sourceMemberId)) {
      adjacencyList.set(rel.sourceMemberId, new Set<string>());
    }
    if (!adjacencyList.has(rel.targetMemberId)) {
      adjacencyList.set(rel.targetMemberId, new Set<string>());
    }

    // Add bidirectional connections
    adjacencyList.get(rel.sourceMemberId)?.add(rel.targetMemberId);
    adjacencyList.get(rel.targetMemberId)?.add(rel.sourceMemberId);
  });
  console.log('[getLimitedFamilySubtree] Adjacency list built.');

  const selectedMemberIds = new Set<string>();
  const selectedRelationshipIds = new Set<string>(); // To store unique relationship IDs
  const queue: string[] = [rootId];
  selectedMemberIds.add(rootId);

  // Map for quick lookup of relationships by source-target pair
  const relationshipLookup = new Map<string, Relationship[]>();
  relationships.forEach(rel => {
    const key1 = `${rel.sourceMemberId}-${rel.targetMemberId}`;
    const key2 = `${rel.targetMemberId}-${rel.sourceMemberId}`; // For bidirectional lookup

    if (!relationshipLookup.has(key1)) relationshipLookup.set(key1, []);
    relationshipLookup.get(key1)?.push(rel);

    if (!relationshipLookup.has(key2)) relationshipLookup.set(key2, []);
    relationshipLookup.get(key2)?.push(rel);
  });


  let head = 0;
  while (head < queue.length && selectedMemberIds.size < maxNodes && selectedRelationshipIds.size < maxRelationships) {
    const currentMemberId = queue[head++];
    const neighbors = adjacencyList.get(currentMemberId) || new Set<string>();

    for (const neighborId of neighbors) {
      // Check limits before adding
      if (selectedMemberIds.size + 1 > maxNodes || selectedRelationshipIds.size + 1 > maxRelationships) {
          console.log(`[getLimitedFamilySubtree] Node or relationship limit hit before adding neighbor ${neighborId}.`);
          break; // Stop if limits are reached
      }

      if (memberMap.has(neighborId) && !selectedMemberIds.has(neighborId)) {
        // Find relationship(s) between currentMemberId and neighborId
        const relationsBetween = relationshipLookup.get(`${currentMemberId}-${neighborId}`) || [];

        // Check if adding these relationships would exceed maxRelationships
        // Only count relationships that are not already selected
        const newRelationsToAdd = relationsBetween.filter(rel => !selectedRelationshipIds.has(rel.id));

        if (selectedRelationshipIds.size + newRelationsToAdd.length > maxRelationships) {
          console.log(`[getLimitedFamilySubtree] Relationship limit hit when trying to add relations for neighbor ${neighborId}.`);
          continue; // Skip this neighbor, try next one
        }

        selectedMemberIds.add(neighborId);
        queue.push(neighborId);
        newRelationsToAdd.forEach(rel => selectedRelationshipIds.add(rel.id));
      }
    }
  }
  console.log('[getLimitedFamilySubtree] BFS complete. Selected member IDs count:', selectedMemberIds.size, 'Selected relationship IDs count:', selectedRelationshipIds.size);

  const filteredMembers = members.filter(m => selectedMemberIds.has(m.id));
  const filteredRelationships = relationships.filter(rel => selectedRelationshipIds.has(rel.id));

  console.log('[getLimitedFamilySubtree] Filtered members count:', filteredMembers.length, 'Filtered relationships count:', filteredRelationships.length);

  return { filteredMembers, filteredRelationships };
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
): { filteredMembers: MemberDto[]; transformedData: CardDataPayload[] } {
  console.log('[transformFamilyData] Start. Initial members:', members.length, 'rootId:', rootId);

  // Apply the node and relationship limits before transforming the data
  const { filteredMembers, filteredRelationships } = getLimitedFamilySubtree(
    members,
    relationships,
    rootId,
    MAX_NODES_TO_DISPLAY,
    MAX_RELATIONSHIPS_TO_DISPLAY // Pass the new limit
  );
  console.log('[transformFamilyData] Limited subtree obtained. Filtered members:', filteredMembers.length, 'Filtered relationships:', filteredRelationships.length);

  const personMap = new Map<string, CardDataPayload>();

  // 1. Initialize all members in a map for quick access
  filteredMembers.forEach((person) => {
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
  console.log('[transformFamilyData] Person map initialized. Map size:', personMap.size);

  // 2. Process relationships to build the tree structure
  filteredRelationships.forEach((rel) => {
    const sourcePerson = personMap.get(String(rel.sourceMemberId));
    const targetPerson = personMap.get(String(rel.targetMemberId));

    if (!sourcePerson || !targetPerson) {
      // This case should be rare if filteredRelationships are correctly generated
      // console.warn('Could not find person for relationship after filtering:', rel);
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
  console.log('[transformFamilyData] Relationships processed.');

  // 3. Mark the root member and return the array of transformed data
  const transformedData = Array.from(personMap.values()).map(person => {
    if (rootId && person.id === rootId) {
      return { ...person, data: { ...person.data, main: true } };
    }
    return person;
  });
  console.log('[transformFamilyData] Transformed data generated. Count:', transformedData.length);

  return { filteredMembers, transformedData };
}

/**
 * Determines the initial main ID for the family tree chart.
 * @param currentMembers All members currently available.
 * @param transformedData The data transformed for the f3 chart.
 * @param providedRootId The root ID provided as a prop.
 * @returns The ID of the main person to set for the chart, or undefined if none can be determined.
 */
export function determineMainChartId(
  filteredMembers: MemberDto[], // Changed from currentMembers to filteredMembers
  transformedData: CardDataPayload[],
  providedRootId: string | null
): string | undefined {
  console.log('[determineMainChartId] Start. Filtered members count:', filteredMembers.length, 'Transformed data count:', transformedData.length, 'Provided rootId:', providedRootId);
  let mainIdToSet: string | undefined;

  if (providedRootId) {
    // Check if the provided rootId exists in the transformed data
    const foundRootMember = transformedData.find(d => d.id === providedRootId);
    if (foundRootMember) {
      mainIdToSet = providedRootId;
      console.log('[determineMainChartId] Provided rootId found in transformed data.');
    } else {
      console.log('[determineMainChartId] Provided rootId not found in transformed data.');
    }
  }

  if (!mainIdToSet) {
    // Fallback to existing logic if providedRootId is not found or not provided
    const rootMember = filteredMembers.find((m: MemberDto) => m.isRoot);
    if (rootMember) {
      mainIdToSet = rootMember.id;
      console.log('[determineMainChartId] Fallback: Found an isRoot member in filtered members.');
    } else if (transformedData.length > 0) {
      mainIdToSet = transformedData[0].id; // Fallback to the first available member
      console.log('[determineMainChartId] Fallback: Using first member in transformed data.');
    } else {
      console.log('[determineMainChartId] Fallback: No members in transformed data.');
    }
  }
  console.log('[determineMainChartId] Returning mainIdToSet:', mainIdToSet);
  return mainIdToSet;
}
