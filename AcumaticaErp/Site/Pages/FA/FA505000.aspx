<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA505000.aspx.cs" Inherits="Page_FA505000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        PrimaryView="Filter" TypeName="PX.Objects.FA.DisposalProcess">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Process" StartNewGroup="True"/>
		    <px:PXDSCallbackCommand Name="Schedule" StartNewGroup="True"/>
		    <px:PXDSCallbackCommand Name="ViewAsset" DependOnGrid="grid" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Caption="Options" DataMember="Filter"  >
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="XM" />

            <px:PXSegmentMask CommitChanges="True" runat="server" DataField="BranchID" ID="edBranchID"  />
            <px:PXSelector CommitChanges="True" runat="server" DataField="ClassID" ID="edClassID"  />
            <px:PXSelector CommitChanges="True" runat="server" DataField="ParentAssetID" ID="edParentAssetID"  />
            <px:PXSelector CommitChanges="True" runat="server" DataField="BookID" ID="edBookID"  />
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="XM" />

            <px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="DisposalDate" ID="edDisposalDate"  />
            <px:PXSelector ID="edDisposalPeriodID" runat="server" DataField="DisposalPeriodID" DataMember="_FinPeriod_"  />
            <px:PXDropDown CommitChanges="True" ID="edDisposalAmtMode" runat="server" AllowNull="False" DataField="DisposalAmtMode" SelectedIndex="-1"  />
            <px:PXNumberEdit runat="server" DataField="DisposalAmt" ID="edDisposalAmt"  />
            <px:PXSelector CommitChanges="True" runat="server" DataField="DisposalMethodID" ID="edDisposalMethodID"  />
            <px:PXSegmentMask CommitChanges="True" runat="server" DataField="DisposalAccountID" ID="edDisposalAccountID"  />
            <px:PXSegmentMask runat="server" DataField="DisposalSubID" ID="edDisposalSubID" AutoRefresh="True" />
            <px:PXCheckBox CommitChanges="True" ID="chkDeprBeforeDisposal" runat="server" DataField="DeprBeforeDisposal" />
            <px:PXTextEdit CommitChanges="True" ID="edReason" runat="server" DataField="Reason"  />
            </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Caption="Assets to Dispose" CaptionVisible="True"
		Width="100%" Height="150px" SkinID="Inquire" 
        AdjustPageSize="Auto" AllowPaging="True" AllowSearch="True" 
        FastFilterFields="AssetCD">
		<Levels>
			<px:PXGridLevel DataKeyNames="AssetCD" DataMember="Assets">
			    <Columns>
                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox" Width="60px" AutoCallBack="True" />
                    <px:PXGridColumn AllowUpdate="False" DataField="BranchID" Label="Branch" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ClassID" Label="Asset Class" />
                    <px:PXGridColumn AllowUpdate="False" DataField="AssetCD" Label="Asset ID" LinkCommand="ViewAsset" />
					<px:PXGridColumn DataField="Description" Label="Description" Width="200px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ParentAssetID" Label="Parent Asset" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="FADetails__CurrentCost" Label="Basis" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="DisposalAmt" Label="Disposal Amount" TextAlign="Right" Width="100px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="FADetails__ReceiptDate" Label="Receipt Date" Width="90px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="UsefulLife" Label="Useful Life, Years" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="FAAccountID" Label="Fixed Assets Account" />
                    <px:PXGridColumn AllowUpdate="False" DataField="FASubID" Label="Fixed Assets Sub." Width="120px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="FADetails__TagNbr" Label="Tag Number" Width="80px" />
                    <px:PXGridColumn DataField="Account__AccountClassID" Label="Account Class" Width="80px" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar PagerVisible="False"/>
	</px:PXGrid>
</asp:Content>
