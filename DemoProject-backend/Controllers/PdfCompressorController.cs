using DemoProject_backend.Helper;
using iLovePdf.Core;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.NetworkInformation;
using ZstdSharp;
using static DemoProject_backend.Dtos.PdfCompressorDto;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DemoProject_backend.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class PdfCompressorController : Controller
    {

            [HttpPost("compress")]
        //    public async Task<IActionResult> CompressPdf(IFormFile file)
        //    {
        //        if (file == null || file.Length == 0)
        //            return BadRequest("No file uploaded.");

        //        // Temp file paths
        //        //Create a temp file path returns the full path
        //        var tempInputPath = Path.GetTempFileName();

        //        //return temp path 
        //        var tempOutputPath = Path.Combine(Path.GetTempPath(), $"compressed_{Path.GetFileName(file.FileName)}");

        //        // Save uploaded file temporarily
        //        //Saves the uploaded file to disk at tempInputPath. because the compressor will work with file path not the IFromFile
        //        using (var stream = new FileStream(tempInputPath, FileMode.Create))
        //        {
        //            await file.CopyToAsync(stream);
        //        }

        //        PdfCompressor.SmartCompress(tempInputPath, tempOutputPath);

        //        // Read compressed PDF into memory and return it
        //        var compressedBytes = await System.IO.File.ReadAllBytesAsync(tempOutputPath);

        //        //MIME stands for Multipurpose Internet Mail Extensions.
        //        //It’s a standard way to describe the type of a file or data being sent over the internet, especially in HTTP(web) communication.
        //        var contentType = "application/pdf";

        //        // Option 1: Return compressed file as download
        //        return File(compressedBytes, contentType, $"compressed_{file.FileName}");

        //}


        public async Task<IActionResult> CompressPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var inputPath = Path.GetTempFileName();
            var outputPath = Path.ChangeExtension(Path.GetTempFileName(), ".pdf");

            // Save uploaded file
            using (var stream = new FileStream(inputPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Run Ghostscript
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "gswin64c", // Ensure this is in your PATH
                    Arguments = $"-sDEVICE=pdfwrite -dCompatibilityLevel=1.4 -dDownsampleColorImages=true -dColorImageResolution=150 -dNOPAUSE -dQUIET -dBATCH -sOutputFile=\"{outputPath}\" \"{inputPath}\"",
               

                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            // Return compressed PDF
            var bytes = await System.IO.File.ReadAllBytesAsync(outputPath);
            return File(bytes, "application/pdf", "compressed.pdf");
        }


        //public async Task<IActionResult> CompressPdfWithILovePdf([FromForm] IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //        return BadRequest("No file uploaded.");

        //    string publicKey = "project_public_95461401d4357bfe6c623e145b4a87de_gK31X51a71ca20716f620caecc8317a5bb917"; // from ilovepdf dashboard

        //    using var httpClient = new HttpClient();

        //    // 1. Start compression task
        //    var startResponse = await httpClient.PostAsJsonAsync("https://api.ilovepdf.com/v1/start/compress", new
        //    {
        //        public_key = publicKey
        //    });

        //    if (!startResponse.IsSuccessStatusCode)
        //        return StatusCode(500, "Failed to start compression task");

        //    var taskJson = await startResponse.Content.ReadFromJsonAsync<Dtos.PdfCompressorDto.StartTaskResponse>();
        //    var taskId = taskJson.task ;
        //    var server = taskJson.server;

        //    // 2. Upload file
        //    using var form = new MultipartFormDataContent();
        //    form.Add(new StringContent(taskId), "task");
        //    form.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);

        //    var uploadResponse = await httpClient.PostAsync($"{server}/v1/upload", form);
        //    if (!uploadResponse.IsSuccessStatusCode)
        //        return StatusCode(500, "Upload failed");

        //    var uploadJson = await uploadResponse.Content.ReadFromJsonAsync<UploadedFileResponse>();

        //    // 3. Process compression
        //    var processPayload = new
        //    {
        //        task = taskId,
        //        tool = "compress",
        //        files = new[] { new { server_filename = uploadJson.server_filename } },
        //        compression_level = "recommended"
        //    };

        //    var processResponse = await httpClient.PostAsJsonAsync($"{server}/v1/process", processPayload);
        //    if (!processResponse.IsSuccessStatusCode)
        //        return StatusCode(500, "Compression failed");

        //    // 4. Download file
        //    var fileBytes = await httpClient.GetByteArrayAsync($"{server}/v1/download/{taskId}");

        //    return File(fileBytes, "application/pdf", "compressed.pdf");
        //}
    }
}
