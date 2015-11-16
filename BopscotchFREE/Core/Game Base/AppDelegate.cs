using System;
using System.Drawing;

using UIKit;
using Foundation;
using AVFoundation;

using Microsoft.Xna.Framework;

namespace Leda.Core
{
	public abstract class AppDelegate : UIApplicationDelegate
	{
		protected GameBase _monogameGame;

		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			NSError audioError;
			AVAudioSession.SharedInstance().SetCategory (new NSString("AVAudioSessionCategoryAmbient"), out audioError);

			app.SetStatusBarHidden (true, UIStatusBarAnimation.Slide);

			_monogameGame = GetInstance ();
			_monogameGame.Run ();

			return true;
		}

//		public override void OnActivated (UIApplication application)
//		{
//			_monogameGame.HandleGameActivatedEvent ();
//		}
//
//		public override void WillEnterForeground (UIApplication application)
//		{
//			_monogameGame.HandleGameResumedEvent ();
//		}
//
//		public override void DidEnterBackground (UIApplication application)
//		{
//			_monogameGame.HandleGameBackgroundEvent ();
//		}
//
//		public override void OnResignActivation (UIApplication application)
//		{
//			_monogameGame.HandleGameResignedEvent ();
//		}

		protected abstract GameBase GetInstance();
	}
}

