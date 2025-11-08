<template>
  <v-form ref="formRef" :disabled="props.readOnly" data-testid="member-form">
    <!-- Thông tin cơ bản -->
    <v-row>
      <v-col cols="12">
        <AvatarInput v-if="!props.readOnly" v-model="formData.avatarUrl" :size="96" />
        <div v-else class="d-flex justify-center mb-4">
          <AvatarDisplay :src="formData.avatarUrl" :size="96" />
        </div>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <family-auto-complete v-model="formData.familyId" :label="t('member.form.familyId')"
          @blur="v$.familyId.$touch()" @update:modelValue="v$.familyId.$touch()"
          :error-messages="v$.familyId.$errors.map(e => e.$message as string)" :readonly="props.readOnly"
          :multiple="false" :disabled="true" data-testid="member-family-select" />
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" md="6">
        <v-text-field v-model="formData.lastName" :label="t('member.form.lastName')" @blur="v$.lastName.$touch()"
          @input="v$.lastName.$touch()" :error-messages="v$.lastName.$errors.map(e => e.$message as string)"
          :readonly="props.readOnly" data-testid="member-last-name-input"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model="formData.firstName" :label="t('member.form.firstName')" @blur="v$.firstName.$touch()"
          @input="v$.firstName.$touch()" :error-messages="v$.firstName.$errors.map(e => e.$message as string)"
          :readonly="props.readOnly" data-testid="member-first-name-input"></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-text-field v-model="formData.nickname" :label="t('member.form.nickname')" :readonly="props.readOnly"
          data-testid="member-nickname-input"></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" md="6">
        <v-date-input v-model="formData.dateOfBirth" :label="t('member.form.dateOfBirth')" @blur="v$.dateOfBirth.$touch()"
          @input="v$.dateOfBirth.$touch()" :error-messages="v$.dateOfBirth.$errors.map(e => e.$message as string)"
          :readonly="props.readOnly" data-testid="member-date-of-birth-input" append-inner-icon="mdi-calendar" />
      </v-col>
      <v-col cols="12" md="6">
        <v-date-input v-model="formData.dateOfDeath" :label="t('member.form.dateOfDeath')" optional
          @blur="v$.dateOfDeath.$touch()" @input="v$.dateOfDeath.$touch()"
          :error-messages="v$.dateOfDeath.$errors.map(e => e.$message as string)" :readonly="props.readOnly"
          data-testid="member-date-of-death-input" append-inner-icon="mdi-calendar" />
      </v-col>
    </v-row>

    <!-- Thông tin cá nhân -->
    <v-row>
      <v-col cols="12" md="4">
        <GenderSelect v-model="formData.gender" :label="t('member.form.gender')" :read-only="props.readOnly"
          data-testid="member-gender-select" />
      </v-col>
      <v-col cols="12" md="4">
        <v-text-field v-model="formData.placeOfBirth" :label="t('member.form.placeOfBirth')" :readonly="props.readOnly"
          data-testid="member-place-of-birth-input"></v-text-field>
      </v-col>
      <v-col cols="12" md="4">
        <v-text-field v-model="formData.placeOfDeath" :label="t('member.form.placeOfDeath')" :readonly="props.readOnly"
          data-testid="member-place-of-death-input"></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-text-field v-model="formData.occupation" :label="t('member.form.occupation')" :readonly="props.readOnly"
          data-testid="member-occupation-input"></v-text-field>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { reactive, toRefs, ref, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Member } from '@/types';
import { Gender } from '@/types';
import { useVuelidate } from '@vuelidate/core';
import { useMemberRules } from '@/validations/member.validation';
import { GenderSelect, AvatarInput, AvatarDisplay } from '@/components/common';

const props = defineProps<{
  readOnly?: boolean;
  initialMemberData?: Member;
  familyId?: string; // Add familyId prop
}>();

const { t } = useI18n();

const formRef = ref<HTMLFormElement | null>(null);

const formData = reactive<Omit<Member, 'id'> | Member>(
  props.initialMemberData
    ? {
      ...props.initialMemberData,
    }
    : {
      lastName: '',
      firstName: '',
      dateOfBirth: undefined,
      gender: Gender.Male,
      familyId: props.familyId || '', // Initialize familyId from prop
    },
);

const state = reactive({
  lastName: toRef(formData, 'lastName'),
  firstName: toRef(formData, 'firstName'),
  dateOfBirth: toRef(formData, 'dateOfBirth'),
  dateOfDeath: toRef(formData, 'dateOfDeath'),
  familyId: toRef(formData, 'familyId'),
});

const rules = useMemberRules(toRefs(state));

const v$ = useVuelidate(rules, state);

// Expose form validation and data for parent component
const validate = async () => {
  const result = await v$.value.$validate();
  return result;
};

const getFormData = () => {
  return formData;
};


defineExpose({
  validate,
  getFormData,
});
</script>
