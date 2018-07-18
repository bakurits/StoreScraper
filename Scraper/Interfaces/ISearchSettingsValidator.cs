using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckOutBot.Interfaces
{
    interface ISearchSettingsValidator
    {
        bool ValidateSearchSettings(object searchSettings, out string errorMessage);
    }
}
