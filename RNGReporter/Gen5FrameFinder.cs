using RNGReporter.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RNGReporter
{
    public partial class Gen5FrameFinder : Form
    {
        private DataGridView MainWindowDgv;
        private string ChatotPitches = "";

        public Gen5FrameFinder(DataGridView mainWindowDgv)
        {
            InitializeComponent();
            this.MainWindowDgv = mainWindowDgv;
            this.MainWindowDgv.DataSourceChanged += MainWindowDgv_DataSourceChanged;
            this.dgvFramePreview.AutoGenerateColumns = false;
        }

        private void Gen5FrameFinder_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        private void MainWindowDgv_DataSourceChanged(object sender, EventArgs e)
        {
            try
            {
                Initialize();
            }
            catch (Exception ex)
            {
                this.Close();
            }
        }

        private void Initialize()
        {
            CopyDgvColumns();
            ChatotPitches = GetChatotPitchesFromFrames();
            FindFrameFromChatotPitches(txtChatotPitches.Text);
        }

        private void CopyDgvColumns()
        {
            try
            {
                this.dgvFramePreview.Columns.Clear();
                foreach (DataGridViewColumn col in MainWindowDgv.Columns)
                {
                    if (col.Visible)
                    {
                        this.dgvFramePreview.Columns.Add(col.Clone() as DataGridViewColumn);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Close();
            }
        }

        private void ClearDgvRows()
        {
            dgvFramePreview.DataSource = null;
            dgvFramePreview.Refresh();
        }

        #region event handler methods for buttons

        private void btnHigh_Click(object sender, EventArgs e)
        {
            txtChatotPitches.Text += "H,";
        }

        private void btnMidHigh_Click(object sender, EventArgs e)
        {
            txtChatotPitches.Text += "MH,";
        }

        private void btnMid_Click(object sender, EventArgs e)
        {
            txtChatotPitches.Text += "M,";
        }

        private void btnMidLow_Click(object sender, EventArgs e)
        {
            txtChatotPitches.Text += "ML,";
        }

        private void btnLow_Click(object sender, EventArgs e)
        {
            txtChatotPitches.Text += "L,";
        }

        private void btnHigh_MidHigh_Click(object sender, EventArgs e)
        {
            txtChatotPitches.Text += "HMH,";
        }

        private void btbMidHigh_Mid_Click(object sender, EventArgs e)
        {
            txtChatotPitches.Text += "MHM,";
        }

        private void btnMid_MidLow_Click(object sender, EventArgs e)
        {
            txtChatotPitches.Text += "MML,";
        }

        private void btnMidLow_Low_Click(object sender, EventArgs e)
        {
            txtChatotPitches.Text += "MLL,";
        }

        private void btnAny_Click(object sender, EventArgs e)
        {
            txtChatotPitches.Text += "*,";
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtChatotPitches.Text.Length > 0)
                {
                    txtChatotPitches.Text = txtChatotPitches.Text.TrimEnd(',');
                    txtChatotPitches.Text = txtChatotPitches.Text.Remove(txtChatotPitches.Text.LastIndexOf(',') + 1);
                }
            }
            catch (ArgumentOutOfRangeException) //an incredibly lazy fix I know, but how could one say no to such a convenient fix?
            {
                txtChatotPitches.Text = "";
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtChatotPitches.Text = "";
            ClearDgvRows();
        }

        #endregion

        private void txtChatotPitches_TextChanged(object sender, EventArgs e)
        {
            try
            {
                FindFrameFromChatotPitches(txtChatotPitches.Text);
            }
            catch (Exception ex)
            {
    
            }
        }

        private List<Frame> GetFramesFromMainWindow()
        {
            return MainWindowDgv.DataSource as List<Frame>;
        }

        private string GetChatotPitchesFromFrames()
        {
            List<Frame> frames = GetFramesFromMainWindow();
            StringBuilder chatotPitches = new StringBuilder();

            foreach (Frame frame in frames)
            {
                string pitch = frame.Chatot64.Split(' ')[0];

                switch (pitch)
                {
                    case "High": pitch = "H"; break;
                    case "Mid-High": pitch = "MH"; break;
                    case "Mid": pitch = "M"; break;
                    case "Mid-Low":  pitch = "ML"; break;
                    case "Low": pitch = "L"; break;
                }
                
                chatotPitches.Append( $"{pitch},");
            }
            
            return chatotPitches.ToString();
        }

        private void FindFrameFromChatotPitches(string chatotPitchSequence)
        {
            if (chatotPitchSequence.Length >= 2)
            {
                string pattern = CreateRegexPattern(chatotPitchSequence);
                List<int> matchingIndexes = Regex.Matches(ChatotPitches, pattern).Cast<Match>().Select(m => m.Index).ToList<int>();
                lblResultCount.Text = matchingIndexes.Count.ToString();

                if (matchingIndexes.Count <= 20)
                {
                    ShowMatchingFramesInDgv(matchingIndexes, chatotPitchSequence);
                }
                else
                {
                    ClearDgvRows();
                }
            }
            else
            {
                lblResultCount.Text = GetFramesFromMainWindow().Count.ToString();
                //displays the amount of results from the main window cause the regex would show matches for whitespaces / empty strings
            }            
        }

        private string CreateRegexPattern(string chatotPitchSequence)
        {
            string pattern = Regex.Escape(chatotPitchSequence);
            pattern = pattern.Replace("\\*", "(H|MH|M|ML|L)");
            pattern = pattern.Replace("HMH", "(H|MH)");
            pattern = pattern.Replace("MHM", "(MH|M)");
            pattern = pattern.Replace("MML", "(M|ML)");
            pattern = pattern.Replace("MLL", "(ML|L)");
            return pattern;
        }

        private void ShowMatchingFramesInDgv(List<int> matchingIndexes, string chatotPitchSequence)
        {
            List<Frame> frames = GetFramesFromMainWindow();
            List<Frame> previewFrames = new List<Frame>();

            dgvFramePreview.SuspendLayout();
            foreach (int matchingIndex in matchingIndexes)
            {
                /*
                 Since our chatot sequence can contain wildcards the string length might vary, therefore
                 we use array lengths to determine our target index
                 */
                int offset = chatotPitchSequence.TrimEnd(',').Split(',').Length; // each array index equals one chatot advance
                string[] framesBeforeFirstAdvance = ChatotPitches.Substring(0, matchingIndex).TrimEnd(',').Split(','); //creates a string[] which contains the chatot pitches from 0 to our matching index
                int initialAdvances = (framesBeforeFirstAdvance[0] != "") ? framesBeforeFirstAdvance.Length : 0;
                //if the matching index is 0 substring will return an empty string which results in array length 1 instead of the 0 we need so we need to account for that edge case

                int currentRowIndex = initialAdvances + offset - 1; // -1 since we're going from total lengh to index number

                Frame frame = frames[currentRowIndex];
                previewFrames.Add(frame);
            }

            dgvFramePreview.DataSource = previewFrames;
            dgvFramePreview.ResumeLayout();
        }

        private void btnJumpToFrame_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvFramePreview.SelectedCells.Count < 1)
                {
                    MessageBox.Show("You need to select a target frame in the preview!", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    List<Frame> frames = GetFramesFromMainWindow();

                    DataGridViewRow row = dgvFramePreview.SelectedCells[0].OwningRow;
                    int targetFrame = Convert.ToInt32((row.DataBoundItem as Frame).Number);
                    int targetFrameIndex = frames.FindIndex(f => f.Number == targetFrame);

                    MainWindowDgv.CurrentCell = MainWindowDgv.Rows[targetFrameIndex].Cells[0];
                }
            }
            catch (Exception ex)
            {

            }
        }

        
    }
}
