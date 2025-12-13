<template>
  <v-form ref="form" @submit.prevent>
    <v-container>
      <v-row>
        <v-col cols="12" md="6">
          <v-text-field v-model="state.memberId" :label="t('memberFace.form.memberId')"
            :readonly="readOnly || !!props.memberId"
            :error-messages="v$.memberId.$errors.map((e: ErrorObject) => e.$message as string)" @blur="v$.memberId.$touch()"
            @input="v$.memberId.$touch()" data-testid="memberFace-memberId"></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field v-model="state.faceId" :label="t('memberFace.form.faceId')" :readonly="readOnly"
            :error-messages="v$.faceId.$errors.map((e: ErrorObject) => e.$message as string)" @blur="v$.faceId.$touch()"
            @input="v$.faceId.$touch()" data-testid="memberFace-faceId"></v-text-field>
        </v-col>
        <v-col cols="12" sm="6">
          <v-text-field v-model.number="state.boundingBox.x" :label="t('memberFace.form.boundingBoxX')"
            :readonly="readOnly" type="number" :error-messages="v$.boundingBox.x.$errors.map((e: ErrorObject) => e.$message as string)"
            @blur="v$.boundingBox.x.$touch()" @input="v$.boundingBox.x.$touch()"></v-text-field>
        </v-col>
        <v-col cols="12" sm="6">
          <v-text-field v-model.number="state.boundingBox.y" :label="t('memberFace.form.boundingBoxY')"
            :readonly="readOnly" type="number" :error-messages="v$.boundingBox.y.$errors.map((e: ErrorObject) => e.$message as string)"
            @blur="v$.boundingBox.y.$touch()" @input="v$.boundingBox.y.$touch()"></v-text-field>
        </v-col>
        <v-col cols="12" sm="6">
          <v-text-field v-model.number="state.boundingBox.width" :label="t('memberFace.form.boundingBoxWidth')"
            :readonly="readOnly" type="number"
            :error-messages="v$.boundingBox.width.$errors.map((e: ErrorObject) => e.$message as string)"
            @blur="v$.boundingBox.width.$touch()" @input="v$.boundingBox.width.$touch()"></v-text-field>
        </v-col>
        <v-col cols="12" sm="6">
          <v-text-field v-model.number="state.boundingBox.height" :label="t('memberFace.form.boundingBoxHeight')"
            :readonly="readOnly" type="number"
            :error-messages="v$.boundingBox.height.$errors.map((e: ErrorObject) => e.$message as string)"
            @blur="v$.boundingBox.height.$touch()" @input="v$.boundingBox.height.$touch()"></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field v-model.number="state.confidence" :label="t('memberFace.form.confidence')" :readonly="readOnly"
            type="number" step="0.01" :error-messages="v$.confidence.$errors.map((e: ErrorObject) => e.$message as string)"
            @blur="v$.confidence.$touch()" @input="v$.confidence.$touch()"></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field v-model="state.thumbnailUrl" :label="t('memberFace.form.thumbnailUrl')" :readonly="readOnly"
            data-testid="memberFace-thumbnailUrl"></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field v-model="state.originalImageUrl" :label="t('memberFace.form.originalImageUrl')"
            :readonly="readOnly" data-testid="memberFace-originalImageUrl"></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field v-model="state.emotion" :label="t('memberFace.form.emotion')" :readonly="readOnly"
            data-testid="memberFace-emotion"></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field v-model.number="state.emotionConfidence" :label="t('memberFace.form.emotionConfidence')"
            :readonly="readOnly" type="number" step="0.01"></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-checkbox v-model="state.isVectorDbSynced" :label="t('memberFace.form.isVectorDbSynced')"
            :readonly="readOnly"></v-checkbox>
        </v-col>
        <v-col cols="12">
          <v-textarea v-model="state.embedding" :label="t('memberFace.form.embedding')" :readonly="readOnly" auto-grow
            rows="2" data-testid="memberFace-embedding"></v-textarea>
        </v-col>
      </v-row>
    </v-container>
  </v-form>
</template>
<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import type { ErrorObject } from '@vuelidate/core';
import type { MemberFace } from '@/types';
import { useMemberFaceForm } from '@/composables/member-face/useMemberFaceForm';

interface MemberFaceFormProps {
  initialMemberFaceData?: MemberFace;
  readOnly?: boolean;
  memberId?: string;
  familyId?: string;
}
const props = defineProps<MemberFaceFormProps>();
const { t } = useI18n();

const { state, v$, validate, getFormData } = useMemberFaceForm({
  initialMemberFaceData: props.initialMemberFaceData,
  memberId: props.memberId,
});

defineExpose({
  validate,
  getFormData,
});
</script>