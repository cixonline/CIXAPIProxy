using System;
using System.Drawing;
using System.Windows.Forms;

namespace CIXAPIProxy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : IDisposable
    {
        readonly NotifyIcon _notifyIcon = new NotifyIcon();
        private bool _isDisposed;
        private readonly CoSyServer _server;

        public MainWindow()
        {
            InitializeComponent();

            ContextMenu contextMenu = new ContextMenu();

            contextMenu.MenuItems.Add(0, new MenuItem(Properties.Resources.About, OnAboutMenu));
            contextMenu.MenuItems.Add(1, new MenuItem(Properties.Resources.Exit, OnExitMenu));

            _notifyIcon.Visible = true;
            _notifyIcon.Icon = new Icon(GetType(), "CIXTelnetD.ico");
            _notifyIcon.Text = Properties.Resources.CIXAPIProxyTitle;
            _notifyIcon.ContextMenu = contextMenu;

            CIXAPI.CIXOAuth.ConsumerKey = CIXOAuthKeys.ConsumerKey;
            CIXAPI.CIXOAuth.ConsumerSecret = CIXOAuthKeys.ConsumerSecret;

            CoSyServer.CIXAPIServer = Properties.Settings.Default.CIXAPIServer;
            CoSyServer.CIXForumsServer = Properties.Settings.Default.CIXForums;

            if (!IsAuthenticated())
            {
                CixLogin loginWindow = new CixLogin();
                loginWindow.ShowDialog();
            }

            CIXAPI.CIXOAuth.OAuthToken = Properties.Settings.Default.oauthToken;
            CIXAPI.CIXOAuth.OAuthTokenSecret = Properties.Settings.Default.oauthTokenSecret;

            _server = new CoSyServer(23);
            _server.Start();
        }

        /// <summary>
        /// Has this app been authenticated? Do we have non-empty cached tokens.
        /// </summary>
        /// <returns>True if we're authenticated, false otherwise</returns>
        private static bool IsAuthenticated()
        {
            string oauthToken = Properties.Settings.Default.oauthToken;
            string oauthTokenSecret = Properties.Settings.Default.oauthTokenSecret;
            return !string.IsNullOrWhiteSpace(oauthToken) && !string.IsNullOrWhiteSpace(oauthTokenSecret);
        }

        /// <summary>
        /// Displays the About window.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void OnAboutMenu(Object sender, EventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }

        /// <summary>
        /// Shuts down the notification application.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void OnExitMenu(Object sender, EventArgs e)
        {
            _notifyIcon.Visible = false;
            _server.Stop();
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// Internal method that controls disposing the notifyicon object
        /// when the main class is disposed.
        /// </summary>
        /// <param name="disposing">Are we disposing or not?</param>
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _notifyIcon.Dispose();
                }
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Calls Dispose to remove any lingering resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
