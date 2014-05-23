<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP302000.aspx.cs"
    Inherits="Page_EP302000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Document" TypeName="PX.Objects.EP.TimeSheetEntry">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" ClosePopup="true" PopupVisible="True" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="Approve" Visible="false" />
            <px:PXDSCallbackCommand Name="Reject" Visible="false" />
            <px:PXDSCallbackCommand Name="Assign" Visible="false" />
            <px:PXDSCallbackCommand Name="Release" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewActivity" Visible="False" CommitChanges="True" DependOnGrid="gridActivities" />
            <px:PXDSCallbackCommand Name="CreateActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="AddTasks" Visible="false" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Document" Caption="Document Summary"
        NoteIndicator="True" FilesIndicator="True" LinkIndicator="True" NotifyIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity"
        DefaultControlID="edTimeSheetCD" TabIndex="13100">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector ID="edTimeSheetCD" runat="server" DataField="TimeSheetCD" ValueField="TimeSheetCD" AutoRefresh="True" DataSourceID="ds" />
            <px:PXSelector CommitChanges="True" ID="edEmployeeID" runat="server" DataField="EmployeeID" TextField="AcctCD" ValueField="AcctCD"
                TextMode="Search" NullText="<SELECT>"/>
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" AllowNull="False" />
            <px:PXDateTimeEdit ID="edDocDate" runat="server" DataField="DocDate" />
            <px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
            <px:PXMaskEdit Enabled="False" ID="edTimeSpent" runat="server" DataField="TimeSpent" />
            <px:PXMaskEdit Enabled="False" ID="edOvertimeSpent" runat="server" DataField="OvertimeSpent" />
            <px:PXMaskEdit ID="edTimeBillable" runat="server" Enabled="False" DataField="TimeBillable" />
            <px:PXMaskEdit ID="edOvertimeBillable" runat="server" Enabled="False" DataField="OvertimeBillable" />
        </Template>
    </px:PXFormView>
    <px:PXSmartPanel ID="PanelTasks" runat="server" AcceptButtonID="PXButtonOK" AutoReload="true" CancelButtonID="PXButtonCancel"
        Caption="Add Tasks" CaptionVisible="True" DesignView="Content" HideAfterAction="false" Key="Tasks" LoadOnDemand="true"
        Style="z-index: 108; left: 351px; position: absolute; top: 117px; width: 812px; height: 400px;" AutoCallBack-Command="Refresh"
        AutoCallBack-Enabled="True" AutoCallBack-Target="gridTasks">
        <px:PXGrid ID="gridTasks" runat="server" DataSourceID="ds" Height="240px" Width="100%" Style="z-index: 100;" ActionsPosition="Top"
            SkinID="Inquire" AdjustPageSize="Auto" AllowPaging="True" AutoAdjustColumns="True">
            <ActionBar PagerVisible="False" />
            <Levels>
                <px:PXGridLevel DataMember="Tasks">
                    <Columns>
                        <px:PXGridColumn AllowNull="False" AllowCheckAll="true" AutoCallBack="true" DataField="Selected" Label="Selected" TextAlign="Center"
                            Type="CheckBox" Width="40px" />
                        <px:PXGridColumn DataField="TaskID" Label="ID" TextAlign="Right" />
                        <px:PXGridColumn DataField="ExtID" Label="External Ref ID" Width="60px" />
                        <px:PXGridColumn AllowNull="False" DataField="Type" Label="Type" RenderEditorText="True" />
                        <px:PXGridColumn DataField="Subject" Label="Subject" Width="200px" />
                        <px:PXGridColumn DataField="ProjectID" Width="63px" />
                        <px:PXGridColumn DataField="ProjectTaskID" Width="63px" />
                        <px:PXGridColumn AllowNull="False" DataField="Priority" Label="Priority" RenderEditorText="True" />
                        <px:PXGridColumn DataField="UIStatus" Label="Status" RenderEditorText="True" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" />
        </px:PXGrid>
        <px:PXPanel ID="PXPanelBtn" runat="server" SkinID="Buttons">>
            <px:PXButton ID="PXButtonOK" runat="server" DialogResult="OK" Text="OK" />
            <px:PXButton ID="PXButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="189px">
        <Items>
            <px:PXTabItem Text="Details">
                <Template>
                    <px:PXGrid ID="gridActivities" runat="server" ActionsPosition="Top" DataSourceID="ds" Style="z-index: 100; left: 0px; top: 0px"
                        Width="100%" BorderWidth="0px" SkinID="DetailsInTab" SyncPosition="true">
                        <Levels>
                            <px:PXGridLevel DataMember="Activities">
                                <RowTemplate>            
                                    <px:PXSegmentMask ID="edProjectTaskID" runat="server" DataField="ProjectTaskID" AutoRefresh="True"/>
                                    <px:PXSegmentMask SuppressLabel="True" ID="edProjectID" runat="server" DataField="ProjectID" AutoRefresh="True"/>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Subject" Width="230px" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="CustomerID" Width="100px" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="LocationID" Width="100px" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="ProjectID" Width="100px" AutoCallBack="true"  />                                                                      
                                    <px:PXGridColumn DataField="ProjectTaskID" Width="100px" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="StartDate_Date" DisplayFormat="g" Width="100px" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="StartDate_Time" TimeMode="true" Width="100px" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="EndDate" DisplayFormat="g" Width="110px" />
                                    <px:PXGridColumn DataField="TimeSpent" TimeMode="true" Width="90px" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="OvertimeSpent" TimeMode="true" Width="90px" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="TimeBillable" TimeMode="true" Width="90px" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="OvertimeBillable" TimeMode="true" Width="90px" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="UIStatus" Width="60px" />
                                    <px:PXGridColumn DataField="LabourItemID" Width="90px" />
                                    <px:PXGridColumn DataField="OvertimeItemID" Width="90px" />
                                </Columns>                               
                            </px:PXGridLevel>
                        </Levels>
                        <Mode InitNewRow="true" />
                        <AutoSize Enabled="True" />
                        <ActionBar DefaultAction="cmdViewActivity">
                            <CustomItems>
                                <px:PXToolBarButton Text="Add Tasks" Tooltip="Add existing tasks" Key="cmdAddTasks" Visible="false">
                                    <AutoCallBack Command="addTasks" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <CallbackCommands>
                            <FetchRow RepaintControlsIDs="form" />
                        </CallbackCommands>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Approval">
                <Template>
                    <px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" NoteIndicator="True">
                        <AutoSize Enabled="true" />
                        <Mode AllowAddNew="false" AllowDelete="false" AllowUpdate="false" />
                        <ActionBar>
                            <Actions>
                                <AddNew Enabled="false" />
                                <EditRecord Enabled="false" />
                                <Delete Enabled="false" />
                            </Actions>
                        </ActionBar>
                        <Levels>
                              <px:PXGridLevel DataMember="Approval" DataKeyNames="ApprovalID,AssignmentMapID">
                                <Columns>
                                    <px:PXGridColumn DataField="WorkgroupID" Width="80px" />
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctCD" Width="160px" />
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctName" Width="160px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="ApprovedByEmployee__AcctCD" Width="100px" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctName" Width="160px" />
                                    <px:PXGridColumn DataField="ApproveDate" Width="90px" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Status" RenderEditorText="True" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
</asp:Content>
