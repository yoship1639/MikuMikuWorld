namespace Exporter_MMW
{
    partial class CollisionCapsule
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
            this.numericUpDown_height = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDown_radius = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_height)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_radius)).BeginInit();
            this.SuspendLayout();
            // 
            // numericUpDown_height
            // 
            this.numericUpDown_height.DecimalPlaces = 2;
            this.numericUpDown_height.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDown_height.Location = new System.Drawing.Point(58, 3);
            this.numericUpDown_height.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown_height.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDown_height.Name = "numericUpDown_height";
            this.numericUpDown_height.Size = new System.Drawing.Size(80, 22);
            this.numericUpDown_height.TabIndex = 0;
            this.numericUpDown_height.Value = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 14);
            this.label1.TabIndex = 1;
            this.label1.Text = "Height";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 14);
            this.label2.TabIndex = 3;
            this.label2.Text = "Radius";
            // 
            // numericUpDown_radius
            // 
            this.numericUpDown_radius.DecimalPlaces = 2;
            this.numericUpDown_radius.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDown_radius.Location = new System.Drawing.Point(58, 31);
            this.numericUpDown_radius.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown_radius.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDown_radius.Name = "numericUpDown_radius";
            this.numericUpDown_radius.Size = new System.Drawing.Size(80, 22);
            this.numericUpDown_radius.TabIndex = 2;
            this.numericUpDown_radius.Value = new decimal(new int[] {
            3,
            0,
            0,
            65536});
            // 
            // CollisionCapsule
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numericUpDown_radius);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDown_height);
            this.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "CollisionCapsule";
            this.Size = new System.Drawing.Size(547, 70);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_height)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_radius)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.NumericUpDown numericUpDown_height;
        public System.Windows.Forms.NumericUpDown numericUpDown_radius;
    }
}
