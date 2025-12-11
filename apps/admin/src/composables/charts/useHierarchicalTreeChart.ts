import { ref, onMounted, onUnmounted, watch, nextTick } from 'vue';
import f3 from 'family-chart';
import 'family-chart/styles/family-chart.css';
import type { Member, Relationship } from '@/types';
import { Gender, RelationshipType } from '@/types';
import { getAvatarUrl } from '@/utils/avatar.utils'; // NEW

// Define the type for the data used in the family chart cards
interface CardData {
  data: {
    id: string;
    data: {
      [key: string]: string | number | undefined;
      avatar?: string;
      gender: 'M' | 'F';
    };
    main?: boolean;
  };
}

export function useHierarchicalTreeChart(
  props: { familyId: string | null; members: Member[]; relationships: Relationship[]; rootId: string | null },
  emit: (event: 'show-member-detail-drawer', ...args: any[]) => void,
  t: (key: string) => string
) {
  const chartContainer = ref<HTMLDivElement | null>(null);
  let chart: any = null; // To hold the chart instance

  // --- DATA TRANSFORMATION ---
  const transformData = (members: Member[], relationships: Relationship[]) => {
    const personMap = new Map<string, any>();

    // 1. Initialize all members in a map for quick access
    members.forEach((person) => {
      personMap.set(String(person.id), {
        id: String(person.id),
        data: {
          fullName: person.fullName || `${person.firstName} ${person.lastName}`,
          birthYear: person.dateOfBirth ? new Date(person.dateOfBirth).getFullYear() : '',
          deathYear: person.dateOfDeath ? new Date(person.dateOfDeath).getFullYear() : '',
          avatar: getAvatarUrl(person.avatarUrl, person.gender),
          gender: person.gender === Gender.Male ? 'M' : 'F',
        },
        rels: {
          spouses: [],
          children: [],
        },
      });
    });

    // 2. Process relationships to build the tree structure
    relationships.forEach((rel) => {
      const sourcePerson = personMap.get(String(rel.sourceMemberId));
      const targetPerson = personMap.get(String(rel.targetMemberId));

      if (!sourcePerson || !targetPerson) {
        // console.warn('Could not find person for relationship:', rel);
        return; // Skip if a person in the relationship doesn't exist in the member list
      }

      switch (rel.type) {
        case RelationshipType.Wife:
        case RelationshipType.Husband:
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
      }
    });

    return Array.from(personMap.values()).map(person => {
      if (props.rootId && person.id === props.rootId) {
        return { ...person, data: { ...person.data, main: true } };
      }
      return person;
    });
  };

  const renderChart = (currentMembers: Member[]) => {
    if (!chartContainer.value) {
      return;
    }

    nextTick(() => { // Wrap the rendering logic in nextTick
      chartContainer.value!.innerHTML = ''; // Use ! for non-null assertion after check
      const transformedData = transformData(currentMembers, props.relationships);
      if (transformedData.length === 0) {
        chartContainer.value!.innerHTML =
          `<div class="empty-message">${t('familyTree.noMembersMessage')}</div>`;
        chart = null;
        return;
      }

      chart = f3
        .createChart(chartContainer.value!, transformedData) // Use ! for non-null assertion
        .setTransitionTime(1000)
        .setCardXSpacing(150)
        .setCardYSpacing(150);

      chart
        .setCardHtml()
        .setCardDim({ w: 150, h: 200 })
        .setOnCardUpdate(Card());

      let mainIdToUpdate: string | undefined;

      if (props.rootId) {
        // Check if the provided rootId exists in the transformed data
        const foundRootMember = transformedData.find(d => d.id === props.rootId);
        if (foundRootMember) {
          mainIdToUpdate = props.rootId;
        }
      }

      if (!mainIdToUpdate) {
        // Fallback to existing logic if rootId is not provided or not found
        const rootMember = currentMembers.find((m: Member) => m.isRoot);
        if (rootMember) {
          mainIdToUpdate = rootMember.id;
        } else if (transformedData.length > 0) {
          mainIdToUpdate = transformedData[0].id;
        }
      }

      if (mainIdToUpdate) {
        chart.updateMainId(mainIdToUpdate);
        chart.updateTree({
          initial: true // Keep initial: true for re-centering
        });
      } else {
        // Handle case where no mainId can be determined (e.g., empty data)
        console.warn('No main ID could be determined for the family tree chart.');
      }
    });
  };

  // --- CUSTOM CARD RENDERING ---
  function Card() {
    return function (this: HTMLElement, d: CardData) {
      this.innerHTML = `
        <div class="card">
          ${d.data.data.avatar ? getCardInnerImage(d) : getCardInnerText(d)}
        </div>
        `;
      // Re-add the card click listener
      this.addEventListener('click', (e: MouseEvent) => onCardClick(e, d));
    };

    function onCardClick(_: Event, d: CardData) {
      emit('show-member-detail-drawer', d.data.id); // Emit event to show member detail drawer
      chart.updateMainId(d.data.id);
      chart.updateTree({});
    }

    function getCardInnerImage(d: CardData) {
      return `
        <div class="card-image ${getClassList(d).join(' ')}">
          <img src="${d.data.data.avatar}" />
          <div class="card-label">${d.data.data.fullName}</div>
          <div class="card-dates">${d.data.data.birthYear} - ${d.data.data.deathYear}</div>
        </div>
        `;
    }

    function getCardInnerText(d: CardData) {
      return `
        <div class="card-text ${getClassList(d).join(' ')}">
          <div class="card-label">${d.data.data.fullName}</div>
          <div class="card-dates">${d.data.data.birthYear} - ${d.data.data.deathYear}</div>
        </div>
        `;
    }

    function getClassList(d: CardData) {
      const class_list = [];
      if (d.data.data.gender === 'M') class_list.push('card-male');
      else if (d.data.data.gender === 'F') class_list.push('card-female');
      else class_list.push('card-genderless');

      if (d.data.main) class_list.push('card-main');

      return class_list;
    }
  }

  onMounted(() => {
    if (props.familyId) {
      renderChart(props.members);
    }
  });

  onUnmounted(() => {
    if (chart && chartContainer.value) {
      chartContainer.value.innerHTML = '';
      chart = null;
    }
  });

  watch([() => props.familyId, () => props.members, () => props.rootId], ([newFamilyId, newMembers]) => {
    if (newFamilyId) {
      renderChart(newMembers);
    } else {
      renderChart([]);
    }
  }, { deep: true });

  return {
    chartContainer,
    renderChart // Expose renderChart if needed for external triggers
  };
}
