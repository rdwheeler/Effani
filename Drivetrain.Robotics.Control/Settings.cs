using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Drivetrain.Robotics.Control
{
	public class SettingsVersion
	{
		#region Properties
		public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
		public string? Description { get; set; }
		public Settings DeviceSettings { get; set; } = new Settings();
		[Key]
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public bool IsActive { get; set; }
		#endregion
	}

	[Owned]
	public class Settings : INotifyPropertyChanged
	{
		#region Properties
		public decimal AspirateKohActuatorStartPosition
		{
			get
			{
				return _aspirateKohActuatorStartPosition;
			}
			set
			{
				if (_aspirateKohActuatorStartPosition == value)
					return;
				_aspirateKohActuatorStartPosition = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateKohActuatorStartPosition)));
			}
		}
		public decimal AspirateKohDistanceToPlate
		{
			get
			{
				return _aspirateKohDistanceToPlate;
			}
			set
			{
				if (_aspirateKohDistanceToPlate == value)
					return;
				_aspirateKohDistanceToPlate = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateKohDistanceToPlate)));
			}
		}
		public Coordinate AspirateKohLocation
		{
			get
			{
				return _aspirateKohLocation;
			}
			set
			{
				if (_aspirateKohLocation == value)
					return;
				_aspirateKohLocation = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateKohLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal AspirateKohLocationX
		{
			get => AspirateKohLocation.X;
			set
			{
				AspirateKohLocation.X = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateKohLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal AspirateKohLocationY
		{
			get => AspirateKohLocation.Y;
			set
			{
				AspirateKohLocation.Y = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateKohLocation)));
			}
		}
		public decimal AspiratePositioningSpeed
		{
			get
			{
				return _aspiratePositioningSpeed;
			}
			set
			{
				if (_aspiratePositioningSpeed == value)
					return;
				_aspiratePositioningSpeed = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspiratePositioningSpeed)));
			}
		}
		public decimal AspirateStirSpeed
		{
			get
			{
				return _aspirateStirSpeed;
			}
			set
			{
				if (_aspirateStirSpeed == value)
					return;
				_aspirateStirSpeed = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateStirSpeed)));
			}
		}
		public decimal AspirateTravelPerMicroLiter
		{
			get
			{
				return _aspirateTravelPerMicroLiter;
			}
			set
			{
				if (_aspirateTravelPerMicroLiter == value)
					return;
				_aspirateTravelPerMicroLiter = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateTravelPerMicroLiter)));
			}
		}
		public int AspirationDwell
		{
			get
			{
				return _aspirationDwell;
			}
			set
			{
				if (_aspirationDwell == value)
					return;
				_aspirationDwell = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirationDwell)));
			}
		}
		public int AspirationSuctionToggleBoardNumber
		{
			get
			{
				return _aspirationSuctionToggleBoardNumber;
			}
			set
			{
				if (_aspirationSuctionToggleBoardNumber == value)
					return;
				_aspirationSuctionToggleBoardNumber = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirationSuctionToggleBoardNumber)));
			}
		}
		public int AspirationSuctionTogglePortNumber
		{
			get
			{
				return _aspirationSuctionTogglePortNumber;
			}
			set
			{
				if (_aspirationSuctionTogglePortNumber == value)
					return;
				_aspirationSuctionTogglePortNumber = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirationSuctionTogglePortNumber)));
			}
		}
		public int BasalIncubationTimerPeriod
		{
			get
			{
				return _basalIncubationTimerPeriod;
			}
			set
			{
				if (_basalIncubationTimerPeriod == value)
					return;
				_basalIncubationTimerPeriod = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BasalIncubationTimerPeriod)));
			}
		}
		public Coordinate BasalLocation
		{
			get
			{
				return _basalLocation;
			}
			set
			{
				if (_basalLocation == value)
					return;
				_basalLocation = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BasalLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal BasalLocationX
		{
			get => BasalLocation.X;
			set
			{
				BasalLocation.X = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BasalLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal BasalLocationY
		{
			get => BasalLocation.Y;
			set
			{
				BasalLocation.Y = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BasalLocation)));
			}
		}
		public decimal BasalSyringeLoadingPosition
		{
			get
			{
				return _basalSyringeLoadingPosition;
			}
			set
			{
				if (_basalSyringeLoadingPosition == value)
					return;
				_basalSyringeLoadingPosition = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BasalSyringeLoadingPosition)));
			}
		}
		public decimal BasalTravelPerMicroLiter
		{
			get
			{
				return _basalTravelPerMicroLiter;
			}
			set
			{
				if (_basalTravelPerMicroLiter == value)
					return;
				_basalTravelPerMicroLiter = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BasalTravelPerMicroLiter)));
			}
		}
		public int BasalVolume
		{
			get
			{
				return _basalVolume;
			}
			set
			{
				if (_basalVolume == value)
					return;
				_basalVolume = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BasalVolume)));
			}
		}
		public Coordinate BiopsyLocation
		{
			get
			{
				return _biopsyLocation;
			}
			set
			{
				if (_biopsyLocation == value)
					return;
				_biopsyLocation = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BiopsyLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal BiopsyLocationX
		{
			get => BiopsyLocation.X;
			set
			{
				BiopsyLocation.X = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BiopsyLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal BiopsyLocationY
		{
			get => BiopsyLocation.Y;
			set
			{
				BiopsyLocation.Y = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BiopsyLocation)));
			}
		}
		public int ComPortNumber
		{
			get
			{
				return _comPortNumber;
			}
			set
			{
				if (_comPortNumber == value)
					return;
				_comPortNumber = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ComPortNumber)));
			}
		}
		public int DnaAspirationVolume
		{
			get
			{
				return _dnaAspirationVolume;
			}
			set
			{
				if (_dnaAspirationVolume == value)
					return;
				_dnaAspirationVolume = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaAspirationVolume)));
			}
		}
		public Coordinate DnaLocation
		{
			get
			{
				return _dnaLocation;
			}
			set
			{
				if (_dnaLocation == value)
					return;
				_dnaLocation = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal DnaLocationX
		{
			get => DnaLocation.X;
			set
			{
				DnaLocation.X = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal DnaLocationY
		{
			get => DnaLocation.Y;
			set
			{
				DnaLocation.Y = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaLocation)));
			}
		}
		public int DnaSensorBoardNumber
		{
			get
			{
				return _dnaSensorBoardNumber;
			}
			set
			{
				if (_dnaSensorBoardNumber == value)
					return;
				_dnaSensorBoardNumber = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaSensorBoardNumber)));
			}
		}
		public Coordinate DnaSensorLocation
		{
			get
			{
				return _dnaSensorLocation;
			}
			set
			{
				if (_dnaSensorLocation == value)
					return;
				_dnaSensorLocation = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaSensorLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal DnaSensorLocationX
		{
			get => DnaSensorLocation.X;
			set
			{
				DnaSensorLocation.X = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaSensorLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal DnaSensorLocationY
		{
			get => DnaSensorLocation.Y;
			set
			{
				DnaSensorLocation.Y = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaSensorLocation)));
			}
		}
		public int DnaSensorNumber
		{
			get
			{
				return _dnaSensorNumber;
			}
			set
			{
				if (_dnaSensorNumber == value)
					return;
				_dnaSensorNumber = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaSensorNumber)));
			}
		}
		public int DnaSensorPortNumber
		{
			get
			{
				return _dnaSensorPortNumber;
			}
			set
			{
				if (_dnaSensorPortNumber == value)
					return;
				_dnaSensorPortNumber = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaSensorPortNumber)));
			}
		}
		public int DnaSensorReadHigh
		{
			get
			{
				return _dnaSensorReadHigh;
			}
			set
			{
				if (_dnaSensorReadHigh == value)
					return;
				_dnaSensorReadHigh = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaSensorReadHigh)));
			}
		}
		public int DnaSensorReadLow
		{
			get
			{
				return _dnaSensorReadLow;
			}
			set
			{
				if (_dnaSensorReadLow == value)
					return;
				_dnaSensorReadLow = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaSensorReadLow)));
			}
		}
		public decimal DnaSyringeLoadingPosition
		{
			get
			{
				return _dnaSyringeLoadingPosition;
			}
			set
			{
				if (_dnaSyringeLoadingPosition == value)
					return;
				_dnaSyringeLoadingPosition = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaSyringeLoadingPosition)));
			}
		}
		public int DnaTimerPeriod
		{
			get
			{
				return _dnaTimerPeriod;
			}
			set
			{
				if (_dnaTimerPeriod == value)
					return;
				_dnaTimerPeriod = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaTimerPeriod)));
			}
		}
		public decimal DnaTravelPerMicroLiter
		{
			get
			{
				return _dnaTravelPerMicroLiter;
			}
			set
			{
				if (_dnaTravelPerMicroLiter == value)
					return;
				_dnaTravelPerMicroLiter = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaTravelPerMicroLiter)));
			}
		}
		public int DnaVolume
		{
			get
			{
				return _dnaVolume;
			}
			set
			{
				if (_dnaVolume == value)
					return;
				_dnaVolume = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaVolume)));
			}
		}
		public string EffaniApiUrl
		{
			get
			{
				return _effaniApiUrl;
			}
			set
			{
				if (_effaniApiUrl == value)
					return;
				_effaniApiUrl = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EffaniApiUrl)));
			}
		}
		public string EffaniPassword
		{
			get
			{
				return _effaniPassword;
			}
			set
			{
				if (_effaniPassword == value)
					return;
				_effaniPassword = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EffaniPassword)));
			}
		}
		public string EffaniUsername
		{
			get
			{
				return _effaniUsername;
			}
			set
			{
				if (_effaniUsername == value)
					return;
				_effaniUsername = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EffaniUsername)));
			}
		}
		public Coordinate FirstWellLocation
		{
			get
			{
				return _firstWellLocation;
			}
			set
			{
				if (_firstWellLocation == value)
					return;
				_firstWellLocation = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FirstWellLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal FirstWellLocationX
		{
			get => FirstWellLocation.X;
			set
			{
				FirstWellLocation.X = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FirstWellLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal FirstWellLocationY
		{
			get => FirstWellLocation.Y;
			set
			{
				FirstWellLocation.Y = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FirstWellLocation)));
			}
		}
		public int FluidEmissionDwell
		{
			get
			{
				return _fluidEmissionDwell;
			}
			set
			{
				if (_fluidEmissionDwell == value)
					return;
				_fluidEmissionDwell = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FluidEmissionDwell)));
			}
		}
		public decimal FluidSyringeSpeed
		{
			get
			{
				return _fluidSyringeSpeed;
			}
			set
			{
				if (_fluidSyringeSpeed == value)
					return;
				_fluidSyringeSpeed = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FluidSyringeSpeed)));
			}
		}
		public int HorizontalWellCount
		{
			get
			{
				return _horizontalWellCount;
			}
			set
			{
				if (_horizontalWellCount == value)
					return;
				_horizontalWellCount = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HorizontalWellCount)));
			}
		}
		public int InitialBasalFluidPrimingVolume
		{
			get
			{
				return _initialBasalFluidPrimingVolume;
			}
			set
			{
				if (_initialBasalFluidPrimingVolume == value)
					return;
				_initialBasalFluidPrimingVolume = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InitialBasalFluidPrimingVolume)));
			}
		}
		public int InitialDnaFluidPrimingVolume
		{
			get
			{
				return _initialDnaFluidPrimingVolume;
			}
			set
			{
				if (_initialDnaFluidPrimingVolume == value)
					return;
				_initialDnaFluidPrimingVolume = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InitialDnaFluidPrimingVolume)));
			}
		}
		public int InitialKohFluidPrimingVolume
		{
			get
			{
				return _initialKohFluidPrimingVolume;
			}
			set
			{
				if (_initialKohFluidPrimingVolume == value)
					return;
				_initialKohFluidPrimingVolume = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InitialKohFluidPrimingVolume)));
			}
		}
		public int InitialNadhFluidPrimingVolume
		{
			get
			{
				return _initialNadhFluidPrimingVolume;
			}
			set
			{
				if (_initialNadhFluidPrimingVolume == value)
					return;
				_initialNadhFluidPrimingVolume = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InitialNadhFluidPrimingVolume)));
			}
		}
		public int KohAspirationVolume
		{
			get
			{
				return _kohAspirationVolume;
			}
			set
			{
				if (_kohAspirationVolume == value)
					return;
				_kohAspirationVolume = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KohAspirationVolume)));
			}
		}
		public Coordinate KohLocation
		{
			get
			{
				return _kohLocation;
			}
			set
			{
				if (_kohLocation == value)
					return;
				_kohLocation = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KohLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal KohLocationX
		{
			get => KohLocation.X;
			set
			{
				KohLocation.X = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KohLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal KohLocationY
		{
			get => KohLocation.Y;
			set
			{
				KohLocation.Y = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KohLocation)));
			}
		}
		public decimal KohSyringeLoadingPosition
		{
			get
			{
				return _kohSyringeLoadingPosition;
			}
			set
			{
				if (_kohSyringeLoadingPosition == value)
					return;
				_kohSyringeLoadingPosition = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KohSyringeLoadingPosition)));
			}
		}
		public int KohTimerPeriod
		{
			get
			{
				return _kohTimerPeriod;
			}
			set
			{
				if (_kohTimerPeriod == value)
					return;
				_kohTimerPeriod = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KohTimerPeriod)));
			}
		}
		public decimal KohTravelPerMicroLiter
		{
			get
			{
				return _kohTravelPerMicroLiter;
			}
			set
			{
				if (_kohTravelPerMicroLiter == value)
					return;
				_kohTravelPerMicroLiter = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KohTravelPerMicroLiter)));
			}
		}
		public int KohVolume
		{
			get
			{
				return _kohVolume;
			}
			set
			{
				if (_kohVolume == value)
					return;
				_kohVolume = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KohVolume)));
			}
		}
		public int NadhIncubationDwell
		{
			get
			{
				return _nadhIncubationDwell;
			}
			set
			{
				if (_nadhIncubationDwell == value)
					return;
				_nadhIncubationDwell = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhIncubationDwell)));
			}
		}
		public Coordinate NadhLocation
		{
			get
			{
				return _nadhLocation;
			}
			set
			{
				if (_nadhLocation == value)
					return;
				_nadhLocation = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal NadhLocationX
		{
			get => NadhLocation.X;
			set
			{
				NadhLocation.X = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal NadhLocationY
		{
			get => NadhLocation.Y;
			set
			{
				NadhLocation.Y = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhLocation)));
			}
		}
		public int NadhReading1TimerPeriod
		{
			get
			{
				return _nadhReading1TimerPeriod;
			}
			set
			{
				if (_nadhReading1TimerPeriod == value)
					return;
				_nadhReading1TimerPeriod = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhReading1TimerPeriod)));
			}
		}
		public int NadhReadingsTimerPeriod
		{
			get
			{
				return _nadhReadingsTimerPeriod;
			}
			set
			{
				if (_nadhReadingsTimerPeriod == value)
					return;
				_nadhReadingsTimerPeriod = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhReadingsTimerPeriod)));
			}
		}
		public int NadhSensorBoardNumber
		{
			get
			{
				return _nadhSensorBoardNumber;
			}
			set
			{
				if (_nadhSensorBoardNumber == value)
					return;
				_nadhSensorBoardNumber = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhSensorBoardNumber)));
			}
		}
		public Coordinate NadhSensorLocation
		{
			get
			{
				return _nadhSensorLocation;
			}
			set
			{
				if (_nadhSensorLocation == value)
					return;
				_nadhSensorLocation = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhSensorLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal NadhSensorLocationX
		{
			get => NadhSensorLocation.X;
			set
			{
				NadhSensorLocation.X = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhSensorLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal NadhSensorLocationY
		{
			get => NadhSensorLocation.Y;
			set
			{
				NadhSensorLocation.Y = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhSensorLocation)));
			}
		}
		public int NadhSensorNumber
		{
			get
			{
				return _nadhSensorNumber;
			}
			set
			{
				if (_nadhSensorNumber == value)
					return;
				_nadhSensorNumber = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhSensorNumber)));
			}
		}
		public int NadhSensorPortNumber
		{
			get
			{
				return _nadhSensorPortNumber;
			}
			set
			{
				if (_nadhSensorPortNumber == value)
					return;
				_nadhSensorPortNumber = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhSensorPortNumber)));
			}
		}
		public int NadhSensorReadHigh
		{
			get
			{
				return _nadhSensorReadHigh;
			}
			set
			{
				if (_nadhSensorReadHigh == value)
					return;
				_nadhSensorReadHigh = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhSensorReadHigh)));
			}
		}
		public int NadhSensorReadLow
		{
			get
			{
				return _nadhSensorReadLow;
			}
			set
			{
				if (_nadhSensorReadLow == value)
					return;
				_nadhSensorReadLow = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhSensorReadLow)));
			}
		}
		public decimal NadhSyringeLoadingPosition
		{
			get
			{
				return _nadhSyringeLoadingPosition;
			}
			set
			{
				if (_nadhSyringeLoadingPosition == value)
					return;
				_nadhSyringeLoadingPosition = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhSyringeLoadingPosition)));
			}
		}
		public decimal NadhTravelPerMicroLiter
		{
			get
			{
				return _nadhTravelPerMicroLiter;
			}
			set
			{
				if (_nadhTravelPerMicroLiter == value)
					return;
				_nadhTravelPerMicroLiter = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhTravelPerMicroLiter)));
			}
		}
		public int NadhVolume
		{
			get
			{
				return _nadhVolume;
			}
			set
			{
				if (_nadhVolume == value)
					return;
				_nadhVolume = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhVolume)));
			}
		}
		public bool PauseCommandsForPrompt { get; set; }
		public Coordinate PlateOffset
		{
			get
			{
				return _plateOffset;
			}
			set
			{
				if (_plateOffset == value)
					return;
				_plateOffset = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlateOffset)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal PlateOffsetX
		{
			get => PlateOffset.X;
			set
			{
				PlateOffset.X = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlateOffset)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal PlateOffsetY
		{
			get => PlateOffset.Y;
			set
			{
				PlateOffset.Y = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlateOffset)));
			}
		}
		public bool SaveAssayFiles
		{
			get
			{
				return _saveAssayFiles;
			}
			set
			{
				if (_saveAssayFiles == value)
					return;
				_saveAssayFiles = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SaveAssayFiles)));
			}
		}
		public int SecondaryBasalFluidPrimingVolume
		{
			get
			{
				return _secondaryBasalFluidPrimingVolume;
			}
			set
			{
				if (_secondaryBasalFluidPrimingVolume == value)
					return;
				_secondaryBasalFluidPrimingVolume = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SecondaryBasalFluidPrimingVolume)));
			}
		}
		public int SecondaryDnaFluidPrimingVolume
		{
			get
			{
				return _secondaryDnaFluidPrimingVolume;
			}
			set
			{
				if (_secondaryDnaFluidPrimingVolume == value)
					return;
				_secondaryDnaFluidPrimingVolume = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SecondaryDnaFluidPrimingVolume)));
			}
		}
		public int SecondaryKohFluidPrimingVolume
		{
			get
			{
				return _secondaryKohFluidPrimingVolume;
			}
			set
			{
				if (_secondaryKohFluidPrimingVolume == value)
					return;
				_secondaryKohFluidPrimingVolume = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SecondaryKohFluidPrimingVolume)));
			}
		}
		public int SecondaryNadhFluidPrimingVolume
		{
			get
			{
				return _secondaryNadhFluidPrimingVolume;
			}
			set
			{
				if (_secondaryNadhFluidPrimingVolume == value)
					return;
				_secondaryNadhFluidPrimingVolume = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SecondaryNadhFluidPrimingVolume)));
			}
		}
		public int SensorReadDwell
		{
			get
			{
				return _sensorReadDwell;
			}
			set
			{
				if (_sensorReadDwell == value)
					return;
				_sensorReadDwell = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SensorReadDwell)));
			}
		}
		public int StandardsProcessingDwell
		{
			get
			{
				return _standardsProcessingDwell;
			}
			set
			{
				if (_standardsProcessingDwell == value)
					return;
				_standardsProcessingDwell = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StandardsProcessingDwell)));
			}
		}
		public int StandardsWellCount
		{
			get
			{
				return _standardsWellCount;
			}
			set
			{
				if (_standardsWellCount == value)
					return;
				_standardsWellCount = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StandardsWellCount)));
			}
		}
		public int TableAxisSpeed
		{
			get
			{
				return _tableAxisSpeed;
			}
			set
			{
				if (_tableAxisSpeed == value)
					return;
				_tableAxisSpeed = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableAxisSpeed)));
			}
		}
		public int TestMediaAspirationVolume
		{
			get
			{
				return _testMediaAspirationVolume;
			}
			set
			{
				if (_testMediaAspirationVolume == value)
					return;
				_testMediaAspirationVolume = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TestMediaAspirationVolume)));
			}
		}
		public int TimerDescriptionUpdateInterval
		{
			get
			{
				return _timerDescriptionUpdateInterval;
			}
			set
			{
				if (_timerDescriptionUpdateInterval == value)
					return;
				_timerDescriptionUpdateInterval = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimerDescriptionUpdateInterval)));
			}
		}
		public Coordinate TrayLoadingLocation
		{
			get
			{
				return _trayLoadingLocation;
			}
			set
			{
				if (_trayLoadingLocation == value)
					return;
				_trayLoadingLocation = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrayLoadingLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal TrayLoadingLocationX
		{
			get => TrayLoadingLocation.X;
			set
			{
				TrayLoadingLocation.X = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrayLoadingLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal TrayLoadingLocationY
		{
			get => TrayLoadingLocation.Y;
			set
			{
				TrayLoadingLocation.Y = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrayLoadingLocation)));
			}
		}
		public Coordinate TraySterilizationLocation
		{
			get
			{
				return _traySterilizationLocation;
			}
			set
			{
				if (_traySterilizationLocation == value)
					return;
				_traySterilizationLocation = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TraySterilizationLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal TraySterilizationLocationX
		{
			get => TraySterilizationLocation.X;
			set
			{
				TraySterilizationLocation.X = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TraySterilizationLocation)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal TraySterilizationLocationY
		{
			get => TraySterilizationLocation.Y;
			set
			{
				TraySterilizationLocation.Y = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TraySterilizationLocation)));
			}
		}
		public int TraySterilizationTime
		{
			get
			{
				return _traySterilizationTime;
			}
			set
			{
				if (_traySterilizationTime == value)
					return;
				_traySterilizationTime = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TraySterilizationTime)));
			}
		}
		public decimal TrayTemperature
		{
			get
			{
				return _trayTemperature;
			}
			set
			{
				if (_trayTemperature == value)
					return;
				_trayTemperature = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrayTemperature)));
			}
		}
		public int VerticalWellCount
		{
			get
			{
				return _verticalWellCount;
			}
			set
			{
				if (_verticalWellCount == value)
					return;
				_verticalWellCount = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VerticalWellCount)));
			}
		}
		public Coordinate WasteLocation
		{
			get
			{
				return _wasteLocation;
			}
			set
			{
				if (_wasteLocation == value)
					return;
				_wasteLocation = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WasteLocation)));
			}
		}
		public decimal WellDepth
		{
			get
			{
				return _wellDepth;
			}
			set
			{
				if (_wellDepth == value)
					return;
				_wellDepth = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WellDepth)));
			}
		}
		public Coordinate WellOffset
		{
			get
			{
				return _wellOffset;
			}
			set
			{
				if (_wellOffset == value)
					return;
				_wellOffset = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WellOffset)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal WellOffsetX
		{
			get => WellOffset.X;
			set
			{
				WellOffset.X = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WellOffset)));
			}
		}
		[JsonIgnore]
		[NotMapped]
		public decimal WellOffsetY
		{
			get => WellOffset.Y;
			set
			{
				WellOffset.Y = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WellOffset)));
			}
		}
		public decimal WellWidth
		{
			get
			{
				return _wellWidth;
			}
			set
			{
				if (_wellWidth == value)
					return;
				_wellWidth = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WellWidth)));
			}
		}
		#endregion

		#region Events
		public event PropertyChangedEventHandler? PropertyChanged;
		#endregion

		#region Fields
		DateTime VersionDate { get; set; }

		decimal _aspirateKohActuatorStartPosition = 470m;
		decimal _aspirateKohDistanceToPlate = 14.5m;
		Coordinate _aspirateKohLocation = new(326, 207);
		decimal _aspiratePositioningSpeed = 200m;
		decimal _aspirateStirSpeed = 200m;
		decimal _aspirateTravelPerMicroLiter = 0.00156m;
		int _aspirationDwell = 15000;
		int _aspirationSuctionToggleBoardNumber = 1;
		int _aspirationSuctionTogglePortNumber = 1;
		int _basalIncubationTimerPeriod = 3600000;
		Coordinate _basalLocation = new(326, 207);
		decimal _basalSyringeLoadingPosition = 102m;
		decimal _basalTravelPerMicroLiter = 0.00153m;
		int _basalVolume = 300;
		Coordinate _biopsyLocation = new(326, 207);
		int _comPortNumber = 4;
		int _dnaAspirationVolume = 350;
		Coordinate _dnaLocation = new(326, 207);
		int _dnaSensorBoardNumber = 2;
		Coordinate _dnaSensorLocation = new(328, 379);
		int _dnaSensorNumber = 0;
		int _dnaSensorPortNumber = 6;
		int _dnaSensorReadHigh = 10000;
		int _dnaSensorReadLow = 0;
		decimal _dnaSyringeLoadingPosition = 102m;
		int _dnaTimerPeriod = 180000;
		decimal _dnaTravelPerMicroLiter = 0.00153m;
		int _dnaVolume = 200;
		string _effaniApiUrl = "https://www.effani.com";
		string _effaniPassword;
		string _effaniUsername;
		Coordinate _firstWellLocation = new(0, 0);
		int _fluidEmissionDwell = 6000;
		decimal _fluidSyringeSpeed = 5m;
		int _horizontalWellCount = 8;
		int _initialBasalFluidPrimingVolume = 1500;
		int _initialDnaFluidPrimingVolume = 1500;
		int _initialKohFluidPrimingVolume = 1500;
		int _initialNadhFluidPrimingVolume = 300;
		int _kohAspirationVolume = 250;
		Coordinate _kohLocation = new(326, 207);
		decimal _kohSyringeLoadingPosition = 102m;
		int _kohTimerPeriod = 14400000;
		decimal _kohTravelPerMicroLiter = 0.00153m;
		int _kohVolume = 300;
		int _nadhIncubationDwell = 60000;
		Coordinate _nadhLocation = new(326, 207);
		int _nadhReading1TimerPeriod = 600000;
		int _nadhReadingsTimerPeriod = 3600000;
		int _nadhSensorBoardNumber = 2;
		Coordinate _nadhSensorLocation = new(169.5m, 381);
		int _nadhSensorNumber = 1;
		int _nadhSensorPortNumber = 6;
		int _nadhSensorReadHigh = 10000;
		int _nadhSensorReadLow = 0;
		decimal _nadhSyringeLoadingPosition = 102m;
		decimal _nadhTravelPerMicroLiter = 0.00153m;
		int _nadhVolume = 300;
		Coordinate _plateOffset = new(90, 0);
		bool _saveAssayFiles = true;
		int _secondaryBasalFluidPrimingVolume = 200;
		int _secondaryDnaFluidPrimingVolume = 200;
		int _secondaryKohFluidPrimingVolume = 200;
		int _secondaryNadhFluidPrimingVolume = 200;
		int _sensorReadDwell = 10000;
		int _standardsProcessingDwell = 180000;
		int _standardsWellCount = 6;
		int _tableAxisSpeed = 4000;
		int _testMediaAspirationVolume = 350;
		int _timerDescriptionUpdateInterval = 1000;
		Coordinate _trayLoadingLocation = new(0, 0);
		Coordinate _traySterilizationLocation = new(320, 20);
		int _traySterilizationTime = 1500;
		decimal _trayTemperature = 38.777778m;
		int _verticalWellCount = 12;
		Coordinate _wasteLocation = new(326, 207);
		decimal _wellDepth = 12m;
		Coordinate _wellOffset = new(326, 207);
		decimal _wellWidth = 6.67m;
		#endregion
	}
}
