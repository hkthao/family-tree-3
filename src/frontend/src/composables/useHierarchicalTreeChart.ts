import { ref, onMounted, onUnmounted, watch, computed, nextTick } from 'vue';
import f3 from 'family-chart';
import 'family-chart/styles/family-chart.css';
import { useI18n } from 'vue-i18n';
import type { Member } from '@/types';
import type { Relationship } from '@/types';
import { Gender, RelationshipType } from '@/types';

import maleAvatar from '@/assets/images/male_avatar.png';
import femaleAvatar from '@/assets/images/female_avatar.png';

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
  familyId: string | null,
  members: Member[],
  relationships: Relationship[],
  emit: (event: 'add-member' | 'edit-member' | 'delete-member' | 'add-father' | 'add-mother' | 'add-child', ...args: any[]) => void
) {
  const { t } = useI18n();
  const chartContainer = ref<HTMLDivElement | null>(null);
  let chart: any = null; // To hold the chart instance

  // Bottom Sheet State (kept here for now, might be moved if it's purely UI related to the chart)
  const showBottomSheet = ref(false);
  const selectedMemberId = ref<string | null>(null);

  const openBottomSheet = (_: MouseEvent, memberId: string) => {
    showBottomSheet.value = false; // Close any existing sheet
    selectedMemberId.value = memberId;
    nextTick(() => {
      showBottomSheet.value = true;
    });
  };

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
          avatar: person.avatarUrl || (person.gender === Gender.Male ? maleAvatar : femaleAvatar),
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
            targetPerson.rels.spouses.push(targetPerson.id);
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

    return Array.from(personMap.values());
  };

  const renderChart = (currentMembers: Member[], currentRelationships: Relationship[]) => {
    if (!chartContainer.value) return;

    chartContainer.value.innerHTML = '';
    const transformedData = transformData(currentMembers, currentRelationships);
    if (transformedData.length === 0) {
      chartContainer.value.innerHTML =
        `<div class="empty-message">${t('familyTree.noMembersMessage')}</div>`;
      chart = null;
      return;
    }

    chart = f3
      .createChart(chartContainer.value, transformedData)
      .setTransitionTime(1000)
      .setCardXSpacing(200)
      .setCardYSpacing(250);

    chart
      .setCardHtml()
      .setCardDim({ w: 150, h: 200 })
      .setOnCardUpdate(Card());

    const rootMember = currentMembers.find((m: Member) => m.isRoot);
    if (rootMember) {
      chart.updateMainId(rootMember.id);
      chart.updateTree({
        initial: true
      });
    }
    else {
      chart.updateMainId(transformedData[0].id);
      chart.updateTree({
        initial: true
      });
    }
  };

  // --- CUSTOM CARD RENDERING ---
  function Card() {
    return function (this: HTMLElement, d: CardData) {
      const menuButtonId = `menu-button-${d.data.id}`; // Generate unique ID
      this.innerHTML = `
        <div class="card">
          ${d.data.data.avatar ? getCardInnerImage(d) : getCardInnerText(d)}
          <div class="card-menu-button" id="${menuButtonId}">
            <i class="mdi mdi-dots-vertical"></i>
          </div>
        </div>
        `;
      // Re-add the card click listener
      this.addEventListener('click', (e: MouseEvent) => onCardClick(e, d));

      // Attach click listener directly to the menu button using its unique ID
      const menuButton = this.querySelector(`#${menuButtonId}`);
      if (menuButton) {
        menuButton.addEventListener('click', (e: Event) => {
          e.stopPropagation(); // Prevent card click from triggering
          openBottomSheet(e as MouseEvent, d.data.id);
        });
      }
    };

    function onCardClick(_: Event, d: CardData) {
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
    if (familyId) {
      renderChart(members, relationships);
    }
  });

  onUnmounted(() => {
    if (chart && chartContainer.value) {
      chartContainer.value.innerHTML = '';
      chart = null;
    }
  });

  watch([() => familyId, () => members, () => relationships], () => {
    if (familyId) {
      renderChart(members, relationships);
    } else {
      renderChart([], []);
    }
  }, { deep: true });

  return {
    chartContainer,
    showBottomSheet,
    selectedMemberId,
    openBottomSheet,
    renderChart // Expose renderChart if needed for external triggers
  };
}
