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
    public partial class UC_KhachHang : UserControl
    {
        string chuoiKN = @"Data Source=MSI\SQLEXPRESS;Initial Catalog=Quanlynuocgiaikhat;Integrated Security=True;TrustServerCertificate=True";

        public UC_KhachHang()
        {
            InitializeComponent();
            dgvKhachHang.AutoGenerateColumns = false;
            if (txtDiem != null) txtDiem.Text = "0";
            if (txtHang != null) txtHang.Text = "Đồng";
        }
        private void LoadDataKH()
        {
            try
            {
                using (SqlConnection kn = new SqlConnection(chuoiKN))
                {
                    string sql = "SELECT MaKH as [MaKH], TenKH as [TenKH], SoDienThoai as [SoDienThoai], " +
                                 "DiemTichLuy as [DiemTichLuy], HangThanhVien as [HangThanhVien], " +
                                 "NgayDangKy as [NgayDangKy], TrangThai as [TrangThai] " +
                                 "FROM KhachHang WHERE TrangThai != N'Đã xóa'";

                    SqlDataAdapter da = new SqlDataAdapter(sql, kn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvKhachHang.DataSource = dt;
                    dgvKhachHang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dgvKhachHang.AllowUserToAddRows = false;
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi load: " + ex.Message); }
        }

        private void UC_KhachHang_Load(object sender, EventArgs e)
        {
            LoadDataKH();
            cbxTrangThai.Items.Clear();
            cbxTrangThai.Items.Add("Hoạt động");
            cbxTrangThai.Items.Add("Khóa");
            cbxTrangThai.SelectedIndex = 0;
        }
        private bool KiemTraTrung(string sdt, string id = "")
        {
            using (SqlConnection kn = new SqlConnection(chuoiKN))
            {
                kn.Open();
                string sql = string.IsNullOrEmpty(id)
                    ? "SELECT COUNT(*) FROM KhachHang WHERE SoDienThoai = @sdt"
                    : "SELECT COUNT(*) FROM KhachHang WHERE SoDienThoai = @sdt AND MaKH != @id";

                SqlCommand cmd = new SqlCommand(sql, kn);
                cmd.Parameters.AddWithValue("@sdt", sdt);
                if (!string.IsNullOrEmpty(id)) cmd.Parameters.AddWithValue("@id", id);

                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            string ten = txtTenKH.Text.Trim();
            string sdt = txtSoDienThoai.Text.Trim();

            if (string.IsNullOrEmpty(ten) || string.IsNullOrEmpty(sdt))
            {
                MessageBox.Show("Vui lòng nhập Họ tên hoặc số điện thoại!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (sdt.Length < 10 || !long.TryParse(sdt, out _))
            {
                MessageBox.Show("Số điện thoại không hợp lệ!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (KiemTraTrung(sdt))
            {
                MessageBox.Show("Số điện thoại này đã có người đăng ký!", "Trùng dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            try
            {
                using (SqlConnection kn = new SqlConnection(chuoiKN))
                {
                    kn.Open();
                    string sql = "INSERT INTO KhachHang (TenKH, SoDienThoai) VALUES (@ten, @sdt)";
                    SqlCommand cmd = new SqlCommand(sql, kn);
                    cmd.Parameters.AddWithValue("@ten", txtTenKH.Text.Trim());
                    cmd.Parameters.AddWithValue("@sdt", txtSoDienThoai.Text.Trim());

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Thêm hội viên mới thành công!");
                    LoadDataKH();
                    ClearFields();
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            string ten = txtTenKH.Text.Trim();
            string sdt = txtSoDienThoai.Text.Trim();
            if (dgvKhachHang.CurrentRow == null) return;
            if (string.IsNullOrEmpty(ten) || string.IsNullOrEmpty(sdt))
            {
                MessageBox.Show("Vui lòng nhập Họ tên hoặc số điện thoại!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string id = dgvKhachHang.CurrentRow.Cells["MaKH"].Value.ToString();
           
            if (KiemTraTrung(sdt, id))
            {
                MessageBox.Show("Không thể sửa thành SĐT này vì đã có khách khác dùng rồi!", "Lỗi trùng", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }

            try
            {
                using (SqlConnection kn = new SqlConnection(chuoiKN))
                {
                    kn.Open();
                    string sql = "UPDATE KhachHang SET TenKH = @ten, SoDienThoai = @sdt, TrangThai = @tt WHERE MaKH = @ma";
                    SqlCommand cmd = new SqlCommand(sql, kn);
                    cmd.Parameters.AddWithValue("@ten", txtTenKH.Text.Trim());
                    cmd.Parameters.AddWithValue("@sdt", txtSoDienThoai.Text.Trim());
                    cmd.Parameters.AddWithValue("@tt", cbxTrangThai.Text);
                    cmd.Parameters.AddWithValue("@ma", id);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Cập nhật thông tin xong xuôi!", "Thông báo");
                    LoadDataKH();
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }
        private void ClearFields()
        {
            txtTenKH.Clear();
            txtSoDienThoai.Clear();

            if (txtDiem != null) txtDiem.Text = "0";
            if (txtHang != null) txtHang.Text = "Đồng";
            dtpNgayDangKy.Value = DateTime.Now;

            // Trạng thái mặc định là Hoạt động
            cbxTrangThai.SelectedIndex = 0; // Chọn dòng đầu tiên (Hoạt động)

            txtTenKH.Focus();
        }

        private void dgvKhachHang_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvKhachHang.Rows[e.RowIndex];
                txtTenKH.Text = row.Cells["TenKH"].Value.ToString();
                txtSoDienThoai.Text = row.Cells["SoDienThoai"].Value.ToString();

                dtpNgayDangKy.Text = row.Cells["NgayDangKy"].Value.ToString();

                // Đổ trạng thái vào ComboBox
                cbxTrangThai.Text = row.Cells["TrangThai"].Value.ToString();
                txtDiem.Text = row.Cells["DiemTichLuy"].Value.ToString();
                txtHang.Text = row.Cells["HangThanhVien"].Value.ToString();
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dgvKhachHang.CurrentRow == null) return;

            string id = dgvKhachHang.CurrentRow.Cells["MaKH"].Value.ToString();
            string ten = dgvKhachHang.CurrentRow.Cells["TenKH"].Value.ToString();

            DialogResult dr = MessageBox.Show($"Bạn có chắc muốn xóa khách [{ten}] không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dr == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection kn = new SqlConnection(chuoiKN))
                    {
                        kn.Open();
                        string sql = "UPDATE KhachHang SET TrangThai = N'Đã xóa' WHERE MaKH = @ma";
                        SqlCommand cmd = new SqlCommand(sql, kn);
                        cmd.Parameters.AddWithValue("@ma", id);
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Đã xóa xong!");
                        LoadDataKH();
                        ClearFields();
                    }
                }
                catch (Exception ex) { MessageBox.Show("Lỗi xóa: " + ex.Message); }
            }
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection kn = new SqlConnection(chuoiKN))
                {
                    // Tìm theo Tên hoặc SĐT
                    string sql = "SELECT MaKH, TenKH, SoDienThoai, DiemTichLuy, NgayDangKy, HangThanhVien, TrangThai " +
                                 "FROM KhachHang WHERE ( TenKH LIKE @find OR SoDienThoai LIKE @find) " +
                                 "AND TrangThai != N'Đã xóa'";
                    SqlDataAdapter da = new SqlDataAdapter(sql, kn);
                    da.SelectCommand.Parameters.AddWithValue("@find", "%" + txtTimKiem.Text + "%");
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvKhachHang.DataSource = dt;
                }

            }
            catch { }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            ClearFields();
            LoadDataKH();

        }

        private void txtTimKiem_Enter(object sender, EventArgs e)
        {
            if (txtTimKiem.Text == "Tìm kiếm...")
            {
                txtTimKiem.Text = "";
                txtTimKiem.ForeColor = Color.Black;

            }
        }

        private void txtTimKiem_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTimKiem.Text))
            {
                txtTimKiem.Text = "Tìm kiếm...";
                txtTimKiem.ForeColor = Color.Gray;
                LoadDataKH();
            }
        }
    }
}

