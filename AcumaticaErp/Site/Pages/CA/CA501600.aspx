<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA501600.aspx.cs" Inherits="Page_CA501600" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="Items" TypeName="PX.Objects.CA.CAExternalTaxCalc">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Process" StartNewGroup="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="ProcessAll" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="true" AdjustPageSize="Auto" AllowSearch="true" DataSourceID="ds" BatchUpdate="True" SkinID="Inquire" Caption="CA Documents">
		<Levels>
			<px:PXGridLevel DataMember="Items">
				<RowTemplate>
					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXSelector SuppressLabel="True" ID="edRefNbr" runat="server" DataField="AdjRefNbr" Enabled="False" AllowEdit="True" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" Width="20px" />
					<px:PXGridColumn DataField="AdjTranType" Type="DropDownList" Width="100px" />
					<px:PXGridColumn DataField="AdjRefNbr" Width="100px" />
					<px:PXGridColumn DataField="Status" Type="DropDownList" Width="100px" />
                    <px:PXGridColumn DataField="DrCr" Type="DropDownList" Width="100px" />
					<px:PXGridColumn DataField="TranDate" Width="90px" />
                    <px:PXGridColumn DataField="FinPeriodID" Width="70px" />
					<px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="CuryID" Width="54px" />
					<px:PXGridColumn DataField="TranDesc" Width="250px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Layout ShowRowStatus="False" />
	</px:PXGrid>
</asp:Content>
