using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using CIXAPI;

namespace CIXAPIProxy
{
    /// <summary>
    /// Interaction logic for CIXLogin.xaml
    /// </summary>
    public partial class CixLogin
    {
        string _oauthToken = "";
        string _oauthTokenSecret = "";

        private const string oauthTokenString = "oauth_token=";
        private const string oauthTokenSecretString = "oauth_token_secret=";

        public CixLogin()
        {
            InitializeComponent();
            CIXOAuth.APIServer = CoSyServer.CIXAPIServer;
        }

        /// <summary>
        /// Begin the desktop OAuth application authentication procedure. This is where we need to bring up a web
        /// browser control to allow the user to authenticate this application for access to their CIX data.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            WebRequest wrGeturl = WebRequest.Create(CIXOAuth.Uri("cix.svc/getrequesttoken", "GET", string.Empty, string.Empty));

            using (Stream objStream = wrGeturl.GetResponse().GetResponseStream())
            {
                if (objStream != null)
                {
                    StreamReader objReader = new StreamReader(objStream);
                    string sLine = objReader.ReadLine();

                    if (sLine != null)
                    {
                        string[] items = sLine.Split(new[] { '&' });
                        foreach (string t in items)
                        {
                            if (t.StartsWith(oauthTokenString))
                            {
                                _oauthToken = t.Substring(oauthTokenString.Length);
                            }
                            if (t.StartsWith(oauthTokenSecretString))
                            {
                                _oauthTokenSecret = t.Substring(oauthTokenSecretString.Length);
                            }
                        }
                    }
                }
            }

            // At this point, _oauthToken should be meaningful. If it isn't then the web browser is going to show
            // an error page. Not really a huge number of useful things we could do if this happens.
            string webUrl = CoSyServer.CIXForumsServer + "/secure/authapp.aspx?oauth_token=" + _oauthToken + "&oauth_callback=notifyapp://home";
            webForm.Source = new Uri(webUrl);
        }

        /// <summary>
        /// Trap navigation to the application domain. This is a hack for desktop OAuth to catch the post
        /// authentication navigation and use that to grab the access tokens and close the browser window
        /// we opened to allow the user to authenticate the app.
        /// 
        /// Any other navigation is passed through as normal.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Navigate event arguments</param>
        private void OnNavigate(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.Uri.AbsoluteUri == "notifyapp://home/")
            {
                WebRequest wrGeturl = WebRequest.Create(CIXOAuth.Uri("cix.svc/getaccesstoken", "GET", _oauthToken, _oauthTokenSecret));
                wrGeturl.Method = "GET";

                Stream objStream = wrGeturl.GetResponse().GetResponseStream();
                if (objStream != null)
                {
                    StreamReader objReader = new StreamReader(objStream);
                    string sLine = objReader.ReadLine();

                    if (sLine != null)
                    {
                        string[] items = sLine.Split(new[] {'&'});
                        foreach (string t in items)
                        {
                            if (t.StartsWith(oauthTokenString))
                            {
                                Properties.Settings.Default.oauthToken = t.Substring(oauthTokenString.Length);
                            }
                            if (t.StartsWith(oauthTokenSecretString))
                            {
                                Properties.Settings.Default.oauthTokenSecret = t.Substring(oauthTokenSecretString.Length);
                            }
                        }
                    }
                    objStream.Close();
                }

                Properties.Settings.Default.Save();
                Close();
            }
        }

        /// <summary>
        /// Trap post-navigation but pre-execution event to ensure that script errors on the
        /// web page are disabled.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Navigation event arguments</param>
        private void OnNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            HideScriptErrors(webForm, true);
        }

        /// <summary>
        /// Modify the web browser control to hide script errors that might pop up on the
        /// remote site. By default script errors are always triggered.
        /// </summary>
        /// <param name="wb">Web browser control</param>
        /// <param name="hide">Specifies whether or not script errors are to be hidden</param>
        private void HideScriptErrors(WebBrowser wb, bool hide)
        {
            var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser != null)
            {
                var objComWebBrowser = fiComWebBrowser.GetValue(wb);
                if (objComWebBrowser == null)
                {
                    wb.Loaded += (o, s) => HideScriptErrors(wb, hide); // In case we are too early
                    return;
                }
                objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
            }
        }
    }
}
