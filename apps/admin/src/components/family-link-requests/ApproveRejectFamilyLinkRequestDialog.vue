<template>
  <v-dialog v-model="showDialog" persistent max-width="500px">
    <v-card>
      <v-card-title class="headline text-h6">{{ dialogTitle }}</v-card-title>
      <v-card-text>
        <v-textarea
          v-model="responseMessage"
          :label="t('familyLinkRequest.form.responseMessage')"
          rows="3"
          clearable
          counter
          maxlength="500"
          data-testid="response-message-field"
        ></v-textarea>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="grey" text @click="handleClose">{{ t('common.cancel') }}</v-btn>
        <v-btn color="primary" @click="handleConfirm">{{ t('common.confirm') }}</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';

interface ApproveRejectFamilyLinkRequestDialogProps {
  show: boolean;
  actionType: 'approve' | 'reject';
}

const props = defineProps<ApproveRejectFamilyLinkRequestDialogProps>();
const emit = defineEmits(['update:show', 'confirm']);

const { t } = useI18n();

const showDialog = ref(props.show);
const responseMessage = ref<string | null>(null);

const dialogTitle = computed(() => {
  if (props.actionType === 'approve') {
    return t('familyLinkRequest.list.confirmApprove.title');
  }
  return t('familyLinkRequest.list.confirmReject.title');
});

watch(() => props.show, (newVal) => {
  showDialog.value = newVal;
  if (newVal) {
    responseMessage.value = null; // Clear message when opening
  }
});

watch(showDialog, (newVal) => {
  emit('update:show', newVal);
});

const handleClose = () => {
  showDialog.value = false;
};

const handleConfirm = () => {
  emit('confirm', responseMessage.value);
  showDialog.value = false;
};
</script>