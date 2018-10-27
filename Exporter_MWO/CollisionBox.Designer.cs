namespace Exporter_MMW
{
    partial class CollisionBox
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

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDown_x = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDown_y = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDown_z = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_x)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_y)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_z)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 14);
            this.label1.TabIndex = 7;
            this.label1.Text = "Half Extents";
            // 
            // numericUpDown_x
            // 
            this.numericUpDown_x.DecimalPlaces = 2;
            this.numericUpDown_x.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDown_x.Location = new System.Drawing.Point(36, 22);
            this.numericUpDown_x.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown_x.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDown_x.Name = "numericUpDown_x";
            this.numericUpDown_x.Size = new System.Drawing.Size(80, 22);
            this.numericUpDown_x.TabIndex = 6;
            this.numericUpDown_x.Value = new decimal(new int[] {
            3,
            0,
            0,
            65536});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 14);
            this.label2.TabIndex = 8;
            this.label2.Text = "X";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(135, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 14);
            this.label3.TabIndex = 10;
            this.label3.Text = "Y";
            // 
            // numericUpDown_y
            // 
            this.numericUpDown_y.DecimalPlaces = 2;
            this.numericUpDown_y.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDown_y.Location = new System.Drawing.Point(155, 22);
            this.numericUpDown_y.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown_y.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDown_y.Name = "numericUpDown_y";
            this.numericUpDown_y.Size = new System.Drawing.Size(80, 22);
            this.numericUpDown_y.TabIndex = 9;
            this.numericUpDown_y.Value = new decimal(new int[] {
            3,
            0,
            0,
            65536});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(254, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(14, 14);
            this.label4.TabIndex = 12;
            this.label4.Text = "Z";
            // 
            // numericUpDown_z
            // 
            this.numericUpDown_z.DecimalPlaces = 2;
            this.numericUpDown_z.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDown_z.Location = new System.Drawing.Point(274, 22);
            this.numericUpDown_z.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown_z.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDown_z.Name = "numericUpDown_z";
            this.numericUpDown_z.Size = new System.Drawing.Size(80, 22);
            this.numericUpDown_z.TabIndex = 11;
            this.numericUpDown_z.Value = new decimal(new int[] {
            3,
            0,
            0,
            65536});
            // 
            // CollisionBox
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.label4);
            this.Controls.Add(this.numericUpDown_z);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numericUpDown_y);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDown_x);
            this.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "CollisionBox";
            this.Size = new System.Drawing.Size(547, 70);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_x)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_y)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_z)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.NumericUpDown numericUpDown_x;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.NumericUpDown numericUpDown_y;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.NumericUpDown numericUpDown_z;
    }
}
