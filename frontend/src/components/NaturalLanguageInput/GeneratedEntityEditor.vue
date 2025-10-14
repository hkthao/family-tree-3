<template>
  <v-expansion-panel elevation="0" class="p-0 m-0">
    <v-expansion-panel-title>
      <div class="d-flex align-center justify-space-between">
        <span class="text-h6">{{ panelTitle }}</span>
        <v-spacer></v-spacer>
        <v-chip :color="isFamily ? 'blue' : 'green'" label class="ms-2">{{ isFamily ? 'Family' : 'Member' }}</v-chip>
      </div>
    </v-expansion-panel-title>
    <v-expansion-panel-text class="no-padding">
      <FamilyForm v-if="isFamily" :initial-family-data="editableEntity as Family" :read-only="false"
        @submit="(val) => updateEntity(val as Family)" />
      <MemberForm v-else :initial-member-data="editableEntity as Member" :read-only="false"
        @submit="(val: Member) => updateEntity(val)" />
    </v-expansion-panel-text>
  </v-expansion-panel>
</template>

<script setup lang="ts">
import { ref, computed, watch, type PropType } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Family, Member } from '@/types';
import FamilyForm from '@/components/family/FamilyForm.vue';
import MemberForm from '@/components/members/MemberForm.vue';

const props = defineProps({
  entity: { type: Object as PropType<Family | Member>, required: true },
  index: { type: Number, required: true },
});

const emit = defineEmits(['update:entity', 'remove:entity']);

const { t } = useI18n();

const isFamily = computed(() => 'name' in props.entity && 'visibility' in props.entity);

const editableEntity = ref<Family | Member>(props.entity);

const panelTitle = computed(() => {
  if (isFamily.value) {
    return `Family: ${(editableEntity.value as Family).name || 'New Family'}`;
  } else {
    return `Member: ${(editableEntity.value as Member).fullName || (editableEntity.value as Member).firstName || 'New Member'}`;
  }
});

watch(() => props.entity, (newVal) => {
  editableEntity.value = newVal;
}, { deep: true });

const updateEntity = (updatedVal: Family | Member) => {
  emit('update:entity', { index: props.index, entity: updatedVal, type: isFamily.value ? 'Family' : 'Member' });
};

const removeEntity = () => {
  emit('remove:entity', { index: props.index, type: isFamily.value ? 'Family' : 'Member' });
};
</script>

<style scoped>
.no-padding {
  padding: 0 !important;
}

/* Add any specific styles here if needed */
</style>
