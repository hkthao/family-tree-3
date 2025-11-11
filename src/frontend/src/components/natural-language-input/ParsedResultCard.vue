<template>
  <v-card class="mt-4 border" elevation="0">
    <v-card-item>
      <v-card-title class="text-h6">{{ title }}</v-card-title>
    </v-card-item>

    <v-card-text class="py-0">
      <v-chip-group column>
        <v-chip v-for="(value, key) in details" :key="key" size="small" variant="outlined">
          <strong>{{ key }}:</strong> {{ value }}
        </v-chip>
      </v-chip-group>

      <v-alert v-if="item.errorMessage" type="error" density="compact" class="mt-2">
        {{ item.errorMessage }}
      </v-alert>
    </v-card-text>

    <v-card-actions>
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

  if (props.type === 'member') {
    const member = props.item as MemberDataDto;
    if (member.dateOfBirth) detailsObj[t('member.form.dateOfBirth')] = formatDate(member.dateOfBirth);
    if (member.dateOfDeath) detailsObj[t('member.form.dateOfDeath')] = formatDate(member.dateOfDeath);
    if (member.gender) {
      const translatedGender = t(`member.gender.${member.gender.toLowerCase()}`);
      detailsObj[t('member.form.gender')] = translatedGender;
    }
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
