<template>
  <v-card class="border d-flex flex-column" :elevation="0" fill-height width="100%">
    <v-progress-linear :active="member.loading" :indeterminate="member.loading" color="primary" absolute
      top></v-progress-linear>
    <v-card-item>
      <v-card-title class="text-h6 text-center">
        <v-icon icon="mdi-account-circle" class="mr-2"></v-icon> {{ serialNumber }}. {{ member.fullName }}
      </v-card-title>
    </v-card-item>

    <v-card-text class="py-0">
      <v-chip-group column>
        <v-chip v-for="detail in details" size="small" :key="detail.originalKey">
          <strong>{{ detail.label }}:</strong> {{ detail.value }}
        </v-chip>
      </v-chip-group>

      <div v-if="recommendations.length > 0">
        <v-chip v-for="(rec, index) in recommendations" :key="`rec-${index}`" color="warning" size="small">
          {{ rec }}
        </v-chip>
      </div>

      <div class="my-2">
        <v-alert v-if="member.errorMessage" type="error" class="m-0">
          {{ member.errorMessage }}
        </v-alert>
        <v-alert v-if="member.saveAlert?.show" :type="member.saveAlert?.type" class="m-0" variant="tonal">
          {{ member.saveAlert?.message }}
        </v-alert>
      </div>

    </v-card-text>
    <v-spacer></v-spacer>
    <v-card-actions v-if="!member.savedSuccessfully">
      <v-spacer></v-spacer>
      <v-btn color="red" @click="deleteMember" size="small">{{ t('common.delete') }}</v-btn>
      <v-btn color="primary" @click="emit('save-member', member)" :disabled="!!member.errorMessage || member.loading"
        :loading="member.loading" size="small">{{ t('common.save')
        }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemberDataDto, RelationshipDataDto } from '@/types/ai';
import { getRelationshipTypeStringName } from '@/utils/enum-helpers';

const props = defineProps<{
  member: MemberDataDto;
  allMembers: MemberDataDto[];
  allRelationships: RelationshipDataDto[];
  serialNumber: number;
}>();

const emit = defineEmits(['delete', 'save-member']);

const { t } = useI18n();

const formatDate = (dateString: string | null | undefined) => {
  if (!dateString) return '';
  try {
    const date = new Date(dateString);
    if (!isNaN(date.getTime())) {
      const day = String(date.getDate()).padStart(2, '0');
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const year = date.getFullYear();
      return `${year}-${month}-${day}`; // Changed to YYYY-MM-DD for consistency
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

const details = computed(() => {
  const detailsArray: { label: string; value: any; originalKey: string }[] = [];

  if (props.member.dateOfBirth) detailsArray.push({ label: t('member.form.dateOfBirth'), value: formatDate(props.member.dateOfBirth), originalKey: 'dateOfBirth' });
  if (props.member.dateOfDeath) detailsArray.push({ label: t('member.form.dateOfDeath'), value: formatDate(props.member.dateOfDeath), originalKey: 'dateOfDeath' });
  if (props.member.gender) {
    const translatedGender = t(`member.gender.${props.member.gender.toLowerCase()}`);
    detailsArray.push({ label: t('member.form.gender'), value: translatedGender, originalKey: 'gender' });
  }

  // Relationship display logic
  props.allRelationships.forEach(rel => {
    if (rel.sourceMemberId === props.member.id) {
      const targetMemberFullName = getMemberFullName(rel.targetMemberId);
      if (targetMemberFullName) {
        detailsArray.push({
          label: t(`relationship.type.${getRelationshipTypeStringName(rel.type as any).toLowerCase()}`),
          value: targetMemberFullName,
          originalKey: `${rel.type}-${rel.targetMemberId}`
        });
      }
    }
  });

  return detailsArray;
});

const recommendations = computed(() => {
  const recs: string[] = [];
  if (props.member.errorMessage) {
    recs.push(props.member.errorMessage);
  }

  if (!props.member.dateOfBirth) recs.push(t('naturalLanguage.recommendations.missingDateOfBirth'));
  if (!props.member.gender) recs.push(t('naturalLanguage.recommendations.missingGender'));
  return recs;
});

const deleteMember = () => {
  emit('delete');
};
</script>
