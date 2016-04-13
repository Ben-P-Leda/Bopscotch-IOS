﻿using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.External_APIS.iOS;

using Bopscotch.Interface;

namespace Bopscotch.Interface.Dialogs.StoreScene
{
    public class PurchaseCompleteDialog : ButtonDialog
    {
        public Dictionary<string, ProductContainer> Products { private get; set; }
        public string ItemCode { private get; set; }

        private bool _displayBonusMessage;

        public PurchaseCompleteDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Definitions.Back_Buffer_Center.Y - (Dialog_Height / 2.0f);

            AddButton("OK", new Vector2(Definitions.Back_Buffer_Center.X, 250), Button.ButtonIcon.Tick, Color.LawnGreen);

            _cancelButtonCaption = "OK";
            _boxCaption = "";
        }

        public override void Activate()
        {
            _displayBonusMessage = false;

            Product selected = Products.FirstOrDefault(x => x.ProductId == ItemCode);
            string productName = selected != null ? selected.Title : ItemCode;
            productName = productName.Replace(" (Bopscotch)", "");

            _boxCaption = Translator.Translation("purchase-complete").Replace("[ITEM]", productName);

            _displayBonusMessage = Data.Profile.AwardIapReward();   

            base.Activate();
        }

        public override void Reset()
        {
            base.Reset();
            WorldPosition = new Vector2(0.0f, -Height);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_displayBonusMessage)
            {
                TextWriter.Write(
                    Translator.Translation("Bonus reward - mummy constume unlocked!"), spriteBatch, 
                    new Vector2(Definitions.Back_Buffer_Center.X, 200.0f + WorldPosition.Y),
                    Color.White, Color.Black, 3.0f, 0.7f, 0.1f, TextWriter.Alignment.Center);
            }

            base.Draw(spriteBatch);
        }

        private const int Dialog_Height = 350;
        private const float Top_Line_Y = 80.0f;
        private const float Line_Height = 50.0f;
    }
}
