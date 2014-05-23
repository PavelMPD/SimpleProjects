<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CT401000.aspx.cs" Inherits="Page_CT401000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter"
		TypeName="PX.Objects.CT.ExpiringContractsEng">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="ViewContract" Visible="False" DependOnGrid="grid" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edBeginDate">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />

			<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Expiration Period" />

				<px:PXDateTimeEdit CommitChanges="True" ID="edBeginDate" runat="server" DataField="BeginDate"  />
				<px:PXNumberEdit CommitChanges="True" ID="edExpireXDays" runat="server" AllowNull="False" DataField="ExpireXDays" />
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />
            <px:PXLabel runat="server"></px:PXLabel>
			<px:PXSelector CommitChanges="True" ID="edCustomerClassID" runat="server" DataField="CustomerClassID"  />
			<px:PXSegmentMask CommitChanges="True" ID="edTemplateID" runat="server" DataField="TemplateID"  />
			  
			<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkShowAutoRenewable" runat="server" DataField="ShowAutoRenewable" /></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
		Width="100%" Caption="Contracts" SkinID="Inquire" RestrictFields="True">
		<ActionBar  DefaultAction="cmdViewContract">
			<CustomItems>
				<px:PXToolBarButton Text="View Contract" Key="cmdViewContract">
				    <AutoCallBack Command="ViewContract" Target="ds">
					</AutoCallBack>
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<Levels>
			<px:PXGridLevel  DataMember="Contracts">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />

					<px:PXSegmentMask ID="edContractCD" runat="server" DataField="ContractCD" AllowEdit="True" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description"  />
					<px:PXDropDown ID="edType" runat="server" AllowNull="False" DataField="Type"  />
					<px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID"  />
					<px:PXSegmentMask ID="edTemplateID" runat="server" DataField="TemplateID"  />
					<px:PXDropDown ID="edStatus" runat="server" AllowNull="False" DataField="Status"  />
					<px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate"  />
					<px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate"  />
					<px:PXCheckBox ID="chkAutoRenew" runat="server" DataField="AutoRenew" />
					<px:PXDateTimeEdit ID="edContractBillingSchedule__NextDate" runat="server" DataField="ContractBillingSchedule__NextDate" Enabled="False"  />
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />

					<px:PXDateTimeEdit ID="edContractBillingSchedule__LastDate" runat="server" DataField="ContractBillingSchedule__LastDate" Enabled="False"  /></RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="ContractCD" DisplayFormat="&gt;aaaaaaaaaa" Width="81px" />
					<px:PXGridColumn DataField="Description" Width="351px" />
					<px:PXGridColumn DataField="TemplateID" DisplayFormat="CCCCCCCCCC" Width="120px" />
					<px:PXGridColumn AllowNull="False" DataField="Type" Width="120px" RenderEditorText="True" />
					<px:PXGridColumn DataField="CustomerID" DisplayFormat="CCCCCCCCCC" Width="81px" />
					<px:PXGridColumn AllowUpdate="False" DataField="Customer__AcctName" Width="200px" />
					<px:PXGridColumn AllowNull="False" DataField="Status" Width="117px" RenderEditorText="True" />
					<px:PXGridColumn DataField="StartDate" Width="90px" />
					<px:PXGridColumn DataField="ExpireDate" Width="90px" />
					<px:PXGridColumn AllowNull="False" DataField="AutoRenew" TextAlign="Center" Type="CheckBox" Width="120px" />
					<px:PXGridColumn AllowUpdate="False" DataField="ContractBillingSchedule__LastDate" Width="90px" />
					<px:PXGridColumn AllowUpdate="False" DataField="ContractBillingSchedule__NextDate" Width="90px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
