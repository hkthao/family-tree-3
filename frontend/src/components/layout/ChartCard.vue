<template>
  <v-card elevation="2">
    <v-card-title>{{ title }}</v-card-title>
    <v-card-subtitle v-if="subtitle">{{ subtitle }}</v-card-subtitle>
    <v-card-text>
      <v-responsive :height="150">
        <v-row v-if="type === 'bar'" class="align-end fill-height">
          <v-col
            v-for="(bar, i) in chartData"
            :key="i"
            class="d-flex justify-center pa-0"
          >
            <v-responsive
              :height="bar.value + '%'"
              class="bg-primary rounded-t"
              max-width="20"
            ></v-responsive>
          </v-col>
        </v-row>
        <v-row v-else-if="type === 'line'" class="align-end fill-height">
          <v-col
            v-for="(point, i) in chartData"
            :key="i"
            class="d-flex justify-center align-end pa-0"
          >
            <v-icon color="primary" size="small">mdi-circle</v-icon>
          </v-col>
        </v-row>
        <div v-else-if="type === 'progress'" class="d-flex justify-center align-center fill-height">
          <v-progress-circular
            :model-value="chartData[0].value"
            :size="100"
            :width="10"
            color="primary"
          >
            <template v-slot:default>
              {{ chartData[0].value }}%
            </template>
          </v-progress-circular>
        </div>
        <div v-else class="fill-height d-flex align-center justify-center">
          <v-alert type="info" variant="outlined">Chart placeholder for {{ type }} chart.</v-alert>
        </div>
      </v-responsive>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
interface ChartDataItem {
  label: string;
  value: number;
}

interface Props {
  title: string;
  subtitle?: string;
  type: 'bar' | 'line' | 'progress' | 'other';
  chartData: ChartDataItem[];
}

defineProps<Props>();
</script>