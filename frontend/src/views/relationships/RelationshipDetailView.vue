<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <v-card v-if="relationshipStore.currentItem">
          <v-card-title class="d-flex align-center">
            {{ t('relationship.detail.title') }}
            <v-spacer></v-spacer>
            <v-btn
              icon="mdi-pencil"
              size="small"
              variant="text"
              @click="navigateToEdit"
            ></v-btn>
          </v-card-title>
          <v-card-text>
            <v-row>
              <v-col cols="12" md="6">
                <v-list-item>
                  <v-list-item-title>{{ t('relationship.form.sourceMember') }}</v-list-item-title>
                  <v-list-item-subtitle>{{ relationshipStore.currentItem.sourceMemberFullName }}</v-list-item-subtitle>
                </v-list-item>
              </v-col>
              <v-col cols="12" md="6">
                <v-list-item>
                  <v-list-item-title>{{ t('relationship.form.targetMember') }}</v-list-item-title>
                  <v-list-item-subtitle>{{ relationshipStore.currentItem.targetMemberFullName }}</v-list-item-subtitle>
                </v-list-item>
              </v-col>
              <v-col cols="12" md="6">
                <v-list-item>
                  <v-list-item-title>{{ t('relationship.form.type') }}</v-list-item-title>
                  <v-list-item-subtitle>{{ getRelationshipTypeTitle(relationshipStore.currentItem.type) }}</v-list-item-subtitle>
                </v-list-item>
              </v-col>
              <v-col cols="12" md="6">
                <v-list-item>
                  <v-list-item-title>{{ t('relationship.form.order') }}</v-list-item-title>
                  <v-list-item-subtitle>{{ relationshipStore.currentItem.order || t('common.na') }}</v-list-item-subtitle>
                </v-list-item>
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>
        <v-alert v-else type="info">{{ t('relationship.detail.noRelationshipSelected') }}</v-alert>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useRelationshipStore } from '@/stores/relationship.store';
import { getRelationshipTypeTitle } from '@/constants/relationshipTypes';

const route = useRoute();
const router = useRouter();
const { t } = useI18n();
const relationshipStore = useRelationshipStore();

const relationshipId = route.params.id as string;

onMounted(async () => {
  if (relationshipId) {
    await relationshipStore.getById(relationshipId);
  }
});

const navigateToEdit = () => {
  if (relationshipStore.currentItem) {
    router.push({ name: 'EditRelationship', params: { id: relationshipStore.currentItem.id } });
  }
};
</script>
