﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Asset_Management;
using Leda.Core.External_APIS.iOS;

using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Scenes.Gameplay.Survival;
using Bopscotch.Interface.Dialogs;
using Bopscotch.Interface.Dialogs.StoreScene;
using Bopscotch.Interface.Content;

namespace Bopscotch.Scenes.NonGame
{
    public class StoreScene : MenuDialogScene
    {
        private bool _returnToGame;

        private PurchaseCompleteDialog _purchaseCompleteDialog;
        private ConsumablesDialog _consumablesDialog;

        public StoreScene()
            : base()
        {
            _purchaseCompleteDialog = new PurchaseCompleteDialog("");
            _purchaseCompleteDialog.SelectionCallback = PurchaseDialogButtonCallback;
            _consumablesDialog = new ConsumablesDialog();

            _dialogs.Add("loading-store", new LoadingDialog(LoadProducts));
            _dialogs.Add("store-closed", new StoreClosedDialog());
            _dialogs.Add("store-items", new StorePurchaseDialog(RegisterGameObject, UnregisterGameObject));
            _dialogs.Add("purchase-complete", _purchaseCompleteDialog);
            _dialogs.Add("consumables", _consumablesDialog);
        }

        private void PurchaseDialogButtonCallback(string buttonCaption)
        {
            if ((buttonCaption == "Back") && (_consumablesDialog.Active)) { _consumablesDialog.DismissWithReturnValue(""); }
        }

        protected override void CompletePostStartupLoadInitialization()
        {
            base.CompletePostStartupLoadInitialization();
            CreateBackgroundForScene(Background_Texture_Name, new int[] { 0, 1, 2, 3 });

            foreach (KeyValuePair<string, ButtonDialog> kvp in _dialogs) { kvp.Value.ExitCallback = HandleActiveDialogExit; }

            GameBase.Instance.PurchaseManager.CompleteTransactionCallback = HandleTransactionCompleteCallback;
            GameBase.Instance.PurchaseManager.ProductLoadCompleteHandler = HandleProductLoadCompleteCallback;
        }

        public override void Activate()
        {
            _returnToGame = NextSceneParameters.Get<bool>("return-to-game");

            base.Activate();

            MusicManager.StopMusic();

            ActivateDialog("loading-store");
        }

        private void LoadProducts()
        {
            
            GameBase.Instance.PurchaseManager.RequestProductData(
                new List<string>() 
                {
                    "com.ledaentertainment.bopscotch.15lives",
                    "com.ledaentertainment.bopscotch.30lives",
                    "com.ledaentertainment.bopscotch.50lives",
                    "com.ledaentertainment.bopscotch.2tickets",
                    "com.ledaentertainment.bopscotch.5tickets"
                });
        }

        private void HandleProductLoadCompleteCallback(Dictionary<string, ProductContainer> products)
        {
            _dialogs["loading-store"].DismissWithReturnValue("");

            if ((products != null) && (products.Count > 0))
            {
                ((StorePurchaseDialog)_dialogs["store-items"]).InitializeProducts(products);
                _purchaseCompleteDialog.Products = products;
                ActivateDialog("store-items");
                _consumablesDialog.Activate();
            }
            else
            {
                ActivateDialog("store-closed");
            }
        }

        private void HandleTransactionCompleteCallback(string returnMessage, bool purchaseSucceeded)
        {
            if (purchaseSucceeded)
            {
                FulfillPurchase(returnMessage);
                _purchaseCompleteDialog.ItemCode = returnMessage;
                ActivateDialog("purchase-complete");
            }
            else
            { 
                ActivateDialog("store-items");
            }
        }

        private void HandleActiveDialogExit(string selectedOption)
        {
            if (selectedOption == "Buy")
            {
                InitiatePurchase(((StorePurchaseDialog)_dialogs["store-items"]).Selection);
            }
            else if (_lastActiveDialogName == "purchase-complete")
            {
                ActivateDialog("store-items");
            }
            else if (!string.IsNullOrWhiteSpace(selectedOption))
            {
                if ((_returnToGame) && (Data.Profile.Lives > 0))
                {
                    NextSceneType = typeof(SurvivalGameplayScene);
                    MusicManager.PlayLoopedMusic("survival-gameplay");
                }
                else
                {
                    NextSceneType = typeof(TitleScene);
                }
                Deactivate();
            }
        }

        private void InitiatePurchase(string selection)
        {
            GameBase.Instance.PurchaseManager.PurchaseProduct(selection);
        }

        private void FulfillPurchase(string productCode)
        {
            switch (productCode)
            {
                case "com.ledaentertainment.bopscotch.15lives": Data.Profile.Lives += 15; break;            // £0.79 - $0.99
                case "com.ledaentertainment.bopscotch.30lives": Data.Profile.Lives += 30; break;            // £1.49 - $1.99
                case "com.ledaentertainment.bopscotch.50lives": Data.Profile.Lives += 50; break;            // £2.29 - $2.99
                case "com.ledaentertainment.bopscotch.2tickets": Data.Profile.GoldenTickets += 2; break;    // £0.79
                case "com.ledaentertainment.bopscotch.5tickets": Data.Profile.GoldenTickets += 5; break;    // £1.49
            }

            Data.Profile.Save();
        }

        private const string Background_Texture_Name = "background-1";
        
        public const float Dialog_Margin = 40.0f;
    }
}
