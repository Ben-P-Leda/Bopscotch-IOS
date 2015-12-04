using System;
using System.Collections.Generic;
using StoreKit;
using Foundation;
using UIKit;

namespace Leda.Core.External_APIS.iOS
{
	public class InAppPurchaseManager : SKProductsRequestDelegate 
	{
        public delegate void ProductLoadCompleteCallback(Dictionary<string, ProductContainer> products);
        public ProductLoadCompleteCallback ProductLoadCompleteHandler { private get; set; }

		public delegate void TransactionCompletionCallback(string message, bool transactionSucceeded);
		private TransactionCompletionCallback _completeTransactionCallback;
		public TransactionCompletionCallback CompleteTransactionCallback 
		{
			set
			{
				_completeTransactionCallback = value;
				theObserver.CompleteTransactionCallback = value;
			}
			private get
			{
				return _completeTransactionCallback;
			}
		}

		public static NSString InAppPurchaseManagerProductsFetchedNotification = new NSString("InAppPurchaseManagerProductsFetchedNotification");
		public static NSString InAppPurchaseManagerTransactionFailedNotification = new NSString("InAppPurchaseManagerTransactionFailedNotification");
		public static NSString InAppPurchaseManagerTransactionSucceededNotification = new NSString("InAppPurchaseManagerTransactionSucceededNotification");
		public static NSString InAppPurchaseManagerRequestFailedNotification = new NSString("InAppPurchaseManagerRequestFailedNotification");

		SKProductsRequest productsRequest;
		CustomPaymentObserver theObserver;

		public static Action Done {get;set;}

		public InAppPurchaseManager ()
		{
			theObserver = new CustomPaymentObserver(this);
			SKPaymentQueue.DefaultQueue.AddTransactionObserver(theObserver);

			CompleteTransactionCallback = null;
		}
		
		// Verify that the iTunes account can make this purchase for this application
		public bool CanMakePayments()
		{
			return SKPaymentQueue.CanMakePayments;	
		}

		// request multiple products at once
		public void RequestProductData (List<string> productIds)
		{
			var array = new NSString[productIds.Count];
			for (var i = 0; i < productIds.Count; i++) {
				array[i] = new NSString(productIds[i]);
			}
		 	NSSet productIdentifiers = NSSet.MakeNSObjectSet<NSString>(array);			

			//set up product request for in-app purchase
			productsRequest = new SKProductsRequest(productIdentifiers);
			productsRequest.Delegate = this; // SKProductsRequestDelegate.ReceivedResponse
			productsRequest.Start();
		}
		// received response to RequestProductData - with price,title,description info
		public override void ReceivedResponse (SKProductsRequest request, SKProductsResponse response)
		{
			SKProduct[] products = response.Products;
           
            Dictionary<string, ProductContainer> productsList = new Dictionary<string, ProductContainer>();
            List<string> invalidProducts = new List<string>();

			NSDictionary userInfo = null;
			if (products.Length > 0) {
				NSObject[] productIdsArray = new NSObject[response.Products.Length];
				NSObject[] productsArray = new NSObject[response.Products.Length];
				for (int i = 0; i < response.Products.Length; i++) 
                {
					productIdsArray[i] = new NSString(response.Products[i].ProductIdentifier);
					productsArray[i] = response.Products[i];

                    ProductContainer container = new ProductContainer()
                    {
                        Id = response.Products[i].ProductIdentifier,
                        FormattedPrice = GetFormattedPrice(response.Products[i].Price),
                        Name = response.Products[i].LocalizedTitle
                    };
                    productsList.Add(container.Id, container);
				}
				userInfo = NSDictionary.FromObjectsAndKeys (productsArray, productIdsArray);
			}
			NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseManagerProductsFetchedNotification,this,userInfo);

			foreach (string invalidProductId in response.InvalidProducts) 
            {
				Console.WriteLine("Invalid product id: " + invalidProductId );
                invalidProducts.Add(invalidProductId);
			}

            if (ProductLoadCompleteHandler != null)
            {
                ProductLoadCompleteHandler(productsList);
            }
		}

