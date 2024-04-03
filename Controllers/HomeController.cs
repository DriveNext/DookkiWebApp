using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DookkiWebApp.Models;
using DookkiWebApp.ViewModels;

using PagedList;
using PagedList.Mvc;


namespace DookkiWebApp.Controllers
{
    public class HomeController : Controller
    {
        DookkiDataContext data = new DookkiDataContext();
        public ActionResult Index()
        {
            if (Session["Taikhoan"] != null)
            {
                KhachHang kh = (KhachHang)Session["Taikhoan"];
                ViewBag.Diemtichluy = data.KhachHangs.Single(n => n.Email == kh.Email).Diem;
            }
            return View();
        }

        public ActionResult Notification()
        {
            if (Session["Taikhoan"] != null)
            {
                KhachHang kh = (KhachHang)Session["Taikhoan"];
                return View(data.ThongBaos.Where(n => n.MaKH == kh.MaKH).ToList());
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
            
        }
        public ActionResult Voucher()
        {
            if (Session["Taikhoan"] != null)
            {
                KhachHang kh = (KhachHang)Session["Taikhoan"];

                var listVoucherKH = data.KH_Vouchers.Where(m => m.MaKH == kh.MaKH);
                var listVoucher = data.Vouchers.Where(n => n.TrangThai == 0).Where(x => x.SoLuong > 0);

                var result = listVoucher.Where(x => listVoucherKH.All(y => y.MaVoucher != x.MaVoucher));
                return View(result.ToList());
            }
            else
            {
                var result = data.Vouchers.Where(n => n.TrangThai == 0).Where(x => x.SoLuong > 0);
                return View(result.ToList());
            }
        }
        public ActionResult FindDookki(string searchString, int ? matp)
        {
            int idcity = (matp ?? 0);
            var model = new FindDookkiViewModel()
            {
                listTP = data.ThanhPhos.ToList(),
                listCN = (matp > 0 ? data.ChiNhanhs.Where(x => x.MaTP == idcity).Where(y => y.TenCN.Contains(searchString)).ToList() : data.ChiNhanhs.ToList()),
            };
            ViewBag.MaTP = matp;
            ViewBag.searchString = searchString;
            return View(model);
        }
        public ActionResult InforBranchDookki(int macn)
        {
            return View(data.ChiNhanhs.Single(a => a.MaCN == macn));
        }
    }
}