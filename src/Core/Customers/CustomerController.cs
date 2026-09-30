using System.Security.Claims;
using BotSaaS.Api.Shared.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BotSaaS.Api.Core.Customers;

// Protected route: manages customers of the caller's company (tenant from JWT).
[Authorize]
[ApiController]
[Route("api/customers")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    // Owner lists all customers of their company
    [HttpGet]
    public async Task<IActionResult> GetCustomers()
    {
        string? companyId = User.FindFirstValue("companyId");
        if (!Guid.TryParse(companyId, out Guid companyGuid))
        {
            return Unauthorized(new { error = "Empresa inválida" });
        }

        List<CustomerResponse> customers = await _customerService.GetCustomers(companyGuid);
        return Ok(customers);
    }

    // Owner retrieves a single customer by ID
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCustomerById(Guid id)
    {
        string? companyId = User.FindFirstValue("companyId");
        if (!Guid.TryParse(companyId, out Guid companyGuid))
        {
            return Unauthorized(new { error = "Empresa inválida" });
        }

        Result<Customer> result = await _customerService.GetCustomerById(id, companyGuid);
        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error });
        }

        Customer customer = result.Value!;
        CustomerResponse response = new CustomerResponse(customer.Id, customer.Name, customer.Phone, customer.CreatedAt);
        return Ok(response);
    }

    // Owner creates a new customer manually
    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        string? companyId = User.FindFirstValue("companyId");
        if (!Guid.TryParse(companyId, out Guid companyGuid))
        {
            return Unauthorized(new { error = "Empresa inválida" });
        }

        Result<Customer> result = await _customerService.FindOrCreateCustomer(companyGuid, request.Phone, request.Name);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        Customer customer = result.Value!;
        CustomerResponse response = new CustomerResponse(customer.Id, customer.Name, customer.Phone, customer.CreatedAt);
        return Created($"/api/customers/{customer.Id}", response);
    }
}
