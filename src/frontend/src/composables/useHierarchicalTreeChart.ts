import { ref, onMounted, onUnmounted, watch, nextTick } from 'vue';
import f3 from 'family-chart';
import 'family-chart/styles/family-chart.css';
import type { Member } from '@/types';
import { Gender } from '@/types';

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
  props: { familyId: string | null; members: Member[] },
  emit: (event: 'show-member-detail-drawer', ...args: any[]) => void,
  t: (key: string) => string
) {
  const chartContainer = ref<HTMLDivElement | null>(null);
  let chart: any = null; // To hold the chart instance

  // --- DATA TRANSFORMATION ---
  const transformData = (members: Member[]) => {
    const personMap = new Map<string, any>();

    // 1. Initialize all members in a map for quick access
    members.forEach((person) => {
      const personData: any = {
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
      };

      if (person.fatherId) {
        personData.rels.father = String(person.fatherId);
      }
      if (person.motherId) {
        personData.rels.mother = String(person.motherId);
      }
      // Handle spouses
      if (person.husbandId) {
        personData.rels.spouses.push(String(person.husbandId));
      }
      if (person.wifeId) {
        personData.rels.spouses.push(String(person.wifeId));
      }

      personMap.set(String(person.id), personData);
    });

    // 2. Build children relationships (since parents are set, children can be derived)
    members.forEach((person) => {
      if (person.fatherId) {
        const father = personMap.get(String(person.fatherId));
        if (father && !father.rels.children.includes(String(person.id))) {
          father.rels.children.push(String(person.id));
        }
      }
      if (person.motherId) {
        const mother = personMap.get(String(person.motherId));
        if (mother && !mother.rels.children.includes(String(person.id))) {
          mother.rels.children.push(String(person.id));
        }
      }
    });

    return Array.from(personMap.values());
  };

  const renderChart = (currentMembers: Member[]) => {
    if (!chartContainer.value) {
      return;
    }

    nextTick(() => { // Wrap the rendering logic in nextTick
      chartContainer.value!.innerHTML = ''; // Use ! for non-null assertion after check
      const transformedData = transformData(currentMembers);
      if (transformedData.length === 0) {
        chartContainer.value!.innerHTML =
          `<div class="empty-message">${t('familyTree.noMembersMessage')}</div>`;
        chart = null;
        return;
      }

      chart = f3
        .createChart(chartContainer.value!, transformedData) // Use ! for non-null assertion
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

  watch([() => props.familyId, () => props.members], ([newFamilyId, newMembers]) => {
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
