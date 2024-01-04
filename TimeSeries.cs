using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json.Serialization;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;

namespace UsefulTimeSeries
{
    
    public class TimeSeries
    {
       
        public string Name { get; set; }
        public string Type { get; set; }
        [JsonIgnore]
        public List<Observation> Observations { get; set; }
        //[JsonIgnore]
        public SummaryStats Stats { get; set; }
        public DateTimeOffset[] ChronArr {get;set;} 
        public double[] MeasArr {get;set;}
        public string[] StatusArr {get;set;}
       
        public TimeSeries()
        {
            Observations = new List<Observation>();
            Name = "";
            Type = "";
            Stats = new SummaryStats();
            MeasArr=Array.Empty<double>();
            ChronArr=Array.Empty<DateTimeOffset>();
            StatusArr=Array.Empty<string>();
        }
        public TimeSeries(string name, string type)
        {
            Observations = new List<Observation>();
            Name = name;
            Type = type;
            Stats = new SummaryStats();
            MeasArr=Array.Empty<double>();
            ChronArr=Array.Empty<DateTimeOffset>();
            StatusArr=Array.Empty<string>();

        }
        public void SortObservationsChron()
        {
            this.Observations.Sort((obs1, obs2) => obs1.Chron.CompareTo(obs2.Chron));
        }
        public void SortObservationsReverseChron()
        {
            this.Observations.Sort((obs1, obs2) => obs2.Chron.CompareTo(obs1.Chron));
        }
        public void SortObservationsMeas(){
            this.Observations.Sort((obs1,obs2)=>obs1.Meas.CompareTo(obs2.Meas));
        }
        public void SortObservationsDchron(){
            this.Observations.Sort((obs1,obs2)=>obs1.Dchron.CompareTo(obs2.Dchron));
        }
        public void SortObservationDmeas(){
            this.Observations.Sort((obs1,obs2)=>obs1.Dmeas.CompareTo(obs2.Dmeas));

        }
        public TimeSeries ExtractLast(int lastn){
            TimeSeries tscut= new TimeSeries(Name,Type);
            SortObservationsChron();
            var len=this.Observations.Count;
            for (int i=(len-lastn);i<len;i++){
                tscut.Append(Observations[i]);
            }
            return tscut;
        }
        public void RunStats()
        {
            Stats.Len = Observations.Count;
            if (Stats.Len > 0)
            {
                bool firstValidObsDetected = false; 
                int moit=Stats.Len/2;
                SortObservationsMeas();
                Stats.Msmed=Observations[moit];
                SortObservationsChron();
                Observations[0].Dmeas = double.NaN;
                Observations[0].Dchron = double.NaN;
                Stats.FirstObs = Observations[0];
                Stats.LastObs = Observations[Stats.Len - 1];
                Observations[0].Idx =0;
                Stats.Chmed=Observations[moit];
                Stats.Sum=Observations[0].Meas;
                if (double.IsNaN(Observations[0].Meas) == false && Observations[0].IsInvalid == false){
                    Stats.FirstValidObs = Observations[0];
                    firstValidObsDetected = true;
                    Stats.MaxObs = Stats.FirstValidObs;
                    Stats.MinObs = Stats.FirstValidObs;
                }
                if (Stats.Len > 1)
                {
                    Stats.MaxDmeas = Observations[1];
                    Stats.MinDmeas = Observations[1];
                    Stats.MaxDchron = Observations[1];
                    Stats.MinDchron = Observations[1];
                }
                for (int i = 1; i < Stats.Len; i++)
                {
                    Observations[i].Idx = i;
                    if (i==moit){Stats.Chmed=Observations[i];}
                    if (double.IsNaN(Observations[i].Meas) == false && Observations[i].IsInvalid == false)
                    {
                        Stats.LenValid += 1;
                        if (firstValidObsDetected == false)
                        {
                            Stats.FirstValidObs = Observations[i];
                            firstValidObsDetected = true;
                            Stats.MaxObs = Stats.FirstValidObs;
                            Stats.MinObs = Stats.FirstValidObs;
                        }
                        
                        Observations[i].Dchron = UsefulDatesTimes.DateTimeToTicks(Observations[i].Chron) - UsefulDatesTimes.DateTimeToTicks(Observations[i - 1].Chron);
                        Observations[i].Dmeas = Observations[i].Meas - Observations[i - 1].Meas;
                        
                        if (Observations[i].Meas > Stats.MaxObs.Meas)
                        {
                            Stats.MaxObs = Observations[i];
                        }
                        if (Observations[i].Meas < Stats.MinObs.Meas)
                        {
                            Stats.MinObs = Observations[i];
                        }
                        Stats.Sum += Observations[i].Meas;

                        if (double.IsNaN(Observations[i].Dmeas) == false && Observations[i].IsInvalid == false)
                        {
                           
                            if (Math.Abs(Observations[i].Dmeas) > Math.Abs(Stats.MaxDmeas.Dmeas) || double.IsNaN(Stats.MaxDmeas.Dmeas) ==true)
                            {
                                Stats.MaxDmeas = Observations[i];
                            }
                            if (Math.Abs(Observations[i].Dmeas) < Math.Abs(Stats.MinDmeas.Dmeas) || double.IsNaN(Stats.MinDmeas.Dmeas) == true)
                            {
                                Stats.MinDmeas = Observations[i];
                            }
                            Stats.Dmeasmean += Observations[i].Dmeas;

                        }
                        if (double.IsNaN(Observations[i].Dchron) == false && Observations[i].IsInvalid == false)
                        {

                            if (Math.Abs(Observations[i].Dchron) > Math.Abs(Stats.MaxDchron.Dchron) || double.IsNaN(Stats.MaxDchron.Dchron) == true)
                            {
                                Stats.MaxDchron = Observations[i];
                            }
                            if (Math.Abs(Observations[i].Dchron) < Math.Abs(Stats.MinDchron.Dchron) || double.IsNaN(Stats.MinDchron.Dchron) == true)
                            {
                                Stats.MinDchron = Observations[i];
                            }

                        }

                    }
                    
                }
                int j= Stats.Len - 1;
                while (j >0 && (double.IsNaN(Observations[j].Meas) == true || Observations[j].IsInvalid == true) )
                {
                    j--;
                }
                if (j== 0 && (double.IsNaN(Observations[j].Meas) == true || Observations[j].IsInvalid == true)) {

                    Stats.LastValidObs = new Observation();
                } else
                {
                    Stats.LastValidObs = Observations[j];
                }
                   
                Stats.Msmean = Stats.Sum / Stats.LenValid;
                SortObservationsDchron();
                Stats.Dchmed=Observations[moit];
                SortObservationDmeas();
                Stats.Dmeasmed=Observations[moit];
                SortObservationsChron();
            }

        }
        public TimeSeries Regularize(TimeSpan duration, string whatfunc)
        {
            TimeSeries tsr = new TimeSeries();
            tsr.Name="Simulation";
            tsr.Type="Regularized";
            try{
                DateTimeOffset NextTicker = UsefulDatesTimes.DateFloor(Observations[0].Chron,duration);
                while (NextTicker < Observations[0].Chron)
                {
                    NextTicker = NextTicker + duration;
                }
                int i = 0;
                while (i < Observations.Count)
                {
                    TimeSeries minits = new TimeSeries();
                    while (i < Observations.Count && Observations[i].Chron <= NextTicker)
                    { minits.Observations.Add(Observations[i]);
                        i++;
                    }
                    if (minits.Observations.Count > 0){
                        minits.RunStats();
                        Observation regulobs = new Observation();
                        switch (whatfunc){
                            case "Max":
                                regulobs.Chron = NextTicker;
                                regulobs.Meas=minits.Stats.MaxObs.Meas;
                                break;
                            case "Min":
                                regulobs.Chron = NextTicker;
                                regulobs.Meas = minits.Stats.MinObs.Meas;
                                break;
                            case "LastValidObs":
                                regulobs.Chron = NextTicker;
                                regulobs.Meas = minits.Stats.LastValidObs.Meas;
                                break;
                            case "LastObs":
                                regulobs.Chron = NextTicker;
                                regulobs.Meas = minits.Stats.LastObs.Meas;
                                break;
                            case "Sum":
                                regulobs.Chron = NextTicker;
                                regulobs.Meas = minits.Stats.Sum;
                                break;
                            default:
                                regulobs.Chron = NextTicker;
                                regulobs.Meas = minits.Stats.LastObs.Meas;
                            
                                break;
                        }
                        tsr.Observations.Add(regulobs);

                    }
                    else
                    {
                        Observation regulobs = new Observation(NextTicker, double.NaN);
                        tsr.Observations.Add(regulobs);
                    }       
                    
                    NextTicker = NextTicker + duration;
                }
                return tsr;
            }
            catch (ArgumentOutOfRangeException ex){
                Console.WriteLine($"Exception caught: {ex.Message}");
                return tsr;
            }
           
        }
        public (TimeSeries, TimeSeries) RemoveOutbounds(double min, double max)
        {
            TimeSeries tsclean = new TimeSeries();
            tsclean.Name = Name;
            tsclean.Type = "Cleaned";
            TimeSeries tsrej = new TimeSeries();
            tsrej.Name = Name;
            tsrej.Type = "Rejected";
            foreach (Observation obs in Observations)
            {
                if (obs.Meas < min || obs.Meas > max || double.IsNaN(obs.Meas))
                {
                    tsrej.Observations.Add(obs);
                }
                else
                {
                    tsclean.Observations.Add(obs);
                }
            }

            return (tsclean, tsrej);
        }
        public (TimeSeries, TimeSeries) RemoveInbounds(double min, double max) {
            TimeSeries tsclean = new TimeSeries();
            tsclean.Name = Name;
            tsclean.Type = "Cleaned";
            TimeSeries tsrej = new TimeSeries();
            tsrej.Name = Name;
            tsrej.Type = "Rejected";
            foreach (Observation obs in Observations)
            {
                if (obs.Meas > min && obs.Meas < max || double.IsNaN(obs.Meas))
                {
                    tsrej.Observations.Add(obs);
                }
                else
                {
                    tsclean.Observations.Add(obs);
                }
            }

            return (tsclean, tsrej);
        }
        public (TimeSeries, TimeSeries)RmQuantileOutbounds(int perc){
            double[] obsarr = this.Observations.Select(obs => obs.Meas).Where(obz => double.IsNaN(obz) == false).ToArray();
            double min = obsarr.Percentile(perc);
            double max=obsarr.Percentile(100-perc);
            var tsuple=this.RemoveOutbounds(min,max);
            return(tsuple.Item1,tsuple.Item2);
        }
        public (TimeSeries, TimeSeries)RmZScore(double lvl){
            double[] obsarr = this.Observations.Select(obs => obs.Meas).Where(obz => double.IsNaN(obz) == false).ToArray();
            double mean = obsarr.Mean();
            double std=obsarr.StandardDeviation();
            var tsuple=this.RemoveOutbounds(mean-lvl*std,mean+lvl*std);
            return(tsuple.Item1,tsuple.Item2);
        }
        public (TimeSeries, TimeSeries) SuspectedZeros()
        {
            TimeSeries tsclean = new TimeSeries();
            tsclean.Name = Name;
            tsclean.Type = "Cleaned";
            TimeSeries tsrej = new TimeSeries();
            tsrej.Name = Name;
            tsrej.Type = "Rejected";
            if (Stats.Len>0){
                for (int i = 1; i < Stats.Len; i++){
                    if (Observations[i].Meas==0 && Observations[i-1].Meas!=0)
                        {
                            tsrej.Observations.Add(Observations[i]);
                        }
                        else
                        {
                            tsclean.Observations.Add(Observations[i]);
                        }
                    }
                    
                }
            return (tsclean, tsrej);
        }
        public (TimeSeries, TimeSeries) NonZeroStatus()
        {
            TimeSeries tsclean = new TimeSeries();
            tsclean.Name = Name;
            tsclean.Type = "Cleaned";
            TimeSeries tsrej = new TimeSeries();
            tsrej.Name = Name;
            tsrej.Type = "Rejected";
            if (Stats.Len>0){
                for (int i = 1; i < Stats.Len; i++){
                    if (Observations[i].IntStatus!=0)
                        {
                            Observations[i].Status="Status "+Observations[i].IntStatus;
                            tsrej.Observations.Add(Observations[i]);
                        }
                        else
                        {
                            tsclean.Observations.Add(Observations[i]);
                        }
                    }
                    
                }
            return (tsclean, tsrej);
        }
        public TimeSeries Warnings(){
            TimeSeries tsw=new TimeSeries();
            if (Stats.Len>0){
                for (int i = 1; i < Stats.Len; i++){
                    double ratd=(Observations[i].Meas/Observations[i-1].Meas)-1;
                    if(ratd>.1){
                        Observation obz=new Observation(Observations[i].Chron,Observations[i].Meas);
                        obz.IsInvalid=false;
                        obz.Status="Warning: Variation from Previous Exceeds 10%";
                        tsw.Observations.Add(obz);
                    }
                }
            }
            tsw.Name="Simulation";
            tsw.Type="Warnings";
            return tsw;
        }
        public void Print()
        {
            Console.WriteLine(Name + " -- " + Type);
            Console.WriteLine("{0,10} | {1,15} | {2,15:F3} | {3,15} | {4,15:F3} | {5,15} | {6,5}", "Idx", "                            Chron", "Meas", "Dchron (sec.)", "Dmeas", "Status","IntStatus");      
            Console.WriteLine("{0,10} | {1,15} | {2,15:F3} | {3,15} | {4,15:F3} | {5,15} | {6,5}", "---", "                            -----", "----", "-------------", "-----", "------", "------");

            foreach (Observation obs in Observations)
            {
                UsefulDatesTimes.PrintObs(obs);
            }
        }
        
