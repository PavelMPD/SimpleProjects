<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TX502010.aspx.cs" Inherits="Page_TX502010" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.TX.ReportTaxDetail" PrimaryView="History_Header" PageLoadBehavior="PopulateSavedValues" >
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="ViewDocument" PostData="Self" Visible="false" DependOnGrid="grid"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="History_Header" Caption="Selection">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" AllowEdit="true" />
			<px:PXSelector CommitChanges="True" ID="edTaxPeriodID" runat="server" DataField="TaxPeriodID" Size="s" />
			<px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" />
			<px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDateInclusive" />
			<px:PXDropDown CommitChanges="True" ID="edLineNbr" runat="server" DataField="LineNbr" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="Top" Caption="Details" SkinID="Inquire">
		<ActionBar>
				<CustomItems>
					<px:PXToolBarButton Text="View Document" Tooltip="View Document">
					<AutoCallBack Target="ds" Command="ViewDocument">
								<Behavior CommitChanges="True" />
						</AutoCallBack>
					</px:PXToolBarButton>
				</CustomItems>
		</ActionBar>
		<Levels>
			<px:PXGridLevel DataMember="History_Detail" >
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXDateTimeEdit ID="edTranDate" runat="server" DataField="TranDate" />
					<px:PXDropDown ID="edModule" runat="server" DataField="Module" />
					<px:PXDropDown ID="edTranType" runat="server" DataField="TranType" />
					<px:PXTextEdit ID="edRefNbr" runat="server" DataField="RefNbr" />
					<px:PXTextEdit ID="edTaxID" runat="server" DataField="TaxID" />
					<px:PXNumberEdit ID="edTaxRate" runat="server" DataField="TaxRate" />
                    <px:PXNumberEdit ID="edNonDeductibleTaxRate" runat="server" DataField="NonDeductibleTaxRate" Enabled="False" />
					<px:PXNumberEdit ID="edTaxableAmt" runat="server" DataField="ReportTaxableAmt" />
					<px:PXNumberEdit ID="edTaxAmt" runat="server" DataField="ReportTaxAmt" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="BranchID" />
					<px:PXGridColumn DataField="Module" TextAlign="Left" Width="54px" />
					<px:PXGridColumn DataField="TranType" TextAlign="Left" Width="90px" />
					<px:PXGridColumn DataField="RefNbr" TextAlign="Left" Width="90px" />
					<px:PXGridColumn DataField="TranDate" TextAlign="Left" Width="90px" />
					<px:PXGridColumn DataField="TaxID" TextAlign="Left" Width="81px" />
					<px:PXGridColumn DataField="TaxRate" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="NonDeductibleTaxRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="ReportTaxableAmt" TextAlign="Right" Width="90px" />
					<px:PXGridColumn DataField="ReportTaxAmt" TextAlign="Right" Width="90px" />
                    <px:PXGridColumn DataField="BAccount__AcctCD" TextAlign="Right" Width="90px" />
                    <px:PXGridColumn DataField="BAccount__AcctName" TextAlign="Right" Width="90px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
