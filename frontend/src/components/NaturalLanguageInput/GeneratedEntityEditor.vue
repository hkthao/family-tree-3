<template>
  <v-chip :color="isFamily ? 'blue' : 'green'" label>{{ isFamily ? 'Family' : 'Member' }}</v-chip>
  <FamilyForm v-if="isFamily" :initial-family-data="editableEntity as Family" :read-only="false"
    @submit="(val) => updateEntity(val as Family)" />
  <MemberForm v-else :initial-member-data="editableEntity as Member" :read-only="false"
    @submit="(val: Member) => updateEntity(val)" />
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

const emit = defineEmits(['update:entity']);

const { t } = useI18n();

const isFamily = computed(() => 'name' in props.entity && 'visibility' in props.entity);

const editableEntity = ref<Family | Member>(props.entity);

watch(() => props.entity, (newVal) => {
  editableEntity.value = newVal;
}, { deep: true });

const updateEntity = (updatedVal: Family | Member) => {
  emit('update:entity', { index: props.index, entity: updatedVal, type: isFamily.value ? 'Family' : 'Member' });
};
</script>

<style scoped>
/* Add any specific styles here if needed */
</style>
