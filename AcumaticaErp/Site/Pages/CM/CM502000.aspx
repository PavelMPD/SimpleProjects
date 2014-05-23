<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CM502000.aspx.cs" Inherits="Page_CM502000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="TranslationReleaseList" TypeName="PX.Objects.CM.TranslationRelease">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Process" StartNewGroup="True" CommitChanges="true" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewTranslation" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" ActionsPosition="Top" AllowSearch="true" DataSourceID="ds" Caption="Translation Worksheets" SkinID="Inquire">
		<Levels>
			<px:PXGridLevel DataMember="TranslationReleaseList">
				<Columns>
					<px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="60px" AllowCheckAll="True" />
					<px:PXGridColumn DataField="ReferenceNbr" />
					<px:PXGridColumn DataField="Description" Width="200px" />
					<px:PXGridColumn DataField="TranslDefId" />
                    <px:PXGridColumn DataField="BranchID" />
					<px:PXGridColumn DataField="LedgerID" />
					<px:PXGridColumn DataField="DateEntered" Width="90px" />
					<px:PXGridColumn DataField="FinPeriodID" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Status" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar DefaultAction="cmdViewTranslation">
			<CustomItems>
				<px:PXToolBarButton Text="View Translation" Key="cmdViewTranslation">
				    <AutoCallBack Command="ViewTranslation" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
