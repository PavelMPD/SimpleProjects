<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AR511500.aspx.cs" Inherits="Page_AR511500"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AR.ARPaymentsAutoProcessing"
		PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Process" CommitChanges="true" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="ProcessAll" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false"  Name="ViewDocument" DependOnGrid="grid"/>
			<px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edPayDate">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="M" />

			<px:PXDateTimeEdit CommitChanges="True" ID="edPayDate" runat="server" DataField="PayDate"  />
			<px:PXSelector CommitChanges="True" ID="edStatementCycleId" runat="server" DataField="StatementCycleId"  />
			<px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID"  />
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="M" />

			<px:PXSelector CommitChanges="True" ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID"  />
			<px:PXSelector CommitChanges="True" ID="edProcessingCenterID" runat="server" DataField="ProcessingCenterID" AutoRefresh="True" />
			<pxa:PXCurrencyRate  DataField="CuryID"
				ID="edCury" runat="server" DataSourceID="ds" 
                RateTypeView="_PaymentFilter_CurrencyInfo_" DataMember="_Currency_">
			</pxa:PXCurrencyRate></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="288px" Style="z-index: 100; left: 0px; top: 0px;"
		Width="100%" Caption="Payment Details" AllowPaging="true" AdjustPageSize="Auto" 
		SkinID="Details">
		<Levels>
			<px:PXGridLevel DataMember="ARDocumentList" >
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />

					<px:PXLayoutRule runat="server" Merge="True" />

					<px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
					<px:PXDropDown Size="s" ID="edDocType" runat="server" DataField="DocType" Enabled="False"  />
					<px:PXLayoutRule runat="server" Merge="False" />

					<px:PXSelector ID="edRefNbr" runat="server" AllowEdit="True" DataField="RefNbr" Enabled="False"  /></RowTemplate>
				<Columns>					
					<px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" />
					<px:PXGridColumn AllowUpdate="False" DataField="DocType" RenderEditorText="True" />
					<px:PXGridColumn AllowUpdate="False" DataField="RefNbr"  />
					<px:PXGridColumn AllowUpdate="False" DataField="DocDate" Width="90px" />
					<px:PXGridColumn AllowUpdate="False" DataField="FinPeriodID" DisplayFormat="##-####"  />					
					<px:PXGridColumn AllowUpdate="False" DataField="CustomerID" DisplayFormat="&gt;AAAAAAAAAA" />
					<px:PXGridColumn DataField="CustomerID_BAccountR_acctName" Width="140px" />					
					<px:PXGridColumn AllowUpdate="False" DataField="CustomerLocationID" DisplayFormat="&gt;AAAAAA" />
					<px:PXGridColumn AllowUpdate="False" DataField="DocDesc" Width="200px" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Status" RenderEditorText="True" />
					<px:PXGridColumn AllowUpdate="False" DataField="CuryID" DisplayFormat="&gt;LLLLL"  />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryOrigDocAmt" TextAlign="Right" Width="100px" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryDocBal" TextAlign="Right" Width="100px" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryDiscBal" TextAlign="Right" Width="100px" />					
					
					<px:PXGridColumn AllowUpdate="False" DataField="CCPaymentStateDescr" Width="200px" />
				
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="PMInstanceDescr" Width="200px" />
					<px:PXGridColumn DataField="IsCCExpired" Width="40px" Type="CheckBox" />
					<px:PXGridColumn AllowUpdate="False" DataField="ProcessingCenterID" Width="90px" />				
					<px:PXGridColumn DataField="ProcessingCenterID_CCProcessingCenter_Name" Width="140px" />					
					<px:PXGridColumn DataField="CCTranDescr" Width="140px" />					
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode InitNewRow="True" AllowDelete="True" />
		<ActionBar>
			<CustomItems>
				<px:PXToolBarButton Text="View Payment" Tooltip="View Payment" >
				    <AutoCallBack Command="ViewDocument" Target="ds">
				        <Behavior CommitChanges="True" />
					</AutoCallBack>
				</px:PXToolBarButton>				
				
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
