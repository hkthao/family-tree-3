<template>
  <v-form ref="formRef" :disabled="isFormReadOnly" data-testid="member-form">
    <!-- Thông tin cơ bản -->
    <v-row>
      <v-col cols="12">
        <AvatarInput v-if="!isFormReadOnly" v-model="formData.avatarBase64" :size="96" :initial-avatar="initialAvatarDisplay" />
        <div v-else class="d-flex justify-center mb-4">
          <AvatarDisplay :src="getAvatarUrl(formData.avatarUrl, formData.gender)" :size="96" />
        </div>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <family-auto-complete v-model="formData.familyId" :label="t('member.form.familyId')"
          :rules="validationRules.familyId"
          :multiple="false" :disabled="true" data-testid="member-family-select" />
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" md="6">
        <v-text-field v-model="formData.lastName" :label="t('member.form.lastName')"
          :rules="validationRules.lastName"
          :readonly="isFormReadOnly" :disabled="isFormReadOnly" data-testid="member-last-name-input"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model="formData.firstName" :label="t('member.form.firstName')"
          :rules="validationRules.firstName"
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
          :rules="validationRules.dateOfBirth" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="member-date-of-birth-input" append-inner-icon="mdi-calendar" />
      </v-col>
      <v-col cols="12" md="4">
        <v-date-input v-model="formData.dateOfDeath" :label="t('member.form.dateOfDeath')" optional
          :rules="validationRules.dateOfDeath" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="member-date-of-death-input" append-inner-icon="mdi-calendar" />
      </v-col>
    </v-row>

    <!-- Thông tin cá nhân -->
    <v-row>
      <v-col cols="12" md="6">
        <v-text-field v-model="formData.phone" :label="t('member.form.phone')" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="member-phone-input"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model="formData.email" :label="t('member.form.email')" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="member-email-input"></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-text-field v-model="formData.address" :label="t('member.form.address')" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="member-address-input"></v-text-field>
      </v-col>
    </v-row>
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
      <v-col cols="12" md="4">
        <v-checkbox v-model="formData.isRoot" :label="t('member.form.isRoot')" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="member-is-root-checkbox"></v-checkbox>
      </v-col>
      <v-col cols="12" md="4">
        <v-checkbox v-model="formData.isDeceased" :label="t('member.form.isDeceased')" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="member-is-deceased-checkbox"></v-checkbox>
      </v-col>
     
    </v-row>

     <!-- Thông tin Cha Mẹ -->
    <v-row>
       <v-col cols="12" md="12">
        <v-text-field v-model.number="formData.order" :label="t('member.form.order')" :rules="validationRules.order" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" type="number" min="1" data-testid="member-order-input"></v-text-field>
      </v-col>
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
import { useI18n } from 'vue-i18n';
import type { MemberDto } from '@/types';
import { GenderSelect, AvatarInput, AvatarDisplay } from '@/components/common';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';
import { useMemberFormComposable } from '@/composables/member/useMemberFormComposable';

const props = defineProps<{
  readOnly?: boolean;
  initialMemberData?: MemberDto | null;
  familyId: string | null;
}>();

const { t } = useI18n();

const {
  formRef,
  formData,
  isFormReadOnly,
  initialAvatarDisplay,
  validate,
  getFormData,
  getAvatarUrl,
  validationRules,
} = useMemberFormComposable({
  readOnly: props.readOnly,
  initialMemberData: props.initialMemberData,
  familyId: props.familyId,
});

defineExpose({
  validate,
  getFormData,
});
</script>
