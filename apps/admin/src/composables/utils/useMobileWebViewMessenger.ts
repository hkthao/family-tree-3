import { ref } from 'vue';

interface MapSelectionData {
  coordinates: { latitude: number; longitude: number };
  location: string;
}

const isReactNativeWebView = () => {
  return typeof window !== 'undefined' && !!(window as any).ReactNativeWebView;
};

export function useMobileWebViewMessenger() {
  const postMapSelectionMessage = (data: MapSelectionData) => {
    if (isReactNativeWebView()) {
      (window as any).ReactNativeWebView.postMessage(JSON.stringify(data));
    }
  };

  return {
    isReactNativeWebView,
    postMapSelectionMessage,
  };
}