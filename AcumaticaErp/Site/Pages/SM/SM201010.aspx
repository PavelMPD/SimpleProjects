<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	CodeFile="SM201010.aspx.cs" Inherits="Page_SM201010" Title="Untitled Page" ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<script type="text/javascript">
		function onCallbackResult(ds, context)
		{
			if (context.command == "loginAsUser")
			{
				window.top.lastUrl = null;
				ds.isDirty = false;
				for (var name in px_all)
				{
					var item = px_all[name];
					if (item && item.getChanged)
					{
						if (item.checkChanges != null) item.checkChanges = false;
					}
				}
				var mainUrl = ((location.href.indexOf("HideScript") > 0) ? "" : "../../") + "Main.aspx";
				px.openUrl(mainUrl, "_top");
			}
		}
	</script>
	<pxa:DynamicDataSource ID="ds" runat="server" Visible="True" PrimaryView="UserList" 
		TypeName="PX.SM.AccessUsers,PX.SM.AccessWebUsers" >
		<ClientEvents CommandPerformed="onCallbackResult" />
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="InsertUsers" HideText="True" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="SaveUsers" />
			<px:PXDSCallbackCommand Name="DeleteUsers" HideText="True" />
			<px:PXDSCallbackCommand Name="FirstUsers" HideText="True" />
			<px:PXDSCallbackCommand Name="PrevUsers" HideText="True" />
			<px:PXDSCallbackCommand Name="NextUsers" HideText="True" />
			<px:PXDSCallbackCommand Name="LastUsers" HideText="True" />
			<px:PXDSCallbackCommand Name="ResetPasswordOK" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="addADUserOK" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="addADUser" CommitChanges="True" />
		</CallbackCommands>
	</pxa:DynamicDataSource>

