using System.Collections.Generic;
using System.Threading;
using System.IO;
using System;
using TrueGearSDK;
using System.Linq;



namespace MyTrueGear
{
    public class TrueGearMod
    {
        private static TrueGearPlayer _player = null;

        private static ManualResetEvent leftHandCrystalPressMRE = new ManualResetEvent(false);
        private static ManualResetEvent rightHandCrystalPressMRE = new ManualResetEvent(false);
        private static ManualResetEvent spellAOEMagicMRE = new ManualResetEvent(false);
        private static ManualResetEvent leftHandSpellMRE = new ManualResetEvent(false);
        private static ManualResetEvent rightHandSpellMRE = new ManualResetEvent(false);
        private static ManualResetEvent teleportMRE = new ManualResetEvent(false);
        private static ManualResetEvent leftHandPickupMRE = new ManualResetEvent(false);
        private static ManualResetEvent rightHandPickupMRE = new ManualResetEvent(false);

        private static int leftSpellCount = 0;
        private static int rightSpellCount = 0;
        public TrueGearMod()
        {
            _player = new TrueGearPlayer("1341490","HellsweeperVR");
            _player.PreSeekEffect("DefaultDamage");
            _player.Start();
            new Thread(new ThreadStart(this.LeftHandCrystalPress)).Start();
            new Thread(new ThreadStart(this.RightHandCrystalPress)).Start();
            new Thread(new ThreadStart(this.SpellAOEMagic)).Start();
            new Thread(new ThreadStart(this.LeftHandSpell)).Start();
            new Thread(new ThreadStart(this.RightHandSpell)).Start();
            new Thread(new ThreadStart(this.Teleport)).Start();
            new Thread(new ThreadStart(this.LeftHandPickup)).Start();
            new Thread(new ThreadStart(this.RightHandPickup)).Start();
        }

        public void LeftHandCrystalPress()
        {
            while (true)
            {
                leftHandCrystalPressMRE.WaitOne();
                _player.SendPlay("LeftHandCrystalPress");
                Thread.Sleep(130);
            }            
        }

        public void RightHandCrystalPress()
        {
            while (true)
            {
                rightHandCrystalPressMRE.WaitOne();
                _player.SendPlay("RightHandCrystalPress");
                Thread.Sleep(130);
            }
        }

        public void SpellAOEMagic()
        {
            while (true)
            {
                spellAOEMagicMRE.WaitOne();
                _player.SendPlay("SpellAOEMagic");
                Thread.Sleep(130);
            }
        }

        public void LeftHandSpell()
        {
            while(true)
            {
                leftHandSpellMRE.WaitOne();
                _player.SendPlay("LeftHandSpell");
                leftSpellCount++;
                if (leftSpellCount >= 76)
                { 
                    StopLeftHandSpell();
                }
                Thread.Sleep(130);
            }
            
        }

        public void RightHandSpell()
        {
            while(true)
            {
                rightHandSpellMRE.WaitOne();
                _player.SendPlay("RightHandSpell");
                rightSpellCount++;
                if (rightSpellCount >= 76)
                {
                    StopRightHandSpell();
                }
                Thread.Sleep(130);
            }            
        }

        public void Teleport()
        {
            while (true)
            {
                teleportMRE.WaitOne();
                _player.SendPlay("Teleport");
                Thread.Sleep(250);
                StopTeleport();
            }
        }

        public void LeftHandPickup()
        {
            while (true)
            {
                leftHandPickupMRE.WaitOne();
                _player.SendPlay("LeftHandPickupItem");
                Thread.Sleep(130);
                StopLeftHandPickup();
            }
        }

        public void RightHandPickup()
        {
            while (true)
            {
                rightHandPickupMRE.WaitOne();
                _player.SendPlay("RightHandPickupItem");
                Thread.Sleep(130);
                StopRightHandPickup();
            }
        }


        public void Play(string Event)
        { 
            _player.SendPlay(Event);
        }

