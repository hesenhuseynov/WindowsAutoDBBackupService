using Microsoft.Data.SqlClient;

namespace DbBackupWindowsService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;


        private readonly IConfiguration _configuration;


        public Worker(ILogger<Worker> logger, IConfiguration cofiguration)
        {
            _logger = logger;
            _configuration = cofiguration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Waiting for 20 seconds before backup...");

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    _logger.LogInformation("Backup process starting...");

                    await PerformDatabaseBackup();
                    _logger.LogInformation("Backup Process succeed");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred during Backup Process: {ex.Message}");
                }

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
            }
        }

        private async Task PerformDatabaseBackup()
        {
            string connectionString = _configuration.GetConnectionString("DbConnection");
            string backupFileName = $"C:\\Backup\\FinanceDb_{DateTime.Now:yyyyMMdd_HHmmss}.bak";

            string backupQuery = $@"
            BACKUP DATABASE [FinanceDb]
            TO DISK = '{backupFileName}'
            WITH INIT;
         ";
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    _logger.LogInformation("Opening connection to the database...");
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(backupQuery, connection))
                    {
                        _logger.LogInformation("Executing backup query...");
                        await command.ExecuteNonQueryAsync();
                    }

                    _logger.LogInformation("Backup completed succcessfully.");
                }
            }

            catch (IOException ex)
            {
                _logger.LogError($"I/O Error occurred while  the file system: {ex.Message}");
            }

            catch (Exception ex)
            {
                _logger.LogError($"Error occurrred during database backup: {ex.Message}");
                throw;
            }
        }
    }
}