        public void PrepareForJsonExport(){
            MeasArr=new double[Stats.Len];
            ChronArr=new DateTimeOffset[Stats.Len];
            StatusArr=new string[Stats.Len];
            for (int i=0;i<Stats.Len;i++){
                this.MeasArr[i]=Observations[i].Meas;
                this.ChronArr[i]=Observations[i].Chron;
                this.StatusArr[i]=Observations[i].Status;
            } 
        }
        /*
        public void PrepareForJsonExport(int size){
            MeasArr=new double[size];
            ChronArr=new DateTimeOffset[size];
            StatusArr=new string[size];
            SortObservationsChron();
            for (int i=(Stats.Len-size-1);i<Stats.Len-1;i++){
                this.MeasArr[i-Stats.Len+size+1]=Observations[i].Meas;
                this.ChronArr[i-Stats.Len+size+1]=Observations[i].Chron;
                this.StatusArr[i-Stats.Len+size+1]=Observations[i].Status;
            } 
        }
        */
        public void Append(DateTimeOffset dt,double ms){
            Observation obs=new Observation(dt,ms);
            this.Observations.Add(obs);
        }
        public void Append(DateTimeOffset dt,double ms, string status){
            Observation obs=new Observation(dt,ms,status);
            this.Observations.Add(obs);
        }
        public void Append(DateTimeOffset dt,double ms, int intstatus){
            Observation obs=new Observation(dt,ms,intstatus);
            this.Observations.Add(obs);
        }
        public void Append(Observation obs){
            this.Observations.Add(obs);
        }
    }
}

