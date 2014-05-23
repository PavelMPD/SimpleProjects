<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="Wiki.aspx.cs"
	Inherits="Page_WikiNew" Title="Untitled Page" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
	<title>Article Search</title>
	<link href="~/App_Themes/Wiki.css" type="text/css" rel="stylesheet" />
</head>

<body style="margin: 0px; overflow: hidden">
	<form id="form1" runat="server">
		<px_pt:PageTitle ID="usrCaption" runat="server" EnableTheming="true" />
		<px_srch:Search ID="ctrlSearch" runat="server" EnableTheming="true" />
		<px_pf:PageFooter ID="usrFooter" runat="server" EnableTheming="true" />
	</form>
</body>
</html>
