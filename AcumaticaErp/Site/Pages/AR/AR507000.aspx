<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR507000.aspx.cs" Inherits="Page_AR507000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.AR.ARFinChargesApplyMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="true" Name="Calculate" 
				StartNewGroup="True" />
			<px:PXDSCallbackCommand CommitChanges="true" Name="Process" StartNewGroup="true" />
			<px:PXDSCallbackCommand CommitChanges="true" Name="ProcessAll" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewDocument" Visible="false" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewLastFinCharge" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" BorderStyle="None" Caption="Selection" DataMember="Filter" DefaultControlID="edFinChargeDate">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="M" />

			<px:PXDateTimeEdit CommitChanges="True" ID="edFinChargeDate" runat="server" DataField="FinChargeDate"  />
			<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID"  />
			<px:PXSelector CommitChanges="True" ID="edStatementCycle" runat="server" DataField="StatementCycle"  />
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />

			<px:PXSelector ID="edCustomerClassID" runat="server" DataField="CustomerClassID" CommitChanges="True" />
			<px:PXSelector ID="edCustomerID" runat="server" DataField="CustomerID" CommitChanges="True" /></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100; left: 0px; top: 0px;" Width="100%" Caption="Overdue Charges Calculated Details" SkinID="Inquire">
		<Levels>
			<px:PXGridLevel  DataMember="ARFinChargeRecords">
				<Columns>
					<px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="50px" TextCase="Upper" />
					<px:PXGridColumn DataField="DocType" Type="DropDownList"  />
					<px:PXGridColumn DataField="RefNbr"  />
					<px:PXGridColumn DataField="DocDate" Width="60px" />
					<px:PXGridColumn DataField="CustomerID" DisplayFormat="CCCCCCCCCC" />
					<px:PXGridColumn AllowUpdate="False" DataField="CustomerID_BAccountR_acctName" Width="130px" />
					<px:PXGridColumn DataField="CuryID" DisplayFormat="&gt;LLLLL" Width="50px" />
					<px:PXGridColumn DataField="CuryOrigDocAmt" TextAlign="Right" Width="90px" />
					<px:PXGridColumn AllowUpdate="False" DataField="CuryDocBal" TextAlign="Right" Width="90px" />
					<px:PXGridColumn DataField="FinChargeID"  />
					<px:PXGridColumn DataField="FinChargeCuryID" DisplayFormat="&gt;LLLLL" Width="50px" />
					<px:PXGridColumn AllowNull="False" DataField="OverdueDays" TextAlign="Right" Width="60px" />
					<px:PXGridColumn DataField="FinChargeAmt" TextAlign="Right" Width="90px" />
          <px:PXGridColumn AllowUpdate="False" DataField="ARAccountID" DisplayFormat="&gt;AAAAAAAAAA" />
          <px:PXGridColumn AllowUpdate="False" DataField="ARSubID" DisplayFormat="&gt;AAAA-AA-AA-AAAA" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar  DefaultAction="cmdViewDocument">
			<Actions>
				<AddNew Enabled="False" />
				<NoteShow Enabled="False" />
			</Actions>
			<CustomItems>
				<px:PXToolBarButton Text="View Document" Key="cmdViewDocument">
				    <AutoCallBack Command="ViewDocument" Target="ds" />					
				</px:PXToolBarButton>
				<px:PXToolBarButton Text="Last Charge" >
					<AutoCallBack Command="ViewLastFinCharge" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
