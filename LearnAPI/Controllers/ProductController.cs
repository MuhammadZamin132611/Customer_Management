using AutoMapper;
using LearnAPI.Helper;
using LearnAPI.Repos;
using LearnAPI.Repos.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace LearnAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IWebHostEnvironment environment;
        private readonly LearndataContext context;

        public ProductController(IWebHostEnvironment environment, LearndataContext context)
        {
            this.environment = environment;
            this.context = context;
        }

        [HttpPut("UploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile fromFile, string productcode )
        {
            APIResponse response = new APIResponse();
            try
            {
                string Filepath = GetFilePath(productcode);
                if(!System.IO.Directory.Exists(Filepath))
                {
                    System.IO.Directory.CreateDirectory(Filepath);
                }

                string imagepath = Filepath + "\\" + productcode + ".png";
                if(System.IO.File.Exists(imagepath))
                {
                    System.IO.File.Delete(imagepath);
                }
                using (FileStream stream = System.IO.File.Create(imagepath))
                {
                    await fromFile.CopyToAsync(stream);
                    response.ResponseCode = 200;
                    response.Result = "Pass";

                }
                
            }
            catch (Exception ex)
            {
                response.Errormessage = ex.Message;
            }
            return Ok(response);
        }

        [HttpPut("MultiUploadImage")]
        public async Task<IActionResult> MultiUploadImage(IFormFileCollection fileCollection, string productcode)
        {
            APIResponse response = new APIResponse();
            int passcount = 0; int errorcount = 0;
            try
            {
                string Filepath = GetFilePath(productcode);
                if (!System.IO.Directory.Exists(Filepath))
                {
                    System.IO.Directory.CreateDirectory(Filepath);
                }

                foreach(var file in fileCollection)
                {
                    string imagepath = Filepath + "\\" + file.FileName;
                    if (System.IO.File.Exists(imagepath))
                    {
                        System.IO.File.Delete(imagepath);
                    }
                    using (FileStream stream = System.IO.File.Create(imagepath))
                    {
                        await file.CopyToAsync(stream);
                        passcount++;
                    }

                }
            }
            catch (Exception ex)
            {
                errorcount++;
                response.Errormessage = ex.Message;
            }

            response.ResponseCode = 200;
            response.Result = passcount + "Files uploaded & " + errorcount + " FIles Failed";
            return Ok(response);
        }

        [HttpGet("GetImage")]
        public async Task<IActionResult> GetImage(string productcode)
        {
            string ImageUrl = string.Empty;
            string hostUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            try
            {
                string Filepath = GetFilePath(productcode);
                string imagepath = Filepath + "\\" + productcode + ".png";
                if (System.IO.File.Exists(imagepath))
                {
                    ImageUrl = hostUrl + "/Upload/product/" + productcode + "/" + productcode + ".png";
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                NotFound();
            }
            return Ok(ImageUrl);
        }


        [HttpGet("GetMultiImage")]
        public async Task<IActionResult> GetMultiImage(string productcode)
        {
            List<string> ImageUrl = new List<string>();
            string hostUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            try
            {
                string Filepath = GetFilePath(productcode);

                if (System.IO.Directory.Exists(Filepath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Filepath);
                    FileInfo[] fileInfos = directoryInfo.GetFiles();
                    foreach(FileInfo fileInfo in fileInfos)
                    {
                        string filename = fileInfo.Name;
                        string imagepath = Filepath + "\\" + filename;
                        if (System.IO.File.Exists(imagepath))
                        {
                            string _ImageUrl = hostUrl + "/Upload/product/" + productcode + "/" + filename;
                            ImageUrl.Add(_ImageUrl);
                        }
                        else { return NotFound(); }
                    }
                }

            }
            catch (Exception ex)
            {
                NotFound();
            }
            return Ok(ImageUrl);
        }

        [HttpGet("download")]
        public async Task<IActionResult> download(string productcode)
        {
            //string ImageUrl = string.Empty;
            //string hostUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            try
            {
                string Filepath = GetFilePath(productcode);
                string imagepath = Filepath + "\\" + productcode + ".png";
                if (System.IO.File.Exists(imagepath))
                {
                    MemoryStream stream = new MemoryStream();
                    using (FileStream fileStream = new FileStream(imagepath, FileMode.Open))
                    {
                        await fileStream.CopyToAsync(stream);
                    }
                    stream.Position = 0;
                    return File(stream, "image/png", productcode + ".png");
                    //ImageUrl = hostUrl + "/Upload/product/" + productcode + "/" + productcode + ".png";
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }


        [HttpDelete("remove")]
        public async Task<IActionResult> remove(string productcode)
        {
            //string ImageUrl = string.Empty;
            //string hostUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            try
            {
                string Filepath = GetFilePath(productcode);
                string imagepath = Filepath + "\\" + productcode + ".png";
                if (System.IO.File.Exists(imagepath))
                {
                    System.IO.File.Delete(imagepath);
                    return Ok("File Deleted");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpDelete("MultiRemove")]
        public async Task<IActionResult> MultiRemove(string productcode)
        {
            //string ImageUrl = string.Empty;
            //string hostUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            try
            {
                string Filepath = GetFilePath(productcode);
                
                if (System.IO.Directory.Exists(Filepath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Filepath);
                    FileInfo[] fileInfos = directoryInfo.GetFiles();
                    foreach(FileInfo fileInfo in fileInfos)
                    {
                        fileInfo.Delete();
                    }
                    return Ok("File Deleted");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpPost("DBMultiUploadImage")]
        public async Task<IActionResult> DBMultiUploadImage(IFormFileCollection fileCollection, string productcode)
        {
            APIResponse response = new APIResponse();
            int passcount = 0; int errorcount = 0;
            try
            {
                foreach (var file in fileCollection)
                {
                    using(MemoryStream stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        this.context.TblProductimages.Add(new TblProductimage()
                        {
                            Productcode = productcode,
                            Productimage = stream.ToArray()
                        });
                        await this.context.SaveChangesAsync();
                        passcount++;
                    }
                }
            }
            catch (Exception ex)
            {
                errorcount++;
                response.Errormessage = ex.Message;
            }

            response.ResponseCode = 200;
            response.Result = passcount + "Files uploaded & " + errorcount + " FIles Failed";
            return Ok(response);
        }

        [HttpGet("GetDBMultiImage")]
        public async Task<IActionResult> GetDBMultiImage(string productcode)
        {
            List<string> ImageUrl = new List<string>();
            //string hostUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            try
            {
                var _productimages = this.context.TblProductimages.Where(x => x.Productcode == productcode).ToList();
                if(_productimages!=null && _productimages.Count > 0)
                {
                    _productimages.ForEach(item =>
                    {
                        ImageUrl.Add(Convert.ToBase64String(item.Productimage));
                    });
                }
                else
                {
                    return NotFound();
                }
                //string Filepath = GetFilePath(productcode);

                //if (System.IO.Directory.Exists(Filepath))
                //{
                //    DirectoryInfo directoryInfo = new DirectoryInfo(Filepath);
                //    FileInfo[] fileInfos = directoryInfo.GetFiles();
                //    foreach (FileInfo fileInfo in fileInfos)
                //    {
                //        string filename = fileInfo.Name;
                //        string imagepath = Filepath + "\\" + filename;
                //        if (System.IO.File.Exists(imagepath))
                //        {
                //            string _ImageUrl = hostUrl + "/Upload/product/" + productcode + "/" + filename;
                //            ImageUrl.Add(_ImageUrl);
                //        }
                //        else { return NotFound(); }
                //    }
                //}

            }
            catch (Exception ex)
            {
                NotFound();
            }
            return Ok(ImageUrl);
        }

        [HttpGet("DBdownload")]
        public async Task<IActionResult> DBdownload(string productcode)
        {
            //string ImageUrl = string.Empty;
            //string hostUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            try
            {
                var _productimages = await this.context.TblProductimages.FirstOrDefaultAsync(x => x.Productcode == productcode);
                if (_productimages != null)
                {
                    return File(_productimages.Productimage, "image/png", productcode + ".png");
                }

                //string Filepath = GetFilePath(productcode);
                //string imagepath = Filepath + "\\" + productcode + ".png";
                //if (System.IO.File.Exists(imagepath))
                //{
                //    MemoryStream stream = new MemoryStream();
                //    using (FileStream fileStream = new FileStream(imagepath, FileMode.Open))
                //    {
                //        await fileStream.CopyToAsync(stream);
                //    }
                //    stream.Position = 0;
                //    return File(stream, "image/png", productcode + ".png");
                //    //ImageUrl = hostUrl + "/Upload/product/" + productcode + "/" + productcode + ".png";
                //}
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }


        [NonAction]
        private string GetFilePath(string productcode)
        {
            return this.environment.WebRootPath + "\\Upload\\product\\" + productcode;
        }
    }
}
