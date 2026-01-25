using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Data.Enums
{
    public enum MovieStatus
    {
        Draft = 0,       // Not published
        Active = 1,      // Published & showing
        Archived = 2,    // No longer showing
        ComingSoon = 3   // Announced but not released
    }
}
