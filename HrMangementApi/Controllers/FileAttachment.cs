using HrMangementApi.Model;
using HrMangementApi.UserDbContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HrMangementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileAttachment : ControllerBase
    {
        private readonly UserdbContext dataContext;
        public FileAttachment(UserdbContext _dataContext)
        {
            dataContext = _dataContext;
        }
        [HttpPost("Attachment"), DisableRequestSizeLimit]

        public IActionResult UploadFileAttachment(IFormFile files, string fileType)
        {
            try
            {
                var file = Request.Form.Files[0];
                fileType = Request.Form["fileType"];
                var date = DateTime.Now.Date.Month.ToString() + " " + DateTime.Now.Date.Year.ToString() + " " + DateTime.Now.Day.ToString();
                var folderName = Path.Combine("Resource", "Images", date);
                var pathtoSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (file.Length > 0)
                {
                    Directory.CreateDirectory(pathtoSave);
                    var fileName = file.FileName.Trim('"');
                    var fullPath = Path.Combine(pathtoSave, fileName).ToString();
                    var fileExtension = Path.GetExtension(fileName);
                    var dbpath = Path.Combine(folderName, fileName);
                    var filePathAttachment = Path.Combine(folderName, fileName).ToString();
                    using (var stream = new FileStream(fullPath, FileMode.Append))
                    {
                        file.CopyTo(stream);
                    }
                    var fileDetails = SaveFileToDB(fileName, fileType, filePathAttachment);

                    return Ok(fileDetails);
                }

                return BadRequest();
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        private FileAttachmentModel SaveFileToDB(string fileName, string fileType, string filePathAttachment)
        {
            var objFiles = new FileAttachmentModel()
            {
                AttachmentId = 0,
                AttachmentName = fileName,
                AttachmentType = fileType,
                AttachmentPath = filePathAttachment
            };

            dataContext.FileAttachment.Add(objFiles);
            dataContext.SaveChanges();
            return objFiles;
        }

        [HttpGet("atttchmentFile")]
        public IActionResult GetAttachmentPath()
        {
            var user = dataContext.FileAttachment.AsQueryable();
            return Ok(user);
        }

        [HttpGet("GetAttachmentDetails")]
        public IActionResult GetAttachmentDetails(int candidateId)
        {
            var userData = dataContext.EmployeeModel.Where(a => a.EmployeeId == candidateId)
                .FirstOrDefault();
            var attachmentList = new List<FileAttachmentModel>();
            if (userData != null)
            {
                var attamenctIds = userData.AttachmentIds.Split(',');

                if (attamenctIds.Any())
                {
                    foreach (var attamenctId in attamenctIds)
                    {
                        var attachment = dataContext.FileAttachment.Where(n => n.AttachmentId.ToString() == attamenctId).FirstOrDefault();
                        attachmentList.Add(attachment);
                    }
                }
            }
            return Ok(attachmentList);
        }

        [HttpGet("Download")]
        public IActionResult DownloadFileAttachment(int id)
        {
            var file = dataContext.FileAttachment.Where(n => n.AttachmentId == id).FirstOrDefault();
            var filepath = Path.Combine(Directory.GetCurrentDirectory(), file.AttachmentPath);
            if (!System.IO.File.Exists(filepath))
                return NotFound();
            var memory = new MemoryStream();
            using (var stream = new FileStream(filepath, FileMode.Open))
            {
                stream.CopyTo(memory);
            }
            memory.Position = 0;
            return File(memory, GetContentType(filepath), filepath);
        }
        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            if (!provider.TryGetContentType(path, out contentType))
            {

                contentType = "application/octet-stream";
            }

            return contentType;
        }
    }

}


