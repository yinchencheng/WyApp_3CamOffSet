
namespace WY_App.Utility
{

    public class Constructor
    {
        public class CameraParams
        {
			public double[] ExposureTime = new double[4];
			public int[] GammaEnable = new int[4];
			public int[] FPNCUserEnable = new int[4];
			public int[] PRNUCUserEnable = new int[4];
			public int[] DeviceTapGeometry = new int[4];
			public int[] Height = new int[4];
			public int[] AcquisitionLineRate = new int[4];
			public int[] PreampGain = new int[4];
			public CameraParams()
            {
                for (int i = 0; i < 4; i++)
                {
					ExposureTime[i] = 50;
					GammaEnable[i] = 0;
					FPNCUserEnable[i] = 0;
					PRNUCUserEnable[i] = 0;
					DeviceTapGeometry[i] = 0;
					Height[i] = 16000;
					AcquisitionLineRate[i] = 50000;
					PreampGain[i] = 0;
				}
            }
        }

		public static CameraParams cameraParams = new CameraParams();

		public class Motor
        {
            public int HighSpeed;
            public int GoHomeSpeed;
            public int FirstPosition;
            public int HandStep;
            public int AutoStep;
            public Motor()
            {
                HighSpeed = 1000;
                GoHomeSpeed = 2000;
                FirstPosition = 50;
                HandStep = 200;
                AutoStep = 300;
            }
        }
    }
}
