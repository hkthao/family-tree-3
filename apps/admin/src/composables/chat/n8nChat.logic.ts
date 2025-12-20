// src/composables/chat/n8nChat.logic.ts
import type { CreateChatOptions } from './n8nChat.adapter';

// Define a type for the translation function
type TranslateFunction = (key: string) => string;

/**
 * Generates the i18n configuration object for the n8n chat widget.
 * @param t The i18n translation function.
 * @returns The i18n configuration object for the chat widget.
 */
export function getN8nChatI18n(t: TranslateFunction): CreateChatOptions['i18n'] {
  return {
    en: {
      title: t('n8nChat.en.title') || '',
      subtitle: t('n8nChat.en.subtitle') || '',
      footer: t('n8nChat.en.footer') || '',
      inputPlaceholder: t('n8nChat.en.inputPlaceholder') || '',
      getStarted: t('n8nChat.en.getStarted') || '',
      closeButtonTooltip: t('n8nChat.en.closeButtonTooltip') || '',
    },
    vi: {
      title: t('n8nChat.vi.title') || '',
      subtitle: t('n8nChat.vi.subtitle') || '',
      footer: t('n8nChat.vi.footer') || '',
      inputPlaceholder: t('n8nChat.vi.inputPlaceholder') || '',
      getStarted: t('n8nChat.vi.getStarted') || '',
      closeButtonTooltip: t('n8nChat.vi.closeButtonTooltip') || '',
    },
  };
}
