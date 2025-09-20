<template>
  <v-row>
    <v-col cols="12">

      <v-data-table
        :headers="relationshipHeaders"
        :items="allRelationships"
        hide-default-footer
      >
        <template #top>
          <v-toolbar flat density="compact">
            <v-spacer></v-spacer>
            <v-btn color="primary" icon @click="addRelationship" :disabled="readOnly">
              <v-icon>mdi-plus</v-icon>
            </v-btn>
          </v-toolbar>
        </template>
        <template #item.type="{ item }">
          {{ t(`relationship.type.${item.type}`) }}
        </template>
        <template #item.actions="{ item }">
          <v-btn icon size="small" variant="text" @click="editRelationship(item)" :disabled="readOnly">
            <v-icon>mdi-pencil</v-icon>
          </v-btn>
          <v-btn icon size="small" variant="text" @click="removeRelationship(item)" :disabled="readOnly">
            <v-icon>mdi-delete</v-icon>
          </v-btn>
        </template>
      </v-data-table>
    </v-col>
  </v-row>

  <RelationshipForm
    v-model="relationshipFormDialog"
    :initial-relationship-data="currentRelationship"
    @save="handleSaveRelationship"
    @cancel="handleCancelRelationship"
  />
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import type { Member } from '@/types/member';
import { useI18n } from 'vue-i18n';
import RelationshipForm from './RelationshipForm.vue'; // Import the new component

const props = defineProps<{
  memberForm: Omit<Member, 'id'> | Member;
  readOnly?: boolean;
}>();

const emit = defineEmits(['addRelationship', 'editRelationship', 'removeRelationship']);

const { t } = useI18n();

const relationshipFormDialog = ref(false);
const currentRelationship = ref<any>(null); // To hold data for editing

const relationshipHeaders = [
  { title: t('member.form.fullName'), key: 'fullName' },
  { title: t('member.form.relationshipType'), key: 'relationshipType' },
  { title: t('member.form.type'), key: 'type' }, // New column to show relationship type (parent, spouse, child)
  { title: t('common.actions'), key: 'actions', sortable: false },
];

const allRelationships = computed(() => {
  const parents = props.memberForm.parents.map(p => ({ ...p, type: 'parent' }));
  const spouses = props.memberForm.spouses.map(s => ({ ...s, type: 'spouse' }));
  const children = props.memberForm.children.map(c => ({ ...c, type: 'child' }));
  return [...parents, ...spouses, ...children];
});

const addRelationship = () => {
  currentRelationship.value = null; // Clear for new relationship
  relationshipFormDialog.value = true;
};

const editRelationship = (item: any) => {
  currentRelationship.value = { ...item }; // Set data for editing
  relationshipFormDialog.value = true;
};

const removeRelationship = (item: any) => {
  emit('removeRelationship', item, item.type);
};

const handleSaveRelationship = (newRelationshipData: any) => {
  if (currentRelationship.value) {
    // Editing existing relationship
    emit('editRelationship', newRelationshipData, newRelationshipData.type);
  } else {
    // Adding new relationship
    emit('addRelationship', newRelationshipData);
  }
  relationshipFormDialog.value = false;
};

const handleCancelRelationship = () => {
  relationshipFormDialog.value = false;
};
</script>