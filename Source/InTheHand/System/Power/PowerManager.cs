﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowerManager.cs" company="In The Hand Ltd">
//   Copyright (c) 2016 In The Hand Ltd, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Reflection;

namespace InTheHand.System.Power
{
    /// <summary>
    /// Provides information about the status of the device's battery.
    /// <para>Not supported for apps deployed via the public Windows Store to Windows 8.1.</para>
    /// </summary>
    public static class PowerManager
    {

#if __IOS__
        private static UIKit.UIDevice _device;
        private static bool _isSimulator;
#elif WINDOWS_APP
        private static bool _on10 = false;
        private static Type _type10 = null;
#elif WINDOWS_PHONE_APP || WINDOWS_PHONE
        private static Windows.Phone.Devices.Power.Battery _battery = Windows.Phone.Devices.Power.Battery.GetDefault();
#endif

#if WINDOWS_APP || __IOS__
        static PowerManager()
        {
#if __IOS__
            _device = UIKit.UIDevice.CurrentDevice;
            if (_device.Model == "iPhone Simulator")
            {
                _isSimulator = true;
            }
#elif WINDOWS_APP
            //check for 10
           _type10 = Type.GetType("Windows.System.Power.PowerManager, Windows, ContentType=WindowsRuntime");
            if(_type10 != null)
            {
                _on10 = true;
            }
#endif
        }
#endif

        /// <summary>
        /// Gets the device's battery status.
        /// </summary>
        public static BatteryStatus BatteryStatus
        {
            get
            {
                return BatteryStatus.Idle;
            }
        }

#if WINDOWS_PHONE_APP || WINDOWS_PHONE_81
        /// <summary>
        /// Gets battery saver status, indicating when to save energy.
        /// </summary>
        public static EnergySaverStatus EnergySaverStatus
        {
            get
            {
                bool saverEnabled = Windows.Phone.System.Power.PowerManager.PowerSavingModeEnabled;
                if (saverEnabled)
                {
                    bool saverOn = Windows.Phone.System.Power.PowerManager.PowerSavingMode == Windows.Phone.System.Power.PowerSavingMode.On;
                    return saverOn ? EnergySaverStatus.On : EnergySaverStatus.Off;
                }
                else
                {
                    return EnergySaverStatus.Disabled;
                }
            }
        }
#endif

        /// <summary>
        /// Gets the total percentage of charge remaining from all batteries connected to the device.
        /// <para>Not supported for Windows 8.1 apps deployed via the public Windows Store.</para>
        /// </summary>
        /// <value>The total percentage of charge remaining from all batteries connected to the device.</value>
        public static int RemainingChargePercent
        {
            get
            {
#if __IOS__
                if (_isSimulator)
                {
                    return 100;
                }

                double percent = _device.BatteryLevel;
                return Convert.ToInt32(percent * 100f);
#elif WINDOWS_APP
                //TODO: add reflection support for Windows 10
                if (_on10)
                {
                    return (int)_type10.GetRuntimeProperty("RemainingChargePercent").GetValue(null);
                }
                else
                {
                    NativeMethods.SYSTEM_POWER_STATUS status;
                    bool success = NativeMethods.GetSystemPowerStatus(out status);
                    return status.BatteryLifePercent;
                }
#elif WINDOWS_PHONE_APP || WINDOWS_PHONE
                return _battery.RemainingChargePercent;
#else
                throw new PlatformNotSupportedException();
#endif
            }
        }

#if !WINDOWS_APP
        private static event EventHandler<object> _remainingChargePercentChanged;
#endif

