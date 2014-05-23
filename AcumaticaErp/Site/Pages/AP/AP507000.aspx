<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP507000.aspx.cs" Inherits="Page_AP507000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" TypeName="PX.Objects.AP.AP1099SummaryEnq" 
        PrimaryView="Year_Header" style="float:left">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="Close1099Year" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Year1099SummaryReport" Visible="False"/>
            <px:PXDSCallbackCommand Name="Year1099DetailReport" Visible="False"/>
        </CallbackCommands>
    </px:PXDataSource>
    <px:PXToolBar ID="toolbar1" runat="server" SkinID="Navigation" BackColor="Transparent" CommandSourceID="ds">
        <Items>
            <px:PXToolBarButton Text="Reports" Tooltip="Reports">
                <MenuItems>
                    <px:PXMenuItem Text="Year 1099 Summary" CommandSourceID="ds" CommandName="Year1099SummaryReport"/>
                    <px:PXMenuItem Text="Year 1099 Detail" CommandSourceID="ds" CommandName="Year1099DetailReport"/>
                </MenuItems>
            </px:PXToolBarButton>
        </Items>
        <Layout ItemsAlign="Left" />
    </px:PXToolBar>
    <div style="clear: left" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Year_Header" Caption="Selection" DefaultControlID="edFinYear">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXSelector ID="edFinYear" runat="server" DataField="FinYear" />
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="false" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" Caption="Details" SkinID="Inquire">
		<Levels>
			<px:PXGridLevel DataMember="Year_Summary">
				<RowTemplate>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="BoxNbr" TextAlign="Right" Width="54px" />
					<px:PXGridColumn DataField="Descr" Width="360px" />
					<px:PXGridColumn DataField="AP1099History__HistAmt" TextAlign="Right" Width="81px" MatrixMode="True" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar>
			<CustomItems>
				<px:PXToolBarButton Text="Cancel" Tooltip="Undo changes." Visible="False">
					<AutoCallBack Command="Cancel" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
