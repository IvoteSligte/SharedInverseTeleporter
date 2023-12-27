using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace SharedInverseTeleporter
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Lethal Company.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance { get; private set; }

        internal static ManualLogSource log;
        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

        private void Awake()
        {
            instance = this;
            log = Logger;
            harmony.PatchAll();
            log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}

namespace SharedInverseTeleporter.patches
{
    [HarmonyPatch]
    class SharedInverseTeleporter
    {
        private static System.Random _random = new System.Random(0);
        private static Vector3 _teleportPos;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipTeleporter), "TeleportPlayerOutWithInverseTeleporter")]
        public static void TeleportPlayerOutWithInverseTeleporter(int playerObj, ref Vector3 teleportPos)
        {
            teleportPos = _teleportPos;
            Plugin.log.LogInfo($"Teleporting player {playerObj} to {_teleportPos}");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipTeleporter), "beamOutPlayer")]
        public static void SetTeleportPosition(bool ___isInverseTeleporter)
        {
            if (___isInverseTeleporter) {
                int rand = _random.Next(0, RoundManager.Instance.insideAINodes.Length);
                _teleportPos = RoundManager.Instance.insideAINodes[rand].transform.position;
                Plugin.log.LogInfo($"Set teleport position to {_teleportPos}");
            }
        }

        [HarmonyPrefix]
		[HarmonyPatch(typeof(ShipTeleporter), "Awake")]
		private static void Awake(ShipTeleporter __instance)
		{
			if (__instance.isInverseTeleporter)
			{
				__instance.cooldownAmount = 10f;
                Plugin.log.LogInfo("Reduced the cooldown of the inverse teleporter to 10 seconds.");
			}
		}

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RoundManager), "GenerateNewLevelClientRpc")]
        public static void GenerateNewLevelClientRpc(int ___randomSeed)
        {
            _random = new System.Random(___randomSeed);
            Plugin.log.LogInfo($"Initialized a new random number generator with seed {___randomSeed}");
        }
    }
}
