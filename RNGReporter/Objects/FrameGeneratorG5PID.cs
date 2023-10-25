using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RNGReporter.Objects
{
    partial class FrameGenerator
    {
        public uint MinAdvances { get; set; }
        public bool SearchForTrigger { get; set; }
        public int RerollCount { get; set; }
        public int CurrentLuckyPowerLVL { get; set; }   // The current Lucky Power level we are testing. Variable 
        public int MaxLuckyPowerLVL { get; set; }       // The total number of levels unlocked. Constant
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
        public bool isBW2 { get; set; }
        public bool MemoryLinkUsed { get; set; }

        public int GenderCase;

        public int pointer;

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

        #region Gen 5 PID
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
            bool G5_WildShadow = EncounterType == EncounterType.WildShadow;
            bool G5_WildSwarm = EncounterType == EncounterType.WildSwarm;
            bool G5_Roamer = EncounterType == EncounterType.Roamer;
            bool G5_Stationary = EncounterType == EncounterType.Stationary;
            bool G5_Gift = EncounterType == EncounterType.Gift;
            bool G5_Jellicent = EncounterType == EncounterType.JellicentHA;
            bool G5_LarvestaHappinyEgg = EncounterType == EncounterType.LarvestaHappiny;
            bool G5_Haxorus = EncounterType == EncounterType.Haxorus;
            bool G5_GibleDratini = EncounterType == EncounterType.GibleDratini;
            bool G5_Entralink = EncounterType == EncounterType.Entralink;
            bool G5_HiddenGrotto = EncounterType == EncounterType.HiddenGrotto;

            bool G5_WildEncounter =
                G5_Wild || G5_WildDarkGrass || G5_WildShakerGrass || G5_WildSurfing || 
                G5_WildSuperRod ||  G5_WildWaterSpot || G5_WildFishingSpot || G5_WildCaveSpot || G5_WildShadow || G5_WildSwarm;

            bool TimeFinder5 = EncounterMod == EncounterMod.Search;            // WHY WAS THE SEARCHER ADDED TO ENCOUNTER MODS???
            bool IsSync = EncounterMod == EncounterMod.Synchronize;
            bool IsCuteCharm = EncounterMod == EncounterMod.CuteCharm;
            bool IsCompEyes = EncounterMod == EncounterMod.Compoundeyes;
            bool IsSuctionCups = EncounterMod == EncounterMod.SuctionCups;

            ulong BattleParam = (ulong)(G5_WildCaveSpot ? 400 : 200);

            // Fix later
            if (InitialFrame <= 1)
                InitialFrame = 2;

            // Memory Link affects these methods only
            if (!(G5_Wild || G5_WildDarkGrass || G5_WildSurfing || G5_WildSwarm))
                this.MemoryLinkUsed = false;

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

            uint StartingFrame = MinAdvances;
            if (MinAdvances < InitialFrame)
                StartingFrame = InitialFrame;

            for (uint cnt = 0; cnt < StartingFrame - 2; cnt++)
                rng.GetNext64BitNumber();

            rngList.Clear();
            for (uint cnt = 0; cnt < 20; cnt++)
                rngList.Add(rng.GetNext32BitNumber());

            for (uint cnt = 0; cnt < maxResults; cnt++, rngList.RemoveAt(0), rngList.Add(rng.GetNext32BitNumber()))
            {
                pointer = 1;

                uint CurrentFrame = cnt + StartingFrame;
                uint nature = 0;
                uint pid = 0;
                bool synchable = false;
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
                            for (CurrentLuckyPowerLVL = 0; CurrentLuckyPowerLVL <= MaxLuckyPowerLVL; CurrentLuckyPowerLVL++)
                            {
                                // Each time we need to check another Lucky Power level, we need to restore the RNG state
                                pointer = 1;
                                if (CheckLead(true))      //Cute Charm Success
                                {
                                    if (SearchForTrigger)
                                        CurrentRatio = getRatio(NextRand());
                                    if (MemoryLinkUsed)
                                        Advance(1);
                                    encounterSlot = getSlot(CurrentLuckyPowerLVL);
                                    level = getLevel(NextRand());
                                    CuteCharmModify(frameCompare, id, sid, idLower, CurrentFrame, CurrentRatio, encounterSlot, level, pointer);
                                }

                                // Restore the RNG state in order to check for Sync / No Lead
                                pointer = 1;
                                synchable = CheckLead(false);
                                if (SearchForTrigger)
                                    CurrentRatio = getRatio(NextRand());
                                if (MemoryLinkUsed)
                                    Advance(1);
                                encounterSlot = getSlot(CurrentLuckyPowerLVL);
                                level = getLevel(NextRand());
                                pid = FindPID(id, sid, idLower, false);
                                nature = (uint)(((ulong)NextRand() * 25) >> 32);
                                if (synchable && !frameCompare.CompareNature(nature))
                                    mod = EncounterMod.Synchronize;
                                else
                                    mod = EncounterMod.None;

                                Finalize(frameCompare, CurrentFrame, pid, id, sid, nature, synchable, encounterSlot, level, 0, 
                                    CurrentRatio, false, 0, mod);
                            }
                            
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

                            if (MemoryLinkUsed)
                                Advance(1);

                            encounterSlot = getSlot(CurrentLuckyPowerLVL);

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
                            for (CurrentLuckyPowerLVL = 0; CurrentLuckyPowerLVL <= MaxLuckyPowerLVL; CurrentLuckyPowerLVL++)
                            {
                                pointer = 1;
                                if (CheckLead(true))      //Cute Charm Success
                                {
                                    if (SearchForTrigger)
                                        CurrentRatio = getRatio(NextRand());
                                    if (MemoryLinkUsed)
                                        Advance(1);
                                    if (DoubleEnc_Swarm())
                                    {
                                        encounterSlot = 12;
                                        Advance(1);
                                    }
                                    else
                                        encounterSlot = getSlot(CurrentLuckyPowerLVL);
                                    level = getLevel(NextRand());
                                    CuteCharmModify(frameCompare, id, sid, idLower, CurrentFrame, CurrentRatio, encounterSlot, level, pointer);
                                }

                                // Restore the RNG state in order to check for Sync / No Lead
                                pointer = 1;
                                synchable = CheckLead(false);
                                if (SearchForTrigger)
                                    CurrentRatio = getRatio(NextRand());
                                if (MemoryLinkUsed)
                                    Advance(1);
                                if (DoubleEnc_Swarm())
                                {
                                    encounterSlot = 12;
                                    Advance(1);
                                }
                                else
                                    encounterSlot = getSlot(CurrentLuckyPowerLVL);
                                level = getLevel(NextRand());
                                pid = FindPID(id, sid, idLower, false);
                                nature = (uint)(((ulong)NextRand() * 25) >> 32);
                                if (synchable && !frameCompare.CompareNature(nature))
                                    mod = EncounterMod.Synchronize;
                                else
                                    mod = EncounterMod.None;

                                Finalize(frameCompare, CurrentFrame, pid, id, sid, nature, synchable, encounterSlot, level, 0,
                                    CurrentRatio, false, 0, mod);
                            }  
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

                            if (MemoryLinkUsed)
                                Advance(1);

                            if (DoubleEnc_Swarm())
                            {
                                encounterSlot = 12;
                                Advance(1);
                            }
                            else
                                encounterSlot = getSlot(CurrentLuckyPowerLVL);

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

                    else if (G5_WildCaveSpot || G5_WildShadow)
                    {
                        if (TimeFinder5)
                        {
                            if (!CheckBattle(BattleParam))
                                continue;

                            for (CurrentLuckyPowerLVL = 0; CurrentLuckyPowerLVL <= MaxLuckyPowerLVL; CurrentLuckyPowerLVL++)
                            {
                                // Let's do Suction Cups since it affects the hittable frames
                                pointer = 2;
                                encounterSlot = getSlot(CurrentLuckyPowerLVL);
                                Advance(1);
                                pid = FindPID(id, sid, idLower, false);
                                nature = (uint)(((ulong)NextRand() * 25) >> 32);

                                frame = Frame.GenerateFrame5(FrameType.Method5Natures, EncounterType, CurrentFrame, rngList[0], rngList[1],
                                    pid, id, sid, nature, false, encounterSlot, 0, item, 0, false, CurrentLuckyPowerLVL);

                                if (frameCompare.Compare(frame))
                                {
                                    frame.EncounterMod = EncounterMod.SuctionCups;
                                    frames.Add(frame);
                                }

                                // Now for regular\Synchronize\Cute Charm encounters

                                // Restore the RNG state in order to check for Cute Charm
                                pointer = 2;

                                if (CheckLead(true))      //Cute Charm Success
                                {
                                    encounterSlot = getSlot(CurrentLuckyPowerLVL);
                                    Advance(1);
                                    CuteCharmModify(frameCompare, id, sid, idLower, CurrentFrame, CurrentRatio, encounterSlot, 0, pointer);
                                }

                                // Restore the RNG state in order to check for Sync / No Lead
                                pointer = 2;

                                synchable = CheckLead(false);
                                encounterSlot = getSlot(CurrentLuckyPowerLVL);
                                Advance(1);
                                pid = FindPID(id, sid, idLower, false);
                                nature = (uint)(((ulong)NextRand() * 25) >> 32);
                                if (synchable && !frameCompare.CompareNature(nature))
                                    mod = EncounterMod.Synchronize;
                                else
                                    mod = EncounterMod.None;

                                Finalize(frameCompare, CurrentFrame, pid, id, sid, nature, synchable, encounterSlot, level, 0,
                                    CurrentRatio, false, 0, mod);
                            }
                        }
                        else
                        {
                            bool battle = CheckBattle(BattleParam);

                            if (!(IsCompEyes || IsSuctionCups))
                                synchable = CheckLead(IsCuteCharm);         // Temporarily use synchable for Cute charm
                            else
                                synchable = false;

                            if (IsCuteCharm && !synchable)
                                Advance(1);

                            encounterSlot = battle ? getSlot(CurrentLuckyPowerLVL) : FindItem(G5_WildCaveSpot);

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
                            for (CurrentLuckyPowerLVL = 0; CurrentLuckyPowerLVL <= MaxLuckyPowerLVL; CurrentLuckyPowerLVL++)
                            {
                                pointer = 1;
                                if (CheckLead(true))      //Cute Charm Success
                                {
                                    DoubleEncounter = DoubleEnc_Swarm();
                                    if (SearchForTrigger)
                                        CurrentRatio = getRatio(NextRand());
                                    if (MemoryLinkUsed)
                                        Advance(1);
                                    encounterSlot = getSlot(CurrentLuckyPowerLVL);
                                    if (DoubleEncounter)
                                        Advance(2);
                                    Advance(1);
                                    CuteCharmModify(frameCompare, id, sid, idLower, CurrentFrame, CurrentRatio, encounterSlot, 0, pointer);
                                }

                                // Restore the RNG state in order to check for Sync / No Lead
                                pointer = 1;
                                synchable = CheckLead(false);
                                DoubleEncounter = DoubleEnc_Swarm();
                                if (SearchForTrigger)
                                    CurrentRatio = getRatio(NextRand());
                                if (MemoryLinkUsed)
                                    Advance(1);
                                encounterSlot = getSlot(CurrentLuckyPowerLVL);
                                if (DoubleEncounter)
                                    Advance(2);
                                Advance(1);
                                pid = FindPID(id, sid, idLower, false);
                                nature = (uint)(((ulong)NextRand() * 25) >> 32);
                                if (synchable && !frameCompare.CompareNature(nature))
                                    mod = EncounterMod.Synchronize;
                                else
                                    mod = EncounterMod.None;

                                Finalize(frameCompare, CurrentFrame, pid, id, sid, nature, synchable, encounterSlot, level, item,
                                    CurrentRatio, DoubleEncounter, entreeTimer.GetTime(rngList[0]), mod);
                            }  
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

                            if (MemoryLinkUsed)
                                Advance(1);

                            encounterSlot = getSlot(CurrentLuckyPowerLVL);

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
                                for (CurrentLuckyPowerLVL = 0; CurrentLuckyPowerLVL <= MaxLuckyPowerLVL; CurrentLuckyPowerLVL++)
                                {
                                    pointer = 1;
                                    synchable = CheckLead(false);
                                    if (getRatio(NextRand()) < 50)      //Successful Fishing encounter trigger - No lead required
                                    {
                                        encounterSlot = getSlot(CurrentLuckyPowerLVL);
                                        level = getLevel(NextRand());
                                        pid = FindPID(id, sid, idLower, false);
                                        nature = (uint)(((ulong)NextRand() * 25) >> 32);
                                        Finalize(frameCompare, CurrentFrame, pid, id, sid, nature, synchable, encounterSlot, level, 0,
                                            CurrentRatio, false, 0, mod);

                                        //Check Cute charm
                                        pointer = 1;
                                        if (CheckLead(true))      //Cute Charm Success
                                        {
                                            CurrentRatio = getRatio(NextRand());
                                            encounterSlot = getSlot(CurrentLuckyPowerLVL);
                                            level = getLevel(NextRand());
                                            CuteCharmModify(frameCompare, id, sid, idLower, CurrentFrame, CurrentRatio, encounterSlot, level, pointer);
                                        }
                                    }

                                    // Check Suction Cups even if the encounter can be triggered without it
                                    pointer = 2;
                                    encounterSlot = getSlot(CurrentLuckyPowerLVL);
                                    level = getLevel(NextRand());
                                    pid = FindPID(id, sid, idLower, false);
                                    nature = (uint)(((ulong)NextRand() * 25) >> 32);

                                    frame = Frame.GenerateFrame5(FrameType.Method5Natures, EncounterType, CurrentFrame, rngList[0], rngList[1],
                                        pid, id, sid, nature, false, encounterSlot, level, item, 0, false, CurrentLuckyPowerLVL);

                                    if (frameCompare.Compare(frame))
                                    {
                                        frame.EncounterMod = EncounterMod.SuctionCups;
                                        frames.Add(frame);
                                    }
                                }

                            }

                            // Searcher for Shaking Grass Spots
                            else
                            {
                                //Advance(1);   // Not useful since pointer will be set to 2 anyway

                                for (CurrentLuckyPowerLVL = 0; CurrentLuckyPowerLVL <= MaxLuckyPowerLVL; CurrentLuckyPowerLVL++)
                                {
                                    pointer = 2;
                                    if (CheckLead(true))      //Cute Charm Success
                                    {
                                        encounterSlot = getSlot(CurrentLuckyPowerLVL);
                                        Advance(1);
                                        CuteCharmModify(frameCompare, id, sid, idLower, CurrentFrame, CurrentRatio, encounterSlot, 0, pointer);
                                    }

                                    pointer = 2;
                                    synchable = CheckLead(false);
                                    encounterSlot = getSlot(CurrentLuckyPowerLVL);
                                    Advance(1);
                                    pid = FindPID(id, sid, idLower, false);
                                    nature = (uint)(((ulong)NextRand() * 25) >> 32);

                                    if (synchable && !frameCompare.CompareNature(nature))
                                        mod = EncounterMod.Synchronize;
                                    else
                                        mod = EncounterMod.None;

                                    Finalize(frameCompare, CurrentFrame, pid, id, sid, nature, synchable, encounterSlot, level, 0,
                                        CurrentRatio, false, 0, mod);
                                }
                                    
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

                            encounterSlot = getSlot(CurrentLuckyPowerLVL);

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
                        nature = (uint)(((ulong)NextRand() * 25) >> 32);
                        synchable = false;
                    }
                    /*else if (G5_Eevee || G5_Deerling)
                    {
                        pid = NextRand() ^ 0x10000;

                        if (G5_Eevee)
                            pid = ModifyPIDGender(frameCompare, pid);

                        if (CheckShiny(id, sid, pid))
                            pid ^= 0x10000000;
                        nature = (uint)(((ulong)NextRand() * 25) >> 32);
                        synchable = false;
                    }*/

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

                if (!(TimeFinder5 && G5_WildEncounter))
                    Finalize(frameCompare, CurrentFrame, pid, id, sid, nature, synchable, encounterSlot, level, item,
                                    CurrentRatio, DoubleEncounter, entreeTimer.GetTime(rngList[0]), mod);

            }

            return frames;

        }

        public void Finalize(FrameCompare frameCompare, uint CurrentFrame, uint pid, uint id, uint sid, uint nature, bool synchable,
            int encounterSlot, byte level, uint item, ulong CurrentRatio, bool DoubleEncounter, uint entreeTimer, EncounterMod mod)
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
                        DoubleEncounter,
                        CurrentLuckyPowerLVL);

            frame.EncounterMod = mod;
            frame.CGearTime = entreeTimer;

            if (frameCompare.Compare(frame))
            {
                frames.Add(frame);
            }
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


        // For everything not shiny locked and affected by shiny charm
        private uint FindPID(uint id, uint sid, uint idLower, bool CC_Success)
        {
            uint pid = 0;
            for (int i = 0; i < RerollCount + (CurrentLuckyPowerLVL >= 3 ? 1 : 0); i++)
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
            uint CurrentFrame, ulong CurrentRatio, int enctrSlot, byte level, int p)
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
                enctrSlot,
                level,
                0,                                              //Item
                CurrentRatio,
                false,
                CurrentLuckyPowerLVL);                          //The current Level of Lucky Power Tested

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


        private int getSlot(int currentLuckyPowerLVL)
        {
            return EncounterSlotCalc.encounterSlotG5(NextRand(), EncounterType, isBW2, currentLuckyPowerLVL);
        }

        public byte getLevel(ulong seed) => (byte)((uint)(seed * 100 >> 32) % (MaxLevel - MinLevel + 1) + MinLevel);

        private bool CheckBattle(ulong battleParam) => ((ulong)NextRand() * 1000 >> 32) < battleParam;

        private bool DoubleEnc_Swarm() => ((ulong)NextRand() * 100 >> 32) < 40;


        private ulong getRatio(uint seed)
        {
            return (((ulong)seed) >> 16) / 656;
            //return ((((ulong)seed) * 0xFFFF) >> 32) / 0x290; // -> Alternative
        }

        //Cave item calculation has minor mistakes, maybe check later
        private int FindItem(bool cave)
        {
            if (cave)
            {
                uint calc = ((ulong)CurrentRand() * 1000 >> 32) < 100 ? 1000u : 1700u;
                uint result = (uint)((ulong)NextRand() * calc >> 32) / 100;

                if (calc == 1000)
                    return (int)result + 13;
                else
                    return (int)result + 23;
            }
            // Credits: https://www.smogon.com/forums/threads/past-gen-rng-research.61090/page-29#post-3536005
            else
            {
                uint calc = (uint)((ulong)CurrentRand() * 1000 >> 32);
                if (calc > 900)
                    return 0x23B;   // Pretty Wing
                else
                    return (int)(((ulong)NextRand() * 6 >> 32) + 0x235);
            }
            
        }
        #endregion


        public List<Frame> GenerateWonderCard(FrameCompare frameCompare, Wondercard wondercard)
        {
            int baseAdvances = 8;
            if (wondercard.eventNature == -1)
                baseAdvances += 2;
            if (wondercard.eventGender != -1)
                baseAdvances += 2;
            for (int i = 0; i < 6; i++)
                if (wondercard.eventIVInfo[i] == -1)
                    baseAdvances += 2;

            frames = new List<Frame>();
            rngList = new List<uint>();

            rng64.Seed = InitialSeed;

            // Fix later
            if (InitialFrame <= 1)
                InitialFrame = 2;

            for (uint cnt = 0; cnt < InitialFrame - 1; cnt++)
                rng64.GetNext64BitNumber();

            for (int cnt = 0; cnt < 34; cnt++)
                rngList.Add(rng64.GetNext32BitNumber());

            for (uint cnt = InitialFrame; cnt < InitialFrame + maxResults; cnt++, rngList.RemoveAt(0), rngList.Add(rng64.GetNext32BitNumber()))
            {
                pointer = baseAdvances;

                // From now on, -1 means that a value is not locked / forced so it needs to be calculated

                Frame frame = new Frame(FrameType.Wondercard5thGen)
                {
                    Number = cnt,
                    RngResult = rngList[0],
                    Hp = wondercard.eventIVInfo[0] == -1 ? NextRand() >> 27 : Convert.ToUInt32(wondercard.eventIVInfo[0]),
                    Atk = wondercard.eventIVInfo[1] == -1 ? NextRand() >> 27 : Convert.ToUInt32(wondercard.eventIVInfo[1]),
                    Def = wondercard.eventIVInfo[2] == -1 ? NextRand() >> 27 : Convert.ToUInt32(wondercard.eventIVInfo[2]),
                    Spa = wondercard.eventIVInfo[3] == -1 ? NextRand() >> 27 : Convert.ToUInt32(wondercard.eventIVInfo[3]),
                    Spd = wondercard.eventIVInfo[4] == -1 ? NextRand() >> 27 : Convert.ToUInt32(wondercard.eventIVInfo[4]),
                    Spe = wondercard.eventIVInfo[5] == -1 ? NextRand() >> 27 : Convert.ToUInt32(wondercard.eventIVInfo[5]),
                };

                Advance(2);

                uint pid;

                if (wondercard.eventGender == -1)
                {
                    pid = NextRand() ^ 0x10000;
                }
                else
                {
                    pid = Functions.CuteCharmModPID(NextRand(), NextRand(), wondercard.genderCase);
                }

                switch (wondercard.eventShininess)
                {
                    case 0:
                        pid = ForceNonShiny(pid, wondercard.eventTid, wondercard.eventSid);
                        break;
                    case 2:
                        pid = ForceShiny(pid, wondercard.eventTid, wondercard.eventSid);
                        break;
                }

                Advance(1);

                frame.Nature = (uint)(wondercard.eventNature == -1 ? ((ulong)NextRand() * 25 >> 32) : (uint)wondercard.eventNature);

                if (wondercard.eventAbility == -1)
                {
                    pid ^= 0x10000;
                    frame.Ability = (pid >> 16) & 1; 
                }
                else
                {
                    pid = (uint)(wondercard.eventAbility == 1 ? pid | 0x10000 : pid & ~0x10000);
                    frame.Ability = (uint)wondercard.eventAbility;
                }

                frame.eventPID = frame.pid = pid;
                frame.Shiny = CheckShiny(wondercard.eventTid, wondercard.eventSid, pid);

                if (frameCompare.Compare(frame))
                    frames.Add(frame);
            }

            return frames;
        }

        private static uint ForceShiny(uint pid, uint tid, uint sid)
        {
            uint lowByte = pid & 0x000000ff;
            return ((lowByte ^ tid ^ sid) << 16) | lowByte;
        }

        private static uint ForceNonShiny(uint pid, uint tid, uint sid)
        {
            if (((pid >> 16) ^ (pid & 0xffff) ^ sid ^ tid) < 8)
                pid = pid ^ 0x10000000;

            return pid;
        }


    }

}
