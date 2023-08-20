﻿using FPTBookShop.DataAccess.Data;
using FPTBookShop.DataAccess.Repository.IRepository;
using FPTBookShop.Models;
using FPTBookShop.Models.ViewModels;
using FPTBookShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FPTBookShop.Areas.Admin.Controllers
{
    [Area(areaName: "Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();

            return View(objProductList);
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                }),
                Product = new Product()
            };
            if (id == null || id == 0)
            {
                return View(productVM);
            } else
            {
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
                return View(productVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\products");

                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        // delete
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        
                        if (System.IO.File.Exists(oldImagePath)) 
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = @"\images\products\" + fileName;
                }

                if (productVM.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);
                } else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                }

                _unitOfWork.Save();
                TempData["success"] = "Product created successful";
                return RedirectToAction("Index");
            } else
            {
                productVM.CategoryList = _unitOfWork.Category
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                });
                return View(productVM);
            }
        }

        #region APICALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new {data =  objProductList});
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDelete = _unitOfWork.Product.Get(u => u.Id == id);

            if (productToBeDelete == null)
            {
                return Json(new { success = false, message = "Error while deleting!" });
            }

            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDelete.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath); 
            }

            _unitOfWork.Product.Remove(productToBeDelete);  
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete successful!" });
        }
        #endregion
    }
}
