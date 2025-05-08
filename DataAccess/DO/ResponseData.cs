using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DO
{
    public class ResponseData
    {
        public int responseCode { get; set; }

        public string Data { get; set; }  // Hoặc kiểu dữ liệu khác bạn cần
        public string responseMessage { get; set; }
    }
}