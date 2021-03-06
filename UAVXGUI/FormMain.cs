
// =================================================================================================
// =                                  UAVX Quadrocopter Groundstation                              =
// =                             Copyright (c) 2008 by Prof. Greg Egan                             =
// =                         Instruments Copyright (c) 2008 Guillaume Choutea                      =                                        */
// =                               http://code.google.com/p/uavp-mods/                             =
// =================================================================================================

//    This is part of UAVX.

//    UAVX is free software: you can redistribute it and/or modify it under the terms of the GNU 
//    General Public License as published by the Free Software Foundation, either version 3 of the 
//    License, or (at your option) any later version.

//    UAVX is distributed in the hope that it will be useful,but WITHOUT ANY WARRANTY; without even 
//    the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU 
//    General Public License for more details.

//    You should have received a copy of the GNU General Public License along with this program.  
//    If not, see http://www.gnu.org/licenses/




using System.Threading;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Net;
using System.Net.Sockets;

using System.Resources;

using System.Speech;
using System.Speech.Synthesis;

using System.Reflection;
using System.Drawing.Drawing2D;
using System.Net.NetworkInformation;

using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.MapProviders;


namespace UAVXGUI
{
    public partial class FormMain : Form
    {
        const byte FR_CYCLETIME =          125;

        const byte FR_PROTOCOL_HEADER = 0x5E;
        const byte FR_PROTOCOL_TAIL = 0x5E;

        // Data Ids  (bp = before decimal point; af = after decimal point)
        // FrSky old DATA IDs (1 byte)
        const byte FR_GPS_ALT_BP_ID =      0x01;
        const byte FR_TEMP1_ID =           0x02;
        const byte FR_RPM_ID =             0x03;
        const byte FR_FUEL_ID =            0x04;
        const byte FR_TEMP2_ID =           0x05;
        const byte FR_VOLTS_ID =           0x06;
        const byte FR_GPS_ALT_AP_ID =      0x09;
        const byte FR_BARO_ALT_BP_ID =     0x10;

        const byte FR_GPS_SPEED_BP_ID =    0x11;
        const byte FR_GPS_LONG_BP_ID =     0x12;
        const byte FR_GPS_LAT_BP_ID =      0x13;
        const byte FR_GPS_COURS_BP_ID =    0x14;

        const byte FR_GPS_DAY_MONTH_ID =   0x15;
        const byte FR_GPS_YEAR_ID =        0x16;
        const byte FR_GPS_HOUR_MIN_ID =    0x17;
        const byte FR_GPS_SEC_ID =         0x18;

        const byte FR_GPS_SPEED_AP_ID =    0x19; // +8 from BP
        const byte FR_GPS_LONG_AP_ID =     0x1A;
        const byte FR_GPS_LAT_AP_ID =      0x1B;
        const byte FR_GPS_COURS_AP_ID =    0x1C;

        const byte FR_BARO_ALT_AP_ID =     0x21;

        const byte FR_GPS_LONG_EW_ID =     0x22;
        const byte FR_GPS_LAT_NS_ID =      0x23;
        const byte FR_ACCEL_X_ID =         0x24;
        const byte FR_ACCEL_Y_ID =         0x25;
        const byte FR_ACCEL_Z_ID =         0x26;
        const byte FR_CURRENT_ID =         0x28;
        const byte FR_VARIO_ID =           0x30;
        const byte FR_VFAS_ID =            0x39;
        const byte FR_VOLTS_BP_ID =        0x3A;
        const byte FR_VOLTS_AP_ID =        0x3B;
        const byte FR_FRSKY_LAST_ID =      0x3F;

        // User defined data IDs
        const byte FR_ID_GYRO_X =            0x40;
        const byte FR_ID_GYRO_Y =            0x41;
        const byte FR_ID_GYRO_Z =            0x42;

	    // 0x43

	    const byte FS_ID_PITCH =            0x44;
	    const byte FS_ID_ROLL =             0x45;
	    //FS_ID_YAW = 0x46, // redundant

	    // 0x47

        const byte FS_ID_MAH =              0x48;


        // FrSky new DATA IDs (2 bytes)
        const int FR_RSSI_ID =                 0xf101;
        const int FR_ADC1_ID =                 0xf102;
        const int FR_ADC2_ID =                 0xf103;
        const int FR_BATT_ID =                 0xf104;
        const int FR_SWR_ID =                  0xf105;
        const int FR_T1_FIRST_ID =             0x0400;
        const int FR_T1_LAST_ID =              0x040f;
        const int FR_T2_FIRST_ID =             0x0410;
        const int FR_T2_LAST_ID =              0x041f;
        const int FR_RPM_FIRST_ID =            0x0500;
        const int FR_RPM_LAST_ID =             0x050f;
        const int FR_FUEL_FIRST_ID =           0x0600;
        const int FR_FUEL_LAST_ID =            0x060f;
        const int FR_ALT_FIRST_ID =            0x0100;
        const int FR_ALT_LAST_ID =             0x010f;
        const int FR_VARIO_FIRST_ID =          0x0110;
        const int FR_VARIO_LAST_ID =           0x011f;
        const int FR_ACCX_FIRST_ID =           0x0700;
        const int FR_ACCX_LAST_ID =            0x070f;
        const int FR_ACCY_FIRST_ID =           0x0710;
        const int FR_ACCY_LAST_ID =            0x071f;
        const int FR_ACCZ_FIRST_ID =           0x0720;
        const int FR_ACCZ_LAST_ID =            0x072f;
        const int FR_CURR_FIRST_ID =           0x0200;
        const int FR_CURR_LAST_ID =            0x020f;
        const int FR_VFAS_FIRST_ID =           0x0210;
        const int FR_VFAS_LAST_ID =            0x021f;
        const int FR_CELLS_FIRST_ID =          0x0300;
        const int FR_CELLS_LAST_ID =           0x030f;
        const int FR_GPS_LONG_LATI_FIRST_ID =  0x0800;
        const int FR_GPS_LONG_LATI_LAST_ID =   0x080f;
        const int FR_GPS_ALT_FIRST_ID =        0x0820;
        const int FR_GPS_ALT_LAST_ID =         0x082f;
        const int FR_GPS_SPEED_FIRST_ID =      0x0830;
        const int FR_GPS_SPEED_LAST_ID =       0x083f;
        const int FR_GPS_COURS_FIRST_ID =      0x0840;
        const int FR_GPS_COURS_LAST_ID =       0x084f;
        const int FR_GPS_TIME_DATE_FIRST_ID =  0x0850;
        const int FR_GPS_TIME_DATE_LAST_ID =   0x085f;

        // FrSky wrong IDs ?
        const int FR_BETA_VARIO_ID =      		0x8030;
        const int FR_BETA_BARO_ALT_ID =        0x8010;

        const float MILLIRADDEG = (float)0.057295;
        const short DEGMILLIRAD = 17;

        const double DEFAULT_HOME_LAT = 0; 
        const double DEFAULT_HOME_LON = 0;
        const double DEFAULT_LON_CORR = 1.0f;

        public const byte MaxWayPoints = 14;

        // ASCII Constants
        const byte NUL = 0;
        const byte SOH = 1;
        const byte EOT = 4;
        const byte ACK = 6;
        const byte HT = 9;
        const byte LF = 10;
        const byte CR = 13;
        const byte NAK = 21;
        const byte ESC = 27;

        static SpeechSynthesizer speech;

        const int Pitch = 0;
        const int Roll = 1;
        const int Yaw = 2;

        const byte FB = 0;
        const byte LR = 1; 
        const byte DU = 2;

        public const byte MAX_PARAMS = 128;
        public const byte MAX_PARAM_SETS = 1; // 4;
        public const short RC_MAX_CHANNELS = 20; // graphic restriction
        const short RCMaximum = 1000;
        const double OUTMaximumScale = 0.5; // 100/200 % for PWM at least

        public const byte UnknownPacketTag = 0;
        public const byte RestartPacketTag = 8;
        public const byte UAVXFlightPacketTag = 13;
        public const byte UAVXNavPacketTag = 14;
        public const byte UAVXStatsPacketTag = 15;
        public const byte UAVXControlPacketTag = 16;
        public const byte UAVXParamPacketTag = 17;
        public const byte UAVXMinPacketTag = 18;
        public const byte UAVXOriginPacketTag = 19;
        public const byte UAVXWPPacketTag = 20;
        public const byte UAVXMissionPacketTag = 21;
        public const byte UAVXRCChannelsPacketTag = 22;

        public const byte UAVXRequestPacketTag = 50;
        public const byte UAVXAckPacketTag = 51;
        public const byte UAVXMiscPacketTag = 52;
        public const byte UAVXNoisePacketTag = 53;
        public const byte UAVXBBPacketTag = 54;
        public const byte UAVXInertialPacketTag = 55; // unused
        public const byte UAVXMinimOSDPacketTag = 56;
        public const byte UAVXTuningPacketTag = 57;

	 public const byte UAVXUKFPacketTag = 58;
	 public const byte UAVXGuidancePacketTag = 59;
	 public const byte UAVXFusionPacketTag = 60;
	 public const byte UAVXSoaringPacketTag = 61;
	 public const byte UAVXCalibrationPacketTag = 62;
	 public const byte UAVXAnglePIDPacketTag = 63;
     public const byte UAVXRatePIDPacketTag = 64;
     public const byte UAVXAltPIDPacketTag = 65;
     public const byte UAVXGPSPIDPacketTag = 66;
     public const byte UAVXRawIMUPacketTag = 67;

        public const byte FrSkyPacketTag = 99;

        public enum RxStates
        {
            WaitRxSentinel, WaitRxBody, WaitRxESC, WaitRxCheck, WaitRxCopy,
            WaitUPTag, WaitUPLength, WaitUPBody, WaitRxTag
        };

        public enum MiscComms 
        { miscCalIMU, miscCalMag, miscLB, miscUnused, miscBBDump, miscGPSBypass, miscCalAcc, miscCalGyro, miscBootLoad}

        public enum NavStates
        {
            HoldingStation, ReturningHome, AtHome, Descending, Touchdown,
            Transiting, Loitering, Orbiting, Perching, Takeoff, PIC, AcquiringAltitude, UsingThermal,
            UsingRidge,
            UsingWave,
            BoostClimb,
            AltitudeLimiting,
            JustGliding,
            RateControl,
            BypassControl,
            HorizonControl,
            NavStateUndefined
        };

        static NavStates PrevNavState = NavStates.NavStateUndefined;

        public enum NavComs { navVia, navOrbit, navPerch, navPOI, navLand, navUnknown };

        public static string[] NavComNames = {
            "Via",
            "Orbit",
            "Perch",
            "POI"
            };

   //     NavComs NavCom;

        const byte MAXPARAMS = 64;

        const double RadToDeg = (double)(180.0 / Math.PI);
        const double DegToRad = (double)(1.0 / RadToDeg);
        const double MilliRadToDeg = (double)(RadToDeg * 0.001);
        // const float AttitudeToDegrees = 156.0f;

        public int MaximumFenceRadius = 250; // You carry total responsibility if you increase this value
        public const int MaximumAltitudeLimit = 120; // You carry total responsibility if you increase this value 

        const double CurrentSensorMax = 50.0; // depends on current ADC used - needs calibration box?
        const double YawGyroRate = 400.0;
        const double RollPitchRateRate = 400.0;

        string[] TagNames = { 
        	"UnknownPacket",
	        "LevPacket",
	        "NavPacket",
	        "MicropilotPacket",
	        "WayPacket",
	        "AirframePacket",
	        "NavUpdatePacket",
	        "BasicPacket",
	        "RestartPacket",
	        "TrimblePacket",
	        "MessagePacket",
	        "EnvironmentPacket",
	        "BeaconPacket",

	        "UAVXFlightPacket",
	        "UAVXNavPacket",
	        "UAVXStatsPacket",
	        "UAVXControlPacket",
	        "UAVXParamPacket",
	        "UAVXMinPacket",
	        "UAVXOriginPacket",
	        "UAVXWPPacket",
            "UAVXMissionPacket",
            "UAVXRCChannelsPacket",

            "UAVXRequestPacketTag",
	        "UAVXAckPacketTag",
	        "UAVXMiscPacketTag",
	        "UAVXNoisePacketTag",
	        "UAVXBBPacketTag",
	        "UAVXInertialPacketTag",
	        "UAVXMinimOSDPacketTag",
	        "UAVXTuningPacketTag",
	        "UAVXUKFPacketTag",
	        "UAVXGuidancePacketTag",
            "UAVXFusionPacketTag",
            "UAVXSoaringPacketTag",
            "UAVXCalibrationPacketTag",
            "UAVXAnglePIDPacketTag",
            "UAVXRatePIDPacketTag",
            "UAVXAltPIDPacketTag",
             
            // needs padding strings zzz

                            

	        "FrSkyPacketTag"};

      static public  string[] AFNames = { 
            "Tricopter",
            "CoaxTri Y6",
            "VTail Y4",
            "Quadcopter",
            "X Quadcopter",
            "Coax Quad",
            "X Coax Quad",
            "Hexacopter",
            "X Hexacopter",
            "Octocopter",
            "X Octocopter",
            "Heli90",
            "Heli120",
            "Flying Wing",
            "Delta",
            "Aileron",
            "Spoilerons",
            "VTail",
            "Rudder Elevator",
            "VTOL",
            "VTOL2",
            "Gimbal",
            "Instrumentation",
            "IREmulator",
            "Unknown Aircraft" };

        public enum FlightStates
        {
            Starting,
            Warmup,
            Landing,
            Landed,
            Shutdown,
            Flying,
            IREmulate,
            Preflight,
            Ready,
            Launching,
            UnknownFlightState
        };


        public enum FlagValues
        {
            AltControlEnabled,
				UsingGPSAlt,
				RapidDescentHazard,
				LandingSwitch,
				NearLevel,
				LowBatt,
				GPSValid,
				OriginValid,

				// 1
				BaroFailure,
				IMUFailure,
				MagnetometerFailure,
				GPSFailure,
				AttitudeHold,
				ThrottleMoving,
				HoldingAlt,
				Navigate,

				// 2
				ReturnHome,
				WayPointAchieved,
				WayPointCentred,
				OrbitingWP,
				UsingRTHAutoDescend,
				BaroActive,
				RangefinderActive,
				UsingRangefinderAlt,

				// 3
				UsingPOI,
				Bypass,
				UsingAngleControl,
				Emulation,
				BadBusConfig,
				DrivesArmed,
				AccZBump,
				UseManualAltHold,

				// 4
				Saturation,
				DumpingBB,
				ParametersValid,
				Signal,
				UsingWPNavigation,
				IMUActive,
				MagnetometerActive,
				IsArmed,

				// 5
				IsFixedWing,
				InvertMagnetometer,
				MagCalibrated,
				UsingUplink,
				NewAltitudeValue,
				IMUCalibrated,
				CrossTrackActive,
				AccCalibrated

        };

        public static bool UsingFixedWing = false;
        public static bool NoUplink = false;

        // struct not used - just to document packet format
        // byte UAVXFlightPacketTag;   
        // byte Length;  
        const byte NoOfFlagBytes = 6;
        byte[] Flags = new byte[NoOfFlagBytes];
        public static FlightStates StateT;     // 8
        public static short BatteryVoltsT;     // 9
        short BatteryCurrentT;          // 11
        short BatteryChargeT;           // 13
        short RCGlitchesT;              // 15
        short DesiredThrottleT;         // 17


        short[] DesiredAngleT = new short[3];
        short[] AngleT = new short[3];
        short[] AccT = new short[3];
        short[] DesiredRateT = new short[3]; 
        short[] RateT = new short[3];

        short ROCT;                     // 43
        short FROCT;
        int AltitudeT;          // 45 24bits
        short CruiseThrottleT;          // 48
        short RangefinderAltitudeT;     // 50
        public static int DesiredAltitudeT;
        public static short HeadingT;  // 56
        public static short DesiredHeadingT;  // 56
        short TiltFFCompT;             // 58
        short BattFFCompT;
        short AltCompT;                 // 60
        short AccConfidenceT;           // 62

        int BaroTemperatureT;
        int BaroPressureT;
 
        int BaroAltitudeT, BaroRawAltitudeT;

        short AccZT, HRAccZT, HRAccZBiasT, FWGlideOffsetAngleT, NewTuningParameterT, FWRateEnergyT;

        short MagHeadingT;

        short[] PWMT = new short[10];    // 63
        short[] PWMDiagT = new short[10];

        int MissionTimeMilliSecT;
        int MissionTimeMicroSecT; 

