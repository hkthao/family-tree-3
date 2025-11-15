<template>
  <v-form ref="formRef" :disabled="isFormReadOnly" data-testid="member-form">
    <!-- Thông tin cơ bản -->
    <v-row>
      <v-col cols="12">
        <AvatarInput v-if="!isFormReadOnly" v-model="formData.avatarUrl" :size="96" />
        <div v-else class="d-flex justify-center mb-4">
          <AvatarDisplay :src="memberAvatarSrc" :size="96" />
        </div>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <family-auto-complete v-model="formData.familyId" :label="t('member.form.familyId')"
          @blur="v$.familyId.$touch()" @update:modelValue="v$.familyId.$touch()"
          :error-messages="v$.familyId.$errors.map(e => e.$message as string)" :readonly="isFormReadOnly"
          :multiple="false" :disabled="isFormReadOnly" data-testid="member-family-select" />
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" md="6">
        <v-text-field v-model="formData.lastName" :label="t('member.form.lastName')" @blur="v$.lastName.$touch()"
          @input="v$.lastName.$touch()" :error-messages="v$.lastName.$errors.map(e => e.$message as string)"
          :readonly="isFormReadOnly" :disabled="isFormReadOnly" data-testid="member-last-name-input"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model="formData.firstName" :label="t('member.form.firstName')" @blur="v$.firstName.$touch()"
          @input="v$.firstName.$touch()" :error-messages="v$.firstName.$errors.map(e => e.$message as string)"
          :readonly="isFormReadOnly" :disabled="isFormReadOnly" data-testid="member-first-name-input"></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-text-field v-model="formData.nickname" :label="t('member.form.nickname')" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="member-nickname-input"></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" md="4">
        <GenderSelect v-model="formData.gender" :label="t('member.form.gender')" :read-only="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="member-gender-select" />
      </v-col>
      <v-col cols="12" md="4">
        <v-date-input v-model="formData.dateOfBirth" :label="t('member.form.dateOfBirth')"
          @blur="v$.dateOfBirth.$touch()" @input="v$.dateOfBirth.$touch()"
          :error-messages="v$.dateOfBirth.$errors.map(e => e.$message as string)" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="member-date-of-birth-input" append-inner-icon="mdi-calendar" />
      </v-col>
      <v-col cols="12" md="4">
        <v-date-input v-model="formData.dateOfDeath" :label="t('member.form.dateOfDeath')" optional
          @blur="v$.dateOfDeath.$touch()" @input="v$.dateOfDeath.$touch()"
          :error-messages="v$.dateOfDeath.$errors.map(e => e.$message as string)" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="member-date-of-death-input" append-inner-icon="mdi-calendar" />
      </v-col>
    </v-row>

    <!-- Thông tin cá nhân -->
    <v-row>
      <v-col cols="12">
        <v-text-field v-model="formData.placeOfBirth" :label="t('member.form.placeOfBirth')" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="member-place-of-birth-input"></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-text-field v-model="formData.placeOfDeath" :label="t('member.form.placeOfDeath')" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="member-place-of-death-input"></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-text-field v-model="formData.occupation" :label="t('member.form.occupation')" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="member-occupation-input"></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-checkbox v-model="formData.isRoot" :label="t('member.form.isRoot')" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="member-is-root-checkbox"></v-checkbox>
      </v-col>
      <v-col cols="12">
        <v-text-field v-model.number="formData.order" :label="t('member.form.order')" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" type="number" min="1" data-testid="member-order-input"></v-text-field>
      </v-col>
    </v-row>

     <!-- Thông tin Cha Mẹ -->
    <v-row>
      <v-col cols="12" md="6">
        <MemberAutocomplete v-model="formData.fatherId" :label="t('member.form.father')" :disabled="isFormReadOnly"
          :family-id="formData.familyId" data-testid="member-father-autocomplete" />
      </v-col>
      <v-col cols="12" md="6">
        <MemberAutocomplete v-model="formData.motherId" :label="t('member.form.mother')" :disabled="isFormReadOnly"
          :family-id="formData.familyId" data-testid="member-mother-autocomplete" />
      </v-col>
    </v-row>

    <!-- Thông tin Vợ/Chồng -->
    <v-row>
      <v-col cols="12" md="6">
        <MemberAutocomplete v-model="formData.husbandId" :label="t('member.form.husband')" :disabled="isFormReadOnly"
          :family-id="formData.familyId" data-testid="member-husband-autocomplete" />
      </v-col>
      <v-col cols="12" md="6">
        <MemberAutocomplete v-model="formData.wifeId" :label="t('member.form.wife')" :disabled="isFormReadOnly"
          :family-id="formData.familyId" data-testid="member-wife-autocomplete" />
      </v-col>
    </v-row>


  </v-form>
