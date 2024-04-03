using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DookkiWebApp.Models;

namespace DookkiWebApp.ViewModels
{
    public class FindDookkiViewModel
    {
        public IEnumerable<ThanhPho> listTP { get; set; }
        public IEnumerable<ChiNhanh> listCN { get; set; }
    }
}