using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using System.Text;
using System.Threading.Tasks;
using RwOrders.Models;

namespace RwOrders
{
    public static class RubyConfig
    {
        public static async Task EmailConfirmation(OrderViewModel order)
        {
            try
            {
                if (!order.EmailConfirmation || string.IsNullOrEmpty(order.Email))
                    return;

                StringBuilder html = new StringBuilder($@"<div style={"\""}font-family: Arial, Helvetica, sans-serif; font-size: small;{"\""}>
    <p>{order.CustomerName}</p>
    <p>Thank you for your order. Please see below details of your purchase. This is just a confirmation of your order, you will be sent an invoice and payment details in due course.</p>
    
    <table style={"\""}font-family: Arial, Helvetica, sans-serif; font-size: small;{"\""}>
        <tr>
            <th style={"\""}padding: 8px; border-bottom: 1px solid #ddd;{"\""}>Stock Code</th>
            <th style={"\""}padding: 8px; border-bottom: 1px solid #ddd;{"\""}>Description</th>
			<th style={"\""}padding: 8px; border-bottom: 1px solid #ddd; text-align:center{"\""}>Quantity</th>
			<th style={"\""}padding: 8px; border-bottom: 1px solid #ddd; text-align:right{"\""}>Unit Price</th>
			<th style={"\""}padding: 8px; border-bottom: 1px solid #ddd; text-align:right{"\""}>Total</th>
		</tr>
        ");

                foreach (var item in order.OrderItems.Where(o => o.Quantity > 0))
                {
                    html.Append($@"<tr>
            <td style={"\""}padding: 8px; border-bottom: 1px solid #ddd;{"\""}>{item.StockCode}</td>
            <td style={"\""}padding: 8px; border-bottom: 1px solid #ddd;{"\""}>{item.Description}</td>
            <td style={"\""}padding: 8px; border-bottom: 1px solid #ddd; text-align:center{"\""}>{item.Quantity}</td>
            <td style={"\""}padding: 8px; border-bottom: 1px solid #ddd; text-align:right{"\""}>{item.UnitPrice.ToString("£#,##0.00")}</td>
            <td style={"\""}padding: 8px; border-bottom: 1px solid #ddd; text-align:right{"\""}>{(item.Quantity * item.UnitPrice).ToString("£#,##0.00")}</td>
        </tr>
        ");
                }

                decimal totalItems = order.OrderItems.Sum(o => o.Quantity * o.UnitPrice);
                html.Append($@"<tr>
            <td style={"\""}padding: 8px;{"\""}></td>
            <td style={"\""}padding: 8px;{"\""}></td>
            <td style={"\""}padding: 8px;{"\""}></td>
            <td style={"\""}padding: 8px;{"\""}></td>
            <td style={"\""}padding: 8px; text-align:right{"\""}><b>{totalItems.ToString("£#,##0.00")}</b></td>
        </tr>
    </table>
    
    <p>Payment Method &gt;&gt; {order.PaymentMethod}");

                if (order.Vouchers > 0)
                    html.Append($@" and {order.Vouchers.ToString("£#,##0.00")} of vouchers used.");

                html.Append(@"</p>
    ");

                if (order.PaymentMethod == PaymentMethod.Account)
                {
                    html.Append($@"<p>
        Please make a payment of {(totalItems - order.Vouchers).ToString("£#,##0.00")} online using your name as a reference:<br />
        Bank Account Number: 12345678<br />
        Sort Code: 12-34-56
    </p>
    <p>You should receive an invoice from Widgets Co giving details of the payment to be made. Please ensure it matches the amount of this confirmation.</p>
    ");
                }

                html.Append($@"<p>Kind regards</p>
    <p>Widgets Co<br /><a href={"\""}mailto:sales@example.com{"\""}>sales@example.com</a></p>
</div>");

                IdentityMessage message = new IdentityMessage();
                message.Destination = order.Email;
                message.Subject = "Widgets Co - Order Confirmation";
                message.Body = html.ToString();

                EmailService es = new EmailService();
                await es.SendAsync(message);
            }
            catch { }
        }
    }
}