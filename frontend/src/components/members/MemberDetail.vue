<template>
  <v-card>
    <v-card-title class="d-flex align-center">
      <v-avatar size="48" class="mr-4">
        <v-img v-if="member.avatarUrl" :src="member.avatarUrl"></v-img>
        <v-icon v-else>mdi-account-circle</v-icon>
      </v-avatar>
      <span class="text-h5">{{ member.fullName }}</span>
      <v-spacer></v-spacer>
      <v-btn icon @click="$emit('close')">
        <v-icon>mdi-close</v-icon>
      </v-btn>
    </v-card-title>
    <v-card-text>
      <v-row>
        <v-col cols="12" md="6">
          <v-list dense>
            <v-list-item>
              <v-list-item-title><strong>{{ t('member.detail.dateOfBirth') }}:</strong> {{ member.dateOfBirth }}</v-list-item-title>
            </v-list-item>
            <v-list-item v-if="member.dateOfDeath">
              <v-list-item-title><strong>{{ t('member.detail.dateOfDeath') }}:</strong> {{ member.dateOfDeath }}</v-list-item-title>
            </v-list-item>
            <v-list-item>
              <v-list-item-title><strong>{{ t('member.detail.gender') }}:</strong> {{ member.gender }}</v-list-item-title>
            </v-list-item>
            <v-list-item v-if="member.placeOfBirth">
              <v-list-item-title><strong>{{ t('member.detail.placeOfBirth') }}:</strong> {{ member.placeOfBirth }}</v-list-item-title>
            </v-list-item>
            <v-list-item v-if="member.placeOfDeath">
              <v-list-item-title><strong>{{ t('member.detail.placeOfDeath') }}:</strong> {{ member.placeOfDeath }}</v-list-item-title>
            </v-list-item>
            <v-list-item v-if="member.occupation">
              <v-list-item-title><strong>{{ t('member.detail.occupation') }}:</strong> {{ member.occupation }}</v-list-item-title>
            </v-list-item>
          </v-list>
        </v-col>
        <v-col cols="12" md="6">
          <v-list dense>
            <v-list-item v-if="member.parents && member.parents.length">
              <v-list-item-title><strong>{{ t('member.detail.parents') }}:</strong> {{ getMemberNames(member.parents) }}</v-list-item-title>
            </v-list-item>
            <v-list-item v-if="member.spouses && member.spouses.length">
              <v-list-item-title><strong>{{ t('member.detail.spouses') }}:</strong> {{ getMemberNames(member.spouses) }}</v-list-item-title>
            </v-list-item>
            <v-list-item v-if="member.children && member.children.length">
              <v-list-item-title><strong>{{ t('member.detail.children') }}:</strong> {{ getMemberNames(member.children) }}</v-list-item-title>
            </v-list-item>
            <v-list-item v-if="member.biography">
              <v-list-item-title><strong>{{ t('member.detail.biography') }}:</strong></v-list-item-title>
              <v-list-item-subtitle>{{ member.biography }}</v-list-item-subtitle>
            </v-list-item>
          </v-list>
        </v-col>
      </v-row>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="primary" @click="$emit('edit', member)">{{ t('common.edit') }}</v-btn>
      <v-btn color="error" @click="$emit('delete', member)">{{ t('common.delete') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import type { PropType } from 'vue';
import type { Member } from '@/types/member';
import { useMembers } from '@/data/members';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

defineProps({
  member: {
    type: Object as PropType<Member>,
    required: true,
  },
});

defineEmits(['close', 'edit', 'delete']);

const { getMemberById } = useMembers();

const getMemberNames = (ids: string[]) => {
  return ids.map(id => getMemberById(id)?.fullName || t('common.unknown')).join(', ');
};
</script>
