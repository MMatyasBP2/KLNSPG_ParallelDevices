namespace StatisticalApp
{
    partial class MainWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.MainTitle = new System.Windows.Forms.Label();
            this.StartButton = new System.Windows.Forms.Button();
            this.SampleNameBox = new System.Windows.Forms.RichTextBox();
            this.StopButton = new System.Windows.Forms.Button();
            this.StatNameBox = new System.Windows.Forms.RichTextBox();
            this.StatLabel = new System.Windows.Forms.Label();
            this.PlotButton = new System.Windows.Forms.Button();
            this.SampleValueBox = new System.Windows.Forms.RichTextBox();
            this.StatValueBox = new System.Windows.Forms.RichTextBox();
            this.RedLight = new System.Windows.Forms.PictureBox();
            this.GreenLight = new System.Windows.Forms.PictureBox();
            this.ConfigButton = new System.Windows.Forms.Button();
            this.ResultButton = new System.Windows.Forms.Button();
            this.ParallelResultBox = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.RedLight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GreenLight)).BeginInit();
            this.SuspendLayout();
            // 
            // MainTitle
            // 
            this.MainTitle.AutoSize = true;
            this.MainTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.MainTitle.Location = new System.Drawing.Point(323, 9);
            this.MainTitle.Name = "MainTitle";
            this.MainTitle.Size = new System.Drawing.Size(400, 46);
            this.MainTitle.TabIndex = 0;
            this.MainTitle.Text = "Statistical Application";
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(99, 157);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(108, 47);
            this.StartButton.TabIndex = 1;
            this.StartButton.Text = "Sample Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // SampleNameBox
            // 
            this.SampleNameBox.Enabled = false;
            this.SampleNameBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SampleNameBox.Location = new System.Drawing.Point(99, 217);
            this.SampleNameBox.Name = "SampleNameBox";
            this.SampleNameBox.Size = new System.Drawing.Size(94, 183);
            this.SampleNameBox.TabIndex = 2;
            this.SampleNameBox.Text = "";
            // 
            // StopButton
            // 
            this.StopButton.Location = new System.Drawing.Point(281, 157);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(67, 47);
            this.StopButton.TabIndex = 5;
            this.StopButton.Text = "STOP";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // StatNameBox
            // 
            this.StatNameBox.Enabled = false;
            this.StatNameBox.Location = new System.Drawing.Point(437, 217);
            this.StatNameBox.Name = "StatNameBox";
            this.StatNameBox.Size = new System.Drawing.Size(97, 183);
            this.StatNameBox.TabIndex = 6;
            this.StatNameBox.Text = "";
            this.StatNameBox.Visible = false;
            // 
            // StatLabel
            // 
            this.StatLabel.AutoSize = true;
            this.StatLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.StatLabel.Location = new System.Drawing.Point(441, 175);
            this.StatLabel.Name = "StatLabel";
            this.StatLabel.Size = new System.Drawing.Size(93, 29);
            this.StatLabel.TabIndex = 7;
            this.StatLabel.Text = "Results";
            this.StatLabel.Visible = false;
            // 
            // PlotButton
            // 
            this.PlotButton.Location = new System.Drawing.Point(99, 428);
            this.PlotButton.Name = "PlotButton";
            this.PlotButton.Size = new System.Drawing.Size(58, 65);
            this.PlotButton.TabIndex = 8;
            this.PlotButton.Text = "Draw";
            this.PlotButton.UseVisualStyleBackColor = true;
            this.PlotButton.Click += new System.EventHandler(this.PlotButton_Click);
            // 
            // SampleValueBox
            // 
            this.SampleValueBox.Enabled = false;
            this.SampleValueBox.Location = new System.Drawing.Point(199, 217);
            this.SampleValueBox.Name = "SampleValueBox";
            this.SampleValueBox.Size = new System.Drawing.Size(86, 183);
            this.SampleValueBox.TabIndex = 9;
            this.SampleValueBox.Text = "";
            // 
            // StatValueBox
            // 
            this.StatValueBox.Enabled = false;
            this.StatValueBox.Location = new System.Drawing.Point(540, 217);
            this.StatValueBox.Name = "StatValueBox";
            this.StatValueBox.Size = new System.Drawing.Size(87, 183);
            this.StatValueBox.TabIndex = 10;
            this.StatValueBox.Text = "";
            this.StatValueBox.Visible = false;
            // 
            // RedLight
            // 
            this.RedLight.Image = ((System.Drawing.Image)(resources.GetObject("RedLight.Image")));
            this.RedLight.InitialImage = ((System.Drawing.Image)(resources.GetObject("RedLight.InitialImage")));
            this.RedLight.Location = new System.Drawing.Point(952, 12);
            this.RedLight.Name = "RedLight";
            this.RedLight.Size = new System.Drawing.Size(104, 86);
            this.RedLight.TabIndex = 11;
            this.RedLight.TabStop = false;
            // 
            // GreenLight
            // 
            this.GreenLight.Image = ((System.Drawing.Image)(resources.GetObject("GreenLight.Image")));
            this.GreenLight.Location = new System.Drawing.Point(952, 12);
            this.GreenLight.Name = "GreenLight";
            this.GreenLight.Size = new System.Drawing.Size(104, 86);
            this.GreenLight.TabIndex = 12;
            this.GreenLight.TabStop = false;
            this.GreenLight.Visible = false;
            // 
            // ConfigButton
            // 
            this.ConfigButton.Location = new System.Drawing.Point(191, 428);
            this.ConfigButton.Name = "ConfigButton";
            this.ConfigButton.Size = new System.Drawing.Size(105, 65);
            this.ConfigButton.TabIndex = 13;
            this.ConfigButton.Text = "Open configuration file";
            this.ConfigButton.UseVisualStyleBackColor = true;
            this.ConfigButton.Click += new System.EventHandler(this.ConfigButton_Click);
            // 
            // ResultButton
            // 
            this.ResultButton.Location = new System.Drawing.Point(437, 428);
            this.ResultButton.Name = "ResultButton";
            this.ResultButton.Size = new System.Drawing.Size(97, 65);
            this.ResultButton.TabIndex = 14;
            this.ResultButton.Text = "Open results";
            this.ResultButton.UseVisualStyleBackColor = true;
            this.ResultButton.Visible = false;
            this.ResultButton.Click += new System.EventHandler(this.ResultButton_Click);
            // 
            // ParallelResultBox
            // 
            this.ParallelResultBox.Enabled = false;
            this.ParallelResultBox.Location = new System.Drawing.Point(694, 217);
            this.ParallelResultBox.Name = "ParallelResultBox";
            this.ParallelResultBox.Size = new System.Drawing.Size(362, 100);
            this.ParallelResultBox.TabIndex = 15;
            this.ParallelResultBox.Text = "";
            this.ParallelResultBox.Visible = false;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1068, 628);
            this.Controls.Add(this.ParallelResultBox);
            this.Controls.Add(this.ResultButton);
            this.Controls.Add(this.ConfigButton);
            this.Controls.Add(this.GreenLight);
            this.Controls.Add(this.RedLight);
            this.Controls.Add(this.StatValueBox);
            this.Controls.Add(this.SampleValueBox);
            this.Controls.Add(this.PlotButton);
            this.Controls.Add(this.StatLabel);
            this.Controls.Add(this.StatNameBox);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.SampleNameBox);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.MainTitle);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainWindow";
            this.Text = "StatisticalApplication";
            ((System.ComponentModel.ISupportInitialize)(this.RedLight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GreenLight)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label MainTitle;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.RichTextBox StatNameBox;
        private System.Windows.Forms.Label StatLabel;
        private System.Windows.Forms.RichTextBox SampleNameBox;
        private System.Windows.Forms.Button PlotButton;
        private System.Windows.Forms.RichTextBox SampleValueBox;
        private System.Windows.Forms.RichTextBox StatValueBox;
        private System.Windows.Forms.PictureBox RedLight;
        private System.Windows.Forms.PictureBox GreenLight;
        private System.Windows.Forms.Button ConfigButton;
        private System.Windows.Forms.Button ResultButton;
        private System.Windows.Forms.RichTextBox ParallelResultBox;
    }
}

