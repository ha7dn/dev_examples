<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Checkout.aspx.cs" Inherits="FormularioPago.Checkout" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="contenedorTitulo">
                <h2>Resumen de pedido</h2>
            </div>
            <div class="contenedorBtn">
                    <div class="col">
                        <asp:Button ClientIDMode="Static" ID="btncheckoutpagar" CssClass="oculto" runat="server" Text="PAGAR" OnClick="btncheckoutpagar_Click"/> 
                    </div>
                </div>
            </div>
    </form>
</body>
</html>
