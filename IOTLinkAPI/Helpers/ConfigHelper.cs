﻿using IOTLinkAPI.Configs;
using IOTLinkAPI.Platform;
using IOTLinkAPI.Platform.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using YamlDotNet.Serialization;

namespace IOTLinkAPI.Helpers
{
    public static class ConfigHelper
    {
        private const string ENGINE_CONF_FILE = "configuration.yaml";

        private static Dictionary<string, ConfigInfo> _configs = new Dictionary<string, ConfigInfo>();

        public delegate void ConfigReloadedHandler(object sender, ConfigReloadEventArgs e);

        public static ApplicationConfig GetEngineConfig()
        {
            string path = Path.Combine(PathHelper.ConfigPath(), ENGINE_CONF_FILE);
            return GetConfiguration<ApplicationConfig>(path);
        }

        public static void SetEngineConfigReloadHandler(ConfigReloadedHandler configReloadedHandler)
        {
            string path = Path.Combine(PathHelper.ConfigPath(), ENGINE_CONF_FILE);
            SetReloadHandler<ApplicationConfig>(path, configReloadedHandler, ConfigType.CONFIGURATION_ENGINE);
        }

        public static T GetConfiguration<T>(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return default(T);

            // Get the full path information
            path = Path.GetFullPath(path);
            object config = null;

            // Return existing configuration if it is already cached
            if (_configs.ContainsKey(path) && _configs[path].Config != null)
            {
                config = _configs[path].Config;
                if (config.GetType() != typeof(T))
                    return default(T);

                return (T)config;
            }
            else
            {
                config = LoadConfiguration<T>(path);
                if (config == null)
                {
                    LoggerHelper.Error("Configuration file not found: {0}", path);
                    return default(T);
                }
            }

            if (!_configs.ContainsKey(path))
                _configs.Add(path, new ConfigInfo());

            _configs[path].Config = config;
            return (T)config;
        }

        public static void SetReloadHandler<T>(string path, ConfigReloadedHandler configReloadedHandler)
        {
            SetReloadHandler<T>(path, configReloadedHandler, ConfigType.CONFIGURATION_ADDON);
        }

        private static void SetReloadHandler<T>(string path, ConfigReloadedHandler configReloadedHandler, ConfigType configType)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            path = Path.GetFullPath(path);
            ConfigInfo configInfo;

            if (!_configs.ContainsKey(path))
                _configs.Add(path, new ConfigInfo());

            configInfo = _configs[path];
            configInfo.ConfigType = configType;
            configInfo.OnConfigReloadHandler += configReloadedHandler;

            if (configInfo.FileSystemWatcher == null)
            {
                LoggerHelper.Debug("Adding file system watcher for {0}", path);

                FileSystemWatcher configWatcher = new FileSystemWatcher(Path.GetDirectoryName(path), Path.GetFileName(path));
                configWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
                configWatcher.Changed += OnConfigChanged<T>;
                configWatcher.Created += OnConfigChanged<T>;
                configWatcher.EnableRaisingEvents = true;

                configInfo.FileSystemWatcher = configWatcher;
            }
        }

        private static T LoadConfiguration<T>(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return default(T);

            path = Path.GetFullPath(path);
            string configText = PathHelper.GetFileText(path);
            if (string.IsNullOrWhiteSpace(configText))
                return default(T);

            try
            {
                StringReader Reader = new StringReader(configText);
                IDeserializer YAMLDeserializer = new DeserializerBuilder().Build();

                return YAMLDeserializer.Deserialize<T>(Reader);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("Error while reading configuration file {0}: {1}", path, ex.ToString());
            }

            return default(T);
        }

        private static void OnConfigChanged<T>(object sender, FileSystemEventArgs e)
        {
            string path = e.FullPath;
            if (!_configs.ContainsKey(path))
            {
                LoggerHelper.Error("File {0} is being watched, but there is no handler for it.", path);
                return;
            }

            try
            {
                // Disable Watcher
                _configs[path].FileSystemWatcher.EnableRaisingEvents = false;

                // This is needed on slow machines which file handlers can take longer to release.
                Thread.Sleep(3000);

                object config = LoadConfiguration<T>(path);
                if (config == null)
                {
                    LoggerHelper.Error("Error while reloading configuration file {0}", path);
                    return;
                }

                LoggerHelper.Debug("Firing events for reloaded configuration file {0}", path);
                ConfigInfo configInfo = _configs[path];
                configInfo.Config = config;

                configInfo.Raise_OnConfigReloadHandler(sender, new ConfigReloadEventArgs
                {
                    FilePath = path,
                    ConfigType = configInfo.ConfigType
                });
            }
            finally
            {
                // Enable Watcher
                _configs[path].FileSystemWatcher.EnableRaisingEvents = false;
            }
        }
    }
}
