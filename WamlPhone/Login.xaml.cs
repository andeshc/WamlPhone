using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Navigation;
using WamlPhone.Resources;

namespace WamlPhone
{
    public partial class Login : PhoneApplicationPage
    {
        static string _code = string.Empty;  // this will be populated during auth

        public Login()
        {
            InitializeComponent();

            _webBrowser.IsScriptEnabled = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string authURL = string.Format(
                "https://login.windows.net/{0}/oauth2/authorize?response_type=" +
                "code&resource={1}&client_id={2}&redirect_uri={3}",
                    AppResources.AAD_TENANT_ID,
                    AppResources.AZURE_SM_ENDPOINT,
                    AppResources.AAD_CLIENT_ID,
                    AppResources.AAD_REDIRECT_URL);

            _webBrowser.Navigate(new Uri(authURL));
        }

        private void WebBrowser_Navigating(object sender, NavigatingEventArgs e)
        {
            string returnURL = e.Uri.ToString();

            if (returnURL.StartsWith(AppResources.AAD_REDIRECT_URL))
            {
                _code = e.Uri.Query.Remove(0, 6);
                e.Cancel = true;
                _webBrowser.Visibility = System.Windows.Visibility.Collapsed;

                GetToken();
            }
        }

        private void GetToken()
        {
            HttpWebRequest hwr = WebRequest.Create(
                string.Format("https://login.windows.net/{0}/oauth2/token",
                    AppResources.AAD_TENANT_ID)) as HttpWebRequest;

            hwr.Method = "POST";
            hwr.ContentType = "application/x-www-form-urlencoded";
            hwr.BeginGetRequestStream(new AsyncCallback(SendTokenEndpointRequest), hwr);
        }

        private void SendTokenEndpointRequest(IAsyncResult rez)
        {
            HttpWebRequest hwr = rez.AsyncState as HttpWebRequest;

            byte[] bodyBits = Encoding.UTF8.GetBytes(
                string.Format(
                    "grant_type=authorization_code&code={0}&client_id={1}&redirect_uri={2}",
                    _code,
                    AppResources.AAD_CLIENT_ID,
                    HttpUtility.UrlEncode(AppResources.AAD_REDIRECT_URL)));

            Stream st = hwr.EndGetRequestStream(rez);
            st.Write(bodyBits, 0, bodyBits.Length);
            st.Close();
            hwr.BeginGetResponse(new AsyncCallback(RetrieveTokenEndpointResponse), hwr);
        }

        private void RetrieveTokenEndpointResponse(IAsyncResult rez)
        {
            HttpWebRequest hwr = rez.AsyncState as HttpWebRequest;
            HttpWebResponse resp = hwr.EndGetResponse(rez) as HttpWebResponse;
            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string responseString = sr.ReadToEnd();

            JObject jo = JsonConvert.DeserializeObject(responseString) as JObject;

            App.AccessToken = (string)jo["access_token"];

            Dispatcher.InvokeAsync(() =>
            {
                NavigationService.Navigate(new Uri("/SubscriptionSelectionView.xaml", 
                    UriKind.Relative));
            });
        }
    }


}