using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.GeneralFunctions.Settings;

public interface ISettings
{
    public SettingsClass ReadSettings();
    public void WriteSettings(SettingsClass settings);
}
