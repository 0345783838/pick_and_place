import cv2
import numpy as np
import glob
import os

def calibrate_camera_from_folder(
        image_folder,
        pattern_size=(9, 6),     # số inner corners (cols, rows)
        square_size=25.0         # mm hoặc đơn vị bạn dùng
):
    """
    image_folder : thư mục chứa ảnh chessboard
    pattern_size : (cols, rows) số INNER corners
    square_size  : kích thước 1 ô (mm)
    """

    # Chuẩn bị object points (0,0,0), (1,0,0), (2,0,0) ...
    objp = np.zeros((pattern_size[0] * pattern_size[1], 3), np.float32)
    objp[:, :2] = np.mgrid[0:pattern_size[0],
                           0:pattern_size[1]].T.reshape(-1, 2)

    objp *= square_size

    objpoints = []  # 3D points
    imgpoints = []  # 2D points

    images = glob.glob(os.path.join(image_folder, '*.bmp'))
    print(f"Found {len(images)} images")

    for fname in images:
        img = cv2.imread(fname)
        gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)

        ret, corners = cv2.findChessboardCorners(gray, pattern_size, None)

        if ret:
            objpoints.append(objp)

            # refine corner
            criteria = (cv2.TERM_CRITERIA_EPS +
                        cv2.TERM_CRITERIA_MAX_ITER, 30, 0.001)

            corners2 = cv2.cornerSubPix(
                gray, corners, (11, 11), (-1, -1), criteria)

            imgpoints.append(corners2)

            print(f"[OK] {fname}")
        else:
            print(f"[FAIL] {fname}")

    # Calibration
    ret, camera_matrix, dist_coeffs, rvecs, tvecs = cv2.calibrateCamera(
        objpoints,
        imgpoints,
        gray.shape[::-1],
        None,
        None
    )
    for fname in images:
        img = cv2.imread(fname)
        undistorted = cv2.undistort(img, camera_matrix, dist_coeffs)
        cv2.imwrite(f"out/{os.path.basename(fname)}", img)
        cv2.imwrite(f"out/{os.path.basename(fname).split('.bmp')[0]}_undistorted.bmp", undistorted)

    cv2.imshow("original", img)
    cv2.imshow("undistorted", undistorted)

    print("\n===== CALIB RESULT =====")
    print("RMS error:", ret)
    print("Camera matrix:\n", camera_matrix)
    print("Distortion coeffs:\n", dist_coeffs)

    return camera_matrix, dist_coeffs


if __name__ == "__main__":
    folder_path = r"chessboard_images"

    camera_matrix, dist_coeffs = calibrate_camera_from_folder(
        folder_path,
        pattern_size=(10, 7),   # chỉnh theo bàn cờ của bạn
        square_size=1.0       # mm
    )

    # Save file
    np.save("camera_matrix.npy", camera_matrix)
    np.save("dist_coeffs.npy", dist_coeffs)

    print("\nSaved camera_matrix.npy & dist_coeffs.npy")