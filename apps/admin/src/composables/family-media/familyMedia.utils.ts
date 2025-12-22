import { MediaType } from '@/types/enums'; // Import MediaType

export const getMediaTypeEnumValue = (mediaTypeString: string | MediaType | undefined): MediaType | undefined => {
  if (mediaTypeString === undefined || typeof mediaTypeString === 'number') {
    return mediaTypeString as MediaType | undefined;
  }
  const key = mediaTypeString as keyof typeof MediaType;
  if (MediaType[key] !== undefined) {
    return MediaType[key];
  }
  return undefined;
};
