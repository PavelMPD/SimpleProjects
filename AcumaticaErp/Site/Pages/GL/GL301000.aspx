<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="GL301000.aspx.cs" Inherits="Page_GL301000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.GL.JournalEntry" PrimaryView="BatchModule">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="CurrencyView" Visible="False" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Release" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Action" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Report" CommitChanges="True" />
			<px:PXDSCallbackCommand Visible="false" Name="CreateSchedule" CommitChanges="True" />
			<px:PXDSCallbackCommand Visible="false" Name="ReverseBatch" CommitChanges="True" />
			<px:PXDSCallbackCommand Visible="false" Name="BatchRegisterDetails" />
			<px:PXDSCallbackCommand Visible="false" Name="GLEditDetails" />
            <px:PXDSCallbackCommand Visible="false" Name="GLReversingBatches" />
			<px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewDocument" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" DataMember="BatchModule" Caption="Batch Summary" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" LinkIndicator="True"
		NotifyIndicator="True" DefaultControlID="edModule" DataSourceID="ds" TabIndex="18100" >
		<Parameters>
			<px:PXQueryStringParam Name="Batch.module" QueryStringField="Module" Type="String" OnLoadOnly="True" />
		</Parameters>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXDropDown ID="edModule" runat="server" DataField="Module"/>
			<px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" AutoRefresh="True" DataSourceID="ds" />
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
			<px:PXCheckBox ID="chkHold" runat="server" DataField="Hold" OnValueChange="Commit" />
			<px:PXDateTimeEdit ID="edDateEntered" runat="server" DataField="DateEntered" OnValueChange="Commit" />
			<px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID" OnValueChange="Commit" DataSourceID="ds" />
			<px:PXLayoutRule runat="server" ColumnSpan="3">
            </px:PXLayoutRule>
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXSegmentMask ID="edBranchID" runat="server" DataField="BranchID" OnValueChange="Commit" DataSourceID="ds" />
			<px:PXSelector ID="edLedgerID" runat="server" DataField="LedgerID" OnValueChange="Commit" DataSourceID="ds" />
			<pxa:PXCurrencyRate ID="edCury" runat="server" DataMember="_Currency_" 
                DataField="CuryID" RateTypeView="_Batch_CurrencyInfo_" DataSourceID="ds"></pxa:PXCurrencyRate>
			<px:PXCheckBox ID="chkAutoReverse" runat="server" DataField="AutoReverse" OnValueChange="Commit" />
			<px:PXCheckBox ID="AutoReverseCopy" runat="server" DataField="AutoReverseCopy"  OnValueChange="Commit"/>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXSelector ID="edOrigBatchNbr" runat="server" DataField="OrigBatchNbr" Enabled="False" AllowEdit="True" DataSourceID="ds" />
			<px:PXNumberEdit ID="edCuryDebitTotal" runat="server" DataField="CuryDebitTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryCreditTotal" runat="server" DataField="CuryCreditTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryControlTotal" runat="server" DataField="CuryControlTotal" />
		    <px:PXCheckBox ID="chkCreateTaxTrans" runat="server" CommitChanges="True" 
                DataField="CreateTaxTrans">
            </px:PXCheckBox>
            <px:PXCheckBox ID="chkSkipTaxValidation" runat="server" CommitChanges="True" 
                DataField="SkipTaxValidation">
            </px:PXCheckBox>
            
            
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Width="100%" Caption="Transaction Details" SkinID="Details" Height="200px" SyncPosition="True" TabIndex="200">
		<Levels>
			<px:PXGridLevel DataMember="GLTranModuleBatNbr">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXSegmentMask ID="edBranchID" runat="server" DataField="BranchID" OnValueChange="Commit" />
					<px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" AutoRefresh="True" OnValueChange="Commit" />
					<px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" AutoRefresh="True" OnValueChange="Commit" />
					<px:PXTextEdit ID="edRefNbr" runat="server" DataField="RefNbr" Size="S" />
					<px:PXDateTimeEdit ID="edTranDate" runat="server" DataField="TranDate" Enabled="False" Size="S" />
					<px:PXSelector ID="edUOM" runat="server" DataField="UOM" AutoRefresh="True" OnValueChange="Commit" Size="S" />
					<px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" Size="S" />
					<px:PXNumberEdit ID="edCuryDebitAmt" runat="server" DataField="CuryDebitAmt" Size="S" />
					<px:PXNumberEdit ID="edCuryCreditAmt" runat="server" DataField="CuryCreditAmt" Size="S" />
					<px:PXLayoutRule runat="server" ColumnSpan="2" />
					<px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXSegmentMask ID="edProjectID" runat="server" DataField="ProjectID" OnValueChange="Commit" />
					<px:PXSegmentMask ID="edTaskID" runat="server" DataField="TaskID" AutoRefresh="True" />
					<px:PXSegmentMask ID="edReferenceID" runat="server" DataField="ReferenceID" Enabled="False" />
					<px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" Enabled="False" />
                    <px:PXSelector ID="edTaxID" runat="server" DataField="TaxID" AutoRefresh="True" OnValueChange="Commit" Size="S" />                    
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="LineNbr" TextAlign="Right" />
					<px:PXGridColumn DataField="BranchID" AutoCallBack="True" />
					<px:PXGridColumn DataField="AccountID" AutoCallBack="True" />
					<px:PXGridColumn DataField="AccountID_Account_description" Width="120px" />
					<px:PXGridColumn DataField="SubID" Width="120px" AutoCallBack="True"/>
					<px:PXGridColumn DataField="ProjectID" Width="90px" AutoCallBack="true" />
					<px:PXGridColumn DataField="TaskID" Label="Task" Width="90px" />
					<px:PXGridColumn DataField="RefNbr" Width="90px" />
					<px:PXGridColumn DataField="TranDate" Width="60px" DisplayFormat="d" />
					<px:PXGridColumn DataField="Qty" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="UOM" Width="54px" AutoCallBack="True" />
					<px:PXGridColumn DataField="CuryDebitAmt" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryCreditAmt" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="TranDesc" Width="180px" />
					<px:PXGridColumn DataField="InventoryID" />
					<px:PXGridColumn DataField="ReferenceID" Width="100px" />
                    <px:PXGridColumn DataField="TaxID" Width="100px" AutoCallBack="True" AllowShowHide="Server"/>
                    <px:PXGridColumn DataField="TaxCategoryID" Width="100px" AutoCallBack="True" AllowShowHide="Server"/>
                    <px:PXGridColumn DataField="NonBillable" Type="CheckBox" />
				</Columns>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode InitNewRow="True" AllowFormEdit="True" AllowUpload="True" />
		<Layout FormViewHeight="400px" />
		<LevelStyles>
			<RowForm Height="159px" />
		</LevelStyles>
		<ActionBar>
			<CustomItems>
				<px:PXToolBarButton Text="View Source Document">
				    <AutoCallBack Command="ViewDocument" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