</template>

<script setup lang="ts">
import { reactive, toRefs, ref, toRef, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Member } from '@/types';
import { Gender } from '@/types'; // Import RelationshipType
import { useVuelidate } from '@vuelidate/core';
import { useMemberRules } from '@/validations/member.validation';
import { GenderSelect, AvatarInput, AvatarDisplay } from '@/components/common';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';
import { useAuth } from '@/composables/useAuth';
import maleAvatar from '@/assets/images/male_avatar.png';
import femaleAvatar from '@/assets/images/female_avatar.png';

const props = defineProps<{
  readOnly?: boolean;
  initialMemberData?: Member;
  familyId: string | null;
}>();

const { t } = useI18n();
const { isAdmin, isFamilyManager } = useAuth();

const formRef = ref<HTMLFormElement | null>(null);

const isFormReadOnly = computed(() => {
  return props.readOnly || !(isAdmin.value || isFamilyManager.value);
});

const memberAvatarSrc = computed(() => {
  if (formData.avatarUrl) {
    return formData.avatarUrl;
  }
  if (formData.gender === Gender.Male) {
    return maleAvatar;
  }
  if (formData.gender === Gender.Female) {
    return femaleAvatar;
  }
  return maleAvatar; // Fallback for 'Other' or undefined gender
});

const formData = reactive<Omit<Member, 'id'> | Member>(
  props.initialMemberData
    ? {
      ...props.initialMemberData,
      fatherId: props.initialMemberData.fatherId,
      motherId: props.initialMemberData.motherId,
      husbandId: props.initialMemberData.husbandId,
      wifeId: props.initialMemberData.wifeId,
      isRoot: props.initialMemberData.isRoot, // Add isRoot here
    }
    : {
      lastName: '',
      firstName: '',
      dateOfBirth: undefined,
      gender: Gender.Male,
      familyId: props.familyId || '', // Initialize familyId from prop, ensure it's a string
      fatherId: undefined, // Initialize fatherId
      motherId: undefined, // Initialize motherId
      husbandId: undefined, // Initialize husbandId
      wifeId: undefined, // Initialize wifeId
      isRoot: false, // Initialize isRoot for new members
      order: undefined, // Initialize order for new members
    },
);

const state = reactive({
  lastName: toRef(formData, 'lastName'),
  firstName: toRef(formData, 'firstName'),
  dateOfBirth: toRef(formData, 'dateOfBirth'),
  dateOfDeath: toRef(formData, 'dateOfDeath'),
  familyId: toRef(formData, 'familyId'),
  fatherId: toRef(formData, 'fatherId'), // Add fatherId to state
  motherId: toRef(formData, 'motherId'), // Add motherId to state
  husbandId: toRef(formData, 'husbandId'), // Add husbandId to state
  wifeId: toRef(formData, 'wifeId'), // Add wifeId to state
  isRoot: toRef(formData, 'isRoot'), // Add isRoot to state
  order: toRef(formData, 'order'), // Add order to state
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
  v$,
});
</script>
