﻿using IOTLinkAPI.Configs;
using IOTLinkAPI.Platform.HomeAssistant;
using System.Collections.Generic;
using System.Windows.Forms;

namespace IOTLinkAddon.Service.Monitors
{
    class PowerMonitor : BaseMonitor
    {
        private static readonly string CONFIG_KEY = "Power";

        public override string GetConfigKey()
        {
            return CONFIG_KEY;
        }

        public override List<MonitorItem> GetMonitorItems(Configuration config, int interval)
        {
            List<MonitorItem> result = new List<MonitorItem>();

            PowerStatus powerStatus = SystemInformation.PowerStatus;

            // Power Status
            result.Add(new MonitorItem
            {
                ConfigKey = CONFIG_KEY,
                Type = MonitorItemType.TYPE_RAW,
                Topic = "Stats/Power/Status",
                Value = powerStatus.PowerLineStatus,
                DiscoveryOptions = new HassDiscoveryOptions()
                {
                    Component = HomeAssistantComponent.BinarySensor,
                    Name = "PowerStatus",
                    PayloadOff = "Offline",
                    PayloadOn = "Online",
                    DeviceClass = "plug"
                }
            });

            // Battery Status
            result.Add(new MonitorItem
            {
                ConfigKey = CONFIG_KEY,
                Type = MonitorItemType.TYPE_RAW,
                Topic = "Stats/Battery/Status",
                Value = powerStatus.BatteryChargeStatus,
                DiscoveryOptions = new HassDiscoveryOptions()
                {
                    Component = HomeAssistantComponent.BinarySensor,
                    Name = "BatteryStatus",
                    PayloadOff = "Offline",
                    PayloadOn = "Online",
                    DeviceClass = "plug"
                }
            });

            // Battery Full Lifetime
            result.Add(new MonitorItem
            {
                ConfigKey = CONFIG_KEY,
                Type = MonitorItemType.TYPE_RAW,
                Topic = "Stats/Battery/FullLifetime",
                Value = powerStatus.BatteryFullLifetime,
                DiscoveryOptions = new HassDiscoveryOptions()
                {
                    Component = HomeAssistantComponent.Sensor,
                    Name = "FullLifetime",
                    Unit = "minutes",
                    ValueTemplate = "{{ ( value | float / 60 ) | int }}",
                    Icon = "mdi:timer"
                }
            });

            // Battery Remaining Time
            result.Add(new MonitorItem
            {
                ConfigKey = CONFIG_KEY,
                Type = MonitorItemType.TYPE_RAW,
                Topic = "Stats/Battery/RemainingTime",
                Value = powerStatus.BatteryLifeRemaining,
                DiscoveryOptions = new HassDiscoveryOptions()
                {
                    Component = HomeAssistantComponent.Sensor,
                    Name = "RemainingTime",
                    Unit = "minutes",
                    ValueTemplate = "{{ ( value | float / 60 ) | int }}",
                    Icon = "mdi:timer"

                }
            });

            // Battery Remaining (%)
            result.Add(new MonitorItem
            {
                ConfigKey = CONFIG_KEY,
                Type = MonitorItemType.TYPE_RAW,
                Topic = "Stats/Battery/RemainingPercent",
                Value = (powerStatus.BatteryLifePercent * 100),
                DiscoveryOptions = new HassDiscoveryOptions()
                {
                    Component = HomeAssistantComponent.Sensor,
                    Name = "Remaining",
                    Unit = "%",
                    DeviceClass = "battery"
                }
            });

            return result;
        }
    }
}
