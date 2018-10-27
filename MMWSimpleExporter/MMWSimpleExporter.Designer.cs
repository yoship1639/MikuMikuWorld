namespace MMWSimpleExporter
{
    partial class MMWSimpleExporter
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_inputPath = new System.Windows.Forms.TextBox();
            this.button_ofd = new System.Windows.Forms.Button();
            this.textBox_desc = new System.Windows.Forms.TextBox();
            this.button_output = new System.Windows.Forms.Button();
            this.checkBox_encrypt = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_name = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_author = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_editor = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_description = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_ver = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "入力モデル";
            // 
            // textBox_inputPath
            // 
            this.textBox_inputPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_inputPath.Location = new System.Drawing.Point(78, 6);
            this.textBox_inputPath.Name = "textBox_inputPath";
            this.textBox_inputPath.Size = new System.Drawing.Size(320, 23);
            this.textBox_inputPath.TabIndex = 1;
            this.textBox_inputPath.TextChanged += new System.EventHandler(this.textBox_inputPath_TextChanged);
            // 
            // button_ofd
            // 
            this.button_ofd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_ofd.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_ofd.Location = new System.Drawing.Point(404, 5);
            this.button_ofd.Name = "button_ofd";
            this.button_ofd.Size = new System.Drawing.Size(26, 23);
            this.button_ofd.TabIndex = 2;
            this.button_ofd.Text = "...";
            this.button_ofd.UseVisualStyleBackColor = true;
            this.button_ofd.Click += new System.EventHandler(this.button_ofd_Click);
            // 
            // textBox_desc
            // 
            this.textBox_desc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_desc.Location = new System.Drawing.Point(12, 283);
            this.textBox_desc.Multiline = true;
            this.textBox_desc.Name = "textBox_desc";
            this.textBox_desc.ReadOnly = true;
            this.textBox_desc.Size = new System.Drawing.Size(420, 112);
            this.textBox_desc.TabIndex = 3;
            // 
            // button_output
            // 
            this.button_output.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_output.Enabled = false;
            this.button_output.Location = new System.Drawing.Point(378, 401);
            this.button_output.Name = "button_output";
            this.button_output.Size = new System.Drawing.Size(54, 23);
            this.button_output.TabIndex = 4;
            this.button_output.Text = "出力";
            this.button_output.UseVisualStyleBackColor = true;
            this.button_output.Click += new System.EventHandler(this.button_output_Click);
            // 
            // checkBox_encrypt
            // 
            this.checkBox_encrypt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_encrypt.AutoSize = true;
            this.checkBox_encrypt.Location = new System.Drawing.Point(15, 405);
            this.checkBox_encrypt.Name = "checkBox_encrypt";
            this.checkBox_encrypt.Size = new System.Drawing.Size(62, 19);
            this.checkBox_encrypt.TabIndex = 5;
            this.checkBox_encrypt.Text = "暗号化";
            this.checkBox_encrypt.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "名前";
            // 
            // textBox_name
            // 
            this.textBox_name.Location = new System.Drawing.Point(66, 26);
            this.textBox_name.Name = "textBox_name";
            this.textBox_name.Size = new System.Drawing.Size(185, 23);
            this.textBox_name.TabIndex = 7;
            this.textBox_name.TextChanged += new System.EventHandler(this.textBox_name_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.textBox_ver);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.textBox_description);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.textBox_editor);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBox_author);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBox_name);
            this.groupBox1.Location = new System.Drawing.Point(12, 35);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(418, 242);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "プロパティ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 15);
            this.label3.TabIndex = 8;
            this.label3.Text = "作者";
            // 
            // textBox_author
            // 
            this.textBox_author.Location = new System.Drawing.Point(66, 55);
            this.textBox_author.Name = "textBox_author";
            this.textBox_author.Size = new System.Drawing.Size(185, 23);
            this.textBox_author.TabIndex = 9;
            this.textBox_author.TextChanged += new System.EventHandler(this.textBox_author_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 87);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 15);
            this.label4.TabIndex = 10;
            this.label4.Text = "改変者";
            // 
            // textBox_editor
            // 
            this.textBox_editor.Location = new System.Drawing.Point(66, 84);
            this.textBox_editor.Name = "textBox_editor";
            this.textBox_editor.Size = new System.Drawing.Size(185, 23);
            this.textBox_editor.TabIndex = 11;
            this.textBox_editor.TextChanged += new System.EventHandler(this.textBox_editor_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(17, 116);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 15);
            this.label5.TabIndex = 12;
            this.label5.Text = "説明文";
            // 
            // textBox_description
            // 
            this.textBox_description.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_description.Location = new System.Drawing.Point(6, 134);
            this.textBox_description.Multiline = true;
            this.textBox_description.Name = "textBox_description";
            this.textBox_description.Size = new System.Drawing.Size(402, 99);
            this.textBox_description.TabIndex = 13;
            this.textBox_description.TextChanged += new System.EventHandler(this.textBox_description_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(270, 29);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(52, 15);
            this.label6.TabIndex = 14;
            this.label6.Text = "バージョン";
            // 
            // textBox_ver
            // 
            this.textBox_ver.Location = new System.Drawing.Point(328, 26);
            this.textBox_ver.Name = "textBox_ver";
            this.textBox_ver.Size = new System.Drawing.Size(80, 23);
            this.textBox_ver.TabIndex = 15;
            this.textBox_ver.TextChanged += new System.EventHandler(this.textBox_ver_TextChanged);
            // 
            // MMWSimpleExporter
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(444, 436);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.checkBox_encrypt);
            this.Controls.Add(this.button_output);
            this.Controls.Add(this.textBox_desc);
            this.Controls.Add(this.button_ofd);
            this.Controls.Add(this.textBox_inputPath);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "MMWSimpleExporter";
            this.Text = "MMW Simple Exporter";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_inputPath;
        private System.Windows.Forms.Button button_ofd;
        private System.Windows.Forms.TextBox textBox_desc;
        private System.Windows.Forms.Button button_output;
        private System.Windows.Forms.CheckBox checkBox_encrypt;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_name;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_author;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_description;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_editor;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox_ver;
    }
}

