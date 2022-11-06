using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;

namespace RNGReporter.Objects
{
    partial class FrameGenerator
    {
        public bool SearchForTrigger { get; set; }
        public int RerollCount { get; set; }
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
        public bool isBW2 { get; set; }

        public int GenderCase;

        public int pointer;

        public bool specificMod;
        public byte modValue;

        public uint CurrentRand() => rngList[pointer];
        public uint NextRand()
        {
            pointer++;
            return rngList[pointer];
        }
        public void Advance(int n)
        {
            pointer += n;
        }

        public List<Frame> GenerateG5PID(FrameCompare frameCompare, uint id, uint sid)
        {
            bool G5_Wild = EncounterType == EncounterType.Wild;
            bool G5_WildDarkGrass = EncounterType == EncounterType.WildDarkGrass;
            bool G5_WildShakerGrass = EncounterType == EncounterType.WildShakerGrass;
            bool G5_WildSurfing = EncounterType == EncounterType.WildSurfing;
            bool G5_WildSuperRod = EncounterType == EncounterType.WildSuperRod;
            bool G5_WildWaterSpot = EncounterType == EncounterType.WildWaterSpot;
            bool G5_WildFishingSpot = EncounterType == EncounterType.WildFishingSpot;
            bool G5_WildCaveSpot = EncounterType == EncounterType.WildCaveSpot;
            bool G5_WildSwarm = EncounterType == EncounterType.WildSwarm;
            bool G5_Roamer = EncounterType == EncounterType.Roamer;
            bool G5_Stationary = EncounterType == EncounterType.Stationary;
            bool G5_Gift = EncounterType == EncounterType.Gift;
            bool G5_Jellicent = EncounterType == EncounterType.JellicentHA;
            bool G5_LarvestaHappinyEgg = EncounterType == EncounterType.LarvestaHappiny;
            bool G5_Haxorus = EncounterType == EncounterType.Haxorus;
            bool G5_GibleDratini = EncounterType == EncounterType.GibleDratini;
            bool G5_Eevee = EncounterType == EncounterType.Eevee;
            bool G5_Deerling = EncounterType == EncounterType.Deerling;
            bool G5_Entralink = EncounterType == EncounterType.Entralink;
            bool G5_HiddenGrotto = EncounterType == EncounterType.HiddenGrotto;

            bool G5_WildEncounter =
                G5_Wild || G5_WildDarkGrass || G5_WildShakerGrass || G5_WildSurfing || 
                G5_WildSuperRod ||  G5_WildWaterSpot || G5_WildFishingSpot || G5_WildCaveSpot || G5_WildSwarm;

            bool TimeFinder5 = EncounterMod == EncounterMod.Search;            // WHY WAS THE SEARCHER ADDED TO ENCOUNTER MODS???
            bool IsSync = EncounterMod == EncounterMod.Synchronize;
            bool IsCuteCharm = EncounterMod == EncounterMod.CuteCharm;
            bool IsCompEyes = EncounterMod == EncounterMod.Compoundeyes;
            bool IsSuctionCups = EncounterMod == EncounterMod.SuctionCups;

            // Fix later
            if (InitialFrame <= 1)
                InitialFrame = 2;

            BWRng rng = new BWRng(InitialSeed);
            frames = new List<Frame>();
            var entreeTimer = new CGearTimer();

            bool DoubleEncounter = false;
            ulong CurrentRatio = 65535;
            int encounterSlot = 0;
            GenderCase = SynchNature;       // Use another variable to avoid confusion
            const uint item = 0;
            var mod = EncounterMod.None;
            uint idLower = (id & 1) ^ (sid & 1);

            for (uint cnt = 0; cnt < InitialFrame - 2; cnt++)
                rng.GetNext64BitNumber();

            rngList.Clear();
            for (uint cnt = 0; cnt < 20; cnt++)
                rngList.Add(rng.GetNext32BitNumber());

            for (uint cnt = 0; cnt < maxResults; cnt++, rngList.RemoveAt(0), rngList.Add(rng.GetNext32BitNumber()))
            {
                pointer = 1;

                uint CurrentFrame = cnt + InitialFrame;
                uint nature;
                uint pid;
                bool synchable;
                byte level = 0;


                if (G5_WildEncounter)
                {
                    #region Wild
                    if (G5_Wild || G5_WildSurfing || G5_WildWaterSpot || G5_WildFishingSpot)
                    {
                        if (G5_WildWaterSpot)
                            Advance(1);

                        if (TimeFinder5)
                        {
                            if (CheckLead(true))      //Cute Charm Success
                            {
                                if (SearchForTrigger)
                                    CurrentRatio = getRatio(NextRand());
                                encounterSlot = getSlot();
                                level = getLevel(NextRand());
                                CuteCharmModify(frameCompare, id, sid, idLower, CurrentFrame, CurrentRatio, encounterSlot, level, pointer);
                            }

                            // Restore the RNG state in order to check for Sync / No Lead
                            pointer = 0;
                            synchable = CheckLead(false);
                            if (SearchForTrigger)
                                CurrentRatio = getRatio(NextRand());
                            encounterSlot = getSlot();
                            level = getLevel(NextRand());
                            pid = FindPID(id, sid, idLower, false);
                            nature = (uint)(((ulong)NextRand() * 25) >> 32);
                            if (synchable && !frameCompare.CompareNature(nature))
                                mod = EncounterMod.Synchronize;
                            else
                                mod = EncounterMod.None;
                            CurrentFrame--;
                        }
                        else
                        {
                            if (!(IsCompEyes || IsSuctionCups))
                                synchable = CheckLead(IsCuteCharm);     // Temporarily use synchable for Cute charm
                            else
                                synchable = false;

                            if (IsCuteCharm && !synchable)
                                Advance(1);

                            if (SearchForTrigger)
                                CurrentRatio = getRatio(NextRand());

                            encounterSlot = getSlot();

                            level = getLevel(NextRand());

                            pid = FindPID(id, sid, idLower, IsCuteCharm && synchable);

                            if (IsSync && synchable)
                                nature = (uint)SynchNature;
                            else
                                nature = (uint)(((ulong)NextRand() * 25) >> 32);

                            if (IsCuteCharm)
                                synchable = false;

                        }
                    }

                    else if (G5_WildSwarm)
                    {
                        if (TimeFinder5)
                        {
                            if (CheckLead(true))      //Cute Charm Success
                            {
                                if (SearchForTrigger)
                                    CurrentRatio = getRatio(NextRand());
                                if (DoubleEnc_Swarm())
                                {
                                    encounterSlot = 12;
                                    Advance(1);
                                }
                                else
                                    encounterSlot = getSlot();
                                level = getLevel(NextRand());
                                CuteCharmModify(frameCompare, id, sid, idLower, CurrentFrame, CurrentRatio, encounterSlot, level, pointer);
                            }

                            // Restore the RNG state in order to check for Sync / No Lead
                            pointer = 0;
                            synchable = CheckLead(false);
                            if (SearchForTrigger)
                                CurrentRatio = getRatio(NextRand());
                            if (DoubleEnc_Swarm())
                            {
                                encounterSlot = 12;
                                Advance(1);
                            }
                            else
                                encounterSlot = getSlot();
                            level = getLevel(NextRand());
                            pid = FindPID(id, sid, idLower, false);
                            nature = (uint)(((ulong)NextRand() * 25) >> 32);
                            if (synchable && !frameCompare.CompareNature(nature))
                                mod = EncounterMod.Synchronize;
                            else
                                mod = EncounterMod.None;
                            CurrentFrame--;
                        }
                        else
                        {
                            if (!(IsCompEyes || IsSuctionCups))
                                synchable = CheckLead(IsCuteCharm);
                            else
                                synchable = false;

                            if (IsCuteCharm && !synchable)
                                Advance(1);

                            if (SearchForTrigger)
                                CurrentRatio = getRatio(NextRand());

                            if (DoubleEnc_Swarm())
                            {
                                encounterSlot = 12;
                                Advance(1);
                            }
                            else
                                encounterSlot = getSlot();

                            level = getLevel(NextRand());

                            pid = FindPID(id, sid, idLower, IsCuteCharm && synchable);

                            if (IsSync && synchable)
                                nature = (uint)SynchNature;
                            else
                                nature = (uint)(((ulong)NextRand() * 25) >> 32);

                            if (IsCuteCharm)
                                synchable = false;
                        }

                    }

                    else if (G5_WildCaveSpot)
                    {
                        if (TimeFinder5)
                        {
                            //Advance(1);
                            if (!CheckBattle())
                                continue;

                            // Let's do Suction Cups since it affects the hittable frames

                            encounterSlot = getSlot();
                            Advance(1);
                            pid = FindPID(id, sid, idLower, false);
                            nature = (uint)(((ulong)NextRand() * 25) >> 32);

                            frame = Frame.GenerateFrame5(FrameType.Method5Natures, EncounterType, CurrentFrame, rngList[0], rngList[1],
                                pid, id, sid, nature, false, encounterSlot, 0, item, 0, false);

                            if (frameCompare.Compare(frame))
                            {
                                frame.EncounterMod = EncounterMod.SuctionCups;
                                frames.Add(frame);
                            }


                            // Now for regular\Synchronize\Cute Charm encounters

                            // Restore the RNG state in order to check for Cute Charm
                            pointer = 0;
                            if (!CheckBattle())
                                continue;

                            if (CheckLead(true))      //Cute Charm Success
                            {
                                encounterSlot = getSlot();
                                Advance(1);
                                CuteCharmModify(frameCompare, id, sid, idLower, CurrentFrame, CurrentRatio, encounterSlot, 0, pointer);
                            }

                            // Restore the RNG state in order to check for Sync / No Lead
                            pointer = 0;
                            if (!CheckBattle())
                                continue;

                            synchable = CheckLead(false);
                            encounterSlot = getSlot();
                            Advance(1);
                            pid = FindPID(id, sid, idLower, false);
                            nature = (uint)(((ulong)NextRand() * 25) >> 32);
                            if (synchable && !frameCompare.CompareNature(nature))
                                mod = EncounterMod.Synchronize;
                            else
                                mod = EncounterMod.None;
                            CurrentFrame--;
                        }
                        else
                        {
                            bool battle = CheckBattle();

                            if (!(IsCompEyes || IsSuctionCups))
                                synchable = CheckLead(IsCuteCharm);         // Temporarily use synchable for Cute charm
                            else
                                synchable = false;

                            if (IsCuteCharm && !synchable)
                                Advance(1);

                            encounterSlot = battle ? getSlot() : FindItem();

                            Advance(1);     // Level is fixed for Cave spots slots

                            pid = FindPID(id, sid, idLower, IsCuteCharm && synchable);

                            if (IsSync && synchable)
                                nature = (uint)SynchNature;
                            else
                                nature = (uint)(((ulong)NextRand() * 25) >> 32);

                            if (IsCuteCharm)
                                synchable = false;
                        }

                    }

                    

                    else if (G5_WildDarkGrass)
                    {
                        if (TimeFinder5)
                        {
                            if (CheckLead(true))      //Cute Charm Success
                            {
                                DoubleEncounter = DoubleEnc_Swarm();
                                if (SearchForTrigger)
                                    CurrentRatio = getRatio(NextRand());
                                encounterSlot = getSlot();
                                if (DoubleEncounter)
                                    Advance(2);
                                Advance(1);
                                CuteCharmModify(frameCompare, id, sid, idLower, CurrentFrame, CurrentRatio, encounterSlot, 0, pointer);
                            }

                            // Restore the RNG state in order to check for Sync / No Lead
                            pointer = 0;
                            synchable = CheckLead(false);
                            DoubleEncounter = DoubleEnc_Swarm();
                            if (SearchForTrigger)
                                CurrentRatio = getRatio(NextRand());
                            encounterSlot = getSlot();
                            if (DoubleEncounter)
                                Advance(2);
                            Advance(1);
                            pid = FindPID(id, sid, idLower, false);
                            nature = (uint)(((ulong)NextRand() * 25) >> 32);
                            if (synchable && !frameCompare.CompareNature(nature))
                                mod = EncounterMod.Synchronize;
                            else
                                mod = EncounterMod.None;
                            CurrentFrame--;
                        }
                        else
                        {
                            if (!(IsCompEyes || IsSuctionCups))
                                synchable = CheckLead(IsCuteCharm);
                            else
                                synchable = false;

                            if (IsCuteCharm && !synchable)
                                Advance(1);

                            DoubleEncounter = DoubleEnc_Swarm();

                            if (SearchForTrigger)
                                CurrentRatio = getRatio(NextRand());

                            encounterSlot = getSlot();

                            if (DoubleEncounter)
                                Advance(2);

                            Advance(1);

                            pid = FindPID(id, sid, idLower, IsCuteCharm && synchable);

                            if (IsSync && synchable)
                                nature = (uint)SynchNature;
                            else
                                nature = (uint)(((ulong)NextRand() * 25) >> 32);

                            if (IsCuteCharm)
                                synchable = false;
                        }

                    }

                    else
                    {
                        // Fishing and Shaking Grass Spots should be seperated

                        if (TimeFinder5)
                        {
                            if (G5_WildSuperRod)
                            {
                                synchable = CheckLead(false);

                                if (getRatio(NextRand()) < 50)      //Successful Fishing encounter trigger
                                {
                                    encounterSlot = getSlot();
                                    level = getLevel(NextRand());
                                    pid = FindPID(id, sid, idLower, false);
                                    nature = (uint)(((ulong)NextRand() * 25) >> 32);

                                    frame = Frame.GenerateFrame5(FrameType.Method5Natures, EncounterType, CurrentFrame, rngList[0], rngList[1],
                                        pid, id, sid, nature, synchable, encounterSlot, level, item, 0, false);

                                    if (frameCompare.Compare(frame))
                                        frames.Add(frame);


                                    //Check Cute charm as well
                                    pointer = 1;
                                    if (CheckLead(true))      //Cute Charm Success
                                    {
                                        CurrentRatio = getRatio(NextRand());
                                        encounterSlot = getSlot();
                                        level = getLevel(NextRand());
                                        CuteCharmModify(frameCompare, id, sid, idLower, CurrentFrame, CurrentRatio, encounterSlot, level, pointer);
                                    }

                                }


                                // Check Suction Cups even if the encounter can be triggered without it
                                pointer = 2;
                                encounterSlot = getSlot();
                                level = getLevel(NextRand());
                                pid = FindPID(id, sid, idLower, false);
                                nature = (uint)(((ulong)NextRand() * 25) >> 32);

                                frame = Frame.GenerateFrame5(FrameType.Method5Natures, EncounterType, CurrentFrame, rngList[0], rngList[1],
                                    pid, id, sid, nature, false, encounterSlot, level, item, 0, false);

                                if (frameCompare.Compare(frame))
                                {
                                    frame.EncounterMod = EncounterMod.SuctionCups;
                                    frames.Add(frame);
                                }

                                // The search for this frame MUST stop here
                                continue;

                            }
                            // Searcher for Shaking Grass Spots
                            else
                            {
                                Advance(1);
                                if (CheckLead(true))      //Cute Charm Success
                                {
                                    encounterSlot = getSlot();
                                    Advance(1);
                                    CuteCharmModify(frameCompare, id, sid, idLower, CurrentFrame, CurrentRatio, encounterSlot, 0, pointer);
                                }

                                pointer = 1;
                                synchable = CheckLead(false);
                                encounterSlot = getSlot();
                                Advance(1);
                                pid = FindPID(id, sid, idLower, false);
                                nature = (uint)(((ulong)NextRand() * 25) >> 32);

                                if (synchable && !frameCompare.CompareNature(nature))
                                    mod = EncounterMod.Synchronize;
                                else
                                    mod = EncounterMod.None;
                                CurrentFrame--;
                            }

                        }
                        else
                        {
                            if (G5_WildSuperRod)
                            {
                                if (IsSuctionCups)
                                {
                                    synchable = false;
                                    Advance(1);
                                }
                                else
                                {
                                    synchable = CheckLead(IsCuteCharm);
                                    if (IsCuteCharm && !synchable)
                                        Advance(1);
                                    CurrentRatio = getRatio(NextRand());
                                }
                            }
                            else
                            {
                                Advance(1);

                                if (!(IsSuctionCups || IsCompEyes))
                                    synchable = CheckLead(IsCuteCharm);
                                else
                                    synchable = false;

                                if (IsCuteCharm && !synchable)
                                    Advance(1);
                            }

                            encounterSlot = getSlot();

                            level = getLevel(NextRand());

                            pid = FindPID(id, sid, idLower, IsCuteCharm && synchable);

                            if (IsSync && synchable)
                                nature = (uint)SynchNature;
                            else
                                nature = (uint)(((ulong)NextRand() * 25) >> 32);

                            if (IsCuteCharm)
                                synchable = false;

                        }
                    }
                    #endregion
                }
                else
                {
                    #region Not Wild
                    if (G5_Stationary)
                    {
                        if (TimeFinder5)
                        {
                            if (CheckLead(true))      //Cute Charm Success
                                CuteCharmModify(frameCompare, id, sid, idLower, CurrentFrame, 0, 0, 0, pointer);

                            // Restore the RNG state in order to check for Sync / No Lead
                            pointer = 0;
                            synchable = CheckLead(false);
                            pid = FindPID(id, sid, idLower, false);
                            nature = (uint)(((ulong)NextRand() * 25) >> 32);
                            if (synchable && !frameCompare.CompareNature(nature))
                                mod = EncounterMod.Synchronize;
                            else
                                mod = EncounterMod.None;
                            CurrentFrame--;
                        }
                        else
                        {
                            synchable = CheckLead(IsCuteCharm);

                            if (IsCuteCharm && !synchable)
                                Advance(1);

                            pid = FindPID(id, sid, idLower, IsCuteCharm && synchable);

                            if (IsSync && synchable)
                                nature = (uint)SynchNature;
                            else
                                nature = (uint)(((ulong)NextRand() * 25) >> 32);

                            if (IsCuteCharm)
                                synchable = false;
                        }

                    }

                    #region I have not checked Entralink yet

                    else if (G5_Entralink)
                    {
                        pid = rngList[1];

                        synchable = false;

                        // genderless
                        if (frameCompare.GenderFilter.GenderValue == 0xFF)
                        {
                            // leave it as-is
                            nature = (uint)(((ulong)rngList[5] * 25) >> 32);
                        }
                        // always female
                        else if (frameCompare.GenderFilter.GenderValue == 0xFE)
                        {
                            var genderAdjustment = (uint)((0x8 * (ulong)rngList[2]) >> 32);
                            pid = (pid & 0xFFFFFF00) | (genderAdjustment + 1);
                            nature = (uint)(((ulong)rngList[6] * 25) >> 32);
                        }
                        // always male
                        else if (frameCompare.GenderFilter.GenderValue == 0x0)
                        {
                            var genderAdjustment = (uint)((0xF6 * (ulong)rngList[2]) >> 32);
                            pid = (pid & 0xFFFFFF00) | (genderAdjustment + 8);
                            nature = (uint)(((ulong)rngList[6] * 25) >> 32);
                        }
                        else
                        {
                            if (frameCompare.GenderFilter.GenderCriteria == GenderCriteria.Male)
                            {
                                var genderAdjustment =
                                    (uint)
                                    (((0xFE - frameCompare.GenderFilter.GenderValue) * (ulong)rngList[2]) >>
                                     32);
                                pid = (pid & 0xFFFFFF00) |
                                      (genderAdjustment + frameCompare.GenderFilter.GenderValue);
                            }
                            else if (frameCompare.GenderFilter.GenderCriteria == GenderCriteria.Female)
                            {
                                var genderAdjustment =
                                    (uint)
                                    (((frameCompare.GenderFilter.GenderValue - 1) * (ulong)rngList[2]) >> 32);
                                pid = (pid & 0xFFFFFF00) | (genderAdjustment + 1);
                            }
                            nature = (uint)(((ulong)rngList[6] * 25) >> 32);
                        }
                        if ((pid & 0x10000) == 0x10000)
                            pid = pid ^ 0x10000;
                    }
                    #endregion

                    else if (G5_Jellicent)
                    {
                        if (!(IsCompEyes || IsSuctionCups))
                            synchable = CheckLead(IsCuteCharm);
                        else
                            synchable = false;

                        if (IsCuteCharm && !synchable)
                            Advance(1);

                        pid = ForcedGenderPID(id, sid, idLower, frameCompare, false);

                        nature = (uint)(((ulong)NextRand() * 25) >> 32);

                        if (synchable && IsSync)
                            nature = (uint)SynchNature;

                        if (TimeFinder5)
                        {
                            if (synchable && !frameCompare.CompareNature(nature))
                                mod = EncounterMod.Synchronize;
                            else
                                mod = EncounterMod.None;
                        }

                        if (IsCuteCharm)
                            synchable = false;
                    }

                    else if (G5_Gift || G5_Roamer)
                    {
                        pid = NextRand();
                        if (!G5_Roamer)
                            pid = pid ^ 0x10000;
                        nature = (uint)(((ulong)NextRand() * 25) >> 32);
                        synchable = false;
                    }
                    else if (G5_LarvestaHappinyEgg)
                    {
                        pid = NextRand();
                        Advance(1);
                        nature = (uint)(((ulong)NextRand() * 25) >> 32);
                        synchable = false;
                    }
                    else if (G5_Haxorus)
                    {
                        if (!(IsCompEyes || IsSuctionCups))
                            synchable = CheckLead(IsCuteCharm);
                        else
                            synchable = false;

                        if (IsCuteCharm && !synchable)
                            Advance(1);

                        pid = NextRand();

                        if (IsCuteCharm && synchable)
                            pid = Functions.CuteCharmModPID(pid, NextRand(), GenderCase);
                        pid = ForceShiny(pid, id, sid);
                        pid = pid ^ 0x10000;

                        if (specificMod && (pid % 256 != modValue))
                            continue;

                        nature = (uint)(((ulong)NextRand() * 25) >> 32);

                        if (synchable && IsSync)
                            nature = (uint)SynchNature;

                        if (TimeFinder5)
                        {
                            if (synchable && !frameCompare.CompareNature(nature))
                                mod = EncounterMod.Synchronize;
                            else
                                mod = EncounterMod.None;
                        }

                        if (IsCuteCharm)
                            synchable = false;
                    }
                    else if (G5_GibleDratini)
                    {
                        pid = NextRand();
                        pid = ModifyPIDGender(frameCompare, pid);
                        pid = ForceShiny(pid, id, sid);
                        if (specificMod && (pid % 256 != modValue))
                            continue;
                        nature = (uint)(((ulong)NextRand() * 25) >> 32);
                        synchable = false;
                    }
                    else if (G5_Eevee || G5_Deerling)
                    {
                        pid = NextRand() ^ 0x10000;

                        if (G5_Eevee)
                            pid = ModifyPIDGender(frameCompare, pid);

                        if (CheckShiny(id, sid, pid))
                            pid ^= 0x10000000;
                        nature = (uint)(((ulong)NextRand() * 25) >> 32);
                        synchable = false;
                    }

                    else if (G5_HiddenGrotto)
                    {
                        Advance(1);

                        if (!(IsCompEyes || IsSuctionCups))
                            synchable = CheckLead(IsCuteCharm);
                        else
                            synchable = false;

                        if (IsCuteCharm && !synchable)
                            Advance(1);

                        pid = ForcedGenderPID(id, sid, idLower, frameCompare, true);

                        nature = (uint)(((ulong)NextRand() * 25) >> 32);

                        if (synchable && IsSync)
                            nature = (uint)SynchNature;

                        if (IsCuteCharm)
                            synchable = false;
                    }
                    else
                    {
                        break;
                    }
                    #endregion
                }



                // worthless calculation
                //int ability = (int) (pid >> 16) & 1;

                if (RNGIVs != null)
                {
                    frame =
                        Frame.GenerateFrame(
                            FrameType.Method5Natures,
                            EncounterType,
                            CurrentFrame,
                            rngList[1],
                            pid,
                            id,
                            sid,
                            nature,
                            synchable,
                            encounterSlot,
                            item,
                            RNGIVs);
                }
                else
                {
                    frame =
                        Frame.GenerateFrame5(
                            FrameType.Method5Natures,
                            EncounterType,
                            CurrentFrame,
                            rngList[1],
                            rngList[2],
                            pid,
                            id,
                            sid,
                            nature,
                            synchable,
                            encounterSlot,
                            level,
                            item,
                            CurrentRatio, 
                            DoubleEncounter);
                }


                frame.EncounterMod = mod;
                frame.CGearTime = entreeTimer.GetTime(rngList[0]);

                if (frameCompare.Compare(frame))
                {
                    frames.Add(frame);
                }


                
            }
            
            return frames;

        }


