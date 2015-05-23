using SonicRetro.SonLVL.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SonicBGScrollSaver
{
	public partial class ConfigDialog : Form
	{
		class LevelKeyValue
		{
			public string Key { get; set; }
			public string Value { get; set; }

			public LevelKeyValue(string key, string value)
			{
				Key = key;
				Value = value;
			}

			public override string ToString()
			{
				return Value;
			}
		}

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
			levelsCheckedListBox.BeginUpdate();
			if (settings.Levels == null)
				settings.Levels = new List<string>();
			foreach (string level in new List<string>(settings.Levels))
				if (levels.ContainsKey(level))
				{
					LevelInfo inf = levels[level];
					levelsCheckedListBox.Items.Add(new LevelKeyValue(level, inf.Name), true);
				}
				else
				{
					MessageBox.Show(this, "Level \"" + level + "\" could not be found.\n\nThis level will be removed from the list.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					settings.Levels.Remove(level);
				}
			foreach (KeyValuePair<string, LevelInfo> inf in levels)
				if (!settings.Levels.Contains(inf.Key))
					levelsCheckedListBox.Items.Add(new LevelKeyValue(inf.Key, inf.Value.Name));
			levelsCheckedListBox.EndUpdate();
			shuffleCheckBox.Checked = settings.Shuffle;
			displayTimeHourControl.TimeSpan = settings.DisplayTime;
		}

		private void levelsListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (levelsCheckedListBox.SelectedIndices.Count == 0)
				levelUpButton.Enabled = levelDownButton.Enabled = false;
			else
			{
				levelUpButton.Enabled = levelsCheckedListBox.SelectedIndices[0] > 0;
				levelDownButton.Enabled = levelsCheckedListBox.SelectedIndices[0] < levelsCheckedListBox.Items.Count - 1;
			}
		}

		private void levelUpButton_Click(object sender, EventArgs e)
		{
			int i = levelsCheckedListBox.SelectedIndices[0];
			object item = levelsCheckedListBox.Items[i];
			levelsCheckedListBox.BeginUpdate();
			levelsCheckedListBox.Items.Remove(item);
			levelsCheckedListBox.Items.Insert(i - 1, item);
			levelsCheckedListBox.EndUpdate();
		}

		private void levelDownButton_Click(object sender, EventArgs e)
		{
			int i = levelsCheckedListBox.SelectedIndices[0];
			object item = levelsCheckedListBox.Items[i];
			levelsCheckedListBox.BeginUpdate();
			levelsCheckedListBox.Items.Remove(item);
			levelsCheckedListBox.Items.Insert(i + 1, item);
			levelsCheckedListBox.EndUpdate();
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			settings.PlayMusic = playMusicCheckBox.Checked;
			settings.MusicVolume = (int)musicVolumeNumericUpDown.Value;
			settings.FramesPerSecond = (byte)framesPerSecondNumericUpDown.Value;
			settings.ScrollSpeed = (short)scrollSpeedNumericUpDown.Value;
			settings.Levels.Clear();
			foreach (LevelKeyValue item in levelsCheckedListBox.CheckedItems)
				settings.Levels.Add(item.Key);
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
