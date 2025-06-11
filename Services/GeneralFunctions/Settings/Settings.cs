using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.GeneralFunctions.Settings;

public class Settings : ISettings
{
    readonly IFileFunctions _FileFunctions;
#if DEBUG
    public string SettingsPath { get; set; } = "Testing/Data/Settings";
#else
    public string SettingsPath { get; set; } = "../Data/Settings";
#endif
    public Settings(IFileFunctions fileFunctions)
    {
        _FileFunctions = fileFunctions;
    }

    public SettingsClass ReadSettings()
    {
        SettingsClass settings = _FileFunctions.ReadFromFile<SettingsClass>(SettingsPath);
        return settings;
    }

    public void WriteSettings(SettingsClass settings)
    {
        _FileFunctions.WriteToFile(SettingsPath, settings);
    }
}
