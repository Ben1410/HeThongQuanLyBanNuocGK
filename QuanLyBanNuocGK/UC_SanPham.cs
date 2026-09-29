using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuanLyBanNuocGK
{
    public partial class UC_SanPham : UserControl
    {
        string chuoiKN = @"Data Source=MSI\SQLEXPRESS;Initial Catalog=Quanlynuocgiaikhat;Integrated Security=True;TrustServerCertificate=True";
        SqlConnection ketNoi;
        public UC_SanPham()
        {
            InitializeComponent();
            if (txtDonGia != null) txtDonGia.Text = "0";
            if (cbxLoaiSP.Items.Count > 0)
                cbxLoaiSP.SelectedIndex = 0;
        }

        private void UC_SanPham_Load(object sender, EventArgs e)
        {
            LoadDataSP();
            if (cbxLoaiSP.Items.Count == 0)
            {
                cbxLoaiSP.Items.AddRange(new string[] { "Cà phê", "Trà", "Trà sữa", "Nước ép", "Sinh tố", "Khác" });
            }
        }
        private void LoadDataSP()
        {
            try
            {
                string sql = "SELECT * FROM SanPham";
                ketNoi = new SqlConnection(chuoiKN);
                SqlDataAdapter da = new SqlDataAdapter(sql, ketNoi);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvSanPham.DataSource = dt;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi load dữ liệu: " + ex.Message); }
        }




        private void btnThem_Click(object sender, EventArgs e)
        {
            // Kiểm tra các ô có bị trống không
            if (!KiemTraNhapLieu()) return;

            // Kiểm tra trùng tên
            if (KiemTraTrungTen(txtTenSP.Text.Trim()))
            {
                MessageBox.Show("Sản phẩm này đã tồn tại!");
                return;
            }

            // Thực hiện lưu vào SQL
            try
            {
                if (ketNoi.State == ConnectionState.Closed) ketNoi.Open();
                string sql = "INSERT INTO SanPham (TenSP, LoaiSP, DonGia) VALUES (@ten, @loai, @gia)";
                SqlCommand cmd = new SqlCommand(sql, ketNoi);

                cmd.Parameters.AddWithValue("@ten", txtTenSP.Text.Trim());
                cmd.Parameters.AddWithValue("@loai", cbxLoaiSP.Text);
                string giaClean = System.Text.RegularExpressions.Regex.Replace(txtDonGia.Text, @"[^0-9]", "");
                cmd.Parameters.AddWithValue("@gia", decimal.Parse(giaClean));
       
                cmd.Parameters.AddWithValue("@tt", cbxTrangThaiSP.Text);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Thêm thành công!");
                LoadDataSP();
                LamMoi();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            finally { ketNoi.Close(); }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (dgvSanPham.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn món cần chỉnh sửa hoặc để mở bán lại!");
                return;
            }

            int maSP = Convert.ToInt32(dgvSanPham.SelectedRows[0].Cells["MaSP"].Value);

            if (!KiemTraNhapLieu()) return;

            try
            {
                if (ketNoi.State == ConnectionState.Closed) ketNoi.Open();

                string sql = @"UPDATE SanPham 
                       SET TenSP=@ten, LoaiSP=@loai, DonGia=@gia, TrangThai=@tt 
                       WHERE MaSP=@ma";

                SqlCommand cmd = new SqlCommand(sql, ketNoi);
                cmd.Parameters.AddWithValue("@ten", txtTenSP.Text.Trim());
                cmd.Parameters.AddWithValue("@loai", cbxLoaiSP.Text);
                string giaClean = System.Text.RegularExpressions.Regex.Replace(txtDonGia.Text, @"[^0-9]", "");
                cmd.Parameters.AddWithValue("@gia", decimal.Parse(giaClean));
                cmd.Parameters.AddWithValue("@ma", maSP);
                cmd.Parameters.AddWithValue("@tt", cbxTrangThaiSP.Text);
                cmd.ExecuteNonQuery();

                MessageBox.Show("Cập nhật và mở bán lại món thành công!", "Thông báo");

                LoadDataSP();
                LamMoi();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            finally { ketNoi.Close(); }
        }

        private void txtDonGia_TextChanged(object sender, EventArgs e)
        {
            txtDonGia.TextChanged -= txtDonGia_TextChanged;
            try
            {
                string value = System.Text.RegularExpressions.Regex.Replace(txtDonGia.Text, @"[^0-9]", "");

                if (!string.IsNullOrEmpty(value))
                {
                    double number = double.Parse(value);

                    txtDonGia.Text = string.Format(System.Globalization.CultureInfo.GetCultureInfo("vi-VN"), "{0:N0}", number);
                    txtDonGia.SelectionStart = txtDonGia.Text.Length;
                }
            }
            catch { }
            txtDonGia.TextChanged += txtDonGia_TextChanged;
        }
        private void LamMoi()
        {
            txtTenSP.Clear();
            txtDonGia.Clear();
            cbxLoaiSP.SelectedIndex = 0;
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dgvSanPham.CurrentRow == null || dgvSanPham.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một món trong danh sách để ngừng kinh doanh!",
                                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int maSP = Convert.ToInt32(dgvSanPham.CurrentRow.Cells["MaSP"].Value);
            string tenSP = dgvSanPham.CurrentRow.Cells["TenSP"].Value.ToString();
            string trangThaiHienTai = dgvSanPham.CurrentRow.Cells["TrangThai"].Value.ToString();

            if (trangThaiHienTai == "Ngừng bán")
            {
                MessageBox.Show($"Món '{tenSP}' đã ở trạng thái Ngừng bán rồi!",
                                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult dr = MessageBox.Show($"Bạn có chắc chắn muốn ngừng kinh doanh món '{tenSP}' không?",
                                              "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dr == DialogResult.Yes)
            {
                try
                {
                    if (ketNoi.State == ConnectionState.Closed) ketNoi.Open();

                    string sql = "UPDATE SanPham SET TrangThai = N'Ngừng bán' WHERE MaSP = @ma";
                    SqlCommand cmd = new SqlCommand(sql, ketNoi);
                    cmd.Parameters.AddWithValue("@ma", maSP);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show($"Đã cập nhật trạng thái Ngừng bán cho món: {tenSP}", "Thành công");
                        LoadDataSP(); // Tải lại bảng để cập nhật màu sắc/chữ trên lưới
                        LamMoi();     // Xóa trắng các ô nhập liệu
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi hệ thống: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    ketNoi.Close();
                }
            }
        }

        private void txtDonGia_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Chặn các phím khác
            }
        }

        private void dgvSanPham_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow r = dgvSanPham.Rows[e.RowIndex];
                txtTenSP.Text = r.Cells["TenSP"].Value.ToString();
                cbxLoaiSP.Text = r.Cells["LoaiSP"].Value.ToString();
                if (r.Cells["DonGia"].Value != null)
                {
                    decimal donGia = Convert.ToDecimal(r.Cells["DonGia"].Value);
                    // Gán vào Textbox, lúc này TextChanged sẽ tự động định dạng thành 10.000
                    txtDonGia.Text = donGia.ToString("0");
                }
        
                string tt = r.Cells["TrangThai"].Value?.ToString();
                if (!string.IsNullOrEmpty(tt))
                {
                    cbxTrangThaiSP.Text = tt;
                }
            }
        }
        private bool KiemTraTrungTen(string tenSP)
        {
            bool trung = false;
            try
            {
                if (ketNoi.State == ConnectionState.Closed) ketNoi.Open();

                // Đếm số lượng sản phẩm có tên giống hệt tên đang nhập
                string sql = "SELECT COUNT(*) FROM SanPham WHERE TenSP = @ten";
                SqlCommand cmd = new SqlCommand(sql, ketNoi);
                cmd.Parameters.AddWithValue("@ten", tenSP);

                int count = (int)cmd.ExecuteScalar(); // Lấy ra một con số duy nhất
                if (count > 0) trung = true; // Nếu lớn hơn 0 nghĩa là đã tồn tại
            }
            catch (Exception ex) { MessageBox.Show("Lỗi kiểm tra trùng: " + ex.Message); }
            finally { ketNoi.Close(); }

            return trung;
        }
        private bool KiemTraNhapLieu()
        {
            // 1. Kiểm tra Tên sản phẩm
            if (string.IsNullOrWhiteSpace(txtTenSP.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên sản phẩm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenSP.Focus();
                return false;
            }

            // 2. Kiểm tra Loại sản phẩm (ComboBox)
            if (cbxLoaiSP.SelectedIndex == -1 && string.IsNullOrWhiteSpace(cbxLoaiSP.Text))
            {
                MessageBox.Show("Vui lòng chọn hoặc nhập Loại sản phẩm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbxLoaiSP.Focus();
                return false;
            }

            // 3. Kiểm tra Đơn giá
            if (string.IsNullOrWhiteSpace(txtDonGia.Text))
            {
                MessageBox.Show("Vui lòng nhập Đơn giá!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDonGia.Focus();
                return false;
            }
            if (cbxTrangThaiSP.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn trạng thái sản phẩm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 5. Kiểm tra giá trị số (phòng trường hợp nhập ký tự lạ)
            try
            {
                decimal gia = decimal.Parse(txtDonGia.Text.Replace(".", ""));

                if (gia < 0)
                {
                    MessageBox.Show("Đơn giá không được là số âm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            catch
            {
                MessageBox.Show("Đơn giá không đúng định dạng số!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true; // Tất cả đều ổn
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            ClearFieldsSP();
            LoadDataSP();
            txtTimKiemSP.Clear();
            dgvSanPham.ClearSelection();
        }
        private void ClearFieldsSP()
        {
            txtTenSP.Clear();

            // Đưa giá bán về 0 hoặc trống
            txtDonGia.Text = "0";

            // Đưa ComboBox loại sản phẩm về món đầu tiên (ví dụ: Cà phê)
            if (cbxLoaiSP.Items.Count > 0)
                cbxLoaiSP.SelectedIndex = 0;
                
            // Đưa trạng thái về "Còn hàng"
            if (cbxTrangThaiSP.Items.Count > 0)
                cbxTrangThaiSP.Text = "Đang bán";

          
            txtTenSP.Focus();
        }

        private void txtTimKiemSP_Enter(object sender, EventArgs e)
        {
            if (txtTimKiemSP.Text == "Tìm kiếm...")
            {
                txtTimKiemSP.Text = "";
                txtTimKiemSP.ForeColor = Color.Black;

            }
        }

        private void txtTimKiemSP_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTimKiemSP.Text))
            {
                txtTimKiemSP.Text = "Tìm kiếm...";
                txtTimKiemSP.ForeColor = Color.Black;
                LoadDataSP();
            }
        }

        private void txtTimKiemSP_TextChanged(object sender, EventArgs e)
        {
            // Lấy bảng dữ liệu đang hiện trên Grid
            DataTable dt = (DataTable)dgvSanPham.DataSource;

            if (dt != null)
            {
                // Tạo bộ lọc: Tìm trong TenSP hoặc LoaiSP
                dt.DefaultView.RowFilter = string.Format("TenSP LIKE '%{0}%' OR LoaiSP LIKE '%{0}%'", txtTimKiemSP.Text.Trim());
            }
        }

        private void txtTimKiemSP_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}

