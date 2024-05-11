using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using POCLargeFileUploadAndDownloadinCsharp.MultipartHelper;
using System.Collections;
using System.Net;

namespace POCLargeFileUploadAndDownloadinCsharp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UploadController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("UploadSmallFile")]
        public IActionResult UploadSmallFile(IFormFile file)
        {

            var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                file.FileName);
            var trustedFileNameForFileStorage = Path.GetRandomFileName() + Path.GetExtension(file.FileName);
            string filename = Path.Combine(_webHostEnvironment.ContentRootPath + "/UploadedFiles",
                trustedFileNameForFileStorage);
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                file.CopyToAsync(fs);
            }
            return Ok();
        }


        [HttpPost("Largefile")]
        //[DisableFormValueModelBinding]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPhysical()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                ModelState.AddModelError("File",
                    $"The request couldn't be processed (Error 1).");
                // Log error

                return BadRequest(ModelState);
            }

            var boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(Request.ContentType),
                70);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();

            while (section != null)
            {

                var hasContentDispositionHeader =
                    ContentDispositionHeaderValue.TryParse(
                        section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    // This check assumes that there's a file
                    // present without form data. If form data
                    // is present, this method immediately fails
                    // and returns the model error.
                    if (!MultipartRequestHelper
                        .HasFileContentDisposition(contentDisposition))
                    {
                        ModelState.AddModelError("File",
                            $"The request couldn't be processed (Error 2).");
                        // Log error

                        return BadRequest(ModelState);
                    }
                    else
                    {
                        // Don't trust the file name sent by the client. To display
                        // the file name, HTML-encode the value.
                        var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                                contentDisposition.FileName.Value);
                        var trustedFileNameForFileStorage = Path.GetRandomFileName() + Path.GetExtension(contentDisposition.FileName.Value);

                        // **WARNING!**
                        // In the following example, the file is saved without
                        // scanning the file's contents. In most production
                        // scenarios, an anti-virus/anti-malware scanner API
                        // is used on the file before making the file available
                        // for download or for use by other systems. 
                        // For more information, see the topic that accompanies 
                        // this sample.

                        //var streamedFileContent = await FileHelpers.ProcessStreamedFile(
                        //    section, contentDisposition, ModelState,
                        //    _permittedExtensions, _fileSizeLimit);

                        if (!ModelState.IsValid)
                        {
                            return BadRequest(ModelState);
                        }


                        string filename = Path.Combine(_webHostEnvironment.ContentRootPath + "/UploadedFiles",
                            trustedFileNameForFileStorage);
                        using (FileStream fs = new FileStream(filename, FileMode.Create))
                        {
                            await section.Body.CopyToAsync(fs);
                        }

                        // You can write our own custom logic for the same...
                        //using (var targetStream = System.IO.File.Create(filename))
                        //{
                        //    int buflen = 1024;
                        //    byte[] buf = new byte[buflen];
                        //    int totalBytesRead = 0;

                        //    while (section.Body.Length > totalBytesRead)
                        //    {
                        //        if ((totalBytesRead + 1024) < section.Body.Length)
                        //        {
                        //            //buflen = section.Body.Length - totalBytesRead;
                        //            // Copy limited size

                        //        }
                        //        else
                        //        {
                        //            // Copy now...
                        //            buflen= (int)section.Body.Length - totalBytesRead;
                        //        }

                        //        await section.Body.ReadAsync(buf, totalBytesRead, buflen);
                        //        await targetStream.WriteAsync(buf, 0, buflen);
                        //        totalBytesRead += 1024;
                        //    }
                        //}
                    }
                }

                // Drain any remaining section body that hasn't been consumed and
                // read the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }

            return Created(nameof(UploadController), null);
        }
    }
}
