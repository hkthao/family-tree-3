import type { FamilyExportDto } from '@/types';

export async function parseJsonFile(file: File): Promise<FamilyExportDto> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();

    reader.onload = (e) => {
      try {
        const fileContent = e.target?.result as string;
        const familyData: FamilyExportDto = JSON.parse(fileContent);
        resolve(familyData);
      } catch (parseError: any) {
        reject(new Error(`Failed to parse JSON file: ${parseError.message}`));
      }
    };

    reader.onerror = (e) => {
      reject(new Error(`Error reading file: ${e.target?.error?.message || 'Unknown error'}`));
    };

    reader.readAsText(file);
  });
}