        public void PlayAngle(string tmpEvent, float tmpAngle, float tmpVertical)
        {
            try
            {
                float angle = (tmpAngle - 22.5f) > 0f ? tmpAngle - 22.5f : 360f - tmpAngle;
                int horCount = (int)(angle / 45) + 1;

                int verCount = tmpVertical > 0.1f ? -4 : tmpVertical < 0f ? 8 : 0;

                TrueGearSDK.EffectObject oriObject = _player.FindEffectByUuid(tmpEvent);
                TrueGearSDK.EffectObject rootObject = TrueGearSDK.EffectObject.Copy(oriObject);

                foreach (TrackObject track in rootObject.trackList)
                {
                    if (track.action_type == ActionType.Shake)
                    {
                        for (int i = 0; i < track.index.Length; i++)
                        {
                            if (verCount != 0)
                            {
                                track.index[i] += verCount;
                            }
                            if (horCount < 8)
                            {
                                if (track.index[i] < 50)
                                {
                                    int remainder = track.index[i] % 4;
                                    if (horCount <= remainder)
                                    {
                                        track.index[i] = track.index[i] - horCount;
                                    }
                                    else if (horCount <= (remainder + 4))
                                    {
                                        var num1 = horCount - remainder;
                                        track.index[i] = track.index[i] - remainder + 99 + num1;
                                    }
                                    else
                                    {
                                        track.index[i] = track.index[i] + 2;
                                    }
                                }
                                else
                                {
                                    int remainder = 3 - (track.index[i] % 4);
                                    if (horCount <= remainder)
                                    {
                                        track.index[i] = track.index[i] + horCount;
                                    }
                                    else if (horCount <= (remainder + 4))
                                    {
                                        var num1 = horCount - remainder;
                                        track.index[i] = track.index[i] + remainder - 99 - num1;
                                    }
                                    else
                                    {
                                        track.index[i] = track.index[i] - 2;
                                    }
                                }
                            }
                        }
                        if (track.index != null)
                        {
                            track.index = track.index.Where(i => !(i < 0 || (i > 19 && i < 100) || i > 119)).ToArray();
                        }
                    }
                    else if (track.action_type == ActionType.Electrical)
                    {
                        for (int i = 0; i < track.index.Length; i++)
                        {
                            if (horCount <= 4)
                            {
                                track.index[i] = 0;
                            }
                            else
                            {
                                track.index[i] = 100;
                            }
                            if (horCount == 1 || horCount == 8 || horCount == 4 || horCount == 5)
                            {
                                track.index = new int[2] { 0, 100 };
                            }
                        }
                    }
                }
                _player.SendPlayEffectByContent(rootObject);
            }
            catch(Exception ex)
            { 
                Console.WriteLine("TrueGear Mod PlayAngle Error :" + ex.Message);
                _player.SendPlay(tmpEvent);
            }          
        }

        public void StartLeftHandCrystalPress()
        {
            leftHandCrystalPressMRE.Set();
        }

        public void StopLeftHandCrystalPress()
        {
            leftHandCrystalPressMRE.Reset();
        }

        public void StartRightHandCrystalPress()
        {
            rightHandCrystalPressMRE.Set();
        }

        public void StopRightHandCrystalPress()
        {
            rightHandCrystalPressMRE.Reset();
        }

        public void StartSpellAOEMagic()
        {
            spellAOEMagicMRE.Set();
        }

        public void StopSpellAOEMagic()
        {
            spellAOEMagicMRE.Reset();
        }

        public void StartLeftHandSpell()
        {
            leftHandSpellMRE.Set();
            leftSpellCount = 0;
        }

        public void StopLeftHandSpell()
        {
            leftHandSpellMRE.Reset();
            leftSpellCount = 0;
        }

        public void StartRightHandSpell()
        {
            rightHandSpellMRE.Set();
            rightSpellCount = 0;
        }

        public void StopRightHandSpell()
        {
            rightHandSpellMRE.Reset();
            rightSpellCount = 0;
        }

        public void StartTeleport()
        {
            teleportMRE.Set();
        }

        public void StopTeleport()
        {
            teleportMRE.Reset();
        }

        public void StartLeftHandPickup()
        {
            leftHandPickupMRE.Set();
        }

        public void StopLeftHandPickup()
        {
            leftHandPickupMRE.Reset();
        }

        public void StartRightHandPickup()
        {
            rightHandPickupMRE.Set();
        }

        public void StopRightHandPickup()
        {
            rightHandPickupMRE.Reset();
        }


    }
}
