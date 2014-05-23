<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM200550.aspx.cs" Inherits="Page_SM200550"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Locales" TypeName="PX.SM.LocaleMaintenance" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="editFormatCommand" DependOnGrid="grid" Visible="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXSmartPanel ID="pnlEditFormat" runat="server" CaptionVisible="True" Caption="Locale Preferences"
		ForeColor="Black" Style="position: static" Height="305px" Width="850px" LoadOnDemand="True"
		Key="Formats" AutoCallBack-Target="formEditFormat" AutoCallBack-Command="Refresh"
		DesignView="Content">
		<px:PXFormView ID="formEditFormat" runat="server" DataSourceID="ds" Style="z-index: 100"
			Width="100%" DataMember="Formats" SkinID="Transparent" Caption="Custom Locale Format"
			TemplateContainer="">
			<AutoSize Container="Window" Enabled="True" MinHeight="200" />
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M"
					ColumnSpan="2" ColumnWidth="XXL" />
				<px:PXSelector CommitChanges="True" ID="edTemplateLocale" runat="server" DataField="TemplateLocale"
					DataMember="_Locale_" DataSourceID="ds" />
				<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Date and Time Formats"
					ControlSize="M" LabelsWidth="SM" StartColumn="True" StartRow="True" />
				<px:PXSelector ID="edDateTimePattern" runat="server" DataField="DateTimePattern"
					AutoRefresh="True" DataSourceID="ds" />
				<px:PXSelector ID="edTimeShortPattern" runat="server" DataField="TimeShortPattern"
					AutoRefresh="True" DataSourceID="ds" />
				<px:PXSelector ID="edTimeLongPattern" runat="server" DataField="TimeLongPattern"
					AutoRefresh="True" DataSourceID="ds" />
				<px:PXSelector ID="edDateShortPattern" runat="server" DataField="DateShortPattern"
					AutoRefresh="True"  />
				<px:PXSelector ID="edDateLongPattern" runat="server" DataField="DateLongPattern"
					AutoRefresh="True" DataSourceID="ds" />
				<px:PXTextEdit ID="edAMDesignator" runat="server" DataField="AMDesignator" />
				<px:PXTextEdit ID="edPMDesignator" runat="server" DataField="PMDesignator" />
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M"
					GroupCaption="Number Format" StartGroup="True" />
				<px:PXDropDown CommitChanges="True" ID="edNumberDecimalSeporator" runat="server"
					AllowEdit="True" DataField="NumberDecimalSeporator" />
				<px:PXDropDown CommitChanges="True" ID="edNumberGroupSeparator" runat="server" AllowEdit="True"
					DataField="NumberGroupSeparator" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnEditFormatOK" runat="server" DialogResult="Cancel" Text="Close" >
				<AutoCallBack Enabled="true" Target="formEditFormat" Command="Save" />
			</px:PXButton>
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" ActionsPosition="Top" AutoAdjustColumns="True" AllowSearch="true"
		DataSourceID="ds" SkinID="Primary" SyncPosition="True">
		<Levels>
			<px:PXGridLevel DataMember="Locales">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXSelector Size="xxs" ID="edLocaleName" runat="server" DataField="LocaleName"
						TextField="CultureReadableName">
						<GridProperties FastFilterFields="CultureReadableName">
						</GridProperties>
					</px:PXSelector>
					<px:PXNumberEdit Size="xxs" ID="edNumber" runat="server" DataField="Number" />
					<px:PXLayoutRule runat="server" Merge="False" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
					<px:PXTextEdit ID="edTranslatedName" runat="server" DataField="TranslatedName" />
					<px:PXTextEdit ID="edCultureReadableName" runat="server" DataField="CultureReadableName" /></RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="LocaleName" TextField="CultureReadableName" Width="200px" />
					<px:PXGridColumn DataField="TranslatedName" Width="150px" />
					<px:PXGridColumn DataField="Description" Multiline="True" Width="200px" />
					<px:PXGridColumn DataField="Number" TextAlign="Right" Width="100px" />
					<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
						Width="60px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar>
			<Actions>
				<EditRecord Enabled="False" />
				<NoteShow Enabled="False" />
			</Actions>
			<%--<CustomItems>
				<px:PXToolBarButton CommandName="editFormatCommand" CommandSourceID="ds" Text="Edit Locale Format" />
			</CustomItems>--%>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
