﻿/*
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
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using RNGReporter.Controls;
using RNGReporter.Objects;
using RNGReporter.Objects.Generators;
using RNGReporter.Objects.Searchers;
using RNGReporter.Properties;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace RNGReporter
{
    public partial class TimeFinder5th : Form
    {
        private static readonly object threadLock = new object();
        private readonly TextBoxBase txtCallerID;
        private readonly TextBoxBase txtCallerSID;
        private readonly CheckBox cbCallerBW2;
        private readonly CheckBox cbCallerShinyCharm;
        private readonly CheckBox cbCallerMemoryLink;
        private int CapHPIndex;
        private int CapNatureIndex;
        private int CapSpeedIndex;
        private int cpus;
        private List<ulong> eggSeeds;
        private FrameCompare frameCompare;
        private FrameGenerator generator;
        private FrameGenerator[] generators;
        private List<IFrameCapture> iframes;
        private List<IFrameCapture> iframesEgg;
        private Thread[] jobs;
        private Dictionary<uint, uint>[] list;
        private BindingSource listBindingCap;
        private BindingSource listBindingEvent;
        private BindingSource listBindingEgg;
        private BindingSource listBindingPickup;
        private bool longSeed;
        private Point oldLocation;
        private BindingSource profilesSource;
        private ulong progressFound;
        private ulong progressSearched;
        private ulong progressTotal;
        private bool refreshQueue;
        private FrameGenerator shinygenerator;
        private FrameGenerator[] shinygenerators;
        private FrameCompare subFrameCompare;
        private DexIVCheck checker;
        private EventWaitHandle waitHandle;

        public TimeFinder5th()
        {
            TabPage = 0;

            InitializeComponent();
        }

        public TimeFinder5th(TextBoxBase id, TextBoxBase sid, CheckBox BW2, CheckBox shinyCharm, CheckBox memoryLink)
        {
            TabPage = 0;

            txtCallerID = id;
            txtCallerSID = sid;

            cbCallerBW2 = BW2;
            cbCallerShinyCharm = shinyCharm;
            cbCallerMemoryLink = memoryLink;

            InitializeComponent();
        }

        public int TabPage { get; set; }

        private void PlatinumTime_Load(object sender, EventArgs e)
        {
            tabControl.TabPages.Remove(tabPageDreamRadar);
            // Add smart comboBox items
            // Would be nice if we left these in the Designer file
            // But Visual Studio seems to like deleting them without warning

            comboBoxMethod.Items.AddRange(new object[]
                {
                    new ComboBoxItem("PID + IVs (Standard)", FrameType.Method5Standard),
                    new ComboBoxItem("PID + IVs (Standard + C-Gear)", FrameType.Method5Natures),
                    new ComboBoxItem("IVs (C-Gear)", FrameType.Method5CGear),
                });

            var ability = new[]
                {
                    new ComboBoxItem("Any", -1),
                    new ComboBoxItem("Ability 0", 0),
                    new ComboBoxItem("Ability 1", 1)
                };

            comboBoxEncounterType.Items.AddRange(new object[]
                {
                    new ComboBoxItem("Tall Grass", EncounterType.Wild),
                    new ComboBoxItem("Dark Grass", EncounterType.WildDarkGrass),
                    new ComboBoxItem("Swarm", EncounterType.WildSwarm),
                    new ComboBoxItem("Surfing", EncounterType.WildSurfing),
                    new ComboBoxItem("Fishing", EncounterType.WildSuperRod),
                    new ComboBoxItem("Shaking Grass", EncounterType.WildShakerGrass),
                    new ComboBoxItem("Bubble Spot", EncounterType.WildWaterSpot),
                    new ComboBoxItem("Fishing Spot", EncounterType.WildFishingSpot),
                    new ComboBoxItem("Cave Spot", EncounterType.WildCaveSpot),
                    new ComboBoxItem("Flying Shadow", EncounterType.WildShadow),
                    new ComboBoxItem("Stationary", EncounterType.Stationary),
                    new ComboBoxItem("Roamer", EncounterType.Roamer),
                    new ComboBoxItem("Gift Pokémon", EncounterType.Gift),
                    new ComboBoxItem("Larvesta/Happiny Egg", EncounterType.LarvestaHappiny),
                    new ComboBoxItem("Jellicent", EncounterType.JellicentHA),
                });

            var shinyNatureList = new BindingSource {DataSource = Objects.Nature.NatureDropDownCollection()};
            comboBoxShinyNature.DataSource = shinyNatureList;

            var shinyEverstoneList = new BindingSource
                {DataSource = Objects.Nature.NatureDropDownCollectionSynch()};
            comboBoxShinyEverstoneNature.DataSource = shinyEverstoneList;
            comboBoxCapNature.Items.AddRange(Objects.Nature.NatureDropDownCollectionSearchNatures());
            glassComboEventNatures.Items.AddRange(Objects.Nature.NatureDropDownCollectionSearchNatures());

            profilesSource = new BindingSource {DataSource = Profiles.List};
            comboBoxProfiles.DataSource = profilesSource;

            Settings.Default.PropertyChanged += ChangeLanguage;
            SetLanguage();


            comboCapAbility.DataSource = ability;
            comboBoxShinyAbility.DataSource = ability;

            comboBoxShinyGender.DataSource = GenderFilter.GenderFilterCollection();

            // Obtain the locations and sizes of the Max Frame textbox,
            // as we cannot be sure of them at compile time.
            oldLocation = labelCapMinMaxFrame.Location;
            longSeed = false;

            // Obtain the indices of the datagrid columns by name,
            // so we don't have to keep track of them

            CapHPIndex = CapHP.Index;
            CapSpeedIndex = CapSpe.Index;
            CapNatureIndex = Nature.Index;

            comboBoxMethod.SelectedIndex = 0;
            comboBoxCapNature.SelectedIndex = 0;
            comboCapAbility.SelectedIndex = 0;
            comboBoxCapGender.SelectedIndex = 0;
            comboBoxCapGenderRatio.SelectedIndex = 0;
            comboBoxEncounterType.SelectedIndex = 0;

            SlotLevel1.SelectedIndex = SlotLevel2.SelectedIndex = SlotLevel3.SelectedIndex =
                SlotLevel4.SelectedIndex = SlotLevel5.SelectedIndex = SlotLevel6.SelectedIndex = 0;

            // This is a rather hackish way of making the custom control
            // display the desired text upon loading

            comboCapEncounterSlot.CheckBoxItems[0].Checked = true;
            comboCapEncounterSlot.CheckBoxItems[0].Checked = false;

            comboBoxCapNature.CheckBoxItems[0].Checked = true;
            comboBoxCapNature.CheckBoxItems[0].Checked = false;

            glassComboEventNatures.CheckBoxItems[0].Checked = true;
            glassComboEventNatures.CheckBoxItems[0].Checked = false;

            comboBoxShinyNature.SelectedIndex = 0;
            comboBoxShinyAbility.SelectedIndex = 0;
            comboBoxShinyGender.SelectedIndex = 0;
            comboBoxShinyEverstoneNature.SelectedIndex = 0;

            dataGridViewCapValues.AutoGenerateColumns = false;
            dataGridViewShinyResults.AutoGenerateColumns = false;
            dataGridViewEventResults.AutoGenerateColumns = false;
            dataGridViewPickup.AutoGenerateColumns = false;

            cbHHMonth.CheckBoxItems[0].Checked = true;
            cbHHMonth.CheckBoxItems[0].Checked = false;
            tabControl.SelectTab(TabPage);

            //  Load all of our items from the registry
            RegistryKey registrySoftware = Registry.CurrentUser.OpenSubKey("Software", true);
            if (registrySoftware != null)
            {
                RegistryKey registryRngReporter = registrySoftware.OpenSubKey("RNGReporter");

                if (Settings.Default.LastVersion < MainForm.VersionNumber && registryRngReporter != null)
                {
                    maskedTextBoxCapYear.Text = maskedTextBoxPickupYear.Text =
                        (string) registryRngReporter.GetValue("pt_cap_year", DateTime.Now.Year.ToString());
                    comboBoxCapMonth.CheckBoxItems[DateTime.Now.Month].Checked = comboBoxPickupMonth.CheckBoxItems[DateTime.Now.Month].Checked = true;
                    maskedTextBoxCapMaxOffset.Text = (string) registryRngReporter.GetValue("pt_cap_offset", "1000");

                    if (maskedTextBoxCapMaxOffset.Text == "0")
                        maskedTextBoxCapMaxOffset.Text = "1";

                    maskedTextBoxCapMinDelay.Text = (string) registryRngReporter.GetValue("pt_cap_delaymin", "600");
                    maskedTextBoxCapMaxDelay.Text = (string) registryRngReporter.GetValue("pt_cap_delaymax", "610");

                    maskedTextBoxShinyYear.Text =
                        (string) registryRngReporter.GetValue("pt_shiny_year", DateTime.Now.Year.ToString());
                    comboBoxShinyMonth.CheckBoxItems[DateTime.Now.Month].Checked = true;
                }
                    //load from settings
                else
                {
                    if (Settings.Default.CapYear < 2000) Settings.Default.CapYear = DateTime.Now.Year;
                    maskedTextBoxCapYear.Text = maskedTextBoxPickupYear.Text = Settings.Default.CapYear.ToString();
                    comboBoxCapMonth.CheckBoxItems[DateTime.Now.Month].Checked = comboBoxPickupMonth.CheckBoxItems[DateTime.Now.Month].Checked = true;
                    maskedTextBoxCapMaxOffset.Text = Settings.Default.CapOffset;

                    maskedTextBoxCapMinDelay.Text = Settings.Default.CapDelayMin;
                    maskedTextBoxCapMaxDelay.Text = Settings.Default.CapDelayMax;

                    if (Settings.Default.ShinyYear < 2000) Settings.Default.ShinyYear = DateTime.Now.Year;
                    maskedTextBoxShinyYear.Text = Settings.Default.ShinyYear.ToString();
                    comboBoxShinyMonth.CheckBoxItems[DateTime.Now.Month].Checked = true;
                }


                cpus = Settings.Default.CPUs;
                if (cpus < 1)
                {
                    cpus = 1;
                }
            }

            maskedTextYear.Text = DateTime.Now.Year.ToString();
            checkBoxComboMonths.CheckBoxItems[DateTime.Now.Month].Checked = true;

            // Hidden Grotto
            cbHHMonth.CheckBoxItems[DateTime.Now.Month].Checked = true;
            txtHHYear.Text = DateTime.Now.Year.ToString();
            // default value, only searching the grotto at index 0
            cbHHHollowNumber.CheckBoxItems[0].Checked = true;

            cbDRNature.Items.AddRange(Objects.Nature.NatureDropDownCollectionSearchNatures());
            cbDRNature.SelectedIndex = 0;
            cbDRNature.CheckBoxItems[0].Checked = true;
            cbDRNature.CheckBoxItems[0].Checked = false;

            cbDRMonth.CheckBoxItems[DateTime.Now.Month].Checked = true;
            txtDRYear.Text = DateTime.Now.Year.ToString();

            RefreshParameters();

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
            var CellStyle = new DataGridViewCellStyle();
            switch ((Language) Settings.Default.Language)
            {
                case (Language.Japanese):
                    CellStyle.Font = new Font("Meiryo", 7.25F);
                    if (CellStyle.Font.Name != "Meiryo")
                    {
                        CellStyle.Font = new Font("Arial Unicode MS", 8.25F);
                        if (CellStyle.Font.Name != "Arial Unicode MS")
                        {
                            CellStyle.Font = new Font("MS Mincho", 8.25F);
                        }
                    }
                    break;
                case (Language.Korean):
                    CellStyle.Font = new Font("Malgun Gothic", 8.25F);
                    if (CellStyle.Font.Name != "Malgun Gothic")
                    {
                        CellStyle.Font = new Font("Gulim", 9.25F);
                        if (CellStyle.Font.Name != "Gulim")
                        {
                            CellStyle.Font = new Font("Arial Unicode MS", 8.25F);
                        }
                    }
                    break;
                default:
                    CellStyle.Font = DefaultFont;
                    break;
            }

            Nature.DefaultCellStyle = CellStyle;
            HiddenPower.DefaultCellStyle = CellStyle;
            ShinyNature.DefaultCellStyle = CellStyle;
            EncounterSlot.DefaultCellStyle = CellStyle;
            EncounterMod.DefaultCellStyle = CellStyle;

            comboBoxCapNature.Font = CellStyle.Font;
            comboBoxShinyNature.Font = CellStyle.Font;
            comboBoxShinyEverstoneNature.Font = CellStyle.Font;

            comboCapEncounterSlot.CheckBoxItems[13].Font = CellStyle.Font;
            comboCapEncounterSlot.CheckBoxItems[13].Text = Functions.encounterItems(12);

            for (int checkBoxIndex = 1; checkBoxIndex < comboBoxCapNature.Items.Count; checkBoxIndex++)
            {
                comboBoxCapNature.CheckBoxItems[checkBoxIndex].Text =
                    (comboBoxCapNature.CheckBoxItems[checkBoxIndex].ComboBoxItem).ToString();
                comboBoxCapNature.CheckBoxItems[checkBoxIndex].Font = CellStyle.Font;
            }

            comboBoxCapNature.CheckBoxItems[0].Checked = true;
            comboBoxCapNature.CheckBoxItems[0].Checked = false;

            ((BindingSource) comboBoxShinyNature.DataSource).ResetBindings(false);
            ((BindingSource) comboBoxShinyEverstoneNature.DataSource).ResetBindings(false);

            dataGridViewCapValues.Refresh();
            dataGridViewShinyResults.Refresh();
        }

        public void RefreshParameters()
        {
            // So we don't have to create a new instance of Time Finder
            // Each time it's called from DS Parameters
            cpus = Settings.Default.CPUs;
            if (cpus < 0) cpus = 1;

            //initialize the profiles
            if (Profiles.List == null || Profiles.List.Count == 0)
            {
                MessageBox.Show("No profiles were detected. Please setup a profile first.");
                Profiles.ProfileManager.Visible = false;
                Profiles.ProfileManager.ShowDialog();
            }
            if (Profiles.List == null || Profiles.List.Count == 0)
            {
                Close();
            }
            profilesSource.DataSource = Profiles.List;
            profilesSource.ResetBindings(false);

            // put a check on the last profile to ensure it's not negative
            if (Settings.Default.LastProfile < 0) Settings.Default.LastProfile = 0;
            if (Settings.Default.LastProfile < Profiles.List.Count)
                comboBoxProfiles.SelectedIndex = Settings.Default.LastProfile;
        }

        private void PlatinumTime_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.CapYear = int.Parse(maskedTextBoxCapYear.Text);
            Settings.Default.CapOffset = maskedTextBoxCapMaxOffset.Text;
            Settings.Default.CapDelayMin = maskedTextBoxCapMinDelay.Text;
            Settings.Default.CapDelayMax = maskedTextBoxCapMaxDelay.Text;
            Settings.Default.ShinyYear = int.Parse(maskedTextBoxShinyYear.Text);
            Settings.Default.LastProfile = comboBoxProfiles.SelectedIndex;

            Settings.Default.Save();

            e.Cancel = true;

            buttonCapGenerate.Enabled = true;
            buttonShinyGenerate.Enabled = true;

            if (jobs != null)
            {
                for (int i = 0; i < jobs.Length; i++)
                {
                    if (jobs[i] != null)
                    {
                        jobs[i].Abort();
                    }
                }
            }
            Hide();
        }


        #region Capture

        private void contextMenuStripCap_Opening(object sender, CancelEventArgs e)
        {
            if (dataGridViewCapValues.SelectedRows.Count == 0)
            {
                e.Cancel = true;
            }
        }

        private void buttonCapGenerate_Click(object sender, EventArgs e)
        {
            var profile = (Profile)comboBoxProfiles.SelectedItem;
            iframes = new List<IFrameCapture>();
            listBindingCap = new BindingSource { DataSource = iframes };
            dataGridViewCapValues.DataSource = listBindingCap;

            if (comboBoxMethod.SelectedItem.ToString().Equals("PID + IVs (Standard + C-Gear)"))
            {
                if (profile.IsBW2())
                {
                    MessageBox.Show("This method is not possible in BW2.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (uint.Parse(maskedTextBoxCapMinOffset.Text) < 2)
                    maskedTextBoxCapMinOffset.Text = "2";
                if (uint.Parse(maskedTextBoxCapMaxOffset.Text) < 2)
                    maskedTextBoxCapMaxOffset.Text = "2";
                if (uint.Parse(maskedTextBoxCapMinDelay.Text) < 1000)
                    maskedTextBoxCapMinDelay.Text = "1000";
                if (uint.Parse(maskedTextBoxCapMaxDelay.Text) < 1000)
                    maskedTextBoxCapMaxDelay.Text = "2000";
            }

            jobs = new Thread[cpus];
            generators = new FrameGenerator[cpus];
            shinygenerators = new FrameGenerator[cpus];
            waitHandle = new EventWaitHandle(true, EventResetMode.ManualReset);

            var year = (uint)DateTime.Now.Year;
            if (maskedTextBoxCapYear.Text != "")
            {
                year = uint.Parse(maskedTextBoxCapYear.Text);
                if (year < 2000)
                {
                    MessageBox.Show("You must enter a year greater than 1999.", "Please Enter a Valid Year", MessageBoxButtons.OK);
                    return;
                }
            }

            uint maxOffset;
            if (maskedTextBoxCapMaxOffset.Text != "")
                maxOffset = uint.Parse(maskedTextBoxCapMaxOffset.Text);
            else
            {
                maskedTextBoxCapMaxOffset.Text = "1000";
                maxOffset = 1000;
            }

            uint minOffset;
            if (maskedTextBoxCapMinOffset.Text != "")
                minOffset = uint.Parse(maskedTextBoxCapMinOffset.Text);
            else
            {
                maskedTextBoxCapMinOffset.Text = "0";
                minOffset = 0;
            }


            if (minOffset > maxOffset)
            {
                maskedTextBoxCapMinOffset.Focus();
                maskedTextBoxCapMinOffset.SelectAll();
                return;
            }

            bool fastSearch = label9.Visible && FastCapFilters() && FastCapFrames();
            if (fastSearch)
            {
                minOffset++;
                maxOffset++;
            }
            bool ConsiderTrigger = checkBoxTriggerBattle.Checked && checkBoxTriggerBattle.Visible;

            // !!! This is generator for IV Frames !!!
            generator = new FrameGenerator
            {
                // Now that each combo box item is a custom object containing the FrameType reference
                // We can simply retrieve the FrameType from the selected item
                FrameType = (FrameType)((ComboBoxItem)comboBoxMethod.SelectedItem).Reference,
                EncounterType = (EncounterType)((ComboBoxItem)comboBoxEncounterType.SelectedItem).Reference,
                EncounterMod = Objects.EncounterMod.Search,
                InitialFrame = minOffset,
                MaxResults = maxOffset - minOffset + 1
            };
            if (generator.FrameType == FrameType.BWBred && profile.IsBW2())
                generator.FrameType = FrameType.BW2Bred;
            if (generator.FrameType == FrameType.BWBredInternational && profile.IsBW2())
                generator.FrameType = FrameType.BW2BredInternational;
            generator.isBW2 = profile.IsBW2();

            if (generator.EncounterType == EncounterType.Roamer && !fastSearch)
            {
                generator.InitialFrame++;
            }

            // set up the hashtables containing precomputed MTRNG values
            // this saves time by reducing the search to a hashtable lookup
            // of MTRNG seeds that corresponds to common spreads
            //list = new Hashtable[6];
            list = new Dictionary<uint, uint>[6];



            //  Build up a FrameComparer

            //  Map the information from the IV box.  Anything
            //  that is blank is considered a zero.

            List<int> encounterSlots = null;
            if (comboCapEncounterSlot.Visible && comboCapEncounterSlot.Text != "Any" && comboCapEncounterSlot.CheckBoxItems.Count > 0)
            {
                encounterSlots = new List<int>();
                for (int i = 0; i < comboCapEncounterSlot.CheckBoxItems.Count; i++)
                {
                    if (comboCapEncounterSlot.CheckBoxItems[i].Checked)
                        // We have to subtract 1 because this custom control contains a hidden item for text display
                        encounterSlots.Add(i - 1);
                }
            }

            List<uint> natures = null;
            if (comboBoxCapNature.Text != "Any" && comboBoxCapNature.CheckBoxItems.Count > 0)
                natures = (from t in comboBoxCapNature.CheckBoxItems where t.Checked select (uint)((Nature)t.ComboBoxItem).Number).ToList();

            uint shinyOffsetMin = 0;
            uint shinyOffsetMax = 0;
            if (ShinyOnly())
            {
                uint.TryParse(maskedTextBoxMinShiny.Text, out shinyOffsetMin);
                uint.TryParse(maskedTextBoxMaxShiny.Text, out shinyOffsetMax);
            }

            Lvl.Visible = LevelConditions();
            EncounterRatio.Visible = ConsiderTrigger && ShinyOnly();
            EncType.Visible = false;// comboBoxMethod.SelectedIndex == 0 && comboBoxEncounterType.SelectedIndex == 1;
            LuckyLevel.Visible = profile.LuckyPowerLVL > 0;

            CapDateTime.Visible = CapKeypress.Visible = CapTimer0.Visible = generator.FrameType != FrameType.Method5CGear;
            copyCgearToClipboard.Visible = calibrateDelayIVs.Visible = generator.FrameType == FrameType.Method5Natures;

            EncounterSlot.Visible = false;
            EncounterMod.Visible = false;
            PID.Visible = false;
            NearestShiny.Visible = false;
            Nature.Visible = false;
            Ability.Visible = false;
            CgearSeed.Visible = false;
            Delay.Visible = false;
            f25.Visible = false;
            f50.Visible = false;
            f75.Visible = false;
            f125.Visible = false;

            if (generator.FrameType == FrameType.Method5Standard || generator.FrameType == FrameType.Method5Natures)
            {
                CapSeed.DefaultCellStyle.Format = "X16";
                CapSeed.Width = seedColumnLong(true, CapSeed);

                CgearSeed.Visible = Delay.Visible = generator.FrameType == FrameType.Method5Natures;

                if (ShinyOnly() && shinyOffsetMax > 0)
                {
                    shinygenerator = new FrameGenerator
                    {
                        FrameType = FrameType.Method5Natures,
                        EncounterType =
                                (EncounterType)
                                ((ComboBoxItem)comboBoxEncounterType.SelectedItem).Reference,
                        EncounterMod = Objects.EncounterMod.Search,
                        InitialFrame = 1,
                        MinAdvances = shinyOffsetMin,
                        MaxResults = shinyOffsetMax,
                        MinLevel = (int)numericLevelMin.Value,
                        MaxLevel = (int)numericLevelMax.Value,
                        SearchForTrigger = ConsiderTrigger,
                        RerollCount = profile.ShinyCharm ? 3 : 1,
                        MemoryLinkUsed = profile.MemoryLink,
                        MaxLuckyPowerLVL = profile.LuckyPowerLVL,
                    };

                    subFrameCompare = new FrameCompare(
                        ivFiltersCapture.IVFilter,
                        natures,
                        (int)((ComboBoxItem)comboCapAbility.SelectedItem).Reference,
                        true,
                        checkBoxSynchOnly.Checked,
                        LevelConditions() ? (int)numericLevel.Value : 0,
                        false,
                        encounterSlots,
                        constructGenderFilter());

                    NearestShiny.Visible = true;
                    PID.Visible = true;

                    if (shinygenerator.EncounterType != EncounterType.Gift &&
                        shinygenerator.EncounterType != EncounterType.Roamer &&
                        shinygenerator.EncounterType != EncounterType.LarvestaHappiny)
                        EncounterMod.Visible = true;
                    else
                        EncounterMod.Visible = false;
                    if (shinygenerator.EncounterType != EncounterType.Stationary &&
                        shinygenerator.EncounterType != EncounterType.Gift &&
                        shinygenerator.EncounterType != EncounterType.Roamer &&
                        shinygenerator.EncounterType != EncounterType.LarvestaHappiny &&
                        shinygenerator.EncounterType != EncounterType.JellicentHA &&
                        shinygenerator.EncounterType != EncounterType.Haxorus &&
                            shinygenerator.EncounterType != EncounterType.GibleDratini)
                        EncounterSlot.Visible = true;
                    else
                        EncounterSlot.Visible = false;

                    Nature.Visible = true;
                    Ability.Visible = shinygenerator.EncounterType != EncounterType.JellicentHA;
                    DisplayGenderColumns();
                }
                if (profile.IsBW2())
                    generator.InitialFrame += 2;
            }
            else
            {
                CapSeed.DefaultCellStyle.Format = "X8";
                CapSeed.Width = seedColumnLong(false, CapSeed);
            }

            if (generator.FrameType == FrameType.Method5Standard || generator.FrameType == FrameType.Method5CGear)
            {
                frameCompare = new FrameCompare(
                    ivFiltersCapture.IVFilter, 
                    null, -1, false, false, 0, false, null, new NoGenderFilter());
            }

            CgearSettings settings = new CgearSettings();
            if (generator.FrameType == FrameType.Method5Natures)
            {
                frameCompare = new FrameCompare(
                    ivFiltersCapture.IVFilter,
                    natures,
                    (int)((ComboBoxItem)comboCapAbility.SelectedItem).Reference,
                    checkBoxShinyOnly.Checked,
                    checkBoxSynchOnly.Checked,
                    LevelConditions() ? (int)numericLevel.Value : 0,
                    false,
                    encounterSlots,
                    constructGenderFilter());

                settings = new CgearSettings()
                {
                    calibDelay = (uint)(numCalibrateDelay.Enabled ? numCalibrateDelay.Value : 0),
                    minIVFrame = uint.Parse(maskedTextBoxCapMinOffset.Text),
                    maxIVFrame = uint.Parse(maskedTextBoxCapMaxOffset.Text) - uint.Parse(maskedTextBoxCapMinOffset.Text) + 1,
                    fixedRTC = checkBoxRAD.Checked,
                };
                settings.delayMin = uint.Parse(maskedTextBoxCapMinDelay.Text) + settings.calibDelay;
                settings.delayMax = uint.Parse(maskedTextBoxCapMaxDelay.Text) + settings.calibDelay;
            }
            
            if (generator.FrameType != FrameType.Method5CGear)
            {
                if (fastSearch)
                {
                    Assembly thisExe = Assembly.GetExecutingAssembly();
                    Stream file;

                    if (generator.EncounterType == EncounterType.Roamer)
                    {
                        file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame1D-Roamer.txt");
                        list[0] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                        file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame2D-Roamer.txt");
                        list[1] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                        file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame3D-Roamer.txt");
                        list[2] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                        file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame4D-Roamer.txt");
                        list[3] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                        file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame5D-Roamer.txt");
                        list[4] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                        file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame6D-Roamer.txt");
                        list[5] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);

                        foreach (var partialList in list)
                        {
                            if (partialList == null)
                                MessageBox.Show("error in loading roamer tables");
                        }
                    }
                    else
                    {
                        if (profile.IsBW2())
                        {
                            // entralink
                            if (minOffset > 21)
                            {
                                file =
                                    thisExe.GetManifestResourceStream(
                                        "RNGReporter.Resources.MTRNG-Frame25-Entralink.txt");
                                list[0] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                                file =
                                    thisExe.GetManifestResourceStream(
                                        "RNGReporter.Resources.MTRNG-Frame26-Entralink.txt");
                                list[1] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                                file =
                                    thisExe.GetManifestResourceStream(
                                        "RNGReporter.Resources.MTRNG-Frame27-Entralink.txt");
                                list[2] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                                file =
                                    thisExe.GetManifestResourceStream(
                                        "RNGReporter.Resources.MTRNG-Frame28-Entralink.txt");
                                list[3] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                                file =
                                    thisExe.GetManifestResourceStream(
                                        "RNGReporter.Resources.MTRNG-Frame29-Entralink.txt");
                                list[4] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                                file =
                                    thisExe.GetManifestResourceStream(
                                        "RNGReporter.Resources.MTRNG-Frame30-Entralink.txt");
                                list[5] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                            }
                            else
                            {
                                file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame3D.txt");
                                list[0] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                                file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame4D.txt");
                                list[1] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                                file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame5D.txt");
                                list[2] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                                file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame6D.txt");
                                list[3] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                                file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame7D.txt");
                                list[4] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                                file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame8D.txt");
                                list[5] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                            }
                        }
                        else
                        {
                            file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame1D.txt");
                            list[0] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                            file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame2D.txt");
                            list[1] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                            file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame3D.txt");
                            list[2] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                            file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame4D.txt");
                            list[3] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                            file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame5D.txt");
                            list[4] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                            file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame6D.txt");
                            list[5] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                        }


                        foreach (var partialList in list)
                        {
                            if (partialList == null)
                                MessageBox.Show("error in loading hashtables");
                        }
                    }
                }

                generateTimesToolStripMenuItem.Visible = false;
                generateEntralinkNatureSeedsToolStripMenuItem.Visible = false;
                generateAdjacentSeedsToolStripMenuItem.Visible = true;

                var months = new List<int>();
                for (int month = 1; month <= 12; month++)
                {
                    if (comboBoxCapMonth.CheckBoxItems[month].Checked)
                        months.Add(month);
                }

                if (months.Count == 0)
                {
                    comboBoxCapMonth.Focus();
                    return;
                }

                List<List<ButtonComboType>> keypresses = profile.GetKeypresses();

                progressSearched = 0;
                progressFound = 0;

                int dayTotal = months.Sum(month => DateTime.DaysInMonth((int)year, month));
                progressTotal =
                    (ulong)
                    (dayTotal * 86400 * (maxOffset - minOffset + 1) * keypresses.Count *
                     (profile.Timer0Max - profile.Timer0Min + 1));

                for (int i = 0; i < jobs.Length; i++)
                {
                    generators[i] = generator.Clone();

                    if (shinygenerator != null)
                    {
                        shinygenerators[i] = shinygenerator.Clone();
                    }

                    //copy to prevent issues with it being incremented before the actual thread really starts
                    int i1 = i;
                    //passing in a profile instead of the params would probably be more efficent
                    jobs[i] = new Thread(() =>
                                GenerateJob(year, months, 0, 23, profile, shinyOffsetMin, shinyOffsetMax, fastSearch, i1, settings));
                    jobs[i].Start();
                    // for some reason not making the thread sleep causes issues with updating dayMin\Max
                    Thread.Sleep(200);
                }
                var progressJob = new Thread(() => ManageProgress(listBindingCap, dataGridViewCapValues, generator.FrameType, 2000));
                progressJob.Start();
                progressJob.Priority = ThreadPriority.Lowest;

                buttonCapGenerate.Enabled = false;
                buttonShinyGenerate.Enabled = false;
            }
            else
            {
                //  We want to get our year and offset ranges here so
                //  that we can have some values for our looping.
                //  Default these to this value, but save to
                //  the registry so we can not have to redo.
                uint maxDelay = 610;
                if (maskedTextBoxCapMaxDelay.Text != "")
                    maxDelay = uint.Parse(maskedTextBoxCapMaxDelay.Text);

                uint minDelay = 600;
                if (maskedTextBoxCapMinDelay.Text != "")
                    minDelay = uint.Parse(maskedTextBoxCapMinDelay.Text);

                uint minEfgh = (year - 2000) + minDelay;
                uint maxEfgh = (year - 2000) + maxDelay;

                if (fastSearch)
                {
                    Assembly thisExe = Assembly.GetExecutingAssembly();

                    Stream file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame21-Entralink.txt");
                    list[0] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                    file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame22-Entralink.txt");
                    list[1] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                    file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame23-Entralink.txt");
                    list[2] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                    file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame24-Entralink.txt");
                    list[3] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                    file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame25-Entralink.txt");
                    list[4] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);
                    file = thisExe.GetManifestResourceStream("RNGReporter.Resources.MTRNG-Frame26-Entralink.txt");
                    list[5] = (Dictionary<uint, uint>)new BinaryFormatter().Deserialize(file);

                    foreach (var partialList in list)
                    {
                        if (partialList == null)
                            MessageBox.Show("error in loading hashtables");
                    }
                }

                generateTimesToolStripMenuItem.Visible = true;
                generateEntralinkNatureSeedsToolStripMenuItem.Visible = true;
                generateAdjacentSeedsToolStripMenuItem.Visible = false;

                jobs = new Thread[1];

                //todo: split this into multiple threads
                //jobs[0] = new Thread(() => GenerateCGearCapJob(profile, minEfgh, maxEfgh, fastSearch));
                jobs[0] =
                    new Thread(
                        () =>
                        GenerateCGearCapJob(profile.MAC_Address, minEfgh, maxEfgh, fastSearch, profile.ID, profile.SID));
                jobs[0].Start();

                progressTotal = (255 * 24 * (maxEfgh - minEfgh + 1) * generator.MaxResults);
                var progressJob =
                    new Thread(() => ManageProgress(listBindingCap, dataGridViewCapValues, generator.FrameType, 0));
                progressJob.Start();
                progressJob.Priority = ThreadPriority.Lowest;

                buttonCapGenerate.Enabled = false;
            }

            dataGridViewCapValues.Focus();
        }

        public void GenerateJob(uint year, List<int> months, int hourMin, int hourMax, Profile profile, 
            uint shinyOffsetMin, uint shinyOffset, bool fastSearch, int listIndex, CgearSettings settings)
        {
            bool ConsiderTrigger = checkBoxTriggerBattle.Checked && checkBoxTriggerBattle.Visible;

            uint minAdvances;
            if ((
                generators[listIndex].FrameType == FrameType.Method5Standard || generators[listIndex].FrameType == FrameType.Method5Natures) 
                && shinyOffset > 0)
                minAdvances = shinygenerators[listIndex].InitialFrame;
            else
                minAdvances = generators[listIndex].InitialFrame;

            var array = new uint[80];
            array[6] = (uint)(profile.MAC_Address & 0xFFFF);

            if (profile.SoftReset)
            {
                array[6] = array[6] ^ 0x01000000;
            }

            var upperMAC = (uint)(profile.MAC_Address >> 16);
            array[7] = (upperMAC ^ (profile.VFrame * 0x1000000) ^ profile.GxStat);

            // Get the version-unique part of the message
            Array.Copy(Nazos.Nazo(profile.Version, profile.Language, profile.DSType), array, 5);

            array[10] = 0x00000000;
            array[11] = 0x00000000;
            array[13] = 0x80000000;
            array[14] = 0x00000000;
            array[15] = 0x000001A0;

            List<List<ButtonComboType>> keypressList = profile.GetKeypresses();

            List<ButtonComboType>[] buttons = keypressList.ToArray();
            var buttonMashValue = new uint[keypressList.Count];

            for (int i = 0; i < buttons.Length; i++)
            {
                buttonMashValue[i] = Functions.buttonMashed(buttons[i]);
            }

            uint searchRange = generator.MaxResults;

            // necessary to keep track of fast searching
            // for frames 1-6

            var included = new bool[6];

            uint start = profile.IsBW2() ? 2u : 0;
            uint entralink = (profile.IsBW2() && minAdvances > 24)
                                 ? 24u
                                 : 0;

            for (int i = 0; i < 6; i++)
            {
                if ((i + start + entralink) >= (generators[listIndex].InitialFrame - 1) &&
                    (i + start + entralink) < (generators[listIndex].InitialFrame + generators[listIndex].MaxResults - 1))
                    included[i] = true;
                else
                    included[i] = false;
            }

            if (generators[listIndex].FrameType == FrameType.Method5Natures)
            {
                generators[listIndex].isBW2 = profile.IsBW2(); // this is for PIDRNG encounter slots. Was missing
                generators[listIndex].MinAdvances = shinyOffsetMin;
                generators[listIndex].MaxResults = shinyOffset;
                generators[listIndex].MinLevel = (int)numericLevelMin.Value;
                generators[listIndex].MaxLevel = (int)numericLevelMax.Value;
                generators[listIndex].SearchForTrigger = ConsiderTrigger;
                generators[listIndex].RerollCount = profile.ShinyCharm ? 3 : 1;
                generators[listIndex].MemoryLinkUsed = profile.MemoryLink;
            }

            foreach (int month in months)
            {
                GetDayIntervalForCpu((int)year, month, listIndex, out var dayMin, out var dayMax);

                string yearMonth = String.Format("{0:00}", year % 2000) + String.Format("{0:00}", month);
                for (int buttonCount = 0; buttonCount < keypressList.Count; buttonCount++)
                {
                    array[12] = buttonMashValue[buttonCount];
                    for (uint Timer0 = profile.Timer0Min; Timer0 <= profile.Timer0Max; Timer0++)
                    {
                        array[5] = (profile.VCount << 16) + Timer0;
                        array[5] = Functions.Reorder(array[5]);

                        for (int day = dayMin; day <= dayMax; day++)
                        {
                            var searchTime = new DateTime((int)year, month, day);

                            string dateString = String.Format("{0:00}", (int)searchTime.DayOfWeek);
                            dateString = String.Format("{0:00}", searchTime.Day) + dateString;
                            dateString = yearMonth + dateString;
                            array[8] = uint.Parse(dateString, NumberStyles.HexNumber);
                            array[9] = 0x0;

                            // For seeds with the same date, the contents of the SHA-1 array will be the same for the first 8 steps
                            // We are precomputing those 8 steps to save time
                            // Trying to precompute beyond 8 steps is complicated and does not save much time, also runs the risk of errors

                            uint[] alpha = Functions.AlphaEncrypt(array);

                            // We are also precomputing select portions of the SHA-1 array during the expansion process
                            // As they are also the same

                            array[16] = Functions.RotateLeft(array[13] ^ array[8] ^ array[2] ^ array[0], 1);
                            array[18] = Functions.RotateLeft(array[15] ^ array[10] ^ array[4] ^ array[2], 1);
                            array[19] = Functions.RotateLeft(array[16] ^ array[11] ^ array[5] ^ array[3], 1);
                            array[21] = Functions.RotateLeft(array[18] ^ array[13] ^ array[7] ^ array[5], 1);
                            array[22] = Functions.RotateLeft(array[19] ^ array[14] ^ array[8] ^ array[6], 1);
                            array[24] = Functions.RotateLeft(array[21] ^ array[16] ^ array[10] ^ array[8], 1);
                            array[27] = Functions.RotateLeft(array[24] ^ array[19] ^ array[13] ^ array[11], 1);

                            for (int hour = hourMin; hour <= hourMax; hour++)
                            {
                                //int seedHour = hour;
                                for (int minute = 0; minute <= 59; minute++)
                                {
                                    waitHandle.WaitOne();
                                    for (int second = 0; second <= 59; second++)
                                    {
                                        array[9] = Functions.seedSecond(second) | Functions.seedMinute(minute) |
                                                   Functions.seedHour(hour, profile.DSType);

                                        ulong seed = Functions.EncryptSeed(array, alpha);

                                        List<Frame> frames;
                                        if (generators[listIndex].FrameType == FrameType.Method5Standard)
                                        {
                                            //  Set this to our seed here
                                            generators[listIndex].InitialSeed = seed >> 32;

                                            if (fastSearch)
                                            {
                                                var testSeed = (uint)generators[listIndex].InitialSeed;
                                                frames = new List<Frame>();

                                                for (uint i = 0; i < 6; i++)
                                                {
                                                    if (included[i])
                                                    {
                                                        if (list[i].ContainsKey(testSeed))
                                                        {
                                                            uint IVHash = list[i][testSeed];
                                                            frames.AddRange(generators[listIndex].Generate(
                                                                frameCompare, testSeed, IVHash,
                                                                i + start + entralink));
                                                        }
                                                    }
                                                }

                                                progressSearched += searchRange;
                                                if (frames.Count == 0)
                                                    continue;
                                            }
                                            else
                                            {
                                                frames = generators[listIndex].Generate(frameCompare, profile.ID, profile.SID);
                                                progressSearched += searchRange;
                                            }

                                            if (iframes.Count > 1000000)
                                                break;

                                            //  This is where we actually go ahead and call our 
                                            //  generator for a list of IVs based on parameters
                                            //  that have been passed in.


                                            //  Now we need to iterate through each result here
                                            //  and create a collection of the information that
                                            //  we are going to place into our grid.
                                            foreach (Frame frame in frames)
                                            {
                                                if (shinyOffset > 0)
                                                {
                                                    shinygenerators[listIndex].isBW2 = profile.IsBW2(); // this is for PIDRNG encounter slots. Was missing
                                                    shinygenerators[listIndex].InitialSeed = seed;
                                                    shinygenerators[listIndex].InitialFrame =
                                                        Functions.initialPIDRNG(seed, profile) + minAdvances;
                                                    shinygenerators[listIndex].MinAdvances = shinyOffsetMin;
                                                    shinygenerators[listIndex].MinLevel = (int)numericLevelMin.Value;
                                                    shinygenerators[listIndex].MaxLevel = (int)numericLevelMax.Value;
                                                    shinygenerators[listIndex].SearchForTrigger = ConsiderTrigger;
                                                    shinygenerators[listIndex].RerollCount = profile.ShinyCharm ? 3 : 1;
                                                    shinygenerators[listIndex].MaxLuckyPowerLVL = profile.LuckyPowerLVL;
                                                    shinygenerators[listIndex].MemoryLinkUsed = profile.MemoryLink;

                                                    List<Frame> shinyFrames =
                                                        shinygenerators[listIndex].GenerateG5PID(subFrameCompare, profile.ID, profile.SID);

                                                    foreach (Frame shinyFrame in shinyFrames)
                                                    {
                                                        if (shinyFrame != null)
                                                        {
                                                            frame.DisplayPrep();
                                                            Frame testFrame = Frame.Clone(frame);

                                                            var iframe = new IFrameCapture
                                                            { NearestShiny = shinyFrame.Number };

                                                            iframe.Offset = testFrame.Number - start;

                                                            // Since turning on the C-gear advance the IV frame by 2,
                                                            // it's impossible to hit IV frame 0 or 1 when using Pass Powers
                                                            if (shinyFrame.luckyPower != 0 && iframe.Offset < 2)
                                                                continue;

                                                            testFrame.Pid = shinyFrame.Pid;
                                                            testFrame.Ability = shinyFrame.Ability;
                                                            testFrame.Nature = shinyFrame.Nature;
                                                            testFrame.EncounterSlot = shinyFrame.EncounterSlot;
                                                            testFrame.EncounterMod = shinyFrame.EncounterMod;
                                                            testFrame.Synchable = shinyFrame.Synchable;
                                                            testFrame.Ratio = shinyFrame.Ratio;
                                                            testFrame.Level = shinyFrame.Level;
                                                            testFrame.Double = shinyFrame.Double;
                                                            testFrame.luckyPower = shinyFrame.luckyPower;

                                                            iframe.Seed = seed;
                                                            iframe.Frame = testFrame;
                                                            iframe.Advances = iframe.NearestShiny -
                                                                              shinygenerators[listIndex].InitialFrame;

                                                            iframe.TimeDate =
                                                                searchTime.AddHours(hour).AddMinutes(minute).AddSeconds(
                                                                    second);
                                                            iframe.KeyPresses = buttons[buttonCount];
                                                            iframe.Timer0 = Timer0;

                                                            lock (threadLock)
                                                            {
                                                                iframes.Add(iframe);
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    var iframe = new IFrameCapture();

                                                    frame.DisplayPrep();
                                                    iframe.Offset = frame.Number - start;
                                                    iframe.Seed = seed;
                                                    iframe.Frame = frame;

                                                    iframe.TimeDate =
                                                        searchTime.AddHours(hour).AddMinutes(minute).AddSeconds(second);
                                                    iframe.KeyPresses = buttons[buttonCount];
                                                    iframe.Timer0 = Timer0;

                                                    lock (threadLock)
                                                    {
                                                        iframes.Add(iframe);
                                                    }
                                                }
                                            }

                                            progressFound = (ulong)iframes.Count;
                                            if (frames.Count > 0)
                                            {
                                                refreshQueue = true;
                                            }
                                        }
                                        else
                                        {
                                            // Set this to our seed here
                                            generators[listIndex].InitialSeed = seed;
                                            generators[listIndex].InitialFrame =
                                                Functions.initialPIDRNG(seed, profile) +
                                                minAdvances;

                                            if (iframes.Count > 1000000)
                                                break;

                                            //  This is where we actually go ahead and call our 
                                            //  generator for a list of IVs based on parameters
                                            //  that have been passed in.
                                            frames = generators[listIndex].GenerateG5PID(frameCompare, profile.ID,
                                                                                    profile.SID);

                                            progressSearched += searchRange;

                                            foreach (Frame frame in frames)
                                            {
                                                frame.DisplayPrep();
                                                var iframe = new IFrameCapture
                                                {
                                                    NearestShiny = frame.Number,
                                                    Seed = seed,
                                                    Frame = frame
                                                };
                                                iframe.Advances = iframe.Offset -
                                                                  (generators[listIndex].InitialFrame - minAdvances);

                                                iframe.TimeDate =
                                                    searchTime.AddHours(hour).AddMinutes(minute).AddSeconds(second);
                                                iframe.KeyPresses = buttons[buttonCount];
                                                iframe.Timer0 = Timer0;

                                                List<Frame> CgearFrames = new List<Frame>();
                                                FrameCompare compareIVs = new FrameCompare(
                                                    ivFiltersCapture.IVFilter, null, -1, false, false, 0, false, null, new NoGenderFilter());

                                                FrameGenerator Cgenerator = generators[listIndex].Clone();
                                                Cgenerator.FrameType = FrameType.Method5Standard;

                                                Cgenerator.InitialFrame = settings.minIVFrame;
                                                Cgenerator.MaxResults = settings.maxIVFrame;
                                                uint minDelay = settings.delayMin;
                                                uint maxDelay = settings.delayMax;

                                                for (uint delay = minDelay; delay <= maxDelay; delay++)
                                                {
                                                    ulong CSeed;
                                                    if (settings.fixedRTC)
                                                    {
                                                        CSeed = (ulong)((((month * day + minute + second) & 0xFF) * 0x1000000) +
                                                                (hour * 0x10000)) +
                                                                (year - 2000 + delay) +
                                                                (profile.MAC_Address & 0xFFFFFF);
                                                    }
                                                    else
                                                    {
                                                        int newMinutes = minute;
                                                        int newHour = hour;
                                                        int newDay = day;

                                                      //int newSeconds = second + delay / 60;
                                                        int newSeconds = second + (int)Math.Round(delay / 60.0);    // Assuming 60 fps for now

                                                        if (newSeconds > 59)
                                                        {
                                                            newMinutes += newSeconds / 60;
                                                            newSeconds = newSeconds % 60;

                                                            if (newMinutes > 59)
                                                            {
                                                                newHour += newMinutes / 60;
                                                                newMinutes = newMinutes % 60;

                                                                if (newHour > 23)
                                                                {
                                                                    //newDay += newHour / 24;
                                                                    // No delay will ever advance more than a single day though
                                                                    newDay++;
                                                                    newHour = newHour % 24;
                                                                }
                                                            }
                                                        }

                                                        CSeed = (ulong)((((month * newDay + newMinutes + newSeconds) & 0xFF) * 0x1000000) +
                                                                (newHour * 0x10000)) +
                                                                (year - 2000 + delay) +
                                                                (profile.MAC_Address & 0xFFFFFF);
                                                    }

                                                    Cgenerator.InitialSeed = CSeed;
                                                    CgearFrames = Cgenerator.Generate(compareIVs, profile.ID, profile.SID);

                                                    foreach (Frame f in CgearFrames)
                                                    {
                                                        Frame testFrame = Frame.ClonePID(frame);

                                                        testFrame.Hp = f.Hp;
                                                        testFrame.Atk = f.Atk;
                                                        testFrame.Def = f.Def;
                                                        testFrame.Spa = f.Spa;
                                                        testFrame.Spd = f.Spd;
                                                        testFrame.Spe = f.Spe;

                                                        iframe.Frame = testFrame;
                                                        iframe.Delay = delay - settings.calibDelay;
                                                        iframe.CSeed = (uint)CSeed;
                                                        iframe.Offset = f.Number;

                                                        lock (threadLock)
                                                        {
                                                            iframes.Add(iframe);
                                                        }
                                                    }
                                                }
                                            }
                                            progressFound = (ulong)iframes.Count;
                                            if (frames.Count > 0)
                                            {
                                                refreshQueue = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GenerateCGearCapJob(ulong mac_address, uint minEfgh, uint maxEfgh, bool fastSearch, ushort id, ushort sid)
        {
            uint seed;
            uint searchRange = generator.MaxResults;
            List<Frame> frames;

            var included = new bool[6];

            for (int i = 0; i < 6; i++)
            {
                if ((i + 20) >= (generator.InitialFrame - 1) &&
                    (i + 20) < (generator.InitialFrame + generator.MaxResults - 1))
                    included[i] = true;
                else
                    included[i] = false;
            }

            if (fastSearch)
            {
                //frames = new List<Frame>();
                //todo: reverse order of ab/efgh loop for optimization
                //  Iterate through delay range + year
                for (uint efgh = minEfgh; efgh <= maxEfgh; efgh++)
                {
                    waitHandle.WaitOne();
                    //  Iterate through all CD
                    for (uint cd = 0; cd <= 23; cd++)
                    {
                        for (uint ab = 1; ab <= 255; ab++)
                        {
                            //  First we need to build a seed for this iteration
                            //  based on all of our information.  This should be
                            //  fairly easy since we are not using dates ;)
                            seed = (ab << 24) + (cd << 16) + efgh;
                            seed = (uint)(seed + (mac_address & 0xFFFFFF));

                            progressSearched += searchRange;

                            for (uint i = 0; i < 6; i++)
                            {
                                if (included[i])
                                {
                                    if (list[i].ContainsKey(seed))
                                    {
                                        uint ivHash = list[i][seed];
                                        frames = generator.Generate(frameCompare, seed, ivHash, i + 21);

                                        progressFound += (uint)frames.Count;

                                        foreach (Frame frame in frames)
                                        {
                                            var iframe = new IFrameCapture();
                                            frame.DisplayPrep();

                                            iframe.Offset = frame.Number;
                                            iframe.Seed = frame.Seed;
                                            iframe.Frame = frame;
                                            iframe.MACAddress = (uint)mac_address;

                                            lock (threadLock)
                                            {
                                                iframes.Add(iframe);
                                            }
                                            refreshQueue = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //  Iterate through all AB
                for (uint ab = 1; ab <= 255; ab++)
                {
                    waitHandle.WaitOne();
                    //  Iterate through all CD
                    for (uint cd = 0; cd <= 23; cd++)
                    {
                        //  Iterate through delay range + year
                        for (uint efgh = minEfgh; efgh <= maxEfgh; efgh++)
                        {
                            //  First we need to build a seed for this iteration
                            //  based on all of our information.  This should be
                            //  fairly easy since we are not using dates ;)
                            seed = (ab << 24) + (cd << 16) + efgh;
                            seed = (uint)(seed + (mac_address & 0xFFFFFF));

                            //  Set this to our seed here
                            generator.InitialSeed = seed;

                            if (iframes.Count > 1000000)
                                break;

                            //  This is where we actually go ahead and call our 
                            //  generator for a list of IVs based on parameters
                            //  that have been passed in.
                            frames = generator.Generate(frameCompare, id, sid);

                            progressSearched += searchRange;
                            progressFound += (uint)frames.Count;

                            //  Now we need to iterate through each result here
                            //  and create a collection of the information that
                            //  we are going to place into our grid.
                            foreach (Frame frame in frames)
                            {
                                var iframe = new IFrameCapture();
                                frame.DisplayPrep();

                                iframe.Offset = frame.Number;
                                iframe.Seed = seed;
                                iframe.Frame = frame;
                                iframe.MACAddress = (uint)mac_address;

                                lock (threadLock)
                                {
                                    iframes.Add(iframe);
                                }
                                refreshQueue = true;
                            }
                        }
                    }
                }
            }
        }

        private void EnableCapGenerate()
        {
            buttonCapGenerate.Enabled = true;
            buttonShinyGenerate.Enabled = true;
            buttonEventGenerate.Enabled = true;
            buttonPickupSearch.Enabled = true;
        }

        private void comboBoxMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((ComboBoxItem)comboBoxMethod.SelectedItem).Reference.Equals(FrameType.Method5Natures) ||
                ((ComboBoxItem)comboBoxMethod.SelectedItem).Reference.Equals(FrameType.Method5Standard))
            {
                labelCapMonth.Visible = true;
                checkBoxShinyOnly.Enabled = true;
                comboBoxCapNature.Enabled = true;
                comboCapAbility.Enabled = true;
                comboBoxEncounterType.Enabled = true;
                comboCapEncounterSlot.Enabled = true;
                comboBoxCapGenderRatio.Enabled = true;

                comboBoxCapMonth.Visible = true;

                labelCapMinMaxFrame.Location = oldLocation;

                if (((ComboBoxItem)comboBoxMethod.SelectedItem).Reference.Equals(FrameType.Method5Standard))
                {
                    label9.Visible = true;
                    maskedTextBoxCapMinDelay.Visible = false;
                    maskedTextBoxCapMaxDelay.Visible = false;
                    labelDelay.Visible = false;
                    checkBoxRAD.Visible = false;
                    checkBoxDelay.Visible = numCalibrateDelay.Visible = false;
                }
                else
                {
                    label9.Visible = false;
                    maskedTextBoxCapMinDelay.Visible = true;
                    maskedTextBoxCapMaxDelay.Visible = true;
                    labelDelay.Visible = true;
                    checkBoxRAD.Visible = true;
                    checkBoxDelay.Visible = numCalibrateDelay.Visible = true;
                }
            }
            else if (((ComboBoxItem)comboBoxMethod.SelectedItem).Reference.Equals(FrameType.Method5CGear))
            {
                labelCapMonth.Visible = false;
                checkBoxShinyOnly.Enabled = false;
                comboBoxCapNature.Enabled = false;
                comboCapAbility.Enabled = false;
                comboBoxEncounterType.Enabled = true;
                comboCapEncounterSlot.Enabled = false;
                comboBoxCapGenderRatio.Enabled = false;

                comboBoxCapMonth.Visible = false;
                maskedTextBoxCapMinDelay.Visible = true;
                maskedTextBoxCapMaxDelay.Visible = true;
                maskedTextBoxCapMinDelay.TabStop = true;
                maskedTextBoxCapMaxDelay.TabStop = true;

                labelCapMinMaxFrame.Text = "Min / Max Frame";
                labelCapMinMaxFrame.Location = oldLocation;

                label9.Visible = true;
                labelDelay.Visible = true;
                checkBoxRAD.Visible = false;
                checkBoxDelay.Visible = numCalibrateDelay.Visible = false;
            }

            checkBoxShinyOnly.Text = "Shiny Only";

            IVFilters_Changed(sender, e);
            labelCapMinMaxLevel.Visible = numericLevelMin.Visible = numericLevelMax.Visible = LevelLabel.Visible = numericLevel.Visible = LevelConditions();

            checkBoxTriggerBattle.Visible = RatioConditions();
        }

        private void buttonAnyNature_Click(object sender, EventArgs e)
        {
            comboBoxCapNature.ClearSelection();
        }

        private void dataGridViewCapValues_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var profile = (Profile)comboBoxProfiles.SelectedItem;

            DefaultFormatting(dataGridViewCapValues, profile.ID, profile.SID, e, "PID");

            if (EncounterRatio.Visible)
            {
                int RatioValue = Convert.ToInt32(dataGridViewCapValues.Rows[e.RowIndex].Cells["EncounterRatio"].Value);
                if (dataGridViewCapValues.Columns[e.ColumnIndex].Name == "EncounterRatio")
                {
                    if ((RatioValue < 14 && comboBoxEncounterType.SelectedIndex <= 2) || (RatioValue < 6 && comboBoxEncounterType.SelectedIndex == 3))
                    {
                        e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                        e.CellStyle.ForeColor = Color.Green;
                    }
                    else
                        e.CellStyle.ForeColor = Color.LightCoral;
                }
            }

            //  Make all of the junk natures show up in a lighter color
            if (e.ColumnIndex == CapNatureIndex)
            {
                var nature = (string)e.Value;

                if ((bool)dataGridViewCapValues.Rows[e.RowIndex].Cells["Synchable"].Value)
                {
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);

                    if (((IFrameCapture)dataGridViewCapValues.Rows[e.RowIndex].DataBoundItem).Frame.EncounterMod ==
                        Objects.EncounterMod.Synchronize)
                    {
                        e.Value = "Synch";
                    }
                }
                else if (nature == Functions.NatureStrings(18) ||
                         nature == Functions.NatureStrings(6) ||
                         nature == Functions.NatureStrings(0) ||
                         nature == Functions.NatureStrings(24) ||
                         nature == Functions.NatureStrings(12) ||
                         nature == Functions.NatureStrings(9) ||
                         nature == Functions.NatureStrings(21))
                {
                    e.CellStyle.ForeColor = Color.Gray;
                }
            }

            if (e.ColumnIndex >= CapHPIndex && e.ColumnIndex <= CapSpeedIndex)
            {
                var number = (uint)e.Value;

                if (number >= 30)
                {
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }

                if (number == 0)
                {
                    e.CellStyle.ForeColor = Color.Red;
                }
            }
        }

        private void generateTimesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridViewCapValues.SelectedRows[0] != null)
            {
                var frame = (IFrameCapture)dataGridViewCapValues.SelectedRows[0].DataBoundItem;

                // This is a bit of a strange hack, because this window
                //  needs to be hidden before we load the seed to time
                //  form or it wont be able to be focused. 
                bool showMap = HgSsRoamerSW.Window.Map.Visible;
                HgSsRoamerSW.Window.Hide();

                var seedToTime = new SeedToTime();

                //  Get the currently selected frame here so we can
                //  pull out some of the values that we are going to
                //  need to use.

                seedToTime.setBW();

                seedToTime.AutoGenerate = true;
                seedToTime.ShowMap = showMap;
                seedToTime.Seed = (uint)frame.Seed;
                seedToTime.MAC_Address = ((Profile)comboBoxProfiles.SelectedItem).MAC_Address;

                //  Grab this from what the user had searched on
                seedToTime.Year = (uint)DateTime.Now.Year;
                if (maskedTextBoxCapYear.Text != "")
                    seedToTime.Year = uint.Parse(maskedTextBoxCapYear.Text);

                seedToTime.Show();
            }
        }

        private void outputCapResultsToTXTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //  Going to need to present the user with a File Dialog and 
            //  then interate through the Grid, outputting columns that
            //  are visible.

            saveFileDialogTxt.AddExtension = true;
            saveFileDialogTxt.Title = "Save Output to TXT";
            saveFileDialogTxt.Filter = "TXT Files|*.txt";
            saveFileDialogTxt.FileName = "rngreporter.txt";
            if (saveFileDialogTxt.ShowDialog() == DialogResult.OK)
            {
                //  Get the name of the file and then go ahead 
                //  and create and save the thing to the hard
                //  drive.   
                List<IFrameCapture> frames = iframes;

                if (frames.Count > 0)
                {
                    var writer = new TXTWriter(dataGridViewCapValues);
                    writer.Generate(saveFileDialogTxt.FileName, frames);
                }
            }
        }

        private void dataGridViewCapValues_MouseDown(object sender, MouseEventArgs e)
        {
            DGV_MouseDown(dataGridViewCapValues, e);
        }

        private void dataGridViewCapValues_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (Settings.Default.ShowToolTips)
            {
                Rectangle cellRect = dataGridViewCapValues.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);

                if (dataGridViewCapValues.Columns[e.ColumnIndex].Name == "EncounterMod")
                {
                    if (e.RowIndex >= 0)
                    {
                        switch (
                            ((IFrameCapture)dataGridViewCapValues.Rows[e.RowIndex].DataBoundItem).Frame.EncounterMod)
                        {
                            case Objects.EncounterMod.Synchronize:
                                toolTipDataGrid.ToolTipTitle = "Synchronize";

                                toolTipDataGrid.Show(
                                    "When encountering the desired Pokémon, the lead Pokémon in your party\r\n" +
                                    "must have the ability Synchronize, and have a nature that matches your\r\n" +
                                    "desired nature.  This will cause the target Pokémon to have your desired nature.",
                                    this,
                                    dataGridViewCapValues.Location.X + cellRect.X + cellRect.Size.Width,
                                    dataGridViewCapValues.Location.Y + cellRect.Y + cellRect.Size.Height,
                                    5000);
                                break;
                            case Objects.EncounterMod.CuteCharm50F:
                            case Objects.EncounterMod.CuteCharm125F:
                            case Objects.EncounterMod.CuteCharm25F:
                            case Objects.EncounterMod.CuteCharm75F:
                            case Objects.EncounterMod.CuteCharm50M:
                            case Objects.EncounterMod.CuteCharm875M:
                            case Objects.EncounterMod.CuteCharm75M:
                            case Objects.EncounterMod.CuteCharm25M:
                            case Objects.EncounterMod.CuteCharmFemale:
                                toolTipDataGrid.ToolTipTitle = "Cute Charm";

                                toolTipDataGrid.Show(
                                    "When encountering the target Pokémon, the lead Pokémon in your party\r\n" +
                                    "must have the ability Cute Charm, and be the opposite gender of the listed target.\r\n" +
                                    "The listed gender ratio must also match that of the target Pokémon.\r\n\r\n" +
                                    "For example: Cute Charm (75% M) indicates that the target Pokémon must be\r\n" +
                                    "male (requiring a female Cute Charm lead), and must be of a species that has a\r\n" +
                                    "75% male gender ratio, such as Alakazam.\r\n\r\n" +
                                    "Cute Charm does not work for species with only one gender, such as Tauros.",
                                    this,
                                    dataGridViewCapValues.Location.X + cellRect.X + cellRect.Size.Width,
                                    dataGridViewCapValues.Location.Y + cellRect.Y + cellRect.Size.Height,
                                    15000);
                                break;
                            case Objects.EncounterMod.SuctionCups:
                                toolTipDataGrid.ToolTipTitle = "Suction Cups";

                                toolTipDataGrid.Show(
                                    "When fishing for the target Pokémon, the lead Pokémon in your party\r\n" +
                                    "must have the ability Suction Cups.  Otherwise, fishing will fail\r\n" +
                                    "with \"Not even a nibble.\"\r\n\r\n" +
                                    "Some non-fishing encounters may also require Suction Cups in order\r\n" +
                                    "to make the frame appear.",
                                    this,
                                    dataGridViewCapValues.Location.X + cellRect.X + cellRect.Size.Width,
                                    dataGridViewCapValues.Location.Y + cellRect.Y + cellRect.Size.Height,
                                    5000);
                                break;
                            case Objects.EncounterMod.Compoundeyes:
                                toolTipDataGrid.ToolTipTitle = "Compoundeyes";
                                break;
                            case Objects.EncounterMod.None:
                                toolTipDataGrid.Hide(this);
                                break;
                        }
                    }
                }
                else if (dataGridViewCapValues.Columns[e.ColumnIndex].Name == "Nature")
                {
                    toolTipDataGrid.ToolTipTitle = "Nature";

                    toolTipDataGrid.Show("A bolded nature indicates that the nature can be changed by a lead\r\n" +
                                         "Pokémon with Synchronize.\r\n\r\n" +
                                         "Greyed-out natures are natures with no competitive value.",
                                         this,
                                         dataGridViewCapValues.Location.X + cellRect.X + cellRect.Size.Width,
                                         dataGridViewCapValues.Location.Y + cellRect.Y + cellRect.Size.Height,
                                         5000);
                }
                else if (dataGridViewCapValues.Columns[e.ColumnIndex].Name == "Shiny")
                {
                    toolTipDataGrid.ToolTipTitle = "!!!";

                    toolTipDataGrid.Show("A !!! in this column indicates the frame will be shiny.",
                                         this,
                                         dataGridViewCapValues.Location.X + cellRect.X + cellRect.Size.Width,
                                         dataGridViewCapValues.Location.Y + cellRect.Y + cellRect.Size.Height,
                                         5000);
                }
                else if (dataGridViewCapValues.Columns[e.ColumnIndex].Name == "EncounterSlot")
                {
                    toolTipDataGrid.ToolTipTitle = "Encounter Slot";

                    toolTipDataGrid.Show("Encounter slots are used to determine what Pokémon appears for\r\n" +
                                         "a wild battle.  Use the encounter tables under the main menus to look up\r\n" +
                                         "which Pokémon appears for each slot in each area.\r\n",
                                         this,
                                         dataGridViewCapValues.Location.X + cellRect.X + cellRect.Size.Width,
                                         dataGridViewCapValues.Location.Y + cellRect.Y + cellRect.Size.Height,
                                         5000);
                }
            }
        }

        private void dataGridViewCapValues_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            toolTipDataGrid.Hide(this);
        }

        private void generateAdjacentSeedsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridViewCapValues.SelectedRows[0] != null)
            {
                var iframe = (IFrameCapture)dataGridViewCapValues.SelectedRows[0].DataBoundItem;

                uint advances = 0;

                switch (iframe.Frame.FrameType)
                {
                    case FrameType.Method5Standard:
                        advances = iframe.Offset;
                        break;
                    case FrameType.Method5Natures:
                    case FrameType.Wondercard5thGen:
                        advances = iframe.Advances;
                        break;
                }
                int profile = comboBoxProfiles.SelectedIndex;
                var adjacents = new Adjacents(iframe.TimeDate,
                                              profile, iframe.KeyPresses,
                                              iframe.Frame.FrameType, iframe.Frame.EncounterType,
                                              advances);
                adjacents.Show();
            }
        }

        private bool FastCapFilters()
        {
            IVFilter filter = ivFiltersCapture.IVFilter;
            return FastFilters(filter);
        }

        private bool FastFilters(IVFilter filter)
        {
            // HP, Def, SpD filters must search for >=30
            // Either Attack or SpA must be >=30 as well
            if (((filter.hpValue >= 30) &&
                 (filter.hpCompare == CompareType.Equal || filter.hpCompare == CompareType.GtEqual)) &&
                ((filter.defValue >= 30) &&
                 (filter.defCompare == CompareType.Equal || filter.defCompare == CompareType.GtEqual)) &&
                ((filter.spdValue >= 30) &&
                 (filter.spdCompare == CompareType.Equal || filter.spdCompare == CompareType.GtEqual)))
            {
                // physical spreads must have Attack >=30 and flawless speed (either for standard or Trick Room)
                if ((filter.atkValue >= 30) &&
                    (filter.atkCompare == CompareType.Equal || filter.atkCompare == CompareType.GtEqual))
                {
                    if ((filter.speValue == 31) &&
                        (filter.speCompare == CompareType.Equal || filter.speCompare == CompareType.GtEqual))
                        return true;

                    // no roamers have Trick Room spreads
                    if (((filter.speValue <= 1) &&
                         (filter.speCompare == CompareType.Equal || filter.speCompare == CompareType.LtEqual)) &&
                        !((ComboBoxItem)comboBoxEncounterType.SelectedItem).Reference.Equals(EncounterType.Roamer))
                        return true;
                }

                // special spreads must have SpAttk >=30 and flawless speed (either for standard or Trick Room)
                // or, if Speed = 30 or 2 and 3, Attack must be HP_O or HP_E to allow for 70-power Hidden Power
                // spreads for roamers must have a speed of 30 or 31
                if ((filter.spaValue >= 30) &&
                    (filter.spaCompare == CompareType.Equal || filter.spaCompare == CompareType.GtEqual))
                {
                    if ((filter.speValue == 31) &&
                        (filter.speCompare == CompareType.Equal || filter.speCompare == CompareType.GtEqual))
                        return true;

                    if (((filter.speValue == 30) &&
                         (filter.speCompare == CompareType.Equal || filter.speCompare == CompareType.GtEqual)) ||
                        (!((ComboBoxItem)comboBoxEncounterType.SelectedItem).Reference.Equals(EncounterType.Roamer) &&
                         (((filter.speValue == 2) && (filter.speCompare == CompareType.Equal)) ||
                          ((filter.speValue == 3) && (filter.speCompare == CompareType.Equal)))))
                    {
                        if (((filter.atkValue >= 30) &&
                            (filter.atkCompare == CompareType.Equal || filter.atkCompare == CompareType.GtEqual)) ||
                            (filter.atkCompare == CompareType.Hidden || filter.atkCompare == CompareType.HiddenEven ||
                            filter.atkCompare == CompareType.HiddenOdd))
                        {
                            return true;
                        }
                    }

                    // no roamers have Trick Room spreads
                    if ((filter.speValue <= 1) &&
                        (filter.speCompare == CompareType.Equal || filter.speCompare == CompareType.LtEqual) &&
                        !((ComboBoxItem)comboBoxEncounterType.SelectedItem).Reference.Equals(EncounterType.Roamer))
                        return true;
                }
            }

            return false;
        }

        private bool FastCapFrames()
        {
            uint minFrame;
            uint maxFrame;

            uint.TryParse(maskedTextBoxCapMinOffset.Text, out minFrame);
            uint.TryParse(maskedTextBoxCapMaxOffset.Text, out maxFrame);

            if (((ComboBoxItem)comboBoxMethod.SelectedItem).Reference.Equals(FrameType.Method5CGear))
            {
                // no roamers appear in the Entralink
                if (((ComboBoxItem)comboBoxEncounterType.SelectedItem).Reference.Equals(EncounterType.Roamer))
                    return false;

                return (minFrame >= 20 && minFrame < 26) && (maxFrame >= 20 && maxFrame <= 25);
            }
            // BW2 Entralink
            if (((Profile)comboBoxProfiles.SelectedItem).IsBW2())
            {
                if (minFrame > 24 && minFrame < 31 && maxFrame > 24 && maxFrame < 31) return true;
            }

            return (minFrame >= 0 && minFrame < 6) && (maxFrame >= 0 && maxFrame < 6);
        }

        #endregion


        #region Wondercard

        private void buttonEventGenerate_Click(object sender, EventArgs e)
        {
            var profile = (Profile)comboBoxProfiles.SelectedItem;
            iframes = new List<IFrameCapture>();
            listBindingEvent = new BindingSource { DataSource = iframes };
            dataGridViewEventResults.DataSource = listBindingEvent;

            jobs = new Thread[cpus];
            generators = new FrameGenerator[cpus];
            shinygenerators = new FrameGenerator[cpus];
            waitHandle = new EventWaitHandle(true, EventResetMode.ManualReset);

            var year = (uint)DateTime.Now.Year;
            if (maskedTextYear.Text != "")
            {
                year = uint.Parse(maskedTextYear.Text);
                if (year < 2000)
                {
                    MessageBox.Show("You must enter a year greater than 1999.", "Please Enter a Valid Year", MessageBoxButtons.OK);
                    return;
                }
            }

            uint maxOffsetEvent;
            if (maskedMaxAdv.Text != "")
                maxOffsetEvent = uint.Parse(maskedMaxAdv.Text);
            else
            {
                maskedMaxAdv.Text = "300";
                maxOffsetEvent = 300;
            }

            uint minOffsetEvent;
            if (maskedMinAdv.Text != "")
                minOffsetEvent = uint.Parse(maskedMinAdv.Text);
            else
            {
                maskedMinAdv.Text = "0";
                minOffsetEvent = 0;
            }


            if (minOffsetEvent > maxOffsetEvent)
            {
                maskedMinAdv.Focus();
                maskedMinAdv.SelectAll();
                return;
            }

            Wondercard wondercard = new Wondercard()
            {
                eventTid = uint.Parse(maskedTextTID.Text),
                eventSid = uint.Parse(maskedTextSID.Text),
                eventShininess = glassComboShininess.SelectedIndex,
                // -1 in the following lines means that the value for a given
                // attribute is "random" and should be calculated
                eventAbility = checkBoxAbility.Checked ? glassComboAbility.SelectedIndex : -1,
                eventNature = checkBoxNatureLock.Checked ? glassComboBoxNatureList.SelectedIndex : -1,
                eventGender = checkBoxGender.Checked ? glassComboGender.SelectedIndex : -1,
                eventIVInfo = new int[6]
                {
                    checkBoxHP.Checked ? int.Parse(maskedTextBoxHP.Text) : -1,
                    checkBoxAtk.Checked ? int.Parse(maskedTextBoxAtk.Text) : -1,
                    checkBoxDef.Checked ? int.Parse(maskedTextBoxDef.Text) : -1,
                    checkBoxSpA.Checked ? int.Parse(maskedTextBoxSpA.Text) : -1,
                    checkBoxSpD.Checked ? int.Parse(maskedTextBoxSpD.Text) : -1,
                    checkBoxSpe.Checked ? int.Parse(maskedTextBoxSpe.Text) : -1,
                }
            };

            wondercard.genderCase = Functions.GenderLockedCase(glassComboGender.SelectedIndex, glassComboGenderRatio.SelectedIndex);

            eventf50.Visible = glassComboGenderRatio.SelectedIndex == 0;
            eventf75.Visible = glassComboGenderRatio.SelectedIndex == 1;
            eventf25.Visible = glassComboGenderRatio.SelectedIndex == 2;
            eventf125.Visible = glassComboGenderRatio.SelectedIndex == 3;

            // !!! This is generator for IV Frames !!!
            generator = new FrameGenerator
            {
                // Now that each combo box item is a custom object containing the FrameType reference
                // We can simply retrieve the FrameType from the selected item
                FrameType = FrameType.Wondercard5thGen,
                //EncounterType = (EncounterType)((ComboBoxItem)comboBoxEncounterType.SelectedItem).Reference,
                EncounterMod = Objects.EncounterMod.Search,
                InitialFrame = minOffsetEvent,
                MaxResults = maxOffsetEvent - minOffsetEvent + 1
            };

            generator.isBW2 = profile.IsBW2();


            //  Build up a FrameComparer

            List<uint> natures = null;
            if (glassComboEventNatures.Text != "Any" && glassComboEventNatures.CheckBoxItems.Count > 0)
                natures = (from t in glassComboEventNatures.CheckBoxItems where t.Checked select (uint)((Nature)t.ComboBoxItem).Number).ToList();

            EventSeed.Width = seedColumnLong(true, EventSeed);

            frameCompare = new FrameCompare(
                ivFiltersEvent.IVFilter,
                natures,
                comboAbilityEvents.SelectedIndex == 0 ? - 1 : comboAbilityEvents.SelectedIndex - 1,
                shinyOnlyEvent.Checked,
                false,
                0,
                false,
                null,
                constructGenderFilter());


            generateTimesToolStripMenuItem.Visible = false;
            generateEntralinkNatureSeedsToolStripMenuItem.Visible = false;
            generateAdjacentSeedsToolStripMenuItem.Visible = true;

            var months = new List<int>();
            for (int month = 1; month <= 12; month++)
            {
                if (checkBoxComboMonths.CheckBoxItems[month].Checked)
                    months.Add(month);
            }

            if (months.Count == 0)
            {
                checkBoxComboMonths.Focus();
                return;
            }

            List<List<ButtonComboType>> keypresses = profile.GetKeypresses();

            progressSearched = 0;
            progressFound = 0;

            int dayTotal = months.Sum(month => DateTime.DaysInMonth((int)year, month));
            progressTotal =
                (ulong)
                (dayTotal * 86400 * (maxOffsetEvent - minOffsetEvent + 1) * keypresses.Count *
                 (profile.Timer0Max - profile.Timer0Min + 1));

            for (int i = 0; i < jobs.Length; i++)
            {
                generators[i] = generator.Clone();

                if (shinygenerator != null)
                {
                    shinygenerators[i] = shinygenerator.Clone();
                }

                //copy to prevent issues with it being incremented before the actual thread really starts
                int i1 = i;
                //passing in a profile instead of the params would probably be more efficent
                //int shiny = comboBoxShiny.SelectedIndex;
                jobs[i] = new Thread(() => GenerateWonderCardJob(year, months, 0, 23, profile, i1, wondercard));
                jobs[i].Start();
                // for some reason not making the thread sleep causes issues with updating dayMin\Max
                Thread.Sleep(200);
            }
            var progressJob = new Thread(() => ManageProgress(listBindingEvent, dataGridViewEventResults, generator.FrameType, 2000));
            progressJob.Start();
            progressJob.Priority = ThreadPriority.Lowest;

            buttonCapGenerate.Enabled = false;
            buttonEventGenerate.Enabled = false;
            buttonShinyGenerate.Enabled = false;

            dataGridViewEventResults.Focus();
        }

        public void GenerateWonderCardJob(uint year, List<int> months, int hourMin, int hourMax, Profile profile, int listIndex, Wondercard wondercard)
        {
            uint minAdvances = generators[listIndex].InitialFrame;

            var array = new uint[80];
            array[6] = (uint)(profile.MAC_Address & 0xFFFF);

            if (profile.SoftReset)
            {
                array[6] = array[6] ^ 0x01000000;
            }

            var upperMAC = (uint)(profile.MAC_Address >> 16);
            array[7] = (upperMAC ^ (profile.VFrame * 0x1000000) ^ profile.GxStat);

            // Get the version-unique part of the message
            Array.Copy(Nazos.Nazo(profile.Version, profile.Language, profile.DSType), array, 5);

            array[10] = 0x00000000;
            array[11] = 0x00000000;
            array[13] = 0x80000000;
            array[14] = 0x00000000;
            array[15] = 0x000001A0;

            List<List<ButtonComboType>> keypressList = profile.GetKeypresses();
            List<ButtonComboType>[] buttons = keypressList.ToArray();
            var buttonMashValue = new uint[keypressList.Count];

            for (int i = 0; i < buttons.Length; i++)
            {
                buttonMashValue[i] = Functions.buttonMashed(buttons[i]);
            }

            uint searchRange = generator.MaxResults;

            // necessary to keep track of fast searching
            // for frames 1-6

            var included = new bool[6];

            for (int i = 0; i < 6; i++)
            {
                if (i >= (generators[listIndex].InitialFrame - 1) &&
                    i < (generators[listIndex].InitialFrame + generators[listIndex].MaxResults - 1))
                    included[i] = true;
                else
                    included[i] = false;
            }

            foreach (int month in months)
            {
                GetDayIntervalForCpu((int)year, month, listIndex, out var dayMin, out var dayMax);

                string yearMonth = String.Format("{0:00}", year % 2000) + String.Format("{0:00}", month);
                for (int buttonCount = 0; buttonCount < keypressList.Count; buttonCount++)
                {
                    array[12] = buttonMashValue[buttonCount];
                    for (uint timer0 = profile.Timer0Min; timer0 <= profile.Timer0Max; timer0++)
                    {
                        array[5] = (profile.VCount << 16) + timer0;
                        array[5] = Functions.Reorder(array[5]);

                        for (int day = dayMin; day <= dayMax; day++)
                        {
                            var searchTime = new DateTime((int)year, month, day);

                            string dateString = String.Format("{0:00}", (int)searchTime.DayOfWeek);
                            dateString = String.Format("{0:00}", searchTime.Day) + dateString;
                            dateString = yearMonth + dateString;
                            array[8] = uint.Parse(dateString, NumberStyles.HexNumber);
                            array[9] = 0x0;

                            // For seeds with the same date, the contents of the SHA-1 array will be the same for the first 8 steps
                            // We are precomputing those 8 steps to save time
                            // Trying to precompute beyond 8 steps is complicated and does not save much time, also runs the risk of errors

                            uint[] alpha = Functions.AlphaEncrypt(array);

                            // We are also precomputing select portions of the SHA-1 array during the expansion process
                            // As they are also the same

                            array[16] = Functions.RotateLeft(array[13] ^ array[8] ^ array[2] ^ array[0], 1);
                            array[18] = Functions.RotateLeft(array[15] ^ array[10] ^ array[4] ^ array[2], 1);
                            array[19] = Functions.RotateLeft(array[16] ^ array[11] ^ array[5] ^ array[3], 1);
                            array[21] = Functions.RotateLeft(array[18] ^ array[13] ^ array[7] ^ array[5], 1);
                            array[22] = Functions.RotateLeft(array[19] ^ array[14] ^ array[8] ^ array[6], 1);
                            array[24] = Functions.RotateLeft(array[21] ^ array[16] ^ array[10] ^ array[8], 1);
                            array[27] = Functions.RotateLeft(array[24] ^ array[19] ^ array[13] ^ array[11], 1);

                            for (int hour = hourMin; hour <= hourMax; hour++)
                            {
                                //int seedHour = hour;
                                for (int minute = 0; minute <= 59; minute++)
                                {
                                    waitHandle.WaitOne();
                                    for (int second = 0; second <= 59; second++)
                                    {
                                        array[9] = Functions.seedSecond(second) | Functions.seedMinute(minute) |
                                                   Functions.seedHour(hour, profile.DSType);

                                        ulong seed = Functions.EncryptSeed(array, alpha);

                                        // Set this to our seed here
                                        generators[listIndex].InitialSeed = seed;
                                        generators[listIndex].InitialFrame = Functions.initialPIDRNG(seed, profile) +
                                                                             minAdvances + (profile.IsBW2() ? 2u : 0);

                                        if (iframes.Count > 1000000)
                                            break;

                                        //  This is where we actually go ahead and call our 
                                        //  generator for a list of IVs based on parameters
                                        //  that have been passed in.
                                        List<Frame> frames = generators[listIndex].GenerateWonderCard(frameCompare, wondercard);

                                        progressSearched += searchRange;
                                        progressFound += (ulong)frames.Count;

                                        //  Now we need to iterate through each result here
                                        //  and create a collection of the information that
                                        //  we are going to place into our grid.
                                        foreach (Frame frame in frames)
                                        {
                                            var iframe = new IFrameCapture();

                                            frame.DisplayPrep();
                                            iframe.Offset = frame.Number;
                                            iframe.Seed = seed;
                                            iframe.Frame = frame;
                                            iframe.Advances = iframe.Offset -
                                                              (generators[listIndex].InitialFrame - minAdvances);

                                            iframe.TimeDate =
                                                searchTime.AddHours(hour).AddMinutes(minute).AddSeconds(second);
                                            iframe.KeyPresses = buttons[buttonCount];
                                            iframe.Timer0 = timer0;

                                            lock (threadLock)
                                            {
                                                iframes.Add(iframe);
                                            }
                                        }

                                        if (frames.Count > 0)
                                        {
                                            refreshQueue = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void dataGridViewEventResults_MouseDown(object sender, MouseEventArgs e)
        {
            DGV_MouseDown(dataGridViewEventResults, e);
        }

        private void dataGridViewEventResults_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DefaultFormatting(dataGridViewEventResults, ushort.Parse(maskedTextTID.Text), ushort.Parse(maskedTextSID.Text), e, "EventPID");

            if (dataGridViewEventResults.Columns[e.ColumnIndex].Name == "EventHP" ||
                dataGridViewEventResults.Columns[e.ColumnIndex].Name == "EventAtk" ||
                dataGridViewEventResults.Columns[e.ColumnIndex].Name == "EventDef" ||
                dataGridViewEventResults.Columns[e.ColumnIndex].Name == "EventSpA" ||
                dataGridViewEventResults.Columns[e.ColumnIndex].Name == "EventSpD" ||
                dataGridViewEventResults.Columns[e.ColumnIndex].Name == "EventSpe")
            {
                var number = (uint)e.Value;
                if (number >= 30)
                {
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }

                if (number == 0)
                {
                    e.CellStyle.ForeColor = Color.Red;
                }
            }

            //  Make all of the junk natures show up in a lighter color
            if (dataGridViewEventResults.Columns[e.ColumnIndex].Name == "EventNature")
            {
                var nature = (string)e.Value;

                if (nature == Functions.NatureStrings(18) ||
                    nature == Functions.NatureStrings(6) ||
                    nature == Functions.NatureStrings(0) ||
                    nature == Functions.NatureStrings(24) ||
                    nature == Functions.NatureStrings(12) ||
                    nature == Functions.NatureStrings(9) ||
                    nature == Functions.NatureStrings(21))
                {
                    e.CellStyle.ForeColor = Color.Gray;
                }
            }
        }

        private void buttonImportWondercard_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "5th Gen Wonder Card |*.pgf",
                Title = "Select a Wonder Card File"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                BinaryReader br = new BinaryReader(File.Open(openFileDialog.FileName, FileMode.Open));
                ReadWondercardInfo(br.ReadBytes(204));
            }
        }

        private void ReadWondercardInfo(byte[] data)
        {
            int species = (BitConverter.ToUInt16(data, 0x1A));
            SpeciesDex.Text = species.ToString();
            //SpeciesDex.Visible = true;

            if (data[0x5C] == 1)    // If egg, it will have the user's TID/SID obviously
            {
                var profile = (Profile)comboBoxProfiles.SelectedItem;
                maskedTextTID.Text = profile.ID.ToString();
                maskedTextSID.Text = profile.SID.ToString();
            }
            else
            {
                maskedTextTID.Text = (BitConverter.ToUInt16(data, 0x0)).ToString();
                maskedTextSID.Text = (BitConverter.ToUInt16(data, 0x2)).ToString();
            }
            
            if (data[0x36] < 3)
            {
                checkBoxAbility.Checked = true;
                glassComboAbility.SelectedIndex = data[0x36];
            }
            else
            {
                glassComboAbility.SelectedItem = null;
                checkBoxAbility.Checked = false;
            }

            checkBoxNatureLock.Checked = data[0x34] != 0xFF;
            if (data[0x34] == 0xFF)
                glassComboBoxNatureList.SelectedItem = null;
            else
                glassComboBoxNatureList.SelectedIndex = data[0x34];

            if (data[0x35] < 2)
            {
                checkBoxGender.Checked = true;
                glassComboGender.SelectedIndex = data[0x35];
            }
            else
            {
                glassComboGender.SelectedItem = null;
                checkBoxGender.Checked = false;
            }

            glassComboGenderRatio.SelectedIndex = Functions.GenderRatio(species);

            glassComboShininess.SelectedIndex = data[0x37];

            checkBoxHP.Checked = data[0x43] != 0xFF;
            maskedTextBoxHP.Text = data[0x43] == 0xFF ? "" : data[0x43].ToString();

            checkBoxAtk.Checked = data[0x44] != 0xFF;
            maskedTextBoxAtk.Text = data[0x44] == 0xFF ? "" : data[0x44].ToString();

            checkBoxDef.Checked = data[0x45] != 0xFF;
            maskedTextBoxDef.Text = data[0x45] == 0xFF ? "" : data[0x45].ToString();

            checkBoxSpe.Checked = data[0x46] != 0xFF;
            maskedTextBoxSpe.Text = data[0x46] == 0xFF ? "" : data[0x46].ToString();

            checkBoxSpA.Checked = data[0x47] != 0xFF;
            maskedTextBoxSpA.Text = data[0x47] == 0xFF ? "" : data[0x47].ToString();

            checkBoxSpD.Checked = data[0x48] != 0xFF;
            maskedTextBoxSpD.Text = data[0x48] == 0xFF ? "" : data[0x48].ToString();

        }

        private void AnyNatureEvent_Click(object sender, EventArgs e)
        {
            glassComboEventNatures.ClearSelection();
        }

        private void checkBoxAbility_CheckedChanged(object sender, EventArgs e)
        {
            glassComboAbility.Enabled = checkBoxAbility.Checked;
        }

        private void checkBoxNatureLock_CheckedChanged(object sender, EventArgs e)
        {
            glassComboBoxNatureList.Enabled = checkBoxNatureLock.Checked;
        }

        private void checkBoxGender_CheckedChanged(object sender, EventArgs e)
        {
            glassComboGender.Enabled = checkBoxGender.Checked;
        }

        private void checkBoxHP_CheckedChanged(object sender, EventArgs e)
        {
            maskedTextBoxHP.Enabled = checkBoxHP.Checked;
        }

        private void checkBoxAtk_CheckedChanged(object sender, EventArgs e)
        {
            maskedTextBoxAtk.Enabled = checkBoxAtk.Checked;
        }

        private void checkBoxDef_CheckedChanged(object sender, EventArgs e)
        {
            maskedTextBoxDef.Enabled = checkBoxDef.Checked;
        }

        private void checkBoxSpA_CheckedChanged(object sender, EventArgs e)
        {
            maskedTextBoxSpA.Enabled = checkBoxSpA.Checked;
        }

        private void checkBoxSpD_CheckedChanged(object sender, EventArgs e)
        {
            maskedTextBoxSpD.Enabled = checkBoxSpD.Checked;
        }

        private void checkBoxSpe_CheckedChanged(object sender, EventArgs e)
        {
            maskedTextBoxSpe.Enabled = checkBoxSpe.Checked;
        }

        #endregion


        #region Eggs

        private void contextMenuStripEggPid_Opening(object sender, CancelEventArgs e)
        {
            DataGridView DGV = tabControl.SelectedTab == tabPageShinyEgg ? dataGridViewShinyResults :
                tabControl.SelectedTab == tabEvent ? dataGridViewEventResults : dataGridViewPickup;
            if (DGV.SelectedRows.Count == 0)
            {
                e.Cancel = true;
            }
        }

        private void buttonShinyGenerate_Click(object sender, EventArgs e)
        {
            //the main function for the shiny Time finder

            //check to make sure the user hasn't filled the text
            //boxes with exception-throwing garbage
            validateShinyInput();

            if (checkBoxShinyDreamWorld.Checked && checkBoxShinyDittoParent.Checked)
            {
                MessageBox.Show("Unable to have Ditto parent and Dream World ability at the same time.");
                checkBoxShinyDittoParent.Focus();
                return;
            }

            //read the user input from the form
            int year = Convert.ToInt32(maskedTextBoxShinyYear.Text);
            if (year < 2000 || year > 2099)
            {
                MessageBox.Show("Year must be between 2000 and 2099.");
                maskedTextBoxShinyYear.Focus();
                return;
            }

            var months = new List<int>();
            for (int month = 1; month <= 12; month++)
            {
                if (comboBoxShinyMonth.CheckBoxItems[month].Checked)
                    months.Add(month);
            }

            if (months.Count == 0)
            {
                comboBoxShinyMonth.Focus();
                return;
            }

            List<uint> nature = null;
            if (comboBoxShinyNature.SelectedIndex != 0)
                nature = new List<uint> { (uint)((Nature)comboBoxShinyNature.SelectedItem).Number };

            if (maskedTextBoxShinyMinFrame.Text == "")
            {
                maskedTextBoxShinyMinFrame.Focus();
                maskedTextBoxShinyMinFrame.SelectAll();
                return;
            }

            if (maskedTextBoxShinyMaxFrame.Text == "")
            {
                maskedTextBoxShinyMaxFrame.Focus();
                maskedTextBoxShinyMaxFrame.SelectAll();
                return;
            }

            var parentA = new uint[6];
            var parentB = new uint[6];

            uint.TryParse(maskedTextBoxShinyHPParentA.Text, out parentA[0]);
            uint.TryParse(maskedTextBoxShinyAtkParentA.Text, out parentA[1]);
            uint.TryParse(maskedTextBoxShinyDefParentA.Text, out parentA[2]);
            uint.TryParse(maskedTextBoxShinySpAParentA.Text, out parentA[3]);
            uint.TryParse(maskedTextBoxShinySpDParentA.Text, out parentA[4]);
            uint.TryParse(maskedTextBoxShinySpeParentA.Text, out parentA[5]);

            uint.TryParse(maskedTextBoxShinyHPParentB.Text, out parentB[0]);
            uint.TryParse(maskedTextBoxShinyAtkParentB.Text, out parentB[1]);
            uint.TryParse(maskedTextBoxShinyDefParentB.Text, out parentB[2]);
            uint.TryParse(maskedTextBoxShinySpAParentB.Text, out parentB[3]);
            uint.TryParse(maskedTextBoxShinySpDParentB.Text, out parentB[4]);
            uint.TryParse(maskedTextBoxShinySpeParentB.Text, out parentB[5]);

            uint minFrame;
            uint maxFrame;

            uint.TryParse(maskedTextBoxShinyMinFrame.Text, out minFrame);
            uint.TryParse(maskedTextBoxShinyMaxFrame.Text, out maxFrame);

            if (minFrame > maxFrame)
            {
                maskedTextBoxShinyMinFrame.Focus();
                maskedTextBoxShinyMinFrame.SelectAll();
                return;
            }

            generator = new FrameGenerator { FrameType = FrameType.Method5Standard, InitialFrame = 8, MaxResults = 1 };

            shinygenerator = new FrameGenerator
            {
                FrameType =
                        !checkBoxIntlParents.Checked ? FrameType.BWBred : FrameType.BWBredInternational,
                ParentA = parentA,
                ParentB = parentB,
                SynchNature = ((Nature)comboBoxShinyEverstoneNature.SelectedItem).Number,
                InitialFrame = minFrame,
                MaxResults = maxFrame - minFrame + 1,
                DittoUsed = checkBoxShinyDittoParent.Checked,
                MaleOnlySpecies = cbNidoBeat.Checked,
                RerollCount = ((Profile)comboBoxProfiles.SelectedItem).ShinyCharm ? 3 : 1,
                MemoryLinkUsed = ((Profile)comboBoxProfiles.SelectedItem).MemoryLink,
                MaxLuckyPowerLVL = ((Profile)comboBoxProfiles.SelectedItem).LuckyPowerLVL,
            };


            frameCompare = new FrameCompare(
                null,
                null,
                -1,
                false,
                false,
                0,
                false,
                null,
                new NoGenderFilter());

            subFrameCompare = new FrameCompare(
                ivFiltersEggs.IVFilter,
                nature,
                (int)((ComboBoxItem)comboBoxShinyAbility.SelectedItem).Reference,
                checkBoxShinyShinyOnly.Checked,
                false,
                0,
                checkBoxShinyDreamWorld.Checked,
                null,
                (GenderFilter)(comboBoxShinyGender.SelectedItem));

            // Here we check the parent IVs
            // To make sure they even have a chance of producing the desired spread

            int parentPassCount = 0;
            for (int i = 0; i < 6; i++)
            {
                if (subFrameCompare.CompareIV(i, parentA[i]) ||
                    subFrameCompare.CompareIV(i, parentB[i]))
                {
                    parentPassCount++;
                }
            }

            if (parentPassCount < 3)
            {
                MessageBox.Show("The parent IVs you have listed cannot produce your desired search results.");
                return;
            }

            iframesEgg = new List<IFrameCapture>();
            listBindingEgg = new BindingSource { DataSource = iframesEgg };
            dataGridViewShinyResults.DataSource = listBindingEgg;

            var profile = (Profile)comboBoxProfiles.SelectedItem;

            List<List<ButtonComboType>> keypresses = profile.GetKeypresses();

            bool fastSearch = FastEggFilters() && eggSeeds != null && eggSeeds.Count > 0; // && FastEggFrames()

            int dayTotal = months.Sum(month => DateTime.DaysInMonth(year, month));

            progressTotal =
                (ulong)
                (dayTotal * 86400 * keypresses.Count * (profile.Timer0Max - profile.Timer0Min + 1) * (maxFrame - minFrame + 1));

            float interval = ((float)24 / cpus + (float)0.05);

            var hourMin = new int[cpus];
            var hourMax = new int[cpus];

            jobs = new Thread[cpus];
            generators = new FrameGenerator[cpus];
            shinygenerators = new FrameGenerator[cpus];
            waitHandle = new EventWaitHandle(true, EventResetMode.ManualReset);

            for (int i = 0; i < jobs.Length; i++)
            {
                hourMin[i] = (int)(interval * i);
                hourMax[i] = (int)(interval * (i + 1) - 1);

                if (hourMax[i] > 23)
                {
                    hourMax[i] = 23;
                }
            }

            for (int i = 0; i < jobs.Length; i++)
            {
                generators[i] = generator.Clone();

                if (shinygenerator != null)
                {
                    shinygenerators[i] = shinygenerator.Clone();
                }

                //copy to prevent issues with i being incremented before the call
                int i1 = i;
                jobs[i] =
                    new Thread(
                        () =>
                        GenerateShinyJob((uint)year, months, hourMin[i1], hourMax[i1], profile, keypresses, fastSearch,
                                         i1));
                jobs[i].Start();
                // for some reason not making the thread sleep causes issues with updating dayMin\Max
                Thread.Sleep(200);
            }

            var progressJob =
                new Thread(
                    () => ManageProgress(listBindingEgg, dataGridViewShinyResults, shinygenerator.FrameType, 2000));
            progressJob.Start();
            progressJob.Priority = ThreadPriority.Lowest;

            buttonCapGenerate.Enabled = false;
            buttonShinyGenerate.Enabled = false;

            dataGridViewShinyResults.Focus();
        }

        public void GenerateShinyJob(uint year, List<int> months, int hourMin, int hourMax, Profile profile,
                                     List<List<ButtonComboType>> keypressList, bool fastSearch, int listIndex)
        {
            var rngIVs = new uint[6];

            List<ButtonComboType>[] buttons = keypressList.ToArray();
            var buttonMashValue = new uint[keypressList.Count];

            for (int i = 0; i < buttons.Length; i++)
            {
                buttonMashValue[i] = Functions.buttonMashed(buttons[i]);
            }

            uint minAdvances = shinygenerators[listIndex].InitialFrame;

            foreach (int month in months)
            {
                int dayMax = DateTime.DaysInMonth((int)year, month);
                for (int buttonCount = 0; buttonCount < buttons.Length; buttonCount++)
                {
                    for (int day = 1; day <= dayMax; day++)
                    {
                        waitHandle.WaitOne();
                        for (uint Timer0 = profile.Timer0Min; Timer0 <= profile.Timer0Max; Timer0++)
                        {
                            for (int hour = hourMin; hour <= hourMax; hour++)
                            {
                                for (int minute = 0; minute <= 59; minute++)
                                {
                                    for (int second = 0; second <= 59; second++)
                                    {
                                        var searchTime = new DateTime((int)year, month, day, hour, minute, second);

                                        ulong seed = Functions.EncryptSeed(searchTime, profile.MAC_Address,
                                                                           profile.Version, profile.Language,
                                                                           profile.DSType,
                                                                           profile.SoftReset,
                                                                           profile.VCount, Timer0, profile.GxStat,
                                                                           profile.VFrame,
                                                                           buttonMashValue[buttonCount]);

                                        generators[listIndex].InitialSeed = seed >> 32;

                                        List<Frame> frames = generators[listIndex].Generate(frameCompare, profile.ID,
                                                                                            profile.SID);
                                        if (frames.Count > 0)
                                        {
                                            if (!frameCompare.CompareEggIVs(frames[0]))
                                            {
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            continue;
                                        }

                                        rngIVs[0] = frames[0].Hp;
                                        rngIVs[1] = frames[0].Atk;
                                        rngIVs[2] = frames[0].Def;
                                        rngIVs[3] = frames[0].Spa;
                                        rngIVs[4] = frames[0].Spd;
                                        rngIVs[5] = frames[0].Spe;

                                        shinygenerators[listIndex].RNGIVs = rngIVs;
                                        shinygenerators[listIndex].InitialSeed = seed;
                                        shinygenerators[listIndex].InitialFrame =
                                            Functions.initialPIDRNG(seed, profile) +
                                            minAdvances;

                                        frames = shinygenerators[listIndex].Generate(subFrameCompare, profile.ID,
                                                                                     profile.SID);

                                        if (frames.Count > 0)
                                        {
                                            foreach (Frame frame in frames)
                                            {
                                                frame.DisplayPrep();
                                                var iframeEgg = new IFrameCapture
                                                {
                                                    Frame = frame,
                                                    Seed = seed,
                                                    Offset = frame.Number,
                                                    TimeDate = searchTime,
                                                    KeyPresses = buttons[buttonCount],
                                                    Timer0 = Timer0
                                                };

                                                lock (threadLock)
                                                {
                                                    iframesEgg.Add(iframeEgg);
                                                }
                                            }
                                            refreshQueue = true;
                                            progressFound = (ulong)iframesEgg.Count;
                                        }
                                        progressSearched += shinygenerators[listIndex].MaxResults;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void buttonShinyClearNature_Click(object sender, EventArgs e)
        {
            comboBoxShinyNature.SelectedIndex = 0;
        }

        //--------------------------------------------------------------------------------------------------

        private void validateShinyInput()
        //get rid of any junk characters in the shiny text boxes
        //and restrict the inputs to their appropriate levels
        //prevents the program confusing the user by throwing nasty exceptions
        {
            //check the date is valid
            maskedTextBoxShinyYear.Text = Functions.NumericFilter(maskedTextBoxShinyYear.Text);
            //int year = Convert.ToInt32(maskedTextBoxShinyYear.Text);
        }

        //--------------------------------------------------------------------------------------------------

        private void dataGridViewShinyResults_MouseDown(object sender, MouseEventArgs e)
        {
            DGV_MouseDown(dataGridViewShinyResults, e);
        }

        private void outputResultsToTXTToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //  Going to need to present the user with a File Dialog and 
            //  then interate through the Grid, outputting columns that
            //  are visible.

            saveFileDialogTxt.AddExtension = true;
            saveFileDialogTxt.Title = "Save Output to TXT";
            saveFileDialogTxt.Filter = "TXT Files|*.txt";
            saveFileDialogTxt.FileName = "rngreporter.txt";
            if (saveFileDialogTxt.ShowDialog() == DialogResult.OK)
            {
                //  Get the name of the file and then go ahead 
                //  and create and save the thing to the hard
                //  drive.   

                // Throws an exception if the wrong object type
                // And goes to the other one

                var writer = new TXTWriter(dataGridViewShinyResults);
                try
                {
                    var frames = (List<IFrameEggPID>)listBindingEgg.DataSource;

                    if (frames.Count > 0)
                    {
                        //  Need to know what sort of display we are doing here.  The
                        //  easiset thing to do is to take the value of the dropdown.
                        //IFrameEggPIDTXTWriter writer = new IFrameEggPIDTXTWriter();

                        writer.Generate(saveFileDialogTxt.FileName, frames);
                    }
                }
                catch
                {
                    var frames = (List<IFrameCapture>)listBindingEgg.DataSource;

                    if (frames.Count > 0)
                    {
                        //  Need to know what sort of display we are doing here.  The
                        //  easiset thing to do is to take the value of the dropdown.
                        //IFrameEggPIDTXTWriter writer = new IFrameEggPIDTXTWriter();

                        writer.Generate(saveFileDialogTxt.FileName, frames);
                    }
                }
            }
        }

        private void dataGridViewShinyResults_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            //  Make all of the junk natures show up in a lighter color
            if (dataGridViewShinyResults.Columns[e.ColumnIndex].Name == "ShinyNature")
            {
                var nature = (string)e.Value;

                if (nature == Functions.NatureStrings(18) ||
                    nature == Functions.NatureStrings(6) ||
                    nature == Functions.NatureStrings(0) ||
                    nature == Functions.NatureStrings(24) ||
                    nature == Functions.NatureStrings(12) ||
                    nature == Functions.NatureStrings(9) ||
                    nature == Functions.NatureStrings(21))
                {
                    e.CellStyle.ForeColor = Color.Gray;
                }
            }

            if (dataGridViewShinyResults.Columns[e.ColumnIndex].Name == "ShinyHP" ||
                dataGridViewShinyResults.Columns[e.ColumnIndex].Name == "ShinyAtk" ||
                dataGridViewShinyResults.Columns[e.ColumnIndex].Name == "ShinyDef" ||
                dataGridViewShinyResults.Columns[e.ColumnIndex].Name == "ShinySpA" ||
                dataGridViewShinyResults.Columns[e.ColumnIndex].Name == "ShinySpD" ||
                dataGridViewShinyResults.Columns[e.ColumnIndex].Name == "ShinySpe")
            {
                if ((string)e.Value == "30" || (string)e.Value == "31")
                {
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }

                if ((string)e.Value == "0")
                {
                    e.CellStyle.ForeColor = Color.Red;
                }

                if ((string)e.Value == "Ma" || (string)e.Value == "Fe")
                {
                    e.CellStyle.ForeColor = Color.Blue;
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxShowInheritance.Checked)
            {
                ShinyHP.DataPropertyName = "DisplayHp";
                ShinyAtk.DataPropertyName = "DisplayAtk";
                ShinyDef.DataPropertyName = "DisplayDef";
                ShinySpA.DataPropertyName = "DisplaySpa";
                ShinySpD.DataPropertyName = "DisplaySpd";
                ShinySpe.DataPropertyName = "DisplaySpe";
            }
            else
            {
                ShinyHP.DataPropertyName = "DisplayHpAlt";
                ShinyAtk.DataPropertyName = "DisplayAtkAlt";
                ShinyDef.DataPropertyName = "DisplayDefAlt";
                ShinySpA.DataPropertyName = "DisplaySpaAlt";
                ShinySpD.DataPropertyName = "DisplaySpdAlt";
                ShinySpe.DataPropertyName = "DisplaySpeAlt";
            }
        }

        #endregion


        #region Various (Misc)

        private void DefaultFormatting(DataGridView DGV, ushort id, ushort sid, DataGridViewCellFormattingEventArgs e, string PIDColumn)
        {
            uint tid = (uint)((id & 0xffff) | ((sid & 0xffff) << 16));
            uint a = Convert.ToUInt32(DGV.Rows[e.RowIndex].Cells[PIDColumn].Value) ^ tid;
            uint b = a & 0xffff;
            uint c = (a >> 16);
            uint d = b ^ c;
            if (d == 0)
                DGV.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Aqua;
            else if (d < 8)
                DGV.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightCyan;
        }

        public void DGV_MouseDown(DataGridView DGV, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                DataGridView.HitTestInfo Hti = DGV.HitTest(e.X, e.Y);

                if (Hti.Type == DataGridViewHitTestType.Cell)
                {
                    if (!((DGV.Rows[Hti.RowIndex])).Selected)
                    {
                        DGV.ClearSelection();

                        (DGV.Rows[Hti.RowIndex]).Selected = true;
                    }
                }
            }
        }

        private int seedColumnLong(bool isLong, DataGridViewTextBoxColumn column)
        {
            if (!longSeed && isLong)
            {
                longSeed = true;
                return column.Width * 2;
            }
            if (longSeed && !isLong)
            {
                longSeed = false;
                return column.Width / 2;
            }
            return column.Width;
        }

        #endregion


        #region Various (UI events)


        #endregion


        #region Performance

        private void ManageProgress(BindingSource bindingSource, DoubleBufferedDataGridView grid, FrameType frameType,
                                    int sleepTimer)
        {
            var progress = new Progress();
            progress.SetupAndShow(this, 0, 0, false, true, waitHandle);

            progressSearched = 0;
            progressFound = 0;

            UpdateGridDelegate gridUpdater = UpdateGrid;
            var updateParams = new object[] { bindingSource };
            ResortGridDelegate gridSorter = ResortGrid;
            var sortParams = new object[] { bindingSource, grid, frameType };
            ThreadDelegate enableGenerateButton = EnableCapGenerate;

            try
            {
                bool alive = true;
                while (alive)
                {
                    progress.ShowProgress(progressSearched / (float)progressTotal, progressSearched, progressFound);
                    if (refreshQueue)
                    {
                        Invoke(gridUpdater, updateParams);
                        refreshQueue = false;
                    }
                    if (jobs != null)
                    {
                        foreach (Thread job in jobs)
                        {
                            if (job != null && job.IsAlive)
                            {
                                alive = true;
                                break;
                            }
                            alive = false;
                        }
                    }
                    if (sleepTimer > 0)
                        Thread.Sleep(sleepTimer);
                }
            }
            catch (ObjectDisposedException)
            {
                // This keeps the program from crashing when the Time Finder progress box
                // is closed from the Windows taskbar.
            }
            catch (Exception exception)
            {
                if (exception.Message != "Operation Cancelled")
                {
                    throw;
                }
            }
            finally
            {
                progress.Finish();

                if (jobs != null)
                {
                    foreach (Thread t in jobs)
                    {
                        if (t != null)
                        {
                            t.Abort();
                        }
                    }
                }

                Invoke(enableGenerateButton);
                Invoke(gridSorter, sortParams);
            }
        }

        // Methods we'll use when we roll up the above functions

        private void UpdateGrid(BindingSource bindingSource)
        {
            bindingSource.ResetBindings(false);
        }

        private void ResortGrid(BindingSource bindingSource, DoubleBufferedDataGridView dataGrid, FrameType frameType)
        {
            switch (frameType)
            {
                case FrameType.Method5Standard:
                case FrameType.Method5Natures:
                case FrameType.Wondercard5thGen:
                case FrameType.Gen5Pickup:
                    var iframeCaptureComparer = new IFrameCaptureComparer { CompareType = "TimeDate" };
                    ((List<IFrameCapture>)bindingSource.DataSource).Sort(iframeCaptureComparer);
                    CapDateTime.HeaderCell.SortGlyphDirection = SortOrder.Ascending;
                    break;
                case FrameType.BWBred:
                case FrameType.BWBredInternational:
                    iframeCaptureComparer = new IFrameCaptureComparer { CompareType = "TimeDate" };
                    ((List<IFrameCapture>)bindingSource.DataSource).Sort(iframeCaptureComparer);
                    ColumnEggDate.HeaderCell.SortGlyphDirection = SortOrder.Ascending;
                    break;
            }

            dataGrid.DataSource = bindingSource;
            bindingSource.ResetBindings(false);
        }


        #endregion


        private void btnHHGenerate_Click(object sender, EventArgs e)
        {
            var searchParams = new HiddenGrottoSearchParams
                {
                    GenerateButton = btnHHGenerate,
                    DataGridView = dgvHiddenGrottos,
                    MaxAdvances = txtHHAdvances,
                    Year = txtHHYear,
                    OpenHollows = txtHHOpenHollows,
                    Months = cbHHMonth,
                    Profile = (Profile) comboBoxProfiles.SelectedItem,
                    Slots = cbHHSlot,
                    SubSlots = cbHHSubSlot,
                    Hollows = cbHHHollowNumber,
                    Gender = cbHHGender,
                    GenderRatio = cbHHGenderRatio
                };
            Searcher searcher = new HiddenGrottoSearcher(searchParams, threadLock, this);
            if (!searcher.ParseInput()) return;
            searcher.RunSearch();
        }


        private void copySeedToClipboardToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (dataGridViewCapValues.SelectedRows[0] != null)
            {
                var frame = (IFrameCapture)dataGridViewCapValues.SelectedRows[0].DataBoundItem;
                Clipboard.SetText(frame.Seed.ToString("X8"));
            }
        }

        private void copyCgearToClipboard_Click(object sender, EventArgs e)
        {
            if (dataGridViewCapValues.SelectedRows[0] != null)
            {
                var frame = (IFrameCapture)dataGridViewCapValues.SelectedRows[0].DataBoundItem;
                Clipboard.SetText(frame.CSeed.ToString("X8"));
            }
        }

        private void copySeedToClipboardToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            DataGridView DGV = tabControl.SelectedTab == tabPageShinyEgg ? dataGridViewShinyResults :
                tabControl.SelectedTab == tabEvent ? dataGridViewEventResults : dataGridViewPickup;

            if (DGV.SelectedRows[0] != null)
            {
                try
                {
                    var frame = (IFrameEggPID)DGV.SelectedRows[0].DataBoundItem;
                    Clipboard.SetText(frame.Seed.ToString("X8"));
                }
                catch
                {
                    var frame = (IFrameCapture)DGV.SelectedRows[0].DataBoundItem;
                    Clipboard.SetText(frame.Seed.ToString("X16"));
                }
            }
        }

        private void calibrateDelayIVs_Click(object sender, EventArgs e)
        {
            if (checker == null)
                checker = new DexIVCheck();
            checker.Show();
            checker.Focus();
            if (dataGridViewCapValues.SelectedRows[0] != null)
            {
                var frame = (IFrameCapture)dataGridViewCapValues.SelectedRows[0].DataBoundItem;
                checker.SetValues(frame.CSeed.ToString("X8"), frame.Delay, (int)frame.Offset);
            }
        }

        private void comboBoxEncounterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((ComboBoxItem)comboBoxEncounterType.SelectedItem).Reference.Equals(EncounterType.JellicentHA))
            {
                comboBoxCapGenderRatio.SelectedIndex = 1;   // 50% gender ratio
                comboBoxCapGenderRatio.Enabled = false;
                if (((Profile)comboBoxProfiles.SelectedItem).VersionStr.Equals("Black2"))
                    comboBoxCapGender.SelectedIndex = 1;
                else if (((Profile)comboBoxProfiles.SelectedItem).VersionStr.Equals("White2"))
                    comboBoxCapGender.SelectedIndex = 2;
            }
            else
            {
                comboBoxCapGenderRatio.Enabled = true;
            }

            if (((ComboBoxItem)comboBoxEncounterType.SelectedItem).Reference.Equals(EncounterType.Roamer))
            {
                comboBoxCapGenderRatio.SelectedIndex = 5;
            }

            IVFilters_Changed(sender, e);

            label54.Visible = comboCapEncounterSlot.Visible = buttonAnySlot.Visible = comboBoxEncounterType.SelectedIndex <= 9;

            labelCapMinMaxLevel.Visible = numericLevelMin.Visible = numericLevelMax.Visible = LevelLabel.Visible = numericLevel.Visible = LevelConditions();
            
            checkBoxTriggerBattle.Visible = RatioConditions();

        }



        // Sorts the grid 
        // Can't use SortCompare method because this grid is data-bound
        private void dataGridViewCapValues_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dataGridViewCapValues.DataSource != null && iframes != null && listBindingCap != null)
            {
                DataGridViewColumn selectedColumn = dataGridViewCapValues.Columns[e.ColumnIndex];

                var iframeCaptureComparer = new IFrameCaptureComparer
                    {CompareType = selectedColumn.DataPropertyName};

                if (selectedColumn.HeaderCell.SortGlyphDirection == SortOrder.Ascending)
                    iframeCaptureComparer.sortOrder = SortOrder.Descending;

                iframes.Sort(iframeCaptureComparer);

                listBindingCap.ResetBindings(false);
                selectedColumn.HeaderCell.SortGlyphDirection = iframeCaptureComparer.sortOrder;
            }
        }

        private void buttonAnySlot_Click(object sender, EventArgs e)
        {
            comboCapEncounterSlot.ClearSelection();
        }

        private void dataGridViewCapValues_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D && e.Control)
            {
                DataObject clipboardContent = dataGridViewCapValues.GetClipboardContent();
                if (clipboardContent != null)
                {
                    var test = (string) clipboardContent.GetData(DataFormats.UnicodeText);
                    test = test.Replace('\t', ' ');
                    Clipboard.SetText(test);
                }
            }
        }

        

        private bool FastEggFilters()
        {
            //quick fix or it will attempt to filter like it's a roamer if roamer is set on capture tab
            //lazy workaround, cleanup if feature is added
            int saved = comboBoxEncounterType.SelectedIndex;
            comboBoxEncounterType.SelectedIndex = 0;
            IVFilter filter = ivFiltersEggs.IVFilter;
            bool result = FastFilters(filter);
            comboBoxEncounterType.SelectedIndex = saved;
            return result;
        }

        private void IVFilters_Changed(object sender, EventArgs e)
        {
            if (FastCapFilters() && FastCapFrames())
                label9.Text = "IV filters are set for fast searching.";
            else if (FastCapFilters())
            {

                if (((ComboBoxItem) comboBoxMethod.SelectedItem).Reference.Equals(FrameType.Method5CGear))
                    label9.Text = "IV filters are set for Entralink fast searching, but\r\n" +
                                  "Min and Max Frames need to be between\r\n" +
                                  "20 and 25.  (Setting both to 20 is recommended.)";
                else
                {
                    label9.Text = "IV filters are set for fast searching, but\r\n" +
                                  "Min and Max Frames need to be between\r\n" +
                                  "0 and 5.  (Setting both to 0 is recommended.)";
                    if (((Profile) comboBoxProfiles.SelectedItem).IsBW2())
                    {
                        label9.Text += "\r\nOr 25 and 30 for Entralink abuse";
                    }
                }
            }
            else
                label9.Text = "IV filters are not set to allow fast searching.\r\nTry searching for a common spread" +
                              "\r\nsuch as flawless, or a Trick Room spread.";
        }

        private GenderFilter constructGenderFilter()
        {
            var criteria = (GenderCriteria) comboBoxCapGender.SelectedIndex;
            uint ratio = 0;

            switch (comboBoxCapGenderRatio.SelectedIndex)
            {
                case 0:
                    ratio = 255;
                    break;
                case 1:
                    ratio = 127;
                    break;
                case 2:
                    ratio = 191;
                    break;
                case 3:
                    ratio = 63;
                    break;
                case 4:
                    ratio = 31;
                    break;
                case 5:
                    ratio = 0;
                    break;
            }

            var filter = new GenderFilter("", ratio, criteria);

            return filter;
        }

        private void comboBoxCapGenderRatio_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxCapGenderRatio.SelectedIndex == 0 || comboBoxCapGenderRatio.SelectedIndex == 5)
            {
                comboBoxCapGender.Enabled = false;
                comboBoxCapGender.SelectedIndex = 0;
            }
            else
                comboBoxCapGender.Enabled = true;
        }

        private void comboBoxCapGenderRatio_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var lookup = new GenderRatioLookup();
                if (lookup.ShowDialog() == DialogResult.OK)
                {
                    switch (lookup.GenderRatio)
                    {
                        case 127:
                            comboBoxCapGenderRatio.SelectedIndex = 1;
                            break;
                        case 191:
                            comboBoxCapGenderRatio.SelectedIndex = 2;
                            break;
                        case 63:
                            comboBoxCapGenderRatio.SelectedIndex = 3;
                            break;
                        case 31:
                            comboBoxCapGenderRatio.SelectedIndex = 4;
                            break;
                        default:
                            comboBoxCapGenderRatio.SelectedIndex = 5;
                            break;
                    }
                }
            }
        }

        private void DisplayGenderColumns()
        {
            switch (comboBoxCapGenderRatio.SelectedIndex)
            {
                case 0:
                    f25.Visible = true;
                    f50.Visible = true;
                    f75.Visible = true;
                    f125.Visible = true;
                    break;
                case 1:
                    f25.Visible = false;
                    f50.Visible = true;
                    f75.Visible = false;
                    f125.Visible = false;
                    break;
                case 2:
                    f25.Visible = false;
                    f50.Visible = false;
                    f75.Visible = true;
                    f125.Visible = false;
                    break;
                case 3:
                    f25.Visible = true;
                    f50.Visible = false;
                    f75.Visible = false;
                    f125.Visible = false;
                    break;
                case 4:
                    f25.Visible = false;
                    f50.Visible = false;
                    f75.Visible = false;
                    f125.Visible = true;
                    break;
                case 5:
                    f25.Visible = false;
                    f50.Visible = false;
                    f75.Visible = false;
                    f125.Visible = false;
                    break;
            }
        }

        

        private void generateAdjacentSeedsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewShinyResults.SelectedRows[0] != null)
                {
                    var iframe = (IFrameCapture)dataGridViewShinyResults.SelectedRows[0].DataBoundItem;

                    const uint advances = 0;
                    int profile = comboBoxProfiles.SelectedIndex;
                    var adjacents = new Adjacents(iframe.TimeDate,
                                                  profile, iframe.KeyPresses,
                                                  FrameType.BWBred, iframe.Frame.EncounterType,
                                                  advances);
                    adjacents.Show();
                }
            }
            catch
            {
                MessageBox.Show("Currently not supported for this method.");
            }
        }

        private void generateEntralinkNatureSeedsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridViewCapValues.SelectedRows[0] != null)
            {
                var iframe = (IFrameCapture) dataGridViewCapValues.SelectedRows[0].DataBoundItem;

                var seedEntree = new EntralinkSeedSearch((uint) iframe.Seed, iframe.Offset,
                                                         comboBoxProfiles.SelectedIndex);
                seedEntree.Show();
            }
        }

        private void FocusControl(object sender, MouseEventArgs e)
        {
            ((Control) sender).Focus();
        }

        private void buttonEditProfile_Click(object sender, EventArgs e)
        {
            var editor = new ProfileEditor {Profile = (Profile) comboBoxProfiles.SelectedItem};
            if (editor.ShowDialog() != DialogResult.OK) return;
            Profiles.List[comboBoxProfiles.SelectedIndex] = editor.Profile;

            profilesSource.DataSource = Profiles.List;
            profilesSource.ResetBindings(false);
            labelProfileInformation.Text = ((Profile) comboBoxProfiles.SelectedItem).ProfileInformation();
        }

        private void comboBoxProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            labelProfileInformation.Text = Profiles.List.Count > 0
                                               ? ((Profile) comboBoxProfiles.SelectedItem).ProfileInformation()
                                               : "No profiles found.";
            Settings.Default.ID = ((Profile) comboBoxProfiles.SelectedItem).ID.ToString();
            Settings.Default.SID = ((Profile) comboBoxProfiles.SelectedItem).SID.ToString();
            Settings.Default.BW2 = ((Profile)comboBoxProfiles.SelectedItem).IsBW2();
            Settings.Default.ShinyCharm = ((Profile)comboBoxProfiles.SelectedItem).ShinyCharm;
            Settings.Default.MemoryLink = ((Profile)comboBoxProfiles.SelectedItem).MemoryLink;
            Settings.Default.Save();
            if (txtCallerID != null)
            {
                txtCallerID.Text = Settings.Default.ID;
                txtCallerSID.Text = Settings.Default.SID;

                cbCallerBW2.Checked = Settings.Default.BW2;
                cbCallerMemoryLink.Checked = Settings.Default.MemoryLink;
                cbCallerShinyCharm.Checked = Settings.Default.ShinyCharm;
            }

            cbActiveRoamer.Enabled = !((Profile)comboBoxProfiles.SelectedItem).IsBW2();
        }

        private void buttonLoadEggSeeds_Click(object sender, EventArgs e)
        {
            new Thread(LoadSeeds).Start();
            MessageBox.Show("Loading the seeds file. Please be patient and wait for it to finish.");
        }

        private void LoadSeeds()
        {
            if (EggSeedSearcher.LoadSeeds("eggseeds.dat", out eggSeeds))
            {
                MessageBox.Show("Seeds successfully loaded.");
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (dgvHiddenGrottos.SelectedRows[0] != null)
            {
                // this should always be a size 16 string
                // catch is only to prevent crashes in stupid cases
                try
                {
                    var frame = (Hollow) dgvHiddenGrottos.SelectedRows[0].DataBoundItem;
                    Clipboard.SetText(frame.Seed.ToString("X16"));
                }
                catch
                {
                }
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (dgvHiddenGrottos.SelectedRows[0] != null)
            {
                var iframe = (Hollow) dgvHiddenGrottos.SelectedRows[0].DataBoundItem;

                const uint advances = 0;
                int profile = comboBoxProfiles.SelectedIndex;
                var adjacents = new Adjacents(iframe.DateTime,
                                              profile, iframe.Keypresses,
                                              FrameType.BWBred, EncounterType.HiddenGrotto,
                                              advances);
                adjacents.Show();
            }
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            //  Going to need to present the user with a File Dialog and 
            //  then interate through the Grid, outputting columns that
            //  are visible.

            saveFileDialogTxt.AddExtension = true;
            saveFileDialogTxt.Title = "Save Output to TXT";
            saveFileDialogTxt.Filter = "TXT Files|*.txt";
            saveFileDialogTxt.FileName = "rngreporter.txt";
            if (saveFileDialogTxt.ShowDialog() == DialogResult.OK)
            {
                //  Get the name of the file and then go ahead 
                //  and create and save the thing to the hard
                //  drive.   

                // Throws an exception if the wrong object type
                // And goes to the other one

                var writer = new TXTWriter(dgvHiddenGrottos);
                try
                {
                    var frames = (List<Hollow>) ((BindingSource) dgvHiddenGrottos.DataSource).DataSource;

                    if (frames.Count > 0)
                    {
                        //  Need to know what sort of display we are doing here.  The
                        //  easiset thing to do is to take the value of the dropdown.
                        //IFrameEggPIDTXTWriter writer = new IFrameEggPIDTXTWriter();

                        writer.Generate(saveFileDialogTxt.FileName, frames);
                    }
                }
                catch
                {
                    var frames = (List<Hollow>) ((BindingSource) dgvHiddenGrottos.DataSource).DataSource;

                    if (frames.Count > 0)
                    {
                        //  Need to know what sort of display we are doing here.  The
                        //  easiset thing to do is to take the value of the dropdown.
                        //IFrameEggPIDTXTWriter writer = new IFrameEggPIDTXTWriter();

                        writer.Generate(saveFileDialogTxt.FileName, frames);
                    }
                }
            }
        }

        private void btnDRGenerate_Click(object sender, EventArgs e)
        {
            var searchParams = new DreamRadarSearchParams
                {
                    Year = txtDRYear,
                    Months = cbDRMonth,
                    MinFrame = txtDRMinFrame,
                    MaxFrame = txtDRMaxFrame,
                    IVFilters = ivDR,
                    Natures = cbDRNature,
                    Gender = cbDRGender,
                    GenderRatio = cbDRRatio,
                    IsShiny = cbDRShiny,
                    Shininess = cbDRShinyness,
                    DataGridView = gvDreamRadar,
                    GenerateButton = btnDRGenerate,
                    Profile = (Profile)comboBoxProfiles.SelectedItem
                };
            Searcher searcher = new DreamRadarSearcher(searchParams, new object(), this);
            if (!searcher.ParseInput()) return;
            searcher.RunSearch();
        }

        private void btnDRAnyNature_Click(object sender, EventArgs e)
        {
            cbDRNature.ClearSelection();
        }

        #region Nested type: ResortGridDelegate

        private delegate void ResortGridDelegate(
            BindingSource bindingSource, DoubleBufferedDataGridView dataGrid, FrameType frameType);

        #endregion

        #region Nested type: ThreadDelegate

        private delegate void ThreadDelegate();

        #endregion

        #region Nested type: UpdateGridDelegate

        private delegate void UpdateGridDelegate(BindingSource bindingSource);

        bool cond = false;
        private bool LevelConditions()
        {
            Invoke(new Action(() => { 
                cond = comboBoxMethod.SelectedIndex < 2 && 
                comboBoxEncounterType.SelectedIndex >= 2 && comboBoxEncounterType.SelectedIndex <= 7 && 
                comboBoxEncounterType.SelectedIndex != 5; }));
            return cond;
        }

        private bool RatioConditions()
        {
            Invoke(new Action(() => { cond = comboBoxMethod.SelectedIndex < 2 && comboBoxEncounterType.SelectedIndex <= 3; }));
            return cond;
        }

        private bool ShinyOnly() => checkBoxShinyOnly.Visible && checkBoxShinyOnly.Checked;

        public Profile getProfile() => (Profile)comboBoxProfiles.SelectedItem;


        #endregion

        private void checkBoxDelay_CheckedChanged(object sender, EventArgs e)
        {
            numCalibrateDelay.Enabled = checkBoxDelay.Checked;
        }

        public struct CgearSettings
        {
            public uint calibDelay;
            public uint delayMin;
            public uint delayMax;
            public uint minIVFrame;
            public uint maxIVFrame;
            public bool fixedRTC;
        }


        #region Pickup


        private void checkBoxParty1_CheckedChanged(object sender, EventArgs e)
        {
            SlotLevel1.Enabled = TargetItem1.Enabled = checkBoxParty1.Checked;
        }

        private void checkBoxParty2_CheckedChanged(object sender, EventArgs e)
        {
            SlotLevel2.Enabled = TargetItem2.Enabled = checkBoxParty2.Checked;
        }

        private void checkBoxParty3_CheckedChanged(object sender, EventArgs e)
        {
            SlotLevel3.Enabled = TargetItem3.Enabled = checkBoxParty3.Checked;
        }

        private void checkBoxParty4_CheckedChanged(object sender, EventArgs e)
        {
            SlotLevel4.Enabled = TargetItem4.Enabled = checkBoxParty4.Checked;
        }

        private void checkBoxParty5_CheckedChanged(object sender, EventArgs e)
        {
            SlotLevel5.Enabled = TargetItem5.Enabled = checkBoxParty5.Checked;
        }

        private void checkBoxParty6_CheckedChanged(object sender, EventArgs e)
        {
            SlotLevel6.Enabled = TargetItem6.Enabled = checkBoxParty6.Checked;
        }


        private void ClearList1_Click(object sender, EventArgs e)
        {
            ClearItems(TargetItem1);
        }

        private void ClearList2_Click(object sender, EventArgs e)
        {
            ClearItems(TargetItem2);
        }

        private void ClearList3_Click(object sender, EventArgs e)
        {
            ClearItems(TargetItem3);
        }

        private void ClearList4_Click(object sender, EventArgs e)
        {
            ClearItems(TargetItem4);
        }

        private void ClearList5_Click(object sender, EventArgs e)
        {
            ClearItems(TargetItem5);
        }

        private void ClearList6_Click(object sender, EventArgs e)
        {
            ClearItems(TargetItem6);
        }

        private void ClearItems(CheckBoxComboBox comboBox)
        {
            for (int i = 1; i < comboBox.Items.Count; i++)
                comboBox.CheckBoxItems[i].Checked = false;
        }


        private void SlotLevel1_SelectedIndexChanged(object sender, EventArgs e)
        {
            setItems(SlotLevel1.SelectedIndex, TargetItem1);
        }

        private void SlotLevel2_SelectedIndexChanged(object sender, EventArgs e)
        {
            setItems(SlotLevel2.SelectedIndex, TargetItem2);
        }

        private void SlotLevel3_SelectedIndexChanged(object sender, EventArgs e)
        {
            setItems(SlotLevel3.SelectedIndex, TargetItem3);
        }

        private void SlotLevel4_SelectedIndexChanged(object sender, EventArgs e)
        {
            setItems(SlotLevel4.SelectedIndex, TargetItem4);
        }

        private void SlotLevel5_SelectedIndexChanged(object sender, EventArgs e)
        {
            setItems(SlotLevel5.SelectedIndex, TargetItem5);
        }

        private void SlotLevel6_SelectedIndexChanged(object sender, EventArgs e)
        {
            setItems(SlotLevel6.SelectedIndex, TargetItem6);
        }

        private void setItems(int LevelRange, CheckBoxComboBox Checked)
        {
            string[] CommonItems = Functions.CommonPickup;
            string[] RareItems = Functions.RarePickup;

            Checked.Items.Clear();
            for (int i = LevelRange; i < LevelRange + 9; i++)
                Checked.Items.Add(CommonItems[i]);
            for (int i = 2; i > 0; i--)
                Checked.Items.Add(RareItems[LevelRange + i]);

            if (Checked.Items.Count != 0)
            {
                Checked.CheckBoxItems[0].Checked = true;
                Checked.CheckBoxItems[0].Checked = false;
            }
        }

        private List<int> GetCandidateItems(CheckBoxComboBox checkedItems)
        {
            List<int> Candidates = new List<int>();
            if (checkedItems.Enabled)   // Ignore selected items if party slot doesn't have Pickup
            {
                for (int i = 1; i < checkedItems.Items.Count; i++)
                    if (checkedItems.CheckBoxItems[i].Checked)
                        Candidates.Add(i - 1);
            }
            return Candidates;
        }

        private List<string> CreateList(CheckBoxComboBox item)
        {
            List<string> list = new List<string>();
            for (int i = 1; i < item.Items.Count; i++)
                list.Add(item.Items[i].ToString());
            return list;
        }

        private void buttonPickupSearch_Click(object sender, EventArgs e)
        {
            var profile = (Profile)comboBoxProfiles.SelectedItem;
            iframes = new List<IFrameCapture>();
            listBindingPickup = new BindingSource { DataSource = iframes };
            dataGridViewPickup.DataSource = listBindingPickup;

            jobs = new Thread[cpus];
            generators = new FrameGenerator[cpus];
            waitHandle = new EventWaitHandle(true, EventResetMode.ManualReset);

            var year = (uint)DateTime.Now.Year;
            if (maskedTextBoxPickupYear.Text != "")
            {
                year = uint.Parse(maskedTextBoxPickupYear.Text);
                if (year < 2000)
                {
                    MessageBox.Show("You must enter a year greater than 1999.", "Please Enter a Valid Year", MessageBoxButtons.OK);
                    return;
                }
            }

            uint maxOffsetPickup;
            if (maxPickupAdv.Text != "")
                maxOffsetPickup = uint.Parse(maxPickupAdv.Text);
            else
            {
                maxPickupAdv.Text = "300";
                maxOffsetPickup = 300;
            }

            uint minOffsetPickup;
            if (minPickupAdv.Text != "")
                minOffsetPickup = uint.Parse(minPickupAdv.Text);
            else
            {
                minPickupAdv.Text = "0";
                minOffsetPickup = 0;
            }


            if (minOffsetPickup > maxOffsetPickup)
            {
                minPickupAdv.Focus();
                minPickupAdv.SelectAll();
                return;
            }

            colItem1.Visible = checkBoxParty1.Checked;
            colItem2.Visible = checkBoxParty2.Checked;
            colItem3.Visible = checkBoxParty3.Checked;
            colItem4.Visible = checkBoxParty4.Checked;
            colItem5.Visible = checkBoxParty5.Checked;
            colItem6.Visible = checkBoxParty6.Checked;

            int consumed = 5;
            if (profile.MemoryLink)
                consumed++;
            if (profile.ShinyCharm)
                consumed += 2;
            //if (WildHeldItem)
                //consumed++;

            generator = new FrameGenerator
            {
                // Now that each combo box item is a custom object containing the FrameType reference
                // We can simply retrieve the FrameType from the selected item
                FrameType = FrameType.Gen5Pickup,
                //EncounterMod = Objects.EncounterMod.Search,
                InitialFrame = minOffsetPickup,
                MaxResults = maxOffsetPickup - minOffsetPickup + 1,
            };

            //  Build up a FrameComparer
            frameCompare = new FrameCompare();
            frameCompare.comparePickupList = new List<int>[6];
            frameCompare.comparePickupList[0] = GetCandidateItems(TargetItem1);
            frameCompare.comparePickupList[1] = GetCandidateItems(TargetItem2);
            frameCompare.comparePickupList[2] = GetCandidateItems(TargetItem3);
            frameCompare.comparePickupList[3] = GetCandidateItems(TargetItem4);
            frameCompare.comparePickupList[4] = GetCandidateItems(TargetItem5);
            frameCompare.comparePickupList[5] = GetCandidateItems(TargetItem6);


            var months = new List<int>();
            for (int month = 1; month <= 12; month++)
            {
                if (comboBoxPickupMonth.CheckBoxItems[month].Checked)
                    months.Add(month);
            }

            if (months.Count == 0)
            {
                comboBoxPickupMonth.Focus();
                return;
            }
            List<List<ButtonComboType>> keypresses = profile.GetKeypresses();

            progressSearched = 0;
            progressFound = 0;

            int dayTotal = months.Sum(month => DateTime.DaysInMonth((int)year, month));
            progressTotal =
                (ulong)
                (dayTotal * 86400 * (maxOffsetPickup - minOffsetPickup + 1) * keypresses.Count *
                 (profile.Timer0Max - profile.Timer0Min + 1));

            for (int i = 0; i < jobs.Length; i++)
            {
                generators[i] = generator.Clone();
                generators[i].PossibleItems = GetItemList();
                generators[i].ConsumedAdvPickup = consumed;
                generators[i].TimeFinder = true;

                //copy to prevent issues with it being incremented before the actual thread really starts
                int i1 = i;
                //passing in a profile instead of the params would probably be more efficent
                jobs[i] = new Thread(() => GeneratePickupJob(year, months, 0, 23, profile, i1));
                jobs[i].Start();
                // for some reason not making the thread sleep causes issues with updating dayMin\Max
                Thread.Sleep(200);
            }
            var progressJob = new Thread(() => ManageProgress(listBindingPickup, dataGridViewPickup, generator.FrameType, 2000));
            progressJob.Start();
            progressJob.Priority = ThreadPriority.Lowest;

            buttonCapGenerate.Enabled = false;
            buttonEventGenerate.Enabled = false;
            buttonShinyGenerate.Enabled = false;
            buttonPickupSearch.Enabled = false;

            dataGridViewPickup.Focus();
        }


        public void GeneratePickupJob(uint year, List<int> months, int hourMin, int hourMax, Profile profile, int listIndex)
        {
            uint minAdvances = generators[listIndex].InitialFrame;
            uint RoamerAdvances = (uint)(!profile.IsBW2() && cbActiveRoamer.Checked ? 1 : 0);

            var array = new uint[80];
            array[6] = (uint)(profile.MAC_Address & 0xFFFF);

            if (profile.SoftReset)
            {
                array[6] = array[6] ^ 0x01000000;
            }

            var upperMAC = (uint)(profile.MAC_Address >> 16);
            array[7] = (upperMAC ^ (profile.VFrame * 0x1000000) ^ profile.GxStat);

            // Get the version-unique part of the message
            Array.Copy(Nazos.Nazo(profile.Version, profile.Language, profile.DSType), array, 5);

            array[10] = 0x00000000;
            array[11] = 0x00000000;
            array[13] = 0x80000000;
            array[14] = 0x00000000;
            array[15] = 0x000001A0;

            List<List<ButtonComboType>> keypressList = profile.GetKeypresses();
            List<ButtonComboType>[] buttons = keypressList.ToArray();
            var buttonMashValue = new uint[keypressList.Count];

            for (int i = 0; i < buttons.Length; i++)
            {
                buttonMashValue[i] = Functions.buttonMashed(buttons[i]);
            }

            uint searchRange = generator.MaxResults;

            foreach (int month in months)
            {
                GetDayIntervalForCpu((int)year, month, listIndex, out var dayMin, out var dayMax);

                string yearMonth = String.Format("{0:00}", year % 2000) + String.Format("{0:00}", month);
                for (int buttonCount = 0; buttonCount < keypressList.Count; buttonCount++)
                {
                    array[12] = buttonMashValue[buttonCount];
                    for (uint timer0 = profile.Timer0Min; timer0 <= profile.Timer0Max; timer0++)
                    {
                        array[5] = (profile.VCount << 16) + timer0;
                        array[5] = Functions.Reorder(array[5]);

                        for (int day = dayMin; day <= dayMax; day++)
                        {
                            var searchTime = new DateTime((int)year, month, day);

                            string dateString = String.Format("{0:00}", (int)searchTime.DayOfWeek);
                            dateString = String.Format("{0:00}", searchTime.Day) + dateString;
                            dateString = yearMonth + dateString;
                            array[8] = uint.Parse(dateString, NumberStyles.HexNumber);
                            array[9] = 0x0;

                            // For seeds with the same date, the contents of the SHA-1 array will be the same for the first 8 steps
                            // We are precomputing those 8 steps to save time
                            // Trying to precompute beyond 8 steps is complicated and does not save much time, also runs the risk of errors

                            uint[] alpha = Functions.AlphaEncrypt(array);

                            // We are also precomputing select portions of the SHA-1 array during the expansion process
                            // As they are also the same

                            array[16] = Functions.RotateLeft(array[13] ^ array[8] ^ array[2] ^ array[0], 1);
                            array[18] = Functions.RotateLeft(array[15] ^ array[10] ^ array[4] ^ array[2], 1);
                            array[19] = Functions.RotateLeft(array[16] ^ array[11] ^ array[5] ^ array[3], 1);
                            array[21] = Functions.RotateLeft(array[18] ^ array[13] ^ array[7] ^ array[5], 1);
                            array[22] = Functions.RotateLeft(array[19] ^ array[14] ^ array[8] ^ array[6], 1);
                            array[24] = Functions.RotateLeft(array[21] ^ array[16] ^ array[10] ^ array[8], 1);
                            array[27] = Functions.RotateLeft(array[24] ^ array[19] ^ array[13] ^ array[11], 1);

                            for (int hour = hourMin; hour <= hourMax; hour++)
                            {
                                //int seedHour = hour;
                                for (int minute = 0; minute <= 59; minute++)
                                {
                                    waitHandle.WaitOne();
                                    for (int second = 0; second <= 59; second++)
                                    {
                                        array[9] = Functions.seedSecond(second) | Functions.seedMinute(minute) |
                                                   Functions.seedHour(hour, profile.DSType);

                                        ulong seed = Functions.EncryptSeed(array, alpha);

                                        // Set this to our seed here
                                        generators[listIndex].InitialSeed = seed;
                                        generators[listIndex].InitialFrame = 
                                            Functions.initialPIDRNG(seed, profile) + minAdvances + RoamerAdvances - 1;

                                        if (iframes.Count > 100000)
                                            break;

                                        //  This is where we actually go ahead and call our 
                                        //  generator for a list of IVs based on parameters
                                        //  that have been passed in.
                                        List<Frame> frames = generators[listIndex].Generate(frameCompare, 0, 0);

                                        progressSearched += searchRange;
                                        progressFound += (ulong)frames.Count;

                                        //  Now we need to iterate through each result here
                                        //  and create a collection of the information that
                                        //  we are going to place into our grid.
                                        foreach (Frame frame in frames)
                                        {
                                            var iframe = new IFrameCapture();

                                            frame.DisplayPrep();
                                            iframe.Offset = frame.Number;
                                            iframe.Seed = seed;
                                            iframe.Frame = frame;
                                            iframe.Advances = iframe.Offset - (generators[listIndex].InitialFrame - minAdvances);

                                            iframe.TimeDate =
                                                searchTime.AddHours(hour).AddMinutes(minute).AddSeconds(second);
                                            iframe.KeyPresses = buttons[buttonCount];
                                            iframe.Timer0 = timer0;

                                            lock (threadLock)
                                            {
                                                iframes.Add(iframe);
                                            }
                                        }

                                        if (frames.Count > 0)
                                        {
                                            refreshQueue = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GetDayIntervalForCpu(int year, int month, int cpuIndex, out int dayMin, out int dayMax)
        {
            int totalDays = DateTime.DaysInMonth(year, month);
            // Divide the days evenly between CPUs
            int daysPerCpu = totalDays / cpus;
            // Extra days to distribute among first CPUs, if any.
            // This also is the last index in the cpu array that would get an extra day
            int extraDays = totalDays % cpus;

            // Calculate min/max days of the month this cpu will process
            dayMin = cpuIndex < extraDays
                ? (cpuIndex * (daysPerCpu + 1)) + 1
                : (extraDays * (daysPerCpu + 1)) + 1 + ((cpuIndex - extraDays) * daysPerCpu);
            dayMax = cpuIndex < extraDays
                ? dayMin + daysPerCpu
                : dayMin + daysPerCpu - 1;
        }

        private void dataGridViewPickup_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dataGridViewPickup.DataSource != null && iframes != null && listBindingPickup != null)
            {
                DataGridViewColumn selectedColumn = dataGridViewPickup.Columns[e.ColumnIndex];

                var iframeCaptureComparer = new IFrameCaptureComparer
                { CompareType = selectedColumn.DataPropertyName };

                if (selectedColumn.HeaderCell.SortGlyphDirection == SortOrder.Ascending)
                    iframeCaptureComparer.sortOrder = SortOrder.Descending;

                iframes.Sort(iframeCaptureComparer);

                listBindingPickup.ResetBindings(false);
                selectedColumn.HeaderCell.SortGlyphDirection = iframeCaptureComparer.sortOrder;
            }
        }

        public List<List<string>> GetItemList()
        {
            List<List<string>> List = new List<List<string>>
            {
                checkBoxParty1.Checked ? CreateList(TargetItem1) : null,
                checkBoxParty2.Checked ? CreateList(TargetItem2) : null,
                checkBoxParty3.Checked ? CreateList(TargetItem3) : null,
                checkBoxParty4.Checked ? CreateList(TargetItem4) : null,
                checkBoxParty5.Checked ? CreateList(TargetItem5) : null,
                checkBoxParty6.Checked ? CreateList(TargetItem6) : null,
            };
            return List;
        }


        #endregion

    }
}