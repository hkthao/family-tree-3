<template>
    <v-form ref="form" :disabled="props.readOnly">
      <!-- Thông tin cơ bản -->
      <v-row>
        <v-col cols="12">
          <AvatarInput
            v-if="!props.readOnly"
            v-model="memberForm.avatarUrl"
            :size="96"
          />
          <div v-else class="d-flex justify-center mb-4">
            <AvatarDisplay :src="memberForm.avatarUrl" :size="96" />
          </div>
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12" md="4">
          <v-text-field
            v-model="memberForm.lastName"
            :label="t('member.form.lastName')"
            :rules="[rules.required]"
            :readonly="props.readOnly"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4">
          <v-text-field
            v-model="memberForm.firstName"
            :label="t('member.form.firstName')"
            :rules="[rules.required]"
            :readonly="props.readOnly"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4">
          <v-text-field
            v-model="memberForm.nickname"
            :label="t('member.form.nickname')"
            :readonly="props.readOnly"
          ></v-text-field>
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12" md="6">
          <DateInputField
            v-model="memberForm.dateOfBirth"
            :label="t('member.form.dateOfBirth')"
            :rules="[rules.required]"
            :readonly="props.readOnly"
          />
        </v-col>
        <v-col cols="12" md="6">
          <DateInputField
            v-model="memberForm.dateOfDeath"
            :label="t('member.form.dateOfDeath')"
            optional
            :readonly="props.readOnly"
            :rules="[dateOfDeathRule]"
          />
        </v-col>
      </v-row>

      <!-- Thông tin cá nhân -->
      <v-row>
        <v-col cols="12" md="4">
          <GenderSelect
            v-model="memberForm.gender"
            :label="t('member.form.gender')"
            :rules="[rules.required]"
            :read-only="props.readOnly"
          />
        </v-col>
        <v-col cols="12" md="4">
          <v-text-field
            v-model="memberForm.placeOfBirth"
            :label="t('member.form.placeOfBirth')"
            :readonly="props.readOnly"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4">
          <v-text-field
            v-model="memberForm.placeOfDeath"
            :label="t('member.form.placeOfDeath')"
            :readonly="props.readOnly"
          ></v-text-field>
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12">
          <v-text-field
            v-model="memberForm.occupation"
            :label="t('member.form.occupation')"
            :readonly="props.readOnly"
          ></v-text-field>
        </v-col>
      </v-row>

      <v-row>
        <v-col cols="12">
          <FamilyAutocomplete
            v-model="memberForm.familyId"
            :label="t('member.form.familyId')"
            :rules="[rules.required]"
            :readonly="props.readOnly"
            :multiple="false"
          />
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12" md="4">
          <MemberAutocomplete
            v-model="memberForm.fatherId"
            :label="t('member.form.father')"
            :readonly="props.readOnly || !memberForm.familyId"
            :additional-filters="{
              familyId: memberForm.familyId,
              gender: Gender.Male,
            }"
            :multiple="false"
          />
        </v-col>
        <v-col cols="12" md="4">
          <MemberAutocomplete
            v-model="memberForm.motherId"
            :label="t('member.form.mother')"
            :readonly="props.readOnly || !memberForm.familyId"
            :additional-filters="{
              familyId: memberForm.familyId,
              gender: Gender.Female,
            }"
            :multiple="false"
          />
        </v-col>
        <v-col cols="12" md="4">
          <MemberAutocomplete
            v-model="memberForm.spouseId"
            :label="t('member.form.spouse')"
            :readonly="props.readOnly || !memberForm.familyId"
            :additional-filters="{ familyId: memberForm.familyId }"
            :multiple="false"
          />
        </v-col>
      </v-row>

      <!-- Thông tin khác -->
      <v-row>
        <v-col cols="12">
          <v-textarea
            v-model="memberForm.biography"
            :label="t('member.form.biography')"
            :readonly="props.readOnly"
          ></v-textarea>
        </v-col>
      </v-row>
    </v-form>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';

import type { Member } from '@/types/family';
import { Gender } from '@/types';

import { DateInputField, GenderSelect, FamilyAutocomplete, MemberAutocomplete, AvatarInput, AvatarDisplay } from '@/components/common';

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
        fatherId: null,
        motherId: null,
        spouseId: null,
      },
);

const rules = {
  required: (value: unknown) => !!value || t('validation.required'),
};

const dateOfDeathRule = (value: string | Date | null) => {
  if (!value) return true;
  if (!memberForm.value.dateOfBirth) return true;

  const dateOfDeath = typeof value === 'string' ? new Date(value) : value;
  const dateOfBirth =
    typeof memberForm.value.dateOfBirth === 'string'
      ? new Date(memberForm.value.dateOfBirth)
      : memberForm.value.dateOfBirth;

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

const emit = defineEmits(['close']);

defineExpose({
  validate,
  getFormData,
});
</script>
