import { ref, reactive, nextTick } from 'vue';
import { useI18n } from 'vue-i18n';
import { useServices } from '@/plugins/services.plugin';
import { useConfirmDialog } from '@/composables/ui/useConfirmDialog';
import { useGlobalSnackbar } from '@/composables/ui/useGlobalSnackbar';
import { useQueryClient } from '@tanstack/vue-query';
import type { GenerateFamilyDataDto, CardData, MemberDto, EventDto } from '@/types';
import { mapCombinedAiContentToCardData } from '@/composables/ai-assistant/aiAssistant.transform';

interface Message {
  from: 'user' | 'ai';
  text?: string;
  cardData?: CardData[];
}

interface UseAiAssistantOptions {
  familyId: string;
}

interface AiAssistantDependencies {
  i18n: ReturnType<typeof useI18n>;
  chatService: ReturnType<typeof useServices>['chat'];
  showConfirmDialog: ReturnType<typeof useConfirmDialog>['showConfirmDialog'];
  showSnackbar: ReturnType<typeof useGlobalSnackbar>['showSnackbar'];
  queryClient: ReturnType<typeof useQueryClient>;
}

const defaultDependencies = (): AiAssistantDependencies => ({
  i18n: useI18n() as any, // Cast to any to resolve strict type issues with useI18n return type
  chatService: useServices().chat,
  showConfirmDialog: useConfirmDialog().showConfirmDialog,
  showSnackbar: useGlobalSnackbar().showSnackbar,
  queryClient: useQueryClient(),
});