        /// <summary>
        /// Occurs when <see cref="RemainingChargePercent"/>changes.
        /// </summary>
        /// <remarks>Not supported on Windows 8.1</remarks>
        public static event EventHandler<object> RemainingChargePercentChanged
#if WINDOWS_APP
            ;
#else
        {
            add
            {
                if (_remainingChargePercentChanged == null)
                {
#if __IOS__
                    if(!_isSimulator && !_device.BatteryMonitoringEnabled)
                    {
                        _device.BatteryMonitoringEnabled = true;
                        UIKit.UIDevice.Notifications.ObserveBatteryLevelDidChange(BatteryLevelDidChangeHandler);
                    }
#elif WINDOWS_PHONE_APP || WINDOWS_PHONE
                    _battery.RemainingChargePercentChanged += _battery_RemainingChargePercentChanged;
#endif
                }
                _remainingChargePercentChanged += value;
            }
            remove
            {
                _remainingChargePercentChanged -= value;

                if(_remainingChargePercentChanged == null)
                {
#if __IOS__
                    _device.BatteryMonitoringEnabled = false;
#elif WINDOWS_PHONE_APP || WINDOWS_PHONE
                    _battery.RemainingChargePercentChanged -= _battery_RemainingChargePercentChanged;
#endif
                }
            }
        }
#endif

#if WINDOWS_PHONE_APP || WINDOWS_PHONE
        private static void _battery_RemainingChargePercentChanged(object sender, object e)
        {
            if(_remainingChargePercentChanged != null)
            {
                _remainingChargePercentChanged(null, e);
            }
        }
#elif __IOS__
        private static void BatteryLevelDidChangeHandler(object sender, global::Foundation.NSNotificationEventArgs e)
        {
            if(_remainingChargePercentChanged != null)
            {
                _remainingChargePercentChanged(null, null);
            }
        }
#endif


#if WINDOWS_APP
        /// <summary>
        /// Gets a value that estimates how long is left until the device's battery is fully discharged.
        /// <para>Not supported for apps deployed on Windows 8.1 from the public Windows Store.</para>
        /// </summary>
        public static TimeSpan RemainingDischargeTime
        {
            get
            {
                if (_on10)
                {
                    return (TimeSpan)_type10.GetRuntimeProperty("RemainingDischargeTime").GetValue(null);
                }
                else
                {
                    NativeMethods.SYSTEM_POWER_STATUS status;
                    bool success = NativeMethods.GetSystemPowerStatus(out status);
                    int seconds = status.BatteryLifeTime;
                    if (seconds == -1)
                    {
                        return TimeSpan.Zero;
                    }

                    return TimeSpan.FromSeconds(seconds);
                }
            }
        }

        private static class NativeMethods
        {
            [global::System.Runtime.InteropServices.DllImport("kernel32.dll")]
            internal static extern bool GetSystemPowerStatus(out SYSTEM_POWER_STATUS lpSystemPowerStatus);

            internal struct SYSTEM_POWER_STATUS
            {
                public byte ACLineStatus;
                public byte BatteryFlag;
                public byte BatteryLifePercent;
                public byte Reserved1;            // set to 0
                public int BatteryLifeTime;
                public int BatteryFullLifeTime;
            }
        }
#endif
    }

    /// <summary>
    /// Indicates the status of the battery.
    /// </summary>
    public enum BatteryStatus
    {
        /// <summary>
        /// The battery or battery controller is not present.
        /// </summary>
        NotPresent = 0,
        /// <summary>
        /// The battery is discharging. 
        /// </summary>
        Discharging = 1,
        /// <summary>
        /// The battery is idle.
        /// </summary>
        Idle = 2,
        /// <summary>
        /// The battery is charging.
        /// </summary>
        Charging = 3,
    }
    /// <summary>
    /// Specifies the status of battery saver.
    /// </summary>
    public enum EnergySaverStatus
    {
        /// <summary>
        /// Battery saver is off permanently or the device is plugged in.
        /// </summary>
        Disabled = 0,
        /// <summary>
        /// Battery saver is off now, but ready to turn on automatically. 
        /// </summary>
        Off = 1,
        /// <summary>
        /// Battery saver is on. Save energy where possible. 
        /// </summary>
        On = 2,
    }

    /// <summary>
    /// Represents the device's power supply status.
    /// </summary>
    /// <remarks>An Inadequate status occurs when the power supply is present, but the charge rate is negative.
    /// For example, the device is plugged in, but it’s losing charge.</remarks>
    public enum PowerSupplyStatus
    {
        /// <summary>
        /// The device has no power supply. 
        /// </summary>
        NotPresent = 0,
        /// <summary>
        /// The device has an inadequate power supply. 
        /// </summary>
        Inadequate = 1,
        /// <summary>
        /// The device has an adequate power supply. 
        /// </summary>
        Adequate = 2,
    }
}
