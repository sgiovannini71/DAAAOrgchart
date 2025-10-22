<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Organigramma.aspx.cs" Inherits="DipendentiWeb.Organigramma" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Organigramma - DipendentiDB</title>
    <style>
        body {
            font-family: Segoe UI, Arial, sans-serif;
            margin: 30px;
            background-color: #fafafa;
        }
        .ufficio {
            margin-left: 20px;
        }
        .titolo {
            font-weight: bold;
            color: #003366;
        }
        .responsabile {
            color: #555;
            font-style: italic;
        }
        .addetti {
    color: #333;
    margin-top: 4px;
    margin-bottom: 8px;
    font-size: 13px;
}
.addetti span.addetto {
    display: inline-block;
    margin-right: 6px;
}

        h1 {
            color: #003366;
            border-bottom: 2px solid #003366;
            padding-bottom: 10px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <h1>Organigramma</h1>
        <asp:Literal ID="litOrganigramma" runat="server" />
    </form>
</body>
</html>
