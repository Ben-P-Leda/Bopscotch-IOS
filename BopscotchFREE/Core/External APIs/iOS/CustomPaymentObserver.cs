using System;
using System.Linq;
using StoreKit;
using Foundation;
using UIKit;

namespace Leda.Core.External_APIS.iOS
{
	internal class CustomPaymentObserver : SKPaymentTransactionObserver 
	{
		public InAppPurchaseManager.TransactionCompletionCallback CompleteTransactionCallback { private get; set; }

		private InAppPurchaseManager theManager;
		
		public CustomPaymentObserver(InAppPurchaseManager manager)
		{
			theManager = manager;
		}
		
		// called when the transaction status is updated
		public override void UpdatedTransactions (SKPaymentQueue queue, SKPaymentTransaction[] transactions)
		{
			Console.WriteLine ("UpdatedTransactions");
			foreach (SKPaymentTransaction transaction in transactions)
			{
			    switch (transaction.TransactionState)
			    {
			        case SKPaymentTransactionState.Purchased:
			           theManager.CompleteTransaction(transaction);
			            break;
			        case SKPaymentTransactionState.Failed:
			           theManager.FailedTransaction(transaction);
			            break;
			        case SKPaymentTransactionState.Restored:
			            theManager.RestoreTransaction(transaction);
			            break;
			        default:
			            break;
			    }
			}
		}
		
		public override void PaymentQueueRestoreCompletedTransactionsFinished (SKPaymentQueue queue)
		{
			// Restore succeeded
			Console.WriteLine(" ** RESTORE PaymentQueueRestoreCompletedTransactionsFinished ");
			if (CompleteTransactionCallback != null)
			{
				CompleteTransactionCallback("", true);
			}
		}
		public override void RestoreCompletedTransactionsFailedWithError (SKPaymentQueue queue, NSError error)
		{
			// Restore failed somewhere...
			Console.WriteLine(" ** RESTORE RestoreCompletedTransactionsFailedWithError " + error.LocalizedDescription);
			if (CompleteTransactionCallback != null)
			{
				CompleteTransactionCallback(error.LocalizedDescription, false);
			}
		}
	}
}

