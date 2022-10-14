﻿using FinalLayiheBackend.Data;
using FinalLayiheBackend.Dtos.ProductDtos.ProductCreateDto;
using FinalLayiheBackend.Dtos.ProductDtos.ProductReturnDtos;
using FinalLayiheBackend.Extentions;
using FinalLayiheBackend.Helpers;
using FinalLayiheBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FinalLayiheBackend.Controllers.AdminController
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminProductController : ControllerBase
    {
        private IWebHostEnvironment _env;
        private readonly AppDbContext _context;

        public AdminProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _env = env;
            _context = context;
        }

        [HttpGet("brandAndCategory")]

        public IActionResult GetBrandAndCategoryId()
        {
            List<Brand> dbBrands = _context.Brands.Where(b => !b.IsDeleted).ToList();
            List<Category> dbCategory = _context.Categories.ToList();

            var obj = new
            {
                Brand = dbBrands,
                Category = dbCategory
            };
            return Ok(obj);
        }

        [HttpPost("createProduct")]

        public IActionResult Create([FromForm] ProductCreateDto productCreateDto)
        {


            foreach (var item in productCreateDto.ChildPhotos)
            {

                if (item == null)
                {

                    return BadRequest("Bosqoyma");
                }
                if (!item.IsImage())
                {

                    return BadRequest("only Photo");

                }
                if (item.ValidSize(200))
                {
                    return BadRequest("olcu uygun deyil");
                }


            }

            if (productCreateDto.Photos == null)
            {

                return BadRequest("Bosqoyma");
            }
            if (!productCreateDto.Photos.IsImage())
            {

                return BadRequest("only Photo");

            }
            if (productCreateDto.Photos.ValidSize(200))
            {
                return BadRequest("olcu uygun deyil");
            }

            List<ProductPhoto> photos = new List<ProductPhoto>();

            foreach (var item in productCreateDto.ChildPhotos)
            {

                ProductPhoto photo = new ProductPhoto
                {
                    Path = "http://localhost:14345/img/" + item.SaveImage(_env, "img"),
                    IsMain = false
                };
                photos.Add(photo);
            }

            ProductPhoto isMainPhoto = new ProductPhoto
            {
                Path = "http://localhost:14345/img/" + productCreateDto.Photos.SaveImage(_env, "img"),
                IsMain = true,
            };
            photos.Add(isMainPhoto);



            Product newProduct = new Product
            {
                Name = productCreateDto.Name,
                Price = productCreateDto.Price,
                Discount = productCreateDto.Discount,
                DiscountPrice= productCreateDto.Price - (productCreateDto.Price * productCreateDto.Discount / 100),
                BrandId = productCreateDto.BrandId,
                CategoryId = productCreateDto.CategoryId,
                Trending=productCreateDto.Trending,
                TypeName=productCreateDto.TypeName,
                Color=productCreateDto.Color,
                ProductPhotos=photos,
                CreatedDate = DateTime.Now,
            };
            newProduct.ProductSizes = new List<ProductSize>();
            foreach (var item in productCreateDto.Sizes)
            {
                ProductSize productSize = new ProductSize();
                Size size = new Size();
                size.Sizes = item;
                productSize.Sizes = size;
                newProduct.ProductSizes.Add(productSize);

            }
        

            _context.Add(newProduct);
            _context.SaveChanges();
            return StatusCode(201);
        }

        [HttpGet("getAll")]
        [Authorize]
        public IActionResult GetAll()
        {
            IQueryable<ProductReturnAdminDto> query = _context.Products.Include(x => x.Brand).Include(x => x.Category).Where(x => !x.isDeleted).Select(p => new ProductReturnAdminDto
            {
                Id = p.Id,
                Photo= p.ProductPhotos.FirstOrDefault(p => p.IsMain),
                Name = p.Name,
                Price = p.Price,
                Discount = p.Discount,
                Color = p.Color,
                Trending = p.Trending,
                Typename=p.TypeName,
                Brand=p.Brand,
                Category=p.Category
            });


            var result = query.ToList();
            return Ok(result);

        }


        [HttpDelete("delete")]
        public IActionResult Delete(int id)
        {



            List<BasketItem> basketItems = _context.BasketItems.Include(b => b.Product).Where(b => b.ProductId == id).ToList();
            Product dbproduct = _context.Products.Include(p => p.ProductPhotos).FirstOrDefault(p => p.Id == id);
            foreach (var item in dbproduct.ProductPhotos)
            {
                string path = Path.Combine(_env.WebRootPath, "img", item.Path);

               
                 Helper.DeleteImage(path);
            }

            foreach (var item in basketItems)
            {
                item.Product.isDeleted = true;
            }

            dbproduct.isDeleted = true;
            _context.Products.Remove(dbproduct);

            _context.SaveChanges();

            return StatusCode(200);
        }


        [HttpGet("getOne")]

        public IActionResult GetOne(int id)
        {
            Product product = _context.Products.Include(p => p.ProductPhotos).Include(x => x.Brand).Include(x => x.Category).FirstOrDefault(x => x.Id == id);

            ProductReturnUpdateAdminDto productUpdateDto = new ProductReturnUpdateAdminDto()
            {
                ProductPhotos = product.ProductPhotos.Select(x => new ProductPhoto
                {
                    Path = x.Path,
                    IsMain = x.IsMain,
                    

                }).ToList(),

                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Discount = product.Discount,
                Typename = product.TypeName,
                Trending=product.Trending,
                Brand = product.Brand,
                Category = product.Category,
                Color=product.Color

            };


            return Ok(productUpdateDto);
        }


        [HttpPut("updateProduct")]
        [Authorize]
        public async Task<IActionResult> Update([FromForm]ProductCreateDto product)
        {
            List<ProductPhoto> productImages = new List<ProductPhoto>();


            Product dbProducts = _context.Products.Include(p => p.ProductPhotos).Include(x => x.Brand).Include(x => x.Category).FirstOrDefault(c => c.Id == product.Id);
            Product productName = _context.Products.FirstOrDefault(p => p.Name.ToLower() == dbProducts.Name.ToLower());
           // Brand brandname = _context.Brands.FirstOrDefault(b => b.Name == dbProducts.Brand.Name);
            if (product.Photos != null)
            {
                foreach (var item in product.ChildPhotos)
                {

                    if (item == null)
                    {

                        return BadRequest("Bosqoyma");
                    }
                    if (!item.IsImage())
                    {

                        return BadRequest("only Photo");

                    }
                    if (item.ValidSize(200))
                    {
                        return BadRequest("olcu uygun deyil");
                    }


                }

                if (product.Photos == null)
                {

                    return BadRequest("Bosqoyma");
                }
                if (!product.Photos.IsImage())
                {

                    return BadRequest("only Photo");

                }
                if (product.Photos.ValidSize(200))
                {
                    return BadRequest("olcu uygun deyil");
                }



            }
            if (productName != null)
            {
                if (productName.Name != dbProducts.Name)
                {
                    return BadRequest("bu adli product var ");
                }
            }


            dbProducts.Name = product.Name;
            dbProducts.Price = product.Price;
            dbProducts.Discount = product.Discount;
            dbProducts.Color = product.Color;
            dbProducts.Trending = product.Trending;
            dbProducts.TypeName = product.TypeName;
            dbProducts.BrandId = product.BrandId;
            dbProducts.CategoryId = product.CategoryId;
            

            await _context.SaveChangesAsync();
            return Ok();


        }

    }
}