
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Organigramma.aspx.cs" Inherits="DAAAOrgchart.Organigramma" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Organigramma</title>
    <style>
        ul.tree, ul.tree ul {
            list-style-type: none;
            margin: 0;
            padding-left: 1em;
            line-height: 1.5em;
        }
        ul.tree ul {
            border-left: 1px solid #ccc;
            margin-left: 1em;
        }
        li {
            margin: 0.3em 0;
        }
        .person {
            margin-left: 1.5em;
            font-size: 0.9em;
            color: #555;
        }
        strong { color: #2a4; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <h2>📊 Organigramma Strutturale</h2>
        <asp:Literal ID="OrganigrammaLiteral" runat="server" />
    </form>
</body>
</html>
