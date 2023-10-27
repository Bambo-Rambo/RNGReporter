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


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using RNGReporter.Objects;
using RNGReporter.Properties;

namespace RNGReporter
{
    public partial class DexIVCheck : Form
    {
        private BindingSource characteristicList;
        private BindingSource natureList;
        private BindingSource pokemonList;
        public DexIVCheck()
        {
            InitializeComponent();
        }

        private void DexIVCheck_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            this.Parent = null;
            e.Cancel = true;
        }

        private void DexIVCheck_Load(object sender, EventArgs e)
        {
            comboBoxPokemon.DisplayMember = "Key";
            comboBoxPokemon.ValueMember = "Value";

            pokemonList = new BindingSource(Pokemon.PokemonCollection(), null);
            comboBoxPokemon.DataSource = pokemonList;
            comboBoxPokemon.SelectedIndex = 0;

            comboBoxNature.DisplayMember = "Key";
            comboBoxNature.ValueMember = "Value";

            natureList = new BindingSource(Nature.NatureCollection(), null);
            comboBoxNature.DataSource = natureList;
            comboBoxNature.SelectedIndex = 0;

            characteristicList = new BindingSource(Characteristic.CharacteristicCollection(), null);
            comboBoxCharacteristic.DataSource = characteristicList;
            comboBoxCharacteristic.SelectedIndex = 0;

            Settings.Default.PropertyChanged += ChangeLanguage;
            SetLanguage();
        }

        public void ChangeLanguage(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Language")
            {
                SetLanguage();
            }
        }

        public void SetLanguage()
        {
            Font font;
            switch ((Language) Settings.Default.Language)
            {
                case (Language.Japanese):
                    font = new Font("Meiryo", 7.25F);
                    if (font.Name != "Meiryo")
                    {
                        font = new Font("Arial Unicode MS", 8.25F);
                        if (font.Name != "Arial Unicode MS")
                        {
                            font = new Font("MS Mincho", 8.25F);
                        }
                    }
                    break;
                case (Language.Korean):
                    font = new Font("Malgun Gothic", 8.25F);
                    if (font.Name != "Malgun Gothic")
                    {
                        font = new Font("Gulim", 9.25F);
                        if (font.Name != "Gulim")
                        {
                            font = new Font("Arial Unicode MS", 8.25F);
                        }
                    }
                    break;
                default:
                    font = DefaultFont;
                    break;
            }

            comboBoxNature.Font = font;
            comboBoxCharacteristic.Font = font;
            comboBoxPokemon.Font = font;
            labelAbility0.Font = font;
            labelAbility1.Font = font;

            natureList.ResetBindings(false);
            pokemonList.ResetBindings(false);
            characteristicList.ResetBindings(false);

            var pokemon = (Pokemon) comboBoxPokemon.SelectedValue;
            labelAbility0.Text = pokemon.Ability0;
            labelAbility1.Text = pokemon.Ability1;
        }

        private void comboBoxPokemon_SelectedIndexChanged(object sender, EventArgs e)
        {
            var pokemon = (Pokemon) comboBoxPokemon.SelectedValue;

            labelBaseHP.Text = pokemon.BaseHp.ToString();
            labelBaseAtk.Text = pokemon.BaseAtk.ToString();
            labelBaseDef.Text = pokemon.BaseDef.ToString();
            labelBaseSpA.Text = pokemon.BaseSpA.ToString();
            labelBaseSpD.Text = pokemon.BaseSpD.ToString();
            labelBaseSpe.Text = pokemon.BaseSpe.ToString();

            labelAbility0.Text = pokemon.Ability0;
            labelAbility1.Text = pokemon.Ability1;
        }

        private void buttonCheck_Click(object sender, EventArgs e)
        {
            uint hp = 0;
            uint atk = 0;
            uint def = 0;
            uint spa = 0;
            uint spd = 0;
            uint spe = 0;

            uint level = 1;

            var pokemon = (Pokemon) comboBoxPokemon.SelectedValue;
            var nature = (Nature) comboBoxNature.SelectedValue;

            if (maskedTextBoxHP.Text != "")
                hp = uint.Parse(maskedTextBoxHP.Text);

            if (maskedTextBoxAtk.Text != "")
                atk = uint.Parse(maskedTextBoxAtk.Text);

            if (maskedTextBoxDef.Text != "")
                def = uint.Parse(maskedTextBoxDef.Text);

            if (maskedTextBoxSpA.Text != "")
                spa = uint.Parse(maskedTextBoxSpA.Text);

            if (maskedTextBoxSpD.Text != "")
                spd = uint.Parse(maskedTextBoxSpD.Text);

            if (maskedTextBoxSpe.Text != "")
                spe = uint.Parse(maskedTextBoxSpe.Text);

            if (maskedTextBoxLevel.Text != "")
                level = uint.Parse(maskedTextBoxLevel.Text);

            var stats = new[] {hp, atk, def, spa, spd, spe};

            Characteristic characteristic = null;

            if (comboBoxCharacteristic.SelectedItem.ToString() != "NONE")
            {
                characteristic = (Characteristic) comboBoxCharacteristic.SelectedItem;
            }

            var ivCheck = new IVCheck(pokemon, level, nature, characteristic, stats);

            //  Get the results back and display them to the user
            textBoxResults.Text = ivCheck.ToString();
            setIVs(ivCheck.getIVRange);
        }


        // C-Gear part
        public void SetValues(string seed, uint delay, int IV)
        {
            TargetSeed.Text = seed;
            TargetDelay.Value = delay;
            IVFrame.Value = IV;
        }

        public void setIVs(uint[,] IVRange)
        {
            minHP.Value = IVRange[0, 0]; maxHP.Value = IVRange[1, 0];
            minAtk.Value = IVRange[0, 1]; maxAtk.Value = IVRange[1, 1];
            minDef.Value = IVRange[0, 2]; maxDef.Value = IVRange[1, 2];
            minSpA.Value = IVRange[0, 3]; maxSpA.Value = IVRange[1, 3];
            minSpD.Value = IVRange[0, 4]; maxSpD.Value = IVRange[1, 4];
            minSpe.Value = IVRange[0, 5]; maxSpe.Value = IVRange[1, 5];
        }
        private void btnCgearIVs_Click(object sender, EventArgs e)
        {
            DGV.Rows.Clear();
            IRNG mt = new MersenneTwister(0);
            uint Seed = uint.Parse(TargetSeed.Text, NumberStyles.HexNumber);
            uint IVAdvances = (uint)IVFrame.Value;
            uint DelayRange = 600;
            uint MinSeed = Seed - DelayRange;
            uint MaxSeed = Seed + DelayRange;
            List<uint> rngList = new List<uint>();

            for (uint i = MinSeed; i < MaxSeed; i++)
            {
                for (int j = -10; j <= 10; j++)
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