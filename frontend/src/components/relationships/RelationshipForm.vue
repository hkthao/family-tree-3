<template>
  <v-form @submit.prevent="save">
    <v-card>
      <v-card-title>{{ formTitle }}</v-card-title>
      <v-card-text>
        <v-row>
          <v-col cols="12">
            <MemberAutocomplete
              v-model="editableRelationship.sourceMemberId"
              :label="t('relationship.form.sourceMember')"
            />
          </v-col>
          <v-col cols="12">
            <MemberAutocomplete
              v-model="editableRelationship.targetMemberId"
              :label="t('relationship.form.targetMember')"
            />
          </v-col>
          <v-col cols="12">
            <v-select
              v-model="editableRelationship.type"
              :items="relationshipTypes"
              :label="t('relationship.form.type')"
            ></v-select>
          </v-col>
        </v-row>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="primary" type="submit">{{ t('common.save') }}</v-btn>
        <v-btn @click="cancel">{{ t('common.cancel') }}</v-btn>
      </v-card-actions>
    </v-card>
  </v-form>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRelationshipStore } from '@/stores/relationship.store';
import type { Relationship } from '@/types';
import { RelationshipType } from '@/types';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';

const props = defineProps<{ id?: string }>();
const emit = defineEmits(['save', 'cancel']);

const { t } = useI18n();
const relationshipStore = useRelationshipStore();

const editableRelationship = ref<Partial<Relationship>>({});

const formTitle = computed(() =>
  props.id ? t('relationship.form.editTitle') : t('relationship.form.addTitle'),
);

const relationshipTypes = computed(() => [
  { title: t('relationship.type.parent'), value: RelationshipType.Parent },
  { title: t('relationship.type.child'), value: RelationshipType.Child },
  { title: t('relationship.type.spouse'), value: RelationshipType.Spouse },
  { title: t('relationship.type.sibling'), value: RelationshipType.Sibling },
]);

onMounted(async () => {
  if (props.id) {
    await relationshipStore.getById(props.id);
    editableRelationship.value = { ...relationshipStore.currentItem };
  }
});

const save = () => {
  emit('save', editableRelationship.value);
};

const cancel = () => {
  emit('cancel');
};
</script>
