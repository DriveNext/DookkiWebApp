using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DookkiWebApp.Models;

namespace DookkiWebApp.ViewModels
{
    public class KhVoucherViewModel
    {
        public IEnumerable<Voucher> listVoucher { get; set; }
        public IEnumerable<KH_Voucher> listVoucherKH { get; set; }
    }
}