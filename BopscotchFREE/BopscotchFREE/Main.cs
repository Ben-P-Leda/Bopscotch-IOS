using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;

using Leda.Core;

namespace Bopscotch
{
	[Register ("AppDelegate")]
	public class Program : AppDelegate
	{
		protected override GameBase GetInstance ()
		{
			return new Game1 ();
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
