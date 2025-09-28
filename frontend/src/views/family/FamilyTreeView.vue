<template>
  <v-container fluid class="fill-height" style="min-height: 100vh">
    <v-row class="fill-height">
      <v-col cols="12" class="d-flex flex-column">
        <v-toolbar dense flat>
          <v-toolbar-title>Cây Gia Phả</v-toolbar-title>
          <v-spacer></v-spacer>
          <v-text-field
            v-model="searchTerm"
            label="Tìm kiếm thành viên..."
            variant="solo-inverted"
            density="compact"
            hide-details
            clearable
            class="ml-4 mr-2"
          ></v-text-field>
          <v-btn @click="exportToPdf" color="primary">Xuất PDF</v-btn>
        </v-toolbar>
        <div
          ref="chartContainer"
          class="family-tree-container f3 flex-grow-1"
        ></div>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue';
import f3 from 'family-chart'; // Corrected import
import html2pdf from 'html2pdf.js';
import rawData from '@/data/family-data.json';
import 'family-chart/styles/family-chart.css';

const chartContainer = ref<HTMLDivElement | null>(null);
const searchTerm = ref('');
let chart: any = null; // To hold the chart instance

const transformData = (data: typeof rawData) => {
  const transformedMap = new Map<string, any>(); // Use a map for easier lookup

  // Initialize all persons with basic data and empty rels
  data.forEach((person) => {
    transformedMap.set(String(person.id), {
      id: String(person.id),
      data: {
        'Họ và tên': person.name,
        'Năm sinh': person.birthYear,
        'Năm mất': person.deathYear || 'nay',
        avatar: person.avatar,
        gender: person.gender, // Added gender
      },
      rels: {
        spouses: [],
        children: [],
        father: undefined,
        mother: undefined,
      },
    });
  });

  // Populate father/mother for children, and children for parents
  data.forEach((person) => {
    const transformedPerson = transformedMap.get(String(person.id));
    if (person.parents && person.parents.length > 0) {
      transformedPerson.rels.father = String(person.parents[0]);
      if (person.parents[1]) {
        transformedPerson.rels.mother = String(person.parents[1]);
      }

      // Add this person as a child to their parents
      person.parents.forEach((parentId) => {
        const transformedParent = transformedMap.get(String(parentId));
        if (
          transformedParent &&
          !transformedParent.rels.children.includes(transformedPerson.id)
        ) {
          transformedParent.rels.children.push(transformedPerson.id);
        }
      });
    }
  });

  // Populate spouses
  // Iterate through all children, if they have two parents, those parents are spouses
  data.forEach((person) => {
    if (person.parents && person.parents.length === 2) {
      const parent1Id = String(person.parents[0]);
      const parent2Id = String(person.parents[1]);

      const transformedParent1 = transformedMap.get(parent1Id);
      const transformedParent2 = transformedMap.get(parent2Id);

      if (transformedParent1 && transformedParent2) {
        if (!transformedParent1.rels.spouses.includes(parent2Id)) {
          transformedParent1.rels.spouses.push(parent2Id);
        }
        if (!transformedParent2.rels.spouses.includes(parent1Id)) {
          transformedParent2.rels.spouses.push(parent1Id);
        }
      }
    }
  });

  return Array.from(transformedMap.values());
};

onMounted(() => {
  if (chartContainer.value) {
    const transformedData = transformData(rawData);

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
      ancestry_depth: 100,
      progeny_depth: 100,
    });
  }
});

onUnmounted(() => {
  if (chart) {
    // family-chart does not have a dedicated destroy method, so we clear the container
    if (chartContainer.value) {
      chartContainer.value.innerHTML = '';
    }
    chart = null;
  }
});

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

// --- FUNCTIONALITY ---
const exportToPdf = () => {
  if (chartContainer.value) {
    const element = chartContainer.value.querySelector('#f3Canvas'); // Target the f3Canvas div
    if (element) {
      const options = {
        margin: 10,
        filename: 'family-tree.pdf',
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2, useCORS: true },
        jsPDF: { unit: 'mm', format: 'a2', orientation: 'landscape' },
      };
      html2pdf().from(element).set(options).save();
    }
  }
};

watch(searchTerm, (query) => {
  if (!chart) return;

  // Reset styles if search is cleared
  if (!query) {
    chart.meta.data.forEach((d: any) => {
      chart.store.update.node({
        id: d.id,
        data: { ...d.data, '[style]': null },
      });
    });
    chart.updateTree({});
    return;
  }

  // Find matching nodes
  const results = chart.meta.data.filter((d: any) =>
    d.data['Họ và tên'].toLowerCase().includes(query.toLowerCase()),
  );

  if (results.length > 0) {
    // Apply highlight style to found nodes
    chart.meta.data.forEach((d: any) => {
      const isMatch = results.some((res: any) => res.id === d.id);
      chart.store.update.node({
        id: d.id,
        data: {
          ...d.data,
          '[style]': isMatch ? 'border: 4px solid #ff5722;' : null,
        },
      });
    });
    chart.updateTree({});

    // Focus on the first found person
    const firstResultId = results[0].id;
    chart.focus(firstResultId);
  }
});
</script>

<style scoped>
.family-tree-container {
  width: 100%;
  height: 100%;
  overflow: hidden;
  /* background-color: white; */
}
</style>

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
