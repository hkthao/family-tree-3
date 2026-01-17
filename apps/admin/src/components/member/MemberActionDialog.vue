<template>
  <v-dialog v-model="dialog" max-width="400">
    <v-card>
      <v-card-title class="text-h5">{{ memberName }}</v-card-title>
      <v-list density="compact">
        <v-list-item
          prepend-icon="mdi-information-outline"
          :title="t('familyTree.actionDialog.viewDetails')"
          :subtitle="t('familyTree.actionDialog.viewDetailsDescription')"
          @click="viewDetails"
        ></v-list-item>
        <v-divider></v-divider>
        <v-list-item
          prepend-icon="mdi-family-tree"
          :title="t('familyTree.actionDialog.viewRelationships')"
          :subtitle="t('familyTree.actionDialog.viewRelationshipsDescription')"
          @click="viewRelationships"
        ></v-list-item>
      </v-list>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="grey-darken-1" variant="text" @click="dialog = false">
          {{ t('common.close') }}
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false,
  },
  memberId: {
    type: String,
    required: true,
  },
  memberName: {
    type: String,
    required: true,
  },
});

const emit = defineEmits(['update:modelValue', 'view-details', 'view-relationships']);

const dialog = ref(props.modelValue);

watch(() => props.modelValue, (newVal) => {
  dialog.value = newVal;
});

watch(dialog, (newVal) => {
  emit('update:modelValue', newVal);
});

const viewDetails = () => {
  emit('view-details', props.memberId);
  dialog.value = false;
};

const viewRelationships = () => {
  emit('view-relationships', props.memberId);
  dialog.value = false;
};
</script>