<template>
  <v-container v-if="relationshipStore.detail.item">
    <v-row>
      <v-col cols="12">
        <v-card data-testid="relationship-edit-view">
          <v-card-title class="d-flex align-center">
            {{ t('relationship.form.editTitle') }}
            <v-spacer></v-spacer>
            <v-btn icon="mdi-close" size="small" variant="text" @click="cancel"></v-btn>
          </v-card-title>
          <v-card-text>
                         <RelationshipForm ref="relationshipForm" :relationship-id="relationshipId" :initial-relationship-data="relationshipStore.detail.item" />          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn @click="cancel" data-testid="relationship-edit-cancel-button">{{ t('common.cancel') }}</v-btn>
            <v-btn color="primary" @click="save" data-testid="relationship-edit-save-button">{{ t('common.save') }}</v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
// import { useRouter } from 'vue-router'; // Removed as not used for navigation directly
import { useI18n } from 'vue-i18n';
import { useRelationshipStore } from '@/stores/relationship.store';
import type { Relationship } from '@/types';
import {RelationshipForm} from '@/components/relationship';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar'; // Import useGlobalSnackbar

const props = defineProps<{ relationshipId: string }>(); // Renamed id to relationshipId
const emit = defineEmits(['close', 'saved']); // Add emit

// const router = useRouter(); // Removed
const { t } = useI18n();
const relationshipStore = useRelationshipStore();
const { showSnackbar } = useGlobalSnackbar(); // Khởi tạo useGlobalSnackbar

const relationshipForm = ref<InstanceType<typeof RelationshipForm> | null>(null);

onMounted(async () => {
  if (props.relationshipId) {
    await relationshipStore.getById(props.relationshipId);
  }
});

const save = async () => {
  const isValid = await relationshipForm.value?.validate();
      if (isValid) {
      const formData = relationshipForm.value?.getFormData();
      if (formData && props.relationshipId) {
        await relationshipStore.updateItem(formData as Relationship);
        if (!relationshipStore.error) {
          showSnackbar(t('relationship.messages.updateSuccess'), 'success');
          emit('saved'); // Emit saved event
        } else {
          showSnackbar(relationshipStore.error || t('relationship.messages.saveError'), 'error');
        }
      }
    }};

const cancel = () => {
  emit('close'); // Emit close event
};
</script>
