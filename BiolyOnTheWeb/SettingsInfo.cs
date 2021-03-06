﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiolyOnTheWeb
{
    public class SettingsInfo
    {
        private const char SETTINGS_DELIMITER = '\n';
        private const char SETTING_KEY_VALUE_DELIMITER = '=';

        private const string BOARD_WIDTH_SETTING_NAME = "boardWidthSetting";
        private const string BOARD_HEIGHT_SETTING_NAME = "boardHeightSetting";
        private const string COMMAND_FREQUENCY_SETTING_NAME = "commandFrequencySetting";
        private const string DROPLET_SPEED_SETTING_NAME = "dropletSpeedSetting";
        private const string DROPLET_SIZE_SETTING_NAME = "dropletSizeSetting";
        private const string ELECTRODE_SIZE_SETTING_NAME = "electrodeSizeSetting";
        private const string EMPTY_RECTANGLES_SETTING_NAME = "emptyRectanglesSetting";
        private const string USE_SIMULATOR_STRICT_MODE_SETTING_NAME = "useSimulatorStrictModeSetting";
        private const string SIMULATOR_UPS_SETTING_NAME = "simulatorUPSSetting";
        private const string CREATE_GRAPH_SETTING_NAME = "createGraphSettingSetting";
        private const string ENABLE_OPTIMIZATIONS_SETTING_NAME = "enableOptimizationsSetting";
        private const string ENABLE_GC_SETTING_NAME = "enableGCSetting";
        private const string ENABLE_SPARSE_BOARD_SETTING_NAME = "enableSparseBoardSetting";

        public Dictionary<string, object> Settings = new Dictionary<string, object>();

        public int BoardWidth => (int)(float)Settings[BOARD_WIDTH_SETTING_NAME];
        public int BoardHeight => (int)(float)Settings[BOARD_HEIGHT_SETTING_NAME];
        public float CommandFrequency => (float)Settings[COMMAND_FREQUENCY_SETTING_NAME];
        public float DropletSpeed => (float)Settings[DROPLET_SPEED_SETTING_NAME];
        public float DropletSize => (float)Settings[DROPLET_SIZE_SETTING_NAME];
        public float ElectrodeSize => (float)Settings[ELECTRODE_SIZE_SETTING_NAME];
        public bool ShowEmptyRectangles => (bool)Settings[EMPTY_RECTANGLES_SETTING_NAME];
        public bool UseSimulatorStrictMode => (bool)Settings[USE_SIMULATOR_STRICT_MODE_SETTING_NAME];
        public float SimulatorUPS => (float)Settings[SIMULATOR_UPS_SETTING_NAME];
        public bool CreateGraph => (bool)Settings[CREATE_GRAPH_SETTING_NAME];
        public bool EnableOptimizations => (bool)Settings[ENABLE_OPTIMIZATIONS_SETTING_NAME];
        public bool EnableGC => (bool)Settings[ENABLE_GC_SETTING_NAME];
        public bool EnableSparseBoard => (bool)Settings[ENABLE_SPARSE_BOARD_SETTING_NAME];

        public SettingsInfo()
        {
            //Default settings
            Settings.Add(BOARD_WIDTH_SETTING_NAME              ,  10f);
            Settings.Add(BOARD_HEIGHT_SETTING_NAME             ,  10f);
            Settings.Add(COMMAND_FREQUENCY_SETTING_NAME        ,  20f);
            Settings.Add(DROPLET_SPEED_SETTING_NAME            , 600f);
            Settings.Add(DROPLET_SIZE_SETTING_NAME             ,   1f);
            Settings.Add(ELECTRODE_SIZE_SETTING_NAME           ,   1f);
            Settings.Add(EMPTY_RECTANGLES_SETTING_NAME         , true);
            Settings.Add(USE_SIMULATOR_STRICT_MODE_SETTING_NAME, true);
            Settings.Add(SIMULATOR_UPS_SETTING_NAME            ,  60f);
            Settings.Add(CREATE_GRAPH_SETTING_NAME             , true);
            Settings.Add(ENABLE_OPTIMIZATIONS_SETTING_NAME     , true);
            Settings.Add(ENABLE_GC_SETTING_NAME                , true);
            Settings.Add(ENABLE_SPARSE_BOARD_SETTING_NAME      , false);
        }

        public void LoadSettings(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            string settingsString = File.ReadAllText(path);
            UpdateSettingsFromString(settingsString);
        }

        public void UpdateSettingsFromString(string settingsString)
        {
            string[] individualSettings = settingsString.Split(SETTINGS_DELIMITER);
            foreach (string settingKeyValue in individualSettings)
            {
                if (settingKeyValue.Trim().Length == 0)
                {
                    continue;
                }
                if (!settingKeyValue.Contains(SETTING_KEY_VALUE_DELIMITER))
                {
                    continue;
                }

                string[] splittedSetting = settingKeyValue.Split(SETTING_KEY_VALUE_DELIMITER);
                string key = splittedSetting[0].Trim();
                string value = splittedSetting[1].Trim();

                if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float numberValue))
                {
                    if (Settings.ContainsKey(key))
                    {
                        Settings[key] = numberValue;
                    }
                    else
                    {
                        Settings.Add(key, numberValue);
                    }
                }
                else if (bool.TryParse(value, out bool boolValue))
                {
                    if (Settings.ContainsKey(key))
                    {
                        Settings[key] = boolValue;
                    }
                    else
                    {
                        Settings.Add(key, boolValue);
                    }
                }
            }
        }

        public void SaveSettings(string settingsString, string path)
        {
            File.WriteAllText(path, settingsString);
        }
    }
}
