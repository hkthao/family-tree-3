<template>
  <v-card class="border d-flex flex-column" :elevation="0" fill-height width="100%">
    <v-progress-linear :active="relationship.loading" :indeterminate="relationship.loading"  absolute
      top></v-progress-linear>

    <v-card-text >
      <v-chip color="primary" class="mt-2 title-chip">
        {{ title }}
      </v-chip>

      <div v-if="recommendations.length > 0" class="mt-2">
        <v-chip v-for="(rec, index) in recommendations" :key="`rec-${index}`" color="warning" size="small" class="ma-1">
          {{ rec }}
        </v-chip>
      </div>

       <div class="my-2">
        <v-alert v-if="relationship.errorMessage" type="error" class="m-0">
          {{ relationship.errorMessage }}
        </v-alert>
        <v-alert v-if="relationship.saveAlert?.show" :type="relationship.saveAlert?.type" class="m-0" variant="tonal">
          {{ relationship.saveAlert?.message }}
        </v-alert>
      </div>
    </v-card-text>

    <v-spacer></v-spacer>
    <v-card-actions v-if="!relationship.savedSuccessfully">
      <v-spacer></v-spacer>
      <v-btn color="red" @click="deleteRelationship" size="small">{{ t('common.delete') }}</v-btn>
      <v-btn color="primary" @click="emit('save-relationship', relationship)"
        :disabled="!!relationship.errorMessage || relationship.loading" :loading="relationship.loading" size="small">{{
          t('common.save')
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
  relationship: RelationshipDataDto;
  allMembers: MemberDataDto[];
  serialNumber: number;
}>();

const emit = defineEmits(['delete', 'save-relationship']);

const { t } = useI18n();

const getMemberFullName = (memberId: string | null | undefined) => {
  if (!memberId) return '';
  const member = props.allMembers.find(m => m.id === memberId);
  return member ? member.fullName : '';
};

const title = computed(() => {
  const sourceMemberName = getMemberFullName(props.relationship.sourceMemberId);
  const targetMemberName = getMemberFullName(props.relationship.targetMemberId);
  const relationshipTypeStringName = getRelationshipTypeStringName(props.relationship.type as any).toLowerCase();

  // Construct the title based on the relationship type
  switch (relationshipTypeStringName) {
    case 'father':
      return `${sourceMemberName} ${t('relationship.isThe')} ${t('relationship.type.father')} ${t('relationship.of')} ${targetMemberName}`;
    case 'mother':
      return `${sourceMemberName} ${t('relationship.isThe')} ${t('relationship.type.mother')} ${t('relationship.of')} ${targetMemberName}`;
    case 'husband':
      return `${sourceMemberName} ${t('relationship.isThe')} ${t('relationship.type.husband')} ${t('relationship.of')} ${targetMemberName}`;
    case 'wife':
      return `${sourceMemberName} ${t('relationship.isThe')} ${t('relationship.type.wife')} ${t('relationship.of')} ${targetMemberName}`;
    case 'child':
      return `${sourceMemberName} ${t('relationship.isThe')} ${t('relationship.type.child')} ${t('relationship.of')} ${targetMemberName}`;
    default:
      return `${sourceMemberName} ${t('relationship.isThe')} ${t(`relationship.type.${relationshipTypeStringName}`)} ${t('relationship.of')} ${targetMemberName}`;
  }
});

const recommendations = computed(() => {
  const recs: string[] = [];
  if (props.relationship.errorMessage) {
    recs.push(props.relationship.errorMessage);
  }
  // Add more specific recommendations here if needed
  return recs;
});

const deleteRelationship = () => {
  emit('delete');
};
</script>
<style>
.title-chip{
  white-space:normal;
  padding-top: 8px !important;
  padding-bottom: 8px !important;
  height: auto !important;
}
</style>