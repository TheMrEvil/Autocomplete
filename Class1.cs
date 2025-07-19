using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using System.Collections.Generic;

namespace Autocomplete
{
    public class Class1 : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Scroll Animation Speedup Mod loaded!");
        }

        [HarmonyPatch(typeof(UIPlayerScroll), "OpenSequence")]
        public class UIPlayerScrollPatch
        {
            static bool Prefix(UIPlayerScroll __instance, float delay, ref IEnumerator __result)
            {
                __result = FastOpenSequence(__instance, delay);
                return false; // Skip original method
            }

            static IEnumerator FastOpenSequence(UIPlayerScroll __instance, float delay)
            {
                // Speed up the animation by making it nearly instant
                float speedMultiplier = 0.01f; // 1% of original speed - effectively skipping
                
                // Use reflection to access private fields
                var isOpeningField = AccessTools.Field(typeof(UIPlayerScroll), "IsOpening");
                var rarityFXProperty = AccessTools.Property(typeof(UIPlayerScroll), "RarityFX");
                
                isOpeningField.SetValue(__instance, true);
                
                // Skip the initial delay
                yield return new WaitForSeconds(delay * speedMultiplier);
                
                // Play spawn effects but skip most of the wait
                var spawnSFXField = AccessTools.Field(typeof(UIPlayerScroll), "SpawnSFX");
                var spawnSFX = (List<AudioClip>)spawnSFXField.GetValue(__instance);
                
                if (spawnSFX != null && spawnSFX.Count > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, spawnSFX.Count);
                    AudioClip clip = spawnSFX[randomIndex];
                    if (clip != null)
                    {
                        AudioManager.PlayInterfaceSFX(clip, 1f, UnityEngine.Random.Range(0.93f, 1.07f));
                    }
                }
                
                __instance.ScrollAnim.CrossFade("Scroll_Horizontal", 0.05f);
                __instance.RevealFX.Play();
                
                // Greatly reduced wait time
                yield return new WaitForSeconds((0.8f + delay * 2.1666665f) * speedMultiplier);
                
                __instance.SealBurstFX.Play();
                
                // Access RarityFX using reflection
                var rarityFX = rarityFXProperty.GetValue(__instance);
                if (rarityFX != null)
                {
                    var openFXField = AccessTools.Field(rarityFX.GetType(), "OpenFX");
                    var openSFXField = AccessTools.Field(rarityFX.GetType(), "OpenSFX");
                    var spinFXField = AccessTools.Field(rarityFX.GetType(), "SpinFX");
                    
                    var openFX = (ParticleSystem)openFXField.GetValue(rarityFX);
                    var openSFX = (List<AudioClip>)openSFXField.GetValue(rarityFX);
                    
                    openFX?.Play();
                    
                    if (openSFX != null && openSFX.Count > 0)
                    {
                        int randomIndex = UnityEngine.Random.Range(0, openSFX.Count);
                        AudioClip clip = openSFX[randomIndex];
                        if (clip != null)
                        {
                            AudioManager.PlayInterfaceSFX(clip, 1f, UnityEngine.Random.Range(0.93f, 1.07f));
                        }
                    }
                    
                    __instance.ScrollAnim.CrossFade("Open", 0.1f);
                    
                    // Skip most of the unwrap animation wait
                    yield return new WaitForSeconds(0.594f * speedMultiplier);
                    
                    var spinFX = (ParticleSystem)spinFXField.GetValue(rarityFX);
                    spinFX?.Play();
                }
                
                // Speed up the unwrap animation dramatically
                float t = 0f;
                float unwrapTime = __instance.UnwrapTime * speedMultiplier;
                
                while (t < 1f)
                {
                    t += Time.deltaTime / unwrapTime;
                    float x = __instance.UnwrapCurve.Evaluate(t) * __instance.UnwrapWidth;
                    __instance.maskRect.sizeDelta = new Vector2(x, __instance.maskRect.sizeDelta.y);
                    yield return null;
                }
                
                isOpeningField.SetValue(__instance, false);
            }
        }

        [HarmonyPatch(typeof(UIPlayerScroll), "ChosenSequence")]
        public class UIPlayerScrollChosenPatch
        {
            static bool Prefix(UIPlayerScroll __instance, ref IEnumerator __result)
            {
                __result = FastChosenSequence(__instance);
                return false; // Skip original method
            }

            static IEnumerator FastChosenSequence(UIPlayerScroll __instance)
            {
                __instance.SplashVFX.Play();
                __instance.FlashGroup.alpha = 1f;
                
                // Skip the flash animation
                float t = 1f;
                while (t > 0f)
                {
                    t -= Time.deltaTime * 40f; // Much faster fade
                    __instance.FlashGroup.alpha = t;
                    yield return null;
                }
                __instance.FlashGroup.alpha = 0f;
                
                // Skip most of the wait
                yield return new WaitForSeconds(0.05f); // Reduced from 0.5f
                
                // Speed up the final fade - use direct alpha manipulation instead of UpdateOpacity
                t = 1f;
                while (t > 0f)
                {
                    t -= Time.deltaTime * 30f; // Very fast fade
                    __instance.UIGroup.alpha = t;
                    __instance.UIGroup.interactable = false;
                    __instance.UIGroup.blocksRaycasts = false;
                    yield return null;
                }
                
                // Ensure final state
                __instance.UIGroup.alpha = 0f;
            }
        }

        [HarmonyPatch(typeof(UIPlayerScroll), "ReleaseSequence")]
        public class UIPlayerScrollReleasePatch
        {
            static bool Prefix(UIPlayerScroll __instance, ref IEnumerator __result)
            {
                __result = FastReleaseSequence(__instance);
                return false; // Skip original method
            }

            static IEnumerator FastReleaseSequence(UIPlayerScroll __instance)
            {
                // Very fast fade out
                float t = 1f;
                while (t > 0f)
                {
                    t -= Time.deltaTime * 20f; // Very fast fade
                    __instance.UIGroup.alpha = t;
                    __instance.UIGroup.interactable = false;
                    __instance.UIGroup.blocksRaycasts = false;
                    yield return null;
                }
                
                // Ensure final state
                __instance.UIGroup.alpha = 0f;
            }
        }

        // Patch the animator speed to make scroll animations faster
        [HarmonyPatch(typeof(UIPlayerScroll), "EntryAnim")]
        public class UIPlayerScrollEntryPatch
        {
            static void Postfix(UIPlayerScroll __instance)
            {
                // Set animation speed to be much faster
                __instance.ScrollAnim.speed = 10f; // 10x faster than normal
            }
        }

        // Optional: Speed up any other scroll-related animations
        [HarmonyPatch(typeof(UIPlayerScroll), "Awake")]
        public class UIPlayerScrollAwakePatch
        {
            static void Postfix(UIPlayerScroll __instance)
            {
                // Start with faster animation speed
                __instance.ScrollAnim.speed = 10f;
                
                // Reduce the random delay for entry animation
                __instance.Invoke("EntryAnim", UnityEngine.Random.Range(0f, 0.05f)); // Much smaller delay
            }
        }
    }
}
