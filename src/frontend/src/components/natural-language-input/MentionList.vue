<template>
  <div class="items" v-if="items.length">
    <button
      class="item"
      :class="{ 'is-selected': index === selectedIndex }"
      v-for="(item, index) in items"
      :key="index"
      @click="selectItem(index)"
    >
      {{ item.label }} ({{ item.id }})
    </button>
  </div>
  <div class="item" v-else>
    {{ t('naturalLanguage.editor.noResult') }}
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';

interface MentionListProps {
  items: { id: string; label: string }[];
  command: (item: { id: string; label: string }) => void;
  onSelectCallback: (handler: { onKeyDown: (props: { event: KeyboardEvent }) => boolean }) => void;
}

const props = defineProps<MentionListProps>();
// const emit = defineEmits(['select']); // No longer needed as we use a prop for callback

const { t } = useI18n();
const selectedIndex = ref(0);

const selectItem = (index: number) => {
  const item = props.items[index];
  if (item) {
    props.command(item);
  }
};

const onKeyDown = ({ event }: { event: KeyboardEvent }) => {
  if (event.key === 'ArrowUp') {
    upHandler();
    return true;
  }

  if (event.key === 'ArrowDown') {
    downHandler();
    return true;
  }

  if (event.key === 'Enter') {
    enterHandler();
    return true;
  }

  return false;
};

const upHandler = () => {
  selectedIndex.value = ((selectedIndex.value + props.items.length) - 1) % props.items.length;
};

const downHandler = () => {
  selectedIndex.value = (selectedIndex.value + 1) % props.items.length;
};

const enterHandler = () => {
  selectItem(selectedIndex.value);
};

watch(
  () => props.items,
  () => {
    selectedIndex.value = 0;
  },
);

onMounted(() => {
  props.onSelectCallback({
    onKeyDown,
  });
});
</script>

<style scoped>
.items {
  padding: 0.2rem;
  position: relative;
  border-radius: 0.5rem;
  background: #e3f2fd; /* Light blue background */
  color: rgba(0, 0, 0, 0.8);
  overflow: hidden;
  font-size: 0.9rem;
  box-shadow: 0 0 0 1px rgba(0, 0, 0, 0.05), 0px 10px 20px rgba(0, 0, 0, 0.1);
  border: 1px solid #bbdefb; /* Light blue border */
  z-index: 9999; /* Ensure it's on top */
}

.item {
  display: block;
  margin: 0;
  width: 100%;
  text-align: left;
  background: transparent;
  border-radius: 0.4rem;
  border: 1px solid transparent;
  padding: 0.2rem 0.4rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.item.is-selected {
  background-color: #bbdefb; /* Slightly darker blue for selected item */
  border-color: #90caf9; /* Blue border for selected item */
}
</style>
