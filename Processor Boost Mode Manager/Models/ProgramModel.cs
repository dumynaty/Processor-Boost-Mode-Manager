using ProcessorBoostModeManager.Enums;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace ProcessorBoostModeManager.Models
{
    public class ProgramModel
    {
        public required string Name { get; set; }
        public required string Location { get; set; }
        public CPUBoostMode BoostMode { get; set; } = CPUBoostMode.Disabled;
    }
}
