using HarmonyLib;
using NeosModLoader;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using FrooxEngine;

namespace CapturePrivateUI
{
    public class CapturePrivateUI : NeosMod
    {
        public override string Name => "CapturePrivateUI";
        public override string Author => "art0007i";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/art0007i/CapturePrivateUI/";

        [AutoRegisterConfigKey]
        static ModConfigurationKey<bool> ENABLE_KEY = new("enable", "When enabled, finger photo will render private UI.", () => true);
        public static ModConfiguration config;

        public override void OnEngineInit()
        {
            config = GetConfiguration();

            Harmony harmony = new Harmony("me.art0007i.CapturePrivateUI");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(PhotoCaptureManager), "OnCommonUpdate")]
        class CapturePrivateUIPatch
        {
            public static void Postfix(PhotoCaptureManager __instance)
            {
                if (!__instance.IsUnderLocalUser)
                {
                    return;
                }
                if (__instance.World.Focus == World.WorldFocus.Background)
                {
                    return;
                }
                var camField = (AccessTools.Field(__instance.GetType(), "_camera").GetValue(__instance) as SyncRef<Camera>);
                if(camField.Target != null)
                {
                    camField.Target.RenderPrivateUI = config.GetValue(ENABLE_KEY);
                }
            }
        }
    }
}