using Simplisity;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Nevoweb.RocketDirectoryMVC.Components
{
    public class MvcData
    {
        public Dictionary<String, String> Settings { get; set; }
        public MvcData()
        {
            Settings = new Dictionary<string, string>();
        }

        public void SetSetting(String key, String value)
        {
            if (Settings == null) Settings = new Dictionary<String, String>();
            if (Settings.ContainsKey(key)) Settings.Remove(key);
            Settings.Add(key, value);
        }
        public String GetSetting(String key, String defaultValue = "")
        {
            if (Settings != null && Settings.ContainsKey(key)) return Settings[key];
            return defaultValue;
        }
        public Boolean GetSettingBool(String key, Boolean defaultValue = false)
        {
            try
            {
                if (Settings == null) Settings = new Dictionary<String, String>();
                if (Settings.ContainsKey(key))
                {
                    var x = Settings[key];
                    // bool usually stored as "True" "False"
                    if (x.ToLower() == "true") return true;
                    // Test for 1 as true also.
                    if (GeneralUtils.IsNumeric(x))
                    {
                        if (Convert.ToInt32(x) > 0) return true;
                    }
                    return false;
                }
                return defaultValue;
            }
            catch (Exception ex)
            {
                var ms = ex.ToString();
                return defaultValue;
            }
        }


        public int GetSettingInt(String key, int defaultValue = -1)
        {
            if (Settings == null) Settings = new Dictionary<String, String>();
            if (Settings.ContainsKey(key))
            {
                var s = Settings[key];
                if (GeneralUtils.IsNumeric(s)) return Convert.ToInt32(s);
            }
            return defaultValue;
        }

    }

}
