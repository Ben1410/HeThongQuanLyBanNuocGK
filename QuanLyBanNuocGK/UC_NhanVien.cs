using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuanLyBanNuocGK
{
    public partial class UC_NhanVien : UserControl
    {
        string chuoiKN = @"Data Source=MSI\SQLEXPRESS;Initial Catalog=Quanlynuocgiaikhat;Integrated Security=True;TrustServerCertificate=True";
        SqlConnection ketNoi;
        public UC_NhanVien()
        {
            InitializeComponent();
          
        }
        private void LoadData()
        {
            string sql = @"SELECT NV.MaNV, NV.HoTen, NV.SoDienThoai, NV.Email, NV.GioiTinh, 
                                  NV.DiaChi, NV.NgaySinh, TK.TenDangNhap 
                           FROM NhanVien NV 
                           LEFT JOIN TaiKhoan TK ON NV.MaNV = TK.MaNV   
                           WHERE NV.TrangThai != N'Đã xóa'
                           ORDER BY NV.MaNV DESC";
            ketNoi = new SqlConnection(chuoiKN);
            SqlDataAdapter da = new SqlDataAdapter(sql, ketNoi);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dgvNhanVien.DataSource = dt;
            dgvNhanVien.DefaultCellStyle.ForeColor = Color.Black;

            if (dgvNhanVien.Columns.Count > 0)
            {
                dgvNhanVien.Columns["MaNV"].HeaderText = "Mã NV";
                dgvNhanVien.Columns["HoTen"].HeaderText = "Họ Tên";
                dgvNhanVien.Columns["SoDienThoai"].HeaderText = "SĐT";
                dgvNhanVien.Columns["Email"].HeaderText = "Email";
                dgvNhanVien.Columns["GioiTinh"].HeaderText = "GT";
                dgvNhanVien.Columns["DiaChi"].HeaderText = "Địa Chỉ";
                dgvNhanVien.Columns["NgaySinh"].HeaderText = "Ngày Sinh";
                dgvNhanVien.Columns["TenDangNhap"].HeaderText = "Tên ĐN";

                dgvNhanVien.Columns["NgaySinh"].DefaultCellStyle.Format = "dd/MM/yyyy";
            }
        }

        private string TaoTenDN(string hoTen, string sdt)
        {
            if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(sdt) || sdt.Length < 3) return "";

            // Bỏ dấu tiếng Việt
            string str = hoTen.ToLower().Trim();
            string[] findText = { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ", "đ", "é", "è", "ẻ", "ẽ", "ẹ", "ê", "ế", "ề", "ể", "ễ", "ệ", "í", "ì", "ỉ", "ĩ", "ị", "ó", "ò", "ỏ", "õ", "ọ", "ô", "ố", "ồ", "ổ", "ỗ", "ộ", "ơ", "ớ", "ờ", "ở", "ỡ", "ợ", "ú", "ù", "ủ", "ũ", "ụ", "ư", "ứ", "ừ", "ử", "ữ", "ự", "ý", "ỳ", "ỷ", "ỹ", "ỵ" };
            string[] replaceText = { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "d", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "i", "i", "i", "i", "i", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "y", "y", "y", "y", "y" };
            for (int i = 0; i < findText.Length; i++) str = str.Replace(findText[i], replaceText[i]);

            // Xóa khoảng trắng và lấy 3 số cuối SĐT
            str = str.Replace(" ", "");
            string duoiSDT = sdt.Substring(sdt.Length - 3);
            return str + duoiSDT;
        }

        private void ResetForm()
        {
            txtMaNV.Text = "";
            txtHoTen.Text = "";
            txtSDT.Text = "";
            txtEmail.Text = "";
            txtDiaChi.Text = "";
            dtpNgaySinh.Value = DateTime.Now;

            cboGioiTinh.Text = "";
            txtTenDangNhap.Text = "";
            txtHoTen.Focus();
        }

        private void UC_NhanVien_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void pnlContent_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnThem_Click_1(object sender, EventArgs e)
        {

            if (!KiemTraNhapLieu(true, 0))
                return;


            string tenDN = TaoTenDN(txtHoTen.Text, txtSDT.Text);

            try
            {
                ketNoi = new SqlConnection(chuoiKN);
                if (ketNoi.State == ConnectionState.Closed) ketNoi.Open();
                SqlTransaction trans = ketNoi.BeginTransaction();

                try
                {
                    string sql = @"
                                INSERT INTO NhanVien (HoTen, NgaySinh, SoDienThoai, Email, GioiTinh, DiaChi) 
                                VALUES (@ten, @ns, @sdt, @email, @gt, @dc);
                                DECLARE @newMaNV INT = SCOPE_IDENTITY();
                                INSERT INTO TaiKhoan (TenDangNhap, MatKhau, MaNV) 
                                VALUES (@tendn, '123456', @newMaNV);
                                SELECT @newMaNV;"; 

                    SqlCommand cmd = new SqlCommand(sql, ketNoi, trans);

                    cmd.Parameters.AddWithValue("@ten", txtHoTen.Text.Trim());
                    cmd.Parameters.AddWithValue("@ns", dtpNgaySinh.Value);
                    cmd.Parameters.AddWithValue("@sdt", txtSDT.Text.Trim());
                    cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@gt", cboGioiTinh.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@dc", txtDiaChi.Text.Trim());
                    cmd.Parameters.AddWithValue("@tendn", tenDN);

                    int maNV = Convert.ToInt32(cmd.ExecuteScalar());

                    trans.Commit();

                    MessageBox.Show($"Thêm nhân viên thành công!\n\nTên đăng nhập: {tenDN}\nMật khẩu: 123456",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadData();
                    ResetForm();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Lỗi thực thi: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ketNoi.Close();
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (dgvNhanVien.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần sửa!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int maNV = Convert.ToInt32(dgvNhanVien.CurrentRow.Cells["MaNV"].Value);
            string hoTenCu = dgvNhanVien.CurrentRow.Cells["HoTen"].Value?.ToString() ?? "";
            string sdtCu = dgvNhanVien.CurrentRow.Cells["SoDienThoai"].Value?.ToString() ?? "";
            string tenDNCu = dgvNhanVien.CurrentRow.Cells["TenDangNhap"].Value?.ToString() ?? "";

            if (!KiemTraNhapLieu(true, maNV))
                return;

            DialogResult kq = MessageBox.Show("Bạn có chắc muốn cập nhật thông tin?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (kq != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection kn = new SqlConnection(chuoiKN))
                {
                    kn.Open();
                    SqlTransaction trans = kn.BeginTransaction();

                    try
                    {
                        string sqlNV = @"
                    UPDATE NhanVien 
                    SET HoTen = @ten, 
                        NgaySinh = @ns, 
                        SoDienThoai = @sdt, 
                        Email = @email, 
                        GioiTinh = @gt, 
                        DiaChi = @dc 
                    WHERE MaNV = @ma";

                        SqlCommand cmdNV = new SqlCommand(sqlNV, kn, trans);
                        cmdNV.Parameters.AddWithValue("@ma", maNV);
                        cmdNV.Parameters.AddWithValue("@ten", txtHoTen.Text.Trim());
                        cmdNV.Parameters.AddWithValue("@ns", dtpNgaySinh.Value.Date);
                        cmdNV.Parameters.AddWithValue("@sdt", txtSDT.Text.Trim());
                        cmdNV.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                        cmdNV.Parameters.AddWithValue("@gt", cboGioiTinh.SelectedItem.ToString());
                        cmdNV.Parameters.AddWithValue("@dc", txtDiaChi.Text.Trim());

                        cmdNV.ExecuteNonQuery();

                        bool hoTenThayDoi = (txtHoTen.Text.Trim() != hoTenCu);
                        bool sdtThayDoi = (txtSDT.Text.Trim() != sdtCu);

                        if (hoTenThayDoi || sdtThayDoi)
                        {
                            // Tạo tên đăng nhập MỚI
                            string tenDNMoi = TaoTenDN(txtHoTen.Text, txtSDT.Text);

                            // Cập nhật tên đăng nhập trong bảng TaiKhoan
                            string sqlTK = "UPDATE TaiKhoan SET TenDangNhap = @tendn WHERE MaNV = @ma";
                            SqlCommand cmdTK = new SqlCommand(sqlTK, kn, trans);
                            cmdTK.Parameters.AddWithValue("@tendn", tenDNMoi);
                            cmdTK.Parameters.AddWithValue("@ma", maNV);
                            cmdTK.ExecuteNonQuery();

                            // Thông báo chi tiết
                            string thongBao = $"✅ Cập nhật thành công!\n\n";
                            thongBao += $"📛 Họ tên cũ: {hoTenCu}\n";
                            thongBao += $"📛 Họ tên mới: {txtHoTen.Text.Trim()}\n";
                            thongBao += $"📞 SĐT cũ: {sdtCu}\n";
                            thongBao += $"📞 SĐT mới: {txtSDT.Text.Trim()}\n\n";
                            thongBao += $"🔐 Tên đăng nhập CŨ: {tenDNCu}\n";
                            thongBao += $"🔐 Tên đăng nhập MỚI: {tenDNMoi}\n";
                            thongBao += $"🔑 Mật khẩu vẫn giữ nguyên: 123456";

                            MessageBox.Show(thongBao, "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("✅ Cập nhật thành công!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        trans.Commit();
                        LoadData();
                        ResetForm();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show("❌ Lỗi cập nhật: " + ex.Message, "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi kết nối: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dgvNhanVien.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần xóa!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Lấy thông tin nhân viên
            int maNV = Convert.ToInt32(dgvNhanVien.CurrentRow.Cells["MaNV"].Value);
            string hoTen = dgvNhanVien.CurrentRow.Cells["HoTen"].Value?.ToString() ?? "";

            DialogResult kq = MessageBox.Show($"Bạn có chắc muốn xóa nhân viên {hoTen}?\n\nNhân viên sẽ được chuyển vào thùng rác và không hiển thị trong danh sách.",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (kq == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection kn = new SqlConnection(chuoiKN))
                    {
                        kn.Open();

                        // Cập nhật trạng thái thành "Đã xóa"
                        string sql = @"UPDATE NhanVien 
                              SET TrangThai = N'Đã xóa' 
                              WHERE MaNV = @ma";

                        SqlCommand cmd = new SqlCommand(sql, kn);
                        cmd.Parameters.AddWithValue("@ma", maNV);
                        int kqXoa = cmd.ExecuteNonQuery();

                        if (kqXoa > 0)
                        {
                            MessageBox.Show($"Đã xóa nhân viên {hoTen} thành công!",
                                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Load lại dữ liệu (tự động không hiển thị nhân viên vừa xóa)
                            LoadData();

                            // Reset form
                           ResetForm();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        

        private void button1_Click(object sender, EventArgs e)
        {
            ResetForm();
        }

        private void dgvNhanVien_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvNhanVien.Rows[e.RowIndex];
                txtMaNV.Text = row.Cells["MaNV"].Value?.ToString() ?? "";
                txtTenDangNhap.Text = row.Cells["TenDangNhap"].Value?.ToString() ?? "";
                txtHoTen.Text = row.Cells["HoTen"].Value?.ToString() ?? "";
                txtSDT.Text = row.Cells["SoDienThoai"].Value?.ToString() ?? "";
                txtEmail.Text = row.Cells["Email"].Value?.ToString() ?? "";
                txtDiaChi.Text = row.Cells["DiaChi"].Value?.ToString() ?? "";
                string gt = row.Cells["GioiTinh"].Value?.ToString() ?? "";

                // Reset ComboBox trước
                cboGioiTinh.SelectedIndex = -1;
   

                // Chọn giá trị tương ứng
                if (!string.IsNullOrEmpty(gt))
                {
                    if (gt == "Nam")
                    {
                        cboGioiTinh.SelectedIndex = 0;
                    }
                    else if (gt == "Nữ")
                    {
                        cboGioiTinh.SelectedIndex = 1;
                    }
                }

                if (row.Cells["NgaySinh"].Value != null && row.Cells["NgaySinh"].Value != DBNull.Value)
                    dtpNgaySinh.Value = Convert.ToDateTime(row.Cells["NgaySinh"].Value);
                dgvNhanVien.Refresh();
            }
        }
        // ===== KIỂM TRA SĐT ĐÃ TỒN TẠI CHƯA =====
        private bool KiemTraTrungSDT(string sdt, int maNVHienTai = 0)
        {
            try
            {
                using (SqlConnection kn = new SqlConnection(chuoiKN))
                {
                    kn.Open();

                    string sql = "SELECT COUNT(*) FROM NhanVien WHERE SoDienThoai = @sdt";
                    if (maNVHienTai > 0)
                    {
                        sql += " AND MaNV != @maNV";
                    }

                    SqlCommand cmd = new SqlCommand(sql, kn);
                    cmd.Parameters.AddWithValue("@sdt", sdt);
                    if (maNVHienTai > 0)
                    {
                        cmd.Parameters.AddWithValue("@maNV", maNVHienTai);
                    }

                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kiểm tra SĐT: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        private bool KiemTraNhapLieu(bool kiemTraTrung = true, int maNVHienTai = 0)
        {
            if (string.IsNullOrWhiteSpace(txtHoTen.Text))
            {
                MessageBox.Show("Vui lòng nhập Họ tên nhân viên!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHoTen.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtSDT.Text))
            {
                MessageBox.Show("Vui lòng nhập Số điện thoại!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSDT.Focus();
                return false;
            }

            if (txtSDT.Text.Length < 10)
            {
                MessageBox.Show("Số điện thoại phải có ít nhất 10 số!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSDT.Focus();
                return false;
            }

            foreach (char c in txtSDT.Text)
            {
                if (!char.IsDigit(c))
                {
                    MessageBox.Show("Số điện thoại chỉ được chứa ký tự số!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtSDT.Focus();
                    return false;
                }
            }

            if (kiemTraTrung && KiemTraTrungSDT(txtSDT.Text, maNVHienTai))
            {
                MessageBox.Show($"Số điện thoại {txtSDT.Text} đã được sử dụng!\nVui lòng nhập số khác.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSDT.Focus();
                txtSDT.SelectAll();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                if (!txtEmail.Text.Contains("@") || !txtEmail.Text.Contains("."))
                {
                    MessageBox.Show("Email không hợp lệ! (phải có @ và dấu .)", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return false;
                }
            }

            if (cboGioiTinh.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn Giới tính!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboGioiTinh.Focus();
                return false;
            }

            return true;
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            ResetForm();

            txtTimKiem.Text = "";

            LoadData();

            txtHoTen.Focus();

        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection kn = new SqlConnection(chuoiKN))
                {
                    string sql = @"SELECT NV.MaNV, NV.HoTen, NV.SoDienThoai, NV.Email, NV.GioiTinh, 
                                  NV.DiaChi, NV.NgaySinh, TK.TenDangNhap 
                           FROM NhanVien NV 
                           LEFT JOIN TaiKhoan TK ON NV.MaNV = TK.MaNV 
                           WHERE NV.HoTen LIKE @find 
                              OR NV.SoDienThoai LIKE @find 
                              OR NV.Email LIKE @find";

                    SqlDataAdapter da = new SqlDataAdapter(sql, kn);
                    da.SelectCommand.Parameters.AddWithValue("@find", "%" + txtTimKiem.Text + "%");

                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvNhanVien.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi tìm kiếm: " + ex.Message);
            }
        }
    }
}
