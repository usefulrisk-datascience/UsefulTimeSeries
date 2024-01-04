using System;
using System.Text.Json.Serialization;

namespace UsefulTimeSeries
{
    public class Observation
    {
        public long Idx { get; set; }
        public double Meas{ get; set; }
        public DateTimeOffset Chron { get;set; } //Le champ est implicitement en UTC. Tous les calculs seront fais en UTC
        public bool IsInvalid { get; set; }
        public string Status { get; set; }
        public double Dchron { get; set; }
        public double Dmeas { get; set; }
        public int IntStatus {get;set;}
        public Observation()
        {
            Idx = 0;
            Meas = double.NaN;
            Chron = DateTimeOffset.MinValue;
            IsInvalid = false;
            Status = "";

        }
        public Observation(string rfc3339DateString, double Measure)
        {
            Chron = UsefulDatesTimes.Rfc3339ToDateTimeOffset(rfc3339DateString);
            Meas = Measure;
            IsInvalid = false;
            Status = "";
        }
        public Observation(DateTimeOffset dt, double Measure)
        {
            Chron = dt.UtcDateTime;
            Meas = Measure;
            IsInvalid = false;
            Status = "";
        }
        public Observation(DateTimeOffset dt, double Measure, string status)
        {
            Chron = dt.UtcDateTime;
            Meas = Measure;
            IsInvalid = false;
            Status = status;
        }
        public Observation(DateTimeOffset dt, double Measure, int intstatus)
        {
            Chron = dt.UtcDateTime;
            Meas = Measure;
            IsInvalid = false;
            IntStatus = intstatus;
        }
        public void Print()
        {
            Console.Write(UsefulDatesTimes.DtToRfc3339(Chron) + " "+ Meas + " "+ Dchron+ " "+ Dmeas + " "+Status+" "+IntStatus+"\n");
        }
       
    }
}

