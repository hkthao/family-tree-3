// src/composables/chat/n8nChat.adapter.ts
import { createChat } from '@n8n/chat';

// Define ChatMetadata locally as it's not exported by @n8n/chat
export interface ChatMetadata {
  familyId?: string;
  [key: string]: any; // Allow other properties
}

// Define CreateChatOptions locally based on usage, as it's not exported by @n8n/chat
export interface CreateChatOptions {
  webhookUrl: string;
  webhookConfig?: {
    method?: string;
    headers?: { [key: string]: string };
  };
  target?: string | Element;
  mode?: 'fullscreen' | 'modal';
  chatInputKey?: string;
  chatSessionKey?: string;
  loadPreviousSession?: boolean;
  metadata?: ChatMetadata;
  showWelcomeScreen?: boolean;
  defaultLanguage?: string;
  initialMessages?: string[];
  i18n?: {
    [key: string]: {
      title: string;
      subtitle: string;
      footer: string;
      inputPlaceholder: string;
      getStarted: string;
      closeButtonTooltip: string;
    };
  };
  enableStreaming?: boolean;
}

export interface N8nChatAdapter {
  mount(options: {
    webhookUrl: string;
    accessToken: string;
    targetSelector: string;
    metadata: ChatMetadata;
    defaultLanguage: string;
    i18n?: {
      [key: string]: {
        title: string;
        subtitle: string;
        footer: string;
        inputPlaceholder: string;
        getStarted: string;
        closeButtonTooltip: string;
      };
    };
  }): void;
  unmount(): void;
  isMounted(): boolean;
}

export class ApiN8nChatAdapter implements N8nChatAdapter {
  private chatInstance: ReturnType<typeof createChat> | null = null;
  private targetElement: Element | null = null;

  mount(options: {
    webhookUrl: string;
    accessToken: string;
    targetSelector: string;
    metadata: ChatMetadata;
    defaultLanguage: string;
    i18n: CreateChatOptions['i18n'];
  }): void {
    if (this.chatInstance) {
      console.warn('Chat instance already mounted. Unmount first before mounting again.');
      return;
    }

    const { webhookUrl, accessToken, targetSelector, metadata, defaultLanguage, i18n } = options;

    this.targetElement = document.querySelector(targetSelector);
    if (!this.targetElement) {
      console.warn(`${targetSelector} not found in DOM for n8n chat, cannot mount.`);
      return;
    }

    this.chatInstance = createChat({
      webhookUrl: webhookUrl,
      webhookConfig: {
        method: 'POST',
        headers: {
          'authorization': `Bearer ${accessToken}`,
        },
      },
      target: targetSelector,
      mode: 'fullscreen',
      chatInputKey: 'chatInput',
      chatSessionKey: 'sessionId',
      loadPreviousSession: false,
      metadata: metadata,
      showWelcomeScreen: false,
      defaultLanguage: defaultLanguage as any,
      initialMessages: [
        // Adapter does not handle i18n for initial messages, that's composable's responsibility
      ],
      i18n: i18n || undefined,
      enableStreaming: false,
    });
  }

  unmount(): void {
    if (this.chatInstance) {
      this.chatInstance.unmount();
      this.chatInstance = null;
      this.targetElement = null;
    }
  }

  isMounted(): boolean {
    return !!this.chatInstance;
  }
}
