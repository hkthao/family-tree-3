<template>
  <v-row class="fill-height">
    <v-col cols="12" class="d-flex flex-column">
      <v-toolbar dense flat>
        <v-toolbar-title>Cây Gia Phả</v-toolbar-title>
        <v-spacer></v-spacer>
        <FamilyAutocomplete
          class="mt-2"
          label="Lọc theo gia đình..."
          v-model="selectedFamilyId"
          @update:model-value="handleFamilySelect"
        />
      </v-toolbar>
      <div ref="chartContainer" class="f3 flex-grow-1"></div>
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, computed } from 'vue';
import f3 from 'family-chart'; // Corrected import
import 'family-chart/styles/family-chart.css';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import { useMemberStore } from '@/stores/member.store';
import type { Member } from '@/types/family';
import { Gender } from '@/types/gender';

const chartContainer = ref<HTMLDivElement | null>(null);
const selectedFamilyId = ref<string | null>(null);
let chart: any = null; // To hold the chart instance
const memberStore = useMemberStore();

// --- DATA TRANSFORMATION ---
const transformData = (members: Member[]) => {
  const transformedMap = new Map<string, any>();

  // First pass: Initialize all persons with basic data and empty rels
  members.forEach((person) => {
    transformedMap.set(String(person.id), {
      id: String(person.id),
      data: {
        'Họ và tên': person.fullName || `${person.firstName} ${person.lastName}`,
        'Năm sinh': person.dateOfBirth?.getFullYear(),
        'Năm mất': person.dateOfDeath?.getFullYear() || ' ',
        avatar: person.avatarUrl,
        gender: person.gender == Gender.Male ? 'M' : 'F', // Map to 'M', 'F', 'U'
      },
      rels: {
        spouses: [],
        children: [],
        father: person.fatherId ? String(person.fatherId) : undefined, // Use fatherId
        mother: person.motherId ? String(person.motherId) : undefined, // Use motherId
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
      '<div style="text-align: center; padding-top: 50px;">Không có thành viên nào để hiển thị.</div>';
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
  renderChart(memberStore.items);
});

const handleFamilySelect = async (familyId: string | null) => {
  let membersToRender: Member[] = [];
  if (familyId) {
    membersToRender = await memberStore.getMembersByFamilyId(familyId);
  } 
  renderChart(membersToRender);
};

// --- CUSTOM CARD RENDERING ---
function Card() {
  return function (this: HTMLElement, d: any) {
    // Update innerHTML instead of outerHTML, and let the library handle positioning
    this.innerHTML = `
      <div class="card">
        ${d.data.data.avatar ? getCardInnerImage(d) : getCardInnerText(d)}
      </div>
      `;
    this.addEventListener('click', (e: Event) => onCardClick(e, d));
  };

  function onCardClick(e: Event, d: any) {
    chart.updateMainId(d.data.id);
    chart.updateTree({});
  }

  function getCardInnerImage(d: any) {
    return `
      <div class="card-image ${getClassList(d).join(' ')}">
        <img src="${d.data.data.avatar}" />
        <div class="card-label">${d.data.data['Họ và tên']}</div>
        <div class="card-dates">${d.data.data['Năm sinh']} - ${d.data.data['Năm mất']}</div>
      </div>
      `;
  }

  function getCardInnerText(d: any) {
    return `
      <div class="card-text ${getClassList(d).join(' ')}">
        <div class="card-label">${d.data.data['Họ và tên']}</div>
        <div class="card-dates">${d.data.data['Năm sinh']} - ${d.data.data['Năm mất']}</div>
      </div>
      `;
  }

  function getClassList(d: any) {
    const class_list = [];
    if (d.data.data.gender === 'M') class_list.push('card-male');
    else if (d.data.data.gender === 'F') class_list.push('card-female');
    else class_list.push('card-genderless');

    if (d.data.main) class_list.push('card-main');

    return class_list;
  }
}
</script>

<style>
.main_svg {
  width: 100% !important;
  height: 100% !important;
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
