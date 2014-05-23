<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR506000.aspx.cs" Inherits="Page_CR506000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%" TypeName="PX.Objects.CR.CRImportLeadsMaint"
		PrimaryView="Filter">
		<CallbackCommands>
			<px:PXDSCallbackCommand StartNewGroup="True" CommitChanges="True" Name="Process" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="ProcessAll" /> 
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%" Caption="Settings">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S"
				ControlSize="XM" />
			<px:PXDropDown ID="dpdnLeadSource" runat="server" DataField="Source" TextField="Source" />
			<px:PXSelector ID="edClassID" runat="server" DataField="ClassID" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="phG">
	<px:PXGrid ID="grdItems" runat="server" DataSourceID="ds" Style="z-index: 100; top: 0px;
		left: 0px; height: 361px;" Width="100%" Caption="Items" AllowPaging="True"
		AdjustPageSize="auto" AllowFilter="false" SkinID="Inquire" NoteIndicator="false" FilesIndicator="false">
		<Levels>
			<px:PXGridLevel DataMember="Items">
				<Columns>
					<px:PXGridColumn AllowCheckAll="True" AllowNull="False" AllowShowHide="False" DataField="Selected"
						TextAlign="Center" Type="CheckBox" Width="20px" />
					<px:PXGridColumn DataField="Title" Width="40px" RenderEditorText="True" />
					<px:PXGridColumn DataField="FirstName" Width="100px" />
					<px:PXGridColumn DataField="LastName" Width="100px" />
					<px:PXGridColumn DataField="Salutation" Width="100px" />
					<px:PXGridColumn DataField="FullName" Width="100px" />
					<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
						Width="40px" />
					<px:PXGridColumn DataField="WebSite" Width="100px" />
					<px:PXGridColumn DataField="EMail" Width="100px" />
					<px:PXGridColumn DataField="Phone1" DisplayFormat="+# (###) ###-#### Ext:####" Width="100px" />
					<px:PXGridColumn DataField="Phone2" DisplayFormat="+# (###) ###-#### Ext:####" Width="100px" />
					<px:PXGridColumn DataField="Phone3" DisplayFormat="+# (###) ###-#### Ext:####" Width="100px" />
					<px:PXGridColumn DataField="Fax" DisplayFormat="+# (###) ###-#### Ext:####" Width="100px" />
					<px:PXGridColumn DataField="DateOfBirth" Width="90px" />
					<px:PXGridColumn DataField="AddressLine1" Width="95px" Visible="False" />
					<px:PXGridColumn DataField="AddressLine2" Width="95px" Visible="False" />
					<px:PXGridColumn DataField="AddressLine3" Width="95px" Visible="False" />
					<px:PXGridColumn DataField="City" Width="90px" />
					<px:PXGridColumn DataField="CountryID" Width="90px" AutoCallBack="true" />
					<px:PXGridColumn DataField="State" Width="90px" />
					<px:PXGridColumn DataField="PostalCode" Width="90px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<ActionBar PagerVisible="False">
			<Actions>
				<Save Enabled="False" />
				<AddNew Enabled="False" />
				<Delete Enabled="False" />
				<EditRecord Enabled="False" />
			</Actions>
		</ActionBar>
		<AutoSize Enabled="True" Container="Window" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowFormEdit="True" AllowUpload="True" />
		<LevelStyles>
			<RowForm Height="281px" Width="627px">
			</RowForm>
		</LevelStyles>
	</px:PXGrid>
</asp:Content>