        // UAVXNavPacket
        //byte UAVXNavPacketTag;
        //byte Length;
        public static NavStates NavStateT;            // 2
        public static byte AlarmStateT;                // 3
        public static byte GPSNoOfSatsT;              // 4
        public static byte GPSFixT;                   // 5

        public static byte CurrWPT;                   // 6

        public static short GPShAccT;              // 16
        public static short GPSvAccT;              // 16
        public static short GPSsAccT;              // 16
        public static short GPScAccT;              // 16

        public static short WPBearingT;        // 18
        public static short CrossTrackET;             // 20

        public static short GPSVelT;                  // 22
        public static short GPSHeadingT;              // 24
        public static int GPSAltitudeT;            // 26 3b
        public static int GPSLatitudeT;               // 29 
        public static int GPSLongitudeT;              // 33
        public static int NorthPosET;                 // 40
        public static int EastPosET;                  // 44
        public static int NavStateTimeoutT;           // 48 3b
        public static short MPU6XXXTempT;             // 51
        public static int GPSMissionTimeT;            // 53

        public static short NavSensitivityT;                 // 57
        public static short NavRCorrT;             // 59
        public static short NavPCorrT;            // 61
        public static short NavHeadingET;             // 63

        //UAVXRCChannelsPacket


        public static double GPSCFKpT;
        public static double AltCFKpT;


        public static short GPSNorthPosT;
        public static short NavNorthPosT;
        public static short GPSEastPosT;
        public static short NavEastPosT;
        public static short SensorAltT;
        public static short CFAltT;

        public static short GPSNorthVelT;
        public static short NavNorthVelT;
        public static short GPSEastVelT;
        public static short NavEastVelT;
        public static short SensorROCT;
        public static short CFROCT;

        public static double Distance;

        //UAVXRCChannelsPacket

        public static short RCPacketIntervalT = 0;
        public static short DiscoveredRCChannelsT = 0;
        public static short[] RCChannel = new short[RC_MAX_CHANNELS];


       //UAVXStatsPacket      
        short[] Cal = new short[32];

        //should be an enum!!!
        const byte GPSAltitudeX = 0;
        const byte BaroRelAltitudeX = 1;
        const byte SaturationX = 2;
        const byte GPSMinSatsX = 3;
        const byte MinROCX = 4;
        const byte MaxROCX = 5;
        const byte GPSMaxVelX = 6;
        const byte AccFailsX = 7;
        const byte CompassFailsX = 8;
        const byte BaroFailsX = 9;

        const byte GPSInvalidX = 10;
        const byte GPSMaxSatsX = 11;
        const byte NavValidX = 12;
        const byte MinhAccX = 13;
        const byte MaxhAccX = 14;
        const byte RCGlitchX = 15;
        const byte SPIFailsX = 16;
        const byte GyroFailsX = 17;
        const byte RCFailSafesX = 18;
        const byte I2CFailsX = 19;

        const byte UtilisationX = 20;
        const byte BadX = 21;
        const byte BadNumX = 22;

        const byte RollAccCorrAvX = 22;
        const byte RollAccCorrMeanX = 23;
        const byte PitchAccCorrAvX = 24;
        const byte PitchAccCorrMeanX = 25;
        const byte PitchRateMaxChangeX = 26;
        const byte FBAccMaxChangeX = 27;
        const byte RollRateMaxChangeX = 28;
        const byte LRAccMaxChangeX = 29;

        const byte YawGyroMaxChangeX = 30;
        const byte DUAccMaxChangeX = 31;

        const byte AirframeX = 32;
        const byte OrientX = 34;

        const byte MaxStats = 32;

        public static short AirframeT;

        short VersionLenT;
        string VersionNameT;

        short[] Stats = new short[MaxStats];

        public struct WPStructNV
        {
            public int LatitudeRaw;            // 0
            public int LongitudeRaw;           // 4
            public short Altitude;             // 8
            public double Velocity;             // 10
            public short Loiter;               // 12
            public short OrbitRadius;            // 14
            public short OrbitGroundAltitude;          // 16
            public double OrbitVelocity;          // 18
            public byte Action;                // 19
        };

        public static WPStructNV[] WP = new WPStructNV[256]; // should use actual max packets

        public struct MissionStruct
        {
            // byte[] UAVXNavFlags = new byte[8]; // 0
            public byte NoOfWayPoints;         // 9
            public byte ProximityAltitude;     // 10
            public byte ProximityRadius;       // 11
            public short FenceRadius;           // 12
            public short OriginAltitude;       // 12
            public int OriginLatitude;         // 14
            public int OriginLongitude;        // 18
        };

        public static double[] Inertial = new double[32];

        short[] Noise = new short[32];
        float NoiseErrors = 0;
        byte NoiseIsGyro = 0;
        float[] PID = new float[32];
        public static byte MaxPID;
        public Boolean AnglePIDHeader = false;
        public Boolean RatePIDHeader = false;
        public Boolean AltPIDHeader = false;
        short[] RawIMU = new short[7];
        public Boolean RawIMUHeader = false;

        public static short WPDistanceT;

        public static MissionStruct Mission;
        public static MissionStruct NewMission;

        public static bool NewMissionAvailable = false;

        public short[] UpdateParamFormet = new short[MAX_PARAMS];

        static byte PrevWPT = 255;

        int SpeakInterval = 20000;

        public static double CurrAlt, AltError;
        double EastDiff, NorthDiff;

        static bool VRSHazardDetected = false;

        static bool AutoLandEnabled = false;
        // bool InFlight = false;

        short NavYCorrT = 0;

        double LongitudeCorrection; 
        bool FirstGPSCoordinates = true;
        int FlightHeading;
        double FlightRoll, FlightPitch;
        double FlightRollp = 0.0;
        double FlightPitchp = 0.0;
        double OSO = 0.0;
        double OCO = 1.0;

        bool UAVXArm = true; // default

        bool CalibrateIMUEnabled = false;
        bool CalibrateAcc6PointEnabled = false;
        bool CalibrateMagEnabled = false;

        bool LogFileHeaderWritten = false;

        public static bool RxLoopbackEnabled = false;


        byte[] UAVXPacket = new byte[256]; // big enough to cope with stats etc at start of dump file
        byte[] FrSkyPacket = new byte[256];

        const short RxQueueLength = 16384;
        const short RxQueueMask = RxQueueLength - 1;
        byte[] RxQueue = new byte[RxQueueLength];
        short RxTail = 0;
        short RxHead = 0;

        bool TelemetryActive = false;
        bool TelemetryLostMessageUsed = false;

        int NavPacketsReceived = 0;
        int FlightPacketsReceived = 0;
        int StatsPacketsReceived = 0;
        int CalibrationPacketsReceived = 0;
        int ControlPacketsReceived = 0;
        short MotorsAndServos;

        public short CurrPS = 0;

        short RxPacketByteCount;
        byte RxPacketTag, RxPacketLength;
        RxStates PacketRxState;
        byte ReceivedPacketTag;
        bool PacketReceived = false;
        long ReplayProgress = 0;
        int ReplayDelay = 10;

        static byte TxCheckSum = 0;

        bool CheckSumError;
        short RxLengthErrors = 0, RxCheckSumErrors = 0, RxIllegalErrors = 0;

        double[,] Params = new double[2,MAXPARAMS];
        int[] Sticks = new int[16];

        byte RxCheckSum;

        bool InFlight = false;

        public static bool[] F = new bool[72];

        string FileName;

        System.IO.FileStream SaveLogFileStream;
        System.IO.BinaryWriter SaveLogFileBinaryWriter;
        System.IO.FileStream SaveBBFileStream;
        System.IO.BinaryWriter SaveBBFileBinaryWriter; 
        System.IO.FileStream SaveTextLogFileStream;
        System.IO.StreamWriter SaveTextLogFileStreamWriter;

        System.IO.FileStream OpenLogFileStream;
        System.IO.BinaryReader OpenLogFileBinaryReader;
 
        bool DoingLogfileReplay = false;
        bool ReadingTelemetry = false;

        NavForm formNav = new NavForm();
        UAVPForm formUAVP = new UAVPForm();

        public static SerialPort serialPort;

        public FormMain()
        {
          int i;

          InitializeComponent();

          for (i = 0; i < NoOfFlagBytes; i++)
              Flags[i] = 0;
          for (i = 0; i < 72; i++)
              F[i] = false;

          serialPort1 = new System.IO.Ports.SerialPort(this.components);
          serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(serialPort_DataReceived);

          speech = new SpeechSynthesizer();
          speech.Rate = -2;

          SpeakVarioCheckBox.Checked = Properties.Settings.Default.SpeakVario;
          SpeakVoltsCheckBox.Checked = Properties.Settings.Default.SpeakVolts;
          SpeakAltitudeCheckBox.Checked = Properties.Settings.Default.SpeakAltitude;
          SpeakWhereCheckBox.Checked = Properties.Settings.Default.SpeakWhere;
          SpeakNavCheckBox.Checked = Properties.Settings.Default.SpeakNav;
          SpeechEnableCheckBox.Checked = Properties.Settings.Default.SpeechEnable;
          timer_announce.Interval = Properties.Settings.Default.SpeakInterval * 1000;

          Mission.OriginAltitude = 0;
          Mission.OriginLatitude = 0; 
          Mission.OriginLongitude = 0;

          string[] AvailableCOMPorts = ComPorts.readPorts();
          foreach (string AvailableCOMPort in AvailableCOMPorts)
              COMSelectComboBox.Items.Add(AvailableCOMPort);
          COMSelectComboBox.Text = Properties.Settings.Default.COMPort;
          COMBaudRateComboBox.Text = string.Format("{0:n0}", Properties.Settings.Default.COMBaudRate);

          Version vrs = new Version(Application.ProductVersion);
          //this.Text = this.Text + " v" + vrs.Major + "." + vrs.Minor + "." + vrs.Build;// vrs.MajorRevision + 

          ReplayDelay = 20 - Convert.ToInt16(ReplayNumericUpDown.Text);

         // FrSkycheckBox1.Checked = UAVXGUI.Properties.Settings.Default.IsFrSky;
       
          string AppLogDir = UAVXGUI.Properties.Settings.Default.LogDirectory;
          if (!(Directory.Exists(AppLogDir))) {
            string sProgFilesLogDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            sProgFilesLogDir += "\\" + "UAVXLogs"; // " + vrs.Major + "." + vrs.Minor;
            if (!(Directory.Exists(sProgFilesLogDir))) {
              Directory.CreateDirectory(sProgFilesLogDir);
            }
            UAVXGUI.Properties.Settings.Default.LogDirectory = sProgFilesLogDir;
          }

        }

        private void timer_telemetry_Tick(object sender, EventArgs e)
        {
            TelemetryActive = false;
        } // timer_announce_Tick

        public static class ComPorts
        {
            public static string[] readPorts()
            {
                string[] ComPorts = System.IO.Ports.SerialPort.GetPortNames();
                Array.Sort(ComPorts);
                return ComPorts;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (serialPort1.IsOpen)
            {           
                e.Cancel = true; //cancel the fom closing
                Thread CloseDown = new Thread(new ThreadStart(CloseSerialOnExit)); //close port in new thread to avoid hang
                CloseDown.Start(); //close port in new thread to avoid hang
            }
   
        }

        private void CloseSerialOnExit()
        {
            try
            {
                serialPort1.Close(); //close the serial port
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); //catch any serial port closing error messages
            }
            this.Invoke(new EventHandler(NowClose)); //now close back in the main thread
        }

        private void NowClose(object sender, EventArgs e)
        {
            this.Close(); //now close the form
        }

        public void SetSerialPort(string portSelected, int speed, ref string errorMessage)
        {
            try
            {
                errorMessage = "";
                serialPort1.PortName = portSelected;
                serialPort1.ReadBufferSize = 65536;
                serialPort1.BaudRate = speed;
                serialPort1.ReceivedBytesThreshold = 1;
                serialPort1.Open();
                serialPort1.Close();
            }
            catch (Exception er)
            {
                //MessageBox.Show(Convert.ToString(er.Message));
                errorMessage = Convert.ToString(er.Message);
            }
        }


       // private void Form1_FormClosing(object sender, FormClosingEventArgs e)
       // {
       //     if (serialPort1.IsOpen) serialPort1.Close();
       // }

        private void COMSelectComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UAVXGUI.Properties.Settings.Default.COMPort = COMSelectComboBox.Text;
            UAVXGUI.Properties.Settings.Default.Save();
        }

        private void COMBaudRateComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UAVXGUI.Properties.Settings.Default.COMBaudRate = Convert.ToInt32(COMBaudRateComboBox.Text);
            UAVXGUI.Properties.Settings.Default.Save();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // If the port is closed, don't try to send a character.
            if (!serialPort1.IsOpen) return;

            // If the port is Open, declare a char[] array with one element.
            char[] buff = new char[1];

            // Load element 0 with the key character.
            buff[0] = e.KeyChar;

            // Send the one character buffer.
            serialPort1.Write(buff, 0, 1);

