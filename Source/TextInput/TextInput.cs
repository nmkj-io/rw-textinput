using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Timers;
using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.Steam;
using Verse.Sound;

namespace TextInput
{
    public class Settings : ModSettings
    {
        public static int Interval = 500;
        public static bool PreventOnScreenKeyboard;
        public static int InputWidth = 0;
        public static int InputHeight = 0;
        public static string IntervalStringBuffer;
        public static string WidthStringBuffer;
        public static string HeightStringBuffer;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref Interval, "TimerInterval");
            Scribe_Values.Look(ref PreventOnScreenKeyboard, "OnScreenKeyboard", SteamDeck.IsSteamDeckInNonKeyboardMode);
            Scribe_Values.Look(ref InputWidth, "InputWidth");
            Scribe_Values.Look(ref InputHeight, "InputHeight");
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

            if (!SteamDeck.IsSteamDeck)
            {
                Settings.PreventOnScreenKeyboard = false;
            }
            
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
            listingStandard.Label("Input Window Width (0 to default)", tooltip: "0 to set default");
            listingStandard.IntEntry(ref Settings.InputWidth, ref Settings.WidthStringBuffer);
            listingStandard.Label("Input Window Height (0 to default)", tooltip: "0 to set default");
            listingStandard.IntEntry(ref Settings.InputHeight, ref Settings.HeightStringBuffer);
            if (SteamDeck.IsSteamDeck)
            {
                listingStandard.GapLine();
                listingStandard.CheckboxLabeled("(Steam Deck) Prevent on-screen keyboard from automatically showing up", ref Settings.PreventOnScreenKeyboard);
            }
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() => "Linux Input Helper";

        private static void OpenWindow(object src, ElapsedEventArgs e)
        {
            var helperPath = Path.Combine(_content.RootDir, "rwtext-go");

#if DEBUG
            Log.Message($"[Text Input] Invoking the helper script at {helperPath}");
#endif

            var stateObject = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            var width = Settings.InputWidth < 1 ? -1 : Settings.InputWidth;
            var height = Settings.InputHeight < 1 ? -1 : Settings.InputHeight;
            var stateText = stateObject.text.Replace("\n", "\\n");
            var argsList = new List<string>{$"-w {width}", $"-h {height}", $"-d \"{stateText}\""};
            if (stateObject.multiline)
            {
                argsList.Add("-m");
            }
            var startInfo = new ProcessStartInfo
            {
                FileName = helperPath,
                Arguments = string.Join(" ", argsList), // $"\"{txtPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            var process = new Process();
            process.StartInfo = startInfo;

            if (SteamDeck.IsSteamDeckInNonKeyboardMode)
            {
                SteamDeck.ShowOnScreenKeyboard("", stateObject.position, stateObject.multiline);
            }

            process.Start();
            process.WaitForExit();

#if DEBUG
            Log.Message($"[Text Input] Process ended with {process.ExitCode}.");
#endif

            if (SteamDeck.KeyboardShowing)
            {
                SteamDeck.HideOnScreenKeyboard();
            }

            if (process.ExitCode == 0)
            {
                var text = process.StandardOutput.ReadToEnd();
#if DEBUG
                Log.Message($"[Text Input] stdout: {text}");
#endif
                GUIUtility.systemCopyBuffer = text.TrimEnd('\n');
                var messageType = MessageTypeDefOf.TaskCompletion;
                Messages.Message("Text has been copied to clipboard. Please paste it with Ctrl-V.", messageType, false);
                messageType.sound.PlayOneShotOnCamera();
            }
            else
            {
                var messageType = MessageTypeDefOf.RejectInput;
                Messages.Message("Text input has been cancelled.", messageType, false);
                messageType.sound.PlayOneShotOnCamera();
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

        [HarmonyPatch(typeof(SteamDeck), nameof(SteamDeck.Update))]
        class SteamDeckKeyboardUpdatePatcher
        {
            [HarmonyPrefix]
            public static bool Prefix()
            {
                return !Settings.PreventOnScreenKeyboard;
            }
        }

        [HarmonyPatch(typeof(SteamDeck), nameof(SteamDeck.RootOnGUI))]
        class SteamDeckKeyboardRootPatcher
        {
            [HarmonyPrefix]
            public static bool Prefix()
            {
                return !Settings.PreventOnScreenKeyboard;
            }
        }
    }
}