using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuanLyBanNuocGK
{
    public static class CurrentUser
    {
        public static int MaNV = 0;      // Lưu mã định danh nhân viên
        public static string HoTen = ""; // Lưu tên hiển thị
        public static string TenDN = ""; // Lưu tên đăng nhập nếu cần
    }
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 login = new Form1();

            if (login.ShowDialog() == DialogResult.OK) // Nếu login trả về OK
            {
                Application.Run(new Form1()); // Thì mới chạy Form chính
            }
        }
    }
}
