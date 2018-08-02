namespace StoreScraper.Interfaces
{
    interface ISearchSettingsValidator
    {
        bool ValidateSearchSettings(object searchSettings, out string errorMessage);
    }
}
