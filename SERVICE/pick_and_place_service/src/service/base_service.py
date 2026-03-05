# Import the Model Achitecture here!!!
import decouple

from src.engine.calibration.calibration_2d import Calibration2D
from src.tools.caliper_advanced_new import AdvancedMultiEdgeCaliper

# Load config here!!!
config = decouple.config
Csv = decouple.Csv


# Check if there is any config


# Get the config from the config file

CALIB_FILE_PATH = config('CALIB_FILE_PATH')

calib = Calibration2D()
calib_file_path = CALIB_FILE_PATH


class BaseService:
    def __init__(self):
        if not hasattr(self, '_initialized'):
            self._initialized = True
            self.calib = calib
            self.calib_file_path = calib_file_path
        else:
            pass
