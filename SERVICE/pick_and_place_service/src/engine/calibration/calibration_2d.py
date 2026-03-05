import numpy as np
import cv2
import json


class Calibration2D:
    def __init__(self):
        self.matrix = None

    def is_calibrated(self):
        return self.matrix is not None

    def fit(self, pixel_points, robot_points):
        pixel_points = np.array(pixel_points, dtype=np.float32)
        robot_points = np.array(robot_points, dtype=np.float32)

        M, inliers = cv2.estimateAffine2D(
            pixel_points,
            robot_points,
            method=cv2.RANSAC
        )

        self.matrix = M
        return M, inliers

    def transform(self, pixel_point):
        pt = np.array([[pixel_point]], dtype=np.float32)
        result = cv2.transform(pt, self.matrix)
        return result[0][0]

    def save(self, path):
        try:
            if self.matrix is None:
                return False, "Matrix is empty"

            with open(path, "w") as f:
                json.dump(self.matrix.tolist(), f)

            return True, None

        except Exception as e:
            return False, str(e)

    def load(self, path):
        try:
            with open(path, "r") as f:
                data = json.load(f)

            self.matrix = np.array(data, dtype=np.float32)

            if self.matrix.shape != (2, 3):
                return False, "Invalid matrix shape"

            return True, None

        except Exception as e:
            return False, str(e)