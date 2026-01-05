<template>
  <v-card flat class="mt-3">
    <div v-if="isLoadingLocations" class="d-flex justify-center align-center py-4">
      <v-progress-circular indeterminate color="primary"></v-progress-circular>
      {{ t('common.loading') }}
    </div>
    <div v-else-if="locationError">
      <v-alert type="error" :text="locationError?.message || 'Failed to load locations'"></v-alert>
    </div>
    <div v-else-if="locationLinks && locationLinks.length === 0" class="text-center py-4">
      {{ t('familyLocation.list.noData') }}
    </div>
    <v-list v-else-if="locationLinks">
      <template v-for="(locationLink, index) in locationLinks" :key="locationLink.id">
        <v-list-item @click="handleSelectLocation(locationLink)" link prepend-icon="mdi-map-marker">
          <v-list-item-title>{{ locationLink.location?.name || locationLink.location?.address }}</v-list-item-title>
          <v-list-item-subtitle>
            {{ getLocationLinkDescription(locationLink) }}
          </v-list-item-subtitle>
        </v-list-item>
        <v-divider v-if="index < locationLinks.length - 1"></v-divider>
      </template>
    </v-list>
  </v-card>
</template>

<script setup lang="ts">
import { toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { useLocationLinksByMemberIdQuery } from '@/composables/location-links/queries/useLocationLinksByMemberIdQuery';
import { type LocationLinkDto, LocationLinkType, RefType } from '@/types'; // Consolidated import

interface MemberLocationsTabProps {
  memberId: string;
}

const props = defineProps<MemberLocationsTabProps>();
const emit = defineEmits(['show-location-detail']);

const { t } = useI18n();

const memberIdRef = toRef(props, 'memberId');
const { data: locationLinks, isLoading: isLoadingLocations, error: locationError } = useLocationLinksByMemberIdQuery(memberIdRef);

const handleSelectLocation = (locationLink: LocationLinkDto) => {
  emit('show-location-detail', locationLink.locationId);
};

const getLocationLinkDescription = (locationLink: LocationLinkDto) => {
  let description = '';
  switch (locationLink.linkType) {
    case LocationLinkType.Birth:
      description = t('familyLocation.locationLinkType.birth');
      break;
    case LocationLinkType.Death:
      description = t('familyLocation.locationLinkType.death');
      break;
    case LocationLinkType.Residence:
      description = t('familyLocation.locationLinkType.residence');
      break;
    case LocationLinkType.General:
      description = t('familyLocation.locationLinkType.general');
      break;
    default:
      description = locationLink.description || '';
  }
  if (locationLink.refType === RefType.Member) {
    description += ` (${t('common.member')})`;
  } else if (locationLink.refType === RefType.Event) {
    description += ` (${t('common.event')})`;
  } else if (locationLink.refType === RefType.Family) {
    description += ` (${t('common.family')})`;
  }
  return description;
};
</script>
