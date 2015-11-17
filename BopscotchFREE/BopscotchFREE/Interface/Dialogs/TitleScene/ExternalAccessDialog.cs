using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Asset_Management;

namespace Bopscotch.Interface.Dialogs.TitleScene
{
	public class ExternalActionDialog : ButtonDialog
    {
		private bool _locked;
		private float _spinnerRotation;

		public bool ActionSuccessful { get; private set; }
		public ActionType Action { get; set; }

		public ExternalActionDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = 400;

			AddButton("OK", new Vector2(Definitions.Back_Buffer_Center.X, 325), Button.ButtonIcon.Back, Color.LawnGreen,0.7f);

			_defaultButtonCaption = "OK";
			_cancelButtonCaption = "OK";
        }

		public override void Activate()
		{
			_boxCaption = "Contacting iTunes...";
			_buttons["OK"].Disabled = true;
			_locked = true;
			_spinnerRotation = 0.0f;

			ActionSuccessful = false;

			base.Activate();
		}

		public override void Update (int millisecondsSinceLastUpdate)
		{
			base.Update (millisecondsSinceLastUpdate);

			_spinnerRotation -= MathHelper.ToRadians(Spin_Degrees_Per_Millisecond) * millisecondsSinceLastUpdate;
		}

		public override void Draw (Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
		{
			base.Draw (spriteBatch);

			if (_locked)
			{
				spriteBatch.Draw(
					TextureManager.Textures["load-spinner"],
					GameBase.ScreenPosition(Definitions.Back_Buffer_Center.X, WorldPosition.Y + 150.0f),
					null,
					Color.White,
					_spinnerRotation,
					new Vector2(TextureManager.Textures["load-spinner"].Width, TextureManager.Textures["load-spinner"].Height) / 2.0f,
					GameBase.ScreenScale(),
					Microsoft.Xna.Framework.Graphics.SpriteEffects.None,
					0.1f);
			}
		}

		protected override bool HandleButtonTouch (string buttonCaption)
		{
			return ((!_locked) && (base.HandleButtonTouch(buttonCaption)));
		}

		public void CompleteAction(string message, bool wasSuccessful)
		{
			ActionSuccessful = wasSuccessful;

			_boxCaption = message;
			_buttons["OK"].Disabled = false;
			_locked = false;
		}

		public enum ActionType
		{
			PurchaseFullGame,
			RateGame
		}

		private const int Dialog_Height = 400;
		private const float Spin_Degrees_Per_Millisecond = 0.1f;
    }
}
