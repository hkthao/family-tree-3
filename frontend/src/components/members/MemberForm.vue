<template>
  <v-card-text>
    <v-form
      ref="form"
      :disabled="props.readOnly"
    >
      <!-- Thông tin cơ bản -->
      <v-row>
        <v-col cols="12">
          <div class="d-flex justify-center mb-4">
            <v-avatar size="96">
              <v-img
                v-if="memberForm.avatarUrl"
                :src="memberForm.avatarUrl"
              ></v-img>
              <v-icon v-else size="96">mdi-account-circle</v-icon>
            </v-avatar>
          </div>
          <v-text-field
            v-model="memberForm.avatarUrl"
            :label="t('member.form.avatarUrl')"
            :readonly="props.readOnly"
          ></v-text-field>
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
            :readonly="true"
            :multiple="false"
          />
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12" md="4">
          <MemberAutocomplete
            v-model="computedFatherId"
            :label="t('member.form.father')"
            :readonly="true"
            :disabled="!memberForm.familyId"
            :additional-filters="{
              familyId: memberForm.familyId,
              gender: Gender.Male,
            }"
            :multiple="false"
          />
        </v-col>
        <v-col cols="12" md="4">
          <MemberAutocomplete
            v-model="computedMotherId"
            :label="t('member.form.mother')"
            :readonly="props.readOnly"
            :disabled="!memberForm.familyId"
            :additional-filters="{
              familyId: memberForm.familyId,
              gender: Gender.Female,
            }"
            :multiple="false"
          />
        </v-col>
        <v-col cols="12" md="4">
          <MemberAutocomplete
            v-model="computedSpouseId"
            :label="t('member.form.spouse')"
            :readonly="true"
            :disabled="!memberForm.familyId"
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
  </v-card-text>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import type { Member } from '@/types/family';
import { useI18n } from 'vue-i18n';
import { DateInputField, GenderSelect, Lookup } from '@/components/common';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue'; // Import FamilyAutocomplete
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue'; // Import MemberAutocomplete
import { useFamilyStore } from '@/stores/family.store';
import { useMemberStore } from '@/stores/member.store';
import { Gender } from '@/types/gender';

const props = defineProps<{
  readOnly?: boolean;
  initialMemberData?: Member;
}>();

const emit = defineEmits(['close']);

const { t } = useI18n();
const familyStore = useFamilyStore();
const memberStore = useMemberStore();

const form = ref<HTMLFormElement | null>(null);

const memberForm = ref<Omit<Member, 'id'> | Member>(
  props.initialMemberData
    ? {
        ...props.initialMemberData,
        fatherId:
          props.initialMemberData.fatherId === undefined
            ? null
            : props.initialMemberData.fatherId,
        motherId:
          props.initialMemberData.motherId === undefined
            ? null
            : props.initialMemberData.motherId,
        spouseId:
          props.initialMemberData.spouseId === undefined
            ? null
            : props.initialMemberData.spouseId,
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

watch(
  () => memberForm.value.familyId,
  (newFamilyId) => {
    // Clear father, mother, and spouse when familyId changes
    if (memberForm.value.fatherId != newFamilyId) {
      memberForm.value.fatherId = null;
      memberForm.value.motherId = null;
      memberForm.value.spouseId = null;
    }
  },
  { immediate: true },
); // Immediate to load members for initial familyId

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

defineExpose({
  validate,
  getFormData,
});

const closeForm = () => {
  emit('close');
};

const computedFatherId = computed<string | undefined>({
  get: () => memberForm.value.fatherId ?? undefined,
  set: (value) => {
    memberForm.value.fatherId = value ?? null;
  },
});

const computedMotherId = computed<string | undefined>({
  get: () => memberForm.value.motherId ?? undefined,
  set: (value) => {
    memberForm.value.motherId = value ?? null;
  },
});

const computedSpouseId = computed<string | undefined>({
  get: () => memberForm.value.spouseId ?? undefined,
  set: (value) => {
    memberForm.value.spouseId = value ?? null;
  },
});
</script>
