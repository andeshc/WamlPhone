using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using Microsoft.WindowsAzure.Management.WebSites;
using Microsoft.WindowsAzure.Management.WebSites.Models;
using Microsoft.WindowsAzure.Management.Storage;
using Microsoft.WindowsAzure.Management.Storage.Models;
using Microsoft.WindowsAzure.Management.Sql;
using Microsoft.WindowsAzure.Management.Sql.Models;
using Microsoft.WindowsAzure;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Management.Compute.Models;

namespace WamlPhone
{
    public partial class AssetListView : PhoneApplicationPage
    {
        public AssetListView()
        {
            InitializeComponent();
        }

        public AssetListViewModel ViewModel
        {
            get { return (AssetListViewModel)this.DataContext; }
            set { this.DataContext = value; }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.ViewModel = new AssetListViewModel();

            await GetWebSites();
            await GetSqlDatabases();
            await GetStorageAccounts();
            await GetHostedServices();
        }

        private TokenCloudCredentials GetCredentials()
        {
            return new TokenCloudCredentials(App.SubscriptionId, App.AccessToken);
        }

        private async Task GetWebSites()
        {
            using (var webSiteClient = 
                CloudContext.Clients.CreateWebSiteManagementClient(GetCredentials()))
            {
                var webSpaceListResult = await webSiteClient.WebSpaces.ListAsync();
                foreach (var webSpace in webSpaceListResult.WebSpaces)
                {
                    var webSitesListResult = 
                        await webSiteClient.WebSpaces.ListWebSitesAsync(webSpace.Name, 
                            new WebSiteListParameters { });

                    foreach (var webSite in webSitesListResult.WebSites)
                    {
                        this.ViewModel.WebSites.Add(webSite);
                    }
                }
            }
        }

        private async Task GetSqlDatabases()
        {
            using (var sqlManagementClient = CloudContext.Clients.CreateSqlManagementClient(GetCredentials()))
            {
                var serverListResult = await sqlManagementClient.Servers.ListAsync();
                foreach (var server in serverListResult.Servers)
                {
                    var databaseListResult = await sqlManagementClient.Databases.ListAsync(server.Name);
                    foreach (var db in databaseListResult.Databases)
                    {
                        this.ViewModel.SqlDatabases.Add(db);
                    }
                }
            }
        }

        private async Task GetStorageAccounts()
        {
            using (var storageManagementClient = CloudContext.Clients.CreateStorageManagementClient(GetCredentials()))
            {
                var storageAccountListResult = await storageManagementClient.StorageAccounts.ListAsync();
                foreach (var storageAccount in storageAccountListResult.StorageServices)
                {
                    this.ViewModel.StorageAccounts.Add(storageAccount);
                }
            }
        }

        private async Task GetHostedServices()
        {
            using (var computeManagementClient = CloudContext.Clients.CreateComputeManagementClient(GetCredentials()))
            {
                var cloudServiceListResult = await computeManagementClient.HostedServices.ListAsync();
                foreach (var hostedService in cloudServiceListResult.HostedServices)
                {
                    this.ViewModel.HostedServices.Add(hostedService);
                }
            }
        }
    }

    public class AssetListViewModel
    {
        public AssetListViewModel()
        {
            this.WebSites = 
                new ObservableCollection<WebSite>();

            this.StorageAccounts = 
                new ObservableCollection<StorageServiceListResponse.StorageService>();

            this.SqlDatabases = 
                new ObservableCollection<DatabaseListResponse.Database>();

            this.HostedServices = 
                new ObservableCollection<HostedServiceListResponse.HostedService>();
        }

        public ObservableCollection<WebSite> 
            WebSites { get; set; }

        public ObservableCollection<StorageServiceListResponse.StorageService> 
            StorageAccounts { get; set; }

        public ObservableCollection<DatabaseListResponse.Database> 
            SqlDatabases { get; set; }

        public ObservableCollection<HostedServiceListResponse.HostedService> 
            HostedServices { get; set; }
    }

}