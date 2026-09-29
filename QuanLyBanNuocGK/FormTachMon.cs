using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuanLyBanNuocGK
{
    public partial class FormTachMon : Form
    {
        public int SoLuongChon { get; set; }
        public bool IsXacNhan { get; set; } = false;
        public FormTachMon(string tenMon, int slHienTai)
        {
            InitializeComponent();
            lblTenMon.Text = tenMon;

            // Cấu hình thanh trượt
            tkbSoLuong.Minimum = 1;
            tkbSoLuong.Maximum = slHienTai;
            tkbSoLuong.Value = 1; // Mặc định tách 1

            // Hiện số lượng ban đầu
            lblHienThiSL.Text = $"Tách: 1 / {slHienTai}";
        }

        private void FormTachMon_Load(object sender, EventArgs e)
        {

        }

        private void tkbSoLuong_Scroll(object sender, EventArgs e)
        {
            lblHienThiSL.Text = $"Tách: {tkbSoLuong.Value} / {tkbSoLuong.Maximum}";
        }

        private void btnXacNhan_Click(object sender, EventArgs e)
        {
            SoLuongChon = tkbSoLuong.Value;
            IsXacNhan = true;
            this.Close();
        }
    }
}
