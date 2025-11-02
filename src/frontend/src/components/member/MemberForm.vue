<template>
  <v-form ref="form" :disabled="props.readOnly" data-testid="member-form">
    <!-- Thông tin cơ bản -->
    <v-row>
      <v-col cols="12">
        <AvatarInput v-if="!props.readOnly" v-model="form.avatarUrl" :size="96" />
        <div v-else class="d-flex justify-center mb-4">
          <AvatarDisplay :src="form.avatarUrl" :size="96" />
        </div>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" md="4">
        <v-text-field v-model="form.lastName" :label="t('member.form.lastName')" 
          @blur="v$.lastName.$touch()" @input="v$.lastName.$touch()"
          :error-messages="v$.lastName.$errors.map(e => e.$message as string)"
          :readonly="props.readOnly" data-testid="member-last-name-input"></v-text-field>
      </v-col>
      <v-col cols="12" md="4">
        <v-text-field v-model="form.firstName" :label="t('member.form.firstName')" 
          @blur="v$.firstName.$touch()" @input="v$.firstName.$touch()"
          :error-messages="v$.firstName.$errors.map(e => e.$message as string)"
          :readonly="props.readOnly" data-testid="member-first-name-input"></v-text-field>
      </v-col>
      <v-col cols="12" md="4">
        <v-text-field v-model="form.nickname" :label="t('member.form.nickname')" :readonly="props.readOnly"
          data-testid="member-nickname-input"></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" md="6">
        <v-date-input v-model="form.dateOfBirth" :label="t('member.form.dateOfBirth')" 
          @blur="v$.dateOfBirth.$touch()" @input="v$.dateOfBirth.$touch()"
          :error-messages="v$.dateOfBirth.$errors.map(e => e.$message as string)"
          :readonly="props.readOnly" data-testid="member-date-of-birth-input" append-inner-icon="mdi-calendar" />
      </v-col>
      <v-col cols="12" md="6">
        <v-date-input v-model="form.dateOfDeath" :label="t('member.form.dateOfDeath')" optional
          @blur="v$.dateOfDeath.$touch()" @input="v$.dateOfDeath.$touch()"
          :error-messages="v$.dateOfDeath.$errors.map(e => e.$message as string)"
          :readonly="props.readOnly" data-testid="member-date-of-death-input"
          append-inner-icon="mdi-calendar" />
      </v-col>
    </v-row>

    <!-- Thông tin cá nhân -->
    <v-row>
      <v-col cols="12" md="4">
        <GenderSelect v-model="form.gender" :label="t('member.form.gender')" :read-only="props.readOnly"
          data-testid="member-gender-select" />
      </v-col>
      <v-col cols="12" md="4">
        <v-text-field v-model="form.placeOfBirth" :label="t('member.form.placeOfBirth')"
          :readonly="props.readOnly" data-testid="member-place-of-birth-input"></v-text-field>
      </v-col>
      <v-col cols="12" md="4">
        <v-text-field v-model="form.placeOfDeath" :label="t('member.form.placeOfDeath')"
          :readonly="props.readOnly" data-testid="member-place-of-death-input"></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-text-field v-model="form.occupation" :label="t('member.form.occupation')" :readonly="props.readOnly"
          data-testid="member-occupation-input"></v-text-field>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <family-auto-complete v-model="form.familyId" :label="t('member.form.familyId')" 
          @blur="v$.familyId.$touch()" @update:modelValue="v$.familyId.$touch()"
          :error-messages="v$.familyId.$errors.map(e => e.$message as string)"
          :readonly="props.readOnly" :multiple="false" data-testid="member-family-select" />
      </v-col>
    </v-row>

    <!-- Thông tin khác -->
    <v-row>
      <v-col cols="12">
        <v-textarea :auto-grow="true" v-model="form.biography" :label="t('member.form.biography')"
          :readonly="props.readOnly" data-testid="member-biography-input"></v-textarea>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { reactive, toRefs } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Member } from '@/types';
import { Gender } from '@/types';
import { VDateInput } from 'vuetify/labs/VDateInput';
import { useVuelidate } from '@vuelidate/core';
import { useMemberRules } from '@/validations/member.validation';
import { GenderSelect, AvatarInput, AvatarDisplay } from '@/components/common';

const props = defineProps<{
  readOnly?: boolean;
  initialMemberData?: Member;
}>();

const { t } = useI18n();

const form = reactive<Omit<Member, 'id'> | Member>(
  props.initialMemberData
    ? {
      ...props.initialMemberData,
    }
    : {
      lastName: '',
      firstName: '',
      dateOfBirth: undefined,
      gender: Gender.Male,
      familyId: '',
    },
);

const state = reactive({
  lastName: form.lastName,
  firstName: form.firstName,
  dateOfBirth: form.dateOfBirth,
  familyId: form.familyId,
  dateOfDeath: form.dateOfDeath,
});

const rules = useMemberRules(toRefs(state));

const v$ = useVuelidate(rules, state);

// Expose form validation and data for parent component
const validate = async () => {
  const result = await v$.value.$validate();
  return result;
};

const getFormData = () => {
  return form;
};


defineExpose({
  validate,
  getFormData,
});
</script>