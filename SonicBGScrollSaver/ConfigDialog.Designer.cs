namespace SonicBGScrollSaver
{
	partial class ConfigDialog
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
			System.Windows.Forms.Label label1;
			System.Windows.Forms.Label label2;
			System.Windows.Forms.Label label3;
			System.Windows.Forms.Label label4;
			System.Windows.Forms.Label label5;
			System.Windows.Forms.Button okButton;
			this.playMusicCheckBox = new System.Windows.Forms.CheckBox();
			this.musicVolumeNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.framesPerSecondNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.scrollSpeedNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.levelsListView = new System.Windows.Forms.ListView();
			this.shuffleCheckBox = new System.Windows.Forms.CheckBox();
			this.levelUpButton = new System.Windows.Forms.Button();
			this.levelDownButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.displayTimeHourControl = new SonicBGScrollSaver.HourControl();
			label1 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			label3 = new System.Windows.Forms.Label();
			label4 = new System.Windows.Forms.Label();
			label5 = new System.Windows.Forms.Label();
			okButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.musicVolumeNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.framesPerSecondNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.scrollSpeedNumericUpDown)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(12, 37);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(76, 13);
			label1.TabIndex = 1;
			label1.Text = "Music Volume:";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(12, 63);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(103, 13);
			label2.TabIndex = 3;
			label2.Text = "Frames Per Second:";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Location = new System.Drawing.Point(12, 89);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(70, 13);
			label3.TabIndex = 5;
			label3.Text = "Scroll Speed:";
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Location = new System.Drawing.Point(12, 338);
			label4.Name = "label4";
			label4.Size = new System.Drawing.Size(70, 13);
			label4.TabIndex = 12;
			label4.Text = "Display Time:";
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Location = new System.Drawing.Point(12, 113);
			label5.Name = "label5";
			label5.Size = new System.Drawing.Size(41, 13);
			label5.TabIndex = 7;
			label5.Text = "Levels:";
			// 
			// okButton
			// 
			okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			okButton.Location = new System.Drawing.Point(137, 365);
			okButton.Name = "okButton";
			okButton.Size = new System.Drawing.Size(75, 23);
			okButton.TabIndex = 14;
			okButton.Text = "&OK";
			okButton.UseVisualStyleBackColor = true;
			okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// playMusicCheckBox
			// 
			this.playMusicCheckBox.AutoSize = true;
			this.playMusicCheckBox.Location = new System.Drawing.Point(12, 12);
			this.playMusicCheckBox.Name = "playMusicCheckBox";
			this.playMusicCheckBox.Size = new System.Drawing.Size(77, 17);
			this.playMusicCheckBox.TabIndex = 0;
			this.playMusicCheckBox.Text = "Play Music";
			this.playMusicCheckBox.UseVisualStyleBackColor = true;
			// 
			// musicVolumeNumericUpDown
			// 
			this.musicVolumeNumericUpDown.Location = new System.Drawing.Point(94, 35);
			this.musicVolumeNumericUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.musicVolumeNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.musicVolumeNumericUpDown.Name = "musicVolumeNumericUpDown";
			this.musicVolumeNumericUpDown.Size = new System.Drawing.Size(51, 20);
			this.musicVolumeNumericUpDown.TabIndex = 2;
			this.musicVolumeNumericUpDown.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
			// 
			// framesPerSecondNumericUpDown
			// 
			this.framesPerSecondNumericUpDown.Location = new System.Drawing.Point(121, 61);
			this.framesPerSecondNumericUpDown.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
			this.framesPerSecondNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.framesPerSecondNumericUpDown.Name = "framesPerSecondNumericUpDown";
			this.framesPerSecondNumericUpDown.Size = new System.Drawing.Size(44, 20);
			this.framesPerSecondNumericUpDown.TabIndex = 4;
			this.framesPerSecondNumericUpDown.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
			// 
			// scrollSpeedNumericUpDown
			// 
			this.scrollSpeedNumericUpDown.Location = new System.Drawing.Point(88, 87);
			this.scrollSpeedNumericUpDown.Maximum = new decimal(new int[] {
            64,
            0,
            0,
            0});
			this.scrollSpeedNumericUpDown.Minimum = new decimal(new int[] {
            64,
            0,
            0,
            -2147483648});
			this.scrollSpeedNumericUpDown.Name = "scrollSpeedNumericUpDown";
			this.scrollSpeedNumericUpDown.Size = new System.Drawing.Size(44, 20);
			this.scrollSpeedNumericUpDown.TabIndex = 6;
			this.scrollSpeedNumericUpDown.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
			// 
			// levelsListView
			// 
			this.levelsListView.CheckBoxes = true;
			this.levelsListView.FullRowSelect = true;
			this.levelsListView.Location = new System.Drawing.Point(62, 113);
			this.levelsListView.MultiSelect = false;
			this.levelsListView.Name = "levelsListView";
			this.levelsListView.Size = new System.Drawing.Size(166, 214);
			this.levelsListView.TabIndex = 8;
			this.levelsListView.UseCompatibleStateImageBehavior = false;
			this.levelsListView.View = System.Windows.Forms.View.List;
			this.levelsListView.SelectedIndexChanged += new System.EventHandler(this.levelsListView_SelectedIndexChanged);
			// 
			// shuffleCheckBox
			// 
			this.shuffleCheckBox.AutoSize = true;
			this.shuffleCheckBox.Location = new System.Drawing.Point(234, 171);
			this.shuffleCheckBox.Name = "shuffleCheckBox";
			this.shuffleCheckBox.Size = new System.Drawing.Size(59, 17);
			this.shuffleCheckBox.TabIndex = 11;
			this.shuffleCheckBox.Text = "Shuffle";
			this.shuffleCheckBox.UseVisualStyleBackColor = true;
			// 
			// levelUpButton
			// 
			this.levelUpButton.AutoSize = true;
			this.levelUpButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.levelUpButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.levelUpButton.Enabled = false;
			this.levelUpButton.Location = new System.Drawing.Point(234, 113);
			this.levelUpButton.Name = "levelUpButton";
			this.levelUpButton.Size = new System.Drawing.Size(29, 23);
			this.levelUpButton.TabIndex = 9;
			this.levelUpButton.Text = "↑";
			this.levelUpButton.UseVisualStyleBackColor = true;
			this.levelUpButton.Click += new System.EventHandler(this.levelUpButton_Click);
			// 
			// levelDownButton
			// 
			this.levelDownButton.AutoSize = true;
			this.levelDownButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.levelDownButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.levelDownButton.Enabled = false;
			this.levelDownButton.Location = new System.Drawing.Point(234, 142);
			this.levelDownButton.Name = "levelDownButton";
			this.levelDownButton.Size = new System.Drawing.Size(29, 23);
			this.levelDownButton.TabIndex = 10;
			this.levelDownButton.Text = "↓";
			this.levelDownButton.UseVisualStyleBackColor = true;
			this.levelDownButton.Click += new System.EventHandler(this.levelDownButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(218, 365);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 15;
			this.cancelButton.Text = "&Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// displayTimeHourControl
			// 
			this.displayTimeHourControl.AutoSize = true;
			this.displayTimeHourControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.displayTimeHourControl.Centiseconds = 0;
			this.displayTimeHourControl.Hours = 0;
			this.displayTimeHourControl.Location = new System.Drawing.Point(88, 333);
			this.displayTimeHourControl.Minutes = 5;
			this.displayTimeHourControl.Name = "displayTimeHourControl";
			this.displayTimeHourControl.Seconds = 0;
			this.displayTimeHourControl.Size = new System.Drawing.Size(205, 26);
			this.displayTimeHourControl.TabIndex = 13;
			this.displayTimeHourControl.TimeSpan = System.TimeSpan.Parse("00:05:00");
			// 
			// ConfigDialog
			// 
			this.AcceptButton = okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(305, 400);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(okButton);
			this.Controls.Add(this.levelDownButton);
			this.Controls.Add(this.levelUpButton);
			this.Controls.Add(this.shuffleCheckBox);
			this.Controls.Add(this.levelsListView);
			this.Controls.Add(label5);
			this.Controls.Add(this.displayTimeHourControl);
			this.Controls.Add(label4);
			this.Controls.Add(this.scrollSpeedNumericUpDown);
			this.Controls.Add(label3);
			this.Controls.Add(this.framesPerSecondNumericUpDown);
			this.Controls.Add(label2);
			this.Controls.Add(this.musicVolumeNumericUpDown);
			this.Controls.Add(this.playMusicCheckBox);
			this.Controls.Add(label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ConfigDialog";
			this.Text = "Sonic Background Scrolling Screensaver Configuration";
			this.Load += new System.EventHandler(this.ConfigDialog_Load);
			((System.ComponentModel.ISupportInitialize)(this.musicVolumeNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.framesPerSecondNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.scrollSpeedNumericUpDown)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckBox playMusicCheckBox;
		private System.Windows.Forms.NumericUpDown musicVolumeNumericUpDown;
		private System.Windows.Forms.NumericUpDown framesPerSecondNumericUpDown;
		private System.Windows.Forms.NumericUpDown scrollSpeedNumericUpDown;
		private HourControl displayTimeHourControl;
		private System.Windows.Forms.ListView levelsListView;
		private System.Windows.Forms.CheckBox shuffleCheckBox;
		private System.Windows.Forms.Button levelUpButton;
		private System.Windows.Forms.Button levelDownButton;
		private System.Windows.Forms.Button cancelButton;
	}
}