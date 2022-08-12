using System.Diagnostics;
using System.IO;
using System.Text;
using System.Timers;
using HarmonyLib;
using Verse;
using UnityEngine;

namespace TextInput
{
    public class Settings : ModSettings
    {
        public static int Interval = 500;
        public static string IntervalStringBuffer;

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
            listingStandard.Label("Call Interval (milliseconds)");
            listingStandard.IntEntry(ref Settings.Interval, ref Settings.IntervalStringBuffer);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() => "Linux Input Helper";

        private static void OpenWindow(object src, ElapsedEventArgs e)
        {
            var helperPath = Path.Combine(_content.RootDir, "rwtext-go");
            // var txtPath = Path.Combine(GenFilePaths.SaveDataFolderPath, "linux_input_helper.txt");
            //
            // if (File.Exists(txtPath))
            // {
            //     File.Delete(txtPath);
            // }
            //
            // File.CreateText(txtPath);

            // File.WriteAllText(txtPath, text);

#if DEBUG
            Log.Message($"[Text Input] Invoking the helper script at {helperPath}");
#endif

            var startInfo = new ProcessStartInfo
            {
                FileName = helperPath,
                Arguments = "", // $"\"{txtPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            var process = new Process();
            process.StartInfo = startInfo;

            process.Start();
            process.WaitForExit();

#if DEBUG
            Log.Message($"[Text Input] Process ended with {process.ExitCode}.");
#endif

            if (process.ExitCode == 0)
            {
                var text = process.StandardOutput.ReadToEnd();
#if DEBUG
                Log.Message($"[Text Input] stdout: {text}");
#endif

                GUIUtility.systemCopyBuffer = text;
            }


#if DEBUG
            var clipboard = GUIUtility.systemCopyBuffer;
            Log.Message($"[Text Input] Your clipboard: {clipboard}");
#endif

            // File.Delete(txtPath);
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
                var iKey = e.keyCode == KeyCode.I;
                if (!iKey || !e.control) return;
                
                TextInput.WindowTimer.Interval = Settings.Interval;
                TextInput.WindowTimer.Enabled = true;
            }
        }
    }
}