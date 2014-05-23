<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL503000.aspx.cs" Inherits="Page_GL503000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="PeriodList"
		TypeName="PX.Objects.GL.Closing">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="close" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="showDocuments" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" AllowSearch="true"
		SkinID="Inquire" Caption="Financial Periods">
		<Levels>
			<px:PXGridLevel DataMember="PeriodList">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXCheckBox SuppressLabel="True" ID="chkSelected" runat="server" DataField="Selected" />
					<px:PXTextEdit ID="edFinPeriodID" runat="server" DataField="FinPeriodID" Enabled="False" />
					<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" Enabled="False" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True"
						AllowSort="False" AllowMove="False" Width="30px" />
					<px:PXGridColumn DataField="FinPeriodID" Width="100px" />
					<px:PXGridColumn DataField="Descr" Width="100px" />
					<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" Width="40px" />
					<px:PXGridColumn DataField="APClosed" TextAlign="Center" Type="CheckBox" Width="40px" />
					<px:PXGridColumn DataField="ARClosed" TextAlign="Center" Type="CheckBox" Width="40px" />
					<px:PXGridColumn DataField="INClosed" TextAlign="Center" Type="CheckBox" Width="40px" />
					<px:PXGridColumn DataField="CAClosed" TextAlign="Center" Type="CheckBox" Width="40px" />
					<px:PXGridColumn DataField="FAClosed" TextAlign="Center" Type="CheckBox" Width="40px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
	    <AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>
