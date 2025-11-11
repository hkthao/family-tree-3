<template>
  <v-card class="mb-2 border" elevation="0">
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

    <v-card-actions class="px-4 pb-4 pt-2">
      <v-spacer></v-spacer>
      <v-btn color="red" @click="deleteItem" size="small">{{ t('common.delete') }}</v-btn>
      <v-btn color="primary" @click="saveItem" :disabled="!!item.errorMessage" size="small">{{ t('common.save') }}</v-btn>
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
  if (props.type === 'member') {
    const member = props.item as MemberDataDto;
    if (member.dateOfBirth) detailsObj[t('member.form.dateOfBirth')] = member.dateOfBirth;
    if (member.dateOfDeath) detailsObj[t('member.form.dateOfDeath')] = member.dateOfDeath;
    if (member.gender) detailsObj[t('member.form.gender')] = member.gender;
  } else {
    const event = props.item as EventDataDto;
    detailsObj[t('event.form.description')] = event.description;
    if (event.date) detailsObj[t('event.form.date')] = event.date;
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
