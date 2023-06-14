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
        public override string Version => "1.1.0";
        public override string Link => "https://github.com/art0007i/CapturePrivateUI/";

        [AutoRegisterConfigKey]
        static ModConfigurationKey<bool> ENABLE_KEY = new("enable", "When enabled, finger photo will render private UI.", () => true);
        [AutoRegisterConfigKey]
        static ModConfigurationKey<bool> TAG_FILTER_ENABLE_KEY = new("tag_filter_enable", "When enabled, all cameras on a slot with the specified tag will render private UI.", () => false);
        [AutoRegisterConfigKey]
        static ModConfigurationKey<string> TAG_FILTER_KEY = new("tag_filter", "The tag that cameras will require to render private UI. Only works when the above option is enabled.", () => "RenderPrivateUI");
        public static ModConfiguration config;

        public override void OnEngineInit()
        {
            config = GetConfiguration();

            config.OnThisConfigurationChanged += (e) => {
                if(e.Key == TAG_FILTER_ENABLE_KEY || e.Key == TAG_FILTER_KEY)
                {
                    // I love updating every thing
                    Engine.Current.WorldManager.Worlds.Do((w) =>
                    {
                        w.RunSynchronously(() =>
                        {
                            w.RootSlot.GetComponentsInChildren<Camera>().Do((c) => c.MarkChangeDirty());
                        });
                    });
                }
            };
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

        [HarmonyPatch(typeof(Camera), nameof(Camera.RenderPrivateUI), MethodType.Getter)]
        class TagFilterPatch
        {
            public static bool Prefix(Camera __instance, ref bool __result)
            {
                if (!config.GetValue(TAG_FILTER_ENABLE_KEY)) return true;
                __result = __instance.Slot.Tag == config.GetValue(TAG_FILTER_KEY);
                return false;
            }
        }
    }
}