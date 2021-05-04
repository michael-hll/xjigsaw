using System;
using System.Collections.Generic;

namespace XJigsaw.Models
{
    public interface IEmailService
    {
        void CreateEmail(List<string> emailAddresses, List<string> ccs, string subject, string body, string htmlBody);
    }
}
