<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Login.master" ClientIDMode="Static" AutoEventWireup="true" 
	CodeFile="Login.aspx.cs" Inherits="Frames_Login" EnableEventValidation="false" ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/Login.master" %>

<asp:Content ID="Content2" ContentPlaceHolderID="phLogo" runat="Server">
	<asp:DropDownList runat="server" ID="cmbLang" CssClass="login_lang" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="phUser" Runat="Server">
	<asp:Label runat="server" Visible="False" ID="lblUnderMaintenance" CssClass="login_error"></asp:Label>
	<asp:Label runat="server" Visible="False" ID="lblUnderMaintenanceReason" CssClass="login_error"></asp:Label>

	<asp:TextBox runat="server" ID="txtUser" CssClass="login_user border-box" placeholder="My Username" />
	<asp:TextBox runat="server" ID="txtPass" Width="100%" CssClass="login_pass border-box" 
		TextMode="Password" placeholder="My Password" />
				
	<asp:TextBox runat="server" ID="txtDummyPass" CssClass="login_pass dummy border-box" ReadOnly="true" Visible="false" />
	<input runat="server" id="txtVeryDummyPass" type="hidden" />

	<asp:DropDownList runat="server" ID="cmbCompany" CssClass="login_company border-box" AutoPostBack="true" />
	<input runat="server" id="txtDummyCpny" type="hidden" />

	<asp:TextBox runat="server" ID="txtNewPassword" TextMode="Password" CssClass="login_pass border-box" 
		placeholder="New Password" Visible="False" />
	<asp:TextBox runat="server" ID="txtConfirmPassword" TextMode="Password" CssClass="login_pass border-box" 
		placeholder="Confirm Password" Visible="False" />

	<asp:TextBox runat="server" ID="txtRecoveryQuestion" CssClass="login_user border-box"  
		placeholder="Recovery Question" Visible="False" />
	<asp:TextBox runat="server" ID="txtRecoveryAnswer" CssClass="login_user border-box"
			placeholder="Your Answer" Visible="False" />

	<asp:Button runat="server" ID="btnLogin" Text="Login" OnClick="btnLogin_Click" CssClass="login_button" />	
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="phInfo" runat="Server">
	<div runat="server" id="login_info" style="display:none;">
		<div id="logOutReasone" runat="server" style="display:none;">
			<div runat="server" id="logOutReasoneMsg" class="login_error">Last update was completed unsuccessfull.</div>
		</div>
		<div id="updateError" runat="server" style="display:none;">
			<div class="login_error">Last update was completed unsuccessfull.</div>
			<div class="label">Contact server administrator.</div>
		</div>
		<div id="customizationError" runat="server" style="display:none;">
			<div class="login_error">Warning: customization failed to apply automatically after the upgrade.</div>
			<div class="label">
				Some functionality may be unavailable.<br /> Contact server administrator.<br />
				Click <a href="#" onclick="document.getElementById('custErrorDetails').style.display='';">
				here</a> to view details about this error.
			</div>
			<div style="display:none" id="custErrorDetails">
				<pre runat="server" id="custErrorContent"></pre>
			</div>
		</div>
	</div>
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="phLinks" runat="Server">
	<input runat="server" id="txtDummyInstallationID" type="hidden" />
	<asp:HyperLink ID="lnkForgotPswd" runat="server" CssClass="login_link" NavigateUrl="~/PasswordRemind.aspx" 
		Text="Forgot Your Credentials?" />
</asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="phStart" runat="Server">
	<script type='text/javascript'>
		window.onload = function ()
		{
			var editor = document.form1['txtUser'];
			if (editor == null || editor.readOnly) editor = document.form1['txtNewPassword'];
			if (editor && !editor.readOnly) editor.focus();
		}
	</script>
</asp:Content>

