import { ref, watch, nextTick } from 'vue';
import { useI18n } from 'vue-i18n';
// VFileUpload is not used in the composable's logic, so it can be removed
// import { VFileUpload } from 'vuetify/labs/VFileUpload';

export function useFaceUploadInput(props: {
  label?: string;
  accept: string;
  multiple: boolean;
}, emit: (event: 'file-uploaded', file: File | File[] | null) => void) {
  const { t } = useI18n();

  const fileUploadRef = ref<any | null>(null); // Use any for VFileUpload instance
  const files = ref<File[]>([]);
  let isClearing = false; // Flag to prevent recursive updates

  watch(files, (newFiles) => {
    if (isClearing) {
      // If we are programmatically clearing, do not emit
      return;
    }

    if (newFiles && newFiles.length > 0) {
      if (props.multiple) {
        emit('file-uploaded', newFiles);
      } else {
        emit('file-uploaded', newFiles[0]);
      }
    } else {
      emit('file-uploaded', null);
    }
  });

  const reset = () => {
    isClearing = true;
    files.value = [];
    nextTick(() => {
      isClearing = false;
    });
  };

  return {
    t,
    fileUploadRef,
    files,
    reset,
  };
}