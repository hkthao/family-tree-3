import { ref, watch, toValue, type Ref } from 'vue';
import { useServices } from '@/plugins/services.plugin';
import type { FamilyFollowDto } from '@/types/familyFollow';
import type { Result } from '@/types';

export function useFamilyFollow(familyId: Ref<string>) {
  const { familyFollow: familyFollowService } = useServices();
  const isFollowing = ref(false);
  const familyFollowData = ref<FamilyFollowDto | null>(null);
  const isLoading = ref(false);
  const error = ref<any>(null);

  const fetchFollowStatus = async () => {
    isLoading.value = true;
    error.value = null;
    try {
      const result: Result<FamilyFollowDto> = await familyFollowService.getFollowStatus(toValue(familyId));
      if (result.ok) {
        familyFollowData.value = result.value;
        isFollowing.value = result.value.isFollowing;
      } else {
        isFollowing.value = false;
        familyFollowData.value = null;
        if (result.error?.name !== 'FamilyFollowStatusError') { // Don't show error if it's just not following
          error.value = result.error;
        }
      }
    } catch (err) {
      error.value = err;
      isFollowing.value = false;
      familyFollowData.value = null;
    } finally {
      isLoading.value = false;
    }
  };

  const followFamily = async () => {
    isLoading.value = true;
    error.value = null;
    try {
      const result: Result<string> = await familyFollowService.followFamily({
        familyId: toValue(familyId),
        notifyDeathAnniversary: true, // Default to true
        notifyBirthday: true, // Default to true
        notifyEvent: true, // Default to true
      });
      if (result.ok) {
        // After following, refetch status to update local state
        await fetchFollowStatus();
      } else {
        error.value = result.error;
      }
    } catch (err) {
      error.value = err;
    } finally {
      isLoading.value = false;
    }
  };

  const unfollowFamily = async () => {
    isLoading.value = true;
    error.value = null;
    try {
      const result: Result<boolean> = await familyFollowService.unfollowFamily(toValue(familyId));
      if (result.ok) {
        isFollowing.value = false;
        familyFollowData.value = null;
      } else {
        error.value = result.error;
      }
    } catch (err) {
      error.value = err;
    } finally {
      isLoading.value = false;
    }
  };

  watch(familyId, (newFamilyId) => {
    if (newFamilyId) {
      fetchFollowStatus();
    } else {
      isFollowing.value = false;
      familyFollowData.value = null;
    }
  }, { immediate: true });

  return {
    state: {
      isFollowing,
      familyFollowData,
      isLoading,
      error,
    },
    actions: {
      fetchFollowStatus,
      followFamily,
      unfollowFamily,
    },
  };
}
