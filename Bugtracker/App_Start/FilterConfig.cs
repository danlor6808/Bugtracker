﻿using Bugtracker.Models;
using System.Web;
using System.Web.Mvc;

namespace Bugtracker
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
