// apps/admin/src/constants/privacy-props.constants.ts

/**
 * Các hằng số định nghĩa các thuộc tính có thể cấu hình quyền riêng tư cho các thực thể khác nhau.
 * Được sử dụng để xác định các trường dữ liệu mà người dùng có thể chọn để hiển thị công khai hoặc giữ riêng tư.
 */

/**
 * Thuộc tính của thành viên (Member).
 */
export const MEMBER_PROPS = {
  PROPERTIES: [
    'lastName',
    'firstName',
    'nickname',
    'gender',
    'dateOfBirth',
    'dateOfDeath',
    'placeOfBirth',
    'placeOfDeath',
    'occupation',
    'biography',
    'education',
    'religion',
    'phoneNumber',
    'email',
    'address',
    'identification',
    'socialMedia',
    'notes',
    'fatherId',
    'motherId',
    'husbandId',
    'wifeId',
  ],
};

/**
 * Thuộc tính của gia đình (Family).
 */
export const FAMILY_PROPS = {
  PROPERTIES: [
    'name',
    'description',
    'headOfFamilyId',
    'foundingDate',
    'foundingPlace',
  ],
};

/**
 * Thuộc tính của sự kiện (Event).
 */
export const EVENT_PROPS = {
  PROPERTIES: [
    'title',
    'description',
    'date',
    'location',
    'type',
  ],
};

/**
 * Thuộc tính của vị trí gia đình (FamilyLocation).
 */
export const FAMILY_LOCATION_PROPS = {
  PROPERTIES: [
    'name',
    'address',
    'latitude',
    'longitude',
    'description',
  ],
};

/**
 * Thuộc tính của mục kỷ niệm (MemoryItem).
 */
export const MEMORY_ITEM_PROPS = {
  PROPERTIES: [
    'title',
    'description',
    'date',
    'location',
    'type',
    'mediaUrl', // Ví dụ: URL hình ảnh, video
  ],
};

/**
 * Thuộc tính liên quan đến khuôn mặt (Face).
 */
export const FACE_PROPS = {
  PROPERTIES: [
    'faceUrl', // URL của ảnh khuôn mặt
    'x',
    'y',
    'width',
    'height',
    'personId', // ID người được nhận diện
  ],
};

/**
 * Thuộc tính liên quan đến bản đồ (Map).
 */
export const MAP_PROPS = {
  PROPERTIES: [
    'centerLatitude',
    'centerLongitude',
    'zoomLevel',
    'mapType',
  ],
};
