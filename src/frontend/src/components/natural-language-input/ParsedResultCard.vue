<template>
  <v-card class="border" :elevation="0">
    <v-card-item>
      <v-card-title class="text-h6 text-center">
        <v-icon :icon="cardIcon" class="mr-2"></v-icon> {{ title }}
      </v-card-title>
    </v-card-item>

    <v-card-text class="py-0">
      <v-chip-group column>
        <v-chip v-for="(value, key) in details" size="small" :key="key" :color="getChipColor(key)">
          <strong>{{ key }}:</strong> {{ value }}
        </v-chip>
      </v-chip-group>

      <v-alert v-if="item.errorMessage" type="error" density="compact" class="mt-2">
        {{ item.errorMessage }}
      </v-alert>
    </v-card-text>

    <v-card-actions >
      <v-spacer></v-spacer>
      <v-btn color="red" @click="deleteItem" size="small">{{ t('common.delete') }}</v-btn>
      <v-btn color="primary" @click="saveItem" :disabled="!!item.errorMessage" size="small">{{ t('common.save')
        }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useNaturalLanguageStore } from '@/stores/naturalLanguage.store';
import type { MemberDataDto, EventDataDto } from '@/types/natural-language.d';

const props = defineProps<{
  item: MemberDataDto | EventDataDto;
  type: 'member' | 'event';
  allMembers: MemberDataDto[]; // New prop
}>();

const emit = defineEmits(['delete']);

const { t } = useI18n();

const title = computed(() => {
  if (props.type === 'member') {
    return (props.item as MemberDataDto).fullName;
  }
  const eventType = (props.item as EventDataDto).type.toLowerCase();
  return t(`event.type.${eventType}`);
});

const cardIcon = computed(() => {
  if (props.type === 'member') {
    return 'mdi-account-circle';
  }
  return 'mdi-calendar-text';
});

const getChipColor = (key: string) => {
  // Use translated keys for comparison
  if (key === t('member.form.dateOfBirth') || key === t('member.form.dateOfDeath') || key === t('event.form.date')) {
    return 'info'; // Blue for dates
  }
  if (key === t('member.form.gender')) {
    return 'purple'; // Purple for gender
  }
  if (key === t('relationship.type.father') || key === t('relationship.type.mother') ||
      key === t('relationship.type.husband') || key === t('relationship.type.wife')) {
    return 'success'; // Green for relationships
  }
  return 'grey'; // Default color
};

const details = computed(() => {
  const detailsObj: Record<string, any> = {};

  const formatDate = (dateString: string | null | undefined) => {
    if (!dateString) return '';
    try {
      // Attempt to parse as YYYY-MM-DD first, then try other formats
      const date = new Date(dateString);
      if (!isNaN(date.getTime())) {
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();
        return `${day}/${month}/${year}`;
      }
    } catch (e) {
      // Fallback to original string if parsing fails
    }
    return dateString; // Return original string if it's not a parsable date
  };

  const getMemberFullName = (memberId: string | null | undefined) => {
    if (!memberId) return '';
    const member = props.allMembers.find(m => m.id === memberId);
    return member ? member.fullName : '';
  };

  if (props.type === 'member') {
    const member = props.item as MemberDataDto;
    if (member.dateOfBirth) detailsObj[t('member.form.dateOfBirth')] = formatDate(member.dateOfBirth);
    if (member.dateOfDeath) detailsObj[t('member.form.dateOfDeath')] = formatDate(member.dateOfDeath);
    if (member.gender) {
      const translatedGender = t(`member.gender.${member.gender.toLowerCase()}`);
      detailsObj[t('member.form.gender')] = translatedGender;
    }

    // Add relationship information
    if (member.fatherId) detailsObj[t('relationship.type.father')] = getMemberFullName(member.fatherId);
    if (member.motherId) detailsObj[t('relationship.type.mother')] = getMemberFullName(member.motherId);
    if (member.husbandId) detailsObj[t('relationship.type.husband')] = getMemberFullName(member.husbandId);
    if (member.wifeId) detailsObj[t('relationship.type.wife')] = getMemberFullName(member.wifeId);

  } else {
    const event = props.item as EventDataDto;
    detailsObj[t('event.form.description')] = event.description;
    if (event.date) detailsObj[t('event.form.date')] = formatDate(event.date);
    if (event.location) detailsObj[t('event.form.location')] = event.location;
  }
  return detailsObj;
});

const deleteItem = () => {
  emit('delete');
};

const naturalLanguageStore = useNaturalLanguageStore();

const saveItem = async () => {
  if (props.type === 'member') {
    await naturalLanguageStore.saveMember(props.item as MemberDataDto);
  } else {
    await naturalLanguageStore.saveEvent(props.item as EventDataDto);
  }
  emit('delete'); // Remove from list after saving
};
</script>
