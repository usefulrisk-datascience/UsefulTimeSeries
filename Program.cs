using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Reflection.Metadata;
using MathNet.Numerics.Distributions;
using UsefulTimeSeries;
using System.Text.Json.Serialization;
using System.Linq;
using System.Collections.Immutable;
using MathNet.Numerics.Statistics;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Npgsql;
using Microsoft.EntityFrameworkCore;

namespace UsefulTimeSeries;

   class SimpleHTTPServer
{
    static async Task Main(string[] args)
   
    {
        string currentDirectory = AppContext.BaseDirectory;
        Console.WriteLine(currentDirectory);
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/"); // Set your desired base URL and port

        listener.Start();
        Console.WriteLine("Server started. Listening on http://localhost:8080/");

        while (true)
        {
            var context = await listener.GetContextAsync();
            HandleRequest(context);
        }
    }

    static void HandleRequest(HttpListenerContext context)
    {
        string url = context.Request.Url.AbsolutePath;

        if (context.Request.HttpMethod == "POST")
        {
            if (url == "/json-response")
            {
                using (var reader = new StreamReader(context.Request.InputStream))
                {
                    string requestBody = reader.ReadToEnd();
                    //------------------------------------------------------------------------------------------------------------
                    Console.WriteLine("On est parti.");
                    DateTime start = DateTime.Now;
                    TimeSeries ts = new TimeSeries();
                    TimeSpan gap = new TimeSpan(0, 0, 5, 0, 0, 0);
                    var noow = DateTime.Now;
                    // ----------------  CREATE SIMULATED TIMESERIES ----------------------------
                    //var begin = new DateTime(noow.Year, noow.Month, noow.Day, noow.Hour, noow.Minute, noow.Second, 0, 0,DateTimeKind.Local);
                    //TimeSeries tsim1= UsefulDatesTimes.Simulate(begin,50, 100.0, 20.0,gap);
                    //TimeSeries tsim2=UsefulDatesTimes.Simulate(begin,50, 100.0, 20.0,gap);
                    //ts=UsefulDatesTimes.Merge(tsim1,tsim2);
                    // ------------------- PROCESSES ----------------------------------------
                    //IEnumerable<Observation> readOnlyList = ts.Observations.AsReadOnly();
                    //ts.RunStats();
                    //ts.Stats.Print();
                    //ts.Print();
                    //ts.PrepareForJsonExport();
                    //TimeSeries tsw=ts.Warnings();
                    //tsw.Stats.Len=tsw.Observations.Count();
                    //tsw.PrepareForJsonExport();
                    //(TimeSeries tsclean,TimeSeries tsrej)=ts.RmZScore(1);
                    //tsclean.RunStats();
                    //tsclean.PrepareForJsonExport();
                    //tsclean.Print();
                    //tsrej.RunStats();
                    //tsrej.PrepareForJsonExport();
                    //tsrej.Print();
                    //TimeSeries tsreg = tsclean.Regularize(new TimeSpan(0, 1, 0, 0, 0, 0),"LastValid");
                    //tsreg.RunStats();
                    //tsreg.PrepareForJsonExport();
                    //tsreg.Print();
                    Dictionary<string, TimeSeries> TsContainer = new Dictionary<string, TimeSeries>();
                    /*
                    TsContainer.Add("Original", ts);
                    TsContainer.Add("Regularized", tsreg);
                    TsContainer.Add("Cleaned",tsclean);
                    TsContainer.Add("Rejected",tsrej);
                    TsContainer.Add("Warnings",tsw);
                    */
                    TimeSeries tsql = new TimeSeries("Occupancy","Original");
                    using (var sqlcontext = new MyDbContext())
                    {
                        var entities = sqlcontext.Occupancy
                        .Where(e => e.Device == Guid.Parse("3ccd4544-e646-4268-92ff-fb81555729c2"))
                        //.Take(10000)
                        .ToList();

                        foreach (var entity in entities)
                        {
                            //Console.WriteLine($"Time: {entity.Time}, Occ: {entity.Occupancy}");
                            tsql.Append(entity.Time,entity.Occupancy,entity.Status);
                        }
                    }
                    
                    ts=tsql.ExtractLast(10000);
                    //tsql=null;
                    ts.RunStats();
                    //ts.Print();
                    ts.Stats.Print();
                    ts.PrepareForJsonExport();
                    TsContainer.Add("Original",ts);
                    //(TimeSeries tsclean,TimeSeries tsrej)=tsql.SuspectedZeros();
                    (TimeSeries tsclean,TimeSeries tsrej)=ts.NonZeroStatus();
                    tsclean.RunStats();
                    tsclean.PrepareForJsonExport();
                    TsContainer.Add("Cleaned",tsclean);
                    tsrej.RunStats();
                    tsrej.PrepareForJsonExport();
                    TsContainer.Add("Rejected",tsrej);
                    TimeSeries tsreg=tsclean.Regularize(new TimeSpan(1, 0, 0, 0, 0, 0),"Max");
                    tsreg.RunStats();
                    tsreg.PrepareForJsonExport();
                    TsContainer.Add("Regularized",tsreg);
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true, // For pretty-printing the JSON
                        NumberHandling=JsonNumberHandling.AllowNamedFloatingPointLiterals,
                        Converters = {new DoubleNaNConverter()}
                    };
                    string jj = JsonSerializer.Serialize(TsContainer, options);

                    // ------------------- SQL -------------------------
                    // Connection string to the PostgreSQL database
            /*
                    var connectionString = "Host=localhost;Port=13402;Username=postgres;Password=password;Database=OccTool2";

                    // Creating a connection to the database
                    using (var conn = new NpgsqlConnection(connectionString))
                    {
                        try
                        {
                            conn.Open();

                            // SQL query
                            var sql = "SELECT \"Time\", \"Occupancy\" FROM public.\"Occupancy\" LIMIT 100;";

                            // Creating a command to execute the SQL query
                            using (var cmd = new NpgsqlCommand(sql, conn))
                            {
                                // Executing the query and retrieving the results
                                using (var sqlreader = cmd.ExecuteReader())
                                {
                                    while (sqlreader.Read())
                                    {
                                        // Accessing data from the query result
                                        DateTimeOffset timestampOffset = reader.GetDateTimeOffset(0);
                                        var column2Value = sqlreader.GetValue(1); // Replace 1 with the index of the column
                                        //var column3Value = sqlreader.GetValue(2); // Replace 1 with the index of the column

                                        // Process retrieved data here
                                        Console.WriteLine($"Time: {UsefulDatesTimes.DtToRfc3339(column1Value)} Occ: {column2Value}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Handle exceptions
                            Console.WriteLine($"An error occurred: {ex.Message}");
                        }
                    }
                    */
                    //Console.WriteLine(jj);
                    //jsonString = JsonSerializer.Serialize(TsContainer);
                    //Console.WriteLine(jsonString);
                    DateTime finish = DateTime.Now;
                    TimeSpan duree = finish - start;
                    Console.WriteLine("Calculation Time: " + duree);

                    // ------------------------------------------------------------------
                    // var responseObject = new { Message = "Received your POST request!", Data = requestBody };
                    // string jsonResponse = System.Text.Json.JsonSerializer.Serialize(ts);

                    byte[] responseBytes = Encoding.UTF8.GetBytes(jj);

                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
                    context.Response.ContentLength64 = responseBytes.Length;

                    context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
                }
            }
            else
            {
                context.Response.StatusCode = 404;
            }
        }
        else
        {
            context.Response.StatusCode = 405; // Method Not Allowed
        }

        context.Response.Close();
    }
}

public class DbEntity{
    public DateTimeOffset Time {get;set;}
    public double Occupancy {get;set;}
    public Guid Device {get;set;}
    public int Status {get;set;}

}

public class MyDbContext : DbContext
{
    public DbSet<DbEntity> Occupancy { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Configure your database connection string here
        optionsBuilder.UseNpgsql("Host=localhost;Port=13402;Username=postgres;Password=password;Database=OccTool2");
    }
    /*protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbEntity>().HasNoKey(); // Define DbEntity as a keyless entity
        modelBuilder.Entity<DbEntity>()
            .Property(e => e.Device)
            .HasConversion<Guid>(); // Convert 'uuid' field to Guid property
        modelBuilder.Entity<DbEntity>()
            .Property(e=>e.IntStatus)
            .HasColumnName("Status");
    }*/
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbEntity>(entity =>
        {
            entity.HasNoKey(); // Define DbEntity as a keyless entity
            entity.Property(e => e.Device)
                .HasConversion<Guid>(); // Convert 'uuid' field to Guid property
        });
    }

}

