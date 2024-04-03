using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using QRCoder;

using DookkiWebApp.Models;
using System.Web.Security;
using DookkiWebApp.common;

namespace DookkiWebApp.Controllers
{
    public class AccountController : Controller
    {
        DookkiDataContext data = new DookkiDataContext();
        // GET: Account
        public void GetInfor(KhachHang kh)
        {
            var thongtin    = data.KhachHangs.Single(n => n.Email == kh.Email);
            ViewBag.MaKH    = thongtin.MaKH;
            ViewBag.HoTen   = thongtin.HoTen;
            ViewBag.SDT     = thongtin.DienThoaiKH;
            ViewBag.Email   = thongtin.Email;
            ViewBag.Diem    = thongtin.Diem;
            ViewBag.TongChiTieu    = thongtin.TongChiTieu;
            if (thongtin.TongChiTieu < 3000000)
            {
                ViewBag.PhanTramTichDiem = 0.05;
            }
            else if (thongtin.TongChiTieu >= 3 && thongtin.TongChiTieu < 5000000)
            {
                ViewBag.PhanTramTichDiem = 0.1;
            }
            else if (thongtin.TongChiTieu >= 5000000 && thongtin.TongChiTieu < 10000000)
            {
                ViewBag.PhanTramTichDiem = 0.2;
            }
            else
            {
                ViewBag.PhanTramTichDiem = 0.5;
            }
        }
        public void SaveQR(Bitmap bitmap, string fileName, string direcotryPath)
        {
            if (!Directory.Exists(direcotryPath))
                Directory.CreateDirectory(direcotryPath);

            fileName = direcotryPath + "\\" + fileName;

            bitmap.Save(fileName, ImageFormat.Png);
        }
        public ActionResult PersonalAccount()
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Login", new { returnUrl = Request.RawUrl.ToString() });
            }
            else
            {
                KhachHang kh = (KhachHang)Session["Taikhoan"];
                GetInfor(kh);
                if (TempData["ChangePassSuccess"] != null)
                {
                    ViewBag.Success = TempData["ChangePassSuccess"];
                    TempData.Remove("ChangePassSuccess");
                }
                return View();
            }
        }
        public ActionResult Login()
        {
            if (Session["Taikhoan"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (TempData["statusSignin"] != null)
                {
                    ViewBag.SigninSuccess = TempData["statusSignin"];
                    TempData.Remove("statusSignin");
                }
                return View();
            }
        }
        [HttpPost]
        public ActionResult Login(FormCollection collection, string returnUrl)
        {
            password EncryptedData = new password();
            var sdt = collection["SDT"];
            var matkhau = collection["Matkhau"];

            if (String.IsNullOrEmpty(sdt))
            {
                ViewData["Loi1"] = "Số điện thoại không được bỏ trống";
            }
            else if (String.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi2"] = "Mật khẩu không được bỏ trống";
            }

            else
            {
                KhachHang kh = data.KhachHangs.SingleOrDefault(model => model.DienThoaiKH == sdt && model.MatKhau == EncryptedData.Encode(matkhau));
                if (kh != null)
                {
                    FormsAuthentication.SetAuthCookie(kh.HoTen, false);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        Session["Taikhoan"] = kh;
                        TempData["LoginSuccess"] = "Đăng nhập thành công";
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        Session["Taikhoan"] = kh;
                        TempData["LoginSuccess"] = "Đăng nhập thành công";
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                    ViewData["LoiDN"] = "Tên đăng nhập hoặc mật khẩu không đúng";
            }
            return View();
        }
        public ActionResult Signin()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Signin(FormCollection collection, KhachHang kh)
        {
            var hoten = collection["HoTen"];
            var ngaysinh = String.Format("{0:dd/MM/yyyy}", collection["NgaySinh"]);
            var sdt = collection["SDT"];
            var email = collection["Email"];
            var matkhau = collection["MatKhau"];
            var matkhaunhaplai = collection["MatKhauNhapLai"];
            password EncryptedData = new password();

            if (String.IsNullOrEmpty(hoten))
            {
                ViewData["Loi1"] = "Tên không được để trống";
            }
            if (String.IsNullOrEmpty(ngaysinh))
            {
                ngaysinh = "01/01/2000";
            }
            if (String.IsNullOrEmpty(sdt))
            {
                ViewData["Loi2"] = "Số điện thoại không được để trống";
            }
            if (String.IsNullOrEmpty(email))
            {
                ViewData["Loi3"] = "Email không được để trống";
            }
            if (String.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi4"] = "Mật khẩu không được để trống";
            }
            if (String.IsNullOrEmpty(matkhaunhaplai))
            {
                ViewData["Loi5"] = "Mật khẩu không được để trống";
            }
            else if (matkhaunhaplai != matkhau)
            {
                ViewData["Loi6"] = "Mật khẩu không khớp";
            }
            else
            {
                kh.HoTen = hoten;
                kh.NgaySinh = DateTime.Parse(ngaysinh);
                kh.DienThoaiKH = sdt;
                kh.GioiTinh = 0;
                kh.Email = email;
                kh.Diem = 500;
                kh.MatKhau = EncryptedData.Encode(matkhau);
                if (data.KhachHangs.Any(x => x.Email == kh.Email))
                {
                    ViewBag.LoiDK = "Email đã được đăng ký";
                }
                else
                {
                    data.KhachHangs.InsertOnSubmit(kh);
                    data.SubmitChanges();
                    TempData["statusSignin"] = "Đăng Ký Thành công";
                    return RedirectToAction("Login");
                }
            }
            return this.Signin();
        }
        public ActionResult SignOut()
        {
            Session["Taikhoan"] = null;
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
        public ActionResult ChangePass()
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Login", new { returnUrl = Request.RawUrl.ToString() });
            }
            else
            {
                KhachHang kh = (KhachHang)Session["Taikhoan"];
                return View();
            }
        }
        [HttpPost]
        public ActionResult ChangePass(FormCollection collection)
        {
            KhachHang sskh = (KhachHang)Session["Taikhoan"];
            KhachHang kh = data.KhachHangs.Single(n => n.Email == sskh.Email);

            var oldpassword = collection["oldpassword"];
            var newpassword = collection["newpassword"];
            var retypepass = collection["retypepass"];

            password EncryptedData = new password();

            if (String.IsNullOrEmpty(oldpassword) || String.IsNullOrEmpty(newpassword) || String.IsNullOrEmpty(retypepass))
            {
                ViewBag.LoiNhap = "Mật khẩu không được để trống!";
            }

            else if (EncryptedData.Encode(oldpassword) != kh.MatKhau)
            {
                ViewData["Loi1"] = "Mật khẩu cũ không đúng!";
            }
            else if (retypepass != newpassword)
            {
                ViewData["Loi2"] = "Mật khẩu nhập lại không khớp!";
            }
            else if (EncryptedData.Encode(newpassword) == kh.MatKhau)
            {
                ViewData["Loi3"] = "Mật khẩu mới không được trùng với mật khẩu cũ!";
            }
            else
            {
                kh.MatKhau = EncryptedData.Encode(newpassword);
                data.SubmitChanges();

                TempData["ChangePassSuccess"] = "Đổi mật khẩu thành công";
                return RedirectToAction("PersonalAccount", "Account");
            }
            return this.Information();
        }
        public ActionResult Information()
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Login", new { returnUrl = Request.RawUrl.ToString() });
            }
            else
            {
                KhachHang kh = (KhachHang)Session["Taikhoan"];
                GetInfor(kh);
                return View(data.KhachHangs.Single(n => n.Email == kh.Email));
            }
        }
        public ActionResult UpdateInfor(int makh, string hoten, string ngaysinh)
        {
            KhachHang kh = data.KhachHangs.SingleOrDefault(n => n.MaKH == makh);

            ngaysinh = String.Format("{0:dd/MM/yyyy}", ngaysinh);
            bool isSuccess = false;

            if (ModelState.IsValid)
            {
                kh.HoTen = hoten;
                kh.NgaySinh = Convert.ToDateTime(ngaysinh);

                data.SubmitChanges();
                isSuccess = true;
            }
            return Json(new { Success = isSuccess});
        }
        public ActionResult InforPoint()
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Login", new { returnUrl = Request.RawUrl.ToString() });
            }
            else
            {
                KhachHang kh = (KhachHang)Session["Taikhoan"];
                GetInfor(kh);
                return View(data.TichDiems.Where(i => i.KhachHang.Email == kh.Email).ToList().OrderByDescending(n => n.ThoiGian));
            }
        }
        public ActionResult PersonalVoucher()
        {
            if (Session["Taikhoan"] != null)
            {
                KhachHang kh = (KhachHang)Session["Taikhoan"];
                return View(data.KH_Vouchers.Where(a => a.MaKH == kh.MaKH).ToList());
            }
            else
            {
                return RedirectToAction("Login", new { returnUrl = Request.RawUrl.ToString() });
            }
        }
        public ActionResult ActivityHis()
        {
            if (Session["Taikhoan"] != null)
            {
                KhachHang kh = (KhachHang)Session["Taikhoan"];
                return View(data.DatBans.Where(n => n.MaKH == kh.MaKH).OrderByDescending(x => x.ThoiGian).ToList());
            }
            else
            {
                return RedirectToAction("Login", new { returnUrl = Request.RawUrl.ToString() });
            }
        }
        public ActionResult DetailBooking(int madb)
        {
            if (Session["Taikhoan"] != null)
            {
                KhachHang kh = (KhachHang)Session["Taikhoan"];
                return View(data.DatBans.Single(m => m.MaDatBan == madb));
            }
            else
            {
                return RedirectToAction("Login", new { returnUrl = Request.RawUrl.ToString() });
            }
        }
        public ActionResult Statistical()
        {
            if (Session["Taikhoan"] != null)
            {
                KhachHang kh = (KhachHang)Session["Taikhoan"];
                GetInfor(kh);
                return View();
            }
            else
            {
                return RedirectToAction("Login", new { returnUrl = Request.RawUrl.ToString() });
            }
        }
        public ActionResult Statistical_month(int month, int year)
        {
            var startOfMonth = new DateTime(year, month, 1);
            var DaysInMonth = DateTime.DaysInMonth(year, month);
            var lastDay = new DateTime(year, month, DaysInMonth);

            var statistical_month = data.DatBans.Where(y => y.ThoiGian >= startOfMonth).Where(z => z.ThoiGian <= lastDay);

            var tongchitieu = statistical_month.Sum(x => (int?)x.TongTien) ?? 0;
            return Json( new { TongChiTieu = @String.Format("{0:0,0}", tongchitieu) + " đồng" });
        }
        public ActionResult Statistical_year(string year)
        {
            var first_year = Convert.ToDateTime("01/01/" + year + " 00:00");
            var end_year = Convert.ToDateTime("12/31/" + year + " 23:59");
            var statistical_year = data.DatBans.Where(y => y.ThoiGian >= first_year).Where(z => z.ThoiGian <= end_year);
            
            var tongchitieu = statistical_year.Sum(x => (int?)x.TongTien) ?? 0;
            return Json( new { TongChiTieu = @String.Format("{0:0,0}", tongchitieu) + " đồng" });
        }
        public ActionResult FormBooking(int macn)
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Login", new { returnUrl = Request.RawUrl.ToString() });
            }
            else
            {
                KhachHang kh = (KhachHang)Session["Taikhoan"];
                GetInfor(kh);
                return View(data.ChiNhanhs.Single(a => a.MaCN == macn));
            }
        }
        public ActionResult Booking(int makh, int macn, string name, string datebooking, string timebooking, int amount, string phone, string email, string note)
        {
            DatBan db = new DatBan();
            int maxid = data.DatBans.Max(m => m.MaDatBan) + 1;
            bool isSuccess = false;
            string thoigian = datebooking + ' ' + timebooking;

            string link = new Uri(Request.Url, Url.Content("~/Account/ScanQRBooking") + "?idBooking=" + maxid).ToString();

            QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
            QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode(link, QRCodeGenerator.ECCLevel.Q);
            QRCode qRCode = new QRCode(qRCodeData);

            var fileName = "QRBooking_" + DateTime.Now.Millisecond + "_" + maxid + ".png";
            var direcotryPath = System.Web.HttpContext.Current.Server.MapPath("~/Assets/Images/QRCodeBooking");

            using (Bitmap bitmap = qRCode.GetGraphic(20))
            {
                SaveQR(bitmap, fileName, direcotryPath);
            }

            if (ModelState.IsValid)
            {
                db.ThoiGian = Convert.ToDateTime(String.Format("{0:dd/MM/yyyy HH:mm}", thoigian));
                db.SoNguoi = amount;
                db.GhiChu = note;
                db.MaQR = fileName;
                db.HoTen = name;
                db.SDT = phone;
                db.Email = email;
                db.MaQR = fileName;
                db.TongTien = 2800000;
                db.TrangThai = 0;
                db.MaKH = makh;
                db.MaCN = macn;

                data.DatBans.InsertOnSubmit(db);
                data.SubmitChanges();
                isSuccess = true;
            }
            return Json(new { Success = isSuccess });
        }

        public ActionResult ScanQRBooking(int idBooking)
        {
            if (Session["Taikhoan"] != null)
            {
                KhachHang kh = (KhachHang)Session["Taikhoan"];
                GetInfor(kh);
                return View(data.DatBans.Single(m => m.MaDatBan == idBooking));
            }
            else
            {
                return RedirectToAction("Login", new { returnUrl = Request.RawUrl.ToString() });
            }
        }
        public ActionResult accumulatePointSuccess()
        {
            return View();
        }
        public ActionResult accumulatePoints(int idBooking, int idCustomer, int point)
        {
            KhachHang kh = data.KhachHangs.SingleOrDefault(n => n.MaKH == idCustomer);
            DatBan db = data.DatBans.SingleOrDefault(n => n.MaDatBan == idBooking);
            TichDiem tichdiem = new TichDiem();
            bool isSuccess = false;
            bool error = false;

            if(db != null)
            {
                if (ModelState.IsValid)
                {
                    if (db.TrangThai != 1)
                    {
                        kh.Diem = kh.Diem + point;
                        kh.TongChiTieu = kh.TongChiTieu + db.TongTien;
                        db.TrangThai = 1;

                        tichdiem.TieuDe = "Đổi điểm hóa đơn";
                        tichdiem.ChiTiet = "Giá trị hóa đơn " + String.Format("{0:0,0}", db.TongTien) + "đ";
                        tichdiem.ThoiGian = Convert.ToDateTime(DateTime.Now);
                        tichdiem.Diem = point;
                        tichdiem.TrangThai = 0;
                        tichdiem.MaKH = idCustomer;

                        data.TichDiems.InsertOnSubmit(tichdiem);
                        data.SubmitChanges();
                        isSuccess = true;
                    }
                    else
                    {
                        isSuccess = false;
                    }
                }
            }
            else
            {
                error = true;
            }
            
            return Json(new { Success = isSuccess,  Error = error});
        }
        public ActionResult ExchangeVoucher(int mavoucher)
        {
            KhachHang sskh = (KhachHang)Session["Taikhoan"];
            KhachHang kh = data.KhachHangs.SingleOrDefault(n => n.MaKH == sskh.MaKH);
            Voucher voucher = data.Vouchers.SingleOrDefault(v => v.MaVoucher == mavoucher);
            KH_Voucher kh_voucher = new KH_Voucher();
            TichDiem tichdiem = new TichDiem();
            ThongBao thongbao = new ThongBao();

            int maxid = data.KH_Vouchers.Max(m => m.ID) + 1;

            var makh = kh.MaKH;
            bool isSuccess = false;

            string link = new Uri(Request.Url, Url.Content("~/Account/ScanQRVoucher") + "?idVoucher=" + maxid + "&idBooking=").ToString();

            QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
            QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode(link, QRCodeGenerator.ECCLevel.Q);
            QRCode qRCode = new QRCode(qRCodeData);

            var fileName = "QRVoucher_" + DateTime.Now.Millisecond + "_" + makh + "_" + mavoucher + "_" + maxid + ".png";
            var direcotryPath = System.Web.HttpContext.Current.Server.MapPath("~/Assets/Images/QRCodeVoucher");

            using (Bitmap bitmap = qRCode.GetGraphic(20))
            {
                SaveQR(bitmap, fileName, direcotryPath);
            }

            if (ModelState.IsValid)
            {
                kh.Diem = kh.Diem - voucher.Diem;
                voucher.SoLuong = voucher.SoLuong - 1;

                kh_voucher.MaKH = makh;
                kh_voucher.MaVoucher = mavoucher;
                kh_voucher.MaQR = fileName;
                kh_voucher.ThoiHan = Convert.ToDateTime(DateTime.Now.AddDays(30));
                kh_voucher.TrangThai = 0;

                tichdiem.TieuDe = "Đổi Voucher";
                tichdiem.ChiTiet = "Giá trị voucher " + String.Format("{0:0,0}", voucher.GiamToiDa) + " đồng";
                tichdiem.ThoiGian = Convert.ToDateTime(DateTime.Now);
                tichdiem.Diem = voucher.Diem;
                tichdiem.TrangThai = 1;
                tichdiem.MaKH = makh;

                thongbao.TieuDe = "Đổi Voucher";
                thongbao.ChiTiet = "Chúc mừng bạn đã đổi thành công voucher trị giá " + String.Format("{0:0,0}", voucher.GiamToiDa) + "đồng";
                thongbao.ThoiGian = Convert.ToDateTime(DateTime.Now);
                thongbao.MaKH = makh;

                data.TichDiems.InsertOnSubmit(tichdiem);
                data.KH_Vouchers.InsertOnSubmit(kh_voucher);
                data.ThongBaos.InsertOnSubmit(thongbao);
                data.SubmitChanges();
                isSuccess = true;
            }
            return Json(new { Success = isSuccess });
        }
        public ActionResult ScanQRVoucher()
        {
            return RedirectToAction("ActivityHis");
        }
    }
}