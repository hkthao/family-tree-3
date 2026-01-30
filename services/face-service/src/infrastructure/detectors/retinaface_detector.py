import numpy as np
from typing import List, Dict, Any
from insightface.app import FaceAnalysis

from src.domain.interfaces.face_detector import IFaceDetector

class RetinaFaceDetector(IFaceDetector):
    def __init__(self):
        # Khởi tạo FaceAnalysis với det_name='retinaface_mnet025' cho RetinaFace MobileNetV2
        # và providers=['CPUExecutionProvider'] để đảm bảo chạy trên CPU
        self.app = FaceAnalysis(name='buffalo_l', providers=['CPUExecutionProvider'])
        self.app.prepare(ctx_id=0, det_size=(640, 640)) # ctx_id=0 cho CPU

    def detect_faces(self, image: np.ndarray) -> List[Dict[str, Any]]:
        # Insightface FaceAnalysis mong đợi input là BGR
        # OpenCV mặc định đọc ảnh là BGR, nhưng nếu ảnh được load bằng PIL thì có thể là RGB
        # Để đảm bảo, nếu ảnh là RGB, cần chuyển đổi sang BGR
        # Hiện tại, giả định input image đã là BGR hoặc không cần chuyển đổi
        
        faces = self.app.get(image)
        detected_faces = []
        for face in faces:
            # Bounding box từ insightface là [x1, y1, x2, y2]
            # Cần chuyển đổi sang [x, y, w, h]
            bbox = face.bbox.astype(int)
            x, y, x2, y2 = bbox
            w, h = x2 - x, y2 - y
            
            # Confidence score
            det_score = face.det_score
            
            detected_faces.append({
                'box': [x, y, w, h],
                'confidence': float(det_score)
            })
        return detected_faces
