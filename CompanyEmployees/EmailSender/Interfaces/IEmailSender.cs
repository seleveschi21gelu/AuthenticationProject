﻿using EmailSenderProject.Models;

namespace EmailSenderProject.Interfaces
{
    public interface IEmailSender
    {
        bool SendEmail(Message message);
        Task<bool> SendEmailAsync(Message message);
    }
}
