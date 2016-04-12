using System;
using System.Collections.Generic;
using Facebook;
using Foundation;
using UIKit;
using MonoTouch.Dialog;
using MonoTouch.Dialog.Utilities;

namespace Leda.FacebookAdapter
{
    public class IOSFacebookAdapter : FacebookAdapterBase, IFacebookAdapter
    {
        private FacebookClient _loginClient;
        private UINavigationController _loginController;
        private bool _manualLoginRequired;

        public UIViewController GameViewController { private get; set; }

        public IOSFacebookAdapter()
            : base()
        {
            _loginClient = new FacebookClient();
            _manualLoginRequired = false;
        }

        public void AttemptLogin()
        {
            _loginController = MakeFacebookLoginView();

            GameViewController.PresentViewController(_loginController, true, null);
        }

        private UINavigationController MakeFacebookLoginView()
        {
            var webView = new UIWebView(UIScreen.MainScreen.Bounds)
                {
                    BackgroundColor = UIColor.White,
                    AutoresizingMask = UIViewAutoresizing.All,
                };

            webView.LoadFinished += HandleWebViewLoadComplete;
            webView.LoadError += HandleWebViewLoadError;

            var root = new RootElement("Log into Facebook") {
                new Section() { webView }
            };
            var cancelButton = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            cancelButton.Clicked += HandleCancelButtonPress;

            var dvc = new DialogViewController(root);
            dvc.NavigationItem.RightBarButtonItem = cancelButton;

            var url = new NSUrl(GetFacebookLoginUrl(ApplicationId,"user_about_me,publish_actions"));
            webView.LoadRequest(NSUrlRequest.FromUrl(url));

            return new UINavigationController(dvc);
        }

        private string GetFacebookLoginUrl (string appId, string extendedPermissions)
        {
            var parameters = new Dictionary<string, object>();
            parameters["client_id"] = appId;
            parameters["redirect_uri"] = "https://www.facebook.com/connect/login_success.html";
            parameters["response_type"] = "token";
            parameters["display"] = "touch";

            // add the 'scope' only if we have extendedPermissions.
            if (!string.IsNullOrEmpty (extendedPermissions))
            {
                // A comma-delimited list of permissions
                parameters["scope"] = extendedPermissions;
            }

            return _loginClient.GetLoginUrl(parameters).AbsoluteUri;
        }

        private void HandleCancelButtonPress(object sender, EventArgs e)
        {
            CloseLoginView();
            CompleteAction(ActionResult.LoginCancelled);
        }

        private void CloseLoginView()
        {
            _manualLoginRequired = false;

            _loginController.DismissViewController(true, null);
            _loginController.Dispose();

            GC.Collect();
        }

        private void CompleteAction(ActionResult actionResult)
        {
            if (ActionCallback != null) { ActionCallback(actionResult); }
        }

        private void HandleWebViewLoadComplete(object sender, EventArgs e)
        {
            var webview = sender as UIWebView;
            FacebookOAuthResult oauthResult;
            if (_loginClient.TryParseOAuthCallbackUrl(new Uri(webview.Request.Url.ToString()), out oauthResult))
            {
                CloseLoginView();

                if (oauthResult.IsSuccess) { FinishLogin(oauthResult.AccessToken, _manualLoginRequired); }
                else { CompleteAction(ActionResult.LoginCancelled); }
            }
            else
            {
                _manualLoginRequired = true;
            }
        }

        private void HandleWebViewLoadError(object sender, EventArgs e)
        {
            CloseLoginView();
            CompleteAction(ActionResult.LoginLoadFailed);
        }

        public override void AttemptLogout()
        {
            _manualLoginRequired = true;

            NSUrlCache.SharedCache.RemoveAllCachedResponses();

            NSHttpCookieStorage storage = NSHttpCookieStorage.SharedStorage;
            foreach(var item in storage.Cookies) { storage.DeleteCookie(item); }

            NSUserDefaults.StandardUserDefaults.Synchronize();

            base.AttemptLogout();
        }
    }
}

