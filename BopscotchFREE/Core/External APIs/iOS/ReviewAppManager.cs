using System;

using Foundation;
using UIKit;
using StoreKit;

namespace Leda.Core.External_APIS.iOS
{
	public class ReviewAppManager
	{
		private SKStoreProductViewController _storeProductViewController;
		private UIViewController _rootViewController;

		public bool Successful { get; private set; }
		public string ErrorMessage { get; private set;}

		public Action CompletionCallback
		{
			set {
				_storeProductViewController.Finished += (sender, err) => {
					_rootViewController.DismissViewController(true, value);
				};
			}
		}

		public ReviewAppManager()
		{
			_storeProductViewController = new SKStoreProductViewController();
		}

		public void InitiateReviewProcess(int appID)
		{
			Successful = false;
			ErrorMessage = "";

			StoreProductParameters spp = new StoreProductParameters(appID);

            if (_rootViewController == null)
            {
                _rootViewController = UIApplication.SharedApplication.Windows[0].RootViewController;
            }

			_storeProductViewController.LoadProduct(spp, (ok, err) => {
				if (ok) {
					_rootViewController.PresentViewController(_storeProductViewController,true,() => {
						Successful = true;
					});
				} else if (err != null) { 
					ErrorMessage = err.ToString();
				}
			});
		}
	}
}

