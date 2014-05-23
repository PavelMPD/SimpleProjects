<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM204505.aspx.cs" Inherits="Page_SM204505"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Visible="True" Width="100%" runat="server" PrimaryView="Projects" TypeName="PX.SM.ProjectList">
		<CallbackCommands>
			<%--<px:PXDSCallbackCommand Name="Insert" Visible="False" />--%>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<%--<px:PXDSCallbackCommand Name="Delete" Visible="False" />--%>
			<%--<px:PXDSCallbackCommand Name="First" StartNewGroup="True" PostData="Self" />--%>
			<px:PXDSCallbackCommand Name="view" CommitChanges="True" DependOnGrid="grid" />
			<%--<px:PXDSCallbackCommand Name="import" CommitChanges="true" PopupPanel="UploadPackageDlg" />--%>
			<px:PXDSCallbackCommand Text="Open from File..." PopupPanel="UploadPackagePanel" Key="OpenFromFile" />
			<px:PXDSCallbackCommand Name="actionPublish" CommitChanges="true" RepaintControls="All"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
<%--	<script language="javascript" type="text/javascript">
		function gridInitialized()
		{
			// pnlImportID variable is registered by server.
			px_all[pnlImportID].events.removeEventHandler("hideAfterUpload", refreshGrid);
			px_all[pnlImportID].events.addEventHandler("hideAfterUpload", refreshGrid);
		}

		function refreshGrid()
		{
			// dsID, pnlImportID, gridID variables are registered by server.
			var ds = px_all[dsID];
			var grid = px_all[gridID];

			grid.checkChanges = false;
			if (ds != null) ds.executeCallback("Cancel");
			px_all[pnlImportID].hide();
			grid.refresh();
		}

	</script>
	<px:PXUploadDialog ID="pnlImport" runat="server" DesignView="Content" Height="120px"
		Style="position: static" Width="560px" Caption="Import Customization" AutoSaveFile="false"
		Key="ImportDialog" AllowedFileTypes=".xml" />
	<px:PXSmartPanel ID="pnlMakeCopy" runat="server" Style="left: 0px; position: relative;
		top: 0px; height: 102px;" Width="379px" Caption="Enter Imported Project Name"
		CaptionVisible="True" Key="MakeCopyDialog" DesignView="Content">
		<px:PXFormView ID="formMakeCopy" runat="server" Height="102px" Width="379px" Style="z-index: 100"
			SkinID="Transparent" DataMember="MakeCopyDialog" CheckChanges="False">
			<Template>
				<px:PXLabel ID="lblName" runat="server" Style="z-index: 100; position: absolute;
					left: 9px; top: 9px;">Project Name:</px:PXLabel>
				<px:PXTextEdit ID="edName" runat="server" DataField="Name" LabelID="lblName" Style="z-index: 101;
					position: absolute; left: 126px; top: 9px; width: 236px;" Width="108px" />
				<px:PXButton ID="btnCancelSaveAs" runat="server" DialogResult="Cancel" Text="Cancel"
					Style="left: 282px; position: relative; top: 71px" Width="79px" Height="20px">
				</px:PXButton>
				<px:PXButton ID="btnSave" runat="server" DialogResult="OK" Text="OK" Width="79px"
					Height="20px" Style="left: 195px; position: absolute; top: 71px">
				</px:PXButton>
			</Template>
		</px:PXFormView>
	</px:PXSmartPanel>--%>
	<px:PXGrid ID="grid" runat="server" 
		Height="400px" 
		Width="100%" 
		AllowPaging="True" 
		ActionsPosition="Top" 
		AutoAdjustColumns="True"
		AllowSearch="true" 
		SkinID="Primary" 
		PageSize="50">
<%--		<ClientEvents Initialize="gridInitialized" />--%>
<%--		<ActionBar>
			<Actions>
				<NoteShow ToolBarVisible="False" MenuVisible="false" />
			</Actions>
			<CustomItems>
				<px:PXToolBarButton CommandName="Save" CommandSourceID="ds" Text="Save"/>
				<px:PXToolBarButton CommandName="view" CommandSourceID="ds" Text="View" />
			    <px:PXToolBarButton Key="ImportFromXML" NavigateUrl="~/Controls/CustomizationExport.aspx?action=import"
					Text="Import from XML" />
			    <px:PXToolBarButton CommandName="actionPublish" CommandSourceID="ds" Text="Publish" />
			    <px:PXToolBarButton Key="ImportFromFile" Text="Open From File..." PopupPanel="UploadPackagePanel" />
			</CustomItems>
		</ActionBar>--%>
		<Levels>
			<px:PXGridLevel DataMember="Projects">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXTextEdit ID="edName" runat="server" DataField="Name" />
					<px:PXCheckBox ID="chkIsWorking" runat="server" DataField="IsWorking" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
					<px:PXSelector ID="edCreatedByID" runat="server" DataField="CreatedByID" Enabled="False"
						TextField="Username" />
					<px:PXDateTimeEdit ID="edCreatedDateTime" runat="server" DataField="CreatedDateTime"
						DisplayFormat="g" Enabled="False" />
					<px:PXSelector ID="edLastModifiedByID" runat="server" DataField="LastModifiedByID"
						Enabled="False" TextField="Username" />
					<px:PXDateTimeEdit ID="edLastModifiedDateTime" runat="server" DataField="LastModifiedDateTime"
						DisplayFormat="g" Enabled="False" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="IsWorking" Width="30px" Type="CheckBox" TextAlign="Center"  AutoCallBack="True" />
					<px:PXGridColumn DataField="IsPublished" Width="30px" Type="CheckBox" TextAlign="Center" />
					<px:PXGridColumn DataField="Name" Width="108px" />
					<px:PXGridColumn DataField="Level" Width="60px"/>
					<px:PXGridColumn DataField="ScreenNames"/>
					<px:PXGridColumn DataField="Description" Width="108px" />
					<px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username"
						Width="108px" />
					<px:PXGridColumn AllowUpdate="False" DataField="CreatedDateTime" Width="90px" />
					<px:PXGridColumn AllowUpdate="False" DataField="LastModifiedByID_Modifier_Username"
						Width="108px" />
					<px:PXGridColumn AllowUpdate="False" DataField="LastModifiedDateTime" Width="90px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Mode AllowAddNew="False" />
	</px:PXGrid>
	
		<px:PXUploadFilePanel ID="UploadPackageDlg" runat="server" 
		CommandSourceID="ds"
		AllowedTypes=".zip" Caption="Open Package" PanelID="UploadPackagePanel" 
		OnUpload="uploadPanel_Upload"
		 CommandName="ReloadPage" />
</asp:Content>
