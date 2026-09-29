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
    public partial class UC_HoaDon : UserControl
    {
        string maHD_DangChon = "";
        string maSP_DangChon = "";
        string chuoiKN = @"Data Source=MSI\SQLEXPRESS;Initial Catalog=Quanlynuocgiaikhat;Integrated Security=True;TrustServerCertificate=True";

        public UC_HoaDon()
        {

            InitializeComponent();
            LoadComboBoxSanPham();
        }
        public void LoadHoaDon()
        {
            try
            {
                string sql = @"SELECT MaHD, NgayBan, MaNV, MaKH, TongTien, HTTT, GiamGia, 
                   TrangThai, -- Giữ lại để check logic
                   CASE WHEN TrangThai = 1 THEN N'Hoàn thành' ELSE N'Đã hủy' END AS TinhTrang, 
                   ISNULL(LyDoHuy, N'') AS LyDoHuy_HienThi -- Đặt tên khác đi để tránh trùng
                   FROM HoaDon ORDER BY NgayBan DESC";

                SqlConnection ketNoi = new SqlConnection(chuoiKN);
                SqlDataAdapter da = new SqlDataAdapter(sql, ketNoi);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvHoaDon.DataSource = dt;

                // --- Cấu hình hiển thị cột ---
                dgvHoaDon.Columns["MaHD"].HeaderText = "Mã HĐ";
                dgvHoaDon.Columns["NgayBan"].HeaderText = "Ngày Lập";
                dgvHoaDon.Columns["TongTien"].HeaderText = "Tổng Tiền";
                dgvHoaDon.Columns["TinhTrang"].HeaderText = "Trạng Thái";
                dgvHoaDon.Columns["LyDoHuy_HienThi"].HeaderText = "Lý Do Hủy";

                if (dgvHoaDon.Columns.Contains("TrangThai")) dgvHoaDon.Columns["TrangThai"].Visible = false;
                if (dgvHoaDon.Columns.Contains("LyDoHuy")) dgvHoaDon.Columns["LyDoHuy"].Visible = false;

                dgvHoaDon.Columns["TongTien"].DefaultCellStyle.Format = "N0";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load danh sách hóa đơn: " + ex.Message);
            }
        }

        private void LoadComboBoxSanPham()
        {
            try
            {
                string sql = "SELECT MaSP, TenSP FROM SanPham";
                SqlConnection ketNoi = new SqlConnection(chuoiKN);
                SqlDataAdapter da = new SqlDataAdapter(sql, ketNoi);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cbxSanPham.DataSource = dt;

                cbxSanPham.DisplayMember = "TenSP";

                cbxSanPham.ValueMember = "MaSP";

                cbxSanPham.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load sản phẩm: " + ex.Message);
            }
        }
        private void UC_HoaDon_Load(object sender, EventArgs e)
        {
            LoadHoaDon();
        }

        private void dgvHoaDon_CellClick(object sender, DataGridViewCellEventArgs e)
        {
           
            if (e.RowIndex >= 0)
            {
                try
                {
                    string maHD = dgvHoaDon.Rows[e.RowIndex].Cells["MaHD"].Value.ToString();
                    TaiChiTietHoaDon(maHD);
                    string sql = $@"SELECT c.MaHD, c.MaSP, s.TenSP, c.SoLuong, c.DonGia, c.ThanhTien 
                            FROM ChiTietHoaDon c 
                            JOIN SanPham s ON c.MaSP = s.MaSP 
                            WHERE c.MaHD = {maHD}";

                    SqlConnection ketNoi = new SqlConnection(chuoiKN);
                    SqlDataAdapter da = new SqlDataAdapter(sql, ketNoi);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvChiTiet.DataSource = dt;



                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi load chi tiết: " + ex.Message);
                }
            }
        }

        private void làmMớiDanhSáchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadHoaDon();
            dgvChiTiet.DataSource = null;
            MessageBox.Show("Đã làm mới danh sách!", "Thông báo");
        }

        private void xóaHóaĐơnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgvHoaDon.CurrentRow == null) return;

            string maHD = dgvHoaDon.CurrentRow.Cells["MaHD"].Value.ToString();
            string tinhTrang = dgvHoaDon.CurrentRow.Cells["TinhTrang"].Value.ToString();

            if (tinhTrang == "Đã hủy")
            {
                MessageBox.Show("Hóa đơn này đã hủy rồi!");
                return;
            }

            using (frmNhapLyDoHuy frm = new frmNhapLyDoHuy())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string sql = $@"UPDATE HoaDon SET TrangThai = 0, LyDoHuy = N'{frm.LyDo}' WHERE MaHD = {maHD}";

                        SqlConnection ketNoi = new SqlConnection(chuoiKN);
                        SqlCommand cmd = new SqlCommand(sql, ketNoi);
                        ketNoi.Open();
                        cmd.ExecuteNonQuery();
                        ketNoi.Close();

                        MessageBox.Show("Đã hủy hóa đơn thành công!");
                        LoadHoaDon();
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
                }
            }
        }

        private void sửaHóaĐơnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgvHoaDon.CurrentRow == null) return;

            string maHD = dgvHoaDon.CurrentRow.Cells["MaHD"].Value.ToString();
            string tinhTrang = dgvHoaDon.CurrentRow.Cells["TinhTrang"].Value.ToString();

            if (tinhTrang == "Đã hủy")
            {
                MessageBox.Show("Không sửa được hóa đơn đã hủy!", "Thông báo");
                return;
            }

            string maNV_Moi = dgvHoaDon.CurrentRow.Cells["MaNV"].Value.ToString();
            string maKH_Moi = dgvHoaDon.CurrentRow.Cells["MaKH"].Value.ToString();
            string httt_Moi = dgvHoaDon.CurrentRow.Cells["HTTT"].Value.ToString();

            DateTime ngayLap_Moi = Convert.ToDateTime(dgvHoaDon.CurrentRow.Cells["NgayBan"].Value);

            try
            {
                string sql = $@"UPDATE HoaDon 
                        SET NgayBan = '{ngayLap_Moi:yyyy-MM-dd}', 
                            MaNV = {maNV_Moi}, 
                            MaKH = {maKH_Moi}, 
                            HTTT = N'{httt_Moi}' 
                        WHERE MaHD = {maHD}";

                using (SqlConnection ketNoi = new SqlConnection(chuoiKN))
                {
                    SqlCommand cmd = new SqlCommand(sql, ketNoi);
                    ketNoi.Open();
                    cmd.ExecuteNonQuery();
                    ketNoi.Close();
                }

                MessageBox.Show("Đã cập nhật thay đổi thành công!", "Thành công");
                LoadHoaDon();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi sửa: " + ex.Message);
            }
        }

        private void btnTimLoc_Click(object sender, EventArgs e)
        {
            try
            {
                string tuNgay = dtpTuNgay.Value.ToString("yyyy-MM-dd");
                string denNgay = dtpDenNgay.Value.ToString("yyyy-MM-dd");

                string sql = $@"SELECT MaHD, NgayBan, MaNV, MaKH, TongTien, HTTT, GiamGia,
                        CASE WHEN TrangThai = 1 THEN N'Hoàn thành' ELSE N'Đã hủy' END AS TinhTrang, 
                        ISNULL(LyDoHuy, N'') AS LyDoHuy 
                        FROM HoaDon 
                        WHERE NgayBan BETWEEN '{tuNgay}' AND '{denNgay}'
                        ORDER BY NgayBan DESC";

                SqlConnection ketNoi = new SqlConnection(chuoiKN);
                SqlDataAdapter da = new SqlDataAdapter(sql, ketNoi);
                DataTable dt = new DataTable();
                da.Fill(dt);
                decimal tongDoanhThu = 0;
                foreach (DataRow row in dt.Rows)
                {
                    if (row["TinhTrang"].ToString() == "Hoàn thành")
                    {
                        tongDoanhThu += Convert.ToDecimal(row["TongTien"]);
                    }
                }
                lblTongDoanhThu.Text = "Tổng doanh thu: " + tongDoanhThu.ToString("N0") + " VNĐ";

                dgvHoaDon.DataSource = dt;

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Không có hóa đơn nào trong khoảng thời gian này!", "Thông báo");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lọc dữ liệu: " + ex.Message);
            }
        }
        private void TaiChiTietHoaDon(string maHD)
        {
            if (string.IsNullOrEmpty(maHD)) return;
            maHD_DangChon = maHD;

            try
            {
                string sql = $@"SELECT c.MaHD, c.MaSP, s.TenSP, c.SoLuong, c.DonGia, c.ThanhTien 
                        FROM ChiTietHoaDon c 
                        JOIN SanPham s ON c.MaSP = s.MaSP 
                        WHERE c.MaHD = '{maHD}'";

                SqlDataAdapter da = new SqlDataAdapter(sql, chuoiKN);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvChiTiet.DataSource = dt;

                if (!string.IsNullOrEmpty(maSP_DangChon))
                {
                    foreach (DataGridViewRow row in dgvChiTiet.Rows)
                    {
                        if (row.Cells["MaSP"].Value?.ToString() == maSP_DangChon)
                        {
                            dgvChiTiet.CurrentCell = row.Cells["SoLuong"];
                            row.Selected = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi Load: " + ex.Message); }
        }
         private void LoadLaiGiaoDienSauKhiSua(string maHD_DangSua)
        {
            LoadHoaDon();

            bool timThay = false;
            foreach (DataGridViewRow row in dgvHoaDon.Rows)
            {
                if (row.Cells["MaHD"].Value?.ToString() == maHD_DangSua)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (cell.Visible == true)
                        {
                            dgvHoaDon.CurrentCell = cell;
                            break;
                        }
                    }

                    row.Selected = true;
                    timThay = true;
                    break;
                }
            }

            if (timThay)
            {
                TaiChiTietHoaDon(maHD_DangSua);
            }
        }
        private void dgvChiTiet_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvHoaDon.CurrentRow == null || dgvChiTiet.Rows[e.RowIndex].Cells["colMaHD"].Value == null) return;

            string trangThai = dgvHoaDon.CurrentRow.Cells["TinhTrang"].Value.ToString();
            string maHD_DangSua = dgvChiTiet.Rows[e.RowIndex].Cells["colMaHD"].Value.ToString();

            if (trangThai == "Đã hủy" || trangThai == "0")
            {
                MessageBox.Show("Hóa đơn đã hủy, không thể chỉnh sửa bất kỳ thông tin nào!");
                TaiChiTietHoaDon(maHD_DangSua);
                return;
            }
            string maSP_DangSua = dgvChiTiet.Rows[e.RowIndex].Cells["MaSP"].Value.ToString();
            string tenCot = dgvChiTiet.Columns[e.ColumnIndex].Name;

            if (tenCot == "colMaHD" || tenCot == "MaHD")
            {
                TaiChiTietHoaDon(maHD_DangSua);
                return;
            }

            try
            {
                object giaTriMoi = dgvChiTiet.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                using (SqlConnection ketNoi = new SqlConnection(chuoiKN))
                {
                    ketNoi.Open();

                    string sqlUpdate = $"UPDATE ChiTietHoaDon SET {tenCot} = @val WHERE MaHD = @mahd AND MaSP = @masp";
                    SqlCommand cmd = new SqlCommand(sqlUpdate, ketNoi);
                    cmd.Parameters.AddWithValue("@val", giaTriMoi ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@mahd", maHD_DangSua);
                    cmd.Parameters.AddWithValue("@masp", maSP_DangSua);
                    cmd.ExecuteNonQuery();

                    string sqlUpHD = "UPDATE HoaDon SET TongTien = (SELECT ISNULL(SUM(ThanhTien), 0) FROM ChiTietHoaDon WHERE MaHD = @mahd) WHERE MaHD = @mahd";
                    SqlCommand cmdUpHD = new SqlCommand(sqlUpHD, ketNoi);
                    cmdUpHD.Parameters.AddWithValue("@mahd", maHD_DangSua);
                    cmdUpHD.ExecuteNonQuery();

                    ketNoi.Close();
                }

                LoadHoaDon();
                bool timThayHD = false;
                foreach (DataGridViewRow rowHD in dgvHoaDon.Rows)
                {
                    if (rowHD.Cells["MaHD"].Value?.ToString() == maHD_DangSua)
                    {
                        foreach (DataGridViewCell cellHD in rowHD.Cells)
                        {
                            if (cellHD.Visible) { dgvHoaDon.CurrentCell = cellHD; break; }
                        }
                        rowHD.Selected = true;
                        timThayHD = true;
                        break;
                    }
                }

                if (timThayHD)
                {
                    TaiChiTietHoaDon(maHD_DangSua);
                    foreach (DataGridViewRow rowCT in dgvChiTiet.Rows)
                    {
                        if (rowCT.Cells["MaSP"].Value?.ToString() == maSP_DangSua)
                        {
                            dgvChiTiet.CurrentCell = rowCT.Cells[e.ColumnIndex];
                            rowCT.Selected = true;
                            break;
                        }
                    }
                }
                dgvChiTiet.EndEdit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi sửa: " + ex.Message);
                TaiChiTietHoaDon(maHD_DangSua);
            }
        }

        private void btnXoaMon_Click(object sender, EventArgs e)
        {
            if (dgvHoaDon.CurrentRow == null) return;

            string trangThai = dgvHoaDon.CurrentRow.Cells["TinhTrang"].Value.ToString();

            if (trangThai == "Đã hủy" || trangThai == "0")
            {
                MessageBox.Show("Hóa đơn này đã hủy không thể xóa món được nữa!", "Thông báo");
                return;
            }
            if (dgvChiTiet.CurrentRow == null) return;

            string maHD = dgvChiTiet.CurrentRow.Cells["colMaHD"].Value.ToString();
            string maSP = dgvChiTiet.CurrentRow.Cells["MaSP"].Value.ToString();
            string tenSP = dgvChiTiet.CurrentRow.Cells["TenSP"].Value.ToString();

            if (MessageBox.Show($"Xóa món {tenSP} khỏi hóa đơn này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    SqlConnection ketNoi = new SqlConnection(chuoiKN);
                    ketNoi.Open();

                    string sqlDelete = $"DELETE FROM ChiTietHoaDon WHERE MaHD = {maHD} AND MaSP = {maSP}";
                    new SqlCommand(sqlDelete, ketNoi).ExecuteNonQuery();

                    string sqlUpdateSum = $@"UPDATE HoaDon 
                                     SET TongTien = ISNULL((SELECT SUM(ThanhTien) FROM ChiTietHoaDon WHERE MaHD = {maHD}), 0)
                                     WHERE MaHD = {maHD}";
                    new SqlCommand(sqlUpdateSum, ketNoi).ExecuteNonQuery();

                    ketNoi.Close();
                    LoadHoaDon();
                    TaiChiTietHoaDon(maHD);
                }
                catch (Exception ex) { MessageBox.Show("Lỗi xóa món: " + ex.Message); }
            }
        }

        private void btnThemMon_Click(object sender, EventArgs e)
        {
            string tt = dgvHoaDon.CurrentRow.Cells["TinhTrang"].Value.ToString();
            if (tt == "Đã hủy")
            {
                MessageBox.Show("Hóa đơn đã hủy không thêm được!!!", "Thông báo");
                return;
            }
            if (dgvHoaDon.CurrentRow == null)
             {
                 MessageBox.Show("Vui lòng chọn một hóa đơn ở bảng trên trước!");
                 return;
             }

             if (cbxSanPham.SelectedValue == null || numSoLuong.Value <= 0)
             {
                 MessageBox.Show("Vui lòng chọn sản phẩm và số lượng > 0!");
                 return;
             }
            
            if (dgvHoaDon.CurrentRow == null) return;
            if (cbxSanPham.SelectedValue == null || numSoLuong.Value <= 0) return;

            string maHD = dgvHoaDon.CurrentRow.Cells["MaHD"].Value.ToString();
            string maSP = cbxSanPham.SelectedValue.ToString();
            int soLuong = (int)numSoLuong.Value;

            try
            {
                using (SqlConnection ketNoi = new SqlConnection(chuoiKN))
                {
                    ketNoi.Open();

                    string sql = $@"
                IF EXISTS (SELECT 1 FROM ChiTietHoaDon WHERE MaHD = @mahd AND MaSP = @masp)
                    UPDATE ChiTietHoaDon SET SoLuong = SoLuong + @sl WHERE MaHD = @mahd AND MaSP = @masp
                ELSE
                    INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia) 
                    VALUES (@mahd, @masp, @sl, (SELECT DonGia FROM SanPham WHERE MaSP = @masp))";

                    SqlCommand cmd = new SqlCommand(sql, ketNoi);
                    cmd.Parameters.AddWithValue("@mahd", maHD);
                    cmd.Parameters.AddWithValue("@masp", maSP);
                    cmd.Parameters.AddWithValue("@sl", soLuong);
                    cmd.ExecuteNonQuery();

                    string sqlUpdate = "UPDATE HoaDon SET TongTien = (SELECT SUM(ThanhTien) FROM ChiTietHoaDon WHERE MaHD = @mahd) WHERE MaHD = @mahd";
                    SqlCommand cmd2 = new SqlCommand(sqlUpdate, ketNoi);
                    cmd2.Parameters.AddWithValue("@mahd", maHD);
                    cmd2.ExecuteNonQuery();

                    ketNoi.Close();
                }

                MessageBox.Show("Thêm thành công!");
                LoadHoaDon();
                string maHD_Luu = maHD;
                LoadLaiGiaoDienSauKhiSua(maHD_Luu);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tại đây: " + ex.Message);
            }
        }

        private void dgvChiTiet_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                maSP_DangChon = dgvChiTiet.Rows[e.RowIndex].Cells["MaSP"].Value.ToString();
            }
           
        }
       
    }
}
