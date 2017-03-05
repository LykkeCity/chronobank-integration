namespace Core.Settings
{
    public class SettingsConverter
    {
        public static BaseSettings ConvertFromGeneralSettings(GeneralSettings settings)
        {
            var converted = settings.ChronobankSettings;
            
            converted.Db.SharedConnString = settings.Db.SharedStorageConnString;
            converted.Db.ChronoNotificationConnString = settings.Db.ChronoBankSrvConnString;

            return converted;
        }
    }
}
