<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="IN503000.aspx.cs" Inherits="Page_IN503000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter"
        TypeName="PX.Objects.IN.INUpdateBasePrice">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Process" StartNewGroup="True" CommitChanges="true" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
        Width="100%" Caption="Selection" DataMember="Filter" 
        DefaultControlID="edPendingBasePriceDate" TabIndex="2700">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="M" />
            <px:PXDateTimeEdit ID="edPendingBasePriceDate" runat="server" DataField="PendingBasePriceDate">
                <AutoCallBack Command="Save" Target="form" />
            </px:PXDateTimeEdit>
            <px:PXSelector ID="edPriceClassID" runat="server" DataField="PriceClassID" 
                DataSourceID="ds">
                <AutoCallBack Command="Save" Target="form" />
            </px:PXSelector>

            <px:PXLayoutRule runat="server" StartColumn="True">
            </px:PXLayoutRule>

            <px:PXLabel ID="lblPriceManagerID" runat="server" />

            <px:PXLabel ID="lblWorkGroupID" runat="server" />

            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="M" />
            <px:PXLayoutRule runat="server" Merge="True" />

            <px:PXCheckBox CommitChanges="True" ID="chkMyUser" runat="server" Checked="True" DataField="MyUser" AlignLeft="True" />
            <px:PXSelector CommitChanges="True" ID="edPriceManagerID" runat="server" 
                DataField="PriceManagerID"  LabelID="lblPriceManagerID" DataSourceID="ds" />
            <px:PXLayoutRule runat="server" />

            <px:PXLayoutRule runat="server" Merge="True" />

            <px:PXCheckBox CommitChanges="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" AlignLeft="True" />
            <px:PXSelector CommitChanges="True" ID="edWorkGroupID" runat="server" 
                DataField="WorkGroupID" LabelID="lblWorkGroupID" DataSourceID="ds" />
            <px:PXLayoutRule runat="server" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
        Width="100%" SkinID="Inquire" Caption="Details" AdjustPageSize="Auto" AllowPaging="True" TabIndex="100">
        <Levels>
            <px:PXGridLevel DataMember="Items" >
                <RowTemplate>
					<px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
					<px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" Enabled="False" AllowEdit="true"  />
					<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" Enabled="False"  />
					<px:PXNumberEdit ID="edPendingBasePrice" runat="server" DataField="PendingBasePrice" Enabled="False"  />
					<px:PXDateTimeEdit ID="edPendingBasePriceDate" runat="server" DataField="PendingBasePriceDate" Enabled="False"  />
					<px:PXNumberEdit ID="edBasePrice" runat="server" DataField="BasePrice" Enabled="False"  />
					<px:PXDateTimeEdit ID="edBasePriceDate" runat="server" DataField="BasePriceDate" Enabled="False"  />
					<px:PXSelector ID="edPriceClassID" runat="server" DataField="PriceClassID" Enabled="False"  />
					<px:PXSelector ID="edBaseUnit" runat="server" DataField="BaseUnit" Enabled="False"  />
					<px:PXSelector ID="edSalesUnit" runat="server" DataField="SalesUnit" Enabled="False"  />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn AllowCheckAll="true" DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn AllowUpdate="False" DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAAAA" Label="Inventory ID" Width="90px" />
					<px:PXGridColumn AllowUpdate="False" DataField="Descr" Label="Description" Width="500px" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="PendingBasePrice" Label="Pending Price" TextAlign="Right" Width="99px" />
					<px:PXGridColumn AllowUpdate="False" DataField="PendingBasePriceDate" Label="Pending Price Date" Width="90px" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="BasePrice" Label="Current Price" TextAlign="Right" Width="99px" />
					<px:PXGridColumn AllowUpdate="False" DataField="BasePriceDate" Label="Effective Date" Width="90px" />
					<px:PXGridColumn AllowUpdate="False" DataField="PriceClassID" DisplayFormat="&gt;aaaaaaaaaa" Label="Price Class" Width="81px" />
				</Columns>
                <Layout FormViewHeight=""></Layout>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar PagerVisible="False" />
    </px:PXGrid>
</asp:Content>
