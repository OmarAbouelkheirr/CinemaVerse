using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Services.DTOs.UserFlow.Auth
{
    public class RefreshRequestDto
    {
        public string RefreshToken { get; set; }
        public string Email { get; set; }
    }
}
