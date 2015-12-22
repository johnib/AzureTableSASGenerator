using System;
using System.Data;
using System.Globalization;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Default
{
    public static class SasGenerator
    {
        public static void Main()
        {
            // fill the details here.
            const string sasName     = "";
            const string accountName = "";
            const string accountKey  = "";
            const string tableName   = "";

            string connectionString =
                $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey}";


            var sas = CreateReadOnlySas(connectionString, tableName, sasName);
            Console.WriteLine(sas);
        }

        private static string CreateReadOnlySas(string connectionString, string tableName, string sasName)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);

            if (!table.Exists())
            {
                throw new InvalidConstraintException(string.Format(CultureInfo.InvariantCulture,
                    "Table '{0}' does not exist", tableName));
            }

            sasName = sasName.ToUpper();
            var permissions = table.GetPermissions();
            if (permissions.SharedAccessPolicies.ContainsKey(sasName))
            {
                SharedAccessTablePolicy policy;
                permissions.SharedAccessPolicies.TryGetValue(sasName, out policy);
            }
            else
            {
                var policy = new SharedAccessTablePolicy()
                {
                    SharedAccessExpiryTime = DateTime.UtcNow.AddYears(1),
                    Permissions = SharedAccessTablePermissions.Query
                };
                permissions.SharedAccessPolicies.Add(sasName, policy);
            }
            table.SetPermissions(permissions);

            var sas = table.GetSharedAccessSignature(null, sasName);
            var uri = new Uri(table.Uri, sas).AbsoluteUri;
            return uri;
        }
    }
}