        // Hidden Grotto, HA Jellicent
        private uint ForcedGenderPID(uint id, uint sid, uint idLower, FrameCompare frameCompare, bool shinyLocked)
        {
            uint pid = 0;
            for (int i = 0; i < RerollCount; i++)
            {
                pid = NextRand() ^ 0x10000;

                pid = ModifyPIDGender(frameCompare, pid);

                if (!shinyLocked)
                    pid = TIDBitTwiddle(idLower, pid);

                if (CheckShiny(id, sid, pid))
                {
                    if (shinyLocked)
                    {
                        // Force non shiny but keep rerolling LOL
                        pid ^= 0x10000000;
                    }
                    else
                        return pid;
                }
            }
            return pid;
        }
        private uint ModifyPIDGender(FrameCompare frameCompare, uint pid)
        {
            // always female
            if (frameCompare.GenderFilter.GenderValue == 0xFE)
            {
                var genderAdjustment = (uint)((0x8 * (ulong)NextRand()) >> 32);
                pid = (pid & 0xFFFFFF00) | (genderAdjustment + 1);
            }
            // always male
            else if (frameCompare.GenderFilter.GenderValue == 0x0)
            {
                var genderAdjustment = (uint)((0xF6 * (ulong)NextRand()) >> 32);
                pid = (pid & 0xFFFFFF00) | (genderAdjustment + 8);
            }
            else
            {
                if (frameCompare.GenderFilter.GenderCriteria == GenderCriteria.Male)
                {
                    var genderAdjustment = (uint)(((0xFE - frameCompare.GenderFilter.GenderValue) * (ulong)NextRand()) >> 32);
                    pid = (pid & 0xFFFFFF00) | (genderAdjustment + frameCompare.GenderFilter.GenderValue);
                }
                else if (frameCompare.GenderFilter.GenderCriteria == GenderCriteria.Female)
                {
                    var genderAdjustment = (uint)(((frameCompare.GenderFilter.GenderValue - 1) * (ulong)NextRand()) >> 32);
                    pid = (pid & 0xFFFFFF00) | (genderAdjustment + 1);
                }
            }
            return pid;
        }


