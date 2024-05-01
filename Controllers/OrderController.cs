using Microsoft.AspNetCore.Mvc;
using api.Services;
using api.Helpers;

namespace api.Controllers;

[ApiController]
[Route("/api/orders")]
public class OrderController : ControllerBase
{
  public OrderService _dbContext;
  public OrderController(OrderService orderService)
  {
    _dbContext = orderService;
  }


  [HttpGet]
  public async Task<IActionResult> GetAllOrders()
  {
    try
    {

      var orders = await _dbContext.GetAllOrderService();
      if (orders.ToList().Count < 1)
      {
        return NotFound(new ErrorMessage
        {
          Message = "No Orders To Display"
        });
      }
      return Ok(new SuccessMessage<IEnumerable<Order>>
      {
        Message = "Orders are returned succeefully",
        Data = orders
      });
    }
    catch (Exception ex)
    {
      Console.WriteLine($"An error occured , can not return the Order list");
      return StatusCode(500, new ErrorMessage
      {
        Message = ex.Message
      });
    }
  }


  [HttpGet("{orderId:guid}")]
  public async Task<IActionResult> GetOrder(string orderId)
  {
    try
    {
      if (!Guid.TryParse(orderId, out Guid orderIdGuid))
      {
        return BadRequest("Invalid Order ID Format");
      }
      var order = await _dbContext.GetOrderByIdService(orderIdGuid);
      if (order == null)
      {
        return NotFound(new ErrorMessage
        {
          Message = $"No Order Found With ID : ({orderIdGuid})"
        });
      }
      else
      {
        return Ok(new SuccessMessage<Order>
        {
          Success = true,
          Message = "Order is returned succeefully",
          Data = order
        });
      }

    }
    catch (Exception ex)
    {
      Console.WriteLine($"An error occured , can not return the Order");
      return StatusCode(500, new ErrorMessage
      {
        Message = ex.Message
      });
    }
  }


  [HttpPost]
  public async Task<IActionResult> CreateOrder(Order newOrder)
  {
    try
    {

      var createdOrder = await _dbContext.CreateOrderService(newOrder);
      if (createdOrder != null)
      {
        return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.OrderId }, createdOrder);
      }
      return Ok(new SuccessMessage<Order>
      {
        Message = "Order is created succeefully",
        Data = createdOrder
      });
    }
    catch (Exception ex)
    {
      Console.WriteLine($"An error occured , can not create new Order");
      return StatusCode(500, new ErrorMessage
      {
        Message = ex.Message
      });
    }
  }


  [HttpPut("{orderId:guid}")]
  public async Task<IActionResult> UpdateOrder(string orderId, Order updateOrder)
  {
    try
    {
      if (!Guid.TryParse(orderId, out Guid orderIdGuid))
      {
        return BadRequest("Invalid Order ID Format");
      }
      var order = await _dbContext.UpdateOrderService(orderIdGuid, updateOrder);
      if (order == null)


      {
        return NotFound(new ErrorMessage
        {
          Message = "No Order To Founed To Update"
        });
      }
      return Ok(new SuccessMessage<Order>
      {
        Message = "Order Is Updated Succeefully",
        Data = order
      });
    }
    catch (Exception ex)
    {
      Console.WriteLine($"An error occured , can not update the Order ");
      return StatusCode(500, new ErrorMessage
      {
        Message = ex.Message
      });
    }
  }


  [HttpDelete("{orderId:guid}")]
  public async Task<IActionResult> DeleteOrder(string orderId)
  {
    try
    {
      if (!Guid.TryParse(orderId, out Guid OrderId_Guid))
      {
        return BadRequest("Invalid Order ID Format");
      }
      var result = await _dbContext.DeleteOrderService(OrderId_Guid);
      if (!result)


      {
        return NotFound(new ErrorMessage
        {
          Message = "The Order is not found to be deleted"
        });
      }
      return Ok(new { success = true, message = " Order is deleted succeefully" });
    }

    catch (Exception ex)
    {
      Console.WriteLine($"An error occured , the Order can not deleted");
      return StatusCode(500, new ErrorMessage
      {
        Message = ex.Message
      });
    }
  }
}
