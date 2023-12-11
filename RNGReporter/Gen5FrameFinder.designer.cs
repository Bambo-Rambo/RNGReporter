
namespace RNGReporter
{
    partial class Gen5FrameFinder
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
            this.tcSearchTypes = new System.Windows.Forms.TabControl();
            this.tpChatotSearch = new System.Windows.Forms.TabPage();
            this.btnAny = new System.Windows.Forms.Button();
            this.btnMidLow_Low = new System.Windows.Forms.Button();
            this.btnMid_MidLow = new System.Windows.Forms.Button();
            this.btbMidHigh_Mid = new System.Windows.Forms.Button();
            this.btnHigh_MidHigh = new System.Windows.Forms.Button();
            this.dgvFramePreview = new System.Windows.Forms.DataGridView();
            this.btnJumpToFrame = new System.Windows.Forms.Button();
            this.lblResultCount = new System.Windows.Forms.Label();
            this.lblResults = new System.Windows.Forms.Label();
            this.btnLow = new System.Windows.Forms.Button();
            this.btnMidLow = new System.Windows.Forms.Button();
            this.btnMid = new System.Windows.Forms.Button();
            this.btnHigh = new System.Windows.Forms.Button();
            this.btnMidHigh = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.txtChatotPitches = new System.Windows.Forms.TextBox();
            this.tcSearchTypes.SuspendLayout();
            this.tpChatotSearch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFramePreview)).BeginInit();
            this.SuspendLayout();
            // 
            // tcSearchTypes
            // 
            this.tcSearchTypes.Controls.Add(this.tpChatotSearch);
            this.tcSearchTypes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcSearchTypes.Location = new System.Drawing.Point(0, 0);
            this.tcSearchTypes.Name = "tcSearchTypes";
            this.tcSearchTypes.SelectedIndex = 0;
            this.tcSearchTypes.Size = new System.Drawing.Size(674, 381);
            this.tcSearchTypes.TabIndex = 1;
            // 
            // tpChatotSearch
            // 
            this.tpChatotSearch.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.tpChatotSearch.Controls.Add(this.btnAny);
            this.tpChatotSearch.Controls.Add(this.btnMidLow_Low);
            this.tpChatotSearch.Controls.Add(this.btnMid_MidLow);
            this.tpChatotSearch.Controls.Add(this.btbMidHigh_Mid);
            this.tpChatotSearch.Controls.Add(this.btnHigh_MidHigh);
            this.tpChatotSearch.Controls.Add(this.dgvFramePreview);
            this.tpChatotSearch.Controls.Add(this.btnJumpToFrame);
            this.tpChatotSearch.Controls.Add(this.lblResultCount);
            this.tpChatotSearch.Controls.Add(this.lblResults);
            this.tpChatotSearch.Controls.Add(this.btnLow);
            this.tpChatotSearch.Controls.Add(this.btnMidLow);
            this.tpChatotSearch.Controls.Add(this.btnMid);
            this.tpChatotSearch.Controls.Add(this.btnHigh);
            this.tpChatotSearch.Controls.Add(this.btnMidHigh);
            this.tpChatotSearch.Controls.Add(this.btnClear);
            this.tpChatotSearch.Controls.Add(this.btnRemove);
            this.tpChatotSearch.Controls.Add(this.txtChatotPitches);
            this.tpChatotSearch.ImageKey = "(Keine)";
            this.tpChatotSearch.Location = new System.Drawing.Point(4, 22);
            this.tpChatotSearch.Name = "tpChatotSearch";
            this.tpChatotSearch.Padding = new System.Windows.Forms.Padding(3);
            this.tpChatotSearch.Size = new System.Drawing.Size(666, 355);
            this.tpChatotSearch.TabIndex = 0;
            this.tpChatotSearch.Text = "Chatot Pitches";
            // 
            // btnAny
            // 
            this.btnAny.Location = new System.Drawing.Point(408, 41);
            this.btnAny.Name = "btnAny";
            this.btnAny.Size = new System.Drawing.Size(38, 25);
            this.btnAny.TabIndex = 12;
            this.btnAny.Text = "Any";
            this.btnAny.UseVisualStyleBackColor = true;
            this.btnAny.Click += new System.EventHandler(this.btnAny_Click);
            // 
            // btnMidLow_Low
            // 
            this.btnMidLow_Low.Location = new System.Drawing.Point(308, 41);
            this.btnMidLow_Low.Name = "btnMidLow_Low";
            this.btnMidLow_Low.Size = new System.Drawing.Size(90, 25);
            this.btnMidLow_Low.TabIndex = 11;
            this.btnMidLow_Low.Text = "Mid-Low / Low";
            this.btnMidLow_Low.UseVisualStyleBackColor = true;
            this.btnMidLow_Low.Click += new System.EventHandler(this.btnMidLow_Low_Click);
            // 
            // btnMid_MidLow
            // 
            this.btnMid_MidLow.Location = new System.Drawing.Point(208, 41);
            this.btnMid_MidLow.Name = "btnMid_MidLow";
            this.btnMid_MidLow.Size = new System.Drawing.Size(90, 25);
            this.btnMid_MidLow.TabIndex = 10;
            this.btnMid_MidLow.Text = "Mid / Mid-Low";
            this.btnMid_MidLow.UseVisualStyleBackColor = true;
            this.btnMid_MidLow.Click += new System.EventHandler(this.btnMid_MidLow_Click);
            // 
            // btbMidHigh_Mid
            // 
            this.btbMidHigh_Mid.Location = new System.Drawing.Point(108, 41);
            this.btbMidHigh_Mid.Name = "btbMidHigh_Mid";
            this.btbMidHigh_Mid.Size = new System.Drawing.Size(90, 25);
            this.btbMidHigh_Mid.TabIndex = 9;
            this.btbMidHigh_Mid.Text = "Mid-High / Mid";
            this.btbMidHigh_Mid.UseVisualStyleBackColor = true;
            this.btbMidHigh_Mid.Click += new System.EventHandler(this.btbMidHigh_Mid_Click);
            // 
            // btnHigh_MidHigh
            // 
            this.btnHigh_MidHigh.Location = new System.Drawing.Point(8, 41);
            this.btnHigh_MidHigh.Name = "btnHigh_MidHigh";
            this.btnHigh_MidHigh.Size = new System.Drawing.Size(90, 25);
            this.btnHigh_MidHigh.TabIndex = 8;
            this.btnHigh_MidHigh.Text = "High / Mid-High";
            this.btnHigh_MidHigh.UseVisualStyleBackColor = true;
            this.btnHigh_MidHigh.Click += new System.EventHandler(this.btnHigh_MidHigh_Click);
            // 
            // dgvFramePreview
            // 
            this.dgvFramePreview.AllowUserToAddRows = false;
            this.dgvFramePreview.AllowUserToDeleteRows = false;
            this.dgvFramePreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvFramePreview.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvFramePreview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFramePreview.Location = new System.Drawing.Point(8, 164);
            this.dgvFramePreview.MultiSelect = false;
            this.dgvFramePreview.Name = "dgvFramePreview";
            this.dgvFramePreview.ReadOnly = true;
            this.dgvFramePreview.RowHeadersVisible = false;
            this.dgvFramePreview.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvFramePreview.Size = new System.Drawing.Size(650, 183);
            this.dgvFramePreview.TabIndex = 16;
            // 
            // btnJumpToFrame
            // 
            this.btnJumpToFrame.Location = new System.Drawing.Point(8, 133);
            this.btnJumpToFrame.Name = "btnJumpToFrame";
            this.btnJumpToFrame.Size = new System.Drawing.Size(127, 25);
            this.btnJumpToFrame.TabIndex = 17;
            this.btnJumpToFrame.Text = "Jump to selected frame";
            this.btnJumpToFrame.UseVisualStyleBackColor = true;
            this.btnJumpToFrame.Click += new System.EventHandler(this.btnJumpToFrame_Click);
            // 
            // lblResultCount
            // 
            this.lblResultCount.AutoSize = true;
            this.lblResultCount.Location = new System.Drawing.Point(101, 110);
            this.lblResultCount.Name = "lblResultCount";
            this.lblResultCount.Size = new System.Drawing.Size(0, 13);
            this.lblResultCount.TabIndex = 27;
            // 
            // lblResults
            // 
            this.lblResults.AutoSize = true;
            this.lblResults.Location = new System.Drawing.Point(8, 110);
            this.lblResults.Name = "lblResults";
            this.lblResults.Size = new System.Drawing.Size(87, 13);
            this.lblResults.TabIndex = 26;
            this.lblResults.Text = "Possible Results:";
            // 
            // btnLow
            // 
            this.btnLow.Location = new System.Drawing.Point(366, 8);
            this.btnLow.Name = "btnLow";
            this.btnLow.Size = new System.Drawing.Size(80, 25);
            this.btnLow.TabIndex = 7;
            this.btnLow.Text = "Low";
            this.btnLow.UseVisualStyleBackColor = true;
            this.btnLow.Click += new System.EventHandler(this.btnLow_Click);
            // 
            // btnMidLow
            // 
            this.btnMidLow.Location = new System.Drawing.Point(276, 8);
            this.btnMidLow.Name = "btnMidLow";
            this.btnMidLow.Size = new System.Drawing.Size(80, 25);
            this.btnMidLow.TabIndex = 6;
            this.btnMidLow.Text = "Mid-Low";
            this.btnMidLow.UseVisualStyleBackColor = true;
            this.btnMidLow.Click += new System.EventHandler(this.btnMidLow_Click);
            // 
            // btnMid
            // 
            this.btnMid.Location = new System.Drawing.Point(186, 8);
            this.btnMid.Name = "btnMid";
            this.btnMid.Size = new System.Drawing.Size(80, 25);
            this.btnMid.TabIndex = 5;
            this.btnMid.Text = "Mid";
            this.btnMid.UseVisualStyleBackColor = true;
            this.btnMid.Click += new System.EventHandler(this.btnMid_Click);
            // 
            // btnHigh
            // 
            this.btnHigh.Location = new System.Drawing.Point(8, 8);
            this.btnHigh.Name = "btnHigh";
            this.btnHigh.Size = new System.Drawing.Size(80, 25);
            this.btnHigh.TabIndex = 3;
            this.btnHigh.Text = "High";
            this.btnHigh.UseVisualStyleBackColor = true;
            this.btnHigh.Click += new System.EventHandler(this.btnHigh_Click);
            // 
            // btnMidHigh
            // 
            this.btnMidHigh.Location = new System.Drawing.Point(96, 8);
            this.btnMidHigh.Name = "btnMidHigh";
            this.btnMidHigh.Size = new System.Drawing.Size(80, 25);
            this.btnMidHigh.TabIndex = 4;
            this.btnMidHigh.Text = "Mid-High";
            this.btnMidHigh.UseVisualStyleBackColor = true;
            this.btnMidHigh.Click += new System.EventHandler(this.btnMidHigh_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(543, 71);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(60, 25);
            this.btnClear.TabIndex = 15;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(473, 71);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(60, 25);
            this.btnRemove.TabIndex = 14;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // txtChatotPitches
            // 
            this.txtChatotPitches.Location = new System.Drawing.Point(8, 74);
            this.txtChatotPitches.Name = "txtChatotPitches";
            this.txtChatotPitches.Size = new System.Drawing.Size(438, 20);
            this.txtChatotPitches.TabIndex = 13;
            this.txtChatotPitches.TextChanged += new System.EventHandler(this.txtChatotPitches_TextChanged);
            // 
            // Gen5FrameFinder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(674, 381);
            this.Controls.Add(this.tcSearchTypes);
            this.Name = "Gen5FrameFinder";
            this.Text = "Gen 5 Frame Finder";
            this.Load += new System.EventHandler(this.Gen5FrameFinder_Load);
            this.tcSearchTypes.ResumeLayout(false);
            this.tpChatotSearch.ResumeLayout(false);
            this.tpChatotSearch.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFramePreview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tcSearchTypes;
        private System.Windows.Forms.TabPage tpChatotSearch;
        private System.Windows.Forms.Label lblResults;
        private System.Windows.Forms.Button btnLow;
        private System.Windows.Forms.Button btnMidLow;
        private System.Windows.Forms.Button btnMid;
        private System.Windows.Forms.Button btnHigh;
        private System.Windows.Forms.Button btnMidHigh;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.TextBox txtChatotPitches;
        private System.Windows.Forms.Label lblResultCount;
        private System.Windows.Forms.Button btnJumpToFrame;
        private System.Windows.Forms.DataGridView dgvFramePreview;
        private System.Windows.Forms.Button btnMidLow_Low;
        private System.Windows.Forms.Button btnMid_MidLow;
        private System.Windows.Forms.Button btbMidHigh_Mid;
        private System.Windows.Forms.Button btnHigh_MidHigh;
        private System.Windows.Forms.Button btnAny;
    }
}