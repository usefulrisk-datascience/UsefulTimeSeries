using System;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace UsefulTimeSeries
{
    public class SummaryStats
    {
        public int Len { get; set; }
        public int LenValid { get; set; }
        public int NbreOfnan { get; set; }
        public Observation FirstObs { get; set; }
        public Observation FirstValidObs { get; set; }
        public Observation LastObs { get; set; }
        public Observation LastValidObs { get; set; }
        public Observation MaxObs { get; set; }
        public Observation MinObs { get; set; }
        public Observation MaxDmeas { get; set; }
        public Observation MaxDchron { get; set; }
        public Observation MinDmeas { get; set; }
        public Observation MinDchron { get; set; }
        public long Chmean { get; set; }
        public long Chstd { get; set; }
        [JsonConverter(typeof(DoubleNaNConverter))]
        public double Msmean { get; set; }
        [JsonConverter(typeof(DoubleNaNConverter))]
        public double Msstd { get; set; }
        public Observation Msmed { get; set; }
        public Observation Dmeasmed { get; set; }
        [JsonConverter(typeof(DoubleNaNConverter))]
        public double Dmeasmean { get; set; }
        [JsonConverter(typeof(DoubleNaNConverter))]
        public double Dmeasstd { get; set; }
        public Observation Chmed { get; set; }
        public Observation Dchmed { get; set; }
        public double Sum {get; set;}

        public SummaryStats()
        {
            FirstObs = new Observation();
            FirstValidObs = new Observation();
            LastObs = new Observation();
            LastValidObs = new Observation();
            MaxObs = new Observation();
            MinObs = new Observation();
            MaxDchron = new Observation();
            MinDchron = new Observation();
            MinDmeas = new Observation();
            MaxDmeas = new Observation();
            Chmed=new Observation();
            Msmed=new Observation();
            Dmeasmed=new Observation();
            Dchmed=new Observation();
            Len = 0;
            LenValid = 0;
        }
        public void Print()

        {
            Console.WriteLine("");
            Console.WriteLine("----------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("{0,20} | {1,15} |","Length",Len);
            Console.WriteLine("{0,20} | {1,15} |", "Nbre of Valid Obs", LenValid);
            Console.WriteLine("{0,20} | {1,15:F3} |", "Mean", Msmean);
            Console.WriteLine(" ");
            Console.WriteLine("{0,20} | {1,6} | {2,15} | {3,15} | {4,15} | {5,15} | {6,15}","Stat","Idx", "                            Chron", "Meas", "Dchron (sec.)", "Dmeas", "Status");
            Console.WriteLine("{0,20} | {1,6} | {2,15} | {3,15} | {4,15} | {5,15} | {6,15}","----","---", "                            -----", "----", "-------------", "-----", "------");
            UsefulDatesTimes.PrintObs(FirstObs, "First Obs");
            UsefulDatesTimes.PrintObs(LastObs, "Last Obs");
            UsefulDatesTimes.PrintObs(FirstValidObs, "FirstValidObs");
            UsefulDatesTimes.PrintObs(LastValidObs, "LastValidObs");
            UsefulDatesTimes.PrintObs(MaxObs,"Maximum");
            UsefulDatesTimes.PrintObs(MinObs, "Minimum");
            UsefulDatesTimes.PrintObs(MaxDmeas, "Maximum Diff");
            UsefulDatesTimes.PrintObs(MinDmeas, "Minimum Diff");
            UsefulDatesTimes.PrintObs(MaxDchron, "Max Chron Diff");
            UsefulDatesTimes.PrintObs(MinDchron, "Min Chron Diff");
            UsefulDatesTimes.PrintObs(Chmed,"Median Chron");
            UsefulDatesTimes.PrintObs(Msmed,"Median Observation");


            Console.WriteLine("----------------------------------------------------------------------------------------------------------------------");

        }


    }
    
}

