using HealthCheck.Core.Modules.Documentation.Attributes;
using HealthCheck.Core.Modules.Documentation.Models.FlowCharts;

namespace HealthCheck.DevTest._TestImplementation.Diagrams
{
    #region Flow charts
    public class FlowLoginController
    {
        [FlowChartStep("Login", "Start", connection: "Login")]
        [FlowChartStep("Login", "Login", connection: "User exists?")]
        public void Login() { }
    }

    public class MyPageController
    {
        [FlowChartStep("Login", "Show My Page")]
        public void IndexAsync() { }
    }

    public class CRMService
    {
        [FlowChartStep("Login", "User exists?", connections: new[] {
            "Yes => Password is correct?",
            "No => Show error"
        })]
        [FlowChartStep("Login", "Password is correct?", type: FlowChartStepType.If, connections: new[] {
            "Yes => Redirect to mypage",
            "No => Show error"
        })]
        [FlowChartStep("Login", "Show error", connection: "Try Again => Login")]
        [FlowChartStep("Login", "Redirect to mypage", connection: "Show My Page")]
        public void UserExists() { }
    }
    #endregion

    #region Sequence Diagrams
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
    #endregion
}