            // Set the KeyPress event as handled so the character won't
            // display locally. If you want it to display, omit the next line.
            e.Handled = true;
        }

        private void ReplayButton_Click(object sender, EventArgs e)
        {
            if (DoingLogfileReplay)
                CloseReplayLogFile();
             else
            {
                OpenLogFileDialog.Filter = "UAVX Log File (*.log)|*.log";
                OpenLogFileDialog.InitialDirectory = UAVXGUI.Properties.Settings.Default.LogDirectory;

                if (OpenLogFileDialog.ShowDialog() == DialogResult.OK)
                {
                    UAVXGUI.Properties.Settings.Default.LogDirectory = OpenLogFileDialog.InitialDirectory;
                    OpenLogFileStream = new System.IO.FileStream(OpenLogFileDialog.FileName, System.IO.FileMode.Open);
                    OpenLogFileBinaryReader = new System.IO.BinaryReader(OpenLogFileStream);

                    FileName = OpenLogFileDialog.FileName;
                    int fileExtPos = FileName.LastIndexOf(".");
                    if (fileExtPos >= 0)
                       FileName = OpenLogFileDialog.FileName.Substring(0, fileExtPos);


                    SaveTextLogFileStream = new System.IO.FileStream(FileName + ".csv", System.IO.FileMode.Create);
                    SaveTextLogFileStreamWriter = new System.IO.StreamWriter(SaveTextLogFileStream, System.Text.Encoding.ASCII);
                    LogFileHeaderWritten = false;
                    DoingLogfileReplay = true;

                    ReplayProgressBar.Value = 0;
                    ReplayProgress = 0;
                    Thread Replay = new Thread(new ThreadStart(ReadReplayLogFile)); 
                    Replay.Start();
                }
            }

        }

        private void CloseReplayLogFile()
        {
            OpenLogFileStream.Close();

            SaveTextLogFileStreamWriter.Flush();
           // SaveTextLogFileStreamWriter.Close();
           // SaveTextLogFileStream.Close();
        }

        private void CreateSaveLogFile()
        {
            FileName = UAVXGUI.Properties.Settings.Default.LogDirectory + "\\UAVX_" + 
                DateTime.Now.Year + "_" +
                DateTime.Now.Month + "_" +
                DateTime.Now.Day + "_" +
                DateTime.Now.Hour + "_" +
                DateTime.Now.Minute;

            SaveLogFileStream = new System.IO.FileStream(FileName + ".log", System.IO.FileMode.Create);
            SaveLogFileBinaryWriter = new System.IO.BinaryWriter(SaveLogFileStream);

            SaveTextLogFileStream = new System.IO.FileStream(FileName + ".csv", System.IO.FileMode.Create);
            SaveTextLogFileStreamWriter = new System.IO.StreamWriter(SaveTextLogFileStream, System.Text.Encoding.ASCII);
            LogFileHeaderWritten = false;

            NavPacketsReceived = 0;
            FlightPacketsReceived = 0;
            StatsPacketsReceived = 0;
            CalibrationPacketsReceived = 0;

        }

        private void CreateSaveBBLogFile()
        {
            FileName = UAVXGUI.Properties.Settings.Default.LogDirectory + "\\UAVX_" +
                DateTime.Now.Year + "_" +
                DateTime.Now.Month + "_" +
                DateTime.Now.Day + "_" +
                DateTime.Now.Hour + "_" +
                DateTime.Now.Minute;


            SaveBBFileStream = new System.IO.FileStream(FileName + "_BB.log", System.IO.FileMode.Create);
            SaveBBFileBinaryWriter = new System.IO.BinaryWriter(SaveBBFileStream);

        }

        private void WriteTextPIDRateFileHeader()
        {

            int i;
            for (i = 0; i < 3;i++ )
                SaveTextLogFileStreamWriter.Write("Desired, Rate, RateP, RateD, DriveOut,");
            SaveTextLogFileStreamWriter.WriteLine();
        }

        private void WriteTextPIDAngleFileHeader()
        {
            int i;
            for (i = 0; i < 2; i++ )
                SaveTextLogFileStreamWriter.Write("Desired, Angle, AngleP, AngleI, RateOut,");
            SaveTextLogFileStreamWriter.Write("AccBF, AccLR, AccUD");
            SaveTextLogFileStreamWriter.WriteLine();
        }

        private void WriteTextPIDAltitudeFileHeader()
        {    
             SaveTextLogFileStreamWriter.WriteLine("Alt, DesAlt, AltE, AltP, AltI, DesROC, ROC, ROCP, ROCI, ROCD, AltComp, DesThr, CruiseThr ");
        }

        private void WriteTextIMUFileHeader()
        {
            SaveTextLogFileStreamWriter.WriteLine("TimeuS, Ax, Ay, Az, Gx, Gy, Gz");

        }

        private void WriteTextLogFileHeader()
        {
            int i;

            SaveTextLogFileStreamWriter.Write("Time, Flight," +

                "AltHEn," +
                "GPSAltActive," + // stick programmed
                "VRSHaz," +
                "LandSw," +
                "Level," +
                "LowBatt," +
                "GPSVal," +
                "NavVal," +


                "BaroFail," +
                "IMUFail," +
                "MagFail," +
                "GPSFail," +
                "AttH," +
                "ThrMov," +
                "AltH," +
                "Nav," +

                "RTH," +
                "WPAchieved," +
                "WPCentred," +
                "WPOrbit," +
                "UseRTHDes," +
                "BaroActive," +
                "RFActive," +
                "UseRFAlt," +

                "UsePOI," +   // stick programmed
                "Bypass," +
                "Angle," +
                "Emulation," +
                "BadBusConfig," +
                "DrivesArmed," +
                "AccZBump," +
                "ManualAH," +

                "Signal," +
                "DumpBB," +
                "ParamVal," +
                "RCNew," +
                "WPNav," +
                "IMUActive," +
                "MagActive," +
                "ARMED," +

                "FixedWing," +
                "InvertMag," +
                "MagCal," +
                "Uplink," +
                "NewAltVal," +
                "IMUCal," +
                "CTrackActive," +
                "6ptAccCal,");

            SaveTextLogFileStreamWriter.Write("StateT," +
            "BattV," +
            "BattI," +
            "BattCh," +
            "RCGlitches, Interval, #Ch, ");

            for (i = 0; i < 9; i++)
                SaveTextLogFileStreamWriter.Write("RC["+ (i+1) + "],");

            SaveTextLogFileStreamWriter.Write("DesThr," +
         

            "DesPitch," +
            "PitchAngle," +
            "FBAcc," +
            "DesPitchRate," +
            "PitchRate," +

               "DesRoll," +
             "RollAngle," +
             "LRAcc," +
             "DesRollRate," +
             "RollRate," +

            "DesYaw," +  
            "YawAngleError," +
            "DUAcc," +      
            "DesYawRate," +       
            "YawRate," +          
            "AccConf," );

            if (Airframe.Text == "Flying_Wing")
                SaveTextLogFileStreamWriter.Write("Thr,R-Elev,L-Elev,-,L-Flap,-,-,-,-,R-Flap,");
            else
                if (Airframe.Text == "Delta") 
                    SaveTextLogFileStreamWriter.Write("Thr,R-Elev,L-Elev,-,L-Flap, -, -, -,Rud,R-Flap,");
                else if ((Airframe.Text == "Aileron") || (Airframe.Text == "Spoilerons") || (Airframe.Text == "Rudder_Elevator"))
                    SaveTextLogFileStreamWriter.Write("Thr, R-Ail,L-Ail,Elev,L-Flap, -, -, -,Rud,R-Flap,");
                else
                    if ((Airframe.Text == "Aileron") || (Airframe.Text == "Spoilerons") )
                        SaveTextLogFileStreamWriter.Write("Thr,RAil,LAil,Elev,LFlap, -, -, -,Rud,RFlap,");
                    else
                        if (Airframe.Text == "Rudder_Elevator")
                            SaveTextLogFileStreamWriter.Write("Thr,-,-,Elev,LFlap, -, -, -,Rud,RFlap,");
                        else
                SaveTextLogFileStreamWriter.Write("K1,K2,K3,K4, K7,K8,K9,K10, K5/CamP,K6/CamR,");

            SaveTextLogFileStreamWriter.Write("Nav," +
           "GPSTime," +
            "NavTimeout," +
            "NavState," +
            "AlarmState," +
            "GPSSats," +
            "GPSFix," +
            "GPShAcc," +
            "GPSvAcc," +
            "GPSsAcc," +
            "GPScAcc," +
            "GPSVel," +
            "GPSHeading," +
            "GPSRelAlt," +
            "GPSLat," +
            "GPSLon," +

            "Sens," +
            "CurrWP," +
            "WPBearing," +

            "CruiseThrottle," +
            "BaroT, " +
            "BaroP, " +
            "BaroRawAlt," +
            "BaroAlt," +
            "GPSAlt," +
            "RFAlt," +
            "RelAlt," +
            "DesAlt," +
            "ROC," +
            "FROC," +
            "AccZ," +
            "AccZBias," +
            "FWEnergy," + "GlideE," + "NewParam," +
            "AltComp," +         
            "TiltFFComp," +
            "BattFFComp," +
            "NorthPosE," +
            "EastPosE," +
            "NavPCorr," +
            "NavRCorr," +
            "GPSHeading," +
            "MagHeading," +
            "Heading," +
            "DesHeading," +

            "SensorTemp,");

            SaveTextLogFileStreamWriter.Write("Noise,N1,N2,N3,N4,N5,N6,N7,N8,");

            SaveTextLogFileStreamWriter.Write("Stats," +

            "GPSAltX," +
            "BaroRelAltX," +
            "ClipX," +
            "GPSMinSatsX," +
            "MinROCX," +
            "MaxROCX," +
            "GPSVelX," +
            "AccFailsX," +
            "CompassFailsX," +
            "BaroFailsX," +

            "GPSInvalidX," +
            "GPSMaxSatsX," +
            "NavValidX," +
            "MinhAccX," +
            "MaxhAccX," +
            "RCGlitchesX," +
            "GPSBaroScaleX," +
            "GyroFailX," +
            "RCFailsafesX," +
            "I2CFailX," +

            "UtilisationX," +
            "BadReferGKEX," +
            "BadNumX," + // 23
            "MinsAcc," +
            "MaxsAcc," +
            "," +
            "," +
            "," +
            "," +
            "," +
            ", " +

            ", " +

        "AFType");

        SaveTextLogFileStreamWriter.WriteLine();
        }
   

        void Connect() {

            if (InFlight)
            {
                InFlight = false;
                UAVXCloseTelemetry();
                ConnectButton.Text = "Disconnected";
                ConnectButton.BackColor = System.Drawing.Color.Red;
                BootLoadButton.BackColor = System.Drawing.Color.Green;
            }
            else
            {
                UAVXOpenTelemetry();
                //if (InFlight) { 
                //    ConnectButton.Text = "Connected";
                //    ConnectButton.BackColor = System.Drawing.Color.Green;
                // }
       
            }
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {

            Connect();

        }

        private void AlarmsButton_Click(object sender, EventArgs e)
        {
            SpeakAlarms();
        }

        private void CalibrateIMUButton_Click(object sender, EventArgs e)
        {
            if (((StateT == FlightStates.Preflight) || (StateT == FlightStates.Ready)) && !CalibrateIMUEnabled)
            {
                SendRequestPacket(UAVXMiscPacketTag, (byte)MiscComms.miscCalIMU, 0);
                CalibrateIMUButton.BackColor = Color.Orange;
                CalibrateIMUEnabled = true;
            }
        }

        private void CalibrateAcc6PointButton_Click(object sender, EventArgs e)
        {
            if (((StateT == FlightStates.Preflight) || (StateT == FlightStates.Ready)) && !CalibrateAcc6PointEnabled)
            {
                SendRequestPacket(UAVXMiscPacketTag, (byte)MiscComms.miscCalAcc, 0);
                CalibrateAcc6PointButton.BackColor = Color.Orange;
                CalibrateAcc6PointEnabled = true;
            }
        }

        private void CalibrateMagButton_Click(object sender, EventArgs e)
        {
            if (((StateT == FlightStates.Preflight) || (StateT == FlightStates.Ready)) && !CalibrateMagEnabled)
             {
                SendRequestPacket(UAVXMiscPacketTag, (byte)MiscComms.miscCalMag, 0);
                CalibrateMagButton.BackColor = Color.Orange;
                CalibrateMagEnabled = true;
            }
        }

        private void GPSBypassButton_Click(object sender, EventArgs e)
        {
            if ((StateT == FlightStates.Preflight) || (StateT == FlightStates.Ready)) 
            {
                SendRequestPacket(UAVXMiscPacketTag, (byte)MiscComms.miscGPSBypass, 0);
                GPSBypassButton.BackColor = Color.Red;
            }
        }

        private void BootLoadButton_Click(object sender, EventArgs e)
        {
            if ((StateT == FlightStates.Preflight) || (StateT == FlightStates.Ready))
            {
                SendRequestPacket(UAVXMiscPacketTag, (byte)MiscComms.miscBootLoad, 0);
                BootLoadButton.BackColor = Color.Red;
                InFlight = false;
                UAVXCloseTelemetry();
                ConnectButton.Text = "Disconnected";
                ConnectButton.BackColor = System.Drawing.Color.Red;
            }
        }
        private void DumpBBButton_Click(object sender, EventArgs e)
        {
            if (((StateT == FlightStates.Preflight) || (StateT == FlightStates.Ready)) && !F[(byte)FlagValues.DumpingBB])
                SendRequestPacket(UAVXMiscPacketTag, (byte)MiscComms.miscBBDump, 0);
        }



        //_______________________________________________________________________________________

        // Speech Announcements

        private void timer_announce_Tick(object sender, EventArgs e)
        {
 
            if (TelemetryActive)
            {
                TelemetryLostMessageUsed = false;

                Properties.Settings.Default.SpeakAltitude = SpeakAltitudeCheckBox.Checked;
                Properties.Settings.Default.SpeakWhere = SpeakWhereCheckBox.Checked;

                if (SpeechEnableCheckBox.Checked)
                {
                    if (SpeakVarioCheckBox.Checked) {
                        if (Math.Abs(ROCT) > 150) {
                            double Rate = Math.Abs(ROCT * 0.01);
                            if (ROCT < 0)
                                speech.SpeakAsync("Down " + string.Format("{0:n1}", Rate));
                            else
                                speech.SpeakAsync("Up " + string.Format("{0:n1}", Rate));

                        }
                       timer_announce.Interval = 5000;
                    } else {
                
                        timer_announce.Interval = SpeakInterval;

                        if (SpeakWhereCheckBox.Checked && (WhereDistance.Text != "?") && !FirstGPSCoordinates)
                               if (Convert.ToInt32(WhereDistance.Text) > 30)
                                   speech.SpeakAsync("Distance " + WhereDistance.Text + " Meters at " + WhereBearing.Text + " True.");
  
                        if (SpeakAltitudeCheckBox.Checked && (StateT == FlightStates.Flying))
                            speech.SpeakAsync("Altitude " + Convert.ToInt16(CurrAlt) + " Meters.");

                        if ((SpeakVoltsCheckBox.Checked) && (StateT == FlightStates.Flying))
                            SpeakBattery();
            }
                }
            }
            else
                if (SpeechEnableCheckBox.Checked && !TelemetryLostMessageUsed)
                {
                   //zzz speech.SpeakAsync("Telemetry Lost.");
                    TelemetryLostMessageUsed = true;
                }


            Properties.Settings.Default.SpeakVario = SpeakVarioCheckBox.Checked;
            Properties.Settings.Default.SpeakVolts = SpeakVoltsCheckBox.Checked;
            Properties.Settings.Default.SpeakAltitude = SpeakAltitudeCheckBox.Checked;
            Properties.Settings.Default.SpeakWhere = SpeakWhereCheckBox.Checked;
            Properties.Settings.Default.SpeakNav = SpeakNavCheckBox.Checked;

           
        } // timer_telemetry_Tick

        private void SpeakAlarms()
        {

            if (InFlight)
            {
                AlarmsButton.BackColor = Color.Orange;

                if (F[(byte)FlagValues.BadBusConfig] ||!F[(byte)FlagValues.ParametersValid]) {
                   if (F[(byte)FlagValues.BadBusConfig]) speech.SpeakAsync("Bus Device Table Order Incorrect.");
                if (!F[(byte)FlagValues.ParametersValid]) speech.SpeakAsync("In valid Parameters.");
            }
                else
                {
                    if (F[(byte)FlagValues.IsArmed]) speech.SpeakAsync("Arming Switch.");
                    if (DesiredThrottleT > 0) speech.SpeakAsync("Throttle open.");
                    if (!F[(byte)FlagValues.Signal]) speech.SpeakAsync("No signal.");

                    if (!F[(byte)FlagValues.OriginValid]) speech.SpeakAsync("Launch Location Not Acquired.");
                    if (RCChannel[4] >= 1200) speech.SpeakAsync("Navigate or return home selected.");

                    if (!F[(byte)FlagValues.IMUActive]) speech.SpeakAsync("IMU inactive.");
                    if (!F[(byte)FlagValues.MagnetometerActive]) speech.SpeakAsync("Magnetometer inactive or not installed.");
                    if (!F[(byte)FlagValues.BaroActive]) speech.SpeakAsync("Barometer inactive or not installed.");
                    if (!F[(byte)FlagValues.IMUCalibrated]) speech.SpeakAsync("IMU uncalibrated.");
                    if (F[(byte)FlagValues.MagnetometerActive] && !F[(byte)FlagValues.MagCalibrated])
                        speech.SpeakAsync("Magnetometer uncalibrated.");
                    if (F[(byte)FlagValues.LowBatt]) SpeakBattery();
                    if (((F[(byte)FlagValues.ReturnHome] || F[(byte)FlagValues.Navigate]) && !F[(byte)FlagValues.Bypass])) speech.SpeakAsync("Navigation enabled.");
                }
               

                AlarmsButton.BackColor = System.Drawing.SystemColors.Control;

            }
            else
            {
                speech.SpeakAsync("Not Connected.");
                AlarmsButton.BackColor = Color.Yellow;
            }

        }

        private void SpeakBattery()
        {
            double Volts = Convert.ToDouble(FormMain.BatteryVoltsT * 0.01);
            short VoltsPlace = (short)((Volts - (short)Volts) * 10.0);
            if ((Flags[0] & 0x20) != 0)
                speech.SpeakAsync("LOW Battery " + (short)Volts + "Point" + VoltsPlace + " Volts."); // . -> point
            else
                speech.SpeakAsync("Battery " + (short)Volts + "Point" + VoltsPlace + " Volts."); // . -> point

        } // SpeakBattery

       
        private void SpeakClimbDescend()
        {
             if (AltError < -5.0)
                 speech.SpeakAsync("Climbing to " + Convert.ToInt16(FormMain.DesiredAltitudeT * 0.01) + "Meters.");
             else
               if (AltError > 5.0)
                    speech.SpeakAsync("Descending to " + Convert.ToInt16(FormMain.DesiredAltitudeT * 0.01) + "Meters.");
        } // SpeakClimbDescend

        private void SpeechCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (SpeechEnableCheckBox.Checked)
            {
                speech.SpeakAsync("Speech Enabled");
                timer_announce.Interval = SpeakInterval = Convert.ToInt32(SpeakIntervalNumericUpDown.Value) * 1000;
            }

            Properties.Settings.Default.SpeechEnable = SpeechEnableCheckBox.Checked;

        } // SpeechCheckBox_CheckedChanged

        private void SpeakIntervalNumericUpDown_Click(object sender, EventArgs e)
        {
            short interval = Convert.ToInt16(SpeakIntervalNumericUpDown.Value);
            timer_announce.Interval = SpeakInterval = interval * 1000;
            Properties.Settings.Default.SpeakInterval = interval;

        } // SpeakIntervalNumericUpDown_Click


        private void SpeakNavStatus()
        {
            
            if (SpeechEnableCheckBox.Checked && SpeakNavCheckBox.Checked)
            {
                if (((PrevNavState != NavStateT) || (PrevWPT != CurrWPT)) && (StateT == FlightStates.Flying))
                {
                    switch (NavStateT)
                    {
                        case NavStates.HoldingStation: speech.SpeakAsync("Holding Position.");
                            break;
                        case NavStates.ReturningHome: speech.SpeakAsync("Return to home.");
                            //SpeakClimbDescend();
                            break;
                        case NavStates.AtHome: speech.SpeakAsync("Reached home.");
                            if (AutoLandEnabled)
                                speech.SpeakAsync("Landing in " + FormMain.NavStateTimeoutT / 1000 + " Seconds.");
                            break;
                        case NavStates.Descending: speech.SpeakAsync("Descending.");
                            break;
                        case NavStates.Touchdown: speech.SpeakAsync("Touchdown.");
                            break;
                        case NavStates.Transiting:
                            speech.SpeakAsync("Going to waypoint " + CurrWPT + ".");
                            //SpeakClimbDescend();
                            break;
                        case NavStates.Loitering: speech.SpeakAsync("Holding at waypoint" + CurrWPT + ".");
                            break;
                        case NavStates.Orbiting: speech.SpeakAsync("Orbiting waypoint" + CurrWPT + ".");
                            break;
                        case NavStates.Perching: speech.SpeakAsync("Perching.");
                            break;
                        case NavStates.Takeoff: speech.SpeakAsync("Takeoff.");
                            break;
                        case NavStates.UsingThermal: speech.SpeakAsync("Thermalling.");
                            break;
                        case NavStates.UsingRidge: speech.SpeakAsync("Ridge Soaring.");
                            break;
                        case NavStates.UsingWave: speech.SpeakAsync("Wave Soaring.");
                            break;
                            case NavStates.BoostClimb: speech.SpeakAsync("Boost Climb.");
                            break;
                            case NavStates.AltitudeLimiting: speech.SpeakAsync("Altitude Limiting.");
                            break;
                        case NavStates.JustGliding: speech.SpeakAsync("Gliding.");
                            break;
                        case NavStates.PIC: speech.SpeakAsync("Pilot in command.");
                            break;
                        case NavStates.RateControl: speech.SpeakAsync("Rate Mode");
                            break;
                        case NavStates.HorizonControl: speech.SpeakAsync("Horizon Mode");
                            break;
                        case NavStates.BypassControl: speech.SpeakAsync("Manual");
                            break;
                     //   case NavStates.AcquiringAltitude: speech.SpeakAsync("Acquiring altitude.");
                     //       SpeakClimbDescend();
                      //      break;
                        default:
                            break;
                    }
                    PrevNavState = NavStateT;
                    PrevWPT = FormMain.CurrWPT;

                }

            }
        } // SpeakNavStatus

        //-----------------------------------------------------------------------

        // UAVX Communications

        private void UAVXOpenTelemetry()
        {

            string sError = "Could not open telemetry link?";

            UAVXCloseTelemetry();

            SetSerialPort(UAVXGUI.Properties.Settings.Default.COMPort, UAVXGUI.Properties.Settings.Default.COMBaudRate, ref sError);
        
            try
                {
                    serialPort1.Open();
                    ConnectButton.Text = "Connected";
                    ConnectButton.BackColor = System.Drawing.Color.Green;
                    BootLoadButton.BackColor = System.Drawing.Color.Green;
                    CreateSaveLogFile();
                    RxHead = RxTail = 0;
                    InitPollPacket();
                    InFlight = true;
                }
            catch (Exception ex)
                {
                    ConnectButton.Text = "No COM Port";
                    ConnectButton.BackColor = System.Drawing.Color.Orange;
                    InFlight = false;
                }
 
        }

        int zzz = 0;

        private void ReadReplayLogFile()
        {
            byte b;
           

            while ( OpenLogFileStream.Position < OpenLogFileStream.Length )
            {
                RxTail++;
                RxTail &= RxQueueMask;
                b = OpenLogFileBinaryReader.ReadByte();
                RxQueue[RxTail] = b;

                ReadingTelemetry = true;
                this.Invoke(new EventHandler(UAVXReadTelemetry));
                while (ReadingTelemetry) { };

                ReplayProgress = (long) (100 * OpenLogFileStream.Position) / OpenLogFileStream.Length;
                if (++zzz > 10)
                {
                    Thread.Sleep(ReplayDelay);
                    zzz = 0;
                }
            }

            if (OpenLogFileStream.Position == OpenLogFileStream.Length)
            {
                DoingLogfileReplay = false;
                CloseReplayLogFile();
            }
             //this.Invoke(new EventHandler(NowCloseReplay)); //now close back in the main thread
        }

        private void NowCloseReplay(object sender, EventArgs e)
        {
            this.Close(); 
        }

        private void serialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            byte b;
            short NewRxTail;

            if (serialPort1.IsOpen) // flakey serial drivers hence heroics
            {
                while ( serialPort1.IsOpen && (serialPort1.BytesToRead != 0))
                {
                        NewRxTail = RxTail;
                        NewRxTail++;
                        NewRxTail &= RxQueueMask;
                        if (NewRxTail != RxHead)
                        {
                            RxTail = NewRxTail;
                            b = (byte)serialPort1.ReadByte();
                            RxQueue[RxTail] = b;
                            SaveLogFileBinaryWriter.Write(b);
                        }
                        else
                        {
                            b = (byte)serialPort1.ReadByte(); // processing overflow - discard
                        }
                }
               if (serialPort1.IsOpen) 
                    this.Invoke(new EventHandler(UAVXReadTelemetry));
            }
        }

        void InitPollPacket()
        {
            RxPacketByteCount = 0;
            RxCheckSum = 0;
            CheckSumError = false;

            RxPacketTag = UnknownPacketTag;

            RxPacketLength = 2; // set as minimum
            PacketRxState = RxStates.WaitRxSentinel;
        } // InitPollPacket

        void AddToBuffer(byte ch)
        {
            bool RxPacketError;

            UAVXPacket[RxPacketByteCount++] = ch;
            if (RxPacketByteCount == 1)
            {
                RxPacketTag = ch;
                PacketRxState = RxStates.WaitRxBody;
            }
            else
                if (RxPacketByteCount == 2)
                {
                    RxPacketLength = ch;
                    PacketRxState = RxStates.WaitRxBody;
                }
                else
                    if (RxPacketByteCount >= (RxPacketLength + 3))
                    {
                        RxPacketError = CheckSumError = !((RxCheckSum == 0) || (RxCheckSum == ESC));

                        if (CheckSumError)
                            RxCheckSumErrors++;

                        if (!RxPacketError)
                        {
                            PacketReceived = true;
                            ReceivedPacketTag = RxPacketTag;
                        }
                        PacketRxState = RxStates.WaitRxSentinel;
                    }
                    else
                        PacketRxState = RxStates.WaitRxBody;
        } // AddToBuffer

  

        void ParsePacket(byte ch)
        {
            RxCheckSum ^= ch;
            switch (PacketRxState)
            {
                case RxStates.WaitRxSentinel:
                    if (ch == SOH)
                    {
                        InitPollPacket();
                        PacketRxState = RxStates.WaitRxBody;
                    }
                    break;
                case RxStates.WaitRxBody:
                    if (ch == ESC)
                        PacketRxState = RxStates.WaitRxESC;
                    else
                        if (ch == SOH) // unexpected start of packet
                        {
                            RxLengthErrors++;

                            InitPollPacket();
                            PacketRxState = RxStates.WaitRxBody;
                        }
                        else
                            if (ch == EOT) // unexpected end of packet 
                            {
                                RxLengthErrors++;
                                PacketRxState = RxStates.WaitRxSentinel;
                            }
                            else
                                AddToBuffer(ch);
                    break;
                case RxStates.WaitRxESC:
                    AddToBuffer(ch);
                    break;
                default:
                    PacketRxState = RxStates.WaitRxSentinel;
                    break;
            }
        } // ParsePacket


        void ExtractFlags()
        {
            int bb, i, cf;

            for (i = 0; i < 6; i++)
            {
                cf = Flags[i];
                for (bb = 7; bb >= 0; bb--)
                    F[i * 8 + bb] = ((cf >> bb) & 1) != 0;
            }
        } // ExtractFlags

        void UpdateFlags()
        {

            ExtractFlags();

            UsingFixedWing = F[(byte)FlagValues.IsFixedWing];

            NoUplink = !F[(byte)FlagValues.UsingUplink];

            AltHoldBox.BackColor = F[(byte)FlagValues.AltControlEnabled] ?
                System.Drawing.Color.Green : System.Drawing.Color.Red;

            GPSAltitudeBox.BackColor = F[(byte)FlagValues.UsingGPSAlt] ?
                System.Drawing.Color.Green : FlagsGroupBox.BackColor;

            TurnToPOIBox.BackColor = F[(byte)FlagValues.UsingPOI] ?
                System.Drawing.Color.Green : FlagsGroupBox.BackColor;

            VRSHazardDetected = (F[(byte)FlagValues.RapidDescentHazard]);

            LandingSwitchBox.BackColor = F[(byte)FlagValues.LandingSwitch] ? 
                System.Drawing.Color.Red : FlagsGroupBox.BackColor;

            NearLevelBox.BackColor = F[(byte)FlagValues.NearLevel] ? 
                System.Drawing.Color.Green : System.Drawing.Color.LightSteelBlue;

            if (F[(byte)FlagValues.LowBatt])
            {
                LowBatteryBox.BackColor = System.Drawing.Color.Red;
                BatteryVolts.BackColor = System.Drawing.Color.Red;
            }
            else
            {
                LowBatteryBox.BackColor = FlagsGroupBox.BackColor;
                BatteryVolts.BackColor = BatteryGroupBox.BackColor;
            }
            GPSValidBox.BackColor = F[(byte)FlagValues.GPSValid] ?
                System.Drawing.Color.Green : System.Drawing.Color.Red;

            NavValidBox.BackColor = F[(byte)FlagValues.OriginValid] ?
                System.Drawing.Color.Green : System.Drawing.Color.Red;

            //if (UAVXArm)
           // GPSROC.Visible = false;
            //else
            //    GPSROC.Visible = true;

            BaroFailBox.BackColor = F[(byte)FlagValues.BaroFailure] ?
                System.Drawing.Color.Red : FlagsGroupBox.BackColor;

            IMUFailBox.BackColor = F[(byte)FlagValues.IMUFailure] || !F[(byte)FlagValues.IMUCalibrated] ?
                System.Drawing.Color.Red : System.Drawing.SystemColors.Control;

            GPSFailBox.BackColor = F[(byte)FlagValues.GPSFailure] ?
                System.Drawing.Color.Red : FlagsGroupBox.BackColor;

            AttitudeHoldBox.BackColor = F[(byte)FlagValues.AttitudeHold] ?
                System.Drawing.Color.Green : System.Drawing.Color.LightSteelBlue;

            ThrottleMovingBox.BackColor = F[(byte)FlagValues.ThrottleMoving] ?
                System.Drawing.Color.LightSteelBlue : FlagsGroupBox.BackColor;

            HoldingAltBox.BackColor = F[(byte)FlagValues.HoldingAlt] ?
                System.Drawing.Color.Green : FlagsGroupBox.BackColor;

            NavigateBox.BackColor = F[(byte)FlagValues.Navigate] ?
                System.Drawing.Color.Green : FlagsGroupBox.BackColor;

            ProximityBox.BackColor = F[(byte)FlagValues.WayPointAchieved] ?
                 System.Drawing.Color.Green : FlagsGroupBox.BackColor;

            CloseProximityBox.BackColor = F[(byte)FlagValues.WayPointCentred] ?
                System.Drawing.Color.Green : FlagsGroupBox.BackColor;

            //0x08

            UseRTHAutoDescendBox.BackColor = F[(byte)FlagValues.UsingRTHAutoDescend] ?
                System.Drawing.Color.Green : FlagsGroupBox.BackColor;

            BaroAltValidBox.BackColor = F[(byte)FlagValues.BaroActive] ?
                 System.Drawing.Color.Green : FlagsGroupBox.BackColor;

            if (F[(byte)FlagValues.RangefinderActive]) 
            {
                RangefinderAltValidBox.BackColor = System.Drawing.Color.Green;
                RangefinderAltitude.BackColor = AltitudeGroupBox.BackColor;
            }
            else
            {
                RangefinderAltValidBox.BackColor = FlagsGroupBox.BackColor;
                RangefinderAltitude.BackColor = System.Drawing.Color.Orange;
            }
            UsingRangefinderBox.BackColor = F[(byte)FlagValues.RangefinderActive] ?
                System.Drawing.Color.Green : FlagsGroupBox.BackColor;

            // 0x01

            BypassBox.BackColor = F[(byte)FlagValues.Bypass] ?
                System.Drawing.Color.Orange : FlagsGroupBox.BackColor;

            WarningPictureBox.Visible = F[(byte)FlagValues.IsArmed];

            AngleControlBox.BackColor = F[(byte)FlagValues.UsingAngleControl] ?
                System.Drawing.Color.Green : System.Drawing.Color.Orange;

            EmulationTextBox.Text = F[(byte)FlagValues.Emulation] ? 
                "EMULATION" : " ";

                RollAngle.BackColor = AttitudeGroupBox.BackColor;
                PitchAngle.BackColor = AttitudeGroupBox.BackColor;

            if (F[(byte)FlagValues.IMUCalibrated]) {
                IMUFailBox.BackColor = System.Drawing.SystemColors.Control;
                CalibrateIMUButton.BackColor = System.Drawing.Color.Green;
                CalibrateIMUEnabled = false;
            }
            else {
                CalibrateIMUButton.BackColor = (CalibrateIMUEnabled) ?
                    Color.Orange : Color.Red;
                IMUFailBox.BackColor = System.Drawing.Color.Orange;
            }

            if (F[(byte)FlagValues.AccCalibrated]) {
                IMUFailBox.BackColor = System.Drawing.SystemColors.Control;
                CalibrateAcc6PointButton.BackColor = System.Drawing.Color.Green;
                CalibrateAcc6PointEnabled = false;
            }
            else {
                CalibrateAcc6PointButton.BackColor = (CalibrateAcc6PointEnabled) ?
                    Color.Orange : Color.Orange;
                IMUFailBox.BackColor = System.Drawing.Color.Orange;
            }

 
            if (F[(byte)FlagValues.MagCalibrated])
            {
                MagFailBox.BackColor = System.Drawing.SystemColors.Control;
                CalibrateMagButton.BackColor = System.Drawing.Color.Green;
                CalibrateMagEnabled = false;
            }
            else
            {
                CalibrateMagButton.BackColor = (CalibrateMagEnabled) ?
                    Color.Orange : Color.Red;
                MagFailBox.BackColor = System.Drawing.Color.Orange;
            }

  
            BadBusConfigBox.BackColor = F[(byte)FlagValues.BadBusConfig] ?
                System.Drawing.Color.Red: Color.Green;     

            DumpBBButton.BackColor = F[(byte)FlagValues.DumpingBB] ?
                 Color.Orange : System.Drawing.SystemColors.Control;

           UsingUplinkFlagBox.BackColor = F[(byte)FlagValues.UsingUplink] ?
               System.Drawing.Color.Green : System.Drawing.Color.Orange;

            DrivesGroupBox.BackColor = F[(byte)FlagValues.Saturation] ?
               System.Drawing.Color.Orange : System.Drawing.SystemColors.Control;
            
        }

        void DoOrientation()
        {
         //  do it all of the time in case some one changes orientation but does not shutdown GS if (!DoneOrientation)
            {
              //  FlightHeadingOffset = F[(byte)FlagValues.Emulation] ?
              //      0.0 : (OrientT * Math.PI) / 24.0;

             //   OSO = Math.Sin(FlightHeadingOffset);
              //  OCO = Math.Cos(FlightHeadingOffset);
            }
        }

        void UpdateFlightState()
        {
           
            switch (StateT)
            {
                case FlightStates.Starting: FlightState.Text = "Starting";
                    FlightState.BackColor = System.Drawing.Color.LightSteelBlue;
                    break;
                case FlightStates.Warmup: FlightState.Text = "Warmup";
                    FlightState.BackColor = System.Drawing.Color.Lime;
                    break;
                case FlightStates.Landing: FlightState.Text = "Landing";
                    FlightState.BackColor = System.Drawing.Color.Green;
                    break;
                case FlightStates.Landed: FlightState.Text = "Landed";
                    FlightState.BackColor = System.Drawing.Color.RosyBrown;
                    break;
                case FlightStates.Shutdown: FlightState.Text = "Shutdown!";
                    FlightState.BackColor = System.Drawing.Color.Orange;
                    break;
                case FlightStates.Flying: FlightState.Text = "Flying";
                    FlightState.BackColor = System.Drawing.Color.Silver;
                    break;
                case FlightStates.IREmulate: FlightState.Text = "IREmulate";
                    FlightState.BackColor = System.Drawing.Color.RosyBrown;
                    break;
                case FlightStates.Preflight: FlightState.Text = "Preflight";
                    FlightState.BackColor = System.Drawing.Color.Red;
                    break;
                case FlightStates.Ready: FlightState.Text = "Ready";
                    FlightState.BackColor = System.Drawing.Color.Gold;
                    break;
                case FlightStates.Launching: FlightState.Text = "Launching";
                    FlightState.BackColor = System.Drawing.Color.Orange;
                    break;
                default: FlightState.Text = "Unknown"; break;
            } // switch
        }

        void UpdateNavState()
        {
            switch (NavStateT)
            {
                case NavStates.HoldingStation: NavState.Text = "Holding";
                    NavState.BackColor = System.Drawing.Color.LightSteelBlue;
                    break;
                case NavStates.ReturningHome: NavState.Text = "Returning";
                    NavState.BackColor = System.Drawing.Color.Lime;
                    break;
                case NavStates.AtHome: NavState.Text = "@Home";
                    NavState.BackColor = System.Drawing.Color.Green;
                    break;
                case NavStates.Descending: NavState.Text = "Descending";
                    NavState.BackColor = System.Drawing.Color.Orange;
                    break;
                case NavStates.Touchdown: NavState.Text = "Touchdown!";
                    NavState.BackColor = System.Drawing.Color.RosyBrown;
                    break;
                case NavStates.Transiting: NavState.Text = "Transiting";
                    NavState.BackColor = System.Drawing.Color.Silver;
                    break;
                case NavStates.Loitering: NavState.Text = "Loitering";
                    NavState.BackColor = System.Drawing.Color.Gold;
                    break;
                case NavStates.Orbiting: NavState.Text = "Orbiting";
                    NavState.BackColor = System.Drawing.Color.Gold;
                    break;
                case NavStates.Perching: NavState.Text = "Perching";
                    NavState.BackColor = System.Drawing.Color.RosyBrown;
                    break;
                case NavStates.Takeoff: NavState.Text = "Takeoff";
                    NavState.BackColor = System.Drawing.Color.Red;
                    break;
                case NavStates.PIC: NavState.Text = "PIC";
                    NavState.BackColor = System.Drawing.Color.Orange;
                    break;
                case NavStates.AcquiringAltitude: NavState.Text = "Acquire Alt";
                    NavState.BackColor = System.Drawing.Color.Orange;
                    break;
                case NavStates.UsingThermal: NavState.Text = "Thermal";
                    NavState.BackColor = System.Drawing.Color.Gold;
                    break;
                case NavStates.UsingRidge: NavState.Text = "Ridge";
                    NavState.BackColor = System.Drawing.Color.Gold;
                    break;
                case NavStates.UsingWave: NavState.Text = "Wave";
                    NavState.BackColor = System.Drawing.Color.Gold;
                    break;
                case NavStates.BoostClimb: NavState.Text = "Boost";
                    NavState.BackColor = System.Drawing.Color.Gold;
                    break;
                case NavStates.AltitudeLimiting: NavState.Text = "Alt Lim";
                    NavState.BackColor = System.Drawing.Color.Gold;
                    break;
                case NavStates.JustGliding: NavState.Text = "Gliding";
                    NavState.BackColor = System.Drawing.Color.Gold;
                    break;
                case NavStates.RateControl:
                    NavState.Text = "RATE";
                    NavState.BackColor = System.Drawing.Color.Orange;
                    break;
                case NavStates.BypassControl:
                    NavState.Text = "BYPASS";
                    NavState.BackColor = System.Drawing.Color.Red;
                    break;
                case NavStates.HorizonControl:
                    NavState.Text = "HORIZON";
                    NavState.BackColor = System.Drawing.Color.Yellow;
                    break;
                default: NavState.Text = "Unknown"; break;
            } // switch
        }

        void UpdateAlarmState()
        {    

	// NoFailsafes, Monitoring, BatteryLow, LostSignal, HitFencRTH

            switch (AlarmStateT)
            {
                case 0: AlarmState.Text = "No Failsafes";
                    AlarmState.BackColor = System.Drawing.Color.Green;
                    break;
                case 1: AlarmState.Text = "Monitoring";
                    AlarmState.BackColor = System.Drawing.Color.Green;
                    break;
                case 2: AlarmState.Text = "Battery Low";
                    AlarmState.BackColor = System.Drawing.Color.RosyBrown;
                    break;
                case 3: AlarmState.Text = "Lost Signal";
                    AlarmState.BackColor = System.Drawing.Color.Red;
                    break;
                case 4: AlarmState.Text = "Fence RTH";
                    AlarmState.BackColor = System.Drawing.Color.Red;
                    break;
                case 5: AlarmState.Text = "Upside down";
                    AlarmState.BackColor = System.Drawing.Color.Red;
                    break;
                case 6: AlarmState.Text = "Forced landing";
                    AlarmState.BackColor = System.Drawing.Color.Orange;
                    break;
                case 7: AlarmState.Text = "GPS bypass";
                    AlarmState.BackColor = System.Drawing.Color.Blue;
                    break;
                case 8: AlarmState.Text = "Arming timeout";
                    AlarmState.BackColor = System.Drawing.Color.Yellow;
                    break;
                default: AlarmState.Text = "Unknown"; break;
            } // switch
        }

        void UpdateAttitude()
        {
                RollRate.Text = string.Format("{0:n2}", RateT[Roll] * MILLIRADDEG);
                PitchRate.Text = string.Format("{0:n2}", RateT[Pitch] * MILLIRADDEG);
                YawGyro.Text = string.Format("{0:n2}", RateT[Yaw] * MILLIRADDEG);
                RollAngle.Text = string.Format("{0:n1}", AngleT[Roll] * MILLIRADDEG);
                PitchAngle.Text = string.Format("{0:n1}", AngleT[Pitch] * MILLIRADDEG);
                YawAngle.Text = string.Format("{0:n1}", AngleT[Yaw] * MILLIRADDEG);
        }

        void UpdateAccelerations()
        {
                LRAcc.Text = string.Format("{0:n2}", (float)AccT[LR] * 0.001);
                FBAcc.Text = string.Format("{0:n2}", (float)AccT[FB] * 0.001);
                DUAcc.Text = string.Format("{0:n2}", (float)AccT[DU] * 0.001);

                FBAccLabel.Text = "FB";
                DUAccLabel.Text = "DU";
        }

        void UpdateCompensation()
        {
            AccConfidence.Text = string.Format("{0:n0}", AccConfidenceT);
            TiltFFComp.Text = string.Format("{0:n2}", TiltFFCompT * 0.001);
            BattFFComp.Text = string.Format("{0:n2}", BattFFCompT * 0.001);
            AltComp.Text = string.Format("{0:n1}", AltCompT * 0.1);
            CruiseThrottle.Text = string.Format("{0:n1}", CruiseThrottleT * 0.1);

            if (AccConfidenceT < 33 )
                AccConfidence.BackColor = System.Drawing.Color.Red;
            else
                if (AccConfidenceT <67)
                    AccConfidence.BackColor = System.Drawing.Color.Orange;
                else
                    AccConfidence.BackColor = System.Drawing.Color.Green;

        }

        void UpdateBattery()
        {
                BatteryVolts.Text = string.Format("{0:n2}", (float)BatteryVoltsT * 0.01);
                BatteryCurrent.Text = string.Format("{0:n1}", (float)BatteryCurrentT * 0.01);
                BatteryCharge.Text = string.Format("{0:n0}", (float)BatteryChargeT);
        }

        void UpdateWhere(byte i)
        {
            double Distance, Direction, Elevation;
                short Guidance;

          //  if (F[(byte)FlagValues.OriginValid]) {

                Distance = ExtractShort(ref UAVXPacket, (byte)(i));
                Direction = ExtractShort(ref UAVXPacket, (byte)(i+2));
                Elevation = ExtractShort(ref UAVXPacket, (byte)(i+4));
                Guidance = ExtractShort(ref UAVXPacket, (byte)(i+5));

                LongitudeCorrection = Math.Abs(Math.Cos((Math.PI / 180.0) * (double)GPSLatitudeT * 1e-7));

                WhereBearing.BackColor = NavGroupBox.BackColor;
                WhereDistance.BackColor = NavGroupBox.BackColor;

               while (Direction < 0.0)
                   Direction += 360.0;
                WhereBearing.Text = string.Format("{0:n0}", Direction);
               if (Distance > 10000.0) 
                    Distance = 10000.0;
                WhereDistance.Text = string.Format("{0:n0}", Distance);
               if (Distance > MaximumFenceRadius)
                    WhereDistance.BackColor = System.Drawing.Color.Orange;
     /*       }
            else
            {
                WhereBearing.BackColor = System.Drawing.Color.Orange;
                WhereDistance.BackColor = System.Drawing.Color.Orange;

                WhereBearing.Text = "?";
                WhereDistance.Text = "?";
            }
     */
        }

        void UpdateControls()
        {
            DesiredThrottle.Text = string.Format("{0:n1}", (float)(DesiredThrottleT * 0.1));
            //DesiredThrottle.Text = string.Format("{0:n0}", ((float)(RCChannel[0] - 1000) * 100.0) / RCMaximum);
                DesiredRoll.Text = string.Format("{0:n0}", ((float)(RCChannel[1] - 1500) * 200.0 - 150.0) / RCMaximum);
                DesiredPitch.Text = string.Format("{0:n0}", ((float)(RCChannel[2] - 1500) * 200.0 - 150.0) / RCMaximum);
                DesiredYaw.Text = string.Format("{0:n0}", ((float)(RCChannel[3] - 1500) * 200.0 - 150.0) / RCMaximum);
        }

      
        void GetAttitude(short p) {
            short a, i;

            DesiredThrottleT = ExtractShort(ref UAVXPacket, p);

            for (a = Pitch; a <=Yaw; a++) {
              i =  Convert.ToInt16(p + 2 + a * 10);
               DesiredAngleT[a] = ExtractShort(ref UAVXPacket, i);
               AngleT[a] = ExtractShort(ref UAVXPacket, (byte)(i + 2));
               AccT[a] = ExtractShort(ref UAVXPacket, (byte)(i + 4));
               DesiredRateT[a] = ExtractShort(ref UAVXPacket, (byte)(i + 6));
               RateT[a] = ExtractShort(ref UAVXPacket, (byte)(i + 8));
            }
        }

        void DoMotorsAndTime(short p)
        {
            short m;
              short v;

            for (m = 0; m < 10; m++)
                    PWMDiagT[m] = PWMT[m] = 0;

               MotorsAndServos = ExtractByte(ref UAVXPacket, (byte)(p));
               m = p;
                for (m = 0; m < MotorsAndServos; m++)
                    PWMT[m] = ExtractShort(ref UAVXPacket, (byte)(p + 1 + m * 2));

                for (m = 0; m < MotorsAndServos; m++)
                {
                    v = Convert.ToInt16(ExtractByte(ref UAVXPacket, (byte)(p + 1 + MotorsAndServos * 2 + m)));
                    PWMDiagT[m] = Convert.ToInt16(v > 127 ? v - 256 : v);
                }
                MissionTimeMilliSecT = ExtractInt24(ref UAVXPacket, (byte)(p + 1 + MotorsAndServos * 3));   
        }


        void UpdateGPS() {

                    GPSNoOfSats.Text = string.Format("{0:n0}", GPSNoOfSatsT);
                    GPSNoOfSats.BackColor = GPSNoOfSatsT < 6 ?
                        System.Drawing.Color.Orange : GPSStatBox.BackColor;

                    GPSFix.Text = string.Format("{0:n0}", GPSFixT);
                    GPSFix.BackColor = GPSFixT < 2 ?
                        System.Drawing.Color.Orange : GPSStatBox.BackColor;

                    GPShAcc.Text = string.Format("{0:n2}", (float)GPShAccT * 0.01);
                    if (GPShAccT > 600)
                        GPShAcc.BackColor = System.Drawing.Color.Red;
                    else
                        GPShAcc.BackColor = GPShAccT > 300 ?
                            System.Drawing.Color.Orange : GPSStatBox.BackColor;

                    GPSvAcc.Text = string.Format("{0:n2}", (float)GPSvAccT * 0.01);
                    if (GPSvAccT > 600)
                        GPSvAcc.BackColor = System.Drawing.Color.Red;
                    else
                        GPSvAcc.BackColor = GPSvAccT > 300 ?
                            System.Drawing.Color.Orange : GPSStatBox.BackColor;

                    GPSsAcc.Text = string.Format("{0:n2}", (float)GPSsAccT * 0.01);
                    if (GPSsAccT > 100)
                        GPSsAcc.BackColor = System.Drawing.Color.Red;
                    else
                        GPSsAcc.BackColor = GPSsAccT > 50 ?
                            System.Drawing.Color.Orange : GPSStatBox.BackColor;           

                    GPScAcc.Text = string.Format("{0:n2}", (float)GPScAccT * 0.01);
                    if (GPScAccT > 100)
                        GPScAcc.BackColor = System.Drawing.Color.Red;
                    else
                        GPScAcc.BackColor = GPScAccT > 50 ?
                            System.Drawing.Color.Orange : GPSStatBox.BackColor;

                    CurrWP.Text = string.Format("{0:n0}", CurrWPT);

                    GPSVel.Text = string.Format("{0:n1}", (double)GPSVelT * 0.1); // M/Sec
                    //GPSROC.Text = string.Format("{0:n1}", (float)GPSROCT * 0.01);
                    GPSHeading.Text = string.Format("{0:n0}", (float)GPSHeadingT * MILLIRADDEG);
                    GPSAltitude.Text = string.Format("{0:n1}", (double)GPSAltitudeT * 0.01);
                    GPSLongitude.Text = string.Format("{0:n6}", (double)GPSLongitudeT * 1e-7);
                    GPSLatitude.Text = string.Format("{0:n6}", (double)GPSLatitudeT * 1e-7);

                    if (GPSFixT >= 3) // GPSValid
                    {
                        GPSVel.BackColor = NavGroupBox.BackColor;
                       // GPSROC.BackColor = NavGroupBox.BackColor;
                        GPSHeading.BackColor = NavGroupBox.BackColor;
                        GPSAltitude.BackColor = NavGroupBox.BackColor;
                        GPSLongitude.BackColor = NavGroupBox.BackColor;
                        GPSLatitude.BackColor = NavGroupBox.BackColor;
                        WayHeading.BackColor = NavGroupBox.BackColor;
                        DistanceToDesired.BackColor = NavGroupBox.BackColor;

                        WayHeading.Text = string.Format("{0:n0}", (float)WPBearingT * MILLIRADDEG);
                        CrossTrackError.Text = string.Format("{0:n1}", (float)CrossTrackET * 0.1);

                        NorthDiff = (double)NorthPosET * 0.1; // scale up to decimetres after conversion
                        EastDiff = (double)EastPosET * 0.1;

                        Distance = Math.Sqrt(NorthDiff * NorthDiff + EastDiff * EastDiff);
                        DistanceToDesired.Text = string.Format("{0:n1}", Distance);
                
                     }
                    else
                    {
                        GPSVel.BackColor = System.Drawing.Color.Orange;
                       // GPSROC.BackColor = System.Drawing.Color.Orange;
                        GPSHeading.BackColor = System.Drawing.Color.Orange;
                        GPSAltitude.BackColor = System.Drawing.Color.Orange;
                        GPSLongitude.BackColor = System.Drawing.Color.Orange;
                        GPSLatitude.BackColor = System.Drawing.Color.Orange;
                        WayHeading.BackColor = System.Drawing.Color.Orange;
                        DistanceToDesired.BackColor = System.Drawing.Color.Orange;

                        WayHeading.Text = "?";
                        DistanceToDesired.Text = "?";
                    }
                   
        }

        int Limit(int v, int Min, int Max)
        {
            if (v < Min) return (Min); else if (v > Max) return Max; else return v;
        } // Limit

        Color PWDiag(short PWDiag)
        {
            return (PWDiag > 3  ? Color.Orange : PWDiag > 5  ? Color.Red : Color.White);
        }

        void UpdateMotors()
        {

                PWMT0ProgressBar.Value = Limit(PWMT[0], 0, 1000);
                PWMT1ProgressBar.Value = Limit(PWMT[1], 0, 1000);
                PWMT2ProgressBar.Value = Limit(PWMT[2], 0, 1000);
                PWMT3ProgressBar.Value = Limit(PWMT[3], 0, 1000);

                PWMT4ProgressBar.Value = Limit(PWMT[4], 0, 1000);
                PWMT5ProgressBar.Value = Limit(PWMT[5], 0, 1000);

                if (DiagnosticCheckBox.Checked) {
                    PWMT0.Text = string.Format("{0:n0}", PWMDiagT[0]);
                    PWMT1.Text = string.Format("{0:n0}", PWMDiagT[1]);
                    PWMT2.Text = string.Format("{0:n0}", PWMDiagT[2]);
                    PWMT3.Text = string.Format("{0:n0}", PWMDiagT[3]);
                    PWMT4.Text = string.Format("{0:n0}", PWMDiagT[4]);
                    PWMT5.Text = string.Format("{0:n0}", PWMDiagT[5]);
             }
                else
                {
                    PWMT0.Text = string.Format("{0:n0}", PWMT[0] * 0.1);
                    PWMT1.Text = string.Format("{0:n0}", PWMT[1] * 0.1);
                    PWMT2.Text = string.Format("{0:n0}", PWMT[2] * 0.1);
                    PWMT3.Text = string.Format("{0:n0}", PWMT[3] * 0.1);
                    PWMT4.Text = string.Format("{0:n0}", PWMT[4] * 0.1);
                    PWMT5.Text = string.Format("{0:n0}", PWMT[5] * 0.1);
                }

                PWMT0ProgressBar.BackColor = PWDiag(PWMDiagT[0]);
                PWMT1ProgressBar.BackColor = PWDiag(PWMDiagT[1]);
                PWMT2ProgressBar.BackColor = PWDiag(PWMDiagT[2]);
                PWMT3ProgressBar.BackColor = PWDiag(PWMDiagT[3]);
                PWMT4ProgressBar.BackColor = PWDiag(PWMDiagT[4]);
                PWMT5ProgressBar.BackColor = PWDiag(PWMDiagT[5]);

  

                if (MotorsAndServos > 6)
                {
                    PWMT6ProgressBar.Value = Limit(PWMT[6], 0, 1000);
                    PWMT7ProgressBar.Value = Limit(PWMT[7], 0, 1000);

                    PWMT8ProgressBar.Value = Limit(PWMT[8], 0, 1000);
                    PWMT9ProgressBar.Value = Limit(PWMT[9], 0, 1000);

                    if (DiagnosticCheckBox.Checked)
                    {
                        PWMT6.Text = string.Format("{0:n0}", PWMDiagT[6]);
                        PWMT7.Text = string.Format("{0:n0}", PWMDiagT[7]);
                        PWMT8.Text = string.Format("{0:n0}", PWMDiagT[8]);
                        PWMT9.Text = string.Format("{0:n0}", PWMDiagT[9]);
                    }
                    else
                    {
                        PWMT6.Text = string.Format("{0:n0}", PWMT[6] * 0.1);
                        PWMT7.Text = string.Format("{0:n0}", PWMT[7] * 0.1);

                        PWMT8.Text = string.Format("{0:n0}", PWMT[8] * 0.1);
                        PWMT9.Text = string.Format("{0:n0}", PWMT[9] * 0.1);
                    }

                    PWMT6ProgressBar.BackColor = PWDiag(PWMDiagT[6]);
                    PWMT7ProgressBar.BackColor = PWDiag(PWMDiagT[7]);
                    PWMT8ProgressBar.BackColor = PWDiag(PWMDiagT[8]);
                    PWMT9ProgressBar.BackColor = PWDiag(PWMDiagT[9]);

                    PWMT6.Visible = PWMT7.Visible = PWMT8.Visible =  PWMT9.Visible =
                    PWMT6ProgressBar.Visible =  PWMT7ProgressBar.Visible = 
                    PWMT8ProgressBar.Visible = PWMT9ProgressBar.Visible =  true;

                }
                else
                    PWMT6.Visible = PWMT7.Visible = PWMT8.Visible = PWMT9.Visible =
                    PWMT6ProgressBar.Visible = PWMT7ProgressBar.Visible = 
                    PWMT8ProgressBar.Visible = PWMT9ProgressBar.Visible = false;

        }

        void UpdateAltitude()
        {
            ROC.Text = string.Format("{0:n1}", (float)ROCT * 0.01);
            BaroAltitude.Text = string.Format("{0:n1}", (float)BaroAltitudeT * 0.01);
            BaroAltitude.BackColor =  (AltitudeT * 0.01) > MaximumAltitudeLimit ?
                System.Drawing.Color.Orange : AltitudeGroupBox.BackColor;

            if ((ROCT * 0.01) < -2) 
                ROC.BackColor = System.Drawing.Color.Red;
            else
                ROC.BackColor = (ROCT * 0.01) < -1 ?
                    System.Drawing.Color.Orange : AltitudeGroupBox.BackColor;

            RangefinderAltitude.Text = string.Format("{0:n2}", (float)RangefinderAltitudeT * 0.01);
             if (F[(byte)FlagValues.UsingRangefinderAlt]) 
                AltitudeSource.Text = "Rangefinder";
            else if (F[(byte)FlagValues.UsingGPSAlt])
                 AltitudeSource.Text =  "GPS"; 
            else
            AltitudeSource.Text =  "Barometer";
            CurrAlt = AltitudeT * 0.01;

            if (AccConfidenceT >= 67)
            {
                LRAcc.BackColor = AccelerationsGroupBox.BackColor;
                FBAcc.BackColor = AccelerationsGroupBox.BackColor;
                DUAcc.BackColor = AccelerationsGroupBox.BackColor;
            }
            else 
            {
                LRAcc.BackColor = System.Drawing.Color.Orange;
                FBAcc.BackColor = System.Drawing.Color.Orange;
                DUAcc.BackColor = System.Drawing.Color.Orange;
            }     

            CurrentAltitude.Text = string.Format("{0:n1}", (float)CurrAlt);
            if (CurrAlt > MaximumAltitudeLimit)
                CurrentAltitude.BackColor = System.Drawing.Color.Orange;
            else
                CurrentAltitude.BackColor = NavGroupBox.BackColor;

            AltError = CurrAlt - DesiredAltitudeT * 0.01;
            AltitudeError.Text = string.Format("{0:n1}", (float)AltError);
        }



        void ProcessPacket()
        {
            byte b, c;
            short i;

            PacketReceived = false;
            TelemetryActive = true;

            if (DoingLogfileReplay)
                ReplayProgressBar.Value = (int)ReplayProgress;

                switch (RxPacketTag)
                {
                    case UAVXAckPacketTag:
                        if (UAVXPacket[2] == (byte)MiscComms.miscLB)
                           RxLoopbackEnabled = UAVXPacket[3] != 0;
                     //   else
                          if (UAVXPacket[3] == 0)
                             speech.SpeakAsync("Failed ");
                       //   else
                       //       speech.SpeakAsync("OK ");
                        break;

                    case UAVXParamPacketTag:
                        int p;

                        ParameterForm.CurrPS = 0; // UAVXPacket[2];
                        for (p = 0; p < MAX_PARAMS; p++) {
                            ParameterForm.UAVXP[p].Value = UAVXPacket[p + 3];
                            ParameterForm.UAVXP[p].Changed = true;
                        }

                        VersionLenT = ExtractByte(ref UAVXPacket, (byte)(MAX_PARAMS + 3));
                        VersionNameT = "UAVXArm32F4.";
                        for (b = 0; b < VersionLenT; b++ )
                            VersionNameT += (char)(ExtractByte(ref UAVXPacket, (byte)(MAX_PARAMS + 4 + b)));
                        MainLabel.Text = VersionNameT;

                        //zzz speech.SpeakAsync("Received " + ParameterForm.CurrPS);

                        ParameterForm.UpdateParamForm = true;

                        break;

                    case UAVXWPPacketTag:
	                    int wp;

	                    wp = UAVXPacket[2];

	                    WP[wp].LatitudeRaw = ExtractInt(ref UAVXPacket, 3);
                        WP[wp].LongitudeRaw = ExtractInt(ref UAVXPacket, 7);
                        WP[wp].Altitude = ExtractShort(ref UAVXPacket, 11);
                        WP[wp].Velocity = ExtractShort(ref UAVXPacket, 13) * 0.1;
                        WP[wp].Loiter = ExtractShort(ref UAVXPacket, 15);

                        WP[wp].OrbitRadius = ExtractShort(ref UAVXPacket, 17);
                        WP[wp].OrbitGroundAltitude = ExtractShort(ref UAVXPacket, 19);
                        WP[wp].OrbitVelocity = ExtractShort(ref UAVXPacket, 21) * 0.1;
                        WP[wp].Action = UAVXPacket[23];

                        break;

                    case UAVXOriginPacketTag:
	                    Mission.NoOfWayPoints = UAVXPacket[2];

                        Properties.Settings.Default.Velocity = ExtractByte(ref UAVXPacket, 3) * 0.1f;
                        Mission.ProximityAltitude = ExtractByte(ref UAVXPacket, 4);
                        Mission.ProximityRadius = ExtractByte(ref UAVXPacket, 5);
                        Mission.FenceRadius = ExtractShort(ref UAVXPacket, 6);

                        Mission.OriginAltitude = ExtractShort(ref UAVXPacket, 8);
                        Mission.OriginLatitude = ExtractInt(ref UAVXPacket, 10);
                        Mission.OriginLongitude = ExtractInt(ref UAVXPacket, 14);

	                    NewMissionAvailable = true;

                        break;
                    case UAVXBBPacketTag:
                        // TODO add seqNo check later?
                        int seqNo = ExtractInt(ref UAVXPacket, 2);
                        short len = ExtractShort(ref UAVXPacket, 6);
                        if (seqNo == -1)
                        {
                            SaveBBFileBinaryWriter.Flush();
                            SaveBBFileBinaryWriter.Close();
                            SaveBBFileStream.Close();
                        }
                        else
                        {
                            if (seqNo == 0)
                                CreateSaveBBLogFile();
                            for (b = 0; b < len; b++)
                                SaveBBFileBinaryWriter.Write(UAVXPacket[b + 6 + 2]);
                            SaveBBFileBinaryWriter.Flush();
                        }
                        break;
                    case UAVXMinPacketTag:

                    for (i = 2; i < (NoOfFlagBytes + 2); i++)
                        Flags[i - 2] = ExtractByte(ref UAVXPacket, i);

                    StateT = (FlightStates)ExtractByte(ref UAVXPacket, 8);               
                    NavStateT = (NavStates)ExtractByte(ref UAVXPacket, 9);
                    AlarmStateT = ExtractByte(ref UAVXPacket, 10);

                    BatteryVoltsT = ExtractShort(ref UAVXPacket, 11);
                    BatteryCurrentT = ExtractShort(ref UAVXPacket, 13);
                    BatteryChargeT = ExtractShort(ref UAVXPacket, 15);

                    AngleT[Roll] = ExtractShort(ref UAVXPacket, 17);
                    AngleT[Pitch] = ExtractShort(ref UAVXPacket, 19);
                    AltitudeT = ExtractInt24(ref UAVXPacket, 21);
                    ROCT = ExtractShort(ref UAVXPacket, 24);  
                    HeadingT = ExtractShort(ref UAVXPacket, 26);

                    GPSLatitudeT = ExtractInt(ref UAVXPacket, 28);
                    GPSLongitudeT = ExtractInt(ref UAVXPacket, 32);

                    AirframeT = ExtractByte(ref UAVXPacket, 36);
                    MissionTimeMilliSecT = ExtractInt24(ref UAVXPacket, 37);

                    DesiredAltitudeT = 0;
                    RangefinderAltitudeT = 0;

                    UAVXArm = (AirframeT & 0x80) != 0;
                    AirframeT &= 0x7f;

                    UpdateFlightState();
                    UpdateNavState();
                    UpdateAlarmState();
                    UpdateBattery();
                   // UpdateWhere();
                    UpdateFlags();

                    Airframe.Text = AFNames[AirframeT];

                    UpdateAltitude();
                    UpdateAttitude();

                    DoOrientation();

                    Heading.Text = string.Format("{0:n0}", (float)HeadingT * MILLIRADDEG);

                    GPSLongitude.Text = string.Format("{0:n6}", (double)GPSLongitudeT * 1e-7); // 6000000
                    GPSLatitude.Text = string.Format("{0:n6}", (double)GPSLatitudeT * 1e-7);

                    if ((Flags[0] & 0x40) != 0) // GPSValid
                    {
                        GPSLongitude.BackColor = NavGroupBox.BackColor;
                        GPSLatitude.BackColor = NavGroupBox.BackColor;
                    }
                    else
                    {
                        GPSLongitude.BackColor = System.Drawing.Color.Orange;
                        GPSLatitude.BackColor = System.Drawing.Color.Orange;
                    }

                    //UpdateWhere();

                    MissionTimeTextBox.Text = string.Format("{0:n0}", MissionTimeMilliSecT / 1000);

                    break;

                case UAVXMinimOSDPacketTag:

                  //  for (i = 0; i < 6; i++)
                  //      Flags[i] = 0;
                  //  UpdateFlags();
//
                  //  GPShAccT = GPSvAccT = GPSsAccT = GPScAccT = 3200;

                    BatteryVoltsT = ExtractShort(ref UAVXPacket, 2);
                    BatteryVoltsT /= 100;
                    BatteryCurrentT = ExtractShort(ref UAVXPacket, 4);
                    BatteryChargeT = ExtractShort(ref UAVXPacket, 6);

                    AngleT[Roll] = ExtractShort(ref UAVXPacket, 8);
                    AngleT[Roll] *= DEGMILLIRAD;
                    AngleT[Pitch] = ExtractShort(ref UAVXPacket, 10);
                    AngleT[Pitch] *= DEGMILLIRAD;

                    AltitudeT = ExtractInt24(ref UAVXPacket, 12);
                    DesiredAltitudeT = ExtractInt24(ref UAVXPacket, 15);
                    ROCT = ExtractShort(ref UAVXPacket, 18);

                    GPSVelT = ExtractShort(ref UAVXPacket, 20);

                    HeadingT = ExtractShort(ref UAVXPacket, 22); // degrees
                    HeadingT *= DEGMILLIRAD;
                    GPSHeadingT = ExtractShort(ref UAVXPacket, 24); // degrees

                    GPSLatitudeT = ExtractInt(ref UAVXPacket, 26);
                    GPSLongitudeT = ExtractInt(ref UAVXPacket, 30);

                    GPSNoOfSatsT = ExtractByte(ref UAVXPacket, 34);
                    GPSFixT = ExtractByte(ref UAVXPacket, 35);
                    GPShAccT = ExtractShort(ref UAVXPacket, 36);

                    CurrWPT = ExtractByte(ref UAVXPacket, 38);
                    WPBearingT = ExtractShort(ref UAVXPacket, 39);
                    WPDistanceT = ExtractShort(ref UAVXPacket, 41);
                    CrossTrackET = ExtractShort(ref UAVXPacket, 43);

                    DesiredThrottleT = ExtractShort(ref UAVXPacket, 45);

                    byte MinimOSDArmedT = ExtractByte(ref UAVXPacket, 47);
                    NavStateT = (NavStates)ExtractByte(ref UAVXPacket, 48);
                    AirframeT = ExtractByte(ref UAVXPacket, 49);                

                    WarningPictureBox.Visible = MinimOSDArmedT != 0;

                    UpdateBattery();
                    //UpdateWhere();
                    //UpdateFlags();

                    WayHeading.Text = string.Format("{0:n0}", (float)WPBearingT);
                    CrossTrackError.Text = string.Format("{0:n1}", (float)CrossTrackET*0.1f);
                  

                    UpdateAltitude();
                    UpdateAttitude();
                    UpdateNavState();

                    UAVXArm = (AirframeT & 0x80) != 0;
                    AirframeT &= 0x7f;
                    Airframe.Text = AFNames[AirframeT];

                    Heading.Text = string.Format("{0:n0}", (float)HeadingT * MILLIRADDEG);                
                    
                   // DesiredThrottle.Text = string.Format("{0:n0}", DesiredThrottleT);

                    UpdateGPS();

                    DistanceToDesired.Text = string.Format("{0:n1}", (float)WPDistanceT * 0.1f);

                    SpeakNavStatus();

                    break;

                    case UAVXCalibrationPacketTag:

                    CalibrationPacketsReceived++;

                    short CalTRefT = ExtractShort(ref UAVXPacket, (byte)(2));

                    for (b = 0; b < 32; b++)
                        Cal[b] = ExtractShort(ref UAVXPacket, (byte)(b * 2 + 4));

                    CalTRefLabel.Text = string.Format("{0:n1}", CalTRefT * 0.1);
                       
	                 RollGyroMLabel.Text = string.Format("{0:n3}", Cal[0] * 0.001);
	                 RollGyroCLabel.Text = string.Format("{0:n3}", Cal[1] * 0.001);
	                 BFAccMLabel.Text = string.Format("{0:n3}", Cal[2] * 0.001);
	                 BFAccCLabel.Text = string.Format("{0:n3}", Cal[3] * 0.001);

                     MXScaleLabel.Text = string.Format("{0:n3}", Cal[4] * 0.001);
	                 MXBiasLabel.Text = string.Format("{0:n3}", Cal[5] * 0.001);
	 
	                 PitchGyroMLabel.Text = string.Format("{0:n3}", Cal[6] * 0.001);
	                 PitchGyroCLabel.Text = string.Format("{0:n3}", Cal[7] * 0.001);
	                 LRAccMLabel.Text = string.Format("{0:n3}", Cal[8] * 0.001);
	                 LRAccCLabel.Text = string.Format("{0:n3}", Cal[9] * 0.001);

                     MYScaleLabel.Text = string.Format("{0:n3}", Cal[10] * 0.001);
	                 MYBiasLabel.Text = string.Format("{0:n3}", Cal[11] * 0.001);
	 
	                 YawGyroMLabel.Text = string.Format("{0:n3}", Cal[12] * 0.001);
	                 YawGyroCLabel.Text = string.Format("{0:n3}", Cal[13] * 0.001);
	                 UDAccMLabel.Text = string.Format("{0:n3}", Cal[14] * 0.001);
	                 UDAccCLabel.Text = string.Format("{0:n3}", Cal[15] * 0.001);

                     MZScaleLabel.Text = string.Format("{0:n3}", Cal[16] * 0.001);
	                 MZBiasLabel.Text = string.Format("{0:n3}", Cal[17] * 0.001);

                     int Nyquist = (Cal[18] + 2) / 4;
                     AccLPFLabel.BackColor = Cal[19] >= (Cal[20]/3) ? System.Drawing.Color.Orange : System.Drawing.Color.Green;
                     PitchRollGyroLabel.BackColor = Cal[20] >= Nyquist   ? System.Drawing.Color.Red : System.Drawing.Color.Green;
                     YawPitchRollGyroLabel.BackColor = Cal[19] >= Cal[20] ? System.Drawing.Color.Orange : System.Drawing.Color.Green;

                     NyquistMarginLabel.Text = string.Format("{0:n0}", (Cal[18]+1) / 2);
                     AccLPFLabel.Text = string.Format("{0:n0}", Cal[19]);
                     PitchRollGyroLabel.Text = string.Format("{0:n0}", Cal[20]);
                     YawPitchRollGyroLabel.Text = string.Format("{0:n0}", Cal[21]);
  
                    break;

                case UAVXStatsPacketTag:
                    StatsPacketsReceived++;

                    for (b = 0; b < MaxStats; b++)
                        Stats[b] = ExtractShort(ref UAVXPacket, (byte)(b * 2 + 2));
                   
                    AirframeT = ExtractByte(ref UAVXPacket, 2 + MaxStats * 2 );
                    
                    UAVXArm = (AirframeT & 0x80) != 0;
                    AirframeT &= 0x7f;

                    I2CSIOFailS.Text = string.Format("{0:n0}", Stats[I2CFailsX] + Stats[SPIFailsX]);
                    GPSFailS.Text = string.Format("{0:n0}", Stats[GPSInvalidX]);
                    AccFailS.Text = string.Format("{0:n0}", Stats[AccFailsX]);
                    GyroFailS.Text = string.Format("{0:n0}", Stats[GyroFailsX]);
                    CompassFailS.Text = string.Format("{0:n0}", Stats[CompassFailsX]);
                    BaroFailS.Text = string.Format("{0:n0}", Stats[BaroFailsX]);
                    SaturationS.Text = string.Format("{0:n0}", Stats[SaturationX]);
                    RCFailSafeS.Text = string.Format("{0:n0}", Stats[RCFailSafesX]);


                    BadS.Text = string.Format("{0:n0}", Stats[BadX]);
                    ErrNoS.Text = string.Format("{0:n0}", Stats[BadNumX]);
                    UtilisationLabel.Text = Stats[UtilisationX] + "%";
                    UtilisationProgressBar.Value = Limit(Stats[UtilisationX], 0, 100);
                    UtilisationProgressBar.ForeColor = Stats[UtilisationX] < 40 ? System.Drawing.Color.Green : Stats[UtilisationX] < 60 ? System.Drawing.Color.Orange : System.Drawing.Color.Red;

                    Airframe.Text = AFNames[AirframeT];

                    DoOrientation();

                    break;
                case UAVXFlightPacketTag:
                    FlightPacketsReceived++;

                    for (i = 2; i < (NoOfFlagBytes + 2); i++)
                        Flags[i - 2] = ExtractByte(ref UAVXPacket, i);

                    StateT = (FlightStates)ExtractByte(ref UAVXPacket, 8);
                    BatteryVoltsT = ExtractShort(ref UAVXPacket, 9);
                    BatteryCurrentT = ExtractShort(ref UAVXPacket, 11);
                    BatteryChargeT = ExtractShort(ref UAVXPacket, 13);
                    RCGlitchesT = ExtractShort(ref UAVXPacket, 15);

                    GetAttitude(17);
                    
                    ROCT = ExtractShort(ref UAVXPacket, 49);
 
                    AltitudeT = ExtractInt24(ref UAVXPacket, 51);

                    CruiseThrottleT = ExtractShort(ref UAVXPacket, 54);
                    RangefinderAltitudeT = ExtractShort(ref UAVXPacket, 56);

                    DesiredAltitudeT = ExtractInt24(ref UAVXPacket, 58); 

                    HeadingT = ExtractShort(ref UAVXPacket, 61);
                    DesiredHeadingT = ExtractShort(ref UAVXPacket, 63);

                    TiltFFCompT = ExtractShort(ref UAVXPacket, 65);

                    BattFFCompT = ExtractShort(ref UAVXPacket, 67);

                    AltCompT = ExtractShort(ref UAVXPacket, 69);
                    AccConfidenceT = ExtractByte(ref UAVXPacket, 71);

                    BaroTemperatureT = ExtractShort(ref UAVXPacket, 72);
                    BaroPressureT = ExtractInt24(ref UAVXPacket, 74);

                    BaroAltitudeT = ExtractInt24(ref UAVXPacket, 77);

                    AccZT = ExtractByte(ref UAVXPacket, 80);
                    if (AccZT > 127) AccZT -= 256;

                    MagHeadingT = ExtractShort(ref UAVXPacket,81);

                    MPU6XXXTempT = ExtractShort(ref UAVXPacket, 83);

                    SensorTemp.Text = string.Format("{0:n1}", MPU6XXXTempT * 0.1);

                    if (MPU6XXXTempT < 0.0)
                        SensorTemp.BackColor = System.Drawing.Color.LightSteelBlue;
                    else
                        SensorTemp.BackColor = MPU6XXXTempT > 300.0 ?
                            System.Drawing.Color.Orange : EnvGroupBox.BackColor;

                    DoMotorsAndTime(85);

                    UpdateFlags();
                    UpdateFlightState();

                    UpdateControls();
                    UpdateAttitude();
                    UpdateAltitude();
                    UpdateAccelerations();
                    UpdateCompensation();
                    UpdateMotors();

                    Heading.Text = string.Format("{0:n0}", (float)HeadingT * MILLIRADDEG);

                    BaroTemperature.Text = string.Format("{0:n2}", BaroTemperatureT * 0.01);
                    BaroPressure.Text = string.Format("{0:n2}", BaroPressureT * 0.001);
                    BaroPressure.BackColor = (BaroPressureT < 800000) || (BaroPressureT > 1100000) ?
                    System.Drawing.Color.Orange : AltitudeGroupBox.BackColor;

                    RCGlitches.Text = string.Format("{0:n0}", RCGlitchesT);
                    RCGlitches.BackColor = RCGlitchesT > 20 ?
                        System.Drawing.Color.Orange : EnvGroupBox.BackColor;

                    MissionTimeTextBox.Text = string.Format("{0:n0}", MissionTimeMilliSecT / 1000);

                    UpdateBattery();
       
                    WriteTextLogFile(); // only log with this packet

                    break;
                case UAVXRCChannelsPacketTag:

                    for (c = 0; c < RC_MAX_CHANNELS; c++)
                        RCChannel[c] = 0;

                    RCPacketIntervalT = ExtractShort(ref UAVXPacket, 2);
                    DiscoveredRCChannelsT = UAVXPacket[4];
                    for (c = 0; c < 12; c++) //DiscoveredRCChannelsT
                        RCChannel[c] = ExtractShort(ref UAVXPacket, (short)(5 + c * 2));

                    break;
                case UAVXControlPacketTag:
                        int a;
                    ControlPacketsReceived++;

                   GetAttitude(2);

                    AirframeT = ExtractByte(ref UAVXPacket, 34);

                    UAVXArm = (AirframeT & 0x80) != 0;
                    AirframeT &= 0x7f;

                    DoMotorsAndTime(35); 

                    UpdateControls();
                    UpdateAttitude();
                    UpdateAccelerations();
                    UpdateCompensation();
                    UpdateMotors();

                    Heading.Text = string.Format("{0:n0}", (float)HeadingT * MILLIRADDEG);

                    MissionTimeTextBox.Text = string.Format("{0:n0}", MissionTimeMilliSecT / 1000);

                    break;

                case UAVXAnglePIDPacketTag:
                        
                    if (!AnglePIDHeader)
                    {
                        AnglePIDHeader = true;
                        WriteTextPIDAngleFileHeader();
                    }
                    MaxPID = ExtractByte(ref UAVXPacket, (byte)(2));
                    for (b = 0; b < MaxPID; b++)
                    {
                        float Temp = ExtractByte(ref UAVXPacket, (byte)(b + 3));
                        PID[b] = (Temp > 127.0f ? Temp - 256.0f : Temp) / 128.0f;
                    }

                    WriteTextPIDLogFile();

                    break;
                case UAVXRatePIDPacketTag:

                    if (!RatePIDHeader)
                    {
                        RatePIDHeader = true;
                        WriteTextPIDRateFileHeader();
                    }
                    MaxPID = ExtractByte(ref UAVXPacket, (byte)(2));
                    for (b = 0; b < MaxPID; b++)
                    {
                        float Temp = ExtractByte(ref UAVXPacket, (byte)(b + 3));
                        PID[b] = (Temp > 127.0f ? Temp - 256.0f : Temp) / 128.0f;
                    }

                    WriteTextPIDLogFile();

                    break;

                case UAVXRawIMUPacketTag:

                    if (!RawIMUHeader)
                    {
                        RawIMUHeader = true;
                        WriteTextIMUFileHeader();
                    }


                    MissionTimeMicroSecT = ExtractInt(ref UAVXPacket, (byte)(2));
                    for (b = 0; b < 6; b++)
                        RawIMU[b] = ExtractShort(ref UAVXPacket, (byte)(b*2 + 6));


                    WriteTextIMULogFile();

                    break;

                case UAVXAltPIDPacketTag:

                    if (!AltPIDHeader)
                    {
                        AltPIDHeader = true;
                        WriteTextPIDAltitudeFileHeader();
                    }

                    SaveTextLogFileStreamWriter.Write( ExtractShort(ref UAVXPacket, (byte)(2)) * 0.01f + ","); // Altitude
                    SaveTextLogFileStreamWriter.Write(ExtractShort(ref UAVXPacket, (byte)(4)) * 0.01f + ","); // Desired Altitude

                    MaxPID = ExtractByte(ref UAVXPacket, (byte)(6));
                    for (b = 0; b < MaxPID; b++)
                    {
                        float Temp = ExtractByte(ref UAVXPacket, (byte)(b + 7));
                        PID[b] = (Temp > 127.0f ? Temp - 256.0f : Temp) / 128.0f;
                    }
                    //Alt, DesAlt, AltE, AltP, AltI, DesROC, ROC, ROCP, ROCI, ROCD, AltComp, DesThr, CruiseThr

                    WriteTextPIDLogFile();

                    break;

                case UAVXNoisePacketTag:

                    NoiseIsGyro = ExtractByte(ref UAVXPacket, (byte)(2));
                    NoiseErrors = ExtractShort(ref UAVXPacket, (byte)(3));

                    SpectraGroupBox.Text = NoiseIsGyro != 0 ? "Gyro (Max % FS) " + string.Format("{0:n0}", NoiseErrors) : "Acc (0.5G FS)";
      
                    for (p = 0; p < 8; p++)
                        Noise[p] = ExtractShort(ref UAVXPacket, (byte)(5 + p * 2));

                       NoiseBar1.Value = Limit(Noise[0], 0, 100);
                       NoiseBar2.Value = Limit(Noise[1], 0, 100);
                       NoiseBar3.Value = Limit(Noise[2], 0, 100);
                       NoiseBar4.Value = Limit(Noise[3], 0, 100);

                       NoiseBar5.Value = Limit(Noise[4], 0, 100);
                       NoiseBar6.Value = Limit(Noise[5], 0, 100);
                       NoiseBar7.Value = Limit(Noise[6], 0, 100);
                       NoiseBar8.Value = Limit(Noise[7], 0, 100);

                    break;
                case UAVXNavPacketTag:
                    NavPacketsReceived++;
                    NavStateT = (NavStates)ExtractByte(ref UAVXPacket, 2);
                    AlarmStateT = ExtractByte(ref UAVXPacket, 3);

                    GPSNoOfSatsT = ExtractByte(ref UAVXPacket, 4);
                    GPSFixT = ExtractByte(ref UAVXPacket, 5);
                    CurrWPT = ExtractByte(ref UAVXPacket, 6);

                    GPShAccT = ExtractShort(ref UAVXPacket, 7);
                    GPSvAccT = ExtractShort(ref UAVXPacket, 9);
                    GPSsAccT = ExtractShort(ref UAVXPacket, 11);
                    GPScAccT = ExtractShort(ref UAVXPacket, 13);

                    WPBearingT = ExtractShort(ref UAVXPacket, 15);
                    CrossTrackET = ExtractShort(ref UAVXPacket, 17);
                    GPSVelT = ExtractShort(ref UAVXPacket, 19);
                    GPSHeadingT = ExtractShort(ref UAVXPacket, 21);
                    GPSAltitudeT = ExtractInt24(ref UAVXPacket, 23);
                    GPSLatitudeT = ExtractInt(ref UAVXPacket, 26);
                    GPSLongitudeT = ExtractInt(ref UAVXPacket, 30);

                    NorthPosET = ExtractInt(ref UAVXPacket, 34);
                    EastPosET = ExtractInt(ref UAVXPacket, 38);
                    NavStateTimeoutT = ExtractInt24(ref UAVXPacket, 42);

                   // MPU6XXXTempT = ExtractShort(ref UAVXPacket, 45);
                    GPSMissionTimeT = ExtractInt(ref UAVXPacket, 47);

                    NavSensitivityT = ExtractShort(ref UAVXPacket, 51);

                    NavPCorrT = ExtractShort(ref UAVXPacket, 53);
                    NavRCorrT = ExtractShort(ref UAVXPacket, 55);

                    NavYCorrT = ExtractShort(ref UAVXPacket, 57);

                    UpdateNavState();
                    UpdateAlarmState();

                    if ( Math.Abs(DesiredThrottleT - CruiseThrottleT) < 3 )
                        DesiredThrottle.BackColor = System.Drawing.Color.Green;
                    else
                        DesiredThrottle.BackColor = Math.Abs(DesiredThrottleT - CruiseThrottleT) < 7 ?
                            System.Drawing.Color.LightGreen : ControlsGroupBox.BackColor;

                    UpdateGPS();

                    SpeakNavStatus();
              
                    NavStateTimeout.Text = NavStateTimeoutT >= 0 ?
                        string.Format("{0:n0}", (float)NavStateTimeoutT * 0.001) : " ";

                    NavSensitivity.Text = string.Format("{0:n0}", (NavSensitivityT) * 0.1);
                    NavRCorr.Text = string.Format("{0:n0}", NavRCorrT * 0.1);
                    NavPCorr.Text = string.Format("{0:n0}", NavPCorrT * 0.1);
                    NavYCorr.Text = string.Format("{0:n0}", NavYCorrT);

                    break;
                    case UAVXGuidancePacketTag:
                        UpdateWhere(2);
                    break;


                case UAVXSoaringPacketTag:      

                    MissionTimeMilliSecT = ExtractInt24(ref UAVXPacket, 2);
         

                    break;
                case UAVXFusionPacketTag:

                    HRAccZT = ExtractShort(ref UAVXPacket, 2);
                    BaroRawAltitudeT = ExtractInt24(ref UAVXPacket, 4);
                    FROCT = ExtractShort(ref UAVXPacket, 7);
                    HRAccZBiasT = ExtractShort(ref UAVXPacket, 9);
                    FWRateEnergyT = ExtractShort(ref UAVXPacket, 11);
                    FWGlideOffsetAngleT = ExtractShort(ref UAVXPacket, 13);
                    NewTuningParameterT = ExtractShort(ref UAVXPacket, 15);

                    FWRateEnergy.Text = string.Format("{0:n0}", FWRateEnergyT);
                    FWGlideOffsetAngle.Text = string.Format("{0:n1}", (float)FWGlideOffsetAngleT*0.1f);

                        // plus 4 more 16bit values

                    break;
                default: break;
            } // switch


            if ((RxPacketTag == UAVXMinimOSDPacketTag) 
                || (RxPacketTag == UAVXControlPacketTag) 
                || (RxPacketTag == UAVXFlightPacketTag) 
                || ( RxPacketTag == UAVXMinPacketTag ) )
            {
                FlightRoll = AngleT[Pitch] * OSO + AngleT[Roll] * OCO;
                FlightPitch = AngleT[Pitch] * OCO - AngleT[Roll] * OSO;

                FlightRollp = FlightRoll;
                FlightPitchp = FlightPitch;

                attitudeIndicatorInstrumentControl1.SetAttitudeIndicatorParameters(
                        FlightPitch * MILLIRADDEG, -FlightRoll * MILLIRADDEG);

            }

            if ((RxPacketTag == UAVXMinimOSDPacketTag) || (RxPacketTag == UAVXFlightPacketTag ))
            {
                FlightHeading = (int)((HeadingT) * MILLIRADDEG);
                if (FlightHeading >= 360) FlightHeading -= 360;

                headingIndicatorInstrumentControl1.SetHeadingIndicatorParameters(FlightHeading);
            }
        }

        public void UAVXReadTelemetry(object sender, EventArgs e)
        {
            byte b;

            while (RxHead != RxTail)
            {
                b = RxQueue[RxHead];
                RxHead++;
                RxHead &= RxQueueMask;

                    ParsePacket(b);
                    if (PacketReceived)
                        ProcessPacket();
 
                RxTypeErr.Text = string.Format("{0:n0}", RxIllegalErrors);
                RxCSumErr.Text = string.Format("{0:n0}", RxCheckSumErrors);
                RxLenErr.Text = string.Format("{0:n0}", RxLengthErrors);
 
            }
    
            ReadingTelemetry = false;
        }


        private static void SendPacketHeader()
        {
            TxChar(0xff); // synchronisation to "jolt" USART
            TxChar(SOH);
            TxCheckSum = 0;
        } // SendPacketHeader

        private static void SendPacketTrailer()
        {
            TxESCu8(TxCheckSum);
            TxChar(EOT);

            TxChar(CR);
            TxChar(LF);
        } // SendPacketTrailer

        public static void SendParamsPacket() 
        {
	        int p;

            if (NoUplink)
            {
                speech.SpeakAsync("No connection to flight controller, possible use of telemetry Rx by GPS on parallel Rx Version 3 board when armed.");
            }
            else
            {

                SendPacketHeader();

                TxESCu8(UAVXParamPacketTag);
                TxESCu8(MAX_PARAMS + 1);

                TxESCu8(ParameterForm.CurrPS);
                for (p = 0; p < MAX_PARAMS; p++)
                    TxESCi8(Convert.ToByte(ParameterForm.P[ParameterForm.CurrPS, p].Value));

                SendPacketTrailer();
                //speech.SpeakAsync("Uploaded set " + ParameterForm.CurrPS);
                speech.SpeakAsync("Updating parameters");
            }

        } // SendParamsPacket

        public static void SendRequestPacket(byte Tag, byte a1, byte a2)
        {
           // TxLabel.Text = "Request " + TagNames[Tag];

            if (NoUplink)
            {
                speech.SpeakAsync("No connection to flight controller, possible use of telemetry Rx by GPS on parallel Rx Version 3 board when armed.");
            }
            else
            {
                SendPacketHeader();
                TxESCu8(UAVXRequestPacketTag);
                TxESCu8(3);

                TxESCu8(Tag);
                TxESCu8(a1);
                TxESCu8(a2);

                SendPacketTrailer();
                if (Tag == UAVXParamPacketTag)
                {
                    if (a1 < 4)
                        speech.SpeakAsync("Using selected default, CAUTION Ensure parameters match the actual aircraft BEFORE setting the ESC type"); // + a1);
                    else
                        speech.SpeakAsync("Reading parameters");
                }
            }
        } // SendRequestPacket


        public static void SendMission()
        {
            int wp;

            for (wp = 1; wp <= Mission.NoOfWayPoints; wp++)
                SendWPPacket(wp);

            SendOriginPacket();
        } // SendMission


        private static void SendOriginPacket()
        {
            SendPacketHeader();

            TxESCu8(UAVXOriginPacketTag);
            TxESCu8(5);

            TxESCu8(Mission.NoOfWayPoints);
            TxESCu8(Mission.ProximityAltitude);
            TxESCu8(Mission.ProximityRadius);
            TxESCi16(Mission.FenceRadius);

            SendPacketTrailer();
        } // SendOriginPacket


        private static void SendWPPacket(int wp)
        {
           // TxLabel.Text = "WP";

            SendPacketHeader();

            TxESCu8(UAVXWPPacketTag);
            TxESCu8(22);

            TxESCu8((byte)wp);
            TxESCi32(WP[wp].LatitudeRaw); // 1e7/degree
            TxESCi32(WP[wp].LongitudeRaw);
            TxESCi16(WP[wp].Altitude); 
            TxESCi16((short)(WP[wp].Velocity * 10.0)); // dM/S
            TxESCi16(WP[wp].Loiter); // S
            TxESCi16(WP[wp].OrbitRadius );
            TxESCi16(WP[wp].OrbitGroundAltitude); // dM relative to Origin
            // TxESCi16(Convert.ToInt16(GetAltitudeData(WP[wp].LatitudeRaw * 1e-7, WP[wp].LongitudeRaw * 1e-7)));
            TxESCi16((short)(WP[wp].OrbitVelocity * 10.0)); // dM/S
            TxESCu8(WP[wp].Action);

            SendPacketTrailer();
        } // SendWPPacket

        //___________________________________________________________________________________


        void WriteTextIMULogFile()
        {
            short i;

            SaveTextLogFileStreamWriter.Write(MissionTimeMicroSecT + ",");
            for (i = 0; i < 6; i++)
                SaveTextLogFileStreamWriter.Write(RawIMU[i] + ",");

            SaveTextLogFileStreamWriter.WriteLine();
            SaveTextLogFileStreamWriter.Flush();

        }

        void WriteTextPIDLogFile()
        {
            short i;

            for (i = 0; i < MaxPID; i++)
                    SaveTextLogFileStreamWriter.Write(PID[i] + ",");

            SaveTextLogFileStreamWriter.WriteLine();
            SaveTextLogFileStreamWriter.Flush();

        }

        void WriteTextLogFile()
        {
            short i, c;

            if (!LogFileHeaderWritten)
            {
                WriteTextLogFileHeader();
                LogFileHeaderWritten = true;
            }

            SaveTextLogFileStreamWriter.Write(MissionTimeMilliSecT * 0.001 + ",");

            SaveTextLogFileStreamWriter.Write("Flight,");

            for (i = 0; i < 48; i++)
                if (F[i])
                    SaveTextLogFileStreamWriter.Write("1,");
                else
                    SaveTextLogFileStreamWriter.Write(",");

            SaveTextLogFileStreamWriter.Write(StateT + "," +

            BatteryVoltsT * 0.01 + "," +
            BatteryCurrentT * 0.01 + "," +

            BatteryChargeT + "," +
            RCGlitchesT + "," +
            RCPacketIntervalT * 0.001f + ",");

            SaveTextLogFileStreamWriter.Write(DiscoveredRCChannelsT + ",");
            for (c = 0; c < 9; c++)
                 SaveTextLogFileStreamWriter.Write(RCChannel[c] * 0.001 + ",");

            SaveTextLogFileStreamWriter.Write(DesiredThrottleT * 0.001+ ",");

            for (i = 0; i < 3; i++)
            {
                SaveTextLogFileStreamWriter.Write(DesiredAngleT[i] * 0.001 + "," +
                AngleT[i] * 0.001 + "," +
                AccT[i] * 0.001 + "," + //zzz FB/LR need to be reversed
                DesiredRateT[i] * 0.001 + "," +
                RateT[i] * 0.001 + ",");
            }

            SaveTextLogFileStreamWriter.Write(AccConfidenceT * 0.01 + "," ); 

            for (i = 0; i < 10; i++)
               SaveTextLogFileStreamWriter.Write(PWMT[i]*0.001 + ",");

            SaveTextLogFileStreamWriter.Write("Nav," +
            GPSMissionTimeT + "," +
            NavStateTimeoutT + "," +
            NavStateT + "," +
            AlarmStateT + "," +

            GPSNoOfSatsT + "," +
            GPSFixT + "," +
            GPShAccT * 0.01 + "," +
            GPSvAccT * 0.01 + "," +
             GPSsAccT * 0.01 + "," +
            GPScAccT * 0.01 + "," +

            GPSVelT * 0.1 + "," +
            GPSHeadingT * MILLIRADDEG + "," +
            GPSAltitudeT * 0.01 + "," +
            GPSLatitudeT * 1e-7 + "," +
            GPSLongitudeT * 1e-7 + "," +

            NavSensitivityT * 0.1 + "," +

            CurrWPT + "," +

            WPBearingT * MILLIRADDEG + "," +

            CruiseThrottleT * 0.001 + "," +
            BaroTemperatureT * 0.01 + "," +

            BaroPressureT * 0.001 + "," +
            BaroRawAltitudeT * 0.01 + "," +  
            BaroAltitudeT * 0.01 + "," +
            GPSAltitudeT * 0.01 + "," +
            RangefinderAltitudeT * 0.01 + "," +
            AltitudeT * 0.01 + "," +
            DesiredAltitudeT * 0.01 + "," +
            ROCT * 0.01 + "," +
            FROCT * 0.01 + "," +
            HRAccZT * 0.001 + "," +
            HRAccZBiasT * 0.001 + "," +
            FWRateEnergyT * 0.001 + "," +
            FWGlideOffsetAngleT * 0.1 + "," +
            NewTuningParameterT + "," +

            AltCompT * 0.001 + "," +
  
            TiltFFCompT * 0.001 + "," +
             BattFFCompT * 0.001 + "," +

            NorthPosET * 0.1 + "," +
            EastPosET * 0.1 + "," +
            NavPCorrT * 0.001 + "," +
            NavRCorrT * 0.001 + "," +

            GPSHeadingT * MILLIRADDEG + "," +
            MagHeadingT * MILLIRADDEG + "," +
            HeadingT * MILLIRADDEG + "," +
            DesiredHeadingT * MILLIRADDEG + "," +

            MPU6XXXTempT * 0.1 + ",");

            if (NoiseIsGyro != 0)
                SaveTextLogFileStreamWriter.Write("GyroDelta,");
            else
                SaveTextLogFileStreamWriter.Write("AccDFT,");
 
            for (i = 0; i < 8; i++)
                SaveTextLogFileStreamWriter.Write(Noise[i] + ",");

            SaveTextLogFileStreamWriter.Write("Stats,");

            for (i = 0; i < MaxStats; i++)
                if (Stats[i] == 0)
                    SaveTextLogFileStreamWriter.Write(",");
                else
                    SaveTextLogFileStreamWriter.Write( Stats[i] + ",");

            SaveTextLogFileStreamWriter.Write( AirframeT + "," );

            SaveTextLogFileStreamWriter.WriteLine();
            SaveTextLogFileStreamWriter.Flush();
        }

        void CloseFiles() {

            SaveLogFileBinaryWriter.Flush();
            SaveLogFileBinaryWriter.Close();
            SaveLogFileStream.Close();

            SaveTextLogFileStreamWriter.Flush();
            SaveTextLogFileStreamWriter.Close();
            SaveTextLogFileStream.Close();
        }

        void UAVXCloseTelemetry()
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
                CloseFiles();
            }
        }

        //----------------------------------------------------------------------- 

        public static void TxChar(byte ch)
        {
            var b = new byte[] { ch };
            TxCheckSum ^= ch;
            serialPort1.Write(b, 0, 1);
        } // TxChar

        public static void TxESCu8(byte ch)
        {
            if ((ch == SOH) || (ch == EOT) || (ch == ESC))
                TxChar(ESC);
            TxChar(ch);
        } // TxESCu8

        public static void TxESCi8(byte b)
        {
            if ((b == SOH) || (b == EOT) || (b == ESC))
                TxChar(ESC);
            TxChar(b);
        } // TxESCu8

        public static void Sendi16(short v)
        {
            TxChar((byte)(v & 0xff));
            TxChar((byte)((v >> 8) & 0xff));
        } // Sendi16

        public static void TxESCi16(short v)
        {
            TxESCu8((byte)(v & 0xff));
            TxESCu8((byte)((v >> 8) & 0xff));
        } // TxESCi16

        public static void TxESCi24(int v)
        {
            TxESCu8((byte)(v & 0xff));
            TxESCu8((byte)((v >> 8) & 0xff));
            TxESCu8((byte)((v >> 16) & 0xff));
        } // TxESCi24

        public static void TxESCi32(int v)
        {
            TxESCi16((short)(v & 0xffff));
            TxESCi16((short)((v >> 16) & 0xffff));
        } // TxESCi32

        public void Zero(ref byte[] a, short len)
        {
            for (int s = 0; s < len; s++)
                a[s] = 0;
        }

        public static byte ExtractByte(ref byte[] a, short p)
        {
            return a[p];
        }

        public static short ExtractSignedByte(ref byte[] a, short p)
        {
            short temp;

            temp = a[p];
            if (temp > 127)
                temp -= 256;

            return temp;
        }

        public static short ExtractShort(ref byte[] a, short p)
        {
            short temp;
            temp = (short)(a[p + 1] << 8);
            temp |= (short)a[p];

            return temp;
        }

        public static int ExtractInt24(ref byte[] a, short p)
        {
            int temp;

            temp = ((int)a[p + 2] << 24);
            temp |= ((int)a[p + 1] << 16);
            temp |= (int)a[p] << 8;
            temp /= 256;
            return temp;
        }

        public static int ExtractInt(ref byte[] a, short p)
        {
            int temp;

            temp = (int)(a[p + 3] << 24);
            temp |= ((int)a[p + 2] << 16);
            temp |= ((int)a[p + 1] << 8);
            temp |= (int)a[p];
            return temp;
        }

        public static double ConvertGPSToM(double c)
        {	
            //return (c * 0.018553257183);
            return ((double)c * 0.011131953098f);
        }

        private void ReplayNumericUpDown_Changed(object sender, EventArgs e)
        {
            ReplayDelay = 20 - Convert.ToInt16(ReplayNumericUpDown.Text);
        }


        private void StartParametersButton_Click(object sender, EventArgs e)
        {

            UAVXOpenTelemetry();

            StartParametersButton.BackColor = System.Drawing.Color.Green;
            ParameterForm formParameters = new ParameterForm();
           // if (formParameters.errorFlag == false)
                formParameters.Show(); // was ShowDialog
        }

        private void PlotButton_Click(object sender, EventArgs e)
        {

            UAVXOpenTelemetry();

            PlotButton.BackColor = System.Drawing.Color.Green;
            PlotForm formParameters = new PlotForm();
            // if (formParameters.errorFlag == false)
            formParameters.Show(); // was ShowDialog
        }

        private void StartNavigationButton_Click(object sender, EventArgs e)
        {

            UAVXOpenTelemetry();

            StartNavigationButton.BackColor = System.Drawing.Color.Green;
           // NavForm formNav = new NavForm();
           // if (formNav.errorFlag == false)
                formNav.Show();

        }

  
     
    }
}