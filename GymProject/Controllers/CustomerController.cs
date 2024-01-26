﻿using FluentValidation;
using GymProject.Helpers;
using GymProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using GymProject.Validators;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GymProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private AlperyurtdasGymProjectContext _dbContext;
        public CustomerController(AlperyurtdasGymProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        [Route("MusteriEkle")]
        public async Task<ApiResponse> AddCustomer(Customer model)
        {
            try
            {
                var validator = new CustomerAddValidator();

                var result = await validator.ValidateAsync(model);

                if (!result.IsValid)
                {
                    return new ApiResponse("Error", $"Hata = {result.Errors}",null);
                }

                var customer = new Customer()
                {
                    CustomerId = NulidGenarator.Id(),
                    CustomerEmail = model.CustomerEmail,
                    CustomerIdentityNumber = model.CustomerIdentityNumber,
                    CustomerName = model.CustomerName,
                    CustomerPhoneNumber = model.CustomerPhoneNumber,
                    CustomerSurname = model.CustomerSurname
                };

                await _dbContext.Customers.AddAsync(customer);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", $"Başarıyla Eklendi", customer);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);

            }

        }

        [HttpGet]
        [Route("MusteriGetirById")]

        public async Task<ApiResponse> GetCustomer(string customerId)
        {
            try
            {
                var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.CustomerId == customerId);

                return customer != null ? new ApiResponse("Success", $"Başarıyla Getirildi", customer) 
                    : new ApiResponse("Error", $"Hata = Müşteri Bulunamadı.", null);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }

        }

        [HttpPost]
        [Route("TumMusterileriGetir")]

        public async Task<ApiResponse> GetAllCustomer()
        {
            try
            {
                var customerList = await _dbContext.Customers.ToListAsync();
                return new ApiResponse("Success", $"Başarıyla Getirildi", customerList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        [HttpPut]
        [Route("MusteriGuncelle")]

        public async Task<ApiResponse> UpdateCustomer(Customer model)
        {
            try
            {
                var validator = new CustomerUpdateValidator();

                var result = await validator.ValidateAsync(model);

                if (!result.IsValid)
                {
                    return new ApiResponse("Error", $"Hata = {result.Errors}", null);
                }

                var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.CustomerId == model.CustomerId);

                if (customer == null)
                {
                    return new ApiResponse("Error", $"Hata = Müşteri Bulunamadı.", null);
                }

                customer.CustomerName = model.CustomerName;
                customer.CustomerEmail = model.CustomerEmail;
                customer.CustomerPhoneNumber = model.CustomerPhoneNumber;
                customer.CustomerSurname = model.CustomerSurname;
                customer.CustomerIdentityNumber = model.CustomerIdentityNumber;

                _dbContext.Customers.Update(customer);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", $"Başarıyla Güncellendi", customer);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);

            }

        }

        [HttpDelete]
        [Route("MusteriSil")]

        public async Task<ApiResponse> DeleteCustomer(string id)
        {
            try
            {

                var validator = new CustomerDeleteValidator();

                var result = await validator.ValidateAsync(id);

                if (!result.IsValid)
                {
                    return new ApiResponse("Error", $"Hata = {result.Errors}", null);
                }

                var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.CustomerId == id);
                if (customer == null)
                    return new ApiResponse("Error", $"Hata = Müşteri Bulunamadı.", null);

                _dbContext.Customers.Remove(customer);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", $"Başarıyla Silindi", customer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }

        }
    }
}
