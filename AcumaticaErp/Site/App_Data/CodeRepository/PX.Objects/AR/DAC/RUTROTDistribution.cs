using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PX.Common;
using PX.Data;
using PX.Objects.BQLConstants;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.AR;
using PX.Objects.GL;

namespace PX.Objects.AR
{
    [Serializable()]
    public partial class RUTROTDistribution : PX.Data.IBqlTable
    {
        #region DocType
        public abstract class docType : PX.Data.IBqlField
        {
        }

        [PXDBString(3, IsKey = true, IsFixed = true)]
        [PXDBDefault(typeof(ARInvoice.docType))]
        public virtual String DocType { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.IBqlField
        {
        }

        [PXParent(typeof(Select<ARInvoice, Where<ARInvoice.docType, Equal<Current<RUTROTDistribution.docType>>, And<ARInvoice.refNbr, Equal<Current<RUTROTDistribution.refNbr>>>>>))]
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
        [PXDBDefault(typeof(ARInvoice.refNbr))]
        public virtual String RefNbr { get; set; }
        #endregion
        #region CuryInfoID
        public abstract class curyInfoID : PX.Data.IBqlField
        {
        }

        [PXDBLong]
        [CurrencyInfo(typeof(ARInvoice.curyInfoID))]
        public virtual Int64? CuryInfoID { get; set; }
        #endregion
        #region PersonalID
        public abstract class personalID : IBqlField { }

        [PXDBString(20, IsUnicode = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Personal ID", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual String PersonalID { get; set; }
        #endregion

        #region CuryAmount
        public abstract class curyAmount : IBqlField { }

        [PXDBCurrency(typeof(RUTROTDistribution.curyInfoID), typeof(RUTROTDistribution.amount))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(null, typeof(SumCalc<ARInvoice.curyRUTROTDistributedAmt>))]
        [PXUIField(DisplayName = "Amount", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual Decimal? CuryAmount { get; set; }
        #endregion

        #region Amount
        public abstract class amount : IBqlField { }

        [PXDBBaseCury]
        public virtual Decimal? Amount { get; set; }
        #endregion

        #region CuryAllowance
        public abstract class curyAllowance : IBqlField { }

        [PXCurrency(typeof(RUTROTDistribution.curyInfoID), typeof(RUTROTDistribution.allowance))]
        [PXFormula(typeof(IsNull<Parent<ARInvoice.curyRUTROTPersonalAllowance>, CS.decimal0>), typeof(SumCalc<ARInvoice.curyRUTROTAllowedAmt>))]
        public virtual Decimal? CuryAllowance { get; set; }
        #endregion

        #region Allowance
        public abstract class allowance : IBqlField { }

        [PXBaseCury]
        public virtual Decimal? Allowance { get; set; }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.IBqlField
        {
        }
        protected Int32? _LineNbr;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr.", Visible = false, FieldClass = AR.RUTROTMessages.FieldClass)]
        [PXLineNbr(typeof(ARInvoice.rUTROTDistributionLineCntr))]
        public virtual Int32? LineNbr { get; set; }
        #endregion
    }
}
