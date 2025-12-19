import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import type { SearchResult } from '@/types';

export function useFaceResultList(_props: {
  results: SearchResult[];
}) {
  const { t } = useI18n();
  const router = useRouter();

  const goToMemberProfile = (memberId: string) => {
    router.push({ name: 'MemberDetail', params: { id: memberId } });
  };

  return {
    t,
    goToMemberProfile,
  };
}