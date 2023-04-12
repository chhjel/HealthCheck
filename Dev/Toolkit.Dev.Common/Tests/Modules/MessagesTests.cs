using QoDL.Toolkit.Core.Modules.Messages.Abstractions;
using QoDL.Toolkit.Core.Modules.Messages.Models;
using QoDL.Toolkit.Core.Modules.Tests.Attributes;
using QoDL.Toolkit.Core.Modules.Tests.Models;

namespace QoDL.Toolkit.Dev.Common.Tests.Modules;

[RuntimeTestClass(
    Name = "Messages",
    DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
    GroupName = RuntimeTestConstants.Group.Modules,
    UIOrder = 30
)]
public class MessagesTests
{
    private readonly ITKMessageStorage _messageStore;

    public MessagesTests(ITKMessageStorage messageStore)
    {
        _messageStore = messageStore;
    }

    [RuntimeTest]
    public TestResult AddSomeMessages(int sms = 8, int mail = 12)
    {
        for (int i = 0; i < sms; i++)
        {
            var msg = new TKDefaultMessageItem($"Some summary here #{i}", $"{i}345678", $"841244{i}", $"Some test message #{i} here etc etc.", false);
            if (i % 4 == 0)
            {
                msg.SetError("Failed to send because of server error.")
                    .AddNote("Mail not actually sent, devmode enabled etc.");
            }
            if (i % 2 == 0)
            {
                msg.AddNote("Mail not actually sent, devmode enabled etc.");
            }
            _messageStore.StoreMessage("sms", msg);
        }

        for (int i = 0; i < mail; i++)
        {
            var msg = new TKDefaultMessageItem($"Subject #{i}, totally not spam", $"test_{i}@somewhe.re", $"to@{i}mail.com",
                    $"<html>" +
                    $"<body>" +
                    $"<style>" +
                    $"div {{" +
                    $"display: inline-block;" +
                    $"font-size: 40px !important;" +
                    $"color: red !important;" +
                    $"}}" +
                    $"</style>" +
                    $"<h3>Super fancy contents here!</h3>Now <b>this</b> is a mail! #{i} or <div>something</div> <img src=\"https://picsum.photos/200\" />.'" +
                    $"</body>" +
                    $"</html>", true);
            if (i % 5 == 0)
            {
                msg.SetError("Failed to send because of invalid email.");
            }
            _messageStore.StoreMessage("mail", msg);
        }

        return TestResult.CreateSuccess($"Created {sms} sms + {mail} mail.");
    }
}
