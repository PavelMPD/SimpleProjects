<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP307000.aspx.cs" Inherits="Page_EP307000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.EP.EmployeeActivitiesEntry">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="View" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="Filter" TabIndex="100">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" />
            <px:PXSelector runat="server" DataField="OwnerID" DataSourceID="ds" ID="OwnerID" CommitChanges="True"/>
            <px:PXDateTimeEdit runat="server" DataField="FromDate" ID="FromDate" CommitChanges="True"/>
            <px:PXDateTimeEdit runat="server" DataField="TillDate" ID="TillDate" CommitChanges="True"/>
            <px:PXLayoutRule runat="server" StartColumn="True"/>
            <px:PXSelector runat="server" DataField="ProjectID" DataSourceID="ds" ID="ProjectID" CommitChanges="True"/>
            <px:PXSegmentMask runat="server" DataField="ProjectTaskID" DataSourceID="ds" ID="ProjectTaskID" CommitChanges="True" AutoRefresh="True"/>
            <px:PXCheckBox runat="server" DataField="IncludeReject" DataSourceID="ds" ID="IncludeReject" CommitChanges="True"/>
            <px:PXLayoutRule runat="server" GroupCaption="Regular" StartColumn="True" StartGroup="True" />
            <px:PXTimeSpan runat="server" DataField="RegularTime" ID="RegularTime" SummaryMode="true" Enabled="false" Size="XS" LabelWidth="55px"  InputMask="hh:mm" MaxHours="99"/>
            <px:PXTimeSpan ID="BillableTime" runat="server" DataField="BillableTime" SummaryMode="true" Enabled="false" Size="XS" LabelWidth="55px"  InputMask="hh:mm" MaxHours="99"/>
            <px:PXLayoutRule runat="server" GroupCaption="Overtime" StartColumn="True" StartGroup="True"/>
            <px:PXTimeSpan runat="server" DataField="RegularOvertime" ID="RegularOvertime" SummaryMode="true" Enabled="false" SuppressLabel="True" Size="XS"  InputMask="hh:mm" MaxHours="99"/>
            <px:PXTimeSpan ID="BillableOvertime" runat="server" DataField="BillableOvertime" SummaryMode="true" Enabled="false" SuppressLabel="True" Size="XS"  InputMask="hh:mm" MaxHours="99"/>
            <px:PXLayoutRule runat="server" GroupCaption="Total" StartColumn="True" StartGroup="True"/>
            <px:PXTimeSpan runat="server" DataField="RegularTotal" ID="RegularTotal" SummaryMode="true" SuppressLabel="True" Enabled="false" Size="XS"  InputMask="hh:mm" MaxHours="99"/>
            <px:PXTimeSpan runat="server" DataField="BillableTotal" ID="BillableTotal" SummaryMode="true" SuppressLabel="True" Enabled="false" Size="XS" InputMask="hh:mm" MaxHours="99"/>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Height="150px" SkinID="Details" TabIndex="700" FilesIndicator="False" NoteIndicator="True" SyncPosition="True"  AdjustPageSize="Auto" AllowPaging="True" >
		<Levels>
			<px:PXGridLevel DataKeyNames="TaskID" DataMember="Activity">
			    <RowTemplate>
					<px:PXDateTimeEdit runat="server" ID="StartDate_Date" DataField="StartDate_Date" CommitChanges="True" />
					<px:PXDateTimeEdit TimeMode="True" ID="PXDateTimeEdit1" runat="server" DataField="StartDate_Time" />
					<px:PXTimeSpan TimeMode="True" ID="edTimeSpent" runat="server" DataField="TimeSpent" InputMask="hh:mm" MaxHours="99" />
					<px:PXTimeSpan TimeMode="True" ID="edTimeBillable" runat="server" DataField="TimeBillable" InputMask="hh:mm" MaxHours="99" />
					<px:PXDateTimeEdit TimeMode="True" ID="edOvertimeBillable" runat="server" DataField="OvertimeBillable" />
                    <px:PXSegmentMask runat="server" DataField="ProjectTaskID" DataSourceID="ds" ID="ProjectTaskID" CommitChanges="True" AutoRefresh="True"/>
                </RowTemplate>
			    
			    <Columns>
                    <px:PXGridColumn DataField="Hold" TextAlign="Center" Type="CheckBox" Width="40px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="ApprovalStatus"  Width="110px"/>
                    <px:PXGridColumn DataField="StartDate_Date" Width="70px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="StartDate_Time" Width="70px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="EarningTypeID" Width="80px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="ParentTaskID" AutoCallBack="True" />
                    <px:PXGridColumn DataField="ProjectID" AutoCallBack="True" Width="120px" />
                    <px:PXGridColumn DataField="ProjectTaskID" AutoCallBack="True"  Width="90px"/>
                    <px:PXGridColumn DataField="TimeSpent" Width="60px" AutoCallBack="True" RenderEditorText="True"/>
                    <px:PXGridColumn DataField="IsBillable" TextAlign="Center" Type="CheckBox"  AutoCallBack="True" Width="70px"/>
                    <px:PXGridColumn DataField="TimeBillable" Width="70px" RenderEditorText="True"/>
                    <px:PXGridColumn DataField="Subject" Width="120px"  AutoCallBack="True"/>
                    <px:PXGridColumn DataField="ApproverID" Width="100px" />
                    <px:PXGridColumn DataField="TimeCardCD" Width="100px" />
					<px:PXGridColumn DataField="CRCase__CaseCD" Width="90px" LinkCommand="ViewCase" />
					<px:PXGridColumn DataField="Contract__ContractCD" Width="90px" LinkCommand="ViewContract" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	    <ActionBar>
            <CustomItems>
                <px:PXToolBarButton>
                    <AutoCallBack Command="View" Target="ds"/>
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>

	    <Mode InitNewRow="True" AutoInsert="False" />
	</px:PXGrid>
</asp:Content>
