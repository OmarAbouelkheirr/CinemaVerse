using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.Constants;

// Fixed capacity and layout per hall type. Capacity is derived from type only (no admin input).
// Each type has a different shape (gaps, seat columns). Layout uses full rows only (no half rows).
public static class HallTypeLayoutConfig
{
    // Returns the fixed capacity for the given hall type.
    // TwoD (largest) → VIP (smallest).

    public static int GetCapacity(HallType hallType)
    {
        return hallType switch
        {
            HallType.TwoD => 80,     // 10 rows × 8 seats (gap 5-6)
            HallType.ThreeD => 72,   // 9 rows × 8 seats (gap 5-8)
            HallType.IMAX => 63,     // 7 rows × 9 seats (gap 6-10)
            HallType.ScreenX => 56,  // 7 rows × 8 seats (gaps sides + middle)
            HallType.VIP => 24,      // 6 rows × 4 seats (gap 3-8)
            _ => 80
        };
    }

    // Returns the fixed layout for the given hall type (different shape per type: gaps, seat columns).
    // Capacity = NumberOfRows × SeatColumns.Count (full rows only).
    public static HallLayoutInfo GetLayout(HallType hallType)
    {
        return hallType switch
        {
            // Standard 2D: 10 rows × 8 seats; columns 1-4 and 7-10 (small gap 5-6 in middle)
            HallType.TwoD => new HallLayoutInfo(10, Enumerable.Range(1, 10).Where(c => c is <= 4 or >= 7).ToList()),
            // 3D: 9 rows × 8 seats; columns 1-4 and 9-12 (wider gap 5-8 in middle)
            HallType.ThreeD => new HallLayoutInfo(9, Enumerable.Range(1, 12).Where(c => c is <= 4 or >= 9).ToList()),
            // IMAX: 7 rows × 9 seats; columns 1-5 and 11-14 (wide gap 6-10 in middle)
            HallType.IMAX => new HallLayoutInfo(7, Enumerable.Range(1, 14).Where(c => c is <= 5 or >= 11).ToList()),
            // ScreenX: 7 rows × 8 seats; columns 2-5 and 10-13 (gaps on sides and middle for curved screen)
            HallType.ScreenX => new HallLayoutInfo(7, Enumerable.Range(1, 14).Where(c => (c >= 2 && c <= 5) || (c >= 10 && c <= 13)).ToList()),
            // VIP: 6 rows × 4 seats; columns 1-2 and 9-10 (huge gap 3-8 for luxury)
            HallType.VIP => new HallLayoutInfo(6, Enumerable.Range(1, 10).Where(c => c is <= 2 or >= 9).ToList()),
            _ => new HallLayoutInfo(10, Enumerable.Range(1, 10).Where(c => c is <= 4 or >= 7).ToList())
        };
    }
}

public sealed record HallLayoutInfo(int NumberOfRows, List<int> SeatColumns)
{
    public int Capacity => NumberOfRows * SeatColumns.Count;
}
