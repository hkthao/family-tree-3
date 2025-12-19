import { ref, reactive, watch, type Ref, computed } from 'vue'; // Added computed
import { useI18n } from 'vue-i18n';
import type { Family, FamilyAddDto, FamilyUpdateDto } from '@/types'; // Updated imports
import { FamilyVisibility } from '@/types';
import { useFamilyRules } from '@/validations/family.validation';
import { getFamilyAvatarUrl } from '@/utils/avatar.utils';
import type { VForm } from 'vuetify/components';
import { useUserByIdsQuery } from '@/composables/user/queries/useUserByIdsQuery'; // NEW import for fetching UserDto

interface UseFamilyFormProps {
  data?: Family;
  readOnly?: boolean;
}

export function useFamilyForm(props: UseFamilyFormProps, formRef: Ref<VForm | null>) {
  const { t } = useI18n();

  const isEditMode = ref(!!props.data);

  // Initial state for formData
  const initialFormData = (): FamilyAddDto | FamilyUpdateDto => {
    if (props.data) {
      return {
        id: props.data.id,
        name: props.data.name,
        description: props.data.description || '',
        address: props.data.address || '',
        avatarUrl: props.data.avatarUrl || '',
        avatarBase64: null,
        visibility: props.data.visibility || FamilyVisibility.Public,
        managerIds: props.data.managerIds || [],
        viewerIds: props.data.viewerIds || [],

      };
    }
    return {
      name: '',
      description: '',
      address: '',
      avatarUrl: '',
      avatarBase64: null,
      visibility: FamilyVisibility.Public,
      managerIds: [],
      viewerIds: [],
    };
  };

  const formData = reactive<FamilyAddDto | FamilyUpdateDto>(initialFormData());

  const initialAvatarDisplay = ref(formData.avatarBase64 || formData.avatarUrl);

  const rules = useFamilyRules();

  // Refs to hold User IDs selected by UserAutocomplete
  const managers = ref<string[]>([]);
  const viewers = ref<string[]>([]);

  // Keep track of initial manager/viewer IDs for calculating deletedUserIds
  const initialManagerIds = ref<string[]>([]);
  const initialViewerIds = ref<string[]>([]);

  // Fetch initial UserDtos for display in UserAutocomplete (not directly bound to v-model)
  // useUserByIdsQuery returns UserDto[]
  const { users: fetchedManagersForDisplay, isLoading: isLoadingManagers } = useUserByIdsQuery(
    computed(() => (isEditMode.value ? (props.data?.managerIds || []) : []))
  );
  const { users: fetchedViewersForDisplay, isLoading: isLoadingViewers } = useUserByIdsQuery(
    computed(() => (isEditMode.value ? (props.data?.viewerIds || []) : []))
  );

  // Update managers and viewers (ID arrays) when initial data is fetched
  watch(fetchedManagersForDisplay, (newManagers) => {
    if (newManagers) {
      managers.value = newManagers.map(u => u.id);
      initialManagerIds.value = newManagers.map(u => u.id); // Set initial IDs for tracking
    }
  }, { immediate: true });

  watch(fetchedViewersForDisplay, (newViewers) => {
    if (newViewers) {
      viewers.value = newViewers.map(u => u.id);
      initialViewerIds.value = newViewers.map(u => u.id); // Set initial IDs for tracking
    }
  }, { immediate: true });

  // Watch for external data changes (e.g., when editing a different family)
  watch(
    () => props.data,
    (newVal) => {
      if (newVal) {
        isEditMode.value = true;
        Object.assign(formData, initialFormData()); // Reset form data
        initialAvatarDisplay.value = formData.avatarBase64 || formData.avatarUrl;
        // The watchers for fetchedManagersForDisplay and fetchedViewersForDisplay will handle updating managers.value and viewers.value (ID arrays)
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
    { deep: true }
  );

  const visibilityItems = computed(() => [
    {
      title: t('family.form.visibility.private'),
      value: FamilyVisibility.Private,
    },
    { title: t('family.form.visibility.public'), value: FamilyVisibility.Public },
  ]);

  const validate = async () => {
    return (await formRef.value?.validate())?.valid || false;
  };

  const getFormData = (): FamilyAddDto | FamilyUpdateDto => {
    const dataToSubmit = {
      ...formData,
      managerIds: managers.value, // Now directly an array of IDs
      viewerIds: viewers.value,   // Now directly an array of IDs
    };

    if (isEditMode.value && 'id' in formData) {
      // Calculate deleted manager IDs
      const deletedManagerIds = initialManagerIds.value.filter(
        id => !managers.value.includes(id)
      );
      (dataToSubmit as FamilyUpdateDto).deletedManagerIds = deletedManagerIds;

      // Calculate deleted viewer IDs
      const deletedViewerIds = initialViewerIds.value.filter(
        id => !viewers.value.includes(id)
      );
      (dataToSubmit as FamilyUpdateDto).deletedViewerIds = deletedViewerIds;
    }
    return dataToSubmit;
  };

  return {
    t,
    formData,
    initialAvatarDisplay,
    managers,
    viewers,
    visibilityItems,
    validate,
    getFormData,
    getFamilyAvatarUrl,
    rules,
    isLoadingUsers: computed(() => isLoadingManagers.value || isLoadingViewers.value),
  };
}