</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" 
		Width="100%" DataMember="UserList" Caption="User Information">
		<Template>
		    <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XL" LabelsWidth="SM"/>
			<px:PXSelector ID="edUsername" runat="server" DataField="Username" AutoRefresh="True" AutoComplete="False">
                <GridProperties FastFilterFields="FullName" />
			    <AutoCallBack Command="CancelUsers" Target="ds"/>
			</px:PXSelector>
			<px:PXTextEdit ID="edPassword" runat="server" DataField="Password" TextMode="Password"/>
            <px:PXCheckBox ID="edGenerate" runat="server" DataField="GeneratePassword" CommitChanges="True"/>
            <px:PXCheckBox ID="edGuest" runat="server" DataField="Guest" CommitChanges="True"/>
		    <px:PXSelector ID="edLoginType" runat="server" DataField="LoginTypeID" CommitChanges="True"  AllowEdit="True"/>
		    <px:PXSelector ID="edContactID" runat="server" DataField="ContactID" CommitChanges="True" AutoRefresh="True" DisplayMode="Text" TextMode="Search" AllowEdit="True"/>
			<px:PXTextEdit ID="edFirstName" runat="server" DataField="FirstName" />
			<px:PXTextEdit ID="edLastName" runat="server" DataField="LastName" />
			<px:PXMailEdit CommitChanges="True" ID="edEmail" runat="server" DataField="Email" />
            <px:PXTextEdit ID="edComment" runat="server" DataField="Comment"
                TextMode="MultiLine" Height="45px" />
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" />
            <px:PXDropDown ID="edState" runat="server" DataField="State" Enabled="False" />
            <px:PXCheckBox ID="chkAllowPasswordRecovery" runat="server" DataField="AllowPasswordRecovery"/>
            <px:PXCheckBox ID="chkPasswordChangeable" runat="server" DataField="PasswordChangeable" />
            <px:PXCheckBox ID="chkPasswordNeverExpires" runat="server" DataField="PasswordNeverExpires" />
            <px:PXCheckBox ID="chkPasswordChangeOnNextLogin" runat="server" DataField="PasswordChangeOnNextLogin" />
            <px:PXCheckBox ID="chkOverride" runat="server" DataField="OverrideADRoles" CommitChanges="True"/>
            <px:PXDropDown ID="edSource" runat="server" DataField="Source" Visible="False" />
            <px:PXLayoutRule runat="server" />
            <px:PXSmartPanel ID="pnlResetPassword" runat="server" Caption="Change password"
                LoadOnDemand="True" Width="400px" Key="UserList" CommandName="ResetPasswordOK" CommandSourceID="ds" AcceptButtonID="btnOk" CancelButtonID="btnCancel" AutoCallBack-Command="Refresh" AutoCallBack-Target="frmResetParams" AutoCallBack-Enabled="true">
                <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
                <px:PXTextEdit ID="edNewPassword" runat="server" DataField="NewPassword" TextMode="Password" Required="True" />
                <px:PXTextEdit ID="edConfirmPassword" runat="server" DataField="ConfirmPassword" TextMode="Password" Required="True" />
                <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
                    <px:PXButton ID="btnOk" runat="server" DialogResult="OK" Text="OK" />
                    <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
                </px:PXPanel>
            </px:PXSmartPanel>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%"  OnCalculateVisibility="tab_DataBound"
		Height="200px" RepaintOnDemand="False" DataSourceID="ds" DataMember="UserListCurrent">
		<Items>
			<px:PXTabItem Key="membership" Text="Roles" LoadOnDemand="True">
				<Template>
					<px:PXGrid ID="gridRoles" runat="server" DataSourceID="ds" Width="100%" ActionsPosition="Top" SkinID="Inquire">
						<Levels>
							<px:PXGridLevel DataMember="AllowedRoles">
								<Columns>
						            <px:PXGridColumn AllowMove="False" AllowSort="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="20px" AutoCallBack="True"/>
									<px:PXGridColumn DataField="Rolename" Width="200px" AllowUpdate ="False"/>
									<px:PXGridColumn AllowUpdate="False" DataField="Rolename_Roles_descr" Width="300px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="250" MinWidth="300" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Roles" Key="roles" LoadOnDemand="True">
				<Template>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" Heigth="100%" ActionsPosition="Top" SkinID="Details">
						<Levels>
							<px:PXGridLevel DataMember="RoleList">
								<Columns>
									<px:PXGridColumn DataField="Rolename" Width="200px" AutoCallBack="True" />
									<px:PXGridColumn AllowUpdate="False" DataField="Rolename_Roles_descr" Width="300px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="250" MinWidth="300" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Key="statistics" Text="Statistics">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XL"/>
					<px:PXDateTimeEdit ID="edCreationDate" runat="server" DataField="CreationDate" DisplayFormat="f"
						EditFormat="f" Size="SM"/>
					<px:PXDateTimeEdit ID="edLastLoginDate" runat="server" DataField="LastLoginDate"
						DisplayFormat="f" EditFormat="f" Size="SM"/>
					<px:PXDateTimeEdit ID="edLastLockedOutDate" runat="server" DataField="LastLockedOutDate"
						DisplayFormat="f" EditFormat="f" Size="SM"/>
					<px:PXDateTimeEdit ID="edLastPasswordChangedDate" runat="server" DataField="LastPasswordChangedDate"
						DisplayFormat="f" EditFormat="f" Size="SM"/>
					<px:PXNumberEdit ID="edFailedPasswordAttemptCount" runat="server" DataField="FailedPasswordAttemptCount" Size="SM"/>
					<px:PXNumberEdit ID="edFailedPasswordAnswerAttemptCount" runat="server" DataField="FailedPasswordAnswerAttemptCount" Size="SM"/>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="IP filter">
				<Template>
					<px:PXGrid ID="gridFilterIP" runat="server" DataSourceID="ds" Style="z-index: 100"
						Height="150px" Width="100%" ActionsPosition="Top" Caption="Allowed IP Address Ranges"
						BorderWidth="0px" SkinID="Details">
						<Levels>
							<px:PXGridLevel DataMember="UserFilters">
								<Columns>
									<px:PXGridColumn DataField="StartIPAddress" Width="200px" />
									<px:PXGridColumn DataField="EndIPAddress" Width="200px" />
								</Columns>
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXMaskEdit ID="edStartIPAddress" runat="server" DataField="StartIPAddress" EmptyChar="0" InputMask="###.###.###.###" />
									<px:PXMaskEdit ID="edEndIPAddress" runat="server" DataField="EndIPAddress" EmptyChar="0" InputMask="###.###.###.###" />
								</RowTemplate>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="250" MinWidth="300" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>
    <px:PXSmartPanel ID="pnlAddADUser" runat="server" Key="ADUser" AutoCallBack-Command="Refresh" AutoCallBack-Target="frmADUser" AutoCallBack-Enabled="true"
        LoadOnDemand="True" AcceptButtonID="cbOk" CancelButtonID="cbCancel" Caption="Active Directory User" CaptionVisible="True" CommandName="addADUserOK" CommandSourceID="ds" >
        <px:PXFormView ID="frmADUser" runat="server" DataSourceID="ds" Style="z-index: 108" Width="100%" DataMember="ADUser"
            Caption="Active Directory User" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" />
                <px:PXSelector CommitChanges="True" ID="edADUsername" runat="server" DataField="Username"/>
            </Template>
        </px:PXFormView>
		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">        
            <px:PXButton ID="cbOk" runat="server" Text="OK" DialogResult="OK" />
            <px:PXButton ID="cbCancel" runat="server" Text="Cancel" DialogResult="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
