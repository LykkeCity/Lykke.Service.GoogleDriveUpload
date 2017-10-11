using Newtonsoft.Json;

namespace Lykke.Service.GoogleDriveUpload.Core
{
    public class AppSettings
    {
        public GoogleDriveUploadSettings GoogleDriveUploadService { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }

    public class GoogleDriveUploadSettings
    {
        public DbSettings Db { get; set; }
        public GoogleDriveApiKey GoogleDriveApiKey { get; set; }
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

    public class GoogleDriveApiKey
    {
        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("project_id")]
        public string project_id { get; set; }

        [JsonProperty("private_key_id")]
        public string private_key_id { get; set; }

        [JsonProperty("private_key")]
        public string private_key { get; set; }

        [JsonProperty("client_email")]
        public string client_email { get; set; }

        [JsonProperty("client_id")]
        public string client_id { get; set; }

        [JsonProperty("auth_uri")]
        public string auth_uri { get; set; }

        [JsonProperty("token_uri")]
        public string token_uri { get; set; }

        [JsonProperty("auth_provider_x509_cert_url")]
        public string auth_provider_x509_cert_url { get; set; }

        [JsonProperty("client_x509_cert_url")]
        public string client_x509_cert_url { get; set; }
    }
}
