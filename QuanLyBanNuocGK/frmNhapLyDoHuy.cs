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
    public partial class frmNhapLyDoHuy : Form
    {
        public frmNhapLyDoHuy()
        {
            InitializeComponent();
        }

        public string LyDo => txtLyDo.Text.Trim();
        private void frmNhapLyDoHuy_Load(object sender, EventArgs e)
        {

        }

        private void btnXacNhan_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLyDo.Text))
            {
                MessageBox.Show("Vui lòng nhập lý do!", "Thông báo");
                this.DialogResult = DialogResult.None; // Không cho đóng Form
            }
            else
            {
                this.DialogResult = DialogResult.OK; // Cho phép đóng và trả về kết quả
            }
        }

        private void btnHuyBo_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
