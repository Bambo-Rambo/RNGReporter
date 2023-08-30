/*
 * This file is part of RNG Reporter
 * Copyright (C) 2012 by Bill Young, Mike Suleski, and Andrew Ringer
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System.Windows.Forms;
using System.Collections.Generic;

namespace RNGReporter.Objects
{
    internal class EncounterSlotCalc
    {
        #region Gen 3/4
        public static int encounterSlot(uint result, FrameType frameType, EncounterType encounterType)
        {
            switch (frameType)
            {
                case FrameType.MethodH4:
                case FrameType.MethodH2:
                case FrameType.MethodH1:
                    return HSlot(result, encounterType);
                case FrameType.MethodK:
                    return KSlot(result, encounterType);
                case FrameType.MethodJ:
                    return JSlot(result, encounterType);
            }
            return -1;
        }

        private static int HSlot(uint result, EncounterType encounterType)
        {
            uint percent = (result >> 16)%100;
            switch (encounterType)
            {
                case EncounterType.WildOldRod:
                    {
                        Range[] ranges = {new Range(0, 69), new Range(70, 99)};
                        return CalcSlot(percent, ranges);
                    }
                case EncounterType.WildGoodRod:
                    {
                        Range[] ranges = {new Range(0, 59), new Range(60, 79), new Range(80, 99)};
                        return CalcSlot(percent, ranges);
                    }
                case EncounterType.WildSuperRod:
                    {
                        Range[] ranges =
                            {
                                new Range(0, 39), new Range(40, 79), new Range(80, 94), new Range(95, 98),
                                new Range(99, 99)
                            };
                        return CalcSlot(percent, ranges);
                    }
                case EncounterType.WildSurfing:
                    {
                        Range[] ranges =
                            {
                                new Range(0, 59), new Range(60, 89), new Range(90, 94), new Range(95, 98),
                                new Range(99, 99)
                            };
                        return CalcSlot(percent, ranges);
                    }
                default:
                    {
                        Range[] ranges =
                            {
                                new Range(0, 19), new Range(20, 39), new Range(40, 49), new Range(50, 59),
                                new Range(60, 69), new Range(70, 79), new Range(80, 84), new Range(85, 89),
                                new Range(90, 93), new Range(94, 97), new Range(98, 98), new Range(99, 99)
                            };
                        return CalcSlot(percent, ranges);
                    }
            }
        }

        private static int KSlot(uint result, EncounterType encounterType)
        {
            uint percent = (result >> 16)%100;
            switch (encounterType)
            {
                case EncounterType.WildSurfing:
                    {
                        Range[] ranges =
                            {
                                new Range(0, 59), new Range(60, 89), new Range(90, 94), new Range(95, 98),
                                new Range(99, 99)
                            };
                        return CalcSlot(percent, ranges);
                    }
                case EncounterType.WildSuperRod:
                case EncounterType.WildGoodRod:
                case EncounterType.WildOldRod:
                    {
                        Range[] ranges =
                            {
                                new Range(0, 39), new Range(40, 69), new Range(70, 84), new Range(85, 94),
                                new Range(95, 99)
                            };
                        return CalcSlot(percent, ranges);
                    }
                case EncounterType.BugCatchingContest:
                    {
                        Range[] ranges =
                            {
                                new Range(80, 99), new Range(60, 79), new Range(50, 59), new Range(40, 49),
                                new Range(30, 39), new Range(20, 29), new Range(15, 19), new Range(10, 14),
                                new Range(5, 9), new Range(0, 4)
                            };
                        return CalcSlot(percent, ranges);
                    }
                case EncounterType.SafariZone:
                    return (int) ((result >> 16)%10);
                case EncounterType.Headbutt:
                    {
                        Range[] ranges =
                            {
                                new Range(0, 49), new Range(50, 64), new Range(65, 79), new Range(80, 89),
                                new Range(90, 94), new Range(95, 99)
                            };
                        return CalcSlot(percent, ranges);
                    }
                default:
                    {
                        Range[] ranges =
                            {
                                new Range(0, 19), new Range(20, 39), new Range(40, 49), new Range(50, 59),
                                new Range(60, 69), new Range(70, 79), new Range(80, 84), new Range(85, 89),
                                new Range(90, 93), new Range(94, 97), new Range(98, 98), new Range(99, 99)
                            };
                        return CalcSlot(percent, ranges);
                    }
            }
        }

        private static int JSlot(uint result, EncounterType encounterType)
        {
            uint percent = (result >> 16)/656;
            // Diamond\Pearl\Platinum Slots
            switch (encounterType)
            {
                case EncounterType.WildOldRod:
                case EncounterType.WildSurfing:
                    {
                        Range[] ranges =
                            {
                                new Range(0, 59), new Range(60, 89), new Range(90, 94), new Range(95, 98),
                                new Range(99, 99)
                            };
                        return CalcSlot(percent, ranges);
                    }
                case EncounterType.WildSuperRod:
                case EncounterType.WildGoodRod:
                    {
                        Range[] ranges =
                            {
                                new Range(0, 39), new Range(40, 79), new Range(80, 94), new Range(95, 98),
                                new Range(99, 99)
                            };
                        return CalcSlot(percent, ranges);
                    }
                default:
                    {
                        Range[] ranges =
                            {
                                new Range(0, 19), new Range(20, 39), new Range(40, 49), new Range(50, 59),
                                new Range(60, 69), new Range(70, 79), new Range(80, 84), new Range(85, 89),
                                new Range(90, 93), new Range(94, 97), new Range(98, 98), new Range(99, 99)
                            };
                        return CalcSlot(percent, ranges);
                    }
            }
        }

        #endregion


        #region Gen 5

        // Credits: https://www.pokebip.com/page/jeuxvideo/dossier_shasse/auras_porte_bonheur

        public static Range[][] WildRanges = new Range[][]
        {
            new Range[]     // No Lucky Power used
            {
                new Range(0, 19),   new Range(20, 39),  new Range(40, 49),  new Range(50, 59),
                new Range(60, 69),  new Range(70, 79),  new Range(80, 84),  new Range(85, 89),
                new Range(90, 93),  new Range(94, 97),  new Range(98, 98),  new Range(99, 99)
            },
            
            new Range[]     // Lucky Power ↑
            {
                new Range(0, 9),    new Range(10, 19),  new Range(20, 29),  new Range(30, 39),
                new Range(40, 49),  new Range(50, 59),  new Range(60, 69),  new Range(70, 79),
                new Range(80, 84),  new Range(85, 89),  new Range(90, 94),  new Range(95, 99)
            },
            
            new Range[]     // Lucky Power ↑↑
            {
                new Range(0, 4),    new Range(4, 9),    new Range(10, 14),  new Range(15, 19),
                new Range(20, 29),  new Range(30, 39),  new Range(40, 49),  new Range(50, 59),
                new Range(60, 69),  new Range(70, 79),  new Range(80, 89),  new Range(90, 99)
            },

            new Range[]     // Lucky Power ↑↑↑
            {
                new Range(0, 0),    new Range(1, 1),    new Range(2, 5),    new Range(6, 9),
                new Range(10, 14),  new Range(15, 19),  new Range(20, 29),  new Range(30, 39),
                new Range(40, 49),  new Range(50, 59),  new Range(60, 79),  new Range(80, 99)
            },
        };


        public static Range[][] SurfingRanges = new Range[][]
        {
            new Range[] { new Range(0, 59), new Range(60, 89), new Range(90, 94), new Range(95, 98), new Range(99, 99) },   // No Lucky Power used
            new Range[] { new Range(0, 49), new Range(50, 79), new Range(80, 89), new Range(90, 94), new Range(95, 99) },   // Lucky Power ↑
            new Range[] { new Range(0, 39), new Range(40, 69), new Range(70, 79), new Range(80, 89), new Range(90, 99) },   // Lucky Power ↑↑
            new Range[] { new Range(0, 29), new Range(30, 49), new Range(50, 59), new Range(60, 79), new Range(80, 99) },   // Lucky Power ↑↑↑
        };


        public static Range[][] FishingRanges = new Range[][]
        {
            new Range[] { new Range(0, 39), new Range(40, 79), new Range(80, 94), new Range(95, 98), new Range(99, 99) },   // No Lucky Power used
            new Range[] { new Range(0, 39), new Range(40, 74), new Range(75, 89), new Range(90, 94), new Range(95, 99) },   // Lucky Power ↑
            new Range[] { new Range(0, 29), new Range(30, 59), new Range(60, 79), new Range(80, 89), new Range(90, 99) },   // Lucky Power ↑↑
            new Range[] { new Range(0, 19), new Range(20, 39), new Range(40, 59), new Range(60, 79), new Range(80, 99) },   // Lucky Power ↑↑↑
        };


        public static int encounterSlotG5(uint result, EncounterType encounterType, bool isBW2, int luckyPowerLVL)
        {
            uint percent = isBW2 ? (uint) (((ulong) result*100) >> 32) : (result >> 16)/656;
            switch (encounterType)
            {
                case EncounterType.WildSurfing:
                case EncounterType.WildWaterSpot:
                    {
                        return CalcSlot(percent, SurfingRanges[luckyPowerLVL]);
                    }
                case EncounterType.WildSuperRod:
                case EncounterType.WildFishingSpot:
                    {
                        return CalcSlot(percent, FishingRanges[luckyPowerLVL]);
                    }
                default:
                    {
                        return CalcSlot(percent, WildRanges[luckyPowerLVL]);
                    }
            }
        }

        #endregion

        /// <summary>
        ///     Calculates the encounter slot based off given values
        /// </summary>
        /// <param name="percent">The percent value of the encounter slot</param>
        /// <param name="ranges">A pair of uints to compare the percent to.</param>
        /// <returns></returns>
        private static int CalcSlot(uint percent, IList<Range> ranges)
        {
            for (int i = 0; i < ranges.Count; ++i)
            {
                if (percent >= ranges[i].Min && percent <= ranges[i].Max)
                    return i;
            }
            return -1;
        }
    }

    public class Range
    {
        public Range()
        {
        }

        public Range(uint min, uint max)
        {
            Min = min;
            Max = max;
        }

        public uint Min { get; set; }
        public uint Max { get; set; }
    };
}