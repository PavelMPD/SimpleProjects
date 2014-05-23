// This File is Distributed as Part of Acumatica Shared Source Code 
/* ---------------------------------------------------------------------*
*                               Acumatica Inc.                          *
*              Copyright (c) 1994-2011 All rights reserved.             *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ProjectX PRODUCT.        *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* ---------------------------------------------------------------------*/

using System;

namespace PX.Data
{
	/// <summary>
	/// Used in the writable cache events.
	/// </summary>
	[Flags]
	public enum PXDBOperation
	{
		Select = 0,
		Update = 1,
		Insert = 2,
		Delete = 3,
		Normal = 0,
		GroupBy = 4,
		Internal = 8,
		External = 12,
		Second = 16,
		ReadOnly = 24,
		Command = 3,
		Option = 28
	}

	//public enum PXDBAggregate
	//{
	//    None,
	//    Sum,
	//    Min,
	//    Max,
	//    Avg,
	//    GroupBy
	//}

	public enum PXAttributeLevel
	{
		Type,
		Cache,
		Item
	}

	public enum PXCacheRights
	{
		Denied,
		Select,
		Update,
		Insert,
		Delete
	}

	public class PXCacheRightsPrioritized
	{
		public readonly bool Prioritized;
		public readonly PXCacheRights Rights;
		public PXCacheRightsPrioritized(bool prioritized, PXCacheRights rights)
		{
			Prioritized = prioritized;
			Rights = rights;
		}
	}

	public enum PXMemberRights
	{
		Denied,
		Visible,
		Enabled
	}

	public class PXMemberRightsPrioritized
	{
		public readonly bool Prioritized;
		public readonly PXMemberRights Rights;
		public PXMemberRightsPrioritized(bool prioritized, PXMemberRights rights)
		{
			Prioritized = prioritized;
			Rights = rights;
		}
	}

	internal enum PXCacheOperation
	{
		Insert,
		Update,
		Delete
	}

	public enum PXTranStatus
	{
		Open,
		Completed,
		Aborted
	}

	public enum PXClearOption
	{
		PreserveData,
		PreserveTimeStamp,
		PreserveQueries,
		ClearAll,
		ClearQueriesOnly
	}

	public enum PXSpecialButtonType
	{
		Default,
		Save,
		SaveNotClose,
		Cancel,
		Refresh,
		Report
	}
}
