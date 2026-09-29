namespace QuanLyBanNuocGK
{
    partial class FormTachMon
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
            this.lblHienThiSL = new System.Windows.Forms.Label();
            this.tkbSoLuong = new System.Windows.Forms.TrackBar();
            this.btnXacNhan = new System.Windows.Forms.Button();
            this.lblTenMon = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.tkbSoLuong)).BeginInit();
            this.SuspendLayout();
            // 
            // lblHienThiSL
            // 
            this.lblHienThiSL.AutoSize = true;
            this.lblHienThiSL.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHienThiSL.Location = new System.Drawing.Point(83, 61);
            this.lblHienThiSL.Name = "lblHienThiSL";
            this.lblHienThiSL.Size = new System.Drawing.Size(120, 20);
            this.lblHienThiSL.TabIndex = 0;
            this.lblHienThiSL.Text = "Số lượng tách:1";
            // 
            // tkbSoLuong
            // 
            this.tkbSoLuong.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tkbSoLuong.Location = new System.Drawing.Point(76, 93);
            this.tkbSoLuong.Name = "tkbSoLuong";
            this.tkbSoLuong.Size = new System.Drawing.Size(137, 45);
            this.tkbSoLuong.TabIndex = 1;
            this.tkbSoLuong.Scroll += new System.EventHandler(this.tkbSoLuong_Scroll);
            // 
            // btnXacNhan
            // 
            this.btnXacNhan.Location = new System.Drawing.Point(103, 144);
            this.btnXacNhan.Name = "btnXacNhan";
            this.btnXacNhan.Size = new System.Drawing.Size(75, 23);
            this.btnXacNhan.TabIndex = 2;
            this.btnXacNhan.Text = "Xác nhận";
            this.btnXacNhan.UseVisualStyleBackColor = true;
            this.btnXacNhan.Click += new System.EventHandler(this.btnXacNhan_Click);
            // 
            // lblTenMon
            // 
            this.lblTenMon.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTenMon.Location = new System.Drawing.Point(73, 30);
            this.lblTenMon.Name = "lblTenMon";
            this.lblTenMon.Size = new System.Drawing.Size(55, 17);
            this.lblTenMon.TabIndex = 3;
            // 
            // FormTachMon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(323, 179);
            this.Controls.Add(this.lblTenMon);
            this.Controls.Add(this.btnXacNhan);
            this.Controls.Add(this.tkbSoLuong);
            this.Controls.Add(this.lblHienThiSL);
            this.Name = "FormTachMon";
            this.Text = "Tách món";
            this.Load += new System.EventHandler(this.FormTachMon_Load);
            ((System.ComponentModel.ISupportInitialize)(this.tkbSoLuong)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblHienThiSL;
        private System.Windows.Forms.TrackBar tkbSoLuong;
        private System.Windows.Forms.Button btnXacNhan;
        private System.Windows.Forms.Label lblTenMon;
    }
}