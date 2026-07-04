using BepInEx.Configuration;
using System.Reflection;

namespace Timbercraft
{
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    public static class Patch_ProgressiveConstruction
    {
        public static ConfigEntry<bool> progressiveEnabled;
        public static ConfigEntry<float> progressiveNpcArrivalDelay;
        public static ConfigEntry<float> progressiveConstructionStartDelay;
        public static ConfigEntry<float> progressivePieceInterval;
        public static ConfigEntry<string> progressiveMessageArriving;
        public static ConfigEntry<string> progressiveMessageStarted;
        public static ConfigEntry<string> progressiveMessageCompleted;

        public static void Init(TimbercraftPlugin plugin)
        {
            progressiveEnabled = plugin.ModConfig("2 - Progressive Construction", "Enable Progressive Construction", true,
                "Enable the progressive construction system. If disabled, houses will appear fully built when placed.");

            progressiveNpcArrivalDelay = plugin.ModConfig("2 - Progressive Construction", "NPC Arrival Delay", 8f,
                "Time in seconds after placing a house before NPCs start arriving.");

            progressiveConstructionStartDelay = plugin.ModConfig("2 - Progressive Construction", "Construction Start Delay", 3f,
                "Time in seconds after NPCs arrive before construction begins.");

            progressivePieceInterval = plugin.ModConfig("2 - Progressive Construction", "Piece Interval Seconds", 2f,
                "Time in seconds between each piece being placed during construction.");

            progressiveMessageArriving = plugin.ModConfig("2 - Progressive Construction", "Message Arriving",
                "The workers are on their way...",
                "Message shown when a house is placed.");

            progressiveMessageStarted = plugin.ModConfig("2 - Progressive Construction", "Message Started",
                "Construction has begun!",
                "Message shown when construction starts.");

            progressiveMessageCompleted = plugin.ModConfig("2 - Progressive Construction", "Message Completed",
                "Construction complete!",
                "Message shown when construction is finished.");

            progressiveEnabled.SettingChanged += (_, _) => SyncToConfig();
            progressiveNpcArrivalDelay.SettingChanged += (_, _) => SyncToConfig();
            progressiveConstructionStartDelay.SettingChanged += (_, _) => SyncToConfig();
            progressivePieceInterval.SettingChanged += (_, _) => SyncToConfig();
            progressiveMessageArriving.SettingChanged += (_, _) => SyncToConfig();
            progressiveMessageStarted.SettingChanged += (_, _) => SyncToConfig();
            progressiveMessageCompleted.SettingChanged += (_, _) => SyncToConfig();

            SyncToConfig();
        }

        private static void SyncToConfig()
        {
            ProgressiveConstructionConfig.Enabled = progressiveEnabled.Value;
            ProgressiveConstructionConfig.NpcArrivalDelay = progressiveNpcArrivalDelay.Value;
            ProgressiveConstructionConfig.ConstructionStartDelay = progressiveConstructionStartDelay.Value;
            ProgressiveConstructionConfig.PieceInterval = progressivePieceInterval.Value;
            ProgressiveConstructionConfig.MessageArriving = progressiveMessageArriving.Value;
            ProgressiveConstructionConfig.MessageStarted = progressiveMessageStarted.Value;
            ProgressiveConstructionConfig.MessageCompleted = progressiveMessageCompleted.Value;
        }
    }
}
