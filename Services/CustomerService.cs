using Microsoft.EntityFrameworkCore;
using api.Data;
using Microsoft.AspNetCore.Identity;
using api.Authentication.Dtos;
using api.Helpers;

namespace api.Services;

public class CustomerService
{

  private readonly AppDbContext _dbContext;
  private readonly IPasswordHasher<Customer> _passwordHasher;
  private readonly IEmailSender _emailSender;

  public CustomerService(AppDbContext dbContext, IPasswordHasher<Customer> passwordHasher, IEmailSender emailSender)
  {
    _dbContext = dbContext;
    _passwordHasher = passwordHasher;
    _emailSender = emailSender;

  }


  public async Task<PaginationResult<Customer>> GetAllCustomersService(int currentPage, int pageSize)
  {
    var totalCustomerCount = await _dbContext.Customers.CountAsync();
    var customer = await _dbContext.Customers
    .Include(a => a.Addresses)
    .Include(o => o.Orders)
      .ThenInclude(op => op.OrderProducts)
    .Include(r => r.Reviews)
    .Skip((currentPage - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();

    return new PaginationResult<Customer>
    {
      Items = customer,
      TotalCount = totalCustomerCount,
      CurrentPage = currentPage,
      PageSize = pageSize,
    };
  }


  public async Task<Customer?> GetCustomerById(Guid customerId)
  {
    return await _dbContext.Customers.FindAsync(customerId);
  }


  public async Task<Customer> CreateCustomerService(Customer newCustomer)
  {
    newCustomer.CustomerId = Guid.NewGuid();
    newCustomer.CreatedAt = DateTime.UtcNow;
    newCustomer.Password = _passwordHasher.HashPassword(newCustomer, newCustomer.Password);
    _dbContext.Customers.Add(newCustomer);
    await _dbContext.SaveChangesAsync();
    return newCustomer;
  }

  public async Task<LoginUserDto?> LoginCustomerService(LoginUserDto loginUserDto)
  {
    var customer = await _dbContext.Customers.SingleOrDefaultAsync(c => c.Email == loginUserDto.Email);
    if (customer == null)
    {
      return null;
    }
    var result = _passwordHasher.VerifyHashedPassword(customer, customer.Password, loginUserDto.Password);
    loginUserDto.UserId = customer.CustomerId;
    loginUserDto.IsAdmin = false;
    return result == PasswordVerificationResult.Success ? loginUserDto : null;

  }


  public async Task<Customer> UpdateCustomerService(Guid customerId, Customer updateCustomer)
  {
    var existingCustomer = await _dbContext.Customers.FindAsync(customerId);
    if (existingCustomer != null)
    {
      existingCustomer.FirstName = updateCustomer.FirstName ?? existingCustomer.FirstName;
      existingCustomer.LastName = updateCustomer.LastName ?? existingCustomer.LastName;
      existingCustomer.Email = updateCustomer.Email ?? existingCustomer.Email;
      existingCustomer.Password = updateCustomer.Password != null ? _passwordHasher.HashPassword(updateCustomer, updateCustomer.Password) : existingCustomer.Password;
      existingCustomer.Mobile = updateCustomer.Mobile ?? existingCustomer.Mobile;
      existingCustomer.Image = updateCustomer.Image ?? existingCustomer.Image;
      existingCustomer.IsBanned = updateCustomer.IsBanned;
      await _dbContext.SaveChangesAsync();
    }
    return existingCustomer;
  }


  public async Task<bool> DeleteCustomerService(Guid customerId)
  {
    var customerToRemove = await _dbContext.Customers.FindAsync(customerId);
    if (customerToRemove != null)
    {
      _dbContext.Customers.Remove(customerToRemove);
      await _dbContext.SaveChangesAsync();
      return true;
    }
    return false;
  }

  public async Task<bool> ForgotPasswordService(string email)
  {
    var customer = await _dbContext.Customers.FirstOrDefaultAsync(e => e.Email == email);
    if (customer == null)
    {
      return false;
    }

    var resetToken = Guid.NewGuid();

    customer.ResetToken = resetToken;
    customer.ResetTokenExpiration = DateTime.UtcNow.AddHours(1);
    // bc we still not have real host so i will just send a token so we can test it using swagger in the production adjust this 2 lines
    // string resetLink = $"http://localhost:5125/api/admins/reset-password?email={email}&token={resetToken}";

    await _emailSender.SendEmailAsync(email, "Password Reset", $"Dear {customer.FirstName},\nThis is your token {resetToken} to reset your password");
    await _dbContext.SaveChangesAsync();
    return true;

  }

  public async Task<bool> ResetPasswordService(ResetPasswordDto resetPasswordDto)
  {
    var customer = await _dbContext.Customers.FirstOrDefaultAsync(a => a.Email == resetPasswordDto.Email);
    if (customer == null || customer.ResetToken != resetPasswordDto.Token || customer.ResetTokenExpiration < DateTime.UtcNow)
    {
      return false;
    }
    customer.Password = _passwordHasher.HashPassword(customer, resetPasswordDto.NewPassword);
    customer.ResetToken = null;
    customer.ResetTokenExpiration = null;
    await _dbContext.SaveChangesAsync();
    return true;

  }


}