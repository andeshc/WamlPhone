using Microsoft.Phone.Controls;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Subscriptions;
using Microsoft.WindowsAzure.Subscriptions.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Navigation;
using WamlPhone.Resources;

namespace WamlPhone
{
    public partial class SubscriptionSelectionView : PhoneApplicationPage
    {
        SubscriptionViewModel _viewModel;

        public SubscriptionSelectionView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _viewModel = new SubscriptionViewModel();
            _viewModel.GetSubscriptions();

            this.DataContext = _viewModel;
        }

        private void SubscriptionSelected(object sender, 
            System.Windows.Controls.SelectionChangedEventArgs e)
        {
            App.SubscriptionId = ((SubscriptionListOperationResponse.Subscription)
                e.AddedItems[0]).SubscriptionId;

            NavigationService.Navigate(new Uri("/AssetListView.xaml", UriKind.Relative));
        }


    }

    public class SubscriptionViewModel
    {
        public ObservableCollection<SubscriptionListOperationResponse.Subscription> 
            Subscriptions { get; set; }

        public SubscriptionListOperationResponse.Subscription 
            SelectedSubscription { get; set; }

        public SubscriptionViewModel()
        {
            this.Subscriptions = 
                new ObservableCollection<SubscriptionListOperationResponse.Subscription>();
        }

        public async void GetSubscriptions()
        {
            var tokenCredentials = new TokenCloudCredentials(App.AccessToken);
            var subscriptionClient = new SubscriptionClient(tokenCredentials);
            var subscriptionListResult = await subscriptionClient.Subscriptions.ListAsync();

            foreach (var subscription in subscriptionListResult.Subscriptions)
            {
                this.Subscriptions.Add(subscription);
            }
        }
    }


}