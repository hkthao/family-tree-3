<template>
  <v-dialog v-model="dialog" max-width="500px">
    <v-card>
      <v-card-title>
        <span class="text-h5">{{ isEditing ? t('relationship.form.editTitle') : t('relationship.form.addTitle') }}</span>
      </v-card-title>
      <v-card-text>
        <v-select
          v-model="relationship.type"
          :items="[
            { title: t('relationship.type.parent'), value: 'parent' },
            { title: t('relationship.type.spouse'), value: 'spouse' },
            { title: t('relationship.type.child'), value: 'child' },
          ]"
          :label="t('relationship.form.type')"
          required
        ></v-select>
        <v-text-field
          v-model="relationship.fullName"
          :label="t('relationship.form.fullName')"
          required
        ></v-text-field>
        <v-select
          v-model="relationship.relationshipType"
          :items="['Ruột thịt', 'Con nuôi', 'Con riêng', 'Đã kết hôn', 'Đã ly hôn']"
          :label="t('relationship.form.relationshipType')"
          required
        ></v-select>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="blue-darken-1" variant="text" @click="cancel">{{ t('common.cancel') }}</v-btn>
        <v-btn color="blue-darken-1" variant="text" @click="save">{{ t('common.save') }}</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';

const props = defineProps<{
  modelValue: boolean; // Controls dialog visibility
  initialRelationshipData?: any; // Data for editing
}>();

const emit = defineEmits(['update:modelValue', 'save', 'cancel']);

const { t } = useI18n();

const dialog = computed({
  get() {
    return props.modelValue;
  },
  set(value) {
    emit('update:modelValue', value);
  },
});

const relationship = ref({
  type: '',
  fullName: '',
  relationshipType: '',
});

const isEditing = computed(() => !!props.initialRelationshipData);

watch(() => props.initialRelationshipData, (newVal) => {
  if (newVal) {
    relationship.value = { ...newVal };
  } else {
    relationship.value = {
      type: '',
      fullName: '',
      relationshipType: '',
    };
  }
}, { immediate: true });

const save = () => {
  emit('save', relationship.value);
  dialog.value = false;
};

const cancel = () => {
  emit('cancel');
  dialog.value = false;
};
</script>