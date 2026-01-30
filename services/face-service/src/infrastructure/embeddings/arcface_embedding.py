import os
import numpy as np
from typing import List
from PIL import Image as PILImage
import cv2
import onnxruntime

from src.domain.interfaces.face_embedding import IFaceEmbedding

class ArcFaceEmbedding(IFaceEmbedding):
    def __init__(self):
        # Tải mô hình nhận dạng trực tiếp từ file .onnx
        model_path = os.path.join('app', 'models', 'onnx_models', 'w600k_r50.onnx')

        self.rec_session = onnxruntime.InferenceSession(model_path, providers=['CPUExecutionProvider'])
        # Lấy tên input và output của mô hình
        self.input_name = self.rec_session.get_inputs()[0].name
        self.output_name = self.rec_session.get_outputs()[0].name 

    def _preprocess(self, face_bgr: np.ndarray) -> np.ndarray:
        """
        Chuẩn hóa ảnh face cho ArcFace MobileFaceNet.
        Output shape: (1, 3, 112, 112)
        """
        face = cv2.resize(face_bgr, (112, 112))
        face = cv2.cvtColor(face, cv2.COLOR_BGR2RGB)
        face = face.astype(np.float32)
        face = (face - 127.5) / 128.0
        face = np.transpose(face, (2, 0, 1))  # HWC → CHW
        face = np.expand_dims(face, axis=0)
        return face 


    def get_embedding(self, face_image: PILImage) -> List[float]:
        # Chuyển đổi PIL Image (RGB) sang mảng NumPy
        img_np = np.array(face_image)

        # Các mô hình của Insightface thường mong đợi định dạng ảnh BGR.
        # Chuyển đổi RGB (từ PIL) sang BGR (cho insightface).
        if img_np.ndim == 3 and img_np.shape[2] == 3:
            img_np_bgr = cv2.cvtColor(img_np, cv2.COLOR_RGB2BGR)
        elif img_np.ndim == 2:
            img_np_bgr = cv2.cvtColor(img_np, cv2.COLOR_GRAY2BGR)
        elif img_np.shape[2] == 4:
            img_np_bgr = cv2.cvtColor(img_np, cv2.COLOR_RGBA2BGR)
        else:
            img_np_bgr = img_np

        # Tiền xử lý ảnh theo yêu cầu của model
        preprocessed_face = self._preprocess(img_np_bgr)


        # Thực hiện suy luận bằng ONNX Runtime Session
        # Input name và output name đã được lấy từ __init__
        embedding_array = self.rec_session.run([self.output_name], {self.input_name: preprocessed_face})[0]
        
        # Flatten the array if it's 2D (e.g., (1, D)) to ensure .tolist() returns List[float]
        if embedding_array.ndim == 2 and embedding_array.shape[0] == 1:
            embedding_array = embedding_array[0] # Take the first (and only) row
        
        if embedding_array.size == 0:
            return []
        
        return embedding_array.tolist()
        