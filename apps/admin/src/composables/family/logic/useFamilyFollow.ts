import { ref, watch, toValue, type Ref } from 'vue';
import { useServices } from '@/plugins/services.plugin';
import type { FamilyFollowDto, FollowFamilyCommand, UpdateFamilyFollowSettingsCommand } from '@/types/familyFollow';
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

  const followFamily = async (command: FollowFamilyCommand): Promise<Result<string>> => {
    isLoading.value = true;
    error.value = null;
    try {
      const result: Result<string> = await familyFollowService.followFamily(command);
      if (result.ok) {
        await fetchFollowStatus(); // Refetch status to update local state
      } else {
        error.value = result.error;
      }
      return result;
    } catch (err: any) {
      error.value = err;
      return { ok: false, error: { name: 'ApiError', message: err.message || 'An unexpected error occurred during followFamily.' } };
    } finally {
      isLoading.value = false;
    }
  };

  const updateFamilyFollowSettings = async (familyIdValue: string, command: UpdateFamilyFollowSettingsCommand): Promise<Result<boolean>> => {
    isLoading.value = true;
    error.value = null;
    try {
      const result: Result<boolean> = await familyFollowService.updateFamilyFollowSettings(familyIdValue, command);
      if (result.ok) {
        await fetchFollowStatus(); // Refetch status to update local state
      } else {
        error.value = result.error;
      }
      return result;
    } catch (err: any) {
      error.value = err;
      return { ok: false, error: { name: 'ApiError', message: err.message || 'An unexpected error occurred during updateFamilyFollowSettings.' } };
    } finally {
      isLoading.value = false;
    }
  };

  const unfollowFamily = async (): Promise<Result<boolean>> => {
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
      return result; // Explicitly return result here
    } catch (err: any) { // Add any type to err for broader compatibility
      error.value = err;
      return { ok: false, error: { name: 'ApiError', message: err.message || 'An unexpected error occurred during unfollowFamily.' } }; // Return an error result
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
      updateFamilyFollowSettings,
      unfollowFamily,
    },
  };
}
