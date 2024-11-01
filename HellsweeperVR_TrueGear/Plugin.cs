using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System;
using MyTrueGear;
using System.Diagnostics;
using System.Numerics;
using UnityEngine;
using System.Threading;

namespace HellsweeperVR_TrueGear
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        internal static new ManualLogSource Log;

        private static UnityEngine.Vector3 leftHandPos = new UnityEngine.Vector3();
        private static UnityEngine.Vector3 rightHandPos = new UnityEngine.Vector3();
        private static UnityEngine.Vector3 playerPos = new UnityEngine.Vector3();

        private static int localPlayerID = -1;

        private static bool isAOEMAgic = false;
        private static bool isRightHandSpell = true;
        private static int leftHandSpellTime = 0;
        private static int rightHandSpellTime = 0;


        private static TrueGearMod _TrueGear = null;

        public override void Load()
        {
            // Plugin startup logic
            Log = base.Log;
            Harmony.CreateAndPatchAll(typeof(Plugin));

            _TrueGear = new TrueGearMod();

            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            _TrueGear.Play("HeartBeat");

        }

        public static int GetMillisecond()
        {
            DateTime now = DateTime.Now;
            int milliseconds = now.Millisecond;
            return milliseconds;
        }

        //private static int leftHandPickupTime = 0;
        //private static int rightHandPickupTime = 0;
        //private static int leftHandWeaponTouchTime = 0;
        //private static int rightHandWeaponTouchTime = 0;
        //private static int LeftHandTelekinesisTime = 0;
        //private static int rightHandTelekinesisTime = 0;
        //private static int teleportTime = 0;

        private static bool canLeftHandPickup = true;
        private static bool canRightHandPickup = true;
        private static bool canLeftHandWeaponTouch = true;
        private static bool canRightHandWeaponTouch = true;
        private static bool canLeftHandTelekinesis = true;
        private static bool canRightHandTelekinesis = true;
        private static bool canTeleport = true;

        private static void LeftHandPickupTimerCallBack(object o)
        {
            canLeftHandPickup = true;
        }
        private static void RightHandPickupTimerCallBack(object o)
        {
            canRightHandPickup = true;
        }
        private static void LeftHandWeaponTouchTimerCallBack(object o)
        {
            canLeftHandWeaponTouch = true;
            leftWeaponHitTimer.Dispose();
            leftWeaponHitTimer = null;
        }
        private static void RightHandWeaponTouchTimerCallBack(object o)
        {
            canRightHandWeaponTouch = true;
            rightWeaponHitTimer.Dispose();
            rightWeaponHitTimer = null;
        }
        private static void LeftHandTelekinesisTimerCallBack(object o)
        {
            canLeftHandTelekinesis = true;
        }
        private static void RightHandTelekinesisTimerCallBack(object o)
        {
            canRightHandTelekinesis = true;
        }
        private static void TeleportTimerCallBack(object o)
        {
            canTeleport = true;
        }

        private static bool canRightSpell = false;
        private static bool canLeftSpell = false;
        private static Timer leftWeaponHitTimer = null;
        private static Timer rightWeaponHitTimer = null;
        private static void EffectCheck(string eventName)
        {
            Log.LogInfo("----------------------------------------------");
            Log.LogInfo(eventName);
            if (eventName.Contains("bullet_time"))
            {
                Log.LogInfo("BulletTime");
                _TrueGear.Play("BulletTime");
            }
            else if (eventName.Contains("grabobject"))
            {
                if (eventName.Contains("_l"))
                {
                    _TrueGear.StartLeftHandPickup();
                }
                else
                {
                    _TrueGear.StartRightHandPickup();
                }
            }
            else if (eventName.Contains("jumping"))
            {
                Log.LogInfo("Jump");
                _TrueGear.Play("Jump");
            }
            else if (eventName.Contains("landing"))
            {
                Log.LogInfo("Fall");
                _TrueGear.Play("Fall");
            }
            else if (eventName.Contains("dashing"))
            {
                Log.LogInfo("Dashing");
                _TrueGear.Play("Dashing");
            }
            else if (eventName.Contains("portal_charging"))
            {
                _TrueGear.StartTeleport();
            }
            else if (eventName.Contains("portal_done"))
            {
                Log.LogInfo("TeleportSuccess");
                _TrueGear.Play("TeleportSuccess");
            }
            else if (eventName.Contains("weapon_spawn"))
            {
                if (eventName.Contains("_l"))
                {
                    Log.LogInfo("LeftHandSummonWeapon");
                    _TrueGear.Play("LeftHandSummonWeapon");
                    isRightHandSpell = false;
                    canLeftSpell = true;
                    new Timer(LeftSpellTimerCallBack,null,30,Timeout.Infinite);
                }
                else
                {
                    Log.LogInfo("RightHandSummonWeapon");
                    _TrueGear.Play("RightHandSummonWeapon");
                    isRightHandSpell = true;
                    canRightSpell = true;
                    new Timer(RightSpellTimerCallBack, null, 30, Timeout.Infinite);
                }
            }
            else if (eventName.Contains("wield"))
            {
                if (eventName.Contains("_l"))
                {
                    Log.LogInfo("LeftHandMeleeHit");
                    _TrueGear.Play("LeftHandMeleeHit");
                }
                else
                {
                    Log.LogInfo("RightHandMeleeHit");
                    _TrueGear.Play("RightHandMeleeHit");
                }
            }
            else if (eventName.Contains("weapon_hit"))
            {
                if (eventName.Contains("_l"))
                {
                    if (leftWeaponHitTimer != null)
                    {
                        leftWeaponHitTimer.Dispose();
                        leftWeaponHitTimer = new Timer(LeftHandWeaponTouchTimerCallBack, null, 150, Timeout.Infinite);
                    }
                    if (!canLeftHandWeaponTouch)
                    {
                        return;
                    }
                    canLeftHandWeaponTouch = false;
                    leftWeaponHitTimer = new Timer(LeftHandWeaponTouchTimerCallBack, null, 150, Timeout.Infinite);
                    Log.LogInfo("LeftHandWeaponTouch");
                    _TrueGear.Play("LeftHandWeaponTouch");
                }
                else
                {
                    if (rightWeaponHitTimer != null)
                    {
                        rightWeaponHitTimer.Dispose();
                        rightWeaponHitTimer = new Timer(RightHandWeaponTouchTimerCallBack, null, 150, Timeout.Infinite);
                    }
                    if (!canRightHandWeaponTouch)
                    {
                        return;
                    }
                    canRightHandWeaponTouch = false;
                    rightWeaponHitTimer = new Timer(RightHandWeaponTouchTimerCallBack, null, 150, Timeout.Infinite);
                    Log.LogInfo("RightHandWeaponTouch");
                    _TrueGear.Play("RightHandWeaponTouch");
                }
            }
            else if (eventName.Contains("eject_from_melee"))
            {
                if (eventName.Contains("_l"))
                {
                    Log.LogInfo("LeftHandWeaponEject");
                    _TrueGear.Play("LeftHandWeaponEject");
                }
                else
                {
                    Log.LogInfo("RightHandWeaponEject");
                    _TrueGear.Play("RightHandWeaponEject");
                }
            }
            else if (eventName.Contains("return_to_melee"))
            {
                if (eventName.Contains("_l"))
                {
                    Log.LogInfo("LeftHandWeaponReturn");
                    _TrueGear.Play("LeftHandWeaponReturn");
                }
                else
                {
                    Log.LogInfo("RightHandWeaponReturn");
                    _TrueGear.Play("RightHandWeaponReturn");
                }
            }
            else if (eventName.Contains("reload"))
            {
                if (eventName.Contains("_l"))
                {
                    Log.LogInfo("LeftHandReload");
                    _TrueGear.Play("LeftHandReload");
                }
                else
                {
                    Log.LogInfo("RightHandReload");
                    _TrueGear.Play("RightHandReload");
                }
            }
            else if (eventName.Contains("pump"))
            {
                if (eventName.Contains("_l"))
                {
                    Log.LogInfo("LeftHandReload");
                    _TrueGear.Play("LeftHandReload");
                }
                else
                {
                    Log.LogInfo("RightHandReload");
                    _TrueGear.Play("RightHandReload");
                }
            }
            else if (eventName.Contains("recoil"))
            {
                if (eventName.Contains("weak"))
                {
                    if (eventName.Contains("_l"))
                    {
                        Log.LogInfo("LeftHandPistolShoot");
                        _TrueGear.Play("LeftHandPistolShoot");
                    }
                    else
                    {
                        Log.LogInfo("RightHandPistolShoot");
                        _TrueGear.Play("RightHandPistolShoot");
                    }
                }
                else if (eventName.Contains("normal"))
                {
                    if (eventName.Contains("_l"))
                    {
                        Log.LogInfo("LeftHandRifleShoot");
                        _TrueGear.Play("LeftHandRifleShoot");
                    }
                    else
                    {
                        Log.LogInfo("RightHandRifleShoot");
                        _TrueGear.Play("RightHandRifleShoot");
                    }
                }
                else
                {
                    if (eventName.Contains("_l"))
                    {
                        Log.LogInfo("LeftHandShotgunShoot");
                        _TrueGear.Play("LeftHandShotgunShoot");
                    }
                    else
                    {
                        Log.LogInfo("RightHandShotgunShoot");
                        _TrueGear.Play("RightHandShotgunShoot");
                    }
                }

            }
            else if (eventName.Contains("telekinesis"))
            {
                if (eventName.Contains("_l"))
                {
                    if (!canLeftHandTelekinesis)
                    {
                        return;
                    }
                    canLeftHandTelekinesis = false;
                    Timer timer = new Timer(LeftHandTelekinesisTimerCallBack, null, 130, Timeout.Infinite);
                    Log.LogInfo("LeftHandTelekinesis");
                    _TrueGear.Play("LeftHandTelekinesis");
                }
                else
                {
                    if (!canRightHandTelekinesis)
                    {
                        return;
                    }
                    canRightHandTelekinesis = false;
                    Timer timer = new Timer(RightHandTelekinesisTimerCallBack, null, 130, Timeout.Infinite);
                    Log.LogInfo("RightHandTelekinesis");
                    _TrueGear.Play("RightHandTelekinesis");
                }
            }
            else if (eventName.Contains("takebackslot"))
            {
                if (eventName.Contains("_l"))
                {
                    Log.LogInfo("LeftBackSlotOutputItem");
                    _TrueGear.Play("LeftBackSlotOutputItem");
                }
                else
                {
                    Log.LogInfo("RightBackSlotOutputItem");
                    _TrueGear.Play("RightBackSlotOutputItem");
                }
            }
            else if (eventName.Contains("impactstrong"))
            {
                Log.LogInfo("FrontMeleeDamage");
                _TrueGear.Play("FrontMeleeDamage");
            }
            else if (eventName.Contains("slashback"))
            {
                Log.LogInfo("BackMeleeDamage");
                _TrueGear.Play("BackMeleeDamage");
            }
            else if (eventName.Contains("pull_bowstring"))
            {
                Log.LogInfo("BowStringPull");
                _TrueGear.Play("BowStringPull");
            }
            else if (eventName.Contains("body_shake_death"))
            {
                Log.LogInfo("PlayerDeath");
                _TrueGear.Play("PlayerDeath");
            }
        }

        private static void LeftSpellTimerCallBack(object o)
        {
            canLeftSpell = false;
        }

        private static void RightSpellTimerCallBack(object o)
        {
            canRightSpell = false;
        }


        [HarmonyPrefix, HarmonyPatch(typeof(BHapticsController), "PlayHaptic")]
        private static void BHapticsController_PlayHaptic_Postfix(BHapticsController __instance, string eventName, float intensity, float duration, float angleX, float offsetY)
        {
            EffectCheck(eventName);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BHapticsController), "Play")]
        private static void BHapticsController_Play_Postfix(BHapticsController __instance, string eventName)
        {
            EffectCheck(eventName);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BHapticsController), "PlayHapticLoop")]
        private static void BHapticsController_PlayHapticLoop_Postfix(BHapticsController __instance, string eventName, float intensity, float duration, float angleX, float offsetY)
        {
            Log.LogInfo("----------------------------------------------");
            Log.LogInfo("PlayHapticLoop");
            Log.LogInfo(eventName);
            Log.LogInfo(intensity);
            Log.LogInfo(duration);
            Log.LogInfo(angleX);
            Log.LogInfo(offsetY);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BHapticsController), "PlayHapticOffset")]
        private static void BHapticsController_PlayHapticOffset_Postfix(BHapticsController __instance, string eventName, float intensity, float duration, float angleX, float offsetY)
        {
            float angle;
            if (angleX < 0f)
            {
                angle = Math.Abs(angleX);
            }
            else
            {
                angle = 360f - angleX;
            }
            Log.LogInfo("----------------------------------------------");
            Log.LogInfo($"DefaultDamage,{angle},{offsetY}");
            _TrueGear.PlayAngle("DefaultDamage", angle, offsetY);
            Log.LogInfo(eventName);
            Log.LogInfo(angleX);
            Log.LogInfo(offsetY);
        }

        //[HarmonyPrefix, HarmonyPatch(typeof(BHapticsController), "StopHaptic")]
        //private static void BHapticsController_StopHaptic_Postfix(BHapticsController __instance, string key)
        //{
        //    Log.LogInfo("----------------------------------------------");
        //    Log.LogInfo("StopHaptic");
        //    Log.LogInfo(key);
        //}

        //[HarmonyPrefix, HarmonyPatch(typeof(BHapticsController), "StopPlaying")]
        //private static void BHapticsController_StopPlaying_Postfix(BHapticsController __instance, string eventName)
        //{
        //    Log.LogInfo("----------------------------------------------");
        //    Log.LogInfo("StopPlaying");
        //    Log.LogInfo(eventName);
        //}




        [HarmonyPrefix, HarmonyPatch(typeof(HellsweeperPlayer), "Update")]
        private static void HellsweeperPlayer_Update_Postfix(HellsweeperPlayer __instance)
        {
            leftHandPos = __instance.leftHand.gameObject.transform.position;
            rightHandPos = __instance.rightHand.gameObject.transform.position;
            playerPos = __instance.gameObject.transform.position;
        }




        private static bool isCrystalPress = false;

        [HarmonyPrefix, HarmonyPatch(typeof(SkillCrystalBehaviour), "AbilityButtonPressed")]
        private static void SkillCrystalBehaviour_AbilityButtonPressed_Postfix(SkillCrystalBehaviour __instance)
        {
            if(localPlayerID != __instance.ownerID)
            {
                return;
            }
            Log.LogInfo("----------------------------------------------");
            if (UnityEngine.Vector3.Distance(leftHandPos, __instance.gameObject.transform.position) < UnityEngine.Vector3.Distance(rightHandPos, __instance.gameObject.transform.position))
            {
                Log.LogInfo("StartLeftHandCrystalPress");
                _TrueGear.StartLeftHandCrystalPress();
            }
            else
            {
                Log.LogInfo("StartRightHandCrystalPress");
                _TrueGear.StartRightHandCrystalPress();
            }
            isCrystalPress = true;
            Log.LogInfo("SkillCrystalAbilityButtonPressed");
            Log.LogInfo(__instance.ownerID);
        }



        [HarmonyPrefix, HarmonyPatch(typeof(SkillCrystalBehaviour), "AbilityButtonUp")]
        private static void SkillCrystalBehaviour_AbilityButtonUp_Postfix(SkillCrystalBehaviour __instance)
        {
            if (localPlayerID != __instance.ownerID)
            {
                return;
            }
            Log.LogInfo("----------------------------------------------");
            if (UnityEngine.Vector3.Distance(leftHandPos, __instance.gameObject.transform.position) < UnityEngine.Vector3.Distance(rightHandPos, __instance.gameObject.transform.position))
            {
                Log.LogInfo("StopLeftHandCrystalPress");
                _TrueGear.StopLeftHandCrystalPress();
            }
            else
            {
                Log.LogInfo("StopRightHandCrystalPress");
                _TrueGear.StopRightHandCrystalPress();
            }
            isCrystalPress = false;
            Log.LogInfo("SkillCrystalAbilityButtonUp");
            Log.LogInfo(__instance.ownerID);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(SkillCrystalBehaviour), "DespawnObject")]
        private static void SkillCrystalBehaviour_DespawnObject_Postfix(SkillCrystalBehaviour __instance)
        {
            if (localPlayerID != __instance.ownerID)
            {
                return;
            }
            if (!isCrystalPress)
            {
                return;
            }
            if (UnityEngine.Vector3.Distance(leftHandPos, __instance.gameObject.transform.position) < UnityEngine.Vector3.Distance(rightHandPos, __instance.gameObject.transform.position))
            {
                Log.LogInfo("LeftHandCrystalBreak");
                _TrueGear.Play("LeftHandCrystalBreak");
            }
            else
            {
                Log.LogInfo("RightHandCrystalBreak");
                _TrueGear.Play("RightHandCrystalBreak");
            }

            Log.LogInfo("SkillCrystalDespawnObject");
            Log.LogInfo("StopLeftHandCrystalPress");
            Log.LogInfo("StopRightHandCrystalPress");
            _TrueGear.StopLeftHandCrystalPress();
            _TrueGear.StopRightHandCrystalPress();
            isCrystalPress = false;
            Log.LogInfo(__instance.ownerID);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(SkillCrystalBehaviour), "OnPickedUp")]
        private static void SkillCrystalBehaviour_OnPickedUp_Postfix(SkillCrystalBehaviour __instance,  PlayerGrip grip)
        {
            if (localPlayerID != __instance.ownerID)
            {
                return;
            }
            Log.LogInfo("----------------------------------------------");
            if (UnityEngine.Vector3.Distance(leftHandPos, __instance.gameObject.transform.position) < UnityEngine.Vector3.Distance(rightHandPos, __instance.gameObject.transform.position))
            {
                Log.LogInfo("LeftHandPickupItem");
                _TrueGear.Play("LeftHandPickupItem");
            }
            else
            {
                Log.LogInfo("RightHandPickupItem");
                _TrueGear.Play("RightHandPickupItem");
            }
            Log.LogInfo("OnPickedUp");
            Log.LogInfo(grip.hc.isRight);
            Log.LogInfo(__instance.ownerID);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(SkillCrystalBehaviour), "OnDropped")]
        private static void SkillCrystalBehaviour_OnDropped_Postfix(SkillCrystalBehaviour __instance)
        {
            if (localPlayerID != __instance.ownerID)
            {
                return;
            }
            Log.LogInfo("----------------------------------------------");
            if (UnityEngine.Vector3.Distance(leftHandPos, __instance.gameObject.transform.position) < UnityEngine.Vector3.Distance(rightHandPos, __instance.gameObject.transform.position))
            {
                Log.LogInfo("StopLeftHandCrystalPress");
                _TrueGear.StopLeftHandCrystalPress();
            }
            else
            {
                Log.LogInfo("StopRightHandCrystalPress");
                _TrueGear.StopRightHandCrystalPress();
            }
            isCrystalPress = false;
            Log.LogInfo("OnDropped");
            Log.LogInfo(__instance.ownerID);
        }


        [HarmonyPrefix, HarmonyPatch(typeof(HellsweeperPlayer), "SetUpPlayerForLevel")]
        private static void HellsweeperPlayer_SetUpPlayerForLevel_Postfix(HellsweeperPlayer __instance )
        {
            Log.LogInfo("----------------------------------------------");
            Log.LogInfo("SetUpPlayerForLevel");
            Log.LogInfo(__instance.localActorID);
            localPlayerID = __instance.localActorID;
        }


        [HarmonyPrefix, HarmonyPatch(typeof(DogCommandWeapon), "AbilityButtonPressed")]
        private static void DogCommandWeapon_AbilityButtonPressed_Postfix(DogCommandWeapon __instance)
        {
            if (localPlayerID != __instance.ownerID)
            {
                return;
            }
            Log.LogInfo("----------------------------------------------");
            if (UnityEngine.Vector3.Distance(leftHandPos, __instance.gameObject.transform.position) < UnityEngine.Vector3.Distance(rightHandPos, __instance.gameObject.transform.position))
            {
                Log.LogInfo("LeftHandPickupItem");
                _TrueGear.Play("LeftHandPickupItem");
            }
            else
            {
                Log.LogInfo("RightHandPickupItem");
                _TrueGear.Play("RightHandPickupItem");
            }
            Log.LogInfo("DogCommandWeaponAbilityButtonPressed");
            Log.LogInfo(__instance.ownerID);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BaseKeyItem), "OnPickedUp")]
        private static void BaseKeyItem_OnPickedUp_Postfix(BaseKeyItem __instance)
        {
            if (localPlayerID != __instance.ownerID)
            {
                return;
            }
            Log.LogInfo("----------------------------------------------");
            if (UnityEngine.Vector3.Distance(leftHandPos, __instance.gameObject.transform.position) < UnityEngine.Vector3.Distance(rightHandPos, __instance.gameObject.transform.position))
            {
                Log.LogInfo("LeftHandPickupItem");
                _TrueGear.Play("LeftHandPickupItem");
            }
            else
            {
                Log.LogInfo("RightHandPickupItem");
                _TrueGear.Play("RightHandPickupItem");
            }
            Log.LogInfo("BaseKeyItemOnPickedUp");
            Log.LogInfo(__instance.ownerID);
        }

        private static bool isRightSpelling = false;
        private static bool isLeftSpelling = false;

        [HarmonyPostfix, HarmonyPatch(typeof(MagicOrbBehaviour), "UpdateItemGripAnimKey")]
        private static void MagicOrbBehaviour_UpdateItemGripAnimKey_Postfix(MagicOrbBehaviour __instance, GripAnimationName str)
        {
            if (localPlayerID != __instance.ownerID)
            {
                return;
            }
            Log.LogInfo("----------------------------------------------");
            if (!isRightHandSpell && canLeftSpell)
            {
                canLeftSpell = false;
                isLeftSpelling = true;
                Log.LogInfo("StartLeftHandSpell");
                _TrueGear.StartLeftHandSpell();
            }
            else if (isRightHandSpell && canRightSpell)
            {
                canRightSpell = false;
                isRightSpelling = true;
                Log.LogInfo("StartRightHandSpell");
                _TrueGear.StartRightHandSpell();
            }
            Log.LogInfo("UpdateItemGripAnimKey");
            Log.LogInfo(__instance.ownerID);
            Log.LogInfo(__instance.magicType);
            //Log.LogInfo($"leftHandPos :{leftHandPos.x},{leftHandPos.y},{leftHandPos.z}");
            //Log.LogInfo($"rightHandPos :{rightHandPos.x},{rightHandPos.y},{rightHandPos.z}");
            //Log.LogInfo($"Pos :{__instance.gameObject.transform.position.x},{__instance.gameObject.transform.position.y},{__instance.gameObject.transform.position.z}");
            //Log.LogInfo($"defaultLocalPos :{__instance.defaultLocalPos.x},{__instance.defaultLocalPos.y},{__instance.defaultLocalPos.z}");
            //Log.LogInfo(UnityEngine.Vector3.Distance(leftHandPos, __instance.gameObject.transform.position));
            //Log.LogInfo(UnityEngine.Vector3.Distance(rightHandPos, __instance.gameObject.transform.position));
        }

        //[HarmonyPrefix, HarmonyPatch(typeof(MagicOrbBehaviour), "ReleaseObjectFunction")]
        //private static void MagicOrbBehaviour_ReleaseObjectFunction_Postfix(MagicOrbBehaviour __instance)
        //{
        //    Log.LogInfo("----------------------------------------------");
        //    Log.LogInfo("MagicOrbReleaseObjectFunction");
        //    Log.LogInfo(__instance.ownerID);
        //}

        [HarmonyPrefix, HarmonyPatch(typeof(MagicOrbBehaviour), "OnDropped")]
        private static void MagicOrbBehaviour_OnDropped_Prefix(MagicOrbBehaviour __instance)
        {
            if (localPlayerID != __instance.ownerID)
            {
                return;
            }
            Log.LogInfo("----------------------------------------------");
            if (!isRightSpelling || !isLeftSpelling)
            {
                Log.LogInfo("StopLeftHandSpell");
                Log.LogInfo("StopRightHandSpell");
                _TrueGear.StopRightHandSpell();
                _TrueGear.StopLeftHandSpell();
                isRightSpelling = false;
                isLeftSpelling = false;
                return;
            }
            if (UnityEngine.Vector3.Distance(leftHandPos, __instance.gameObject.transform.position) < UnityEngine.Vector3.Distance(rightHandPos, __instance.gameObject.transform.position) && UnityEngine.Vector3.Distance(leftHandPos, __instance.gameObject.transform.position) < 3f)
            {
                isLeftSpelling = false;
                Log.LogInfo("StopLeftHandSpell");
                _TrueGear.StopLeftHandSpell();
            }
            else if (UnityEngine.Vector3.Distance(leftHandPos, __instance.gameObject.transform.position) >= UnityEngine.Vector3.Distance(rightHandPos, __instance.gameObject.transform.position) && UnityEngine.Vector3.Distance(rightHandPos, __instance.gameObject.transform.position) < 3f)
            {
                isRightSpelling = false;
                Log.LogInfo("StopRightHandSpell");
                _TrueGear.StopRightHandSpell();
            }
            Log.LogInfo($"Pos :{__instance.gameObject.transform.position.x},{__instance.gameObject.transform.position.y},{__instance.gameObject.transform.position.z}");

            Log.LogInfo("MagicOrbOnDropped");
            Log.LogInfo(__instance.ownerID);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(MagicOrbBehaviour), "TriggerMagicAOE")]
        private static void MagicOrbBehaviour_TriggerMagicAOE_Postfix(MagicOrbBehaviour __instance)
        {
            if (localPlayerID != __instance.ownerID)
            {
                return;
            }
            isAOEMAgic = true;
            Log.LogInfo("----------------------------------------------");
            Log.LogInfo("StartSpellAOEMagic");
            _TrueGear.StartSpellAOEMagic();
            Log.LogInfo(__instance.ownerID);
            Log.LogInfo(__instance.magicType);
            Log.LogInfo(__instance.orbVFX.name);
            Log.LogInfo(__instance.orbVFX.GetInstanceID());
        }




        [HarmonyPrefix, HarmonyPatch(typeof(SimpleParticleBehaviour), "EndParticle")]
        private static void SimpleParticleBehaviour_EndParticle_Postfix(SimpleParticleBehaviour __instance)
        {
            if (!__instance.name.Contains("VFX_AOE_"))
            {
                return;
            }
            if (!isAOEMAgic)
            {
                return;
            }
            UnityEngine.Vector3 effectPos = __instance.gameObject.transform.position;
            if (UnityEngine.Vector3.Distance(effectPos, playerPos) > 1f)
            {
                Log.LogInfo($"returnDis :{UnityEngine.Vector3.Distance(effectPos, playerPos)}");
                return;
            }
            isAOEMAgic = false;
            isLeftSpelling = false;
            isRightSpelling = false;
            Log.LogInfo("----------------------------------------------");
            Log.LogInfo("StopSpellAOEMagic");
            Log.LogInfo("StopLeftHandSpell");
            Log.LogInfo("StopRightHandSpell");
            _TrueGear.StopLeftHandSpell();
            _TrueGear.StopRightHandSpell();
            _TrueGear.StopSpellAOEMagic();
            Log.LogInfo(__instance.name);
            Log.LogInfo(__instance.GetInstanceID());
            Log.LogInfo($"EffectPos :{effectPos.x},{effectPos.y},{effectPos.z}");
            Log.LogInfo($"PlayerPos :{playerPos.x},{playerPos.y},{playerPos.z}");
            Log.LogInfo($"Dis :{UnityEngine.Vector3.Distance(effectPos, playerPos)}");
        }

    }
}
