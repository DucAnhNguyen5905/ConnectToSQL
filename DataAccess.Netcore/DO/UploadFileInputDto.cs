using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DataAccess.Netcore.DO
{
    public class UploadFileInputDto
    {
        [Required]
        public IFormFile? File { get; set; }
    }

}
