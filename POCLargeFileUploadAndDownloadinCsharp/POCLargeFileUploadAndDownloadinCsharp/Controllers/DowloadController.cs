using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using static System.Collections.Specialized.BitVector32;

namespace POCLargeFileUploadAndDownloadinCsharp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DowloadController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DowloadController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }


        [HttpGet("smallfile/{fileName}")]
        public IActionResult GetSmallFile(string fileName)
        {
            string filePathWithName = Path.Combine(_webHostEnvironment.ContentRootPath + "/UploadedFiles",
                fileName);

            if (!System.IO.File.Exists(filePathWithName))
            {
                return NotFound();
            }

            var fileStream = new FileStream(filePathWithName, FileMode.Open, FileAccess.Read);
            return File(fileStream, GetMimeType(filePathWithName));

        }

        [HttpGet("{fileName}")]
        public async void Get(string fileName)
        {
            try
            {
                string filePathWithName = Path.Combine(_webHostEnvironment.ContentRootPath + "/UploadedFiles",
                    fileName);

                if (!System.IO.File.Exists(filePathWithName))
                {
                    return;
                }
                Response.ContentType = GetMimeType(filePathWithName);
                Response.Headers.Add("Content-Disposition", "attachment; filename=" + fileName);

                // This will used for sending the small files
                await Response.SendFileAsync(filePathWithName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            //using (var fileStream = new FileStream(filePathWithName, FileMode.Open))
            //{
            //byte[] buffer = new byte[2048];
            //int totalBytesRead = 0;
            //int buflen = 0;
            //while (fileStream.Length > totalBytesRead)
            //{
            //    if ((totalBytesRead + 1024) < fileStream.Length)
            //    {
            //        buflen =(int) fileStream.Length - totalBytesRead;
            //        // copy limited size

            //    }
            //    else
            //    {
            //        // copy now...
            //        buflen = (int)fileStream.Length - totalBytesRead;
            //    }

            //    await fileStream.ReadAsync(buffer, 0, buflen);
            //    await Response.W(buffer);
            //    totalBytesRead += buflen;
            //}

            //            }
        }

        private string GetMimeType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
    }
}
