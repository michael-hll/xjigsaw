using System;
using System.Collections.Generic;
using MessageUI;
using UIKit;
using XJigsaw.iOS;
using XJigsaw.Models;

[assembly: Xamarin.Forms.Dependency(typeof(EmailService))]
namespace XJigsaw.iOS
{
    public class EmailService : IEmailService
    {
        public void CreateEmail(List<string> emailAddresses, List<string> ccs, string subject, string body, string htmlBody)
        {
            var vc = new MFMailComposeViewController();
            vc.MailComposeDelegate = (IMFMailComposeViewControllerDelegate)this;

            if (emailAddresses?.Count > 0)
            {
                vc.SetToRecipients(emailAddresses.ToArray());
            }

            if (ccs?.Count > 0)
            {
                vc.SetCcRecipients(ccs.ToArray());
            }

            vc.SetSubject(subject);
            vc.SetMessageBody(htmlBody, true);
            vc.Finished += (sender, e) =>
            {
                vc.DismissModalViewController(true);
            };

            UIApplication.SharedApplication.Windows[0].
                RootViewController.PresentViewController(vc, true, null);

        }
    }
}
