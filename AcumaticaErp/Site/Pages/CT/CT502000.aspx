<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CT502000.aspx.cs" Inherits="Page_CT502000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter"
		TypeName="PX.Objects.CT.RenewContracts">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Process" CommitChanges="true" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="ViewContract" Visible="False" DependOnGrid="grid" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edBeginDate">
<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />

			<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Expiration Period" />

				<px:PXDateTimeEdit CommitChanges="True" ID="edBeginDate" runat="server" DataField="BeginDate"  />
				<px:PXNumberEdit CommitChanges="True" ID="edExpireXDays" runat="server" AllowNull="False" DataField="ExpireXDays" />
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />
            <px:PXLabel runat="server"></px:PXLabel>
			<px:PXSelector CommitChanges="True" ID="edCustomerClassID" runat="server" DataField="CustomerClassID"  />
			<px:PXSegmentMask CommitChanges="True" ID="edTemplateID" runat="server" DataField="TemplateID"  /></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
		Width="100%" Caption="Contracts" SkinID="Inquire">
		<Levels>
			<px:PXGridLevel  DataMember="Items">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />

					<px:PXSelector ID="edContractID" runat="server" DataField="ContractID" Enabled="False" ValueField="ContractID"  />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Enabled="False"  />
					<px:PXDropDown ID="edType" runat="server" AllowNull="False" DataField="Type" Enabled="False"  />
					<px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" Enabled="False"  />
					<px:PXSegmentMask ID="edTemplateID" runat="server" DataField="TemplateID" Enabled="False"  />
					<px:PXDropDown ID="edStatus" runat="server" AllowNull="False" DataField="Status" Enabled="False"  />
					<px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" Enabled="False"  />
					<px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate" Enabled="False"  />
					<px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" /></RowTemplate>
				<Columns>
					<px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="30px" />
					<px:PXGridColumn AllowUpdate="False" DataField="ContractID" DisplayFormat="&gt;aaaaaaaaaa" Visible="False" LinkCommand="ViewContract" Width="120px"/>
					<px:PXGridColumn AllowUpdate="False" DataField="Description" Width="251px" />
					<px:PXGridColumn AllowUpdate="False" DataField="TemplateID" DisplayFormat="CCCCCCCCCC" Width="120px" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Type" Width="120px" RenderEditorText="True" />
					<px:PXGridColumn AllowUpdate="False" DataField="CustomerID" DisplayFormat="CCCCCCCCCC" Width="120px" />
					<px:PXGridColumn AllowUpdate="False" DataField="CustomerName" Width="200px" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Status" Width="90px" RenderEditorText="True" />
					<px:PXGridColumn AllowUpdate="False" DataField="StartDate" Width="90px" />
					<px:PXGridColumn AllowUpdate="False" DataField="ExpireDate" Width="90px" />
					<px:PXGridColumn AllowUpdate="False" DataField="LastDate" Width="90px" />
					<px:PXGridColumn AllowUpdate="False" DataField="NextDate" Width="90px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
