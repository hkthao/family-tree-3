import numpy as np
from typing import List
from PIL import Image as PILImage
import cv2
from insightface.app import FaceAnalysis

from src.domain.interfaces.face_embedding import IFaceEmbedding

class ArcFaceEmbedding(IFaceEmbedding):
    def __init__(self):
        # Khởi tạo FaceAnalysis để tải mô hình nhúng.
        # 'buffalo_l' thường bao gồm một mô hình nhúng mạnh mẽ.
        self.app = FaceAnalysis(name='buffalo_l', providers=['CPUExecutionProvider'])
        # Chuẩn bị ứng dụng. Nó sẽ tải tất cả các mô hình cần thiết, bao gồm mô hình nhận dạng.
        self.app.prepare(ctx_id=0, det_size=(640, 640)) 

    def get_embedding(self, face_image: PILImage) -> List[float]:
        # Chuyển đổi PIL Image (RGB) sang mảng NumPy
        img_np = np.array(face_image)

        # Các mô hình của Insightface thường mong đợi định dạng ảnh BGR.
        # Chuyển đổi RGB (từ PIL) sang BGR (cho insightface).
        if img_np.ndim == 3 and img_np.shape[2] == 3: # Kiểm tra xem đây có phải là ảnh 3 kênh (RGB) không
            img_np_bgr = cv2.cvtColor(img_np, cv2.COLOR_RGB2BGR)
        elif img_np.ndim == 2: # Ảnh xám, chuyển đổi sang BGR
            img_np_bgr = cv2.cvtColor(img_np, cv2.COLOR_GRAY2BGR)
        elif img_np.shape[2] == 4: # RGBA, chuyển đổi sang BGR
            img_np_bgr = cv2.cvtColor(img_np, cv2.COLOR_RGBA2BGR)
        else:
            img_np_bgr = img_np # Giả sử nó đã là BGR hoặc định dạng khác mà insightface có thể xử lý

        # Sử dụng app.get() để phát hiện, căn chỉnh và nhúng.
        # Ngay cả khi đầu vào là một khuôn mặt đã được cắt, app.get() vẫn hoạt động.
        faces = self.app.get(img_np_bgr)

        if not faces:
            # Nếu không tìm thấy khuôn mặt nào trong ảnh đã cắt, trả về nhúng rỗng hoặc báo lỗi.
            # Trả về một danh sách rỗng có thể an toàn hơn để nhất quán.
            print("Warning: No face detected in the provided cropped image for embedding.")
            return []

        # Giả sử ảnh đã cắt chỉ chứa một khuôn mặt, chúng ta lấy khuôn mặt đầu tiên.
        face = faces[0]

        # Nhúng là một mảng numpy, chuyển đổi sang danh sách các số float
        return face.embedding.tolist()
