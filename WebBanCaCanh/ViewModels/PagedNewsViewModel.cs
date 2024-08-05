using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebBanCaCanh.Models;

namespace WebBanCaCanh.ViewModels
{
    public class PagedNewsViewModel
    {
        public List<News> NewsList { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

}