﻿using HealthCheck.Core.Modules.Diagrams.SequenceDiagrams;

namespace HealthCheck.DevTest._TestImplementation.Diagrams
{
    public class CheckoutController
    {
        [SequenceDiagramStep("Payment", "Web")]
        [SequenceDiagramStep("Payment", "Nets", "User redirected to payment terminal")]
        [SequenceDiagramStep("Payment", "Nets", "User completes payment",
            nextClass: nameof(PaymentCallbackController), nextMethod: nameof(PaymentCallbackController.NetsCallback))]
        public void InitiatePayment() { }
    }

    public class PaymentCallbackController
    {
        [SequenceDiagramStep("Payment", "Web", "Redirected back to website")]
        [SequenceDiagramStep("Payment", "File", "Create order file on disk")]
        [SequenceDiagramStep("Payment", "Visma", "Visma reads file", branches: new[] { "Payment - Invoice" })]
        [SequenceDiagramStep("Payment", "File", "Visma creates response file", branches: new[] { "Payment - Invoice" })]
        [SequenceDiagramStep("Payment", "OrderService", "Service handles response file", note: "Test note", remarks: "Service runs as a windows service on prod server. The service project can be found here: ")]
        [SequenceDiagramStep("Payment", "Web", "Service POSTS results to Web",
            nextClass: nameof(OrderConfirmationController), nextMethod: nameof(OrderConfirmationController.Index))]
        public void NetsCallback() { }
    }

    public class OrderConfirmationController
    {
        [SequenceDiagramStep("Payment", "Web", "Redirected to order confirmation page", remarks: "Endpoint: /etc/OrderConfirmation")]
        public void Index() { }
    }

    public class LoginController
    {
        [SequenceDiagramStep("Login", "Frontend")]
        [SequenceDiagramStep("Login", "Web", "User enters details")]
        [SequenceDiagramStep("Login", "OBOS IT", "Login is checked against OBOS IT endpoint")]
        [SequenceDiagramStep("Login", "Web", "Responds with 200", optionalGroupName: "Successfull login")]
        [SequenceDiagramStep("Login", "Frontend", "Redirected to MyPage", optionalGroupName: "Successfull login")]
        [SequenceDiagramStep("Login", "Web", "Responds with 400", optionalGroupName: "Wrong login")]
        [SequenceDiagramStep("Login", "Frontend", "Shown error", optionalGroupName: "Wrong login")]
        public void Login() { }
    }
}