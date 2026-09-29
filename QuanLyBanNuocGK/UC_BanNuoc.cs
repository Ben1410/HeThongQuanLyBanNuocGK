using System;
using System.Collections;
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
    public partial class UC_BanNuoc : UserControl
    {

        string chuoiKN = @"Data Source=MSI\SQLEXPRESS;Initial Catalog=Quanlynuocgiaikhat;Integrated Security=True;TrustServerCertificate=True";
        SqlConnection ketNoi;

        int maKH_HienTai = 0;     // Mặc định là Khách lẻ (MaKH = 1)
        decimal phanTramGiam = 0;  // Lưu mức giảm dựa trên hạng
        decimal tongTienSauGiam = 0;
        public UC_BanNuoc()
        {
            InitializeComponent();   
          
        }

        private void UC_BanNuoc_Load(object sender, EventArgs e)
        {   
            
            ResetKhachLe();
            LoadDanhSachSanPham();

         
        }
        private void LoadDanhSachSanPham()
        {
            try
            {
                ketNoi = new SqlConnection(chuoiKN);

                string sql = "SELECT MaSP, TenSP, LoaiSP, DonGia, TrangThai FROM SanPham";

                SqlDataAdapter da = new SqlDataAdapter(sql, ketNoi);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvDanhSachSP.DataSource = dt;

            }
            catch (Exception ex) { MessageBox.Show("Lỗi load sản phẩm: " + ex.Message); }
        }

        private void btnThemVaoGio_Click(object sender, EventArgs e)
        {
            if (dgvDanhSachSP.CurrentRow == null) return;

            // 1. Lấy thông tin sản phẩm
            string ma = dgvDanhSachSP.CurrentRow.Cells["MaSP"].Value.ToString();
            string ten = dgvDanhSachSP.CurrentRow.Cells["col_DS_TenSP"].Value.ToString();
            decimal gia = Convert.ToDecimal(dgvDanhSachSP.CurrentRow.Cells["col_DS_DonGia"].Value);
            string trangThai = "";
            if (dgvDanhSachSP.CurrentRow.Cells["TrangThai"] != null)
            {
                trangThai = dgvDanhSachSP.CurrentRow.Cells["TrangThai"].Value?.ToString() ?? "";
            }

            // 🔍 KIỂM TRA 2: Nếu sản phẩm ngừng bán thì không cho thêm
            if (trangThai == "Ngừng bán" || trangThai == "Hết hàng" || trangThai.ToLower().Contains("ngừng"))
            {
                MessageBox.Show($"❌ Sản phẩm [{ten}] đã ngừng bán!\nVui lòng chọn sản phẩm khác.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            int soLuongThem = (int)nmSoLuong.Value;

            if (soLuongThem <= 0) return;

            // 2. Kiểm tra trùng trong giỏ hàng
            bool daCoTrongGio = false;
            foreach (DataGridViewRow row in dgvGioHang.Rows)
            {
                if (row.Cells["ghMaSP"].Value != null && row.Cells["ghMaSP"].Value.ToString() == ma)
                {
                    int slCu = Convert.ToInt32(row.Cells["ghSoLuong"].Value);
                    int slMoi = slCu + soLuongThem;
                    row.Cells["ghSoLuong"].Value = slMoi;
                    row.Cells["ghThanhTien"].Value = slMoi * gia;

                    daCoTrongGio = true;
                    break;
                }
            }

            // 3. Nếu mới thì thêm dòng
            if (!daCoTrongGio)
            {
                dgvGioHang.Rows.Add(ma, ten, soLuongThem, gia, soLuongThem * gia);
            }

            TinhTongTien();
        }
        private void TinhTongTien()
        {
            decimal tamTinh = 0;
            foreach (DataGridViewRow row in dgvGioHang.Rows)
            {
                if (row.Cells["ghThanhTien"].Value != null)
                    tamTinh += Convert.ToDecimal(row.Cells["ghThanhTien"].Value);
            }

            decimal tienGiam = tamTinh * phanTramGiam;
            tongTienSauGiam = tamTinh - tienGiam;

            lblTamTinh.Text = tamTinh.ToString("N0") + " VNĐ";
            lblTienGiam.Text = tienGiam.ToString("N0") + " VNĐ";
            lblTongTien.Text = tongTienSauGiam.ToString("N0") + " VNĐ";
           
        }


        private void btnXoaGioHang_Click(object sender, EventArgs e)
        {
            if (dgvGioHang.SelectedRows.Count > 0)
            {
                // Duyệt ngược để xóa (phòng trường hợp chọn nhiều dòng)
                foreach (DataGridViewRow row in dgvGioHang.SelectedRows)
                {
                    if (!row.IsNewRow)
                    {
                        dgvGioHang.Rows.Remove(row);
                    }
                }

                // Tính lại tiền ngay sau khi xóa
                TinhTongTien();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn món cần xóa trong giỏ hàng!");
            }
        }

        private void dgvGioHang_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow rowGioHang = dgvGioHang.Rows[e.RowIndex];
            string tenMon = rowGioHang.Cells["ghTenSP"].Value.ToString();
            int slTrongGio = Convert.ToInt32(rowGioHang.Cells["ghSoLuong"].Value);
            decimal donGia = Convert.ToDecimal(rowGioHang.Cells["ghDonGia"].Value);

            FormTachMon f = new FormTachMon(tenMon, slTrongGio);
            if (f.ShowDialog() == DialogResult.OK || f.IsXacNhan)
            {
                int soLuongTach = f.SoLuongChon;
                int slConLai = slTrongGio - soLuongTach;

                if (slConLai > 0)
                {
                    rowGioHang.Cells["ghSoLuong"].Value = slConLai;
                    rowGioHang.Cells["ghThanhTien"].Value = slConLai * donGia;
                }
                else
                {
                    dgvGioHang.Rows.RemoveAt(e.RowIndex);
                }
                TinhTongTien();
            }
        }

        private void TinhTienThoi()
        {
            try
            {
                // 1. Lấy tổng tiền từ Label (Bóc vỏ dấu chấm và VNĐ)
                string sTongTien = lblTongTien.Text.Replace(".", "").Replace("VNĐ", "").Trim();
                decimal tongTien = decimal.TryParse(sTongTien, out decimal dtp) ? dtp : 0;

                // 2. Lấy tiền khách đưa từ TextBox (Bóc vỏ dấu chấm và VNĐ)
                string sKhachDua = txtTienKhachDua.Text.Replace(".", "").Replace("VNĐ", "").Trim();
                decimal khachDua = decimal.TryParse(sKhachDua, out decimal dkd) ? dkd : 0;

                // 3. Tính toán
                decimal tienThoi = khachDua - tongTien;

                // 4. Hiển thị kết quả (Gói lại vỏ dấu chấm và VNĐ)
                if (tienThoi >= 0)
                {
                    lblTienThoi.Text = tienThoi.ToString("N0").Replace(",", ".") + " VNĐ";
                    lblThongBao.Text = "";
                }
                else
                {
                    lblTienThoi.Text = "0 VNĐ";
                    lblThongBao.Text = "Chưa đủ tiền!";
                }
            }
            catch
            {
                lblTienThoi.Text = "0 VNĐ";
            }
        }

        private void txtTienKhachDua_TextChanged(object sender, EventArgs e)
        {
            txtTienKhachDua.TextChanged -= txtTienKhachDua_TextChanged;

            try
            {
                // 2. Lấy con số thuần túy (loại bỏ dấu chấm người dùng có thể gõ nhầm)
                string content = txtTienKhachDua.Text.Replace(".", "").Trim();

                if (long.TryParse(content, out long value))
                {
                    // 3. Định dạng lại thành số có dấu chấm (Ví dụ: 10000 -> 10.000)
                    txtTienKhachDua.Text = value.ToString("N0", new System.Globalization.CultureInfo("is-IS"));

                    // 4. Đưa con trỏ về cuối ô để nhập tiếp không bị gián đoạn
                    txtTienKhachDua.SelectionStart = txtTienKhachDua.Text.Length;
                }
                else if (string.IsNullOrEmpty(content))
                {
                    txtTienKhachDua.Text = "";
                }
            }
            catch { }

            // 5. Đăng ký lại sự kiện
            txtTienKhachDua.TextChanged += txtTienKhachDua_TextChanged;

            // 6. Gọi hàm tính tiền thối
            TinhTienThoi();
        }

        private void btnHuyDon_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có muốn xóa toàn bộ giỏ hàng?", "Xác nhận",
        MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                dgvGioHang.Rows.Clear();
                lblTongTien.Text = "0 VNĐ";
                txtTienKhachDua.Clear();
                lblTienThoi.Text = "0 VNĐ";
                lblTamTinh.Text = "0 VNĐ";
                lblThongBaoGiamGia.Text = "0 VNĐ";
                txtGhiChu.Clear();
                lblTienGiam.Text = "0 VNĐ";
                dgvDanhSachSP.ClearSelection();
            }
        }
        private void ResetKhachLe()
        {
            // 1. Đưa mã khách về mặc định (ID = 1 là ID của Khách lẻ trong SQL)
            maKH_HienTai = 0;

            // 2. Đưa mức giảm giá về 0%
            phanTramGiam = 0;

            // 3. Xóa thông tin trên giao diện để nhân viên biết là đang ở chế độ mặc định
            lblThongTinKH.Text = " ";
            txtSDTKhach.Clear(); // Xóa SĐT cũ
            lblThongTinKH.Text = "CHẾ ĐỘ: KHÁCH LẺ";
            lblTienThoi.Text = "0 VNĐ";
            lblTamTinh.Text = "0 VNĐ";
            lblTongTien.Text = "0 VNĐ";
            lblTienGiam.Text = "0 VNĐ";
            cbxHTTT.Text = "Tiền mặt";
            lblThongBaoGiamGia.Text = " ";
            dgvDanhSachSP.ClearSelection();
            // 4. Tính lại tổng tiền để đảm bảo 0% giảm giá được áp dụng ngay lập tức
            TinhTongTien();
        }
        private void btnCheckHoiVien_Click(object sender, EventArgs e)
        {
            try
            {
                if (ketNoi.State == ConnectionState.Closed) ketNoi.Open();
                string sql = "SELECT MaKH, TenKH, DiemTichLuy, HangThanhVien, TrangThai FROM KhachHang WHERE SoDienThoai = @sdt";
                SqlCommand cmd = new SqlCommand(sql, ketNoi);
                cmd.Parameters.AddWithValue("@sdt", txtSDTKhach.Text.Trim());

                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    string trangThaiKH = dr["TrangThai"].ToString();
                    if (trangThaiKH == "Khóa" || trangThaiKH == "1")
                    {
                        MessageBox.Show("Tài khoản khách hàng này đã bị khóa! Không thể áp dụng ưu đãi.", "Thông báo");
                        maKH_HienTai = (int)dr["MaKH"];
                        phanTramGiam = 0; // Ép về 0%
                        lblThongTinKH.Text = $"KHÁCH HÀNG: {dr["TenKH"].ToString().ToUpper()}\nTRẠNG THÁI: TÀI KHOẢN ĐANG BỊ KHÓA";
                        lblThongBaoGiamGia.Text = "Không áp dụng giảm giá cho tài khoản bị khóa";
                        dr.Close(); // Đóng dr trước khi gọi hàm khác
                        TinhTongTien();
                        return; // Thoát ra, không chạy đoạn tính giảm giá bên dưới
                    }
                    maKH_HienTai = (int)dr["MaKH"];
                    string hang = dr["HangThanhVien"].ToString();
                    string ten = dr["TenKH"].ToString();
                    int diem = Convert.ToInt32(dr["DiemTichLuy"]);
     
                    // Gán mức giảm dựa trên hạng
                    switch (hang)
                    {
                        case "Kim Cương": phanTramGiam = 0.20m; break;
                        case "Vàng": phanTramGiam = 0.15m; break;
                        case "Bạc": phanTramGiam = 0.10m; break;
                        default: phanTramGiam = 0; break;
                    }

                   lblThongTinKH.Text = $"CHÀO MỪNG: {ten.ToUpper()}\n" +
                                              $"HẠNG: {hang}\n" +
                                              $"ĐIỂM HIỆN TẠI: {diem}đ";
                    if (hang == "Kim Cương")
                    {
                        phanTramGiam = 0.20m;
                        lblThongBaoGiamGia.Text = "Ưu đãi Kim Cương: Giảm 20%";

                    }
                    else if (hang == "Vàng")
                    {
                        phanTramGiam = 0.15m;
                        lblThongBaoGiamGia.Text = "Ưu đãi Vàng: Giảm 15%";

                    }
                    else if (hang == "Bạc")
                    {
                        phanTramGiam = 0.10m;
                        lblThongBaoGiamGia.Text = "Ưu đãi Bạc: Giảm 10%";

                    }
                    else if (hang == "Đồng")
                    {
                        phanTramGiam = 0; // Hạng đồng không giảm
                        lblThongBaoGiamGia.Text = "Ưu đãi Đồng: Giảm 0%";

                    }
                    TinhTongTien();
                }
                else
                {
                    MessageBox.Show("Khách hàng này chưa đăng ký hội viên!");
                    ResetKhachLe();

                }
                dr.Close();
                
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            finally { ketNoi.Close(); }

        }

        private void btnThanhToan_Click(object sender, EventArgs e)
        {
            if (dgvGioHang.Rows.Count == 0)
            {
                MessageBox.Show("Giỏ hàng trống!");
                return;
            }
            if (string.IsNullOrEmpty(txtTienKhachDua.Text))
            {
                MessageBox.Show("Vui lòng nhập tiền khách đưa!");
                return;
            }
            using (SqlConnection conn = new SqlConnection(chuoiKN))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    decimal tamTinh = 0;
                    foreach (DataGridViewRow row in dgvGioHang.Rows)
                    {
                        if (row.Cells["ghThanhTien"].Value != null)
                            tamTinh += Convert.ToDecimal(row.Cells["ghThanhTien"].Value);
                    }
                    decimal soTienGiam = tamTinh * phanTramGiam;
                    // 1. Lưu Hóa Đơn
                    string sqlHD = @"INSERT INTO HoaDon (NgayBan, MaNV, MaKH, TongTien, GiamGia, HTTT) 
                                     VALUES (GETDATE(), @maNV, @maKH, @tong, @giam, @httt); 
                                     SELECT SCOPE_IDENTITY();";
                    SqlCommand cmdHD = new SqlCommand(sqlHD, conn, trans);
                    cmdHD.Parameters.AddWithValue("@maNV", BienDungChung.MaNV);
                    cmdHD.Parameters.AddWithValue("@maKH", maKH_HienTai);
                    cmdHD.Parameters.AddWithValue("@tong", tongTienSauGiam);
                    cmdHD.Parameters.AddWithValue("@giam", soTienGiam);
                    cmdHD.Parameters.AddWithValue("@httt", cbxHTTT.Text);

                    int maHDVuaTao = Convert.ToInt32(cmdHD.ExecuteScalar());

                    // 2. Lưu Chi Tiết Hóa Đơn
                    foreach (DataGridViewRow row in dgvGioHang.Rows)
                    {
                        if (row.Cells["ghMaSP"].Value != null)
                        {
                            string sqlCT = "INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia) VALUES (@maHD, @maSP, @sl, @gia)";
                            SqlCommand cmdCT = new SqlCommand(sqlCT, conn, trans);
                            cmdCT.Parameters.AddWithValue("@maHD", maHDVuaTao);
                            cmdCT.Parameters.AddWithValue("@maSP", row.Cells["ghMaSP"].Value);
                            cmdCT.Parameters.AddWithValue("@sl", row.Cells["ghSoLuong"].Value);
                            cmdCT.Parameters.AddWithValue("@gia", row.Cells["ghDonGia"].Value);
                            cmdCT.ExecuteNonQuery();
                        }
                    }

                    // 3. Tích điểm hội viên
                    if (maKH_HienTai != 0)
                    {
                        int diemCong = (int)(tongTienSauGiam / 10000);
                        if (diemCong > 0)
                        {
                            string sqlDiem = "UPDATE KhachHang SET DiemTichLuy = DiemTichLuy + @diem WHERE MaKH = @maKH";
                            SqlCommand cmdDiem = new SqlCommand(sqlDiem, conn, trans);
                            cmdDiem.Parameters.AddWithValue("@diem", diemCong);
                            cmdDiem.Parameters.AddWithValue("@maKH", maKH_HienTai);
                            cmdDiem.ExecuteNonQuery();
                        }
                    }

                    trans.Commit();
                    MessageBox.Show("Thanh toán thành công!");

                    dgvGioHang.Rows.Clear();
                    
                    txtTienKhachDua.Clear();
                    ResetKhachLe();
                    dgvDanhSachSP.ClearSelection();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Lỗi thanh toán: " + ex.Message);
                }
            }
        }

        private void lblTongTien_Click(object sender, EventArgs e)
        {

        }

        private void btnThemHoiVien_Click(object sender, EventArgs e)
        {
            // 1. Kiểm tra nhập liệu
            string hoTen = txtHoTenKH.Text.Trim();
            string sdt = txtSDTKhachHang.Text.Trim();

            if (string.IsNullOrEmpty(hoTen) || string.IsNullOrEmpty(sdt))
            {
                MessageBox.Show("Vui lòng nhập đủ Tên và SĐT mới thêm hội viên được nhé!");
                return;
            }

            // 2. Kết nối và thêm vào CSDL
            using (SqlConnection conn = new SqlConnection(chuoiKN))
            {
                try
                {
                    conn.Open();

                    // Kiểm tra xem số điện thoại này đã tồn tại chưa để tránh trùng
                    string checkSql = "SELECT COUNT(*) FROM KhachHang WHERE SoDienThoai = @sdt";
                    SqlCommand cmdCheck = new SqlCommand(checkSql, conn);
                    cmdCheck.Parameters.AddWithValue("@sdt", sdt);
                    int count = (int)cmdCheck.ExecuteScalar();

                    if (count > 0)
                    {
                        MessageBox.Show("Số điện thoại này đã là hội viên rồi!");
                        return;
                    }

                    // Thực hiện thêm mới
                    string sql = "INSERT INTO KhachHang (TenKH, SoDienThoai, DiemTichLuy, HangThanhVien) " +
                                 "VALUES (@ten, @sdt, 0, N'Đồng'); SELECT SCOPE_IDENTITY();";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@ten", hoTen);
                    cmd.Parameters.AddWithValue("@sdt", sdt);

                    // Lấy luôn mã khách hàng vừa tạo để áp dụng giảm giá ngay cho đơn hàng này
                    maKH_HienTai = Convert.ToInt32(cmd.ExecuteScalar());

                    MessageBox.Show("Chúc mừng! Đã thêm hội viên mới thành công.");

                    // 3. Cập nhật giao diện bán hàng ngay lập tức
                    phanTramGiam = 0;
                    lblThongTinKH.Text = $"Khách: {hoTen}\n" + $"Hạng: Đồng (0đ)"; 


                    TinhTongTien(); // Tính lại tiền
                    lblThongBaoGiamGia.Text = "Ưu đãi Đồng: Giảm giá 0%";
                    txtHoTenKH.Clear();
                    txtSDTKhachHang.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi thêm hội viên: " + ex.Message);
                }
            }
        }

        private void ptbLamMoi_Click(object sender, EventArgs e)
        {
            txtHoTenKH.Clear();
            txtSDTKhachHang.Clear();
            txtHoTenKH.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
     
            dgvDanhSachSP.ClearSelection();
            dgvGioHang.ClearSelection();
            nmSoLuong.Value = 1;
            LoadDanhSachSanPham();

            int soMonDaXoa = KiemTraVaXoaMonNgungBan();

            nmSoLuong.Value = 1;

            if (soMonDaXoa > 0)
            {
                MessageBox.Show($"Đã xóa {soMonDaXoa} món đã ngừng bán khỏi giỏ hàng!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            TinhTongTien();
        }
        private int KiemTraVaXoaMonNgungBan()
        {
            int soMonDaXoa = 0;

            for (int i = dgvGioHang.Rows.Count - 1; i >= 0; i--)
            {
                DataGridViewRow row = dgvGioHang.Rows[i];

                if (row.Cells["ghMaSP"].Value != null)
                {
                    int maSP = Convert.ToInt32(row.Cells["ghMaSP"].Value);

                    if (!KiemTraSanPhamConBan(maSP))
                    {
                        dgvGioHang.Rows.RemoveAt(i);
                        soMonDaXoa++;
                    }
                }
            }

            return soMonDaXoa;
        }

        private bool KiemTraSanPhamConBan(int maSP)
        {
            try
            {
                using (SqlConnection kn = new SqlConnection(chuoiKN))
                {
                    kn.Open();

                    string sql = "SELECT COUNT(*) FROM SanPham WHERE MaSP = @ma AND TrangThai = N'Đang bán'";

                    SqlCommand cmd = new SqlCommand(sql, kn);
                    cmd.Parameters.AddWithValue("@ma", maSP);

                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
            catch
            {
                return false;
            }
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
                txtTimKiem.ForeColor = Color.Black;
                LoadDanhSachSanPham();
            }
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgvDanhSachSP.DataSource;

            if (dt != null)
            {
                dt.DefaultView.RowFilter = string.Format("TenSP LIKE '%{0}%' OR LoaiSP LIKE '%{0}%'", txtTimKiem.Text.Trim());
            }
        }
    }
}