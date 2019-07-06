using Aktiv.RtAdmin.Properties;

namespace Aktiv.RtAdmin
{
    public class ConfigLinesStorage : BaseLocalStorage
    {
        public ConfigLinesStorage() 
            : base(Resources.ConfigFileNotFound, Resources.ConfigFileIsEmpty, Resources.ConfigFileLinesHaveEnded)
        {
        }
    }
}
