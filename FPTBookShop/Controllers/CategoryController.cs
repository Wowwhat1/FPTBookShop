﻿using Microsoft.AspNetCore.Mvc;

namespace FPTBookShop.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
