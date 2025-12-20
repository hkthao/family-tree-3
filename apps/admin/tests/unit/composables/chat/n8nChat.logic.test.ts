// tests/unit/composables/chat/n8nChat.logic.test.ts
import { describe, it, expect, vi } from 'vitest';
import { getN8nChatI18n } from '@/composables/chat/n8nChat.logic';

describe('n8nChat.logic', () => {
  it('should generate correct i18n structure for en-US', () => {
    const t = vi.fn((key: string) => key); // Simple mock for i18n translation
    const i18nConfig = getN8nChatI18n(t);

    expect(t).toHaveBeenCalledWith('n8nChat.en.title');
    expect(t).toHaveBeenCalledWith('n8nChat.en.subtitle');
    // ... and so on for all keys

    expect(i18nConfig).toEqual({
      en: {
        title: 'n8nChat.en.title',
        subtitle: 'n8nChat.en.subtitle',
        footer: 'n8nChat.en.footer',
        inputPlaceholder: 'n8nChat.en.inputPlaceholder',
        getStarted: 'n8nChat.en.getStarted',
        closeButtonTooltip: 'n8nChat.en.closeButtonTooltip',
      },
      vi: {
        title: 'n8nChat.vi.title',
        subtitle: 'n8nChat.vi.subtitle',
        footer: 'n8nChat.vi.footer',
        inputPlaceholder: 'n8nChat.vi.inputPlaceholder',
        getStarted: 'n8nChat.vi.getStarted',
        closeButtonTooltip: 'n8nChat.vi.closeButtonTooltip',
      },
    });
  });
});
