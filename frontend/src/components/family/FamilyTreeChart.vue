<template>
  <div ref="chartContainer" class="f3 flex-grow-1"></div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue';
import f3 from 'family-chart'; // Corrected import
import 'family-chart/styles/family-chart.css';
import { useMemberStore } from '@/stores/member.store';
import type { Member } from '@/types/family';
import { Gender } from '@/types/gender';
import { useI18n } from 'vue-i18n';

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
// eslint-disable-next-line @typescript-eslint/no-explicit-any
let chart: any = null; // To hold the chart instance, 'any' is used due to the library's lack of exported types
const memberStore = useMemberStore();

// --- DATA TRANSFORMATION ---
const transformData = (members: Member[]) => {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const transformedMap = new Map<string, any>();

  // First pass: Initialize all persons with basic data and empty rels
  members.forEach((person) => {
    transformedMap.set(String(person.id), {
      id: String(person.id),
      data: {
        [t('familyTree.fullName')]: person.fullName || `${person.firstName} ${person.lastName}`,
        [t('familyTree.birthYear')]: person.dateOfBirth?.getFullYear(),
        [t('familyTree.deathYear')]: person.dateOfDeath?.getFullYear() || ' ',
        avatar: person.avatarUrl,
        gender: person.gender == Gender.Male ? 'M' : 'F', // Map to 'M', 'F', 'U'
      },
      rels: {
        spouses: [],
        children: [],
        father: person.fatherId ? String(person.fatherId) : undefined,
        mother: person.motherId ? String(person.motherId) : undefined,
      },
    });
  });

  // Second pass: Populate children and spouses
  members.forEach((person) => {
    const transformedPerson = transformedMap.get(String(person.id));

    // Populate children for parents
    // Iterate through all members to find their children
    members.forEach(child => {
      if (child.fatherId === person.id || child.motherId === person.id) {
        if (transformedPerson && !transformedPerson.rels.children.includes(String(child.id))) {
          transformedPerson.rels.children.push(String(child.id));
        }
      }
    });

    // Populate spouses
    if (person.spouseId) {
      if (transformedPerson && !transformedPerson.rels.spouses.includes(String(person.spouseId))) {
        transformedPerson.rels.spouses.push(String(person.spouseId));
      }
    }
  });

  return Array.from(transformedMap.values());
};

const renderChart = (dataToRender: Member[], mainId: string | null = null) => {
  if (!chartContainer.value) return;

  // Clear previous chart
  chartContainer.value.innerHTML = '';

  const transformedData = transformData(dataToRender);

  if (transformedData.length === 0) {
    // Display a message if no data
    chartContainer.value.innerHTML =
      `<div style="display: flex; justify-content: center; align-items: center; height: 100%; width: 100%; font-size: 1.2em; color: #666;">${t('familyTree.noMembersMessage')}</div>`;
    chart = null;
    return;
  }

  // Determine main_id for the chart
  let chartMainId = mainId || transformedData[0].id;
  if (!transformedData.some((d) => d.id === chartMainId)) {
    chartMainId = transformedData[0].id; // Fallback if mainId is not in current data
  }

  chart = f3
    .createChart(chartContainer.value, transformedData)
    .setTransitionTime(1000)
    .setCardXSpacing(200) // Adjust spacing (card width 150 + 50 padding)
    .setCardYSpacing(250); // Adjust spacing (card height 200 + 50 padding)

  chart
    .setCardHtml()
    .setCardDim({ w: 150, h: 200 }) // Communicate custom card dimensions
    .setOnCardUpdate(Card());

  chart.updateTree({
    initial: true,
    main_id: chartMainId,
    ancestry_depth: 100,
    progeny_depth: 100,
  });
};

onMounted(async () => {
  if (props.familyId) {
    const members = await memberStore.getMembersByFamilyId(props.familyId);
    renderChart(members);
  } 
});

onUnmounted(() => {
  if (chart) {
    if (chartContainer.value) {
      chartContainer.value.innerHTML = '';
    }
    chart = null;
  }
});

// --- CUSTOM CARD RENDERING ---
function Card() {
  return function (this: HTMLElement, d: CardData) {
    // Update innerHTML instead of outerHTML, and let the library handle positioning
    this.innerHTML = `
      <div class="card">
        ${d.data.data.avatar ? getCardInnerImage(d) : getCardInnerText(d)}
      </div>
      `;
    this.addEventListener('click', (e: Event) => onCardClick(e, d));
  };

  function onCardClick(e: Event, d: CardData) {
    chart.updateMainId(d.data.id);
    chart.updateTree({});
  }

  function getCardInnerImage(d: CardData) {
    return `
      <div class="card-image ${getClassList(d).join(' ')}">
        <img src="${d.data.data.avatar}" />
        <div class="card-label">${d.data.data[t('familyTree.fullName')]}</div>
        <div class="card-dates">${d.data.data[t('familyTree.birthYear')]} - ${d.data.data[t('familyTree.deathYear')]}</div>
      </div>
      `;
  }

  function getCardInnerText(d: CardData) {
    return `
      <div class="card-text ${getClassList(d).join(' ')}">
        <div class="card-label">${d.data.data[t('familyTree.fullName')]}</div>
        <div class="card-dates">${d.data.data[t('familyTree.birthYear')]} - ${d.data.data[t('familyTree.deathYear')]}</div>
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
  let membersToRender: Member[] = [];
  if (newFamilyId) {
    membersToRender = await memberStore.getMembersByFamilyId(newFamilyId);
  } 
  renderChart(membersToRender);
});
</script>

<style>
.main_svg {
  width: 100% !important;
  height: 80vh !important;
}
.f3{
  cursor: pointer;
}

.f3 div.card {
  cursor: pointer;
  pointer-events: auto;
  color: #fff;
  position: relative;
  margin-top: -50px;
  margin-left: -50px;
}

.f3 div.card-image {
  border-radius: 50%;
  padding: 5px;
  width: 90px;
  height: 90px;
}

.f3 div.card-image div.card-label {
  position: absolute;
  bottom: -12px;
  left: 50%;
  transform: translate(-50%, 50%);
  max-width: 150%;
  text-align: center;
  background-color: rgba(0, 0, 0, 0.5);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  border-radius: 3px;
  padding: 0 5px;
}

.f3 div.card-image div.card-dates {
  position: absolute;
  bottom: -32px;
  left: 50%;
  transform: translate(-50%, 50%);
  max-width: 150%;
  text-align: center;
  background-color: rgba(0, 0, 0, 0.5);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  border-radius: 3px;
  padding: 0 5px;
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
}

.f3 div.card > div {
  transition: transform 0.2s ease-in-out;
}

.f3 div.card:hover > div {
  transform: scale(1.1);
}

.f3 div.card-main {
  transform: scale(1.2) !important;
}

.f3 div.card-female {
  background-color: rgb(196, 138, 146);
}
.f3 div.card-male {
  background-color: rgb(120, 159, 172);
}
.f3 div.card-genderless {
  background-color: lightgray;
}
.f3 div.card-main {
  box-shadow: 0 0 20px 0 rgba(0, 0, 0, 0.8);
}
</style>
