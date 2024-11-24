using StardewModdingAPI;

namespace Email
{
    public partial class ModEntry : Mod
    {
        public static ModEntry context;

        public static ModConfig Config;
        public static IModHelper SHelper;
        public static IMonitor SMonitor;
        public static IMobilePhoneApi api;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            SHelper = Helper;
            SMonitor = Monitor;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        }
    }
}
