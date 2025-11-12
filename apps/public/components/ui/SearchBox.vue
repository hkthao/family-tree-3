<script setup lang="ts">
import { ref, watch } from 'vue';
import { Combobox, ComboboxInput, ComboboxOption, ComboboxOptions } from '@headlessui/vue';
import { CheckIcon, ChevronUpDownIcon } from '@heroicons/vue/20/solid';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

interface Member {
  id: number;
  name: string;
  imageUrl: string;
}

const props = defineProps<{
  members: Member[];
  loading: boolean;
  error: string | null;
}>();

const emit = defineEmits(['search']);

const query = ref('');
const selectedMember = ref<Member | null>(null);

const filteredMembers = ref<Member[]>([]);

watch(query, (newQuery) => {
  if (newQuery === '') {
    filteredMembers.value = [];
  } else {
    filteredMembers.value = props.members.filter((member) =>
      member.name.toLowerCase().includes(newQuery.toLowerCase())
    );
  }
});

watch(() => props.members, (newMembers) => {
  if (query.value !== '') {
    filteredMembers.value = newMembers.filter((member) =>
      member.name.toLowerCase().includes(query.value.toLowerCase())
    );
  }
});

const handleSearch = () => {
  emit('search', query.value);
};
</script>

<template>
  <div class="w-full max-w-md mx-auto">
    <Combobox v-model="selectedMember" @update:modelValue="(member) => $router.push(`/member/${member?.id}`)">
      <div class="relative mt-1">
        <div
          class="relative w-full cursor-default overflow-hidden rounded-lg bg-white text-left shadow-md focus:outline-none focus-visible:ring-2 focus-visible:ring-white focus-visible:ring-opacity-75 focus-visible:ring-offset-2 focus-visible:ring-offset-teal-300 sm:text-sm"
        >
          <ComboboxInput
            class="w-full border-none py-2 pl-3 pr-10 text-sm leading-5 text-gray-900 focus:ring-0"
            :display-value="(member: Member) => member?.name"
            @change="query = $event.target.value"
            @keyup.enter="handleSearch"
            :placeholder="t('searchMembers')"
          />
          <button
            type="button"
            class="absolute inset-y-0 right-0 flex items-center pr-2"
            @click="handleSearch"
          >
            <ChevronUpDownIcon class="h-5 w-5 text-gray-400" aria-hidden="true" />
          </button>
        </div>
        <Transition
          leave-active-class="transition ease-in duration-100"
          leave-from-class="opacity-100"
          leave-to-class="opacity-0"
        >
          <ComboboxOptions
            class="absolute mt-1 max-h-60 w-full overflow-auto rounded-md bg-white py-1 text-base shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm"
          >
            <div
              v-if="loading"
              class="relative cursor-default select-none py-2 px-4 text-gray-700"
            >
              {{ t('search.loading') }}
            </div>
            <div
              v-else-if="error"
              class="relative cursor-default select-none py-2 px-4 text-red-700"
            >
              {{ error }}
            </div>
            <div
              v-else-if="filteredMembers.length === 0 && query !== ''"
              class="relative cursor-default select-none py-2 px-4 text-gray-700"
            >
              {{ t('nothingFound') }}
            </div>

            <ComboboxOption
              v-for="member in filteredMembers"
              :key="member.id"
              :value="member"
              as="template"
              v-slot="{ selected, active }"
            >
              <li
                class="relative cursor-default select-none py-2 pl-10 pr-4"
                :class="{
                  'bg-indigo-600 text-white': active,
                  'text-gray-900': !active,
                }"
              >
                <span
                  class="block truncate"
                  :class="{ 'font-medium': selected, 'font-normal': !selected }"
                >
                  {{ member.name }}
                </span>
                <span
                  v-if="selected"
                  class="absolute inset-y-0 left-0 flex items-center pl-3"
                  :class="{ 'text-white': active, 'text-indigo-600': !active }"
                >
                  <CheckIcon class="h-5 w-5" aria-hidden="true" />
                </span>
              </li>
            </ComboboxOption>
          </ComboboxOptions>
        </Transition>
      </div>
    </Combobox>
  </div>
</template>
