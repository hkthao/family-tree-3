import { useFamilyDataManagement } from './logic/useFamilyDataManagement';

export function useFamilyImportExport(familyId: string) {
  const { state, actions } = useFamilyDataManagement(familyId);

  return {
    state: {
      isImporting: state.isImportingFamilyData,
      importFile: state.importFile,
    },
    actions: {
      importData: actions.importData,
    },
  };
}
