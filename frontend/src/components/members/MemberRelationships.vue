<template>
  <v-card flat>
    <v-card-text>
      <v-toolbar flat density="compact">
        <v-toolbar-title>{{ $t('member.form.tab.relationships') }}</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn icon size="small" variant="text" @click="openAddRelationshipForm" :disabled="readOnly">
          <v-icon>mdi-plus</v-icon>
        </v-btn>
      </v-toolbar>
      <v-data-table
        :headers="headers"
        :items="relationships"
        hide-default-footer
      >
        <template #item.relatedMemberName="{ item }">
          {{ getMemberName(item.relatedMemberId) }}
        </template>
        <template #item.relationshipType="{ item }">
          {{ getRelationshipTypeName(item.relationshipType) }}
        </template>
        <template #item.actions="{ item }">
          <v-btn v-if="!readOnly" icon size="small" variant="text" @click="openEditRelationshipForm(item)" :disabled="readOnly">
            <v-icon>mdi-pencil</v-icon>
          </v-btn>
          <v-btn v-if="!readOnly" icon size="small" variant="text" @click="$emit('delete', item)" :disabled="readOnly">
            <v-icon>mdi-delete</v-icon>
          </v-btn>
        </template>
      </v-data-table>
    </v-card-text>

    <v-dialog v-model="relationshipFormDialog" max-width="600px" persistent>
      <RelationshipForm
        :initial-relationship-data="selectedRelationship"
        :title="isEditRelationshipMode ? t('relationship.form.editTitle') : t('relationship.form.addTitle')"
        @submit="handleSaveRelationship"
        @cancel="handleCancelRelationship"
      />
    </v-dialog>
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMembers } from '@/data/members'; // To get member names
import RelationshipForm from './RelationshipForm.vue';

const props = defineProps<{
  relationships: Array<any>; // Define a more specific type later
  readOnly?: boolean;
}>();

const emit = defineEmits(['add', 'edit', 'delete']);

const { t } = useI18n();
const { getMemberById, members: allMembers } = useMembers();

const relationshipFormDialog = ref(false);
const selectedRelationship = ref<any>(null); // Define a more specific type later
const isEditRelationshipMode = ref(false);

const headers = computed(() => [
  { title: t('member.form.fullName'), key: 'relatedMemberName' },
  { title: t('member.form.relationshipType'), key: 'relationshipType' },
  { title: t('common.actions'), key: 'actions', sortable: false },
]);

const getMemberName = (id: string) => {
  const member = getMemberById(id);
  return member ? member.fullName : t('common.unknown');
};

const getRelationshipTypeName = (type: string) => {
  // This will be replaced by a proper i18n mapping for relationship types
  return type;
};

const openAddRelationshipForm = () => {
  selectedRelationship.value = null;
  isEditRelationshipMode.value = false;
  relationshipFormDialog.value = true;
};

const openEditRelationshipForm = (relationship: any) => {
  selectedRelationship.value = { ...relationship };
  isEditRelationshipMode.value = true;
  relationshipFormDialog.value = true;
};

const handleSaveRelationship = (relationshipData: any) => {
  // This event will be handled by MemberForm.vue
  // For now, just close the dialog
  console.log('Saving relationship:', relationshipData);
  relationshipFormDialog.value = false;
  selectedRelationship.value = null;
};

const handleCancelRelationship = () => {
  relationshipFormDialog.value = false;
  selectedRelationship.value = null;
};
</script>