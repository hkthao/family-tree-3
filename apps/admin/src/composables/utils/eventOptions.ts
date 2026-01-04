import { type ComposerTranslation } from 'vue-i18n';
import { EventType } from '@/types'; // Assuming EventType is defined here

export function getEventTypeOptions(t: ComposerTranslation) {
  return [
    { title: t('event.type.birth'), value: EventType.Birth },
    { title: t('event.type.marriage'), value: EventType.Marriage },
    { title: t('event.type.death'), value: EventType.Death },
    { title: t('event.type.other'), value: EventType.Other },
  ];
}

export function getEventTypeTitle(t: ComposerTranslation, type: EventType) {
  switch (type) {
    case EventType.Birth: return t('event.type.birth');
    case EventType.Marriage: return t('event.type.marriage');
    case EventType.Death: return t('event.type.death');
    case EventType.Other: return t('event.type.other');
    default: return t('common.unknown');
  }
}

export function getEventTypeIcon(type: EventType): string {
  switch (type) {
    case EventType.Birth:
      return 'mdi-cake-variant';
    case EventType.Marriage:
      return 'mdi-ring';
    case EventType.Death:
      return 'mdi-grave';
    case EventType.Other:
    default:
      return 'mdi-calendar-text';
  }
}
