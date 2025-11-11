<template>
  <v-card class="border" :elevation="0" fill-height width="100%">
    <v-progress-linear :active="item.loading" :indeterminate="item.loading" color="primary" absolute
      top></v-progress-linear>
    <v-card-item>
      <v-card-title class="text-h6 text-center">
        <v-icon :icon="cardIcon" class="mr-2"></v-icon> {{ serialNumber }}. {{ title }}
      </v-card-title>
    </v-card-item>

    <v-card-text class="py-0">
      <v-chip-group column>
        <v-chip v-for="detail in details" size="small" :key="detail.originalKey">
          <strong>{{ detail.label }}:</strong> {{ detail.value }}
        </v-chip>
      </v-chip-group>

      <div v-if="recommendations.length > 0" >
        <v-chip v-for="(rec, index) in recommendations" :key="`rec-${index}`" color="warning" size="small">
          {{ rec }}
        </v-chip>
      </div>

    </v-card-text>

    <v-alert v-if="item.errorMessage" type="error" density="compact" class="mt-2">
      {{ item.errorMessage }}
    </v-alert>
    <v-alert v-if="item.saveAlert?.show" :type="item.saveAlert?.type" density="compact" class="mx-4 my-2"
      variant="tonal">
      {{ item.saveAlert?.message }}
    </v-alert>

    <v-card-actions v-if="!item.savedSuccessfully">
      <v-spacer></v-spacer>
      <v-btn color="red" @click="deleteItem" size="small">{{ t('common.delete') }}</v-btn>
      <v-btn color="primary" @click="emit('save-item', item)" :disabled="!!item.errorMessage || item.loading"
        :loading="item.loading" size="small">{{ t('common.save')
        }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemberDataDto, EventDataDto } from '@/types/natural-language.d';

const props = defineProps<{
  item: MemberDataDto | EventDataDto;
  type: 'member' | 'event';
  allMembers: MemberDataDto[];
  serialNumber: number;
}>();

const emit = defineEmits(['delete', 'save-item']); // Add save-item emit

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

const details = computed(() => {
  const detailsArray: { label: string; value: any; originalKey: string }[] = [];

  const formatDate = (dateString: string | null | undefined) => {
    if (!dateString) return '';
    try {
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
    return dateString;
  };

  const getMemberFullName = (memberId: string | null | undefined) => {
    if (!memberId) return '';
    const member = props.allMembers.find(m => m.id === memberId);
    return member ? member.fullName : '';
  };

  if (props.type === 'member') {
    const member = props.item as MemberDataDto;
    if (member.dateOfBirth) detailsArray.push({ label: t('member.form.dateOfBirth'), value: formatDate(member.dateOfBirth), originalKey: 'dateOfBirth' });
    if (member.dateOfDeath) detailsArray.push({ label: t('member.form.dateOfDeath'), value: formatDate(member.dateOfDeath), originalKey: 'dateOfDeath' });
    if (member.gender) {
      const translatedGender = t(`member.gender.${member.gender.toLowerCase()}`);
      detailsArray.push({ label: t('member.form.gender'), value: translatedGender, originalKey: 'gender' });
    }

    // Add relationship information
    if (member.fatherId) detailsArray.push({ label: t('relationship.type.father'), value: getMemberFullName(member.fatherId), originalKey: 'fatherId' });
    if (member.motherId) detailsArray.push({ label: t('relationship.type.mother'), value: getMemberFullName(member.motherId), originalKey: 'motherId' });
    if (member.husbandId) detailsArray.push({ label: t('relationship.type.husband'), value: getMemberFullName(member.husbandId), originalKey: 'husbandId' });
    if (member.wifeId) detailsArray.push({ label: t('relationship.type.wife'), value: getMemberFullName(member.wifeId), originalKey: 'wifeId' });

  } else {
    const event = props.item as EventDataDto;
    detailsArray.push({ label: t('event.form.description'), value: event.description, originalKey: 'description' });
    if (event.date) detailsArray.push({ label: t('event.form.date'), value: formatDate(event.date), originalKey: 'eventDate' });
    if (event.location) detailsArray.push({ label: t('event.form.location'), value: event.location, originalKey: 'location' });
  }
  return detailsArray;
});

const recommendations = computed(() => {
  const recs: string[] = [];
  if (props.item.errorMessage) {
    recs.push(props.item.errorMessage);
  }

  if (props.type === 'member') {
    const member = props.item as MemberDataDto;
    if (!member.dateOfBirth) recs.push(t('naturalLanguage.recommendations.missingDateOfBirth'));
    if (!member.gender) recs.push(t('naturalLanguage.recommendations.missingGender'));
    // Add more specific recommendations here if needed, e.g., for relationships
  } else {
    const event = props.item as EventDataDto;
    if (!event.date) recs.push(t('naturalLanguage.recommendations.missingDate'));
    if (!event.location) recs.push(t('naturalLanguage.recommendations.missingLocation'));
  }
  return recs;
});

const deleteItem = () => {
  emit('delete');
};
</script>
