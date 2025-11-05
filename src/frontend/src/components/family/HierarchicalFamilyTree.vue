<template>
  <div class="hierarchical-tree-container">
    <div ref="chartContainer" class="f3 flex-grow-1" data-testid="family-tree-canvas"></div>
    <div class="legend">
      <div class="legend-item">
        <span class="legend-color-box legend-male"></span>
        <span>{{ t('member.gender.male') }}</span>
      </div>
      <div class="legend-item">
        <span class="legend-color-box legend-female"></span>
        <span>{{ t('member.gender.female') }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, computed } from 'vue';
import f3 from 'family-chart';
import 'family-chart/styles/family-chart.css';
import { useTreeVisualizationStore } from '@/stores/tree-visualization.store';
import type { Member } from '@/types';
import type { Relationship } from '@/types';
import { Gender, RelationshipType } from '@/types';
import { useI18n } from 'vue-i18n';

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

const { t } = useI18n();

const props = defineProps({
  familyId: { type: String, default: null },
});

const chartContainer = ref<HTMLDivElement | null>(null);
let chart: any = null; // To hold the chart instance

const treeVisualizationStore = useTreeVisualizationStore();

const members = computed(() => treeVisualizationStore.getMembers(props.familyId));
const relationships = computed(() => treeVisualizationStore.getRelationships(props.familyId));

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

  return Array.from(personMap.values());
};

const renderChart = (members: Member[], relationships: Relationship[]) => {
  if (!chartContainer.value) return;

  chartContainer.value.innerHTML = '';
  const transformedData = transformData(members, relationships);
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

  const rootMember = members.find((m: Member) => m.isRoot);
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

const initialize = async (familyId: string) => {
  if (familyId) {
    await treeVisualizationStore.fetchTreeData(familyId);
    renderChart(members.value, relationships.value);
  }
};

onMounted(async () => {
  if (props.familyId) {
    await initialize(props.familyId);
  }
});

onUnmounted(() => {
  if (chart && chartContainer.value) {
    chartContainer.value.innerHTML = '';
    chart = null;
  }
});

// --- CUSTOM CARD RENDERING ---
function Card() {
  return function (this: HTMLElement, d: CardData) {
    this.innerHTML = `
      <div class="card">
        ${d.data.data.avatar ? getCardInnerImage(d) : getCardInnerText(d)}
      </div>
      `;
    this.addEventListener('click', (e: Event) => onCardClick(e, d));
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

watch(() => props.familyId, async (newFamilyId) => {
  if (newFamilyId) {
    await initialize(newFamilyId);
  } else {
    renderChart([], []);
  }
});
</script>

<style>
.hierarchical-tree-container {
  position: relative;
  width: 100%;
  height: 80vh;
}

.main_svg {
  width: 100% !important;
  height: 100% !important;
}

.main_svg path.link {
  stroke: rgb(var(--v-theme-on-surface));
}

.f3 {
  cursor: pointer;
  height: 100%;
}

.f3 div.card {
  cursor: pointer;
  pointer-events: auto;
  color: rgb(var(--v-theme-on-surface));
  position: relative;
  margin-top: -30px;
  margin-left: -30px;
}

.f3 div.card-image {
  border-radius: 50%;
  padding: 5px;
  width: 60px;
  height: 60px;
}

.f3 div.card-image div.card-label,
.f3 div.card-image div.card-dates {
  position: absolute;
  left: 50%;
  transform: translateX(-50%) !important;
  max-width: 150%;
  text-align: center;
  background-color: rgba(var(--v-theme-surface-variant-rgb), 0.7);
  color: rgb(var(--v-theme-on-surface));
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  border-radius: 3px;
  padding: 1px 6px;
  font-size: 12px;
}

.f3 div.card-image div.card-dates {
  bottom: -20px;
}

.f3 div.card-image div.card-label {
  bottom: -35px;
}

.f3 div.card-image img {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  object-fit: cover;
}

.f3 div.card-text {
  padding: 5px;
  border-radius: 3px;
  width: 120px;
  height: 70px;
  overflow: hidden;
  line-height: 1.2;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  text-align: center;
}

.f3 div.card-text .card-label {
  font-weight: bold;
  margin-bottom: 4px;
}

.f3 div.card>div {
  transition: transform 0.2s ease-in-out;
}

.f3 div.card:hover>div {
  transform: scale(1.1);
}

.f3 div.card-main>div {
  transform: scale(1.2) !important;
}

.f3 div.card-female {
  background-color: rgb(var(--v-theme-secondary));
}

.f3 div.card-male {
  background-color: rgb(var(--v-theme-primary));
}

.f3 div.card-genderless {
  background-color: lightgray;
}

.f3 div.card-main {
  box-shadow: 0 0 20px 0 rgba(0, 0, 0, 0.8);
}

.empty-message {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  flex-direction: column;
  height: 100%;
}

/* Legend Styles */
.legend {
  position: absolute;
  top: 20px;
  left: 20px;
  background: rgba(var(--v-theme-surface-variant-rgb), 0.8);
  padding: 10px;
  border-radius: 5px;
  color: rgb(var(--v-theme-on-surface));
}

.legend-item {
  display: flex;
  align-items: center;
  margin-bottom: 5px;
}

.legend-item:last-child {
  margin-bottom: 0;
}

.legend-color-box {
  width: 15px;
  height: 15px;
  margin-right: 8px;
  border-radius: 3px;
  border: 1px solid #ccc;
}

.legend-male {
  background-color: rgb(var(--v-theme-primary));
}

.legend-female {
  background-color: rgb(var(--v-theme-secondary));
}
</style>