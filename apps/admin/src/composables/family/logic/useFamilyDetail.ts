import { computed, toRef, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useAuth } from '@/composables';
import { useFamilyQuery } from '@/composables';

interface UseFamilyDetailDeps {
  useI18n: typeof useI18n;
  useRouter: typeof useRouter;
  useAuth: typeof useAuth;
  useFamilyQuery: (familyIdRef: Ref<string>) => ReturnType<typeof useFamilyQuery>;
}

const defaultDeps: UseFamilyDetailDeps = {
  useI18n,
  useRouter,
  useAuth,
  useFamilyQuery,
};

export function useFamilyDetail(
  props: { familyId: string; readOnly: boolean },
  emit: (event: 'openEditDrawer', id: string) => void,
  deps: UseFamilyDetailDeps = defaultDeps,
) {
  const { useRouter, useAuth, useFamilyQuery } = deps;
  const router = useRouter();
  const { state } = useAuth();

  const familyIdRef = toRef(props, 'familyId');
  const { family: familyData, isLoading, error } = useFamilyQuery(familyIdRef);

  const canManageFamily = computed(() => {
    return state.isAdmin.value || state.isFamilyManager.value(props.familyId);
  });

  const openEditDrawer = () => {
    emit('openEditDrawer', props.familyId);
  };

  const closeView = () => {
    router.push('/family');
  };

  return {
    state: {
      familyData,
      isLoading,
      error,
      canManageFamily,
    },
    actions: {
      openEditDrawer,
      closeView,
    },
  };
}
