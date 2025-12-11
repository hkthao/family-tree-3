import { useQuery } from '@tanstack/vue-query';
import { queryKeys } from '@/constants/queryKeys';
import { useAuthService } from '@/services/auth/authService';

export function useAccessToken() {
  const authService = useAuthService();

  const query = useQuery<string | null>({
    queryKey: queryKeys.auth.accessToken,
    queryFn: async () => {
      const token = await authService.getAccessToken();
      return token;
    },
    // Token is usually long-lived or refreshed by Auth0 SDK, so no aggressive refetching needed
    staleTime: Infinity, // Access token is managed by Auth0 SDK, consider it always fresh

  });

  return {
    accessToken: query.data,
    isLoading: query.isLoading,
    isError: query.isError,
    error: query.error,
    isFetching: query.isFetching,
    refetch: query.refetch,
  };
}
