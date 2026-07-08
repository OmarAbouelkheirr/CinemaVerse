/** Represents a .NET TimeSpan value returned by the API for movie duration. */
export interface IMovieDuration {
  ticks: number;
  days: number;
  hours: number;
  milliseconds: number;
  microseconds: number;
  nanoseconds: number;
  minutes: number;
  seconds: number;
  totalDays: number;
  totalHours: number;
  totalMilliseconds: number;
  totalMicroseconds: number;
  totalNanoseconds: number;
  totalMinutes: number;
  totalSeconds: number;
}