        private string GetFormattedPrice(NSDecimalNumber price)
        {
            Decimal d = Convert.ToDecimal(price.ToString());
            return d.ToString("C", System.Globalization.CultureInfo.CurrentCulture);
        }

		public void PurchaseProduct(string appStoreProductId)
		{
			Console.WriteLine("PurchaseProduct " + appStoreProductId);
			SKPayment payment = SKPayment.PaymentWithProduct (appStoreProductId);	
			SKPaymentQueue.DefaultQueue.AddPayment (payment);
		}

		public void CompleteTransaction (SKPaymentTransaction transaction)
		{
			Console.WriteLine ("CompleteTransaction " + transaction.TransactionIdentifier);
			var productId = transaction.Payment.ProductIdentifier;

			if (CompleteTransactionCallback != null) { CompleteTransactionCallback(productId, true); }

			FinishTransaction (transaction, true);
		}
		public void RestoreTransaction (SKPaymentTransaction transaction)
		{
			// Restored Transactions always have an 'original transaction' attached
			Console.WriteLine("RestoreTransaction " + transaction.TransactionIdentifier + "; OriginalTransaction " + transaction.OriginalTransaction.TransactionIdentifier);
			var productId = transaction.OriginalTransaction.Payment.ProductIdentifier;
			// Register the purchase, so it is remembered for next time
			if (CompleteTransactionCallback != null) { CompleteTransactionCallback(productId, true); }
			//PhotoFilterManager.Purchase(productId); // it's as though it was purchased again
			FinishTransaction(transaction, true);
		}
		public void FailedTransaction (SKPaymentTransaction transaction)
		{
			string errorMessage = "Unable to connect to iTunes";
			if (transaction.Error != null)
			{
				if (transaction.Error.Code == 2)
				{
					Console.WriteLine("User CANCELLED FailedTransaction Code=" + transaction.Error.Code + " " + transaction.Error.LocalizedDescription);
				}
				else
				{
					Console.WriteLine("FailedTransaction Code=" + transaction.Error.Code + " " + transaction.Error.LocalizedDescription);
				}
				errorMessage = transaction.Error.LocalizedDescription;
			}

			if (CompleteTransactionCallback != null) { CompleteTransactionCallback(errorMessage, false); }

			FinishTransaction(transaction,false);
		}
		public void FinishTransaction(SKPaymentTransaction transaction, bool wasSuccessful)
		{
			Console.WriteLine("FinishTransaction " + wasSuccessful);
			// remove the transaction from the payment queue.
			SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);		// THIS IS IMPORTANT - LET'S APPLE KNOW WE'RE DONE !!!!
			
			using (var pool = new NSAutoreleasePool()) {
				NSDictionary userInfo = NSDictionary.FromObjectsAndKeys(new NSObject[] {transaction},new NSObject[] {new NSString("transaction")});
				if (wasSuccessful) {
					// send out a notification that we’ve finished the transaction
					NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseManagerTransactionSucceededNotification,this,userInfo);
				} else {
					// send out a notification for the failed transaction
					NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseManagerTransactionFailedNotification,this,userInfo);
				}
			}
		}

		/// <summary>
		/// Probably could not connect to the App Store (network unavailable?)
		/// </summary>
		public override void RequestFailed (SKRequest request, NSError error)
		{
			Console.WriteLine (" ** InAppPurchaseManager RequestFailed() " + error.LocalizedDescription);
			using (var pool = new NSAutoreleasePool()) {
				NSDictionary userInfo = NSDictionary.FromObjectsAndKeys(new NSObject[] {error},new NSObject[] {new NSString("error")});
				// send out a notification for the failed transaction
				NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseManagerRequestFailedNotification,this,userInfo);
			}
		}

		/// <summary>
		/// Restore any transactions that occurred for this Apple ID, either on 
		/// this device or any other logged in with that account.
		/// </summary>
		public void Restore()
		{
			Console.WriteLine (" ** InAppPurchaseManager Restore()");
			// theObserver will be notified of when the restored transactions start arriving <- AppStore
			SKPaymentQueue.DefaultQueue.RestoreCompletedTransactions();			
		}
	}
}