<template>
  <v-form ref="formRef" :disabled="isFormReadOnly" data-testid="family-dict-form">
    <v-row>
      <v-col cols="12">
        <v-text-field v-model="formData.name" :label="t('familyDict.form.name')"
          :rules="rules.name"
          :readonly="isFormReadOnly" :disabled="isFormReadOnly" data-testid="family-dict-name-input"
          prepend-inner-icon="mdi-alphabetical"></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-textarea v-model="formData.description" :label="t('familyDict.form.description')"
          :rules="rules.description"
          :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="family-dict-description-input"
          prepend-inner-icon="mdi-text-box-outline"></v-textarea>
      </v-col>
      <v-col cols="12" md="6">
        <v-select v-model="formData.type" :items="familyDictTypes" :label="t('familyDict.form.type')"
          :rules="rules.type"
          :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="family-dict-type-select"
          prepend-inner-icon="mdi-tag"></v-select>
      </v-col>
      <v-col cols="12" md="6">
        <v-select v-model="formData.lineage" :items="familyDictLineages" :label="t('familyDict.form.lineage')"
          :rules="rules.lineage"
          :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="family-dict-lineage-select"
          prepend-inner-icon="mdi-family-tree"></v-select>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <h3 class="text-h6">{{ t('familyDict.form.namesByRegion') }}</h3>
      </v-col>
      <v-col cols="12">
        <v-text-field v-model="formData.namesByRegion.north" :label="t('familyDict.form.namesByRegion.north')"
          :rules="rules.namesByRegion.north"
          :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="family-dict-names-north-input"
          prepend-inner-icon="mdi-arrow-up"></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-text-field v-model="formData.namesByRegion.central" :label="t('familyDict.form.namesByRegion.central')"
          :rules="rules.namesByRegion.central"
          :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="family-dict-names-central-input"
          prepend-inner-icon="mdi-map-marker"></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-text-field v-model="formData.namesByRegion.south" :label="t('familyDict.form.namesByRegion.south')"
          :rules="rules.namesByRegion.south"
          :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="family-dict-names-south-input"
          prepend-inner-icon="mdi-arrow-down"></v-text-field>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import type { FamilyDict } from '@/types';
import { useFamilyDictForm } from '@/composables';

const props = defineProps<{
  readOnly?: boolean;
  initialFamilyDictData?: FamilyDict;
}>();

const { t } = useI18n();

const {
  state: { formRef, isFormReadOnly, familyDictTypes, familyDictLineages, formData, rules },
  actions: { validate, getFormData },
} = useFamilyDictForm(props);

defineExpose({
  validate,
  getFormData,
});
</script>
