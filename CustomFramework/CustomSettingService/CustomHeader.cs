using LabApi.Loader.Features.Plugins.Enums;
using System.Collections.Generic;
using UserSettings.ServerSpecific;

namespace CustomFramework.CustomSettingService
{
    public class CustomHeader
    {
        internal static ServerSpecificSettingBase[] WildcardSettings { get; set; }
        internal static List<CustomHeader> HeaderSettings { get; set; }
        internal static ServerSpecificSettingBase[] GetSettings()
        {
            return WildcardSettings;
        }

        internal SSGroupHeader Base;
        
        public string Label { get; set; }
        public bool ReducedPadding { get; set; } = false;
        public string Hint { get; set; } = null;

        public CustomHeader(string label, bool reducedPadding = false, string hint = null)
        {
            Label = label;
            ReducedPadding = reducedPadding;
            Hint = hint;


        }
    }
}
