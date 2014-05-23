<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SearchSP.ascx.cs" Inherits="Controls_SearchNewPP" %>

<script language="javascript" type="text/javascript">
    function txtSearch_KeyDown(sender, e) {
    	if (e.keyCode == 13) {
    		debugger;
            px.cancelEvent(e);
            px.doPost(e.srcElement.getAttribute("name"), "");
            return true;
        }
    }
</script>

<div style="background-color: White">
    <px:PXFormView ID="PXFormView1" runat="server" CaptionVisible="False" Style="position: static;"
        Width="100%" AllowFocus="False">
        <Template>
            <div style="padding: 5px; position: static; border-style: none;">
                <table style="position: static; margin-left: 5px; border-color: #ECE9E8; height: 20px; width: 600px;">
                        <tr>
                            <td>
                                <table style="position: static; border-color: #ECE9E8; height: 20px;">
                                    <tr>
                                        <td style="height: 20px; padding-right: 25px; white-space: nowrap">
                                            <px:PXLabel runat="server" ID="lblSearch" Text="Search :" Style="white-space: nowrap" Width="60px"></px:PXLabel>
                                        </td>
                                        <td style="height: 20px; white-space: nowrap">
                                            <px:PXTextEdit ID="txtSearch" runat="server" Style="position: static;"
                                                ToolTip="Search for Wiki articles." Width="450px" MaxLength="255">
                                                <ClientEvents KeyDown="txtSearch_KeyDown" />
                                            </px:PXTextEdit>
                                        </td>
                                        <td>
                                            <px:PXButton runat="server" ID="btnSearch" AutoPostBack="true" BorderStyle="Solid"
                                                Text="Go" Width="70px" Height="20px" EnableTheming="False" Style="margin-left: 10px">
                                            </px:PXButton>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>

                        <tr id="fullTextRow" runat="server">
                            <td>
                                <table style="position: static; border-color: #ECE9E8; height: 20px;">
                                    <tr>
                                        <td style="height: 20px; padding-right: 25px; white-space: nowrap">
                                            <px:PXLabel runat="server" ID="lblFilter" Text="Filter :" Style="white-space: nowrap" Width="60px"></px:PXLabel>
                                        </td>

                                        <td style="height: 20px; white-space: nowrap">
                                            <px:PXDropDown ID="SearchCaption" runat="server" Style="height: 20px; width: 140px" OnCallBack="SearchCaption_WikiChange">
                                                <AutoCallBack Command="SearchCaption_WikiChange">
	                                                <Behavior PostData="Container" />
                                                </AutoCallBack>
                                            </px:PXDropDown>
                                        </td>
                                        <td style="height: 20px; white-space: nowrap">
                                            <px:PXDropDown ID="SearchCaptionCategory" runat="server" Style="height: 20px; margin-left: 10px; width: 140px;" OnCallBack="SearchCaption_CategoryChange">
                                                <AutoCallBack Command="SearchCaption_CategoryChange" Target="ds">
	                                                <Behavior PostData="Container" />
                                                </AutoCallBack>
                                            </px:PXDropDown>
                                        </td>
                                        <td style="height: 20px; white-space: nowrap">

                                            <px:PXDropDown ID="SearchCaptionProduct" runat="server" Style="height: 20px; margin-left: 10px; width: 140px" OnCallBack="SearchCaption_ProductChange">
                                                <AutoCallBack Command="SearchCaption_ProductChange" Target="ds">
	                                                <Behavior PostData="Container" />
                                                </AutoCallBack>
                                            </px:PXDropDown>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>

            <div id="divResults" style="background-color: #d5ddf3; position: static; width: 100%;">
                <table style="position: static; margin-left: 5px; border-color: #ECE9E8; height: 20px; width: 100%;">
                    <tr style="width: auto; white-space: nowrap;">
                        <td style="height: 20px; white-space: nowrap; width: auto">
                            <asp:Label ID="lblResults" runat="server" CssClass="wiki" ForeColor="Black" Style="position: static; background-color: Transparent; width: auto"
                                Text="Results"></asp:Label>
                        </td>
                        <td style="width: 100%;">
                            <asp:Label ID="Label1" runat="server" Width="100%" />
                        </td>
                        <td style="height: 20px; white-space: nowrap; width: auto">
                            <px:PXDropDown ID="OrderCaption" runat="server" Style="height: 20px; width: 140px; margin-right: 20px" OnCallBack="OrderCaption_OrderChange">
                                <AutoCallBack Command="OrderCaption_OrderChange">
                                </AutoCallBack>
                            </px:PXDropDown>
                        </td>
                    </tr>
                </table>
            </div>
            <div id="divMessage" style="position: static;">
                <asp:Image ID="imgMessage" runat="server" Style="position: static; margin-left: 10px; margin-top: 10px; background-color: Transparent" />
                <asp:Label ID="lblMessage" runat="server" CssClass="wiki" Font-Size="Large" ForeColor="Sienna"
                    Style="position: static; padding-bottom: 10px; background-color: Transparent" Visible="False"></asp:Label>&nbsp;
            </div>
        </Template>
        <AutoSize Enabled="true" Container="Window" />
        <ContentStyle BackColor="White" BorderStyle="None" ForeColor="Black">
        </ContentStyle>
    </px:PXFormView>
</div>
