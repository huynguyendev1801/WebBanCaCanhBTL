using System.Collections.Generic;

namespace WebBanCaCanh.ViewModels
{
    public class BreadcrumbViewModel
    {
        public List<BreadcrumbItem> Breadcrumbs { get; set; } = new List<BreadcrumbItem>();
    }

    public class BreadcrumbItem
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public bool IsActive { get; set; }
    }
}
