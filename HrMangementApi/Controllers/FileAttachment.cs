using HrMangementApi.Model;
using HrMangementApi.UserDbContext;
using Microsoft.AspNetCore.Cors;
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
    [EnableCors("AllowOrigin")]

    public class FileAttachment : ControllerBase
    {
        private readonly UserdbContext dataContext;
        private string photoFileName;
        private string photoPathForDb;
        private string resumeFileName;
        private string resumePathForDb;
        private string certFileName;
        private string certPathForDb;

        public FileAttachment(UserdbContext _dataContext)
        {
            dataContext = _dataContext;
        }
        [HttpPost("Attachment"), DisableRequestSizeLimit]

        public IActionResult UploadFileAttachment(IFormFile files, string fileType)
        {
            for (int i = 0; i < Request.Form.Files.Count; ++i)
            {
                var file = Request.Form.Files[i];
                var date = DateTime.Now.Date.Month.ToString() + "-" + DateTime.Now.Date.Year.ToString() + "-" + DateTime.Now.Day.ToString();
                if (file.Name == "image")
                {
                    photoFileName = file.FileName;
                    photoPathForDb = Path.Combine("Resource", "Images", date, photoFileName);
                    var photoFolderName = Path.Combine(Directory.GetCurrentDirectory(), "Resource", "Images", date);
                    Directory.CreateDirectory(photoFolderName);
                    var photoPath = Path.Combine(photoFolderName, photoFileName).ToString();
                    using (var stream = new FileStream(photoPath, FileMode.Append))
                    {
                        file.CopyTo(stream);
                    }
                }
                else if (file.Name == "resume")
                {
                    resumeFileName = file.FileName;
                    resumePathForDb = Path.Combine("Resource", "Images", date, resumeFileName);
                    var resumeFolderName = Path.Combine(Directory.GetCurrentDirectory(), "Resource", "Images", date);
                    Directory.CreateDirectory(resumeFolderName);
                    var photoPath = Path.Combine(resumeFolderName, resumeFileName).ToString();
                    using (var stream = new FileStream(photoPath, FileMode.Append))
                    {
                        file.CopyTo(stream);
                    }
                }
                else
                {
                    certFileName = file.FileName;
                    certPathForDb = Path.Combine("Resource", "Images", date, certFileName);
                    var certFolderName = Path.Combine(Directory.GetCurrentDirectory(), "Resource", "Images", date);
                    Directory.CreateDirectory(certFolderName);
                    var photoPath = Path.Combine(certFolderName, certFileName).ToString();
                    using (var stream = new FileStream(photoPath, FileMode.Append))
                    {
                        file.CopyTo(stream);
                    }
                }
            }
            var fileDetails = SaveFileToDB(photoFileName, photoPathForDb, resumeFileName, resumePathForDb, certFileName, certPathForDb);

            return Ok(fileDetails);
        }
        private FileAttachmentModel SaveFileToDB(string photoName, string photoPath, string resumeName, string resumePath, string certificateName, string certificatePath)
        {
            var objFiles = new FileAttachmentModel()
            {
                AttachmentId = 0,
                PhotoName = photoName,
                PhotoPath = photoPath,
                ResumeName = resumeName,
                ResumePath = resumePath,
                CertificateName = certificateName,
                CertificatePath = certificatePath

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
            var filepath = Path.Combine(Directory.GetCurrentDirectory(), file.PhotoPath);
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


