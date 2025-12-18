<template>
  <v-form ref="formRef" :disabled="isFormReadOnly" data-testid="family-dict-form">
    <v-row>
      <v-col cols="12">
        <v-text-field v-model="formData.name" :label="t('familyDict.form.name')" @blur="v$.name.$touch()"
          @input="v$.name.$touch()" :error-messages="v$.name.$errors.map(e => e.$message as string)"
          :readonly="isFormReadOnly" :disabled="isFormReadOnly" data-testid="family-dict-name-input"></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-textarea v-model="formData.description" :label="t('familyDict.form.description')"
          @blur="v$.description.$touch()" @input="v$.description.$touch()"
          :error-messages="v$.description.$errors.map(e => e.$message as string)" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="family-dict-description-input"></v-textarea>
      </v-col>
      <v-col cols="12" md="6">
        <v-select v-model="formData.type" :items="familyDictTypes" :label="t('familyDict.form.type')"
          @blur="v$.type.$touch()" @update:modelValue="v$.type.$touch()"
          :error-messages="v$.type.$errors.map(e => e.$message as string)" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="family-dict-type-select"></v-select>
      </v-col>
      <v-col cols="12" md="6">
        <v-select v-model="formData.lineage" :items="familyDictLineages" :label="t('familyDict.form.lineage')"
          @blur="v$.lineage.$touch()" @update:modelValue="v$.lineage.$touch()"
          :error-messages="v$.lineage.$errors.map(e => e.$message as string)" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="family-dict-lineage-select"></v-select>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <h3 class="text-h6">{{ t('familyDict.form.namesByRegion') }}</h3>
      </v-col>
      <v-col cols="12">
        <v-text-field v-model="formData.namesByRegion.north" :label="t('familyDict.form.namesByRegion.north')"
          @blur="v$.namesByRegion.north.$touch()" @input="v$.namesByRegion.north.$touch()"
          :error-messages="v$.namesByRegion.north.$errors.map(e => e.$message as string)" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="family-dict-names-north-input"></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-text-field v-model="formData.namesByRegion.central" :label="t('familyDict.form.namesByRegion.central')"
          @blur="v$.namesByRegion.central.$touch()" @input="v$.namesByRegion.central.$touch()"
          :error-messages="v$.namesByRegion.central.$errors.map(e => e.$message as string)" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="family-dict-names-central-input"></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-text-field v-model="formData.namesByRegion.south" :label="t('familyDict.form.namesByRegion.south')"
          @blur="v$.namesByRegion.south.$touch()" @input="v$.namesByRegion.south.$touch()"
          :error-messages="v$.namesByRegion.south.$errors.map(e => e.$message as string)" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="family-dict-names-south-input"></v-text-field>
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
  formRef,
  isFormReadOnly,
  familyDictTypes,
  familyDictLineages,
  formData,
  v$,
  validate,
  getFormData,
} = useFamilyDictForm(props);

defineExpose({
  validate,
  getFormData,
  v$,
});
</script>
