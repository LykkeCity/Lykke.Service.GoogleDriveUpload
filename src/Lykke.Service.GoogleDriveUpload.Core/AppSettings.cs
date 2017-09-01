namespace Lykke.Service.GoogleDriveUpload.Core
{
    public class AppSettings
    {
        public GoogleDriveUploadSettings GoogleDriveUploadService { get; set; }
    }

    public class GoogleDriveUploadSettings
    {
        public DbSettings Db { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }

    public class DbSettings
    {
        public string LogsConnString { get; set; }
    }

    public class SlackNotificationsSettings
    {
        public AzureQueueSettings AzureQueue { get; set; }
    }

    public class AzureQueueSettings
    {
        public string ConnectionString { get; set; }

        public string QueueName { get; set; }
    }
}
