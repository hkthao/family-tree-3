import { ref, reactive, watch, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyDto, FamilyAddDto, FamilyUpdateDto } from '@/types';
import { FamilyVisibility } from '@/types';
import { useFamilyRules } from '@/validations/family.validation';
import { getFamilyAvatarUrl } from '@/utils/avatar.utils';
import type { VForm } from 'vuetify/components';
import { getFamilyVisibilityOptions } from '@/composables/utils/familyOptions';

interface UseFamilyFormProps {
  data?: FamilyDto;
  readOnly?: boolean;
}

export function useFamilyForm(props: UseFamilyFormProps, formRef: Ref<VForm | null>) {
  const { t } = useI18n();

  const isEditMode = ref(!!props.data);

  // Initial state for formData
  const initialFormData = (): FamilyAddDto | FamilyUpdateDto | FamilyDto => {
    if (props.data) {
      return {
        id: props.data.id,
        name: props.data.name,
        code: props.data.code,
        description: props.data.description || '',
        address: props.data.address || '',
        genealogyRecord: props.data.genealogyRecord || '',
        progenitorName: props.data.progenitorName || '',
        familyCovenant: props.data.familyCovenant || '',
        contactInfo: props.data.contactInfo || '',
        locationId: props.data.locationId || null, // Initialize locationId
        avatarUrl: props.data.avatarUrl || '',
        avatarBase64: null,
        visibility: props.data.visibility || FamilyVisibility.Public,
        managerIds: props.data.managerIds || [],
        viewerIds: props.data.viewerIds || [],
        familyLimitConfiguration: props.data.familyLimitConfiguration,
      };
    }
    return {
      name: '',
      code: '',
      description: '',
      address: '',
      genealogyRecord: '',
      progenitorName: '',
      familyCovenant: '',
      contactInfo: '',
      locationId: null, // Initialize locationId for add mode
      avatarUrl: '',
      avatarBase64: null,
      visibility: FamilyVisibility.Public,
      managerIds: [],
      viewerIds: [],
    };
  };

  const formData = reactive<FamilyAddDto | FamilyUpdateDto | FamilyDto>(initialFormData());

  const initialAvatarDisplay = ref(formData.avatarBase64 || formData.avatarUrl);

  const rules = useFamilyRules();

  // Refs to hold User IDs selected by UserAutocomplete
  const managers = ref<string[]>([]);
  const viewers = ref<string[]>([]);

  // Keep track of initial manager/viewer IDs for calculating deletedUserIds
  const initialManagerIds = ref<string[]>([]);
  const initialViewerIds = ref<string[]>([]);

  // Initialize managers and viewers from props.data
  watch(
    () => props.data,
    (newVal) => {
      if (newVal) {
        isEditMode.value = true;
        (formData as FamilyUpdateDto).id = newVal.id;
        formData.name = newVal.name;
        formData.code = newVal.code; // Set the code field
        formData.description = newVal.description || '';
        formData.address = newVal.address || '';
        formData.genealogyRecord = newVal.genealogyRecord || '';
        formData.progenitorName = newVal.progenitorName || '';
        formData.familyCovenant = newVal.familyCovenant || '';
        formData.contactInfo = newVal.contactInfo || '';
        formData.locationId = newVal.locationId || null;
        formData.avatarUrl = newVal.avatarUrl || '';
        formData.visibility = newVal.visibility || FamilyVisibility.Public;
        formData.familyLimitConfiguration = newVal.familyLimitConfiguration;

        initialAvatarDisplay.value = formData.avatarBase64 || formData.avatarUrl;
        managers.value = newVal.managerIds || [];
        viewers.value = newVal.viewerIds || [];
        initialManagerIds.value = [...(newVal.managerIds || [])]; // Store initial IDs
        initialViewerIds.value = [...(newVal.viewerIds || [])]; // Store initial IDs
      } else {
        isEditMode.value = false;
        Object.assign(formData, initialFormData()); // Reset form data for add mode
        managers.value = [];
        viewers.value = [];
        initialManagerIds.value = [];
        initialViewerIds.value = [];
        initialAvatarDisplay.value = '';
      }
    },
    { deep: true, immediate: true } // immediate: true to run on component mount
  );

  const visibilityItems = getFamilyVisibilityOptions(t);

  const validate = async () => {
    return (await formRef.value?.validate())?.valid || false;
  };

  const getFormData = (): FamilyAddDto | FamilyUpdateDto => {
    const dataToSubmit = {
      ...formData,
      managerIds: managers.value,
      viewerIds: viewers.value,
    };

    if (isEditMode.value && 'id' in formData) {
      // Calculate deleted manager IDs
      const deletedManagerIds = initialManagerIds.value.filter(
        (id) => !managers.value.includes(id)
      );
      (dataToSubmit as FamilyUpdateDto).deletedManagerIds = deletedManagerIds;

      // Calculate deleted viewer IDs
      const deletedViewerIds = initialViewerIds.value.filter(
        (id) => !viewers.value.includes(id)
      );
      (dataToSubmit as FamilyUpdateDto).deletedViewerIds = deletedViewerIds;
    }
    return dataToSubmit;
  };

      return {
        state: {
          formData,
          initialAvatarDisplay,
          managers,
          viewers,
          visibilityItems,
          getFamilyAvatarUrl,
          rules,
          isLoadingUsers: ref(false),
          isEditMode, // Export isEditMode
        },
        actions: {
          validate,
          getFormData,
        },
      };}
