<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="EP404300.aspx.cs" Inherits="Page_EP404300" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="true" Width="100%"
		PrimaryView="Filter" TypeName="PX.Objects.EP.ActivitiesEnq" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="gridActivities" Name="Activities_ViewDetails" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="gridActivities" Name="viewEntity" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="gridActivities" Name="viewOwner" Visible="False" />
			<px:PXDSCallbackCommand Name="createNew" Visible="False"
				RepaintControls="All" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Caption="Selection"
        Width="100%">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" />
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" ControlSize="XM"/>
			<px:PXSelector AutoRefresh="True" ommitChanges="True" ID="edOwnerID" runat="server" DataField="OwnerID" />
			<px:PXCheckBox CommitChanges="True" ID="chkMyOwner" runat="server" Checked="True" DataField="MyOwner" />
			<px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="False" />
			<px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="True" ControlSize="XM"/>
			<px:PXSelector AutoRefresh="True" CommitChanges="True" ID="edWorkGroupID" runat="server" DataField="WorkGroupID" />
			<px:PXCheckBox CommitChanges="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" />
			<px:PXLayoutRule ID="PXLayoutRule5"  runat="server" Merge="False" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="server">
	<px:PXGrid ID="gridActivities" runat="server" DataSourceID="ds" Width="100%" ActionsPosition="Top"
		AllowPaging="true" AdjustPageSize="Auto" AllowSearch="true" SkinID="Inquire"
		Caption="Activities" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="Activities">
				 <RowTemplate>
					  <px:PXTimeSpan TimeMode="True" ID="edTimeSpent" runat="server" DataField="TimeSpent" InputMask="hh:mm" MaxHours="99" />
					  <px:PXTimeSpan TimeMode="True" ID="PXTimeSpan1" runat="server" DataField="OvertimeSpent" InputMask="hh:mm" MaxHours="99" />
					  <px:PXTimeSpan TimeMode="True" ID="PXTimeSpan2" runat="server" DataField="TimeBillable" InputMask="hh:mm" MaxHours="99" />
					  <px:PXTimeSpan TimeMode="True" ID="PXTimeSpan3" runat="server" DataField="OvertimeBillable" InputMask="hh:mm" MaxHours="99" />
				</RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="IsCompleteIcon" Width="20px" AllowShowHide="False" Label="Complete Icon" AllowResize="False" ForceExport="True" />
                    <px:PXGridColumn DataField="Subject" Width="300px" LinkCommand="Activities_ViewDetails" />
                    <px:PXGridColumn DataField="UIStatus" Width="90px" AllowNull="False" />
                    <px:PXGridColumn DataField="StartDate" DisplayFormat="g" Width="130px" />
                    <px:PXGridColumn DataField="EndDate" DisplayFormat="g" Width="130px" />
                    <px:PXGridColumn DataField="DayOfWeek" Width="90px" />
                    <px:PXGridColumn DataField="TimeSpent" Width="100px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="OvertimeSpent" Width="100px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="TimeBillable" Width="100px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="OvertimeBillable" Width="100px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="ProjectID" Width="90px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="ProjectTaskID" Width="90px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="GroupID" Width="150px" SyncVisible="False" SyncVisibility="False" Visible="False" />
                    <px:PXGridColumn DataField="Owner" Width="150px" LinkCommand="viewOwner" SyncVisible="False" SyncVisibility="False" Visible="False" DisplayMode="Text" />
                    <px:PXGridColumn DataField="CreatedByID_Creator_Username" Width="110px" SyncVisibility="False" SyncVisible="False" Visible="False" />
                    <px:PXGridColumn DataField="Source" Width="100px" SyncVisible="False" SyncVisibility="False" Visible="False" />
                    <px:PXGridColumn DataField="Source_Description" Width="200px" SyncVisible="False" SyncVisibility="False" Visible="True" LinkCommand="viewEntity" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" />
		<ActionBar DefaultAction="cmdviewDetail" PagerVisible="False">
			<CustomItems>
				<px:PXToolBarButton Key="cmdviewDetail" Visible="False">
					<AutoCallBack Target="ds" Command="Activities_ViewDetails" />
					<ActionBar GroupIndex="0" />
					<PopupCommand Target="gridActivities" Command="Refresh" />
					<Images Normal="main@Inquiry" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Key="cmdcreateNew" DisplayStyle="Image">
					<AutoCallBack Target="ds" Command="createNew" />
					<PopupCommand Target="gridActivities" Command="Refresh" />
					<Images Normal="main@AddNew" />
				</px:PXToolBarButton>
			</CustomItems>
			<Actions>
				<AddNew Enabled="False" />
				<Delete Enabled="False" />
				<EditRecord Enabled="False" />
			</Actions>
		</ActionBar>
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
</asp:Content>
