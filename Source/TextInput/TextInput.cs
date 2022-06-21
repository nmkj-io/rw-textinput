using System.Diagnostics;
using System.IO;
using System.Timers;
using HarmonyLib;
using Verse;
using UnityEngine;

namespace TextInput
{
    public class Settings : ModSettings
    {
        public static int Interval = 500;
        public static string StringBuffer;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref Interval, "TimerInterval");
            base.ExposeData();
        }
    }
    public class TextInput : Mod
    {
        public static Timer WindowTimer { get; } = new Timer();
        private Settings _settings;

        private static ModContentPack _content;

        public TextInput(ModContentPack content) : base(content)
        {
            _settings = GetSettings<Settings>();
            
            WindowTimer.AutoReset = false;
            WindowTimer.Interval = Settings.Interval;
            WindowTimer.Elapsed += OpenWindow;

            _content = content;
            
            var harmony = new Harmony("nmkj.textinput");
            harmony.PatchAll();
        }

        public override void DoSettingsWindowContents(Rect inRect) // The GUI part to edit the mod settings.
        {
            var listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("Timer Interval");
            listingStandard.IntEntry(ref Settings.Interval, ref Settings.StringBuffer);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() => "Text Input";

        private static void OpenWindow(object src, ElapsedEventArgs e)
        {
            var scriptPath = Path.Combine(_content.RootDir, "helper.py");
            
            var startInfo = new ProcessStartInfo
            {
                FileName = "python3",
                Arguments = scriptPath,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            var process = new Process();
            process.StartInfo = startInfo;

#if DEBUG
            Log.Message($"[Text Input] Invoking the helper script at {scriptPath}");
#endif
            process.Start();
            process.WaitForExit();
#if DEBUG
            var clipboard = GUIUtility.systemCopyBuffer;
            Log.Message($"[Text Input] Process ended. Your text: {clipboard}");
#endif
        }
    }

    static class HarmonyPatches
    {
        [HarmonyPatch(typeof(Text), nameof(Text.StartOfOnGUI))]
        class UIRootOnGuiPatcher
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                var e = Event.current;
                if (!e.control) return;
                if (e.keyCode != KeyCode.I) return;

                TextInput.WindowTimer.Interval = Settings.Interval;
                TextInput.WindowTimer.Enabled = true;
            }
        }
    }
}