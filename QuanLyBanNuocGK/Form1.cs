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
    public partial class Form1 : Form
    {

        private UC_BanNuoc UcBanNuoc;

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(BienDungChung.HoTenNhanVien))
            {
                FormDangNhap formDN = new FormDangNhap();
                this.Hide();

                if (formDN.ShowDialog() == DialogResult.OK)
                {
                    this.Show();
                    HienThiTheoQuyen();

                }
                else
                {
                    Application.Exit();
                }
            }
            else
            {
                HienThiTheoQuyen();
            }

                    // Hiển thị tên nhân viên
            lblNhanVien.Text = "👤 Chào, " + BienDungChung.HoTenNhanVien;
            lblQuyen.Text = "Vai trò: " + BienDungChung.Quyen;
            // Mặc định mở form bán nước
            LoadUserControlBanNuoc();

        }
        private void HienThiTheoQuyen()
        {
            bool laAdmin = (BienDungChung.Quyen == "Admin");

            // Admin thấy tất cả
            btnNhanVien.Visible = laAdmin;
            btnSanPham.Visible = laAdmin;
            btnKhachHang.Visible = laAdmin;
            btnHoaDon.Visible = laAdmin;
            btnThongKe.Visible = laAdmin;

            if (!laAdmin)
            {
                // Tìm và ẩn các nút quản lý
                foreach (Control ctrl in panelMenu.Controls)
                {
                    if (ctrl is Button btn && btn != btnBanNuoc)
                    {
                        btn.Visible = false;
                    }
                }
                Label lblThongBao = new Label();
                lblThongBao.Text = "🔒 Khu vực quản lý";
                lblThongBao.Font = new Font("Segoe UI", 12, FontStyle.Italic);
                lblThongBao.ForeColor = Color.White;
                lblThongBao.Location = new Point(btnNhanVien.Location.X, btnNhanVien.Location.Y);
                lblThongBao.Size = new Size(150, 30);
                lblThongBao.TextAlign = ContentAlignment.MiddleLeft;
                panelMenu.Controls.Add(lblThongBao);
                btnBanNuoc.FlatStyle = FlatStyle.Flat;
                btnBanNuoc.FlatAppearance.BorderSize = 0;
            }
        }
        private void btnBanNuoc_Click(object sender, EventArgs e)
        {
            pnlMainContent.Controls.Clear();
            HighlightButton(sender);
            if (UcBanNuoc == null)
            {
                UcBanNuoc = new UC_BanNuoc();
                UcBanNuoc.Dock = DockStyle.Fill;         
                btnBanNuoc.FlatStyle = FlatStyle.Flat;
                btnBanNuoc.FlatAppearance.BorderSize = 0;
            }

            if (!pnlMainContent.Controls.Contains(UcBanNuoc))
            {
                pnlMainContent.Controls.Clear();
                pnlMainContent.Controls.Add(UcBanNuoc);

            }

            UcBanNuoc.BringToFront();
            UcBanNuoc.Dock = DockStyle.Fill;
            pnlMainContent.Controls.Add(UcBanNuoc);

   
        }
        private void btnNhanVien_Click(object sender, EventArgs e)
        {
   
                    pnlMainContent.Controls.Clear();

                    UC_NhanVien uc = new UC_NhanVien();
                    uc.Dock = DockStyle.Fill;
                    pnlMainContent.Controls.Add(uc);
                    uc.BringToFront();
                    HighlightButton(sender);

        }

        private void btnSanPham_Click(object sender, EventArgs e)
        {
                    pnlMainContent.Controls.Clear();

                    UC_SanPham uc = new UC_SanPham();
                    uc.Dock = DockStyle.Fill;
                    pnlMainContent.Controls.Add(uc);
                    uc.BringToFront();
                    HighlightButton(sender);
        }
        private void HighlightButton(object sender)
        {
            // Duyệt qua tất cả các control trong panelMenu
            foreach (Control ctrl in panelMenu.Controls)
            {
                // Chỉ xử lý nếu nó là nút bấm (Button)
                if (ctrl is Button btn)
                {
                    // Trả về màu mặc định (Nền xanh, chữ trắng)
                    btn.BackColor = Color.FromArgb(0, 120, 215); // Màu xanh Menu
                    btn.ForeColor = Color.White;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                }
            }

            // Riêng cái nút vừa được nhấn (sender) thì đổi ngược lại
            Button btnClick = (Button)sender;
                btnClick.ForeColor = Color.FromArgb(0, 120, 215);
                btnClick.BackColor = Color.White;
            btnBanNuoc.FlatStyle = FlatStyle.Flat;
            btnBanNuoc.FlatAppearance.BorderSize = 0;

        }

        private void btnKhachHang_Click(object sender, EventArgs e)
        {

           
                    pnlMainContent.Controls.Clear();

                    UC_KhachHang uc = new UC_KhachHang();
                    uc.Dock = DockStyle.Fill;
                    pnlMainContent.Controls.Add(uc);
                    uc.BringToFront();
                    HighlightButton(sender);
              
            

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
           /* FormDangNhap f = new FormDangNhap();

            // Dòng quan trọng nhất: ShowDialog sẽ chặn mọi thao tác dưới Form Chính 
            // cho đến khi Form Đăng nhập này được đóng lại.

            if (f.ShowDialog() != DialogResult.OK)
            {
                Application.Exit();
                return; // Dừng luôn không chạy code bên dưới nữa
            }
            lblNhanVien.Text = "Chào, " + BienDungChung.HoTenNhanVien;
            LoadUserControlBanNuoc();
*/
        }
        private void LoadUserControlBanNuoc()
        {
            HighlightButton(btnBanNuoc);

            if (UcBanNuoc == null)
            {
                UcBanNuoc = new UC_BanNuoc();
                UcBanNuoc.Dock = DockStyle.Fill;


            }

            pnlMainContent.Controls.Clear();
            pnlMainContent.Controls.Add(UcBanNuoc);
            UcBanNuoc.BringToFront();               
        }

        private void btnHoaDon_Click(object sender, EventArgs e)
        {
                    pnlMainContent.Controls.Clear();

                    UC_HoaDon uc = new UC_HoaDon();
                    uc.Dock = DockStyle.Fill;
                    pnlMainContent.Controls.Add(uc);
                    uc.BringToFront();
                    HighlightButton(sender);
                
            
        }

        private void btnThongKe_Click(object sender, EventArgs e)
        {
           
                    pnlMainContent.Controls.Clear();

                    UC_ThongKe uc = new UC_ThongKe();
                    uc.Dock = DockStyle.Fill;
                    pnlMainContent.Controls.Add(uc);
                    uc.BringToFront();
                    HighlightButton(sender);
                
            
        }

        private void btnDangXuat_Click(object sender, EventArgs e)
        {
            // Hỏi xác nhận trước khi đăng xuất
            
        }

        private void lblDangXuat_Click(object sender, EventArgs e)
        {
                DialogResult result = MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Xóa thông tin phiên làm việc
                BienDungChung.MaNV = "";
                BienDungChung.HoTenNhanVien = "";
                BienDungChung.TenDangNhap = "";

                // Mở lại form đăng nhập
                FormDangNhap formDN = new FormDangNhap();

                // Ẩn form hiện tại
                this.Hide();

                // Hiển thị form đăng nhập và chờ đăng nhập lại
                if (formDN.ShowDialog() == DialogResult.OK)
                {
                    // Nếu đăng nhập thành công, hiển thị lại form chính
                    this.Show();

                    lblNhanVien.Text = "Chào, " + BienDungChung.HoTenNhanVien;
                    lblQuyen.Text = "Vai trò: " + BienDungChung.Quyen;
                    LoadUserControlBanNuoc();
                    HienThiTheoQuyen();
                }
                else
                {
                    // Nếu không đăng nhập (thoát form đăng nhập) thì đóng ứng dụng
                    Application.Exit();
                }
            }
        }
    }
}
