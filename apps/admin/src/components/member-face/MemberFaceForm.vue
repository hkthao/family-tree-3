<template>
  <v-form ref="formRef" @submit.prevent>
      <v-row>
        <v-col cols="12" md="6">
          <v-text-field v-model="state.memberId" :label="t('memberFace.form.memberId')"
            :readonly="readOnly || !!props.memberId" :rules="rules.memberId"
            data-testid="memberFace-memberId"></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field v-model="state.faceId" :label="t('memberFace.form.faceId')" :readonly="readOnly"
            data-testid="memberFace-faceId"></v-text-field>
        </v-col>
        <v-col cols="12" sm="6">
          <v-text-field v-model.number="state.boundingBox.x" :label="t('memberFace.form.boundingBoxX')"
            :readonly="readOnly" type="number"></v-text-field>
        </v-col>
        <v-col cols="12" sm="6">
          <v-text-field v-model.number="state.boundingBox.y" :label="t('memberFace.form.boundingBoxY')"
            :readonly="readOnly" type="number"></v-text-field>
        </v-col>
        <v-col cols="12" sm="6">
          <v-text-field v-model.number="state.boundingBox.width" :label="t('memberFace.form.boundingBoxWidth')"
            :readonly="readOnly" type="number"></v-text-field>
        </v-col>
        <v-col cols="12" sm="6">
          <v-text-field v-model.number="state.boundingBox.height" :label="t('memberFace.form.boundingBoxHeight')"
            :readonly="readOnly" type="number"></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field v-model.number="state.confidence" :label="t('memberFace.form.confidence')" :readonly="readOnly"
            type="number" step="0.01"></v-text-field>
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
  </v-form>
</template>
<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import type { MemberFace } from '@/types';
import { useMemberFaceForm } from '@/composables';
import { ref } from 'vue';
import type { VForm } from 'vuetify/components';

interface MemberFaceFormProps {
  initialMemberFaceData?: MemberFace;
  readOnly?: boolean;
  memberId?: string;
  familyId?: string;
}
const props = defineProps<MemberFaceFormProps>();
const { t } = useI18n();

const formRef = ref<VForm | null>(null);

const { state, validate, getFormData, rules } = useMemberFaceForm(
  {
    initialMemberFaceData: props.initialMemberFaceData,
    memberId: props.memberId,
  },
  formRef
);

defineExpose({
  validate,
  getFormData,
});
</script>