using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TipTrip.Common.Models;

namespace TipTrip.Common.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetAsync(ResetPasswordRequest resetPasswordRequest);

        Task<bool> SendPasswordResetLinkAsync(string email, string callbackUrl);

        Task<bool> SendEmailConfirmationAsync(string email, string accToken);

    }
}
