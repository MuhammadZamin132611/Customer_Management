﻿using ClosedXML.Excel;
using LearnAPI.Modal;
using LearnAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Data;

namespace LearnAPI.Controllers
{
    //[Authorize]
    //[DisableCors]
    //[EnableRateLimiting("fixedwindow")]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService service;
        private readonly IWebHostEnvironment environment;

        public CustomerController(ICustomerService service, IWebHostEnvironment environment)
        {
            this.service = service;
            this.environment = environment;
        }

        //[AllowAnonymous]
        //[EnableCors("corspolicy1")]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var data = await service.GetAll();
            if(data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }

        //[DisableRateLimiting]
        [HttpGet("GetbyCode")]
        public async Task<IActionResult> GetbyCode(string code)
        {
            var data = await service.GetbyCode(code);
            if(data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(Customermodal _data)
        {
            var data = await service.Create(_data);
            return Ok(data);
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update(Customermodal _data, string code)
        {
            var data = await service.Update(_data, code);
            return Ok(data);
        }


        [HttpDelete("Remove")]
        public async Task<IActionResult> Remove(string code)
        {
            var data = await service.Remove(code);
            return Ok(data);
        }

        [AllowAnonymous]
        [HttpGet("Exportexcel")]
        public async Task<IActionResult> Exportexcel()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Code",typeof(string));
                dt.Columns.Add("Name",typeof(string));
                dt.Columns.Add("Email",typeof(string));
                dt.Columns.Add("Phone",typeof(string));
                dt.Columns.Add("CreditLimit",typeof(int));
                var data = await this.service.GetAll();
                if(data != null && data.Count > 0)
                {
                    data.ForEach(item =>
                    {
                        dt.Rows.Add(item.Code, item.Name, item.Email, item.Phone, item.Creditlimit);
                    });
                }
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.AddWorksheet(dt, "Customer Info");
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CustomerInfo.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [AllowAnonymous]
        [HttpGet("Excel")]
        public async Task<IActionResult> Excel()
        {
            try
            {
                string path = GetFilePath();
                string excelpath = path + "\\CustomerInfo.xlsx";
                DataTable dt = new DataTable();
                dt.Columns.Add("Code", typeof(string));
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("Email", typeof(string));
                dt.Columns.Add("Phone", typeof(string));
                dt.Columns.Add("CreditLimit", typeof(int));
                var data = await this.service.GetAll();
                if (data != null && data.Count > 0)
                {
                    data.ForEach(item =>
                    {
                        dt.Rows.Add(item.Code, item.Name, item.Email, item.Phone, item.Creditlimit);
                    });
                }
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.AddWorksheet(dt, "Customer Info");
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);

                        if(System.IO.File.Exists(excelpath))
                        {
                            System.IO.File.Delete(excelpath);
                        }
                        wb.SaveAs(excelpath);

                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CustomerInfo.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }


        [NonAction]
        private string GetFilePath()
        {
            return this.environment.WebRootPath + "\\Export\\";
        }
    }
}
