using ProcessorBoostModeManager.Enums;

namespace ProcessorBoostModeManager.Models.Poco
{
    public class ProgramModel
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public CPUBoostMode BoostMode { get; set; } = CPUBoostMode.Disabled;
    }
}
