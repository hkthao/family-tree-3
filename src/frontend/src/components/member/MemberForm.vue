<template>
  <v-form ref="form" :disabled="props.readOnly" data-testid="member-form">
    <!-- Thông tin cơ bản -->
    <v-row>
      <v-col cols="12">
        <AvatarInput v-if="!props.readOnly" v-model="memberForm.avatarUrl" :size="96" />
        <div v-else class="d-flex justify-center mb-4">
          <AvatarDisplay :src="memberForm.avatarUrl" :size="96" />
        </div>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" md="4">
        <v-text-field v-model="memberForm.lastName" :label="t('member.form.lastName')" :rules="[rules.required]"
          :readonly="props.readOnly" data-testid="member-last-name-input"></v-text-field>
      </v-col>
      <v-col cols="12" md="4">
        <v-text-field v-model="memberForm.firstName" :label="t('member.form.firstName')" :rules="[rules.required]"
          :readonly="props.readOnly" data-testid="member-first-name-input"></v-text-field>
      </v-col>
      <v-col cols="12" md="4">
        <v-text-field v-model="memberForm.nickname" :label="t('member.form.nickname')" :readonly="props.readOnly"
          data-testid="member-nickname-input"></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" md="6">
        <DateInputField v-model="memberForm.dateOfBirth" :label="t('member.form.dateOfBirth')" :rules="[rules.required]"
          :readonly="props.readOnly" data-testid="member-date-of-birth-input" />
      </v-col>
      <v-col cols="12" md="6">
        <DateInputField v-model="memberForm.dateOfDeath" :label="t('member.form.dateOfDeath')" optional
          :readonly="props.readOnly" :rules="[dateOfDeathRule]" data-testid="member-date-of-death-input" />
      </v-col>
    </v-row>

    <!-- Thông tin cá nhân -->
    <v-row>
      <v-col cols="12" md="4">
        <GenderSelect v-model="memberForm.gender" :label="t('member.form.gender')" :rules="[rules.required]"
          :read-only="props.readOnly" data-testid="member-gender-select" />
      </v-col>
      <v-col cols="12" md="4">
        <v-text-field v-model="memberForm.placeOfBirth" :label="t('member.form.placeOfBirth')"
          :readonly="props.readOnly" data-testid="member-place-of-birth-input"></v-text-field>
      </v-col>
      <v-col cols="12" md="4">
        <v-text-field v-model="memberForm.placeOfDeath" :label="t('member.form.placeOfDeath')"
          :readonly="props.readOnly" data-testid="member-place-of-death-input"></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-text-field v-model="memberForm.occupation" :label="t('member.form.occupation')" :readonly="props.readOnly"
          data-testid="member-occupation-input"></v-text-field>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <FamilyAutocomplete v-model="memberForm.familyId" :label="t('member.form.familyId')" :rules="[rules.required]"
          :readonly="props.readOnly" :multiple="false" data-testid="member-family-select" />
      </v-col>
    </v-row>

    <!-- Thông tin khác -->
    <v-row>
      <v-col cols="12">
        <v-textarea :auto-grow="true" v-model="memberForm.biography" :label="t('member.form.biography')"
          :readonly="props.readOnly" data-testid="member-biography-input"></v-textarea>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Member } from '@/types';
import { Gender } from '@/types';
import { DateInputField, GenderSelect, FamilyAutocomplete, AvatarInput, AvatarDisplay } from '@/components/common';

const props = defineProps<{
  readOnly?: boolean;
  initialMemberData?: Member;
}>();

const { t } = useI18n();

const form = ref<HTMLFormElement | null>(null);

const memberForm = ref<Omit<Member, 'id'> | Member>(
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

const rules = {
  required: (value: unknown) => !!value || t('validation.required'),
};

const dateOfDeathRule = (value: string | Date | null) => {
  if (!value) return true;
  const dateOfDeath = memberForm.value.dateOfDeath;
  const dateOfBirth = memberForm.value.dateOfBirth
  if (!dateOfDeath || !dateOfBirth) return true;
  return dateOfDeath > dateOfBirth || t('validation.dateOfDeathAfterBirth');
};

// Expose form validation and data for parent component
const validate = async () => {
  if (form.value) {
    const { valid } = await form.value.validate();
    return valid;
  }
  return false;
};

const getFormData = () => {
  return memberForm.value;
};



defineExpose({
  validate,
  getFormData,
});
</script>
