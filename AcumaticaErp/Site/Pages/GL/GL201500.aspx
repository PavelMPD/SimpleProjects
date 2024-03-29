<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="GL201500.aspx.cs" Inherits="Page_GL201500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:pxdatasource id="ds" width="100%" runat="server" typename="PX.Objects.GL.GeneralLedgerMaint" primaryview="LedgerRecords" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
		</CallbackCommands>
	</px:pxdatasource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:pxgrid id="grid" runat="server" height="400px" width="100%" allowpaging="True" adjustpagesize="Auto" allowsearch="True" skinid="Primary">
		<Levels>
			<px:PXGridLevel DataMember="LedgerRecords">
				<Mode InitNewRow="True" />
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXMaskEdit ID="edLedgerCD" runat="server" DataField="LedgerCD" />
					<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
					<px:PXDropDown ID="edBalanceType" runat="server" DataField="BalanceType" />
					<px:PXSelector ID="edBaseCuryID" runat="server" DataField="BaseCuryID" />
					<px:PXCheckBox ID="chkConsolAllowed" runat="server" DataField="ConsolAllowed" />
					<px:PXCheckBox ID="chkPostInterCompany" runat="server" DataField="PostInterCompany" />
					<px:PXSelector ID="edDefBranchID" runat="server" DataField="DefBranchID" AllowAddNew="true" />
					<px:PXNumberEdit ID="edLedgerID" runat="server" DataField="LedgerID" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="LedgerCD" Width="100px" AutoCallBack="True"/>
					<px:PXGridColumn  DataField="Descr" Width="300px" />
					<px:PXGridColumn  DataField="BalanceType" RenderEditorText="True" Width="106px" AutoCallBack="True"/>
					<px:PXGridColumn DataField="BaseCuryID" Width="107px" AutoCallBack="True"/>
					<px:PXGridColumn DataField="LedgerID" TextAlign="Right" Visible="False" AllowShowHide="False" />
					<px:PXGridColumn DataField="DefBranchID" Width="100px" CommitChanges="true"/>
					<px:PXGridColumn  DataField="PostInterCompany" TextAlign="Center" Type="CheckBox" Width="80px" CommitChanges="true" />
					<px:PXGridColumn  DataField="ConsolAllowed" TextAlign="Center" Type="CheckBox" Width="80px"/>
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Mode AllowFormEdit="True" AllowUpload="True" />
		<LevelStyles>
			<RowForm Height="150px" Width="270px">
			</RowForm>
		</LevelStyles>
		<ActionBar>
			<Actions>
				<NoteShow Enabled="False" />
			</Actions>
		</ActionBar>
	</px:pxgrid>
</asp:Content>
