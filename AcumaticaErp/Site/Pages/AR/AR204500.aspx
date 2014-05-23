<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AR204500.aspx.cs" Inherits="Page_AR204500"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.AR.ARFinChargesMaint" Visible="True"
		PrimaryView="ARFinChargesList">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" AllowSearch="true"
		SkinID="Primary">
		<Levels>
			<px:PXGridLevel DataMember="ARFinChargesList">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXSelector ID="edTermsID" runat="server" DataField="TermsID" />
					<px:PXSegmentMask CommitChanges="True" ID="edFinChargeAccountID" runat="server" DataField="FinChargeAccountID" />
					<px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AutoRefresh="True"/>
					<px:PXMaskEdit ID="edFinChargeID" runat="server" DataField="FinChargeID" />
					<px:PXSegmentMask ID="edFinChargeSubID" runat="server" DataField="FinChargeSubID" />
					<px:PXTextEdit ID="edFinChargeDesc" runat="server" DataField="FinChargeDesc" />
					<px:PXCheckBox ID="chkBaseCurFlag" runat="server" DataField="BaseCurFlag" />
					<px:PXCheckBox ID="chkMinFinChargeFlag" runat="server" DataField="MinFinChargeFlag" />
					<px:PXNumberEdit ID="edMinFinChargeAmount" runat="server" DataField="MinFinChargeAmount" />
					<px:PXCheckBox ID="chkPercentFlag" runat="server" DataField="PercentFlag" />
					<px:PXNumberEdit ID="edFinChargePercent" runat="server" DataField="FinChargePercent" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="FinChargeID" Width="70px" />
					<px:PXGridColumn DataField="TermsID" Width="70px" />
					<px:PXGridColumn DataField="FinChargeDesc" Width="250px" />
					<px:PXGridColumn DataField="BaseCurFlag" TextAlign="Center" Type="CheckBox" Width="70px" />
					<px:PXGridColumn DataField="MinFinChargeFlag" TextAlign="Center" Type="CheckBox"
						Width="60px" AutoCallBack="True" />
					<px:PXGridColumn DataField="MinFinChargeAmount" TextAlign="Right" Width="70px" />
					<px:PXGridColumn DataField="PercentFlag" TextAlign="Center" Type="CheckBox" AutoCallBack="True"
						Width="50px" />
					<px:PXGridColumn DataField="FinChargePercent" TextAlign="Right" />
					<px:PXGridColumn DataField="FinChargeAccountID" AutoCallBack="True"
						Width="100px" />
					<px:PXGridColumn DataField="FinChargeSubID" Width="150px" />
					<px:PXGridColumn DataField="TaxCategoryID" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>
