namespace WestDungeon
{
    partial class Game
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            gameScreen = new PictureBox();
            gameTimer = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)gameScreen).BeginInit();
            SuspendLayout();
            // 
            // gameScreen
            // 
            gameScreen.BackColor = Color.Black;
            gameScreen.Dock = DockStyle.Fill;
            gameScreen.Location = new Point(0, 0);
            gameScreen.Margin = new Padding(4, 5, 4, 5);
            gameScreen.Name = "gameScreen";
            gameScreen.Size = new Size(478, 444);
            gameScreen.TabIndex = 0;
            gameScreen.TabStop = false;
            gameScreen.Click += OnClick;
            gameScreen.Paint += OnRender;
            gameScreen.MouseClick += OnMClick;
            // 
            // gameTimer
            // 
            gameTimer.Enabled = true;
            gameTimer.Interval = 60;
            gameTimer.Tick += OnTick;
            // 
            // Game
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(478, 444);
            Controls.Add(gameScreen);
            Margin = new Padding(4, 5, 4, 5);
            Name = "Game";
            Text = "Form1";
            Click += OnClick;
            PreviewKeyDown += OnKey;
            ((System.ComponentModel.ISupportInitialize)gameScreen).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox gameScreen;
        private System.Windows.Forms.Timer gameTimer;
    }
}
