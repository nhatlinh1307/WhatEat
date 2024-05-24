using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using whatseat_server.Data;
using whatseat_server.Models;
using whatseat_server.Models.DTOs.Requests;
using whatseat_server.Models.DTOs.Responses;
using whatseat_server.Services;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace whatseat_server.Controllers;
[ApiController]
[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly CustomerService _customerService;
    private readonly WhatsEatContext _context;

    public ProductController(
        ProductService productService,
        WhatsEatContext context,
        CustomerService customerService
        )
    {
        _context = context;
        _productService = productService;
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> SearchProduct([FromQuery] ProductFilter searchParams)
    {
        var products = await _productService.FullTextSearchProduct(searchParams);

        var productRes = new List<ProductResponse>();
        foreach (var item in products)
        {
            productRes.Add(new ProductResponse
            {
                Images = _productService.ConvertJsonToPhotos(item.PhotoJson),
                ProductId = item.ProductId,
                Name = item.Name,
                InStock = item.InStock,
                BasePrice = item.BasePrice,
                Description = item.Description,
                WeightServing = item.WeightServing,
                TotalSell = item.TotalSell,
                ProductCategoryId = item.ProductCategory.ProductCategoryId,
                StoreName = item.Store.ShopName,
                StoreId = item.Store.StoreId,
                CreatedOn = item.CreatedOn,
                Status = item.Status,
                TotalView = await _productService.GetProductViews(item)
            });
        }
        var metadata = new
        {
            products.TotalCount,
            products.PageSize,
            products.CurrentPage,
            products.TotalPages,
            products.HasNext,
            products.HasPrevious
        };

        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
        return Ok(productRes);
    }

    [HttpGet]
    [Route("top-8")]
    public async Task<IActionResult> GetBestSellers()
    {
        var products = await _productService.Get8BestSellerProduct();

        var productRes = new List<ProductResponse>();
        foreach (var item in products)
        {
            productRes.Add(new ProductResponse
            {
                Images = _productService.ConvertJsonToPhotos(item.PhotoJson),
                ProductId = item.ProductId,
                Name = item.Name,
                InStock = item.InStock,
                BasePrice = item.BasePrice,
                Description = item.Description,
                WeightServing = item.WeightServing,
                TotalSell = item.TotalSell,
                ProductCategoryId = item.ProductCategory.ProductCategoryId,
                StoreName = item.Store.ShopName,
                StoreId = item.Store.StoreId,
                CreatedOn = item.CreatedOn,
                Status = item.Status,
                TotalView = await _productService.GetProductViews(item)
            });
        }
        var metadata = new
        {
            products.TotalCount,
            products.PageSize,
            products.CurrentPage,
            products.TotalPages,
            products.HasNext,
            products.HasPrevious
        };

        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
        return Ok(productRes);
    }

    [HttpGet]
    [Route("{productId}", Name = "productId")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductDetails(int productId)
    {
        Customer customer = null;
        if (User.FindFirst("Id") is not null)
        {
            Guid userId = new Guid(User.FindFirst("Id")?.Value);
            customer = await _customerService.FindCustomerByIdAsync(userId);
        }
        var item = await _productService.FindProductById(productId);
        await _productService.AddProductHistory(customer, item);
        return item is not null ? Ok(new ProductResponse
        {
            Images = _productService.ConvertJsonToPhotos(item.PhotoJson),
            ProductId = item.ProductId,
            Name = item.Name,
            InStock = item.InStock,
            BasePrice = item.BasePrice,
            Description = item.Description,
            WeightServing = item.WeightServing,
            TotalSell = item.TotalSell,
            ProductCategoryId = item.ProductCategory is not null ? item.ProductCategory.ProductCategoryId : -1,
            StoreName = item.Store.ShopName,
            StoreId = item.Store.StoreId,
            CreatedOn = item.CreatedOn,
            Status = item.Status,
            TotalView = await _productService.GetProductViews(item)
        }) : NotFound(new { message = "product not found" });
    }

    [HttpGet("reviews")]
    public async Task<IActionResult> GetReviews([FromQuery] PagedProductReviewRequest request)
    {
        var reviews = await _productService.GetAllProductReviews(request);

        var metadata = new
        {
            reviews.TotalCount,
            reviews.PageSize,
            reviews.CurrentPage,
            reviews.TotalPages,
            reviews.HasNext,
            reviews.HasPrevious
        };

        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));

        return Ok(reviews);
    }

    [HttpPost]
    [Route("review")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "customer")]
    public async Task<IActionResult> PostReview([FromBody] ProductReviewRequest request)
    {
        Guid userId = new Guid(User.FindFirst("Id")?.Value);
        var customer = await _customerService.FindCustomerByIdAsync(userId);
        var product = await _productService.FindProductById(request.ProductId);
        ProductReview productReview = new ProductReview
        {
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedOn = DateTime.UtcNow,
            Product = product,
            Customer = customer
        };

        var addRes = await _context.ProductReviews.AddAsync(productReview);
        var changRes = await _context.SaveChangesAsync();

        return Ok(new { message = "Success" });
    }
    //[HttpGet]
    //[Route("categories/{Id}")]
    //public async Task<IActionResult> GetCategoriesByStoreId([FromRoute] string Id)
    //{
    //    try
    //    {
    //        var categoryNames = await _context.ProductCategories
    //            .Join(
    //                _context.Products,
    //                pc => pc.ProductCategoryId,
    //                p => p.ProductCategory.ProductCategoryId,
    //                (pc, p) => new { pc, p }
    //            )
    //            .Join(
    //                _context.Stores,
    //                pc_p => pc_p.p.Store.StoreId,
    //                s => s.StoreId,
    //                (pc_p, s) => new { pc_p, s }
    //            )
    //            .Where(joinResult => joinResult.s.UserId == Id)
    //            .Select(joinResult => joinResult.pc_p.pc.Name)
    //            .Distinct()
    //            .ToListAsync();

    //        return Ok(categoryNames);
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error");
    //    }
    //}
    [HttpGet]
    [Route("categories")]
    public async Task<IActionResult> GetCategories()
    {
        return Ok(await _context.ProductCategories.AsNoTracking().ToListAsync());
    }

    [HttpGet]
    [Route("categories/{categoryId}", Name = "categoryId")]
    public async Task<IActionResult> GetProductByCategories(int categoryId)
    {
        return Ok(await _productService.FindProductByCategoryId(categoryId));
    }

    [HttpGet]
    [Route("payment-method")]
    public async Task<IActionResult> GetPaymentMethods()
    {
        return Ok(await _context.PaymentMethods.AsNoTracking().ToListAsync());
    }
}