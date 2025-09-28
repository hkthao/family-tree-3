<template>
  <v-container fluid class="fill-height">
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
            style="max-width: 300px"
          ></v-text-field>
          <v-btn @click="exportToPdf" color="primary">Xuất PDF</v-btn>
        </v-toolbar>
        <div ref="chartContainer" class="family-tree-container flex-grow-1"></div>
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

interface Member {
  id: number;
  name: string;
  avatar: string;
  birthYear: number;
  deathYear: number | null;
  parents: number[];
  gender: string; // Added gender
}

// --- REFS & STATE ---
const chartContainer = ref<HTMLDivElement | null>(null);
const searchTerm = ref('');
let chart: any = null; // To hold the chart instance

// --- DATA TRANSFORMATION ---
const transformData = (data: typeof rawData) => {
  const transformedMap = new Map<string, any>(); // Use a map for easier lookup

  // Initialize all persons with basic data and empty rels
  data.forEach(person => {
    transformedMap.set(String(person.id), {
      id: String(person.id),
      data: {
        'Họ và tên': person.name,
        'Năm sinh': person.birthYear,
        'Năm mất': person.deathYear || 'nay',
        'avatar': person.avatar,
        'gender': person.gender, // Added gender
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
  data.forEach(person => {
    const transformedPerson = transformedMap.get(String(person.id));
    if (person.parents && person.parents.length > 0) {
      transformedPerson.rels.father = String(person.parents[0]);
      if (person.parents[1]) {
        transformedPerson.rels.mother = String(person.parents[1]);
      }

      // Add this person as a child to their parents
      person.parents.forEach(parentId => {
        const transformedParent = transformedMap.get(String(parentId));
        if (transformedParent && !transformedParent.rels.children.includes(transformedPerson.id)) {
          transformedParent.rels.children.push(transformedPerson.id);
        }
      });
    }
  });

  // Populate spouses
  // Iterate through all children, if they have two parents, those parents are spouses
  data.forEach(person => {
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

// --- CHART INITIALIZATION ---
onMounted(() => {
  if (chartContainer.value) {
    const transformedData = transformData(rawData);

    chart = f3.createChart(chartContainer.value, transformedData, {
      enableAdmin: false, // Disable editing features
      node_separation: 100, // Adjust spacing
      level_separation: 150, // Adjust spacing
      main_id: '4', // Start from Queen Elizabeth II
    });

    chart.setCardSvg()
      .setCardDisplay([['Họ và tên'], ['Năm sinh', 'Năm mất']]);

    chart.updateTree({ initial: true, ancestry_depth: 100, progeny_depth: 100 });
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

// --- FUNCTIONALITY ---
const exportToPdf = () => {
  if (chartContainer.value) {
    const element = chartContainer.value.querySelector('svg');
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
      chart.store.update.node({id: d.id, data: {...d.data, '[style]': null}})
    })
    chart.updateTree({});
    return;
  }

  // Find matching nodes
  const results = chart.meta.data.filter((d: any) =>
    d.data['Họ và tên'].toLowerCase().includes(query.toLowerCase())
  );

  if (results.length > 0) {
    // Apply highlight style to found nodes
    chart.meta.data.forEach((d: any) => {
      const isMatch = results.some((res: any) => res.id === d.id);
      chart.store.update.node({id: d.id, data: {...d.data, '[style]': isMatch ? 'border: 4px solid #ff5722;' : null}})
    })
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
}

</style>

<style>
.main_svg{
  width: 100% !important;
  height: 100% !important;
}
</style>