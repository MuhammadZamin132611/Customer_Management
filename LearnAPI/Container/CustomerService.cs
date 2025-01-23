﻿using AutoMapper;
using LearnAPI.Helper;
using LearnAPI.Modal;
using LearnAPI.Repos;
using LearnAPI.Repos.Models;
using LearnAPI.Service;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LearnAPI.Container
{
    public class CustomerService : ICustomerService
    {
        private readonly LearndataContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerService> logger;

        public CustomerService(LearndataContext context, IMapper mapper, ILogger<CustomerService> logger)
        {
            _context = context;
            _mapper = mapper;
            this.logger = logger;
        }

        public async Task<APIResponse> Create(Customermodal data)
        {
            APIResponse response = new APIResponse();
            try
            {
                this.logger.LogInformation("Create Begins");
                TblCustomer _customer = _mapper.Map<Customermodal, TblCustomer>(data);
                await _context.TblCustomers.AddAsync(_customer);
                await _context.SaveChangesAsync();
                response.ResponseCode = 201;
                response.Result = data.Code;
            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.Errormessage = ex.Message;
                this.logger.LogError(ex.Message,ex);
            }
            return response;
        }

        public async Task<List<Customermodal>> GetAll()
        {
            List<Customermodal> _response = new List<Customermodal>();
            var _data = await _context.TblCustomers.ToListAsync();
            if (_data != null)
            {
                _response = _mapper.Map<List<TblCustomer>, List<Customermodal>>(_data);
            }
            return _response;
        }

        public async Task<Customermodal> GetbyCode(string code)
        {
            Customermodal _response = new Customermodal();
            var _data = await _context.TblCustomers.FindAsync(code);
            if (_data != null)
            {
                _response = _mapper.Map<TblCustomer, Customermodal>(_data);
            }
            return _response;
        }

        public async Task<APIResponse> Remove(string code)
        {
            APIResponse response = new APIResponse();
            try
            {
                var _customer = await this._context.TblCustomers.FindAsync(code);
                if (_customer != null)
                {
                    _context.TblCustomers.Remove(_customer);
                    await _context.SaveChangesAsync();
                    response.ResponseCode = 200;
                    response.Result = code;
                }
                else
                {
                    response.ResponseCode = 404;
                    response.Errormessage = "Customer Data not found!";
                }
               
            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.Errormessage = ex.Message;
            }
            return response;
        }

        public async Task<APIResponse> Update(Customermodal data, string code)
        {
            APIResponse response = new APIResponse();
            try
            {
                var _customer = await this._context.TblCustomers.FindAsync(code);
                if (_customer != null)
                {
                   _customer.Name = data.Name;
                    _customer.Email = data.Email;
                    _customer.Phone = data.Phone;
                    _customer.IsActive = data.IsActive;
                    _customer.Creditlimit = data.Creditlimit;
                    await _context.SaveChangesAsync();
                    response.ResponseCode = 200;
                    response.Result = code;
                }
                else
                {
                    response.ResponseCode = 404;
                    response.Errormessage = "Customer Data not found!";
                }

            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.Errormessage = ex.Message;
            }
            return response;
        }
    }
}