        // For cases the are not shiny locked and are affected by shiny charm
        private uint FindPID(uint id, uint sid, uint idLower, bool CC_Success)
        {
            uint pid = 0;
            for (int i = 0; i < RerollCount; i++)
            {
                pid = NextRand() ^ 0x10000;

                if (CC_Success)
                    pid = Functions.CuteCharmModPID(pid, NextRand(), GenderCase);

                pid = TIDBitTwiddle(idLower, pid);

                if (CheckShiny(id, sid, pid))
                    return pid;
            }
            return pid;
        }

        private uint TIDBitTwiddle(uint idLower, uint pid)
        {
            if ((idLower ^ (pid & 1) ^ (pid >> 31)) == 1)
                pid = pid ^ 0x80000000;
            return pid;
        }

        private bool CheckShiny(uint id, uint sid, uint pid)
        {
            uint tid = (id & 0xffff) | ((sid & 0xffff) << 16);
            uint a = pid ^ tid;
            uint b = a & 0xffff;
            uint c = (a >> 16) & 0xffff;
            uint d = b ^ c;

            return d < 8;
        }

        private void CuteCharmModify(FrameCompare frameCompare, uint id, uint sid, uint idLower, 
            uint CurrentFrame, ulong CurrentRatio, int encounterSlot, byte level, int p)
        {
            // Add all the Cute Charm possibilities
            for (GenderCase = -4; GenderCase < 5; GenderCase++)
            {
                if (GenderCase == 0)
                    continue;

                // Restore the RNG state
                pointer = p;

                frame = Frame.GenerateFrame5(FrameType.Method5Natures, 
                    EncounterType,
                    CurrentFrame,                                   //cnt + InitialFrame
                    rngList[0],
                    rngList[1],
                    FindPID(id, sid, idLower, true),                //PID. CC_Success is always true at this point
                    id,
                    sid,
                    (uint)(((ulong)NextRand() * 25) >> 32),         //Nature
                    false,                                          //Synchable. False for Cute Charm obviously
                    encounterSlot,
                    level,
                    0,                                              //Item
                    CurrentRatio,
                    false);

                switch (GenderCase)
                {
                    case 1: frame.EncounterMod = EncounterMod.CuteCharm50M; break;
                    case 2: frame.EncounterMod = EncounterMod.CuteCharm75M; break;
                    case 3: frame.EncounterMod = EncounterMod.CuteCharm25M; break;
                    case 4: frame.EncounterMod = EncounterMod.CuteCharm875M; break;
                    case -1: frame.EncounterMod = EncounterMod.CuteCharm50F; break;
                    case -2: frame.EncounterMod = EncounterMod.CuteCharm75F; break;
                    case -3: frame.EncounterMod = EncounterMod.CuteCharm25F; break;
                    case -4: frame.EncounterMod = EncounterMod.CuteCharm125F; break;
                }

                if (frameCompare.Compare(frame))
                {
                    frames.Add(frame);
                }

            }
        }

