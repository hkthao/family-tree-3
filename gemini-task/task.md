Người dùng muốn tôi thiết kế tính năng "Quản lý khuôn mặt" cho một ứng dụng
  web gia phả. Tôi cần cung cấp một thiết kế chi tiết bao gồm bố cục UI, hệ
  thống phân cấp thành phần, tương tác người dùng, thiết kế Pinia store, luồng
   dữ liệu, các yếu tố hình ảnh, khả năng truy cập và cấu trúc thư mục.

  Tôi sẽ chia nhỏ điều này thành các phần được yêu cầu:

   1. Cấu trúc thư mục: Xác định đường dẫn cho các view, thành phần và store.
   2. Thiết kế Pinia Store (`faceStore`): Chi tiết trạng thái, getters và
      actions.
   3. Thiết kế các View chính:
       * FaceUploadView
       * FaceLabelingView
       * FaceSearchView
   4. Thiết kế các Thành phần:
       * FaceBoundingBoxViewer
       * FaceLabelCard
       * FaceResultList
   5. Tương tác người dùng & Luồng dữ liệu: Mô tả luồng giữa các view, thành
      phần và store.
   6. Kiểu UI & Khả năng truy cập: Tóm tắt các cân nhắc về hình ảnh và khả
      năng truy cập.

  Tôi sẽ bắt đầu bằng cách phác thảo cấu trúc thư mục và sau đó chuyển sang
  thiết kế Pinia store.

  ---

  Thiết kế tính năng Quản lý Khuôn mặt

  Đây là thiết kế chi tiết cho tính năng Quản lý Khuôn mặt, bao gồm cấu
  trúc thư mục, thiết kế Pinia store, các view chính, các component, tương
  tác người dùng, luồng dữ liệu, và các yếu tố về giao diện người dùng và
  khả năng tiếp cận.

  1. Cấu trúc Thư mục

    1 frontend/
    2 ├── src/
    3 │   ├── views/
    4 │   │   └── face/
    5 │   │       ├── FaceUploadView.vue         // View để tải 
      ảnh lên và phát hiện khuôn mặt
    6 │   │       ├── FaceLabelingView.vue       // View để gán 
      nhãn khuôn mặt
    7 │   │       └── FaceSearchView.vue         // View để tìm 
      kiếm khuôn mặt
    8 │   │
    9 │   ├── components/
   10 │   │   └── face/
   11 │   │       ├── FaceBoundingBoxViewer.vue  // Hiển thị ảnh
      với các hộp giới hạn khuôn mặt
   12 │   │       ├── FaceLabelCard.vue          // Card để gán 
      nhãn cho một khuôn mặt
   13 │   │       └── FaceResultList.vue         // Danh sách 
      kết quả tìm kiếm khuôn mặt
   14 │   │       └── FaceUploadInput.vue        // Component
      input tải ảnh lên (có thể tái sử dụng)
   15 │   │       └── FaceDetectionSidebar.vue   // Sidebar để
      hiển thị và quản lý các khuôn mặt được phát hiện
   16 │   │
   17 │   └── stores/
   18 │       └── face.store.ts                  // Pinia store 
      cho quản lý khuôn mặt
   19 │
   20 └── types/
   21     └── face.d.ts                        // Định nghĩa
      kiểu dữ liệu liên quan đến khuôn mặt

  Tóm tắt tệp:
   * FaceUploadView.vue: Cho phép người dùng tải lên ảnh nhóm, hiển thị các
     khuôn mặt được phát hiện và chuyển hướng đến FaceLabelingView.
   * FaceLabelingView.vue: Hiển thị các khuôn mặt được phát hiện từ ảnh đã tải
     lên, cho phép người dùng gán nhãn cho từng khuôn mặt với thành viên hiện
     có hoặc tạo thành viên mới.
   * FaceSearchView.vue: Cho phép người dùng tải lên một ảnh khuôn mặt duy
     nhất để tìm kiếm các thành viên phù hợp trong cây gia phả.
   * FaceBoundingBoxViewer.vue: Component hiển thị hình ảnh và vẽ các hộp giới
     hạn xung quanh các khuôn mặt được phát hiện.
   * FaceLabelCard.vue: Component hiển thị một khuôn mặt đã cắt và cung cấp
     các trường nhập liệu để gán nhãn cho khuôn mặt đó (chọn thành viên, tạo
     thành viên mới).
   * FaceResultList.vue: Component hiển thị danh sách các thành viên phù hợp
     từ kết quả tìm kiếm khuôn mặt, bao gồm độ tin cậy phù hợp.
   * FaceUploadInput.vue: Component input chung để tải ảnh lên, có thể tái sử
     dụng.
   * FaceDetectionSidebar.vue: Sidebar hiển thị danh sách các khuôn mặt được
     phát hiện trong FaceUploadView và FaceLabelingView, cho phép người dùng
     chọn để gán nhãn.
   * face.store.ts: Pinia store quản lý trạng thái liên quan đến khuôn mặt
     (ảnh tải lên, khuôn mặt được phát hiện, khuôn mặt được chọn, kết quả tìm
     kiếm, trạng thái API).
   * face.d.ts: Định nghĩa các kiểu dữ liệu TypeScript cho khuôn mặt, hộp giới
     hạn, kết quả phát hiện, v.v.

  2. Thiết kế Pinia Store (faceStore)

  `frontend/src/stores/face.store.ts`

     1 import { defineStore } from 'pinia';
     2 import type { DetectedFace, FaceMapping, SearchResult,
       Member } from '@/types/face.d.ts';
     3 import { ApiError, Result } from '@/types'; // Assuming 
       Result and ApiError are defined globally or in common 
       types
     4 import i18n from '@/plugins/i18n'; // For localization
     5 
     6 interface FaceState {
     7   uploadedImage: string | null; // Base64 or URL of the 
       uploaded image
     8   detectedFaces: DetectedFace[]; // Array of detected 
       faces with bounding boxes
     9   selectedFaceId: string | null; // ID of the currently 
       selected face for labeling
    10   faceSearchResults: SearchResult[]; // Results from face
       search
    11   loading: boolean;
    12   error: string | null;
    13 }
    14 
    15 export const useFaceStore = defineStore('face', {
    16   state: (): FaceState => ({
    17     uploadedImage: null,
    18     detectedFaces: [],
    19     selectedFaceId: null,
    20     faceSearchResults: [],
    21     loading: false,
    22     error: null,
    23   }),
    24 
    25   getters: {
    26     // Getters to retrieve specific data from the state
    27     currentSelectedFace: (state) => state.detectedFaces.
       find(face => face.id === state.selectedFaceId),
    28     unlabeledFaces: (state) => state.detectedFaces.filter
       (face => !face.memberId),
    29     labeledFaces: (state) => state.detectedFaces.filter(
       face => face.memberId),
    30   },
    31 
    32   actions: {
    33     // Action to handle image upload and face detection
    34     async detectFaces(imageFile: File): Promise<void> {
    35       this.loading = true;
    36       this.error = null;
    37       try {
    38         // Simulate API call
    39         // const result: Result<DetectedFace[], ApiError>
       = await this.services.face.detect(imageFile);
    40         // For now, simulate with mock data
    41         const mockDetectedFaces: DetectedFace[] = [
    42           { id: 'face1', boundingBox: { x: 10, y: 20,
       width: 50, height: 60 }, imageUrl:
       'path/to/cropped/face1.jpg', memberId: null, status:
       'unrecognized' },
    43           { id: 'face2', boundingBox: { x: 70, y: 80,
       width: 45, height: 55 }, imageUrl:
       'path/to/cropped/face2.jpg', memberId: 'member123',
       status: 'recognized' },
    44           // ... more mock faces
    45         ];
    46         this.uploadedImage = URL.createObjectURL
       (imageFile); // Store URL for display
    47         this.detectedFaces = mockDetectedFaces; // 
       Replace with actual API result
    48 
    49         // if (result.ok) {
    50         //   this.uploadedImage = 
       URL.createObjectURL(imageFile);
    51         //   this.detectedFaces = result.value.map(face 
       => ({ ...face, status: face.memberId ? 'recognized' : 
       'unrecognized' }));
    52         // } else {
    53         //   this.error = result.error?.message || 
       i18n.global.t('face.errors.detectionFailed');
    54         // }
    55       } catch (err: any) {
    56         this.error = err.message || i18n.global.t(
       'face.errors.unexpectedError');
    57       } finally {
    58         this.loading = false;
    59       }
    60     },
    61 
    62     // Action to select a face for labeling
    63     selectFace(faceId: string | null): void {
    64       this.selectedFaceId = faceId;
    65     },
    66 
    67     // Action to save face mapping to a member
    68     async saveFaceMapping(faceId: string, memberId:
       string): Promise<void> {
    69       this.loading = true;
    70       this.error = null;
    71       try {
    72         // Simulate API call
    73         // const result: Result<void, ApiError> = await 
       this.services.face.saveMapping(faceId, memberId);
    74         // For now, update local state
    75         const faceIndex = this.detectedFaces.findIndex(f
       => f.id === faceId);
    76         if (faceIndex !== -1) {
    77           this.detectedFaces[faceIndex].memberId =
       memberId;
    78           this.detectedFaces[faceIndex].status =
       'newly-labeled'; // Or 'recognized'
    79         }
    80 
    81         // if (result.ok) {
    82         //   // Update the detected face in the state
    83         //   const faceIndex = 
       this.detectedFaces.findIndex(f => f.id === faceId);
    84         //   if (faceIndex !== -1) {
    85         //     this.detectedFaces[faceIndex].memberId = 
       memberId;
    86         //     this.detectedFaces[faceIndex].status = 
       'newly-labeled'; // Assuming API confirms
    87         //   }
    88         //   // Show success notification
    89         //   // 
       this.notificationStore.showSnackbar(i18n.global.t('fa
       ce.success.saveMapping'), 'success');
    90         // } else {
    91         //   this.error = result.error?.message || 
       i18n.global.t('face.errors.saveMappingFailed');
    92         // }
    93       } catch (err: any) {
    94         this.error = err.message || i18n.global.t(
       'face.errors.unexpectedError');
    95       } finally {
    96         this.loading = false;
    97       }
    98     },
    99 
   100     // Action to search for members by a single face 
       image
   101     async searchByFace(imageFile: File): Promise<void> {
   102       this.loading = true;
   103       this.error = null;
   104       this.faceSearchResults = [];
   105       try {
   106         // Simulate API call
   107         // const result: Result<SearchResult[], ApiError>
       = await this.services.face.search(imageFile);
   108         // For now, simulate with mock data
   109         const mockSearchResults: SearchResult[] = [
   110           { member: { id: 'member123', fullName: 'John 
       Doe', avatarUrl: 'path/to/john.jpg' }, confidence: 0.95
       },
   111           { member: { id: 'member456', fullName: 'Jane 
       Smith', avatarUrl: 'path/to/jane.jpg' }, confidence: 0.80
       },
   112         ];
   113         this.faceSearchResults = mockSearchResults; // 
       Replace with actual API result
   114 
   115         // if (result.ok) {
   116         //   this.faceSearchResults = result.value;
   117         // } else {
   118         //   this.error = result.error?.message || 
       i18n.global.t('face.errors.searchFailed');
   119         // }
   120       } catch (err: any) {
   121         this.error = err.message || i18n.global.t(
       'face.errors.unexpectedError');
   122       } finally {
   123         this.loading = false;
   124       }
   125     },
   126 
   127     // Reset the store state
   128     resetState(): void {
   129       this.uploadedImage = null;
   130       this.detectedFaces = [];
   131       this.selectedFaceId = null;
   132       this.faceSearchResults = [];
   133       this.loading = false;
   134       this.error = null;
   135     },
   136   },
   137 });

  `frontend/src/types/face.d.ts`

    1 // Assuming Member type is already defined elsewhere, 
      e.g., in '@/types/member.d.ts'
    2 // import { Member } from '@/types/member.d.ts';
    3 
    4 interface BoundingBox {
    5   x: number;
    6   y: number;
    7   width: number;
    8   height: number;
    9 }
   10 
   11 type FaceStatus = 'recognized' | 'unrecognized' |
      'newly-labeled';
   12 
   13 interface DetectedFace {
   14   id: string; // Unique ID for the detected face instance
   15   boundingBox: BoundingBox;
   16   imageUrl: string; // URL to the cropped face image
   17   memberId: string | null; // ID of the associated member,
      if recognized/labeled
   18   status: FaceStatus; // For UI styling: 'recognized', 
      'unrecognized', 'newly-labeled'
   19 }
   20 
   21 interface FaceMapping {
   22   faceId: string;
   23   memberId: string;
   24 }
   25 
   26 interface SearchResult {
   27   member: Member; // Full member object or a simplified 
      version
   28   confidence: number; // Matching confidence (0.0 - 1.0)
   29 }
   30 
   31 // Placeholder for Member type if not already defined
   32 interface Member {
   33   id: string;
   34   fullName: string;
   35   avatarUrl?: string;
   36   // ... other relevant member properties
   37 }

  3. Thiết kế các View chính

  ##### 3.1. FaceUploadView.vue

   * Mục đích: Cho phép người dùng tải lên ảnh nhóm và hiển thị các khuôn mặt
     được phát hiện.
   * Bố cục UI:
       * Tiêu đề: "Tải ảnh lên để phát hiện khuôn mặt"
       * Component FaceUploadInput để chọn/kéo thả ảnh.
       * Khi ảnh được tải lên và khuôn mặt được phát hiện:
           * Hiển thị ảnh đã tải lên trong FaceBoundingBoxViewer.
           * Hiển thị FaceDetectionSidebar ở bên cạnh hoặc dưới ảnh, liệt kê
             các khuôn mặt được phát hiện.
           * Nút "Tiếp tục" để chuyển sang FaceLabelingView.
   * Tương tác:
       * Người dùng tải ảnh lên (kéo thả hoặc chọn tệp).
       * FaceUploadInput kích hoạt hành động detectFaces trong faceStore.
       * FaceBoundingBoxViewer hiển thị ảnh và các hộp giới hạn.
       * Người dùng có thể nhấp vào một hộp giới hạn hoặc một mục trong
         FaceDetectionSidebar để chọn một khuôn mặt.
       * Nút "Tiếp tục" chuyển hướng đến FaceLabelingView.
   * Luồng dữ liệu:
       * FaceUploadInput -> faceStore.detectFaces(file)
       * faceStore.uploadedImage, faceStore.detectedFaces ->
         FaceBoundingBoxViewer, FaceDetectionSidebar
       * faceStore.loading, faceStore.error -> Hiển thị trạng thái tải/lỗi.

  ##### 3.2. FaceLabelingView.vue

   * Mục đích: Gán nhãn cho các khuôn mặt được phát hiện với các thành viên
     hiện có hoặc tạo thành viên mới.
   * Bố cục UI:
       * Tiêu đề: "Gán nhãn khuôn mặt"
       * Hiển thị ảnh đã tải lên trong FaceBoundingBoxViewer (chỉ đọc các hộp
         giới hạn).
       * FaceDetectionSidebar hiển thị danh sách các khuôn mặt được phát hiện
         (đã gán nhãn, chưa gán nhãn, mới gán nhãn) với màu sắc tương ứng.
       * Khi một khuôn mặt được chọn từ sidebar hoặc bằng cách nhấp vào hộp
         giới hạn:
           * Hiển thị FaceLabelCard cho khuôn mặt được chọn.
           * FaceLabelCard cho phép tìm kiếm thành viên hiện có hoặc nhập
             thông tin để tạo thành viên mới.
       * Nút "Hoàn tất" để quay lại FaceUploadView hoặc trang tổng quan.
   * Tương tác:
       * Người dùng chọn một khuôn mặt từ FaceDetectionSidebar hoặc
         FaceBoundingBoxViewer.
       * FaceLabelCard hiển thị thông tin chi tiết và các tùy chọn gán nhãn.
       * Người dùng tìm kiếm thành viên, chọn thành viên, hoặc nhập thông tin
         thành viên mới.
       * Nút "Lưu" trong FaceLabelCard kích hoạt faceStore.saveFaceMapping.
       * Sau khi lưu, trạng thái của khuôn mặt trong FaceDetectionSidebar được
         cập nhật.
   * Luồng dữ liệu:
       * faceStore.uploadedImage, faceStore.detectedFaces ->
         FaceBoundingBoxViewer, FaceDetectionSidebar
       * faceStore.selectedFaceId -> FaceLabelCard
       * FaceLabelCard -> faceStore.saveFaceMapping(faceId, memberId)
       * faceStore.loading, faceStore.error -> Hiển thị trạng thái tải/lỗi.

  ##### 3.3. FaceSearchView.vue

   * Mục đích: Tải lên một ảnh khuôn mặt duy nhất và tìm kiếm các thành viên
     phù hợp.
   * Bố cục UI:
       * Tiêu đề: "Tìm kiếm khuôn mặt"
       * Component FaceUploadInput (chỉ cho phép một ảnh).
       * Khi ảnh được tải lên và tìm kiếm hoàn tất:
           * Hiển thị ảnh khuôn mặt đã tải lên.
           * Hiển thị FaceResultList với các thành viên phù hợp.
   * Tương tác:
       * Người dùng tải lên một ảnh khuôn mặt.
       * FaceUploadInput kích hoạt hành động searchByFace trong faceStore.
       * faceStore gửi yêu cầu API tìm kiếm khuôn mặt.
       * API trả về SearchResult[].
       * faceStore cập nhật faceSearchResults.
       * FaceResultList hiển thị các kết quả.
       * Người dùng có thể nhấp vào một kết quả để xem hồ sơ thành viên.
   * Luồng dữ liệu:
       * FaceUploadInput -> faceStore.searchByFace(file)
       * faceStore.faceSearchResults -> FaceResultList
       * faceStore.loading, faceStore.error -> Hiển thị trạng thái tải/lỗi.

  4. Thiết kế các Component

  ##### 4.1. FaceBoundingBoxViewer.vue

   * Mục đích: Hiển thị hình ảnh và vẽ các hộp giới hạn xung quanh các khuôn
     mặt được phát hiện.
   * Props: imageSrc (string), faces (DetectedFace[]), selectable (boolean,
     default: false), selectedFaceId (string | null).
   * Events: @face-selected (faceId: string).
   * UI:
       * Sử dụng v-img của Vuetify để hiển thị imageSrc.
       * Sử dụng một lớp phủ (overlay) hoặc SVG để vẽ các hộp giới hạn
         (v-overlay hoặc v-sheet với position: absolute).
       * Mỗi hộp giới hạn sẽ có một đường viền màu sắc khác nhau dựa trên
         face.status (xanh lá cho recognized, cam cho unrecognized, xanh dương
         cho newly-labeled).
       * Khi selectable là true, các hộp giới hạn có thể nhấp được và phát ra
         sự kiện @face-selected.
       * Hộp giới hạn của selectedFaceId sẽ có một kiểu dáng nổi bật (ví dụ:
         đường viền dày hơn).

  ##### 4.2. FaceLabelCard.vue

   * Mục đích: Hiển thị một khuôn mặt đã cắt và cung cấp các trường nhập liệu
     để gán nhãn cho khuôn mặt đó.
   * Props: face (DetectedFace).
   * Events: @save-mapping (faceId: string, memberId: string),
     @create-new-member (faceId: string, memberData: any).
   * UI:
       * v-card để chứa thông tin.
       * v-avatar hoặc v-img để hiển thị face.imageUrl (khuôn mặt đã cắt).
       * Trường tìm kiếm (v-autocomplete hoặc v-select) để tìm kiếm và chọn
         thành viên hiện có.
           * Hiển thị ảnh đại diện và tên đầy đủ của thành viên trong danh
             sách gợi ý.
       * Nút "Tạo thành viên mới" (v-btn) mở một dialog/form nhỏ để nhập tên
         và các thông tin cơ bản khác.
       * Nút "Lưu" (v-btn) để gửi faceId và memberId đã chọn.
       * Hiển thị trạng thái tải (v-progress-circular) và lỗi (v-alert).

  ##### 4.3. FaceResultList.vue

   * Mục đích: Hiển thị danh sách các thành viên phù hợp từ kết quả tìm kiếm
     khuôn mặt.
   * Props: results (SearchResult[]).
   * UI:
       * v-list để hiển thị từng kết quả.
       * Mỗi v-list-item hiển thị:
           * v-avatar của thành viên.
           * Tên đầy đủ của thành viên.
           * Độ tin cậy phù hợp (ví dụ: "95% Match").
           * Nút "Xem hồ sơ" (v-btn) để điều hướng đến trang hồ sơ thành
             viên.
       * Hiển thị thông báo "Không tìm thấy kết quả" nếu danh sách trống.

  ##### 4.4. FaceUploadInput.vue (Component chung)

   * Mục đích: Cung cấp giao diện tải ảnh lên đơn giản.
   * Props: label (string), accept (string), multiple (boolean).
   * Events: @file-uploaded (file: File | File[]).
   * UI:
       * v-file-input hoặc v-card với vùng kéo thả (v-overlay hoặc v-sheet
         với v-icon và văn bản hướng dẫn).
       * Hiển thị xem trước ảnh đã chọn (nếu có).

  ##### 4.5. FaceDetectionSidebar.vue

   * Mục đích: Hiển thị danh sách các khuôn mặt được phát hiện, cho phép chọn
     để gán nhãn.
   * Props: faces (DetectedFace[]), selectedFaceId (string | null).
   * Events: @face-selected (faceId: string).
   * UI:
       * v-navigation-drawer hoặc v-sheet để tạo sidebar.
       * v-list để hiển thị từng khuôn mặt.
       * Mỗi v-list-item hiển thị:
           * v-avatar của khuôn mặt đã cắt (face.imageUrl).
           * Tên thành viên đã gán nhãn (nếu có).
           * Biểu tượng trạng thái (ví dụ: dấu tích xanh cho đã gán nhãn, dấu
             chấm than cam cho chưa gán nhãn).
           * Màu nền hoặc đường viền của v-list-item thay đổi dựa trên
             face.status và selectedFaceId.

  5. Tương tác người dùng & Luồng dữ liệu

   1. Tải ảnh lên & Phát hiện:
       * Người dùng truy cập FaceUploadView.
       * Tải ảnh lên qua FaceUploadInput.
       * FaceUploadInput gọi faceStore.detectFaces(file).
       * faceStore gửi yêu cầu API phát hiện khuôn mặt.
       * API trả về DetectedFace[].
       * faceStore cập nhật uploadedImage và detectedFaces.
       * FaceBoundingBoxViewer và FaceDetectionSidebar phản ứng với các thay
         đổi của faceStore.uploadedImage và faceStore.detectedFaces.
       * Người dùng nhấp vào "Tiếp tục" để điều hướng đến FaceLabelingView.

   2. Gán nhãn khuôn mặt:
       * FaceLabelingView hiển thị ảnh và sidebar.
       * Người dùng nhấp vào một khuôn mặt trong FaceBoundingBoxViewer hoặc
         FaceDetectionSidebar.
       * Sự kiện @face-selected được phát ra, gọi
         faceStore.selectFace(faceId).
       * FaceLabelCard hiển thị cho faceStore.currentSelectedFace.
       * Người dùng tìm kiếm/chọn thành viên hoặc tạo thành viên mới trong
         FaceLabelCard.
       * Khi lưu, FaceLabelCard gọi faceStore.saveFaceMapping(faceId, 
         memberId).
       * faceStore gửi yêu cầu API để lưu ánh xạ.
       * Sau khi thành công, faceStore cập nhật trạng thái của DetectedFace
         đó (ví dụ: status: 'newly-labeled').
       * FaceDetectionSidebar cập nhật hiển thị.

   3. Tìm kiếm khuôn mặt:
       * Người dùng truy cập FaceSearchView.
       * Tải lên một ảnh khuôn mặt duy nhất qua FaceUploadInput.
       * FaceUploadInput gọi faceStore.searchByFace(file).
       * faceStore gửi yêu cầu API tìm kiếm khuôn mặt.
       * API trả về SearchResult[].
       * faceStore cập nhật faceSearchResults.
       * FaceResultList hiển thị các kết quả.
       * Người dùng nhấp vào một kết quả để điều hướng đến hồ sơ thành viên.

  6. Phong cách UI & Khả năng tiếp cận

   * Phong cách UI:
       * Sử dụng các thành phần Vuetify 3 như v-card, v-container, v-row,
         v-col, v-btn, v-text-field, v-autocomplete, v-avatar, v-img,
         v-snackbar, v-dialog, v-navigation-drawer.
       * Bố cục hiện đại với lưới và khoảng cách đáp ứng.
       * Góc bo tròn (rounded-lg), đổ bóng nhẹ (elevation-2, elevation-4).
       * Sử dụng v-progress-circular hoặc v-overlay để hiển thị trạng thái
         tải.
       * Thiết kế sạch sẽ, thân thiện với người dùng, dễ hiểu cho cả những
         thành viên gia đình không chuyên về công nghệ.
   * Khả năng tiếp cận:
       * Sử dụng v-label và label cho tất cả các trường nhập liệu.
       * Cung cấp tooltip (v-tooltip) cho các biểu tượng hoặc hành động không
         rõ ràng.
       * Sử dụng màu sắc có độ tương phản cao.
       * Mã màu cho trạng thái khuôn mặt:
           * Xanh lá cây: Khuôn mặt được nhận dạng (recognized).
           * Cam: Khuôn mặt chưa được nhận dạng (unrecognized).
           * Xanh dương: Khuôn mặt mới được gán nhãn (newly-labeled).
       * Đảm bảo điều hướng bằng bàn phím và hỗ trợ trình đọc màn hình.