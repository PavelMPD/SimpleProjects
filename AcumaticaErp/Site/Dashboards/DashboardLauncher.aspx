<%@ Page Language="C#" MasterPageFile="~/MasterPages/Workspace.master" AutoEventWireup="true"
  CodeFile="DashboardLauncher.aspx.cs" Inherits="Pages_DashboardLauncher" Title="ProjectX" %>


<%@ MasterType VirtualPath="~/MasterPages/Workspace.master" %>
<asp:Content ID="c1" ContentPlaceHolderID="phDS" runat="Server">
  <pxa:PXDashboardContainer ID="dashSet" runat="server" EnableTheming="true" >
	<AutoSize Enabled="true" />
  </pxa:PXDashboardContainer>
  <pxa:PXDashboardFlyContainer ID="flowDashSet" runat="server" EnableTheming="true" AlwaysFlow="true" 
	LayoutGraphName="PX.Web.Objects.Dashboards.DashLayoutMaint" >
		<AutoSize Enabled="true" Container="Window" />
  </pxa:PXDashboardFlyContainer>
</asp:Content>
