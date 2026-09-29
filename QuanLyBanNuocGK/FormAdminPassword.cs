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
    public partial class FormAdminPassword : Form
    {
        public bool IsAuthenticated { get; private set; } = false;
        public string PasswordEntered { get; set; }
        public FormAdminPassword()
        {
            InitializeComponent();
        }
        private bool CheckAdminPasswordFromDB(string pass)
        {
     
            string connString = @"Data Source=MSI\SQLEXPRESS;Initial Catalog=Quanlynuocgiaikhat;Integrated Security=True;TrustServerCertificate=True";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM MKAdmin WHERE MatKhauAdmin = @pass";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@pass", pass);

                    int count = (int)cmd.ExecuteScalar(); 

                    return count > 0; 
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi kết nối DB: " + ex.Message);
                    return false;
                }
            }
        }
        private void FormAdminPassword_Load(object sender, EventArgs e)
        {
            
        }
       

       



        private void btnXacNhan_Click_1(object sender, EventArgs e)
        {
            string matKhauNhap = txtPassword.Text;

            if (CheckAdminPasswordFromDB(matKhauNhap))
            {
                IsAuthenticated = true;
                this.DialogResult = DialogResult.OK;
                this.Close(); // Đóng form mật khẩu lại
            }
            else
            {
                MessageBox.Show("Mật khẩu Admin không chính xác!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Clear();
                txtPassword.Focus();

            }
        }

        private void btnHuy_Click_1(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
    }