        private bool CheckLead(bool CC)
        {
            if (CC)     //Cute Charm check
                return (((ulong)NextRand() * 0xFFFF) >> 32) / 656 < 67;
            else
                return (NextRand() >> 31) == 1;
        }

        private int getSlot()
        {
            return EncounterSlotCalc.encounterSlot(NextRand(), frameType, EncounterType, isBW2);
        }

        public byte getLevel(ulong seed) => (byte)((uint)(seed * 100 >> 32) % (MaxLevel - MinLevel + 1) + MinLevel);

        private bool CheckBattle() => ((ulong)NextRand() * 1000 >> 32) < 400;

        private bool DoubleEnc_Swarm() => ((ulong)NextRand() * 100 >> 32) < 40;


        private ulong getRatio(uint seed)
        {
            return (((ulong)seed) >> 16) / 656;
            //return ((((ulong)seed) * 0xFFFF) >> 32) / 0x290; // -> Alternative
        }

        //Item calculation has minor mistakes, maybe check later
        private int FindItem()
        {
            uint calc = ((ulong)CurrentRand() * 1000 >> 32) < 100 ? 1000u : 1700u;
            uint result = (uint)((ulong)NextRand() * calc >> 32) / 100;

            if (calc == 1000)
                return (int)result + 13;
            else
                return (int)result + 23;
        }

    }

}
