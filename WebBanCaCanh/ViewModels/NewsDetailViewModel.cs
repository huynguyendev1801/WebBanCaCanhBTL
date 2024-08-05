using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebBanCaCanh.Models;

namespace WebBanCaCanh.ViewModels
{
    public class NewsDetailViewModel
    {
        public News News { get; set; }
        public List<Category> Categories { get; set; }
        public List<News> RelatedNews { get; set; }
    }
}