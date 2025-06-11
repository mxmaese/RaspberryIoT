using Entities;
using Services.Databases;
using Services.GeneralFunctions;
using Services.GeneralFunctions.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Cookies;

namespace Services.Traductions;

public class TraductionManager : ITraductionManager
{
    private readonly IDatabasesActions _databasesActions;
    private readonly ICookiesManager _cookieManager;
    private readonly ISettings _settingsActions;
    private SettingsClass _settings;
    public TraductionManager(IDatabasesActions databasesActions, ICookiesManager cookieManager, ISettings settings)
    {
        _databasesActions = databasesActions;
        _cookieManager = cookieManager;
        _settingsActions = settings;
        UpdateSettings();
    }

    public string GetTraduction(string key)
    {
        UpdateSettings();
        var languageId = GetLanguageId();
        return _databasesActions.GetTraduction(key, languageId);
    }
    private int GetLanguageId()
    {
        var cookieName = _settings.Cookies.SingleOrDefault(x=>x.CookieType==SettingsClass.Cookie.CookieTypes.LanguageId)?.CookieName;
        var languageidstring = _cookieManager.GetCookie(cookieName);
        if (languageidstring.IsNullOrEmpty())
        {
           _cookieManager.SetCookie(cookieName, "0");
        }
        var languageid = languageidstring.IsNullOrEmpty() ? 0 : int.Parse(languageidstring);
        return languageid;
    }
    private void UpdateSettings()
    {
        _settings = _settingsActions.ReadSettings();
    }
    public string GetTraduction(string key, int languageId)
    {
        return _databasesActions.GetTraduction(key, languageId);
    }
}

public interface ITraductionManager
{
    string GetTraduction(string key, int languageId);
    string GetTraduction(string key);
}