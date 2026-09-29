using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace QuanLyBanNuocGK
{
    public partial class UC_ThongKe : UserControl
    {
        private string chuoiKN = @"Data Source=MSI\SQLEXPRESS;Initial Catalog=Quanlynuocgiaikhat;Integrated Security=True;TrustServerCertificate=True";
      
        public UC_ThongKe()
        {
            InitializeComponent();
          

        }

        private void UC_ThongKe_Load(object sender, EventArgs e)
        {
            dtpTuNgay.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // Ngày đầu tháng
            dtpDenNgay.Value = DateTime.Now; // Hôm nay
            cboLoaiThongKe.SelectedIndex = 0;
        }

        private void btnThongKe_Click(object sender, EventArgs e)
        {

            try
            {
                // Kiểm tra ngày tháng hợp lệ
                if (dtpTuNgay.Value > dtpDenNgay.Value)
                {
                    MessageBox.Show("Từ ngày phải nhỏ hơn hoặc bằng đến ngày!", "Cảnh báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Xóa dữ liệu cũ trên chart
                chartDoanhThu.Series["DoanhThu"].Points.Clear();

                // Thực hiện thống kê theo loại đã chọn
                switch (cboLoaiThongKe.SelectedIndex)
                {
                    case 0: ThongKeTheoNgay(); break;
                    case 1: ThongKeTheoThang(); break;
                    case 2: ThongKeTheoNam(); break;
                    case 3: ThongKeTheoSanPham(); break;
                    case 4: ThongKeTheoKhachHang(); break;
                    case 5: ThongKeTheoNhanVien(); break;
                    default: ThongKeTheoNgay(); break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ThongKeTheoNgay()
        {
            string query = @"
        SELECT 
            CONVERT(DATE, NgayBan) as Ngay,
            COUNT(*) as SoLuong,
            SUM(TongTien) as DoanhThu,
            ISNULL(SUM(GiamGia), 0) as TongGiamGia
        FROM HoaDon 
        WHERE NgayBan BETWEEN @TuNgay AND @DenNgay
        AND TrangThai != 0
        GROUP BY CONVERT(DATE, NgayBan)
        ORDER BY Ngay";

            DataTable dt = ExecuteQuery(query);
            dgvChiTiet.DataSource = dt;

            // Định dạng cột
            if (dgvChiTiet.Columns.Count > 0)
            {
                dgvChiTiet.Columns["Ngay"].HeaderText = "Ngày";
                dgvChiTiet.Columns["SoLuong"].HeaderText = "Số hóa đơn";
                dgvChiTiet.Columns["DoanhThu"].HeaderText = "Doanh thu";
                dgvChiTiet.Columns["TongGiamGia"].HeaderText = "Giảm giá";
                dgvChiTiet.Columns["DoanhThu"].DefaultCellStyle.Format = "N0";
                dgvChiTiet.Columns["TongGiamGia"].DefaultCellStyle.Format = "N0";
            }

            // Cập nhật chart
            chartDoanhThu.ChartAreas[0].AxisX.Title = "Ngày";
            chartDoanhThu.Series["DoanhThu"].Points.Clear();
            foreach (DataRow row in dt.Rows)
            {
                string ngay = Convert.ToDateTime(row["Ngay"]).ToString("dd/MM");
                double doanhThu = Convert.ToDouble(row["DoanhThu"]);
                chartDoanhThu.Series["DoanhThu"].Points.AddXY(ngay, doanhThu);
            }

            decimal tongDoanhThu = 0;
            int tongDon = 0;
            decimal tongGiamGia = 0;

            foreach (DataRow row in dt.Rows)
            {
                tongDoanhThu += Convert.ToDecimal(row["DoanhThu"]);
                tongDon += Convert.ToInt32(row["SoLuong"]);
                tongGiamGia += Convert.ToDecimal(row["TongGiamGia"]);
            }

            int tongSanPham = LayTongSoLuongSanPham();

            lblTongDoanhThu.Text = tongDoanhThu.ToString("N0") + " VNĐ";
            lblTongDonHang.Text = tongDon.ToString();
            lblTongSanPham.Text = tongSanPham.ToString("N0");
            lblDoanhThuTB.Text = (tongDon > 0 ? (tongDoanhThu / tongDon).ToString("N0") : "0") + " VNĐ";

            int soKhach = LaySoLuongKhachHang();
            lblKhachHang.Text = soKhach.ToString();

            lblGiamGia.Text = tongGiamGia.ToString("N0") + " VNĐ";
        }

        private void btnXuatExcel_Click_1(object sender, EventArgs e)
        {

            if (dgvChiTiet.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx|CSV Files|*.csv",
                FileName = "ThongKe_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"),
                Title = "Chọn nơi lưu file Excel"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Xuất dữ liệu thành công!\nFile đã được lưu tại: " + saveFileDialog.FileName,
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void ThongKeTheoThang()
        {
            string query = @"
        SELECT 
            YEAR(NgayBan) as Nam,
            MONTH(NgayBan) as Thang,
            COUNT(*) as SoLuong,
            SUM(TongTien) as DoanhThu,
            ISNULL(SUM(GiamGia), 0) as TongGiamGia
        FROM HoaDon 
        WHERE NgayBan BETWEEN @TuNgay AND @DenNgay
        AND TrangThai != 0
        GROUP BY YEAR(NgayBan), MONTH(NgayBan)
        ORDER BY Nam, Thang";

            DataTable dt = ExecuteQuery(query);
            dt.Columns.Add("ThangNam", typeof(string), "Thang + '/' + Nam");

            dgvChiTiet.DataSource = dt;

            if (dgvChiTiet.Columns.Count > 0)
            {
                dgvChiTiet.Columns["ThangNam"].HeaderText = "Tháng/Năm";
                dgvChiTiet.Columns["SoLuong"].HeaderText = "Số hóa đơn";
                dgvChiTiet.Columns["DoanhThu"].HeaderText = "Doanh thu";
                dgvChiTiet.Columns["TongGiamGia"].HeaderText = "Giảm giá";
                dgvChiTiet.Columns["DoanhThu"].DefaultCellStyle.Format = "N0";
                dgvChiTiet.Columns["TongGiamGia"].DefaultCellStyle.Format = "N0";
                dgvChiTiet.Columns["Nam"].Visible = false;
                dgvChiTiet.Columns["Thang"].Visible = false;
            }

            chartDoanhThu.ChartAreas[0].AxisX.Title = "Tháng";
            chartDoanhThu.Series["DoanhThu"].Points.Clear();

            foreach (DataRow row in dt.Rows)
            {
                string thang = row["Thang"] + "/" + row["Nam"];
                double doanhThu = Convert.ToDouble(row["DoanhThu"]);
                chartDoanhThu.Series["DoanhThu"].Points.AddXY(thang, doanhThu);
            }

            // TÍNH TOÁN CARD
            decimal tongDoanhThu = 0;
            int tongDon = 0;
            decimal tongGiamGia = 0;

            foreach (DataRow row in dt.Rows)
            {
                tongDoanhThu += Convert.ToDecimal(row["DoanhThu"]);
                tongDon += Convert.ToInt32(row["SoLuong"]);
                tongGiamGia += Convert.ToDecimal(row["TongGiamGia"]);
            }

            // Lấy tổng số lượng sản phẩm đã bán
            int tongSanPham = LayTongSoLuongSanPham();
            int soKhach = LaySoLuongKhachHang();

            // Cập nhật card
            lblTongDoanhThu.Text = tongDoanhThu.ToString("N0") + " VNĐ";
            lblTongDonHang.Text = tongDon.ToString();
            lblTongSanPham.Text = tongSanPham.ToString("N0");
            lblDoanhThuTB.Text = (tongDon > 0 ? (tongDoanhThu / tongDon).ToString("N0") : "0") + " VNĐ";
            lblKhachHang.Text = soKhach.ToString();
            lblGiamGia.Text = tongGiamGia.ToString("N0") + " VNĐ";
        }

        private void ThongKeTheoNam()
        {
            string query = @"
        SELECT 
            YEAR(NgayBan) as Nam,
            COUNT(*) as SoLuong,
            SUM(TongTien) as DoanhThu,
            ISNULL(SUM(GiamGia), 0) as TongGiamGia
        FROM HoaDon 
        WHERE NgayBan BETWEEN @TuNgay AND @DenNgay
        AND TrangThai != 0
        GROUP BY YEAR(NgayBan)
        ORDER BY Nam";

            DataTable dt = ExecuteQuery(query);

            dgvChiTiet.DataSource = dt;

            if (dgvChiTiet.Columns.Count > 0)
            {
                dgvChiTiet.Columns["Nam"].HeaderText = "Năm";
                dgvChiTiet.Columns["SoLuong"].HeaderText = "Số hóa đơn";
                dgvChiTiet.Columns["DoanhThu"].HeaderText = "Doanh thu";
                dgvChiTiet.Columns["TongGiamGia"].HeaderText = "Giảm giá";
                dgvChiTiet.Columns["DoanhThu"].DefaultCellStyle.Format = "N0";
                dgvChiTiet.Columns["TongGiamGia"].DefaultCellStyle.Format = "N0";
            }

            chartDoanhThu.ChartAreas[0].AxisX.Title = "Năm";
            chartDoanhThu.Series["DoanhThu"].Points.Clear();

            foreach (DataRow row in dt.Rows)
            {
                string nam = row["Nam"].ToString();
                double doanhThu = Convert.ToDouble(row["DoanhThu"]);
                chartDoanhThu.Series["DoanhThu"].Points.AddXY(nam, doanhThu);
            }

            // TÍNH TOÁN CARD
            decimal tongDoanhThu = 0;
            int tongDon = 0;
            decimal tongGiamGia = 0;

            foreach (DataRow row in dt.Rows)
            {
                tongDoanhThu += Convert.ToDecimal(row["DoanhThu"]);
                tongDon += Convert.ToInt32(row["SoLuong"]);
                tongGiamGia += Convert.ToDecimal(row["TongGiamGia"]);
            }

            // Lấy tổng số lượng sản phẩm đã bán
            int tongSanPham = LayTongSoLuongSanPham();
            int soKhach = LaySoLuongKhachHang();

            lblTongDoanhThu.Text = tongDoanhThu.ToString("N0") + " VNĐ";
            lblTongDonHang.Text = tongDon.ToString();
            lblTongSanPham.Text = tongSanPham.ToString("N0");
            lblDoanhThuTB.Text = (tongDon > 0 ? (tongDoanhThu / tongDon).ToString("N0") : "0") + " VNĐ";
            lblKhachHang.Text = soKhach.ToString();
            lblGiamGia.Text = tongGiamGia.ToString("N0") + " VNĐ";
        }

        private void ThongKeTheoSanPham()
        {
            string query = @"
                SELECT TOP 10
                    sp.TenSP,
                    sp.LoaiSP,
                    ISNULL(SUM(cthd.SoLuong), 0) as SoLuongBan,
                    ISNULL(SUM(cthd.ThanhTien), 0) as DoanhThu,
                    COUNT(DISTINCT hd.MaHD) as SoDonHang
                FROM SanPham sp
                LEFT JOIN ChiTietHoaDon cthd ON sp.MaSP = cthd.MaSP
                LEFT JOIN HoaDon hd ON cthd.MaHD = hd.MaHD 
                    AND hd.NgayBan BETWEEN @TuNgay AND @DenNgay
                    AND hd.TrangThai != 0
                GROUP BY sp.TenSP, sp.LoaiSP
                HAVING SUM(cthd.ThanhTien) > 0
                ORDER BY DoanhThu DESC";

            DataTable dt = ExecuteQuery(query);

            dgvChiTiet.DataSource = dt;

            if (dgvChiTiet.Columns.Count > 0)
            {
                dgvChiTiet.Columns["TenSP"].HeaderText = "Sản phẩm";
                dgvChiTiet.Columns["LoaiSP"].HeaderText = "Loại";
                dgvChiTiet.Columns["SoLuongBan"].HeaderText = "SL bán";
                dgvChiTiet.Columns["DoanhThu"].HeaderText = "Doanh thu";
                dgvChiTiet.Columns["SoDonHang"].HeaderText = "Số đơn";

                dgvChiTiet.Columns["DoanhThu"].DefaultCellStyle.Format = "N0";
            }

            chartDoanhThu.Series["DoanhThu"].ChartType = SeriesChartType.Pie;
            chartDoanhThu.Series["DoanhThu"].Points.Clear();

            foreach (DataRow row in dt.Rows)
            {
                string tenSP = row["TenSP"].ToString();
                if (tenSP.Length > 15) tenSP = tenSP.Substring(0, 12) + "...";
                double doanhThu = Convert.ToDouble(row["DoanhThu"]);
                chartDoanhThu.Series["DoanhThu"].Points.AddXY(tenSP, doanhThu);
            }

            decimal tongDoanhThu = 0;

           


            foreach (DataRow row in dt.Rows)
            {
                tongDoanhThu += Convert.ToDecimal(row["DoanhThu"]);
            }
            string queryTongDon = @"
            SELECT COUNT(DISTINCT MaHD) as TongSoDon
            FROM HoaDon 
            WHERE NgayBan BETWEEN @TuNgay AND @DenNgay
              AND TrangThai != 0";

            DataTable dtTongDon = ExecuteQuery(queryTongDon);
            int tongDon = 0;
            if (dtTongDon.Rows.Count > 0)
            {
                tongDon = Convert.ToInt32(dtTongDon.Rows[0]["TongSoDon"]);
            }
            string queryTongSL = @"
            SELECT ISNULL(SUM(cthd.SoLuong), 0) as TongSoLuong
            FROM ChiTietHoaDon cthd
            JOIN HoaDon hd ON cthd.MaHD = hd.MaHD
            WHERE hd.NgayBan BETWEEN @TuNgay AND @DenNgay
              AND hd.TrangThai != 0";

            DataTable dtTongSL = ExecuteQuery(queryTongSL);
            int tongSanPham = 0;
            if (dtTongSL.Rows.Count > 0)
            {
                tongSanPham = Convert.ToInt32(dtTongSL.Rows[0]["TongSoLuong"]);
            }
            lblTongDoanhThu.Text = tongDoanhThu.ToString("N0") + " VNĐ";
            lblTongDonHang.Text = tongDon.ToString();
            lblTongSanPham.Text = tongSanPham.ToString("N0");

            lblDoanhThuTB.Text = (tongDon > 0 ? (tongDoanhThu / tongDon).ToString("N0") : "0") + " VNĐ";

            int soKhach = LaySoLuongKhachHang();
            lblKhachHang.Text = soKhach.ToString();

            decimal tongGiamGia = LayTongGiamGia();
            lblGiamGia.Text = tongGiamGia.ToString("N0") + " VNĐ";
        }

        private void ThongKeTheoKhachHang()
        {
            string query = @"
        SELECT TOP 10
            ISNULL(kh.TenKH, N'Khách vãng lai') as TenKH,
            kh.SoDienThoai,
            COUNT(hd.MaHD) as SoDonHang,
            ISNULL(SUM(hd.TongTien), 0) as TongChiTieu,
            ISNULL(AVG(hd.TongTien), 0) as TrungBinh,
            ISNULL(SUM(hd.GiamGia), 0) as TongGiamGia
        FROM KhachHang kh
        LEFT JOIN HoaDon hd ON kh.MaKH = hd.MaKH 
            AND hd.NgayBan BETWEEN @TuNgay AND @DenNgay
            AND hd.TrangThai != 0
        GROUP BY kh.TenKH, kh.SoDienThoai
        HAVING SUM(hd.TongTien) > 0
        ORDER BY TongChiTieu DESC";

            DataTable dt = ExecuteQuery(query);
            dgvChiTiet.DataSource = dt;

            if (dgvChiTiet.Columns.Count > 0)
            {
                dgvChiTiet.Columns["TenKH"].HeaderText = "Khách hàng";
                dgvChiTiet.Columns["SoDienThoai"].HeaderText = "SĐT";
                dgvChiTiet.Columns["SoDonHang"].HeaderText = "Số đơn";
                dgvChiTiet.Columns["TongChiTieu"].HeaderText = "Tổng chi tiêu";
                dgvChiTiet.Columns["TrungBinh"].HeaderText = "TB/đơn";
                dgvChiTiet.Columns["TongGiamGia"].HeaderText = "Giảm giá";
                dgvChiTiet.Columns["TongChiTieu"].DefaultCellStyle.Format = "N0";
                dgvChiTiet.Columns["TrungBinh"].DefaultCellStyle.Format = "N0";
                dgvChiTiet.Columns["TongGiamGia"].DefaultCellStyle.Format = "N0";
            }

            chartDoanhThu.Series["DoanhThu"].ChartType = SeriesChartType.Column;
            chartDoanhThu.ChartAreas[0].AxisX.Title = "Khách hàng";
            chartDoanhThu.Series["DoanhThu"].Points.Clear();
            foreach (DataRow row in dt.Rows)
            {
                string ten = row["TenKH"].ToString();
                if (ten.Length > 15) ten = ten.Substring(0, 12) + "...";
                double tien = Convert.ToDouble(row["TongChiTieu"]);
                chartDoanhThu.Series["DoanhThu"].Points.AddXY(ten, tien);
            }

            decimal tongDoanhThu = 0;
            int tongDon = 0;
            decimal tongGiamGia = 0;

            foreach (DataRow row in dt.Rows)
            {
                tongDoanhThu += Convert.ToDecimal(row["TongChiTieu"]);
                tongDon += Convert.ToInt32(row["SoDonHang"]);
                tongGiamGia += Convert.ToDecimal(row["TongGiamGia"]);
            }

            int tongSanPham = LayTongSoLuongSanPham();

            lblTongDoanhThu.Text = tongDoanhThu.ToString("N0") + " VNĐ";
            lblTongDonHang.Text = tongDon.ToString();
            lblTongSanPham.Text = tongSanPham.ToString("N0");
            lblDoanhThuTB.Text = (tongDon > 0 ? (tongDoanhThu / tongDon).ToString("N0") : "0") + " VNĐ";
            lblKhachHang.Text = dt.Rows.Count.ToString();
            lblGiamGia.Text = tongGiamGia.ToString("N0") + " VNĐ";
        }

        private void ThongKeTheoNhanVien()
        {
            string query = @"
        SELECT 
            nv.HoTen,
            COUNT(hd.MaHD) as SoDonHang,
            ISNULL(SUM(hd.TongTien), 0) as DoanhThu,
            ISNULL(AVG(hd.TongTien), 0) as TrungBinh,
            ISNULL(SUM(hd.GiamGia), 0) as TongGiamGia
        FROM NhanVien nv
        LEFT JOIN HoaDon hd ON nv.MaNV = hd.MaNV 
            AND hd.NgayBan BETWEEN @TuNgay AND @DenNgay
            AND hd.TrangThai != 0
        GROUP BY nv.HoTen
        HAVING SUM(hd.TongTien) > 0
        ORDER BY DoanhThu DESC";

            DataTable dt = ExecuteQuery(query);

            dgvChiTiet.DataSource = dt;

            if (dgvChiTiet.Columns.Count > 0)
            {
                dgvChiTiet.Columns["HoTen"].HeaderText = "Nhân viên";
                dgvChiTiet.Columns["SoDonHang"].HeaderText = "Số đơn";
                dgvChiTiet.Columns["DoanhThu"].HeaderText = "Doanh thu";
                dgvChiTiet.Columns["TrungBinh"].HeaderText = "TB/đơn";
                dgvChiTiet.Columns["TongGiamGia"].HeaderText = "Giảm giá";
                dgvChiTiet.Columns["DoanhThu"].DefaultCellStyle.Format = "N0";
                dgvChiTiet.Columns["TrungBinh"].DefaultCellStyle.Format = "N0";
                dgvChiTiet.Columns["TongGiamGia"].DefaultCellStyle.Format = "N0";
            }

            chartDoanhThu.Series["DoanhThu"].ChartType = SeriesChartType.Column;
            chartDoanhThu.ChartAreas[0].AxisX.Title = "Nhân viên";
            chartDoanhThu.Series["DoanhThu"].Points.Clear();

            foreach (DataRow row in dt.Rows)
            {
                string ten = row["HoTen"].ToString();
                double doanhThu = Convert.ToDouble(row["DoanhThu"]);
                chartDoanhThu.Series["DoanhThu"].Points.AddXY(ten, doanhThu);
            }

            decimal tongDoanhThu = 0;
            int tongDon = 0;
            decimal tongGiamGia = 0;

            foreach (DataRow row in dt.Rows)
            {
                tongDoanhThu += Convert.ToDecimal(row["DoanhThu"]);
                tongDon += Convert.ToInt32(row["SoDonHang"]);
                tongGiamGia += Convert.ToDecimal(row["TongGiamGia"]);
            }

            int tongSanPham = LayTongSoLuongSanPham();
            int soKhach = LaySoLuongKhachHang();

            lblTongDoanhThu.Text = tongDoanhThu.ToString("N0") + " VNĐ";
            lblTongDonHang.Text = tongDon.ToString();
            lblTongSanPham.Text = tongSanPham.ToString("N0");
            lblDoanhThuTB.Text = (tongDon > 0 ? (tongDoanhThu / tongDon).ToString("N0") : "0") + " VNĐ";
            lblKhachHang.Text = soKhach.ToString();
            lblGiamGia.Text = tongGiamGia.ToString("N0") + " VNĐ";
        }
        private int LayTongSoLuongSanPham()
        {
            string query = @"
        SELECT ISNULL(SUM(cthd.SoLuong), 0)
        FROM ChiTietHoaDon cthd
        JOIN HoaDon hd ON cthd.MaHD = hd.MaHD
        WHERE hd.NgayBan BETWEEN @TuNgay AND @DenNgay AND hd.TrangThai != 0";

            try
            {
                using (SqlConnection conn = new SqlConnection(chuoiKN))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TuNgay", dtpTuNgay.Value.Date);
                        cmd.Parameters.AddWithValue("@DenNgay", dtpDenNgay.Value.Date.AddDays(1).AddSeconds(-1));

                        object result = cmd.ExecuteScalar();
                        return result != DBNull.Value ? Convert.ToInt32(result) : 0;
                    }
                }
            }
            catch
            {
                return 0;
            }
        }
        private int LaySoLuongHoaDon()
        {
            string query = @"
        SELECT COUNT(DISTINCT MaHD) 
        FROM HoaDon 
        WHERE NgayBan BETWEEN @TuNgay AND @DenNgay
          AND TrangThai != 0";

            try
            {
                using (SqlConnection conn = new SqlConnection(chuoiKN))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@TuNgay", dtpTuNgay.Value.Date);
                    cmd.Parameters.AddWithValue("@DenNgay", dtpDenNgay.Value.Date.AddDays(1).AddSeconds(-1));

                    return (int)cmd.ExecuteScalar();
                }
            }
            catch
            {
                return 0;
            }
        }
        private DataTable ExecuteQuery(string query)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(chuoiKN))
                {
                    conn.Open();

                    string finalQuery = query;

                    using (SqlCommand cmd = new SqlCommand(finalQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@TuNgay", dtpTuNgay.Value.Date);
                        cmd.Parameters.AddWithValue("@DenNgay", dtpDenNgay.Value.Date.AddDays(1).AddSeconds(-1));

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi truy vấn: " + ex.Message);
            }

            return dt;
        }

        private decimal LayTongGiamGia()
        {
            string query = @"
                SELECT ISNULL(SUM(GiamGia), 0) 
                FROM HoaDon 
                WHERE NgayBan BETWEEN @TuNgay AND @DenNgay AND TrangThai != 0";

            try
            {
                using (SqlConnection conn = new SqlConnection(chuoiKN))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TuNgay", dtpTuNgay.Value.Date);
                        cmd.Parameters.AddWithValue("@DenNgay", dtpDenNgay.Value.Date.AddDays(1).AddSeconds(-1));

                        return Convert.ToDecimal(cmd.ExecuteScalar());
                    }
                }
            }
            catch
            {
                return 0;
            }
        }

        private int LaySoLuongKhachHang()
        {
            string query = @"
                SELECT COUNT(DISTINCT MaKH) 
                FROM HoaDon 
                WHERE NgayBan BETWEEN @TuNgay AND @DenNgay
                AND TrangThai != 0  
                AND MaKH IS NOT NULL";

            try
            {
                using (SqlConnection conn = new SqlConnection(chuoiKN))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TuNgay", dtpTuNgay.Value.Date);
                        cmd.Parameters.AddWithValue("@DenNgay", dtpDenNgay.Value.Date.AddDays(1).AddSeconds(-1));

                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch
            {
                return 0;
            }
        }

    }
}
