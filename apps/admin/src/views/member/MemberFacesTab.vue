<template>
  <div>
    <div v-if="isLoading" class="text-center py-4">
      <v-progress-circular indeterminate color="primary"></v-progress-circular>
      <p>{{ t('common.loading') }}</p>
    </div>
    <v-alert v-else-if="error" type="error" class="mb-4">{{ error?.message || t('memberFace.errors.loadList')
      }}</v-alert>
    <v-list v-else-if="memberFaces && memberFaces.length > 0">
      <v-list-item v-for="face in memberFaces" :key="face.id" class="mb-2"
        :title="face.memberName || t('memberFace.detail.title')"
        :subtitle="face.emotion ? `${t('memberFace.emotion.' + face.emotion.toLowerCase())} (${face.emotionConfidence?.toFixed(2)})` : ''">
        <template v-slot:prepend>
          <v-avatar rounded="0" size="48">
            <v-img :src="face.thumbnailUrl || '/family_avatar.png'" cover></v-img>
          </v-avatar>
        </template>
        <template v-slot:append>
          <v-btn icon variant="text" size="small" :href="face.originalImageUrl" target="_blank"
            v-if="face.originalImageUrl">
            <v-icon>mdi-image-search</v-icon>
            <v-tooltip activator="parent" location="top">{{ t('memberFace.form.viewOriginal') }}</v-tooltip>
          </v-btn>
        </template>
      </v-list-item>
    </v-list>
    <v-alert v-else type="info" variant="tonal" class="ma-2">{{ t('memberFace.list.noFacesFound') }}</v-alert>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { useMemberFacesQuery } from '@/composables/member-face/useMemberFacesQuery';

interface MemberFacesTabProps {
  memberId: string;
}

const props = defineProps<MemberFacesTabProps>();
const { t } = useI18n();

const { data: memberFaces, isLoading, error } = useMemberFacesQuery(props.memberId);
</script>

<style scoped></style>