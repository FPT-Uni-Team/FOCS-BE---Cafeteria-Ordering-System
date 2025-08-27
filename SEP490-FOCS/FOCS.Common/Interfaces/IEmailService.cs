using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FOCS.Common.Models;

namespace FOCS.Common.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetAsync(ResetPasswordRequest resetPasswordRequest);

        Task<bool> SendPasswordResetLinkAsync(string email, string callbackUrl, string storeId, string tableId);

        Task<bool> SendEmailConfirmationAsync(string email, string accToken, string storeId, string tableId);

    }
}
