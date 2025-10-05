import { Gender } from '@/types';
import i18n from '@/plugins/i18n';

export const GENDER_OPTIONS = [
  { title: i18n.global.t('member.gender.male'), value: Gender.Male },
  { title: i18n.global.t('member.gender.female'), value: Gender.Female },
  { title: i18n.global.t('member.gender.other'), value: Gender.Other },
];

export function getGenderTitle(gender: Gender | undefined): string {
  if (gender === undefined) {
    return i18n.global.t('common.unknown');
  }
  const option = GENDER_OPTIONS.find(opt => opt.value === gender);
  return option ? option.title : i18n.global.t('common.unknown');
}
