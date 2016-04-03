using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;

using Leda.Core;
using Leda.FacebookAdapter;

namespace Bopscotch
{
	[Register ("AppDelegate")]
	public class Program : AppDelegate
	{
		protected override GameBase GetInstance()
		{
            Game1 gameInstance = new Game1();

            Game1.FacebookAdapter = new Leda.FacebookAdapter.IOSFacebookAdapter()
            {
                ApplicationId = "251583331847146",
                GameViewController = gameInstance.Services.GetService(typeof(UIViewController)) as UIViewController
            };

            return gameInstance;
		}

		// This is the main entry point of the application.
		static void Main (string[] args)
		{
			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			UIApplication.Main (args, null, "AppDelegate");
		}
	}
}
