<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="GenreAlbumTracks.aspx.cs" Inherits="SamplePages_GenreAlbumTracks" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" Runat="Server">
    <h1> Genre Album Tracks</h1>
    <!-- inside a repeater you need a minimum of a Itemplate
         other templates include HeaderTemplate, FooterTemplate
            AlternatingItemTempate, SeparatorTemplate
        
        outer repeater will display the first fields from the DTO Class 
           which do not repeater
        outer repeater get its data from an ODS
        
        nested repeater will display the collection of the DTO file
        nested repeater will get is datasource from the collection of the
            DTO class (either a POCO or another DTO
        
        This pattern repeats for all levels of your data set 
        -->
    <asp:Repeater ID="GenreAlbumTrackList" runat="server" 
        DataSourceID="GenreAlbumTrackListODS" 
         ItemType="Chinook.Data.DTOs.GenreDTO">
        <ItemTemplate>
            <h2>Genre: <%# Eval("genre") %></h2>
            <asp:Repeater ID="GenreAlbums" runat="server"
                DataSource='<%# Eval("albums") %>' 
                 ItemType="Chinook.Data.DTOs.AlbumDTO">
                    <ItemTemplate>
                        <h4>Album: 
                            <%# string.Format("{0} ({1}) Tracks: {2}",
                                Eval("title"), Eval("releaseyear"),
                                Eval("numberoftracks")) %></h4><br/>
                        <asp:Repeater ID="AlbumTracks" runat="server"
                             DataSource="<%# Item.tracks %>" 
                             ItemType="Chinook.Data.POCOs.TrackPOCO">
                            <HeaderTemplate>
                                <table>
                                    <tr>
                                        <th>Song</th>
                                        <th>Length</th>
                                    </tr>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td style="width:600px">
                                        <%# Item.song %>
                                    </td>
                                    <td>
                                        <%# Item.length %>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </ItemTemplate>
            <SeparatorTemplate>
                <hr style="height:3px;border:none;color:#000;background-color:#000;" />
            </SeparatorTemplate>
            </asp:Repeater>
        </ItemTemplate>
    </asp:Repeater>
    <asp:ObjectDataSource ID="GenreAlbumTrackListODS" runat="server"
        OldValuesParameterFormatString="original_{0}" 
        SelectMethod="Genre_GenreAlbumTracks" 
        TypeName="ChinookSystem.BLL.GenreController">
    </asp:ObjectDataSource>
</asp:Content>

