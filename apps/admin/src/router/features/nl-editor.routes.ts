import type { RouteRecordRaw } from 'vue-router';
import { NLEditorView } from '@/views';

export const nlEditorRoutes: RouteRecordRaw[] = [
  {
    path: 'nl-editor',
    name: 'NLEditor',
    component: NLEditorView,
    meta: { breadcrumb: 'naturalLanguage.editor.title' },
  },
];
