<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL501000.aspx.cs" Inherits="Page_GL501000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="BatchList" TypeName="PX.Objects.GL.BatchRelease"
		Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" />
			<px:PXDSCallbackCommand StartNewGroup="True" CommitChanges="True" Name="Process" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="ProcessAll" />
			<px:PXDSCallbackCommand Visible="False" Name="ViewBatch" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Visible="False" Name="GLEditReport"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" AllowPaging="True"
		AdjustPageSize="Auto" AllowSearch="True" SkinID="Inquire"
		Caption="Batches" DataSourceID="ds" FastFilterFields="BatchNbr,Description" 
		NoteIndicator="True">
		<Levels>
			<px:PXGridLevel DataMember="BatchList">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXCheckBox SuppressLabel="True" ID="chkSelected" runat="server" 
						DataField="Selected" AlignLeft="True" />
					<px:PXDropDown ID="edModule" runat="server" DataField="Module" Enabled="False" />
					<px:PXSelector ID="edBatchNbr" runat="server" AllowEdit="True" DataField="BatchNbr"
						Enabled="False" />
					<px:PXSelector ID="edLedgerID" runat="server" DataField="LedgerID" />
					<px:PXDateTimeEdit ID="edDateEntered" runat="server" DataField="DateEntered" />
					<px:PXSelector ID="edLastModifiedByID" runat="server" DataField="LastModifiedByID" />
					<px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
					<px:PXNumberEdit ID="edControlTotal" runat="server" DataField="ControlTotal" />
					<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
					<px:PXTextEdit ID="Description" runat="server" DataField="Description">
					</px:PXTextEdit>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="30px"
						AllowCheckAll="True" AllowSort="False" AllowMove="False" />
					<px:PXGridColumn DataField="Module" Width="70px" />
					<px:PXGridColumn DataField="BatchNbr" Width="100px" />
					<px:PXGridColumn DataField="LedgerID" Width="100px" />
					<px:PXGridColumn DataField="DateEntered" Width="100px" />
					<px:PXGridColumn DataField="LastModifiedByID_Modifier_Username" Width="150px" />
					<px:PXGridColumn DataField="FinPeriodID" Width="100px" />
					<px:PXGridColumn DataField="ControlTotal" TextAlign="Right" Width="100px" />
			
					<px:PXGridColumn DataField="Description" Width="300px">
					</px:PXGridColumn>
			
				</Columns>

<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Mode AllowAddNew="False" />
		<LevelStyles>
			<RowForm Height="300px" Width="400px">
			</RowForm>
		</LevelStyles>
		<ActionBar>
			<CustomItems>
				<px:PXToolBarButton Text="View Batch">
				    <AutoCallBack Command="ViewBatch" Target="ds" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Text="GL Edit Report" Key="cmdPrintReport" >
				    <AutoCallBack Command="GLEditReport" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<Layout ShowRowStatus="False" />
	</px:PXGrid>
</asp:Content>
