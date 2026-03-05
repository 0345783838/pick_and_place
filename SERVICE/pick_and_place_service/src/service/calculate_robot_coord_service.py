import math
import time
import cv2
import numpy as np
from src.dtos.meta import DataResponse, ErrorCode
from src.service.base_service import BaseService
import base64
import ast


class CalculateRobotCoordService(BaseService):
    def __init__(self):
        super().__init__()
        pass

    @staticmethod
    def _convert_2_base64(image):
        success, encoded_image = cv2.imencode('.png', image)
        if not success:
            return None
        image_bytes = encoded_image.tobytes()
        img_base64 = base64.b64encode(image_bytes).decode("utf-8")

        return img_base64

    @staticmethod
    def _get_box_centers(boxes):
        centers = []
        for box in boxes:
            x_center = (box[0] + box[2]) / 2
            y_center = (box[1] + box[3]) / 2
            centers.append([x_center, y_center])
        return np.array(centers)

    @staticmethod
    def euclidean_distance(pointA, pointB):
        return np.linalg.norm(pointA - pointB)

    def cal_robot_coord(self, image):
        # calculate robot arm coord
        pass


if __name__ == '__main__':
    pass