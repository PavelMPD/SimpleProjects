<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="DR202000.aspx.cs" Inherits="Page_DR202000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="deferredcode" TypeName="PX.Objects.DR.DeferredCodeMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="deferredcode" 
        Caption="Deferral Code" NoteIndicator="True"
        FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" TabIndex="-10836">
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXSelector ID="edDeferredCodeID" runat="server" DataField="DeferredCodeID" DataSourceID="ds" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXDropDown CommitChanges="True" ID="edMethod" runat="server" AllowNull="False" DataField="Method" />
            <px:PXNumberEdit ID="edReconNowPct" runat="server" DataField="ReconNowPct" />
            <px:PXNumberEdit ID="edStartOffset" runat="server" DataField="StartOffset" />
            <px:PXNumberEdit ID="edOccurrences" runat="server" DataField="Occurrences" />
            <px:PXDropDown ID="edAccountType" runat="server" AllowNull="False" DataField="AccountType" />
            <px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" 
                              DataSourceID="ds" />
            <px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" DataSourceID="ds" />
            <px:PXLayoutRule runat="server" LabelsWidth="XS" ControlSize="M" StartColumn="True" GroupCaption="Schedule Settings" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXNumberEdit Size="xxs" ID="edFrequency" runat="server" DataField="Frequency" />
            <px:PXLabel Height="15px" Size="xs" ID="lblPeriodFrequencyDH" runat="server">Period(s)</px:PXLabel>
            <px:PXLayoutRule runat="server" />
            <px:PXGroupBox CommitChanges="True" RenderStyle="Fieldset" ID="gbPeriodically" runat="server" Caption="Document Date Selection" DataField="ScheduleOption">
                <Template>
                    <px:PXRadioButton Size="xm" ID="rbStartOfPeriod" runat="server" Value="S" Text="Start of Financial Period" />
                    <px:PXRadioButton ID="rbEndOfPeriod" runat="server" Value="E" Text="End of Financial Period" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXRadioButton Size="" ID="rbFixedDay" runat="server" Value="D" Text="Fixed Day of the Period" />
                    <px:PXNumberEdit SuppressLabel="True" Size="xxs" ID="edFixedDay" runat="server" DataField="FixedDay" />
                    <px:PXLayoutRule runat="server" />
                </Template>
            </px:PXGroupBox>
        </Template>
    </px:PXFormView>
</asp:Content>
