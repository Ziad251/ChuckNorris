using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


var factory = new ChuckContextFactory();
using var context = factory.CreateDbContext();

await Get();



#region Load GET request and Add to Localdb

async Task Get()
{
    HttpClient client = new()
    {
        BaseAddress = new Uri("https://api.chucknorris.io")
    };
    ChuckJokes chuckJokes = new();

    try
    {
         chuckJokes = await client.GetFromJsonAsync<ChuckJokes>("jokes/random");
    }
    catch (HttpRequestException e)
    {
        Console.WriteLine($"Exception {e} caught!");
    }

    Chuck chucks = new();
    chucks.Joke = chuckJokes.Value ;
    chucks.ChuckNorrisId = chuckJokes.Id;
    chucks.URL = chuckJokes.Url;

    await context.AddAsync(chucks);
    
    Console.WriteLine(chucks.Joke.ToString());

    await context.SaveChangesAsync();
}
#endregion


#region JSON Model

class ChuckJokes
{
    public string Id { get; set; }
    public string Url { get; set; }
    public string Value { get; set; }
}

#endregion

#region Database model
class Chuck
{
    public int Id { get; set; }

    //id from chucknorris.io
    [MaxLength(40)]
    public string ChuckNorrisId { get; set; }

    //url from chucknorris.io
    [MaxLength(1024)]
    public string URL { get; set; }

    //value from chucknorris.io
    public string Joke { get; set; }

}
#endregion

#region Context

class ChuckContext : DbContext
{
    public DbSet<Chuck> ChuckNorris { get; set; }

    public ChuckContext(DbContextOptions<ChuckContext> options)
        : base(options)
    { }

}

class ChuckContextFactory : IDesignTimeDbContextFactory<ChuckContext>
{
    public ChuckContext CreateDbContext(string[] args = null)
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        var optionsBuilder = new DbContextOptionsBuilder<ChuckContext>();
        optionsBuilder
            // Uncomment the following line if you want to print generated
            // SQL statements on the console.
            //.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
            .UseSqlServer(configuration["ConnectionStrings:DefaultConnection"]);

        return new ChuckContext(optionsBuilder.Options);
    }
}
#endregion

