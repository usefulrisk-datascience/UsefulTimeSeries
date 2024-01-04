using System;
using System.Xml;
using MathNet.Numerics.Distributions;
namespace UsefulTimeSeries
{
    public class UsefulDatesTimes
    {
        public const long microsecond = 100;
        public const long millisecond = 100 * microsecond;
        public const long second = 1000 * millisecond;
        public const long minute = 60 * second;
        public const long hour = 60 * minute;
        public const long day = 24 * hour;
        public const long week = 7 * day;
        public const long month = 30 * day;
        public const long year = 365 * day;

        // Un tick = 100 nanosecondes = 0.1 microsecondes = 10 000 millisecondes.
        // 1 seconde = 1 000 microsecondes = 1 000 000 nanosecondes

        /**
         *Un TimeSpan est une durée. Elle  s'exprime comme suit:
         * TimeSpan ex1 = new TimeSpan(day,hour,minute,second,millisecond,microsecond);
         * Chaque fois qu'on fait appel à une propriété genre: ex1.Seconds il donnera le champ secondes.
         * Par contre si on demande la propriété ex1.Ticks il donne la somme des Ticks que représente le TimeSpan
         * et pas un multiple des microsecondes
         **/
        public static Int64 Rfc3339ToTicks(string rfc3339)
        {
            DateTimeOffset rfc3339Date = DateTimeOffset.ParseExact(rfc3339,"yyyy-MM-dd'T'HH:mm:ss.fffffffK", System.Globalization.CultureInfo.InvariantCulture);
            DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            TimeSpan timeSinceEpoch = rfc3339Date - unixEpoch;
            long nanoseconds = timeSinceEpoch.Ticks; // Convert ticks to nanoseconds
            return nanoseconds;
        }
        public static string TicksToRfc3339(Int64 chron)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(chron);

            // Format as RFC3339 date string
            string rfc3339Date = dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");

            return rfc3339Date;

        }
        public static DateTime TicksToDateTime(Int64 chron)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(chron);

            return dateTime;

        }
        public static double DateTimeToTicks(DateTimeOffset dt)
        {

            DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            TimeSpan timeSinceEpoch = dt - unixEpoch;
            long ticks = timeSinceEpoch.Ticks; // Convert ticks to nanoseconds
            return ticks;
        }
        public static DateTime Rfc3339ToDateTimeOffset(string rfc3339)
        {
            DateTime rfc3339Date = DateTimeOffset.Parse(rfc3339, null, System.Globalization.DateTimeStyles.RoundtripKind).UtcDateTime;

            return rfc3339Date;
        }
        public static string DtToRfc3339(DateTimeOffset dt)
        {   
            return dt.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffffK");
        }
        public static DateTimeOffset DateFloor(DateTimeOffset dt,TimeSpan dur)
        {
            if (dur.Days != 0)
            {
                DateTime floor = new DateTime(dt.Year, dt.Month, 1,
                                        0, 0,0, 0);
                return floor;
            }
            else if (dur.Hours!=0)
            {
                DateTime floor = new DateTime(dt.Year, dt.Month, dt.Day,
                                       0,0,
                                       0, 0);
                return floor;
            }
            else if (dur.Minutes != 0)
            {
                DateTime floor = new DateTime(dt.Year, dt.Month, dt.Day,
                                       dt.Hour, 0,
                                      0, 0);
                return floor;
            }
            else if (dur.Seconds!=0)
            {
                DateTime floor = new DateTime(dt.Year, dt.Month, dt.Day,
                                       dt.Hour, dt.Minute,
                                       0,0);
                return floor;
            }
            else if (dur.Milliseconds != 0)
            {
                DateTimeOffset floor = new DateTime(dt.Year, dt.Month, dt.Day,
                                      dt.Hour, dt.Minute,
                                      dt.Second,0,DateTimeKind.Utc);
                return floor;
            }
            else
            {
                return dt;
            }
        }
       
        public static void PrintObs(Observation obs, string desc)
        {
            Console.WriteLine("{0,20} | {1,6} | {2,15} | {3,15:F3} | {4,15} | {5,15:F3} | {6,15}", desc, obs.Idx, DtToRfc3339( obs.Chron), obs.Meas, obs.Dchron/10000000, obs.Dmeas, obs.Status);
        }

        public static void PrintObs(Observation obs)
        {
            Console.WriteLine("{0,10} | {1,15} | {2,15:F3} | {3,15} | {4,15:F3} | {5,15} | {6,5}", obs.Idx, DtToRfc3339(obs.Chron), obs.Meas, obs.Dchron/10000000, obs.Dmeas, obs.Status,obs.IntStatus);
        }
        public static TimeSeries Merge(TimeSeries ts1, TimeSeries ts2){
            TimeSeries tsm = new TimeSeries();
            foreach (Observation obs in ts1.Observations){
                tsm.Observations.Add(obs);
            }
            foreach (Observation obs  in ts2.Observations)
            {
                tsm.Observations.Add(obs);
            }
            tsm.SortObservationsChron();
            return tsm;
        }
        public static TimeSeries Merge(List<TimeSeries> tslist){
            TimeSeries mergedtslist = new TimeSeries();
            foreach (TimeSeries ts in tslist){
                foreach (Observation obs in ts.Observations){
                    mergedtslist.Observations.Add(obs);
                }
            }
            mergedtslist.SortObservationsChron();
            return mergedtslist;
        }
        public static TimeSeries Simulate(DateTime start, int sample, double mean, double stdDev, TimeSpan gap)
        {
            TimeSeries tsim=new TimeSeries();
            for (int i = 0; i < sample; i++)
            {
                var normalgap = Normal.WithMeanStdDev(0, 10);
                int randomgap = (int)Math.Floor(normalgap.Sample());
                TimeSpan duration = gap*i+new TimeSpan(0,0,0,randomgap,0,0);
                DateTimeOffset dt = start + duration;
                var normalmeas = Normal.WithMeanStdDev(mean, stdDev);
                double randomNumber = normalmeas.Sample();
                Observation obs3 = new Observation(dt, randomNumber);
                tsim.Observations.Add(obs3);
            }
            tsim.Name = "Simulation";
                    tsim.Type = "Original";
                
                    tsim.Observations[0].Meas = double.NaN;
                    tsim.Observations[tsim.Observations.Count - 1].Meas = double.NaN;
                    tsim.Observations[tsim.Observations.Count/2].Meas=double.NaN;
                    tsim.Observations[tsim.Observations.Count/2+1].Meas=double.NaN;
                    tsim.Observations[tsim.Observations.Count/2+2].Meas=double.NaN;
                    tsim.Observations[tsim.Observations.Count/2+3].Meas=double.NaN;
            return tsim;
        }
        
    }
}