export function useAiAssistant(options: UseAiAssistantOptions, deps?: Partial<AiAssistantDependencies>) {
  const dependencies = { ...defaultDependencies(), ...deps };
  const { t } = dependencies.i18n;
  const { chatService, showConfirmDialog, showSnackbar, queryClient } = dependencies;

  const chatInput = ref('');
  const messages = ref<Message[]>([]);
  const isLoadingAiResponse = ref(false);
  const addMemberDrawer = ref(false);
  const memberDataToAdd = ref<MemberDto | null>(null);
  const savingCardId = ref<string | null>(null);
  const addEventDrawer = ref(false); // New ref for EventAddView drawer
  const eventDataToAdd = ref<EventDto | null>(null); // New ref to hold event data to add
  const selectedFile = ref<File | null>(null); // New ref for uploaded file
  const isLoadingOcr = ref(false); // New ref for OCR loading state

  const sendMessage = async () => {
    if (chatInput.value.trim()) {
      messages.value.push({ from: 'user', text: chatInput.value.trim() });
      const userMessage = chatInput.value.trim();
      chatInput.value = '';

      isLoadingAiResponse.value = true;

      try {
        const command: GenerateFamilyDataDto = {
          familyId: options.familyId,
          chatInput: userMessage,
        };

        const result = await chatService.generateFamilyData(command);

        if (result.ok && result.value) {
          const cardData = mapCombinedAiContentToCardData(result.value, options.familyId);
          messages.value.push({ from: 'ai', cardData: cardData });
        } else if (!result.ok) {
          messages.value.push({ from: 'ai', text: `Error: ${result.error?.message || 'Failed to generate content.'}` });
        }
      } finally {
        isLoadingAiResponse.value = false;
      }
    }
  };

  const handleFileChange = async (event: Event) => {
    const target = event.target as HTMLInputElement;
    if (target.files && target.files.length > 0) {
      selectedFile.value = target.files[0];
      await processOcr(); // Automatically process after file selection
    } else {
      selectedFile.value = null;
    }
  };

  const processOcr = async () => {
    if (!selectedFile.value) {
      showSnackbar(t('aiAssistant.selectFileForOcr'), 'warning');
      return;
    }

    isLoadingOcr.value = true;
    try {
      const ocrResult = await chatService.performOcr(selectedFile.value);
      if (ocrResult.ok && ocrResult.value) {
        chatInput.value = ''; // Clear input first
        await nextTick(); // Wait for the DOM to update with empty value
        chatInput.value = ocrResult.value.text; // Then set the new value
        showSnackbar(t('aiAssistant.ocrSuccess'), 'success');
      } else if (!ocrResult.ok) { // Properly narrow the type
        showSnackbar(ocrResult.error?.message || t('aiAssistant.ocrError'), 'error');
      }
    } catch (error: any) {
      showSnackbar(error.message || t('aiAssistant.ocrError'), 'error');
    } finally {
      isLoadingOcr.value = false;
      removeSelectedFile(); // Clear selected file after processing
    }
  };

      const removeSelectedFile = () => {
        selectedFile.value = null;
      };
    
      const clearFileInput = (fileInputElement: HTMLInputElement | null) => {
        if (fileInputElement) {
          fileInputElement.value = '';
        }
        selectedFile.value = null;
      };
    const handleSaveCard = (id: string) => {
    let foundCard: CardData | undefined;
    for (const message of messages.value) {
      if (message.cardData) {
        foundCard = message.cardData.find((card: CardData) => card.id === id); // Explicitly type card
        if (foundCard) {
          break;
        }
      }
    }

    if (foundCard) {
      if (foundCard.type === 'Member') {
        memberDataToAdd.value = foundCard.data as MemberDto;
        savingCardId.value = id;
        addMemberDrawer.value = true;
      } else if (foundCard.type === 'Event') { // Handle Event type
        eventDataToAdd.value = foundCard.data as EventDto;
        savingCardId.value = id;
        addEventDrawer.value = true;
      } else if (foundCard.type === 'Family') {
        showSnackbar(t('aiAssistant.messages.familySaveNotImplemented'), 'info');
      }
    }
  };

  const handleMemberClosed = () => {
    addMemberDrawer.value = false;
    memberDataToAdd.value = null;
    savingCardId.value = null;
  };

  const handleMemberSaved = () => {
    addMemberDrawer.value = false;
    memberDataToAdd.value = null;

    if (savingCardId.value) {
      messages.value.forEach((message: Message) => { // Explicitly type message
        if (message.cardData) {
          const card = message.cardData.find((c: CardData) => c.id === savingCardId.value); // Explicitly type c
          if (card) {
            card.isSaved = true;
          }
        }
      });
      savingCardId.value = null;
    }

    queryClient.invalidateQueries({ queryKey: ['members', 'list'] });
    showSnackbar(t('member.messages.addSuccess'), 'success');
  };

  const handleEventClosed = () => {
    addEventDrawer.value = false;
    eventDataToAdd.value = null;
    savingCardId.value = null;
  };

  const handleEventSaved = () => {
    addEventDrawer.value = false;
    eventDataToAdd.value = null;

    if (savingCardId.value) {
      messages.value.forEach((message: Message) => { // Explicitly type message
        if (message.cardData) {
          const card = message.cardData.find((c: CardData) => c.id === savingCardId.value); // Explicitly type c
          if (card) {
            card.isSaved = true;
          }
        }
      });
      savingCardId.value = null;
    }

    queryClient.invalidateQueries({ queryKey: ['events', 'list'] }); // Invalidate event list to refetch
    showSnackbar(t('event.messages.addSuccess'), 'success');
  };

  const handleDeleteCard = async (id: string) => {
    const result = await showConfirmDialog({
      title: t('common.confirm'),
      message: t('aiAssistant.confirmDeleteCard'),
      confirmText: t('common.confirm'),
      cancelText: t('common.cancel'),
      color: 'error',
    });

    if (result) {
      messages.value.forEach((message: Message) => { // Explicitly type message
        if (message.cardData) {
          message.cardData = message.cardData.filter((card: CardData) => card.id !== id); // Explicitly type card
        }
      });
    }
  };

  return {
    state: reactive({
      chatInput,
      messages,
      isLoadingAiResponse,
      addMemberDrawer,
      memberDataToAdd,
      savingCardId,
      addEventDrawer,
      eventDataToAdd,
      selectedFile,
      isLoadingOcr,
    }),
    actions: {
      sendMessage,
      handleSaveCard,
      handleMemberClosed,
      handleMemberSaved,
      handleEventClosed,
      handleEventSaved,
      handleDeleteCard,
      handleFileChange,
      processOcr,
      removeSelectedFile,
      clearFileInput,
    },
  };
}