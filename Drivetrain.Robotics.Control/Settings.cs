using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Drivetrain.Robotics.Control
{
    [Owned]
    public class Settings : INotifyPropertyChanged
    {
        #region Properties
        public decimal AspirateActuatorStartPosition
        {
            get
            {
                return _aspirateActuatorStartPosition;
            }
            set
            {
                if (_aspirateActuatorStartPosition == value)
                    return;
                _aspirateActuatorStartPosition = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateActuatorStartPosition)));
            }
        }
        public decimal AspirateDistanceToPlate
        {
            get
            {
                return _aspirateDistanceToPlate;
            }
            set
            {
                if (_aspirateDistanceToPlate == value)
                    return;
                _aspirateDistanceToPlate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateDistanceToPlate)));
            }
        }
        public Coordinate AspirateLocation
        {
            get
            {
                return _aspirateLocation;
            }
            set
            {
                if (_aspirateLocation == value)
                    return;
                _aspirateLocation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateLocation)));
            }
        }
        [JsonIgnore]
        [NotMapped]
        public decimal AspirateLocationX
        {
            get => AspirateLocation.X;
            set
            {
                AspirateLocation.X = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateLocation)));
            }
        }
        [JsonIgnore]
        [NotMapped]
        public decimal AspirateLocationY
        {
            get => AspirateLocation.Y;
            set
            {
                AspirateLocation.Y = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateLocation)));
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
        public int ControllerBoardCount
        {
            get
            {
                return _controllerBoardCount;
            }
            set
            {
                if (_controllerBoardCount == value)
                    return;
                _controllerBoardCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ControllerBoardCount)));
            }
        }
        /// <summary>Number of IO input ports (io0.in to io{N-1}.in) on each controller board.</summary>
        public int InputPortsPerBoard
        {
            get
            {
                return _inputPortsPerBoard;
            }
            set
            {
                if (_inputPortsPerBoard == value)
                    return;
                _inputPortsPerBoard = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InputPortsPerBoard)));
            }
        }
        // Direction of motor movement for each axis (written to config.g as the S parameter of M569)
        public Direction TableXDirection
        {
            get
            {
                return _tableXDirection;
            }
            set
            {
                if (_tableXDirection == value)
                    return;
                _tableXDirection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableXDirection)));
            }
        }
        public Direction TableYDirection
        {
            get
            {
                return _tableYDirection;
            }
            set
            {
                if (_tableYDirection == value)
                    return;
                _tableYDirection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableYDirection)));
            }
        }
        public Direction AspirateDirection
        {
            get
            {
                return _aspirateDirection;
            }
            set
            {
                if (_aspirateDirection == value)
                    return;
                _aspirateDirection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateDirection)));
            }
        }
        public Direction DnaReagentDirection
        {
            get
            {
                return _dnaReagentDirection;
            }
            set
            {
                if (_dnaReagentDirection == value)
                    return;
                _dnaReagentDirection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaReagentDirection)));
            }
        }
        public Direction KohDirection
        {
            get
            {
                return _kohDirection;
            }
            set
            {
                if (_kohDirection == value)
                    return;
                _kohDirection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KohDirection)));
            }
        }
        public Direction NadhDirection
        {
            get
            {
                return _nadhDirection;
            }
            set
            {
                if (_nadhDirection == value)
                    return;
                _nadhDirection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhDirection)));
            }
        }
        public Direction BasalDirection
        {
            get
            {
                return _basalDirection;
            }
            set
            {
                if (_basalDirection == value)
                    return;
                _basalDirection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BasalDirection)));
            }
        }
        // Which end of each axis's travel the endstop is at (sets the M574 end and the homing direction)
        public EndstopLocation TableXEndstopLocation
        {
            get
            {
                return _tableXEndstopLocation;
            }
            set
            {
                if (_tableXEndstopLocation == value)
                    return;
                _tableXEndstopLocation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableXEndstopLocation)));
            }
        }
        public EndstopLocation TableYEndstopLocation
        {
            get
            {
                return _tableYEndstopLocation;
            }
            set
            {
                if (_tableYEndstopLocation == value)
                    return;
                _tableYEndstopLocation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableYEndstopLocation)));
            }
        }
        public EndstopLocation AspirateEndstopLocation
        {
            get
            {
                return _aspirateEndstopLocation;
            }
            set
            {
                if (_aspirateEndstopLocation == value)
                    return;
                _aspirateEndstopLocation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateEndstopLocation)));
            }
        }
        public EndstopLocation DnaReagentEndstopLocation
        {
            get
            {
                return _dnaReagentEndstopLocation;
            }
            set
            {
                if (_dnaReagentEndstopLocation == value)
                    return;
                _dnaReagentEndstopLocation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaReagentEndstopLocation)));
            }
        }
        public EndstopLocation KohEndstopLocation
        {
            get
            {
                return _kohEndstopLocation;
            }
            set
            {
                if (_kohEndstopLocation == value)
                    return;
                _kohEndstopLocation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KohEndstopLocation)));
            }
        }
        public EndstopLocation NadhEndstopLocation
        {
            get
            {
                return _nadhEndstopLocation;
            }
            set
            {
                if (_nadhEndstopLocation == value)
                    return;
                _nadhEndstopLocation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhEndstopLocation)));
            }
        }
        public EndstopLocation BasalEndstopLocation
        {
            get
            {
                return _basalEndstopLocation;
            }
            set
            {
                if (_basalEndstopLocation == value)
                    return;
                _basalEndstopLocation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BasalEndstopLocation)));
            }
        }
        // Motion parameters of the fluid pump actuators (written to config.g as M92, M208, M906, M203, M201 and M566)
        public decimal DnaReagentStepsPerMillimeter
        {
            get
            {
                return _dnaReagentStepsPerMillimeter;
            }
            set
            {
                if (_dnaReagentStepsPerMillimeter == value)
                    return;
                _dnaReagentStepsPerMillimeter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaReagentStepsPerMillimeter)));
            }
        }
        public int DnaReagentTravelLength
        {
            get
            {
                return _dnaReagentTravelLength;
            }
            set
            {
                if (_dnaReagentTravelLength == value)
                    return;
                _dnaReagentTravelLength = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaReagentTravelLength)));
            }
        }
        public int DnaReagentMotorCurrent
        {
            get
            {
                return _dnaReagentMotorCurrent;
            }
            set
            {
                if (_dnaReagentMotorCurrent == value)
                    return;
                _dnaReagentMotorCurrent = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaReagentMotorCurrent)));
            }
        }
        public int DnaReagentMaxSpeed
        {
            get
            {
                return _dnaReagentMaxSpeed;
            }
            set
            {
                if (_dnaReagentMaxSpeed == value)
                    return;
                _dnaReagentMaxSpeed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaReagentMaxSpeed)));
            }
        }
        public int DnaReagentAcceleration
        {
            get
            {
                return _dnaReagentAcceleration;
            }
            set
            {
                if (_dnaReagentAcceleration == value)
                    return;
                _dnaReagentAcceleration = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaReagentAcceleration)));
            }
        }
        public int DnaReagentMaxInstantaneousSpeedChange
        {
            get
            {
                return _dnaReagentMaxInstantaneousSpeedChange;
            }
            set
            {
                if (_dnaReagentMaxInstantaneousSpeedChange == value)
                    return;
                _dnaReagentMaxInstantaneousSpeedChange = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaReagentMaxInstantaneousSpeedChange)));
            }
        }
        public decimal KohStepsPerMillimeter
        {
            get
            {
                return _kohStepsPerMillimeter;
            }
            set
            {
                if (_kohStepsPerMillimeter == value)
                    return;
                _kohStepsPerMillimeter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KohStepsPerMillimeter)));
            }
        }
        public int KohTravelLength
        {
            get
            {
                return _kohTravelLength;
            }
            set
            {
                if (_kohTravelLength == value)
                    return;
                _kohTravelLength = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KohTravelLength)));
            }
        }
        public int KohMotorCurrent
        {
            get
            {
                return _kohMotorCurrent;
            }
            set
            {
                if (_kohMotorCurrent == value)
                    return;
                _kohMotorCurrent = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KohMotorCurrent)));
            }
        }
        public int KohMaxSpeed
        {
            get
            {
                return _kohMaxSpeed;
            }
            set
            {
                if (_kohMaxSpeed == value)
                    return;
                _kohMaxSpeed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KohMaxSpeed)));
            }
        }
        public int KohAcceleration
        {
            get
            {
                return _kohAcceleration;
            }
            set
            {
                if (_kohAcceleration == value)
                    return;
                _kohAcceleration = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KohAcceleration)));
            }
        }
        public int KohMaxInstantaneousSpeedChange
        {
            get
            {
                return _kohMaxInstantaneousSpeedChange;
            }
            set
            {
                if (_kohMaxInstantaneousSpeedChange == value)
                    return;
                _kohMaxInstantaneousSpeedChange = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KohMaxInstantaneousSpeedChange)));
            }
        }
        public decimal NadhStepsPerMillimeter
        {
            get
            {
                return _nadhStepsPerMillimeter;
            }
            set
            {
                if (_nadhStepsPerMillimeter == value)
                    return;
                _nadhStepsPerMillimeter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhStepsPerMillimeter)));
            }
        }
        public int NadhTravelLength
        {
            get
            {
                return _nadhTravelLength;
            }
            set
            {
                if (_nadhTravelLength == value)
                    return;
                _nadhTravelLength = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhTravelLength)));
            }
        }
        public int NadhMotorCurrent
        {
            get
            {
                return _nadhMotorCurrent;
            }
            set
            {
                if (_nadhMotorCurrent == value)
                    return;
                _nadhMotorCurrent = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhMotorCurrent)));
            }
        }
        public int NadhMaxSpeed
        {
            get
            {
                return _nadhMaxSpeed;
            }
            set
            {
                if (_nadhMaxSpeed == value)
                    return;
                _nadhMaxSpeed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhMaxSpeed)));
            }
        }
        public int NadhAcceleration
        {
            get
            {
                return _nadhAcceleration;
            }
            set
            {
                if (_nadhAcceleration == value)
                    return;
                _nadhAcceleration = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhAcceleration)));
            }
        }
        public int NadhMaxInstantaneousSpeedChange
        {
            get
            {
                return _nadhMaxInstantaneousSpeedChange;
            }
            set
            {
                if (_nadhMaxInstantaneousSpeedChange == value)
                    return;
                _nadhMaxInstantaneousSpeedChange = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhMaxInstantaneousSpeedChange)));
            }
        }
        public decimal BasalStepsPerMillimeter
        {
            get
            {
                return _basalStepsPerMillimeter;
            }
            set
            {
                if (_basalStepsPerMillimeter == value)
                    return;
                _basalStepsPerMillimeter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BasalStepsPerMillimeter)));
            }
        }
        public int BasalTravelLength
        {
            get
            {
                return _basalTravelLength;
            }
            set
            {
                if (_basalTravelLength == value)
                    return;
                _basalTravelLength = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BasalTravelLength)));
            }
        }
        public int BasalMotorCurrent
        {
            get
            {
                return _basalMotorCurrent;
            }
            set
            {
                if (_basalMotorCurrent == value)
                    return;
                _basalMotorCurrent = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BasalMotorCurrent)));
            }
        }
        public int BasalMaxSpeed
        {
            get
            {
                return _basalMaxSpeed;
            }
            set
            {
                if (_basalMaxSpeed == value)
                    return;
                _basalMaxSpeed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BasalMaxSpeed)));
            }
        }
        public int BasalAcceleration
        {
            get
            {
                return _basalAcceleration;
            }
            set
            {
                if (_basalAcceleration == value)
                    return;
                _basalAcceleration = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BasalAcceleration)));
            }
        }
        public int BasalMaxInstantaneousSpeedChange
        {
            get
            {
                return _basalMaxInstantaneousSpeedChange;
            }
            set
            {
                if (_basalMaxInstantaneousSpeedChange == value)
                    return;
                _basalMaxInstantaneousSpeedChange = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BasalMaxInstantaneousSpeedChange)));
            }
        }
        // Motion parameters of the table axes (written to config.g as M92, M208, M906, M201 and M566)
        public decimal TableXStepsPerMillimeter
        {
            get
            {
                return _tableXStepsPerMillimeter;
            }
            set
            {
                if (_tableXStepsPerMillimeter == value)
                    return;
                _tableXStepsPerMillimeter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableXStepsPerMillimeter)));
            }
        }
        public int TableXTravelLength
        {
            get
            {
                return _tableXTravelLength;
            }
            set
            {
                if (_tableXTravelLength == value)
                    return;
                _tableXTravelLength = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableXTravelLength)));
            }
        }
        public int TableXMotorCurrent
        {
            get
            {
                return _tableXMotorCurrent;
            }
            set
            {
                if (_tableXMotorCurrent == value)
                    return;
                _tableXMotorCurrent = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableXMotorCurrent)));
            }
        }
        public int TableXAcceleration
        {
            get
            {
                return _tableXAcceleration;
            }
            set
            {
                if (_tableXAcceleration == value)
                    return;
                _tableXAcceleration = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableXAcceleration)));
            }
        }
        public int TableXMaxInstantaneousSpeedChange
        {
            get
            {
                return _tableXMaxInstantaneousSpeedChange;
            }
            set
            {
                if (_tableXMaxInstantaneousSpeedChange == value)
                    return;
                _tableXMaxInstantaneousSpeedChange = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableXMaxInstantaneousSpeedChange)));
            }
        }
        public decimal TableYStepsPerMillimeter
        {
            get
            {
                return _tableYStepsPerMillimeter;
            }
            set
            {
                if (_tableYStepsPerMillimeter == value)
                    return;
                _tableYStepsPerMillimeter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableYStepsPerMillimeter)));
            }
        }
        public int TableYTravelLength
        {
            get
            {
                return _tableYTravelLength;
            }
            set
            {
                if (_tableYTravelLength == value)
                    return;
                _tableYTravelLength = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableYTravelLength)));
            }
        }
        public int TableYMotorCurrent
        {
            get
            {
                return _tableYMotorCurrent;
            }
            set
            {
                if (_tableYMotorCurrent == value)
                    return;
                _tableYMotorCurrent = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableYMotorCurrent)));
            }
        }
        public int TableYAcceleration
        {
            get
            {
                return _tableYAcceleration;
            }
            set
            {
                if (_tableYAcceleration == value)
                    return;
                _tableYAcceleration = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableYAcceleration)));
            }
        }
        public int TableYMaxInstantaneousSpeedChange
        {
            get
            {
                return _tableYMaxInstantaneousSpeedChange;
            }
            set
            {
                if (_tableYMaxInstantaneousSpeedChange == value)
                    return;
                _tableYMaxInstantaneousSpeedChange = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableYMaxInstantaneousSpeedChange)));
            }
        }
        // Motion parameters of the aspirate needle actuator (written to config.g as M92, M208, M906, M203, M201 and M566)
        public decimal AspirateStepsPerMillimeter
        {
            get
            {
                return _aspirateStepsPerMillimeter;
            }
            set
            {
                if (_aspirateStepsPerMillimeter == value)
                    return;
                _aspirateStepsPerMillimeter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateStepsPerMillimeter)));
            }
        }
        public int AspirateTravelLength
        {
            get
            {
                return _aspirateTravelLength;
            }
            set
            {
                if (_aspirateTravelLength == value)
                    return;
                _aspirateTravelLength = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateTravelLength)));
            }
        }
        public int AspirateMotorCurrent
        {
            get
            {
                return _aspirateMotorCurrent;
            }
            set
            {
                if (_aspirateMotorCurrent == value)
                    return;
                _aspirateMotorCurrent = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateMotorCurrent)));
            }
        }
        public int AspirateMaxSpeed
        {
            get
            {
                return _aspirateMaxSpeed;
            }
            set
            {
                if (_aspirateMaxSpeed == value)
                    return;
                _aspirateMaxSpeed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateMaxSpeed)));
            }
        }
        public int AspirateAcceleration
        {
            get
            {
                return _aspirateAcceleration;
            }
            set
            {
                if (_aspirateAcceleration == value)
                    return;
                _aspirateAcceleration = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateAcceleration)));
            }
        }
        public int AspirateMaxInstantaneousSpeedChange
        {
            get
            {
                return _aspirateMaxInstantaneousSpeedChange;
            }
            set
            {
                if (_aspirateMaxInstantaneousSpeedChange == value)
                    return;
                _aspirateMaxInstantaneousSpeedChange = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateMaxInstantaneousSpeedChange)));
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
        public bool Debug
        {
            get
            {
#if DEBUG
				return true;
#endif
                return _debug;
            }
            set
            {
                if (_debug == value)
                    return;
                _debug = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Debug)));
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
                if (Debug)
                    return "https://localhost:44384/umbraco/api";

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
        /// <summary>Name of the WiFi network the controller board is configured with (M587).</summary>
        public string WifiSsid
        {
            get
            {
                return _wifiSsid;
            }
            set
            {
                if (_wifiSsid == value)
                    return;
                _wifiSsid = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WifiSsid)));
            }
        }
        public string WifiPassword
        {
            get
            {
                return _wifiPassword;
            }
            set
            {
                if (_wifiPassword == value)
                    return;
                _wifiPassword = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WifiPassword)));
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
                return _nadhBasalMixingTimerPeriod;
            }
            set
            {
                if (_nadhBasalMixingTimerPeriod == value)
                    return;
                _nadhBasalMixingTimerPeriod = value;
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
        public decimal ReprimePumpInterval
        {
            get
            {
                return _reprimePumpInterval;
            }
            set
            {
                if (_reprimePumpInterval == value)
                    return;
                _reprimePumpInterval = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReprimePumpInterval)));
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

        #region Board connections
        public int TableXDriverBoardNumber
        {
            get
            {
                return _tableXDriverBoardNumber;
            }
            set
            {
                if (_tableXDriverBoardNumber == value)
                    return;
                _tableXDriverBoardNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableXDriverBoardNumber)));
            }
        }
        public int TableXDriverPortNumber
        {
            get
            {
                return _tableXDriverPortNumber;
            }
            set
            {
                if (_tableXDriverPortNumber == value)
                    return;
                _tableXDriverPortNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableXDriverPortNumber)));
            }
        }
        public int TableXEndstopBoardNumber
        {
            get
            {
                return _tableXEndstopBoardNumber;
            }
            set
            {
                if (_tableXEndstopBoardNumber == value)
                    return;
                _tableXEndstopBoardNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableXEndstopBoardNumber)));
            }
        }
        public int TableXEndstopPortNumber
        {
            get
            {
                return _tableXEndstopPortNumber;
            }
            set
            {
                if (_tableXEndstopPortNumber == value)
                    return;
                _tableXEndstopPortNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableXEndstopPortNumber)));
            }
        }
        public int TableYDriverBoardNumber
        {
            get
            {
                return _tableYDriverBoardNumber;
            }
            set
            {
                if (_tableYDriverBoardNumber == value)
                    return;
                _tableYDriverBoardNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableYDriverBoardNumber)));
            }
        }
        public int TableYDriverPortNumber
        {
            get
            {
                return _tableYDriverPortNumber;
            }
            set
            {
                if (_tableYDriverPortNumber == value)
                    return;
                _tableYDriverPortNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableYDriverPortNumber)));
            }
        }
        public int TableYEndstopBoardNumber
        {
            get
            {
                return _tableYEndstopBoardNumber;
            }
            set
            {
                if (_tableYEndstopBoardNumber == value)
                    return;
                _tableYEndstopBoardNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableYEndstopBoardNumber)));
            }
        }
        public int TableYEndstopPortNumber
        {
            get
            {
                return _tableYEndstopPortNumber;
            }
            set
            {
                if (_tableYEndstopPortNumber == value)
                    return;
                _tableYEndstopPortNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableYEndstopPortNumber)));
            }
        }
        public int AspirateDriverBoardNumber
        {
            get
            {
                return _aspirateDriverBoardNumber;
            }
            set
            {
                if (_aspirateDriverBoardNumber == value)
                    return;
                _aspirateDriverBoardNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateDriverBoardNumber)));
            }
        }
        public int AspirateDriverPortNumber
        {
            get
            {
                return _aspirateDriverPortNumber;
            }
            set
            {
                if (_aspirateDriverPortNumber == value)
                    return;
                _aspirateDriverPortNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateDriverPortNumber)));
            }
        }
        public int AspirateEndstopBoardNumber
        {
            get
            {
                return _aspirateEndstopBoardNumber;
            }
            set
            {
                if (_aspirateEndstopBoardNumber == value)
                    return;
                _aspirateEndstopBoardNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateEndstopBoardNumber)));
            }
        }
        public int AspirateEndstopPortNumber
        {
            get
            {
                return _aspirateEndstopPortNumber;
            }
            set
            {
                if (_aspirateEndstopPortNumber == value)
                    return;
                _aspirateEndstopPortNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AspirateEndstopPortNumber)));
            }
        }
        public int DnaReagentDriverBoardNumber
        {
            get
            {
                return _dnaReagentDriverBoardNumber;
            }
            set
            {
                if (_dnaReagentDriverBoardNumber == value)
                    return;
                _dnaReagentDriverBoardNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaReagentDriverBoardNumber)));
            }
        }
        public int DnaReagentDriverPortNumber
        {
            get
            {
                return _dnaReagentDriverPortNumber;
            }
            set
            {
                if (_dnaReagentDriverPortNumber == value)
                    return;
                _dnaReagentDriverPortNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaReagentDriverPortNumber)));
            }
        }
        public int DnaReagentEndstopBoardNumber
        {
            get
            {
                return _dnaReagentEndstopBoardNumber;
            }
            set
            {
                if (_dnaReagentEndstopBoardNumber == value)
                    return;
                _dnaReagentEndstopBoardNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaReagentEndstopBoardNumber)));
            }
        }
        public int DnaReagentEndstopPortNumber
        {
            get
            {
                return _dnaReagentEndstopPortNumber;
            }
            set
            {
                if (_dnaReagentEndstopPortNumber == value)
                    return;
                _dnaReagentEndstopPortNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaReagentEndstopPortNumber)));
            }
        }
        public int KohDriverBoardNumber
        {
            get
            {
                return _kohDriverBoardNumber;
            }
            set
            {
                if (_kohDriverBoardNumber == value)
                    return;
                _kohDriverBoardNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KohDriverBoardNumber)));
            }
        }
        public int KohDriverPortNumber
        {
            get
            {
                return _kohDriverPortNumber;
            }
            set
            {
                if (_kohDriverPortNumber == value)
                    return;
                _kohDriverPortNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KohDriverPortNumber)));
            }
        }
        public int KohEndstopBoardNumber
        {
            get
            {
                return _kohEndstopBoardNumber;
            }
            set
            {
                if (_kohEndstopBoardNumber == value)
                    return;
                _kohEndstopBoardNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KohEndstopBoardNumber)));
            }
        }
        public int KohEndstopPortNumber
        {
            get
            {
                return _kohEndstopPortNumber;
            }
            set
            {
                if (_kohEndstopPortNumber == value)
                    return;
                _kohEndstopPortNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KohEndstopPortNumber)));
            }
        }
        public int NadhDriverBoardNumber
        {
            get
            {
                return _nadhDriverBoardNumber;
            }
            set
            {
                if (_nadhDriverBoardNumber == value)
                    return;
                _nadhDriverBoardNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhDriverBoardNumber)));
            }
        }
        public int NadhDriverPortNumber
        {
            get
            {
                return _nadhDriverPortNumber;
            }
            set
            {
                if (_nadhDriverPortNumber == value)
                    return;
                _nadhDriverPortNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhDriverPortNumber)));
            }
        }
        public int NadhEndstopBoardNumber
        {
            get
            {
                return _nadhEndstopBoardNumber;
            }
            set
            {
                if (_nadhEndstopBoardNumber == value)
                    return;
                _nadhEndstopBoardNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhEndstopBoardNumber)));
            }
        }
        public int NadhEndstopPortNumber
        {
            get
            {
                return _nadhEndstopPortNumber;
            }
            set
            {
                if (_nadhEndstopPortNumber == value)
                    return;
                _nadhEndstopPortNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhEndstopPortNumber)));
            }
        }
        public int BasalDriverBoardNumber
        {
            get
            {
                return _basalDriverBoardNumber;
            }
            set
            {
                if (_basalDriverBoardNumber == value)
                    return;
                _basalDriverBoardNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BasalDriverBoardNumber)));
            }
        }
        public int BasalDriverPortNumber
        {
            get
            {
                return _basalDriverPortNumber;
            }
            set
            {
                if (_basalDriverPortNumber == value)
                    return;
                _basalDriverPortNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BasalDriverPortNumber)));
            }
        }
        public int BasalEndstopBoardNumber
        {
            get
            {
                return _basalEndstopBoardNumber;
            }
            set
            {
                if (_basalEndstopBoardNumber == value)
                    return;
                _basalEndstopBoardNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BasalEndstopBoardNumber)));
            }
        }
        public int BasalEndstopPortNumber
        {
            get
            {
                return _basalEndstopPortNumber;
            }
            set
            {
                if (_basalEndstopPortNumber == value)
                    return;
                _basalEndstopPortNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BasalEndstopPortNumber)));
            }
        }
        public int TrayUvBoardNumber
        {
            get
            {
                return _trayUvBoardNumber;
            }
            set
            {
                if (_trayUvBoardNumber == value)
                    return;
                _trayUvBoardNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrayUvBoardNumber)));
            }
        }
        public int TrayUvPortNumber
        {
            get
            {
                return _trayUvPortNumber;
            }
            set
            {
                if (_trayUvPortNumber == value)
                    return;
                _trayUvPortNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrayUvPortNumber)));
            }
        }
        public int ChamberUvBoardNumber
        {
            get
            {
                return _chamberUvBoardNumber;
            }
            set
            {
                if (_chamberUvBoardNumber == value)
                    return;
                _chamberUvBoardNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChamberUvBoardNumber)));
            }
        }
        public int ChamberUvPortNumber
        {
            get
            {
                return _chamberUvPortNumber;
            }
            set
            {
                if (_chamberUvPortNumber == value)
                    return;
                _chamberUvPortNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChamberUvPortNumber)));
            }
        }
        public int NadhLaserBoardNumber
        {
            get
            {
                return _nadh532LaserBoardNumber;
            }
            set
            {
                if (_nadh532LaserBoardNumber == value)
                    return;
                _nadh532LaserBoardNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhLaserBoardNumber)));
            }
        }
        public int NadhLaserPortNumber
        {
            get
            {
                return _nadh532LaserPortNumber;
            }
            set
            {
                if (_nadh532LaserPortNumber == value)
                    return;
                _nadh532LaserPortNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NadhLaserPortNumber)));
            }
        }
        public int DnaLaserBoardNumber
        {
            get
            {
                return _dna488LaserBoardNumber;
            }
            set
            {
                if (_dna488LaserBoardNumber == value)
                    return;
                _dna488LaserBoardNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaLaserBoardNumber)));
            }
        }
        public int DnaLaserPortNumber
        {
            get
            {
                return _dna488LaserPortNumber;
            }
            set
            {
                if (_dna488LaserPortNumber == value)
                    return;
                _dna488LaserPortNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DnaLaserPortNumber)));
            }
        }
        #endregion
        #endregion

        #region Fields
        DateTime VersionDate { get; set; }

        decimal _aspirateActuatorStartPosition = 90m;
        decimal _aspirateDistanceToPlate = 11m;
        Coordinate _aspirateLocation = new(330, 203);
        decimal _aspiratePositioningSpeed = 200m;
        decimal _aspirateStirSpeed = 200m;
        decimal _aspirateTravelPerMicroLiter = 0.013m;
        int _aspirationDwell = 8000;
        int _aspirationSuctionToggleBoardNumber = 1;
        int _aspirationSuctionTogglePortNumber = 2;
        int _basalIncubationTimerPeriod = 900000;
        Coordinate _basalLocation = new(165, 257);
        decimal _basalSyringeLoadingPosition = 117m;
        decimal _basalTravelPerMicroLiter = 0.00153m;
        int _basalVolume = 300;
        Coordinate _biopsyLocation = new(245, 110);
        int _comPortNumber = 4;
        int _controllerBoardCount = 2;
        int _inputPortsPerBoard = 7;
        Direction _tableXDirection = Direction.Backwards;
        Direction _tableYDirection = Direction.Backwards;
        Direction _aspirateDirection = Direction.Forwards;
        Direction _dnaReagentDirection = Direction.Forwards;
        Direction _kohDirection = Direction.Forwards;
        Direction _nadhDirection = Direction.Forwards;
        Direction _basalDirection = Direction.Forwards;
        EndstopLocation _tableXEndstopLocation = EndstopLocation.LowEnd;
        EndstopLocation _tableYEndstopLocation = EndstopLocation.LowEnd;
        EndstopLocation _aspirateEndstopLocation = EndstopLocation.LowEnd;
        EndstopLocation _dnaReagentEndstopLocation = EndstopLocation.HighEnd;
        EndstopLocation _kohEndstopLocation = EndstopLocation.HighEnd;
        EndstopLocation _nadhEndstopLocation = EndstopLocation.HighEnd;
        EndstopLocation _basalEndstopLocation = EndstopLocation.HighEnd;
        decimal _dnaReagentStepsPerMillimeter = 1259.84m;
        int _dnaReagentTravelLength = 117;
        int _dnaReagentMotorCurrent = 1500;
        int _dnaReagentMaxSpeed = 2286;
        int _dnaReagentAcceleration = 500;
        int _dnaReagentMaxInstantaneousSpeedChange = 900;
        decimal _kohStepsPerMillimeter = 1259.84m;
        int _kohTravelLength = 117;
        int _kohMotorCurrent = 1500;
        int _kohMaxSpeed = 2286;
        int _kohAcceleration = 500;
        int _kohMaxInstantaneousSpeedChange = 900;
        decimal _nadhStepsPerMillimeter = 1259.84m;
        int _nadhTravelLength = 117;
        int _nadhMotorCurrent = 1500;
        int _nadhMaxSpeed = 2286;
        int _nadhAcceleration = 500;
        int _nadhMaxInstantaneousSpeedChange = 900;
        decimal _basalStepsPerMillimeter = 1259.84m;
        int _basalTravelLength = 117;
        int _basalMotorCurrent = 1500;
        int _basalMaxSpeed = 2286;
        int _basalAcceleration = 500;
        int _basalMaxInstantaneousSpeedChange = 900;
        decimal _aspirateStepsPerMillimeter = 1259.84m;
        int _aspirateTravelLength = 90;
        int _aspirateMotorCurrent = 500;
        int _aspirateMaxSpeed = 4000;
        int _aspirateAcceleration = 500;
        int _aspirateMaxInstantaneousSpeedChange = 900;
        decimal _tableXStepsPerMillimeter = 400m;
        int _tableXTravelLength = 500;
        int _tableXMotorCurrent = 400;
        int _tableXAcceleration = 500;
        int _tableXMaxInstantaneousSpeedChange = 900;
        decimal _tableYStepsPerMillimeter = 400m;
        int _tableYTravelLength = 500;
        int _tableYMotorCurrent = 400;
        int _tableYAcceleration = 500;
        int _tableYMaxInstantaneousSpeedChange = 900;
        bool _debug;
        int _dnaAspirationVolume = 50;
        Coordinate _dnaLocation = new(331, 304);
        Coordinate _dnaSensorLocation = new(329, 379);
        int _dnaSensorNumber = 0;
        int _dnaSensorReadHigh = 4095;
        int _dnaSensorReadLow = 0;
        decimal _dnaSyringeLoadingPosition = 117m;
        int _dnaTimerPeriod = 360000;
        decimal _dnaTravelPerMicroLiter = 0.00153m;
        int _dnaVolume = 200;
        string _effaniApiUrl = "https://www.effani.com/umbraco/api";
        string _effaniPassword;
        string _effaniUsername;
        string _wifiSsid = "";
        string _wifiPassword = "";
        Coordinate _firstWellLocation = new(0, 0);
        int _fluidEmissionDwell = 10000;
        decimal _fluidSyringeSpeed = 5m;
        int _horizontalWellCount = 8;
        int _initialBasalFluidPrimingVolume = 1000;
        int _initialDnaFluidPrimingVolume = 1000;
        int _initialKohFluidPrimingVolume = 100;
        int _initialNadhFluidPrimingVolume = 100;
        int _kohAspirationVolume = 50;
        Coordinate _kohLocation = new(331, 256);
        decimal _kohSyringeLoadingPosition = 117m;
        int _kohTimerPeriod = 480000;
        decimal _kohTravelPerMicroLiter = 0.00897m;
        int _kohVolume = 20;
        int _nadhIncubationDwell = 120000;
        Coordinate _nadhLocation = new(166, 304);
        int _nadhBasalMixingTimerPeriod = 360000;
        int _nadhReadingsTimerPeriod = 480000;
        Coordinate _nadhSensorLocation = new(168, 382.5m);
        int _nadhSensorNumber = 1;
        int _nadhSensorReadHigh = 4095;
        int _nadhSensorReadLow = 0;
        decimal _nadhSyringeLoadingPosition = 117m;
        decimal _nadhTravelPerMicroLiter = 0.00897m;
        int _nadhVolume = 20;
        Coordinate _plateOffset = new(87.48m, 0);
        bool _saveAssayFiles = false;
        int _secondaryBasalFluidPrimingVolume = 200;
        int _secondaryDnaFluidPrimingVolume = 200;
        int _secondaryKohFluidPrimingVolume = 20;
        int _secondaryNadhFluidPrimingVolume = 20;
        int _sensorReadDwell = 6000;
        int _standardsProcessingDwell = 360000;
        int _standardsWellCount = 6;
		decimal _reprimePumpInterval = 20;
        int _tableAxisSpeed = 4000;
        int _testMediaAspirationVolume = 50;
        int _timerDescriptionUpdateInterval = 1000;
        Coordinate _trayLoadingLocation = new(0, 0);
        Coordinate _traySterilizationLocation = new(320, 20);
        int _traySterilizationTime = 6;
        decimal _trayTemperature = 38.777778m;
        int _verticalWellCount = 12;
        Coordinate _wasteLocation = new(0, 0);
        decimal _wellDepth = 12m;
        Coordinate _wellOffset = new(9, 9);
        decimal _wellWidth = 6.67m;

        // Board connections (board 0 is the main board; expansion boards are numbered by CAN address)
        int _tableXDriverBoardNumber = 1;
        int _tableXDriverPortNumber = 0;
        int _tableXEndstopBoardNumber = 1;
        int _tableXEndstopPortNumber = 0;
        int _tableYDriverBoardNumber = 1;
        int _tableYDriverPortNumber = 2;
        int _tableYEndstopBoardNumber = 1;
        int _tableYEndstopPortNumber = 1;
        int _aspirateDriverBoardNumber = 0;
        int _aspirateDriverPortNumber = 0;
        int _aspirateEndstopBoardNumber = 0;
        int _aspirateEndstopPortNumber = 0;
        int _dnaReagentDriverBoardNumber = 0;
        int _dnaReagentDriverPortNumber = 4;
        int _dnaReagentEndstopBoardNumber = 0;
        int _dnaReagentEndstopPortNumber = 4;
        int _kohDriverBoardNumber = 0;
        int _kohDriverPortNumber = 2;
        int _kohEndstopBoardNumber = 0;
        int _kohEndstopPortNumber = 2;
        int _nadhDriverBoardNumber = 0;
        int _nadhDriverPortNumber = 1;
        int _nadhEndstopBoardNumber = 0;
        int _nadhEndstopPortNumber = 1;
        int _basalDriverBoardNumber = 0;
        int _basalDriverPortNumber = 3;
        int _basalEndstopBoardNumber = 0;
        int _basalEndstopPortNumber = 3;
        int _trayUvBoardNumber = 0;
        int _trayUvPortNumber = 6;
        int _chamberUvBoardNumber = 0;
        int _chamberUvPortNumber = 5;
        int _nadh532LaserBoardNumber = 0;
        int _nadh532LaserPortNumber = 2;
        int _nadhSensorBoardNumber = 0;
        int _nadhSensorPortNumber = 0;
        int _dna488LaserBoardNumber = 0;
        int _dna488LaserPortNumber = 1;
        int _dnaSensorBoardNumber = 0;
        int _dnaSensorPortNumber = 1;
        #endregion

        #region Events
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion
    }
}
