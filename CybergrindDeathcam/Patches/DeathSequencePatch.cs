using CybergrindDeathcam.Components;
using HarmonyLib;
using UnityEngine;

namespace CybergrindDeathcam.Patches
{
    [HarmonyPatch(typeof(DeathSequence))]
    public class DeathSequencePatch
    {
        private const string CyberRankCanvasPath = "/Player/FinishCanvas (1)";
        private const string BlackScreenCanvasPath = "/Canvas/BlackScreen";
        
        // Wacky way to make a stats screen appear
        [HarmonyPostfix]
        [HarmonyPatch(typeof(DeathSequence), "EndSequence")]
        public static void DeathSequence_EndSequence_Postfix()
        {
            if (SceneHelper.CurrentScene != "Endless")
                return;
            
            var muman = MonoSingleton<MusicManager>.Instance;
            var timeController = MonoSingleton<TimeController>.Instance;
            
            timeController.timeScale = 0.0f;

            var cgCanvas = GameObject.Find(CyberRankCanvasPath);
            var finalCyberRank = cgCanvas.transform.Find("Panel").GetComponent<FinalCyberRank>();
            var blackScreen = GameObject.Find(BlackScreenCanvasPath).transform;
            
            blackScreen.Find("YouDiedText").gameObject.SetActive(false);
            blackScreen.Find("LaughingSkull").gameObject.SetActive(false);

            var deathcam = NewMovement.Instance.gameObject.GetComponent<DeathCam>();

            if (deathcam == null || !deathcam.skipLeaderboards)
            {
                finalCyberRank.Appear();
                muman.forcedOff = true;
                muman.StopMusic();
            }
            else
            {
                GameProgressSaver.AddMoney(finalCyberRank.totalPoints);
                SceneHelper.RestartScene();
            }

            cgCanvas.transform.parent = GameObject.Find("/Canvas").transform;
        }
    }
}