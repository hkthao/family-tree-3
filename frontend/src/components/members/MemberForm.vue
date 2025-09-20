<template>
  <v-card>
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ isEditMode ? t('member.form.editTitle') : t('member.form.addTitle') }}</span>
    </v-card-title>
    <v-card-text>
      <v-form ref="form" @submit.prevent="saveMember">
        <!-- Thông tin cơ bản -->
        <v-row>
          <v-col cols="12">
            <div class="d-flex justify-center mb-4">
              <v-avatar size="96">
                <v-img v-if="memberForm.avatarUrl" :src="memberForm.avatarUrl"></v-img>
                <v-icon v-else size="96">mdi-account-circle</v-icon>
              </v-avatar>
            </div>
            <v-text-field
              v-model="memberForm.avatarUrl"
              :label="t('member.form.avatarUrl')"
            ></v-text-field>
          </v-col>
        </v-row>
        <v-row>
          <v-col cols="12">
            <v-text-field
              v-model="memberForm.fullName"
              :label="t('member.form.fullName')"
              :rules="[rules.required]"
            ></v-text-field>
          </v-col>
        </v-row>
        <v-row>
          <v-col cols="12" md="6">
            <DateInputField
              v-model="memberForm.dateOfBirth"
              :label="t('member.form.dateOfBirth')"
              :rules="[rules.required]"
            />
          </v-col>
          <v-col cols="12" md="6">
            <DateInputField
              v-model="memberForm.dateOfDeath"
              :label="t('member.form.dateOfDeath')"
              optional
            />
          </v-col>
        </v-row>

        <!-- Thông tin cá nhân -->
        <v-row>
          <v-col cols="12" md="4">
            <v-select
              v-model="memberForm.gender"
              :label="t('member.form.gender')"
              :items="genderOptions"
              :rules="[rules.required]"
            ></v-select>
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field
              v-model="memberForm.placeOfBirth"
              :label="t('member.form.placeOfBirth')"
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field
              v-model="memberForm.placeOfDeath"
              :label="t('member.form.placeOfDeath')"
            ></v-text-field>
          </v-col>
        </v-row>
        <v-row>
          <v-col cols="12">
            <v-text-field
              v-model="memberForm.occupation"
              :label="t('member.form.occupation')"
            ></v-text-field>
          </v-col>
        </v-row>

        <!-- Thông tin gia đình -->
        <v-row>
          <v-col cols="12" md="4">
            <v-autocomplete
              v-model="memberForm.parents"
              :items="availableMembers"
              item-title="fullName"
              item-value="id"
              :label="t('member.form.parents')"
              multiple
              chips
              clearable
            ></v-autocomplete>
          </v-col>
          <v-col cols="12" md="4">
            <v-autocomplete
              v-model="memberForm.spouses"
              :items="availableMembers"
              item-title="fullName"
              item-value="id"
              :label="t('member.form.spouses')"
              multiple
              chips
              clearable
            ></v-autocomplete>
          </v-col>
          <v-col cols="12" md="4">
            <v-autocomplete
              v-model="memberForm.children"
              :items="availableMembers"
              item-title="fullName"
              item-value="id"
              :label="t('member.form.children')"
              multiple
              chips
              clearable
            ></v-autocomplete>
          </v-col>
        </v-row>

        <!-- Thông tin khác -->
        <v-row>
          <v-col cols="12">
            <v-textarea
              v-model="memberForm.biography"
              :label="t('member.form.biography')"
            ></v-textarea>
          </v-col>
        </v-row>
      </v-form>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="blue-darken-1" variant="text" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="blue-darken-1" variant="text" @click="saveMember">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import type { Member } from '@/types/member';
import { useMembers } from '@/data/members';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import DateInputField from '@/components/common/DateInputField.vue'; // Add import
import { useNotificationStore } from '@/stores/notification';

const notificationStore = useNotificationStore();
const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const { members: allMembers, getMemberById, addMember, updateMember } = useMembers(); // Destructure members as allMembers

const form = ref<HTMLFormElement | null>(null);
const dateOfBirthMenu = ref(false);
const memberForm = ref<Omit<Member, 'id'> | Member>({ // Allow Member type for edit mode
  fullName: '',
  dateOfBirth: null, // Initialize as null
  gender: 'Male',
  parents: [],
  spouses: [],
  children: [],
});

const isEditMode = computed(() => !!route.params.id);

const genderOptions = [
  { title: t('member.gender.male'), value: 'Male' },
  { title: t('member.gender.female'), value: 'Female' },
  { title: t('member.gender.other'), value: 'Other' },
];

const rules = {
  required: (value: string) => !!value || t('validation.required'),
};

const availableMembers = computed(() => {
  // Filter out the current member from the list of available members for relationships
  return allMembers.value.filter(m => m.id !== (isEditMode.value ? route.params.id : null));
});

// Load member data if in edit mode
onMounted(() => {
  if (isEditMode.value) {
    const memberId = route.params.id as string;
    const member = getMemberById(memberId);
    if (member) {
      memberForm.value = {
        ...member,
        dateOfBirth: member.dateOfBirth ? new Date(member.dateOfBirth) : null,
        dateOfDeath: member.dateOfDeath ? new Date(member.dateOfDeath) : null,
        parents: [...member.parents],
        spouses: [...member.spouses],
        children: [...member.children]
      };
    } else {
      // Handle case where member is not found (e.g., redirect to 404 or members list)
      router.push('/members');
    }
  }
});


const saveMember = async () => {
  if (form.value) {
    const { valid } = await form.value.validate();
    if (valid) {
      try {
        const memberToSave = { ...memberForm.value };
        if (isEditMode.value) {
          await updateMember(memberToSave as Member);
          notificationStore.showSnackbar(t('member.messages.updateSuccess'), 'success');
        } else {
          await addMember(memberToSave as Omit<Member, 'id'>);
          notificationStore.showSnackbar(t('member.messages.addSuccess'), 'success');
        }
        closeForm();
      } catch (error) {
        notificationStore.showSnackbar(t('member.messages.saveError'), 'error');
      }
    }
  }
};

const closeForm = () => {
  router.push('/members'); // Navigate back to members list
};
</script>
