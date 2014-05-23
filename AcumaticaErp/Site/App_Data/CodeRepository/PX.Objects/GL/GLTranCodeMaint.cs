using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.CM;

namespace PX.Objects.GL
{

	[Serializable]
	public partial class TranTypeDescr : IBqlTable
	{
				#region Module
				public abstract class module : IBqlField { }
				protected String _Module;
				[PXString(3, IsKey = true)]
				public String Module
				{
					get { return this._Module; }
					set { this._Module = value; }
				}
				#endregion

				#region TranType
				public abstract class tranType : IBqlField { }
				protected String _TranType;

				[PXString(3, IsKey = true)]
				[PXUIField(DisplayName = "Tran. Type")]
				public String TranType
				{
					get { return this._TranType; }
					set { this._TranType = value; }
				}

				#endregion
				#region Descr
				public abstract class descr : IBqlField { }
				protected String _Descr;
				[PXString(45, IsUnicode = true)]
				[PXUIField(DisplayName = "Description")]
				public String Descr
				{
					get { return this._Descr; }
					set { this._Descr = value; }
				}
				#endregion

				public TranTypeDescr Copy(string aTranType, string aDescr)
				{
					this._TranType = aTranType;
					this._Descr = aDescr;
					return this;
				}
	}

    [Serializable]
    public class GLTranCodeMaint : PXGraph<GLTranCodeMaint>
    {
		#region Type Override
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]		
		[ModuleTranTypeSelector(DescriptionField=typeof(TranTypeDescr.descr))]
		[PXUIField(DisplayName = "Module Tran. Type", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void GLTranCode_TranType_CacheAttached(PXCache sender)
		{
		}


        [Serializable]
		public class ModuleTranTypeSelector : PXCustomSelectorAttribute
		{
			#region Ctor + Methods
			public ModuleTranTypeSelector()
				: base(typeof(Search<TranTypeDescr.tranType, Where<TranTypeDescr.module, Equal<Argument<string>>>>),
					typeof(TranTypeDescr.descr))
			{
			}

			protected override string GenerateViewName()
			{
				return "_TranTypeDescr_";
			}

			public virtual IEnumerable GetRecords(string module)
			{
				Dictionary<string, string> types;
				switch (module)
				{
					case GL.BatchModule.AP:
						if (Types.TryGetValue(GL.BatchModule.AP, out types))
						{
							foreach (KeyValuePair<string, string> pair in types)
							{
								yield return ((new TranTypeDescr()).Copy(pair.Key, PXMessages.LocalizeNoPrefix(pair.Value)));
							}
						}
						break;
					case GL.BatchModule.AR:
						if (Types.TryGetValue(GL.BatchModule.AR, out types))
						{
							foreach (KeyValuePair<string, string> pair in types)
							{
								yield return ((new TranTypeDescr()).Copy(pair.Key, PXMessages.LocalizeNoPrefix(pair.Value)));
							}
						}
						break;
					case GL.BatchModule.CA:
						if (Types.TryGetValue(GL.BatchModule.CA, out types))
						{
							foreach (KeyValuePair<string, string> pair in types)
							{
								yield return ((new TranTypeDescr()).Copy(pair.Key, PXMessages.LocalizeNoPrefix(pair.Value)));
							}
						}
						break;
					case GL.BatchModule.GL:
						if (Types.TryGetValue(GL.BatchModule.GL, out types))
						{
							foreach (KeyValuePair<string, string> pair in types)
							{
								yield return ((new TranTypeDescr()).Copy(pair.Key, PXMessages.LocalizeNoPrefix(pair.Value)));
							}
						}
						break;
					default:
						yield break;
				}
			}

			public override void DescriptionFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, string alias)
			{
				base.DescriptionFieldSelecting(sender, e, alias);
				if (e.ReturnValue == null)
				{
					GLTranCode row = e.Row as GLTranCode;
					if (row != null && row.TranType!=null)
					{
						Dictionary<string, string> types;
						if (Types.TryGetValue(row.Module, out types))
						{
							string temp;
							if (types.TryGetValue(row.TranType, out temp))
								e.ReturnValue = PXMessages.LocalizeNoPrefix(temp);
							else
								row.TranType = null;
						}
					}
				}
			}
			#endregion

			#region Static types
			protected internal static Dictionary<string, string> APTypes = new Dictionary<string, string>()
			{
				{AP.APInvoiceType.Invoice, AP.Messages.Invoice},
				{AP.APInvoiceType.QuickCheck, AP.Messages.QuickCheck},
				{AP.APInvoiceType.Check, AP.Messages.Check},
				{AP.APInvoiceType.CreditAdj, AP.Messages.CreditAdj},
				{AP.APInvoiceType.DebitAdj, AP.Messages.DebitAdj},
				{AP.APInvoiceType.Prepayment, AP.Messages.Prepayment}
			};
			protected internal static Dictionary<string, string> ARTypes = new Dictionary<string, string>()
			{
				{AR.ARInvoiceType.Invoice, AR.Messages.Invoice},
				{AR.ARInvoiceType.CashSale, AR.Messages.CashSale},
				{AR.ARInvoiceType.CreditMemo, AR.Messages.CreditMemo},
				{AR.ARInvoiceType.DebitMemo, AR.Messages.DebitMemo},
				{AR.ARInvoiceType.Payment, AR.Messages.Payment}
			};
			protected internal static Dictionary<string, string> CATypes = new Dictionary<string, string>()
			{
				{CA.CATranType.CAAdjustment, CA.Messages.CAAdjustment}
			};
			protected internal static Dictionary<string, string> GLTypes = new Dictionary<string, string>()
			{
				{CA.CAAPARTranType.GLEntry, CA.Messages.GLEntry}
			};
			protected internal static Dictionary<string, Dictionary<string, string>> Types = new Dictionary<string, Dictionary<string, string>>()
			{
				{GL.BatchModule.AP, APTypes},
				{GL.BatchModule.AR, ARTypes},
				{GL.BatchModule.CA, CATypes},
				{GL.BatchModule.GL, GLTypes}
			};
			#endregion
		}				
		#endregion

        [PXImport(typeof(GLTranCode))]
        public PXSelect<GLTranCode> TranCodes;
	
        public PXSavePerRow<GLTranCode> Save;
        public PXCancel<GLTranCode> Cancel;

		public virtual void GLTranCode_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row != null)
			{
				GLTranCode row = (GLTranCode)e.Row;
				bool isSupported = IsSupported(row);
				if (!isSupported)
				{
					PXErrorLevel level = (row.Active == true) ? PXErrorLevel.Error : PXErrorLevel.Warning;
					sender.RaiseExceptionHandling<GLTranCode.tranType>(row, row.TranType, new PXSetPropertyException(Messages.DocumentTypeIsNotSupportedYet, level));
				}
				else
				{
					sender.RaiseExceptionHandling<GLTranCode.tranType>(row, row.TranType, null);
				}
			}
		}

		public virtual void GLTranCode_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row != null)
			{
				GLTranCode row = (GLTranCode)e.Row;
				if (row.Active == true)
				{
					if (!IsSupported(row))
					{
						sender.RaiseExceptionHandling<GLTranCode.tranType>(row, row.TranType, new PXSetPropertyException(Messages.DocumentTypeIsNotSupportedYet, PXErrorLevel.Error));
					}
				}
			}
		}

		public virtual void GLTranCode_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			if (e.Row == null) return;

			PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof (GLTranDoc.tranCode));
		}

		protected static bool IsSupported(GLTranCode row)
		{
			bool isSupported = true;
			if ((row.Module == GL.BatchModule.AP &&
					(row.TranType == AP.APPaymentType.Refund ||
						//row.TranType == AP.APPaymentType.Check ||
						row.TranType == AP.APPaymentType.VoidCheck ||
						row.TranType == AP.APPaymentType.VoidQuickCheck))
				|| (row.Module == GL.BatchModule.AR
					&& (row.TranType == AR.ARPaymentType.Refund ||
						row.TranType == AR.ARPaymentType.FinCharge ||						
						row.TranType == AR.ARPaymentType.SmallBalanceWO ||
						row.TranType == AR.ARPaymentType.SmallCreditWO ||						
						row.TranType == AR.ARPaymentType.NoUpdate ||
						row.TranType == AR.ARPaymentType.Undefined ||
						row.TranType == AR.ARPaymentType.VoidPayment||
						row.TranType == AR.ARPaymentType.CashReturn))
				|| (row.Module == GL.BatchModule.CA
					&& (row.TranType == CA.CATranType.CAAdjustmentRGOL ||
						row.TranType == CA.CATranType.CADeposit ||
						row.TranType == CA.CATranType.CAVoidDeposit ||
						row.TranType == CA.CATranType.CATransferExp ||
						row.TranType == CA.CATranType.CATransferOut ||
						row.TranType == CA.CATranType.CATransferIn)))
			{
				isSupported = false;
			}
			return isSupported;
		}
    }
}
