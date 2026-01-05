<template>
  <v-card class="mt-2" flat>
    <v-list density="compact">
      <template v-for="(member, index) in generatedData.members" :key="`member-${index}`">
        <v-list-item>
          <template v-slot:prepend>
            <v-icon>mdi-account</v-icon>
          </template>
          <v-list-item-title>{{ member.firstName }} {{ member.lastName }}</v-list-item-title>
          <v-list-item-subtitle>
            {{ member.gender === Gender.Male ? t('common.male') : t('common.female') }}
            <template v-if="member.dateOfBirth">, {{ t('common.born') }} {{ new Date(member.dateOfBirth).getFullYear()
              }}</template>
            <template v-if="member.dateOfDeath">, {{ t('common.died') }} {{ new Date(member.dateOfDeath).getFullYear()
              }}</template>
          </v-list-item-subtitle>
          <template v-slot:append>
            <v-btn icon size="small" variant="text" @click="generatedDataStore.setMemberToAdd(member)">
              <v-icon>mdi-plus-circle-outline</v-icon>
            </v-btn>
          </template>
        </v-list-item>
        <v-divider v-if="index < generatedData.members.length - 1 || generatedData.events.length > 0 || (generatedData.locations?.length || 0) > 0"></v-divider>
      </template>

      <template v-for="(event, index) in generatedData.events" :key="`event-${index}`">
        <v-list-item>
          <template v-slot:prepend>
            <v-icon>mdi-calendar-month</v-icon>
          </template>
          <v-list-item-title>{{ event.name }}</v-list-item-title>
          <v-list-item-subtitle>
            <template v-if="event.description">{{ event.description }}</template>
            <template v-if="event.solarDate">, {{ t('common.date') }} {{ new Date(event.solarDate).toLocaleDateString()
              }}</template>
          </v-list-item-subtitle>
          <template v-slot:append>
            <v-btn icon size="small" variant="text" @click="generatedDataStore.setEventToAdd(event)">
              <v-icon>mdi-plus-circle-outline</v-icon>
            </v-btn>
          </template>
        </v-list-item>
        <v-divider v-if="index < generatedData.events.length - 1 || (generatedData.locations?.length || 0) > 0"></v-divider>
      </template>

      <!-- NEW: Locations Display -->
      <template v-for="(location, index) in generatedData.locations || []" :key="`location-${index}`">
        <v-list-item>
          <template v-slot:prepend>
            <v-icon>mdi-map-marker</v-icon>
          </template>
          <v-list-item-title>{{ location.location.name }}</v-list-item-title>
          <v-list-item-subtitle>
            {{ location.location.address || `Lat: ${location.location.latitude}, Lng: ${location.location.longitude}` }}
          </v-list-item-subtitle>
          <template v-slot:append>
            <v-btn icon size="small" variant="text" @click="emit('add-generated-location', location)">
              <v-icon>mdi-plus-circle-outline</v-icon>
            </v-btn>
          </template>
        </v-list-item>
        <v-divider v-if="index < (generatedData.locations?.length || 0) - 1"></v-divider>
      </template>
      <!-- END NEW -->
    </v-list>
  </v-card>
</template>

<script setup lang="ts">
import type { PropType } from 'vue';
import type { CombinedAiContentDto } from '@/types'; // Import FamilyLocation
import { Gender } from '@/types/member.d';
import { useI18n } from 'vue-i18n';
import { useGeneratedDataStore } from '@/stores/generatedData.store'; // Import the new store

defineProps({
  generatedData: {
    type: Object as PropType<CombinedAiContentDto>,
    required: true,
  },
  familyId: {
    type: String,
    required: true,
  },
});

const emit = defineEmits([
  'add-generated-member',
  'add-generated-event',
  'add-generated-location', // NEW: Add emit for locations
]);

const { t } = useI18n();
const generatedDataStore = useGeneratedDataStore(); // Use the store
</script>