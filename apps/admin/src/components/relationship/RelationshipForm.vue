<template>
  <v-form ref="formRef" :disabled="props.readOnly" data-testid="relationship-form">
    <v-row>
      <v-col cols="12" md="6">
        <family-auto-complete v-model="formData.familyId" :label="t('relationship.form.family')"
          @blur="v$.familyId.$touch()" @update:modelValue="v$.familyId.$touch()"
          :error-messages="v$.familyId.$errors.map(e => e.$message as string)" :readonly="props.readOnly"
          data-testid="relationship-family-autocomplete" />
      </v-col>
      <v-col cols="12" md="6">
        <v-select v-model="formData.type" :items="relationshipTypes" :label="t('relationship.form.type')"
          @blur="v$.type.$touch()" @input="v$.type.$touch()"
          :error-messages="v$.type.$errors.map(e => e.$message as string)" :readonly="props.readOnly "
          data-testid="relationship-type-select"></v-select>
      </v-col>

    </v-row>
    <v-row>
      <v-col cols="12" md="6">
        <member-auto-complete v-model="formData.sourceMemberId" :label="t('relationship.form.sourceMember')"
          @blur="v$.sourceMemberId.$touch()" @update:modelValue="v$.sourceMemberId.$touch()"
          :error-messages="v$.sourceMemberId.$errors.map(e => e.$message as string)" :readonly="props.readOnly || !formData.familyId"
          :family-id="formData.familyId" data-testid="relationship-source-member-autocomplete" />
      </v-col>
      <v-col cols="12" md="6">
        <member-auto-complete v-model="formData.targetMemberId" :label="t('relationship.form.targetMember')"
          @blur="v$.targetMemberId.$touch()" @update:modelValue="v$.targetMemberId.$touch()"
          :error-messages="v$.targetMemberId.$errors.map(e => e.$message as string)" :readonly="props.readOnly || !formData.familyId"
          :family-id="formData.familyId" data-testid="relationship-target-member-autocomplete" />
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <v-text-field v-model.number="formData.order" :label="t('relationship.form.order')" type="number"
          :readonly="props.readOnly" data-testid="relationship-order-input"></v-text-field>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { reactive, toRefs, ref, toRef, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { RelationshipType, type Relationship } from '@/types';
import { RELATIONSHIP_TYPE_OPTIONS } from '@/constants/relationshipTypes';
import { useVuelidate } from '@vuelidate/core';
import { useRelationshipRules } from '@/validations/relationship.validation';

const relationshipTypes = RELATIONSHIP_TYPE_OPTIONS;
const props = defineProps<{
  id?: string;
  readOnly?: boolean;
  initialRelationshipData?: Relationship;
}>();

const { t } = useI18n();

const formRef = ref<HTMLFormElement | null>(null);

const formData = reactive<Omit<Relationship, 'id'> | Relationship>(
  props.initialRelationshipData
    ? { ...props.initialRelationshipData }
    : {
      sourceMemberId: '',
      targetMemberId: '',
      type: RelationshipType.Father,
      order: undefined,
      familyId: undefined
    },
);

const state = reactive({
  sourceMemberId: toRef(formData, 'sourceMemberId'),
  targetMemberId: toRef(formData, 'targetMemberId'),
  type: toRef(formData, 'type'),
  familyId: toRef(formData, 'familyId')
});

watch(() => formData.familyId, (newFamilyId, oldFamilyId) => {
  if (newFamilyId !== oldFamilyId) {
    formData.sourceMemberId = '';
    formData.targetMemberId = '';
  }
});

const rules = useRelationshipRules(toRefs(state));

const v$ = useVuelidate(rules, state);

const validate = async () => {
  const result = await v$.value.$validate();
  return result;
};

const getFormData = () => {
  return formData;
};

// save and cancel methods are removed

defineExpose({
  validate,
  getFormData,
});
</script>