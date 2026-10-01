using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Seller;

[ApiController]
[Route("api/seller/products")]
public class ProductsController : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "Product.Create")]
    public IActionResult CreateProduct()
    {
        return Ok(new
        {
            message = "Product created successfully."
        });
    }
}