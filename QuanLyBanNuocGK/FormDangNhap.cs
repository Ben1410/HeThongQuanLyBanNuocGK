using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuanLyBanNuocGK
{
    public partial class FormDangNhap : Form
    {
        string chuoiKN = @"Data Source=MSI\SQLEXPRESS;Initial Catalog=Quanlynuocgiaikhat;Integrated Security=True;TrustServerCertificate=True";

        public FormDangNhap()
        {
            InitializeComponent();
        }
        public static class Session
        {
            public static string TenNhanVien = "";
            public static string MaNhanVien = "";
            public static string Quyen = ""; // Nếu có phân quyền Admin/Nhân viên
        }
        private void FormDangNhap_Load(object sender, EventArgs e)
        {
            this.ActiveControl = lblTieuDe;
        }

        private void txtTenDangNhap_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            string user = txtUser.Text.Trim();
            string pass = txtPass.Text.Trim();

            using (SqlConnection ketNoi = new SqlConnection(chuoiKN))
            {
                ketNoi.Open();

                string sql = @"
        SELECT TK.MaNV, TK.TenDangNhap, TK.Quyen, NV.HoTen, NV.TrangThai
        FROM TaiKhoan TK
        JOIN NhanVien NV ON TK.MaNV = NV.MaNV
        WHERE TK.TenDangNhap = @user AND TK.MatKhau = @pass";

                SqlCommand cmd = new SqlCommand(sql, ketNoi);
                cmd.Parameters.AddWithValue("@user", user);
                cmd.Parameters.AddWithValue("@pass", pass);

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string trangThai = dr["TrangThai"].ToString();

                    if (trangThai == "Đã xóa")
                    {
                        dr.Close();
                        MessageBox.Show("Tài khoản này đã bị vô hiệu hóa!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        // Đăng nhập thành công
                        BienDungChung.MaNV = dr["MaNV"].ToString();
                        BienDungChung.TenDangNhap = dr["TenDangNhap"].ToString();
                        BienDungChung.Quyen = dr["Quyen"].ToString();
                        BienDungChung.HoTenNhanVien = dr["HoTen"].ToString();

                        dr.Close();


                        this.DialogResult = DialogResult.OK;
                        this.Close(); // Đóng form đăng nhập

                    }
                }
                else
                {
                    dr.Close();
                    MessageBox.Show("Sai tài khoản hoặc mật khẩu!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPass.Text = "";
                    txtPass.Focus();
                }
            }
        }
        

        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn thoát không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Application.Exit(); // Đóng toàn bộ ứng dụng
            }
        }

        private void ckbHienMatKhau_CheckedChanged(object sender, EventArgs e)
        {
            if (txtPass.Text == "Mật khẩu")
            {
                txtPass.UseSystemPasswordChar = false;
            }

            else
            {
                
                    txtPass.UseSystemPasswordChar = !ckbHienMatKhau.Checked;

                
            }  
           
        }

        private void txtTenDangNhap_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtPass.Focus();
            }
        }

        private void txtTenDangNhap_Enter(object sender, EventArgs e)
        {

            if (txtUser.Text == "Tên đăng nhập")
            {
                txtUser.Text = "";
                txtUser.ForeColor = Color.Black;

            }
        }

        private void txtTenDangNhap_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUser.Text))
            {
                txtUser.Text = "Tên đăng nhập";
                txtUser.ForeColor = Color.Black;
              
            }
        }

        private void txtMatKhau_Enter(object sender, EventArgs e)
        {
             if (txtPass.Text == "Mật khẩu")
            {
                txtPass.Text = "";
                txtPass.ForeColor = Color.Black; 
                txtPass.UseSystemPasswordChar = !ckbHienMatKhau.Checked;
            }
        }

        private void txtMatKhau_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPass.Text))
            {
                txtPass.Text = "Mật khẩu"; 
                txtPass.ForeColor = Color.Black;
                txtPass.UseSystemPasswordChar = false;
            }
        }
        private void txtMatKhau_TextChanged(object sender, EventArgs e)
        {
            if (txtPass.Text != "Mật khẩu" && !string.IsNullOrEmpty(txtPass.Text))
            {
               txtPass.UseSystemPasswordChar = !ckbHienMatKhau.Checked; 
              
            }

        }

        private void txtPass_KeyPress(object sender, KeyPressEventArgs e)
        {
           
        }

        private void txtPass_KeyDown(object sender, KeyEventArgs e)
        { 
            if (e.KeyCode == Keys.Enter)
            {
                btnDangNhap.PerformClick();
                e.SuppressKeyPress = true;
            }

        }
    }
}
