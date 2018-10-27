namespace MikuMikuWorld
{
    partial class PictChatForm
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
            this.button_send = new System.Windows.Forms.Button();
            this.button_clear = new System.Windows.Forms.Button();
            this.checkBox_drawgrid = new System.Windows.Forms.CheckBox();
            this.button_save = new System.Windows.Forms.Button();
            this.button_load = new System.Windows.Forms.Button();
            this.comboBox_pics = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // button_send
            // 
            this.button_send.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_send.Location = new System.Drawing.Point(473, 526);
            this.button_send.Name = "button_send";
            this.button_send.Size = new System.Drawing.Size(53, 23);
            this.button_send.TabIndex = 0;
            this.button_send.Text = "Send";
            this.button_send.UseVisualStyleBackColor = true;
            this.button_send.Click += new System.EventHandler(this.button_send_Click);
            // 
            // button_clear
            // 
            this.button_clear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_clear.Location = new System.Drawing.Point(409, 526);
            this.button_clear.Name = "button_clear";
            this.button_clear.Size = new System.Drawing.Size(58, 23);
            this.button_clear.TabIndex = 1;
            this.button_clear.Text = "Clear";
            this.button_clear.UseVisualStyleBackColor = true;
            this.button_clear.Click += new System.EventHandler(this.button_clear_Click);
            // 
            // checkBox_drawgrid
            // 
            this.checkBox_drawgrid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox_drawgrid.AutoSize = true;
            this.checkBox_drawgrid.Checked = true;
            this.checkBox_drawgrid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_drawgrid.Location = new System.Drawing.Point(349, 529);
            this.checkBox_drawgrid.Name = "checkBox_drawgrid";
            this.checkBox_drawgrid.Size = new System.Drawing.Size(54, 18);
            this.checkBox_drawgrid.TabIndex = 2;
            this.checkBox_drawgrid.Text = "Grid";
            this.checkBox_drawgrid.UseVisualStyleBackColor = true;
            this.checkBox_drawgrid.CheckedChanged += new System.EventHandler(this.checkBox_whiteline_CheckedChanged);
            // 
            // button_save
            // 
            this.button_save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_save.Location = new System.Drawing.Point(12, 526);
            this.button_save.Name = "button_save";
            this.button_save.Size = new System.Drawing.Size(53, 23);
            this.button_save.TabIndex = 3;
            this.button_save.Text = "Save";
            this.button_save.UseVisualStyleBackColor = true;
            this.button_save.Click += new System.EventHandler(this.button_save_Click);
            // 
            // button_load
            // 
            this.button_load.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_load.Location = new System.Drawing.Point(71, 526);
            this.button_load.Name = "button_load";
            this.button_load.Size = new System.Drawing.Size(53, 23);
            this.button_load.TabIndex = 4;
            this.button_load.Text = "Load";
            this.button_load.UseVisualStyleBackColor = true;
            this.button_load.Click += new System.EventHandler(this.button_load_Click);
            // 
            // comboBox_pics
            // 
            this.comboBox_pics.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBox_pics.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_pics.FormattingEnabled = true;
            this.comboBox_pics.Location = new System.Drawing.Point(130, 527);
            this.comboBox_pics.Name = "comboBox_pics";
            this.comboBox_pics.Size = new System.Drawing.Size(137, 22);
            this.comboBox_pics.TabIndex = 5;
            // 
            // PictChatForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(538, 561);
            this.Controls.Add(this.comboBox_pics);
            this.Controls.Add(this.button_load);
            this.Controls.Add(this.button_save);
            this.Controls.Add(this.checkBox_drawgrid);
            this.Controls.Add(this.button_clear);
            this.Controls.Add(this.button_send);
            this.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "PictChatForm";
            this.Text = "Picture Chat";
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictChatForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictChatForm_MouseMove);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_send;
        private System.Windows.Forms.Button button_clear;
        private System.Windows.Forms.CheckBox checkBox_drawgrid;
        private System.Windows.Forms.Button button_save;
        private System.Windows.Forms.Button button_load;
        private System.Windows.Forms.ComboBox comboBox_pics;
    }
}