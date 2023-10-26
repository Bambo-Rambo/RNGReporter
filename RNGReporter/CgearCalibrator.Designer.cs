namespace RNGReporter
{
    partial class CgearCalibrator
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.TargetSeed = new RNGReporter.Controls.MaskedTextBox2();
            this.IVFrame = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.DGV = new RNGReporter.DoubleBufferedDataGridView();
            this.btnSearch = new RNGReporter.GlassButton();
            this.minHP = new System.Windows.Forms.NumericUpDown();
            this.minAtk = new System.Windows.Forms.NumericUpDown();
            this.minDef = new System.Windows.Forms.NumericUpDown();
            this.minSpA = new System.Windows.Forms.NumericUpDown();
            this.minSpD = new System.Windows.Forms.NumericUpDown();
            this.minSpe = new System.Windows.Forms.NumericUpDown();
            this.maxHP = new System.Windows.Forms.NumericUpDown();
            this.maxAtk = new System.Windows.Forms.NumericUpDown();
            this.maxDef = new System.Windows.Forms.NumericUpDown();
            this.maxSpA = new System.Windows.Forms.NumericUpDown();
            this.maxSpD = new System.Windows.Forms.NumericUpDown();
            this.maxSpe = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.TargetDelay = new System.Windows.Forms.NumericUpDown();
            this.InfoBox = new System.Windows.Forms.GroupBox();
            this.WildIVs = new System.Windows.Forms.GroupBox();
            this.Seed = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Difference = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NewDelay = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Atk = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Def = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SpA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SpD = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Spe = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.IVFrame)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DGV)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.minHP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.minAtk)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.minDef)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.minSpA)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.minSpD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.minSpe)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxHP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxAtk)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxDef)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxSpA)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxSpD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxSpe)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TargetDelay)).BeginInit();
            this.InfoBox.SuspendLayout();
            this.WildIVs.SuspendLayout();
            this.SuspendLayout();
            // 
            // TargetSeed
            // 
            this.TargetSeed.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.TargetSeed.Hex = false;
            this.TargetSeed.Location = new System.Drawing.Point(136, 31);
            this.TargetSeed.Mask = "AAAAAAAA";
            this.TargetSeed.Name = "TargetSeed";
            this.TargetSeed.Size = new System.Drawing.Size(61, 20);
            this.TargetSeed.TabIndex = 0;
            // 
            // IVFrame
            // 
            this.IVFrame.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.IVFrame.Location = new System.Drawing.Point(136, 108);
            this.IVFrame.Name = "IVFrame";
            this.IVFrame.Size = new System.Drawing.Size(61, 22);
            this.IVFrame.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(45, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Target Seed";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(45, 110);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "IV Frame";
            // 
            // DGV
            // 
            this.DGV.AllowUserToAddRows = false;
            this.DGV.AllowUserToDeleteRows = false;
            this.DGV.AllowUserToResizeRows = false;
            this.DGV.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Seed,
            this.Difference,
            this.NewDelay,
            this.HP,
            this.Atk,
            this.Def,
            this.SpA,
            this.SpD,
            this.Spe});
            this.DGV.Location = new System.Drawing.Point(12, 215);
            this.DGV.Name = "DGV";
            this.DGV.RowHeadersVisible = false;
            this.DGV.RowTemplate.Height = 20;
            this.DGV.Size = new System.Drawing.Size(490, 351);
            this.DGV.TabIndex = 7;
            // 
            // btnSearch
            // 
            this.btnSearch.BackColor = System.Drawing.Color.AntiqueWhite;
            this.btnSearch.ForeColor = System.Drawing.Color.Black;
            this.btnSearch.Location = new System.Drawing.Point(12, 173);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.OuterBorderColor = System.Drawing.Color.Transparent;
            this.btnSearch.ShineColor = System.Drawing.SystemColors.Window;
            this.btnSearch.Size = new System.Drawing.Size(239, 36);
            this.btnSearch.TabIndex = 8;
            this.btnSearch.Text = "Search";
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // minHP
            // 
            this.minHP.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.minHP.Location = new System.Drawing.Point(70, 28);
            this.minHP.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.minHP.Name = "minHP";
            this.minHP.Size = new System.Drawing.Size(56, 22);
            this.minHP.TabIndex = 9;
            // 
            // minAtk
            // 
            this.minAtk.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.minAtk.Location = new System.Drawing.Point(70, 54);
            this.minAtk.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.minAtk.Name = "minAtk";
            this.minAtk.Size = new System.Drawing.Size(56, 22);
            this.minAtk.TabIndex = 10;
            // 
            // minDef
            // 
            this.minDef.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.minDef.Location = new System.Drawing.Point(70, 80);
            this.minDef.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.minDef.Name = "minDef";
            this.minDef.Size = new System.Drawing.Size(56, 22);
            this.minDef.TabIndex = 11;
            // 
            // minSpA
            // 
            this.minSpA.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.minSpA.Location = new System.Drawing.Point(70, 106);
            this.minSpA.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.minSpA.Name = "minSpA";
            this.minSpA.Size = new System.Drawing.Size(56, 22);
            this.minSpA.TabIndex = 12;
            // 
            // minSpD
            // 
            this.minSpD.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.minSpD.Location = new System.Drawing.Point(70, 132);
            this.minSpD.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.minSpD.Name = "minSpD";
            this.minSpD.Size = new System.Drawing.Size(56, 22);
            this.minSpD.TabIndex = 13;
            // 
            // minSpe
            // 
            this.minSpe.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.minSpe.Location = new System.Drawing.Point(70, 158);
            this.minSpe.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.minSpe.Name = "minSpe";
            this.minSpe.Size = new System.Drawing.Size(56, 22);
            this.minSpe.TabIndex = 14;
            // 
            // maxHP
            // 
            this.maxHP.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.maxHP.Location = new System.Drawing.Point(148, 28);
            this.maxHP.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.maxHP.Name = "maxHP";
            this.maxHP.Size = new System.Drawing.Size(56, 22);
            this.maxHP.TabIndex = 15;
            this.maxHP.Value = new decimal(new int[] {
            31,
            0,
            0,
            0});
            // 
            // maxAtk
            // 
            this.maxAtk.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.maxAtk.Location = new System.Drawing.Point(148, 54);
            this.maxAtk.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.maxAtk.Name = "maxAtk";
            this.maxAtk.Size = new System.Drawing.Size(56, 22);
            this.maxAtk.TabIndex = 16;
            this.maxAtk.Value = new decimal(new int[] {
            31,
            0,
            0,
            0});
            // 
            // maxDef
            // 
            this.maxDef.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.maxDef.Location = new System.Drawing.Point(148, 80);
            this.maxDef.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.maxDef.Name = "maxDef";
            this.maxDef.Size = new System.Drawing.Size(56, 22);
            this.maxDef.TabIndex = 17;
            this.maxDef.Value = new decimal(new int[] {
            31,
            0,
            0,
            0});
            // 
            // maxSpA
            // 
            this.maxSpA.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.maxSpA.Location = new System.Drawing.Point(148, 106);
            this.maxSpA.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.maxSpA.Name = "maxSpA";
            this.maxSpA.Size = new System.Drawing.Size(56, 22);
            this.maxSpA.TabIndex = 18;
            this.maxSpA.Value = new decimal(new int[] {
            31,
            0,
            0,
            0});
            // 
            // maxSpD
            // 
            this.maxSpD.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.maxSpD.Location = new System.Drawing.Point(148, 132);
            this.maxSpD.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.maxSpD.Name = "maxSpD";
            this.maxSpD.Size = new System.Drawing.Size(56, 22);
            this.maxSpD.TabIndex = 19;
            this.maxSpD.Value = new decimal(new int[] {
            31,
            0,
            0,
            0});
            // 
            // maxSpe
            // 
            this.maxSpe.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.maxSpe.Location = new System.Drawing.Point(148, 158);
            this.maxSpe.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.maxSpe.Name = "maxSpe";
            this.maxSpe.Size = new System.Drawing.Size(56, 22);
            this.maxSpe.TabIndex = 20;
            this.maxSpe.Value = new decimal(new int[] {
            31,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(45, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 13);
            this.label3.TabIndex = 21;
            this.label3.Text = "Target Delay";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(132, 30);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(10, 13);
            this.label4.TabIndex = 22;
            this.label4.Text = "-";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(132, 56);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(10, 13);
            this.label5.TabIndex = 23;
            this.label5.Text = "-";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(132, 82);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(10, 13);
            this.label6.TabIndex = 24;
            this.label6.Text = "-";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(132, 108);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(10, 13);
            this.label7.TabIndex = 25;
            this.label7.Text = "-";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(132, 134);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(10, 13);
            this.label8.TabIndex = 26;
            this.label8.Text = "-";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(132, 160);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(10, 13);
            this.label9.TabIndex = 27;
            this.label9.Text = "-";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(34, 30);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(22, 13);
            this.label10.TabIndex = 28;
            this.label10.Text = "HP";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(34, 56);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(23, 13);
            this.label11.TabIndex = 29;
            this.label11.Text = "Atk";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(34, 82);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(24, 13);
            this.label12.TabIndex = 30;
            this.label12.Text = "Def";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(34, 108);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(27, 13);
            this.label13.TabIndex = 31;
            this.label13.Text = "SpA";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(34, 134);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(28, 13);
            this.label14.TabIndex = 32;
            this.label14.Text = "SpD";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(34, 160);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(26, 13);
            this.label15.TabIndex = 33;
            this.label15.Text = "Spe";
            // 
            // TargetDelay
            // 
            this.TargetDelay.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.TargetDelay.Location = new System.Drawing.Point(136, 67);
            this.TargetDelay.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.TargetDelay.Name = "TargetDelay";
            this.TargetDelay.Size = new System.Drawing.Size(61, 22);
            this.TargetDelay.TabIndex = 34;
            // 
            // InfoBox
            // 
            this.InfoBox.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.InfoBox.Controls.Add(this.label1);
            this.InfoBox.Controls.Add(this.TargetDelay);
            this.InfoBox.Controls.Add(this.TargetSeed);
            this.InfoBox.Controls.Add(this.IVFrame);
            this.InfoBox.Controls.Add(this.label2);
            this.InfoBox.Controls.Add(this.label3);
            this.InfoBox.Location = new System.Drawing.Point(12, 12);
            this.InfoBox.Name = "InfoBox";
            this.InfoBox.Size = new System.Drawing.Size(239, 155);
            this.InfoBox.TabIndex = 35;
            this.InfoBox.TabStop = false;
            this.InfoBox.Text = "RNG Info";
            // 
            // WildIVs
            // 
            this.WildIVs.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.WildIVs.Controls.Add(this.minAtk);
            this.WildIVs.Controls.Add(this.minHP);
            this.WildIVs.Controls.Add(this.label15);
            this.WildIVs.Controls.Add(this.minDef);
            this.WildIVs.Controls.Add(this.label14);
            this.WildIVs.Controls.Add(this.minSpA);
            this.WildIVs.Controls.Add(this.label13);
            this.WildIVs.Controls.Add(this.minSpD);
            this.WildIVs.Controls.Add(this.label12);
            this.WildIVs.Controls.Add(this.minSpe);
            this.WildIVs.Controls.Add(this.label11);
            this.WildIVs.Controls.Add(this.maxHP);
            this.WildIVs.Controls.Add(this.label10);
            this.WildIVs.Controls.Add(this.maxAtk);
            this.WildIVs.Controls.Add(this.label9);
            this.WildIVs.Controls.Add(this.maxDef);
            this.WildIVs.Controls.Add(this.label8);
            this.WildIVs.Controls.Add(this.maxSpA);
            this.WildIVs.Controls.Add(this.label7);
            this.WildIVs.Controls.Add(this.maxSpD);
            this.WildIVs.Controls.Add(this.label6);
            this.WildIVs.Controls.Add(this.maxSpe);
            this.WildIVs.Controls.Add(this.label5);
            this.WildIVs.Controls.Add(this.label4);
            this.WildIVs.Location = new System.Drawing.Point(257, 12);
            this.WildIVs.Name = "WildIVs";
            this.WildIVs.Size = new System.Drawing.Size(245, 197);
            this.WildIVs.TabIndex = 36;
            this.WildIVs.TabStop = false;
            this.WildIVs.Text = "Wild IVs";
            // 
            // Seed
            // 
            this.Seed.HeaderText = "Seed";
            this.Seed.Name = "Seed";
            this.Seed.ReadOnly = true;
            // 
            // Difference
            // 
            this.Difference.HeaderText = "Difference";
            this.Difference.Name = "Difference";
            this.Difference.ReadOnly = true;
            this.Difference.Width = 70;
            // 
            // NewDelay
            // 
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.NewDelay.DefaultCellStyle = dataGridViewCellStyle1;
            this.NewDelay.HeaderText = "New Delay";
            this.NewDelay.Name = "NewDelay";
            this.NewDelay.ReadOnly = true;
            this.NewDelay.Width = 90;
            // 
            // HP
            // 
            this.HP.HeaderText = "HP";
            this.HP.Name = "HP";
            this.HP.ReadOnly = true;
            this.HP.Width = 35;
            // 
            // Atk
            // 
            this.Atk.HeaderText = "Atk";
            this.Atk.Name = "Atk";
            this.Atk.ReadOnly = true;
            this.Atk.Width = 35;
            // 
            // Def
            // 
            this.Def.HeaderText = "Def";
            this.Def.Name = "Def";
            this.Def.ReadOnly = true;
            this.Def.Width = 35;
            // 
            // SpA
            // 
            this.SpA.HeaderText = "SpA";
            this.SpA.Name = "SpA";
            this.SpA.ReadOnly = true;
            this.SpA.Width = 35;
            // 
            // SpD
            // 
            this.SpD.HeaderText = "SpD";
            this.SpD.Name = "SpD";
            this.SpD.ReadOnly = true;
            this.SpD.Width = 35;
            // 
            // Spe
            // 
            this.Spe.HeaderText = "Spe";
            this.Spe.Name = "Spe";
            this.Spe.ReadOnly = true;
            this.Spe.Width = 35;
            // 
            // CgearCalibrator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 578);
            this.Controls.Add(this.WildIVs);
            this.Controls.Add(this.InfoBox);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.DGV);
            this.Name = "CgearCalibrator";
            this.Text = "C-Gear Calibrator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CgearCalibrator_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.IVFrame)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DGV)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.minHP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.minAtk)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.minDef)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.minSpA)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.minSpD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.minSpe)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxHP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxAtk)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxDef)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxSpA)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxSpD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxSpe)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TargetDelay)).EndInit();
            this.InfoBox.ResumeLayout(false);
            this.InfoBox.PerformLayout();
            this.WildIVs.ResumeLayout(false);
            this.WildIVs.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.MaskedTextBox2 TargetSeed;
        private System.Windows.Forms.NumericUpDown IVFrame;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private DoubleBufferedDataGridView DGV;
        private GlassButton btnSearch;
        private System.Windows.Forms.NumericUpDown minHP;
        private System.Windows.Forms.NumericUpDown minAtk;
        private System.Windows.Forms.NumericUpDown minDef;
        private System.Windows.Forms.NumericUpDown minSpA;
        private System.Windows.Forms.NumericUpDown minSpD;
        private System.Windows.Forms.NumericUpDown minSpe;
        private System.Windows.Forms.NumericUpDown maxHP;
        private System.Windows.Forms.NumericUpDown maxAtk;
        private System.Windows.Forms.NumericUpDown maxDef;
        private System.Windows.Forms.NumericUpDown maxSpA;
        private System.Windows.Forms.NumericUpDown maxSpD;
        private System.Windows.Forms.NumericUpDown maxSpe;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.NumericUpDown TargetDelay;
        private System.Windows.Forms.GroupBox InfoBox;
        private System.Windows.Forms.GroupBox WildIVs;
        private System.Windows.Forms.DataGridViewTextBoxColumn Seed;
        private System.Windows.Forms.DataGridViewTextBoxColumn Difference;
        private System.Windows.Forms.DataGridViewTextBoxColumn NewDelay;
        private System.Windows.Forms.DataGridViewTextBoxColumn HP;
        private System.Windows.Forms.DataGridViewTextBoxColumn Atk;
        private System.Windows.Forms.DataGridViewTextBoxColumn Def;
        private System.Windows.Forms.DataGridViewTextBoxColumn SpA;
        private System.Windows.Forms.DataGridViewTextBoxColumn SpD;
        private System.Windows.Forms.DataGridViewTextBoxColumn Spe;
    }
}