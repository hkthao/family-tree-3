// apps/admin/src/composables/useFaceDataConsent.ts


const CONSENT_LOCAL_STORAGE_KEY = 'faceDataConsentGiven';

export function useFaceDataConsent() {
  /**
   * Checks if face data consent has been given.
   * @returns {boolean} True if consent is given, false otherwise.
   */
  const checkConsent = (): boolean => {
    return localStorage.getItem(CONSENT_LOCAL_STORAGE_KEY) === 'true';
  };

  /**
   * Sets the face data consent status to true.
   */
  const setConsent = () => {
    localStorage.setItem(CONSENT_LOCAL_STORAGE_KEY, 'true');
    // TODO: Optionally send consent log to backend here
  };

  /**
   * Sets the face data consent status to false.
   */
  const denyConsent = () => {
    localStorage.setItem(CONSENT_LOCAL_STORAGE_KEY, 'false');
    // TODO: Optionally send consent denial log to backend here
  };

  /**
   * Resets the face data consent status.
   */
  const resetConsent = () => {
    localStorage.removeItem(CONSENT_LOCAL_STORAGE_KEY);
    // TODO: Optionally send consent reset log to backend here
  };

  return {
    checkConsent,
    setConsent,
    denyConsent,
    resetConsent,
  };
}