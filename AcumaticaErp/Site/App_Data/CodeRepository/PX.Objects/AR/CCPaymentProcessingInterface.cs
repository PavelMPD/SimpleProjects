using System;
using System.Collections.Generic;
using System.Text;

namespace PX.CCProcessing
{

	public enum CCTranType
	{
		AuthorizeAndCapture,
		AuthorizeOnly,
		PriorAuthorizedCapture,
		CaptureOnly,
		Credit,
		Void,
		VoidOrCredit,
	}

	public enum CCProcessingFeature
	{
		Base,
		Tokenization,
		HostedForm
	}

	public enum CCProcessingSettingsType
	{
		ProcessingCenter,
		PaymentMethodDetails
	}

	public enum CCTranStatus
	{
		Approved,
		Declined,
		Error,
		HeldForReview,
		Unknown
	}

	public enum CcvVerificationStatus
	{
		Match,
		NotMatch,
		NotProcessed,
		ShouldHaveBeenPresent,
		IssuerUnableToProcessRequest,
		RelyOnPreviousVerification,
		Unknown
	}

	[Flags]
	public enum CCResultFlag 
	{
		None = 0x0000,
		OrigTransactionExpired =0x0001,
		OrigTransactionNotFound =0x0002,
	}

	public class CCErrors
	{
		public enum CCErrorSource
		{
			None,
			Internal,
			ProcessingCenter,
			Network,
		}

		public string ErrorMessage;

		public CCErrorSource source;

		private static string[] _codes = { "NON", "INT", "PRC", "NET" };

		private static string[] _descr = {"No Error", "Internal Error", "Processing Center Error", "Network Error"};

		public static string GetCode(CCErrorSource aErrSrc)
		{
			return _codes[(int)aErrSrc];
		}

		public static string GetDescription(CCErrorSource aErrSrc)
		{
			return _descr[(int)aErrSrc];
		}
	}



	public class ProcessingResult
	{
		public int TranID;
		public CCTranStatus TranStatus;
		public bool isAuthorized;
		public string PCTranNumber;
		public string PCResponseCode;
		public string PCResponseReasonCode;
		public string PCResponse;
		public string PCCVVResponse;
		public string AuthorizationNbr;
		public string PCResponseReasonText;
		public string ErrorText;
		public int? ExpireAfterDays;
		public CCResultFlag ResultFlag;
		public CcvVerificationStatus CcvVerificatonStatus;
		public CCErrors.CCErrorSource ErrorSource = CCErrors.CCErrorSource.None;
	}

	public class APIResponse
	{
		public bool isSucess = false;
		public Dictionary<string, string> Messages; 
		public CCErrors.CCErrorSource ErrorSource = CCErrors.CCErrorSource.None;

		public APIResponse()
		{
			Messages = new Dictionary<string, string>();
		}
	}

	 

	public class SyncPMResponse
	{
		public Dictionary<string, Dictionary<string,string>> PMList;

		public SyncPMResponse()
		{
			PMList = new Dictionary<string, Dictionary<string, string>>();
		}
	}

	public class ProcessingInput
	{
		public int TranID;
		public int PMInstanceID;
		public string CustomerCD;
		public string DocType;
		public string DocRefNbr;
		public string OrigRefNbr;
		public string CuryID; //ISO Code
		public decimal Amount;
		public bool VerifyCVV;
	}

	public class DocDetailInfo 
	{
		public string ItemID;
		public string ItemName;
		public string ItemDescription;
		public decimal Quantity;
		public decimal Price;
		public bool? IsTaxable;

	}

	public interface IProcessingCenterSettingsStorage
	{
		void ReadSettings(Dictionary<string, string> aSettings);		
	}

	public interface ICreditCardDataReader
	{
		void ReadData(Dictionary<string, string> aData);
		string Key_CardNumber { get; }
		string Key_CardExpiryDate { get; }
		string Key_CardCVV { get;}
		string Key_PMCCProcessingID { get; }
	}

	public interface ICustomerDataReader
	{
		void ReadData(Dictionary<string, string> aData);
		string Key_CustomerCD { get; }
		string Key_CustomerName { get; }
		string Key_Customer_FirstName { get; }
		string Key_Customer_LastName { get; }
		string Key_Customer_CCProcessingID { get; }
		string Key_BillAddr_Country { get;}
		string Key_BillAddr_State { get;}
		string Key_BillAddr_City { get;}
		string Key_BillAddr_Address { get;}
		string Key_BillAddr_PostalCode { get;}
		string Key_BillContact_Phone { get;}
		string Key_BillContact_Fax { get;}
		string Key_BillContact_Email { get; }    
	}

	public interface IDocDetailsDataReader 
	{
		void ReadData(List<DocDetailInfo> aData);		
	}
	
	public abstract class ICCPaymentProcessing
	{
		abstract public void Initialize(IProcessingCenterSettingsStorage aSettingsReader, ICreditCardDataReader aCardDataReader, ICustomerDataReader aCustomerDataReader, IDocDetailsDataReader aDocDetailsReader);
		abstract public void Initialize(IProcessingCenterSettingsStorage aSettingsReader, ICreditCardDataReader aCardDataReader, ICustomerDataReader aCustomerDataReader);
		abstract public bool DoTransaction(CCTranType aType, ProcessingInput aInputData, ProcessingResult aResult);
		abstract public bool IsSupported(CCTranType aType);
		abstract public void ExportSettings(IList<ISettingsDetail> aSettings);
		abstract public void ExportSettings(IList<ISettingsDetail> aSettings, CCProcessingSettingsType settingsType);
		abstract public CCErrors ValidateSettings(ISettingsDetail setting);
		abstract public void TestCredentials(APIResponse apiResponse);
	}
	public interface ISettingsDetail
	{
		int? ControlType { get; set; }
		string Descr { get; set; }
		string DetailID { get; set; }
		string Value { get; set; }
		bool? IsEncryptionRequired { get; set; }

		IList<KeyValuePair<string, string>> GetComboValues();
		void SetComboValues(IList<KeyValuePair<string, string>> list);
	}
	public interface ICCTokenizedPaymnetProcessing
	{
		void CreateCustomer(APIResponse apiResponse, out long id);
		void CheckCustomerID(APIResponse apiResponse);
		void DeleteCustomer(APIResponse apiResponse);
		//PMI stands for Payment Method Instance
		void CreatePMI(APIResponse apiResponse, out long id);
		void GetPMI(APIResponse apiResponse, SyncPMResponse syncResponse);
		void DeletePMI(APIResponse apiResponse);
	}
	public interface ICCProcessingHostedForm
	{
		void CreatePaymentMethodHostedForm(APIResponse apiResponse, string callbackURL);
		void SynchronizePaymentMethods(APIResponse apiResponse, SyncPMResponse syncResponse);
		void ManagePaymentMethodHostedForm(APIResponse apiResponse, string callbackURL);
	}
}
