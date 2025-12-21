// tests/unit/composables/family-dict/useFamilyDictFormLogic.test.ts
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { useFamilyDictFormLogic } from '@/composables/family-dict/useFamilyDictFormLogic';
import { ref } from 'vue';

import { useAddFamilyDictMutation } from '@/composables';


// Mock dependencies
const mockAddFamilyDictMutation = vi.fn();
const mockShowSnackbar = vi.fn();
const mockT = vi.fn((key) => key);
const mockEmit = vi.fn();
const mockValidate = vi.fn();
const mockGetFormData = vi.fn();

vi.mock('@/composables', async (importOriginal) => {
  const original = await importOriginal();
  return {
    ...original,
    useAddFamilyDictMutation: vi.fn(),
    useGlobalSnackbar: vi.fn(() => ({
      showSnackbar: mockShowSnackbar,
    })),
  };
});

vi.mock('vue-i18n', async (importOriginal) => {
  const actual = await importOriginal() as typeof import('vue-i18n');
  return {
    ...actual,
    useI18n: vi.fn(() => ({
      t: mockT,
    })),
  };
});


describe('useFamilyDictFormLogic', () => {

  beforeEach(() => {
    vi.clearAllMocks();
    // The mock for useAddFamilyDictMutation will create its own internal isPending ref.

    // This mock will be the default, and individual test cases will override the 'mutate' behavior.
    (useAddFamilyDictMutation as vi.Mock).mockImplementation(() => {
      return {
        mutate: vi.fn(), // Will be overridden by individual tests if needed
        isPending: ref(false), // Default to a simple ref
      };
    });
  });

  afterEach(() => {
    // No need to restore real timers as fake timers are no longer used
  });

  it('should call closeForm when closeForm action is triggered', () => {
    const { actions } = useFamilyDictFormLogic({
      emit: mockEmit,
    });

    actions.closeForm();
    expect(mockEmit).toHaveBeenCalledWith('close');
  });

  it('should not add family dict if form validation fails', async () => {
    mockValidate.mockResolvedValue(false);

    const { actions } = useFamilyDictFormLogic({
      emit: mockEmit,
      formActions: { validate: mockValidate, getFormData: mockGetFormData },
    });

    await actions.handleAddFamilyDict();

    expect(mockValidate).toHaveBeenCalledOnce();
    expect(mockGetFormData).not.toHaveBeenCalled();
    expect(mockAddFamilyDictMutation).not.toHaveBeenCalled();
    expect(mockShowSnackbar).not.toHaveBeenCalled();
    expect(mockEmit).not.toHaveBeenCalledWith('saved');
  });

  it('should log an error if formActions are not provided', async () => {
    const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

    const { actions } = useFamilyDictFormLogic({
      emit: mockEmit,
    });

    await actions.handleAddFamilyDict();

    expect(consoleErrorSpy).toHaveBeenCalledWith('FamilyDictFormActions are not provided to useFamilyDictFormLogic.');
    expect(mockValidate).not.toHaveBeenCalled();
    expect(mockGetFormData).not.toHaveBeenCalled();
    expect(mockAddFamilyDictMutation).not.toHaveBeenCalled();
    consoleErrorSpy.mockRestore();
  });
});
