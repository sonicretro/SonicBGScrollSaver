using SonicRetro.SonLVL.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SonicBGScrollSaver
{
	public partial class ConfigDialog : Form
	{
		public ConfigDialog()
		{
			InitializeComponent();
		}

		Settings settings;

		private void ConfigDialog_Load(object sender, EventArgs e)
		{
			Environment.CurrentDirectory = Application.StartupPath;
			settings = Settings.Load();
			playMusicCheckBox.Checked = settings.PlayMusic;
			musicVolumeNumericUpDown.Value = settings.MusicVolume;
			framesPerSecondNumericUpDown.Value = settings.FramesPerSecond;
			scrollSpeedNumericUpDown.Value = settings.ScrollSpeed;
			Dictionary<string, LevelInfo> levels = new Dictionary<string, LevelInfo>();
			foreach (string filename in Directory.GetFiles(Environment.CurrentDirectory, "setup.ini", SearchOption.AllDirectories))
				levels.Add(Path.GetDirectoryName(filename).Substring(Environment.CurrentDirectory.Length + 1), IniSerializer.Deserialize<LevelInfo>(filename));
			levelsListView.BeginUpdate();
			if (settings.Levels == null)
				settings.Levels = new List<string>();
			foreach (string level in new List<string>(settings.Levels))
				if (levels.ContainsKey(level))
				{
					LevelInfo inf = levels[level];
					levelsListView.Items.Add(new ListViewItem(inf.Name) { Tag = level, Checked = true });
				}
				else
				{
					MessageBox.Show(this, "Level \"" + level + "\" could not be found.\n\nThis level will be removed from the list.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					settings.Levels.Remove(level);
				}
			foreach (KeyValuePair<string, LevelInfo> inf in levels)
				if (!settings.Levels.Contains(inf.Key))
					levelsListView.Items.Add(new ListViewItem(inf.Value.Name) { Tag = inf.Key });
			levelsListView.EndUpdate();
			shuffleCheckBox.Checked = settings.Shuffle;
			displayTimeHourControl.TimeSpan = settings.DisplayTime;
		}

		private void levelsListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (levelsListView.SelectedIndices.Count == 0)
				levelUpButton.Enabled = levelDownButton.Enabled = false;
			else
			{
				levelUpButton.Enabled = levelsListView.SelectedIndices[0] > 0;
				levelDownButton.Enabled = levelsListView.SelectedIndices[0] < levelsListView.Items.Count - 1;
			}
		}

		private void levelUpButton_Click(object sender, EventArgs e)
		{
			int i = levelsListView.SelectedIndices[0];
			ListViewItem item = levelsListView.Items[i];
			levelsListView.BeginUpdate();
			levelsListView.Items.Remove(item);
			levelsListView.Items.Insert(i - 1, item);
			levelsListView.EndUpdate();
		}

		private void levelDownButton_Click(object sender, EventArgs e)
		{
			int i = levelsListView.SelectedIndices[0];
			ListViewItem item = levelsListView.Items[i];
			levelsListView.BeginUpdate();
			levelsListView.Items.Remove(item);
			levelsListView.Items.Insert(i + 1, item);
			levelsListView.EndUpdate();
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			settings.PlayMusic = playMusicCheckBox.Checked;
			settings.MusicVolume = (int)musicVolumeNumericUpDown.Value;
			settings.FramesPerSecond = (byte)framesPerSecondNumericUpDown.Value;
			settings.ScrollSpeed = (short)scrollSpeedNumericUpDown.Value;
			settings.Levels.Clear();
			foreach (ListViewItem item in levelsListView.CheckedItems)
				settings.Levels.Add((string)item.Tag);
			settings.Shuffle = shuffleCheckBox.Checked;
			settings.DisplayTime = displayTimeHourControl.TimeSpan;
			settings.Save();
			Close();
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
