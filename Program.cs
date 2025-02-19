namespace DbBackupWindowsService
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = Host.CreateApplicationBuilder(args);


            builder.Services.AddLogging(log =>
            {
                log.AddConfiguration(builder.Configuration.GetSection("Logging"));
                log.AddFile(builder.Configuration.GetSection("Logging:File:Path").Value);
            });

            builder.Services.AddHostedService<Worker>();



            var host = builder.Build();
            host.Run();
        }
    }
}