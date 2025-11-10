<template>
  <v-card>
    <v-card-title class="text-h5 text-uppercase">
      {{ t('naturalLanguage.confirmation.title') }}
    </v-card-title>
    <v-card-subtitle>{{ t('naturalLanguage.confirmation.entityType') }}: {{ nlStore.entityType }}</v-card-subtitle>
    <v-card-text>
      <v-list dense>
        <v-list-item v-for="(value, key) in editableData" :key="key">
          <v-text-field
            :label="String(key)"
            v-model="editableData[key as keyof typeof editableData]"
            dense
            variant="outlined"
          ></v-text-field>
        </v-list-item>
      </v-list>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="cancel">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="save" :loading="loading">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useNaturalLanguageStore } from '@/stores/naturalLanguage.store';
import { useMemberStore } from '@/stores/member.store';
import { useEventStore } from '@/stores/event.store';
import { useFamilyStore } from '@/stores/family.store';
import { useRelationshipStore } from '@/stores/relationship.store';
import { useNotificationStore } from '@/stores/notification.store';
import type { Member, Event, Family, Relationship } from '@/types';

const { t } = useI18n();
const router = useRouter();
const nlStore = useNaturalLanguageStore();
const memberStore = useMemberStore();
const eventStore = useEventStore();
const familyStore = useFamilyStore();
const relationshipStore = useRelationshipStore();
const notificationStore = useNotificationStore();

const editableData = ref<any>({});
const loading = ref(false);

onMounted(() => {
  if (nlStore.parsedData) {
    editableData.value = JSON.parse(JSON.stringify(nlStore.parsedData));
  } else {
    // If there's no parsed data, redirect back to the previous page or show an error
    router.back();
  }
});

const save = async () => {
  loading.value = true;
  try {
    switch (nlStore.entityType) {
      case 'Member':
        await memberStore.addItem(editableData.value as Omit<Member, 'id'>);
        break;
      case 'Event':
        await eventStore.addItem(editableData.value as Omit<Event, 'id'>);
        break;
      case 'Family':
        await familyStore.addItem(editableData.value as Omit<Family, 'id'>);
        break;
      case 'Relationship':
        await relationshipStore.addItem(editableData.value as Omit<Relationship, 'id'>);
        break;
    }
    notificationStore.showSnackbar(t('naturalLanguage.confirmation.saveSuccess'), 'success');
    router.push(`/${nlStore.entityType?.toLowerCase()}`); // Redirect to the corresponding list view
  } catch (error) {
    notificationStore.showSnackbar(t('naturalLanguage.confirmation.saveError'), 'error');
  } finally {
    loading.value = false;
  }
};

const cancel = () => {
  router.back();
};
</script>
