using RNGReporter.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace RNGReporter
{
    public partial class CgearCalibrator : Form
    {
        public CgearCalibrator()
        {
            InitializeComponent();
        }
        private void CgearCalibrator_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            this.Parent = null;
            e.Cancel = true;
        }

        public void SetValues(string seed, uint delay, int IV)
        {
            TargetSeed.Text = seed;
            TargetDelay.Value = delay;
            IVFrame.Value = IV;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            DGV.Rows.Clear();
            IRNG mt = new MersenneTwister(0); ;
            uint Seed = uint.Parse(TargetSeed.Text, NumberStyles.HexNumber);
            uint IVAdvances = (uint)IVFrame.Value;

            uint DelayRange = 1000;

            uint MinSeed = Seed - DelayRange;
            uint MaxSeed = Seed + DelayRange;
            List<uint> rngList = new List<uint>();

            for (uint i = MinSeed; i < MaxSeed; i++)
            {
                for (int j = -8; j < 8; j++)
                {
                    uint currentSeed = (uint)(i + j * 0x1000000);

                    mt.Reseed(currentSeed);

                    for (uint cnt = 0; cnt < IVAdvances; cnt++)
                        mt.Nextuint();

                    rngList.Clear();
                    for (int iv = 0; iv < 6; iv++)
                        rngList.Add(mt.Nextuint() >> 27);

                    if (rngList[0] >= minHP.Value && rngList[0] <= maxHP.Value)
                        if (rngList[1] >= minAtk.Value && rngList[1] <= maxAtk.Value)
                            if (rngList[2] >= minDef.Value && rngList[2] <= maxDef.Value)
                                if (rngList[3] >= minSpA.Value && rngList[3] <= maxSpA.Value)
                                    if (rngList[4] >= minSpD.Value && rngList[4] <= maxSpD.Value)
                                        if (rngList[5] >= minSpe.Value && rngList[5] <= maxSpe.Value)
                                        {
                                            int difference = (ushort)currentSeed - (ushort)Seed;
                                            DGV.Rows.Add(currentSeed.ToString("X"), difference, 
                                                TargetDelay.Value - difference,
                                                rngList[0], 
                                                rngList[1], 
                                                rngList[2], 
                                                rngList[3], 
                                                rngList[4], 
                                                rngList[5]);
                                        }
                }
            }
        }
    }
}