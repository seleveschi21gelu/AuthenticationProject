using EmailSenderProject.Interfaces;
using EmailSenderProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EmailSenderProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        readonly IEmailSender _emailSender;

        public EmailController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        [HttpPost]
        [Route("sendEmail")]
        public ActionResult SendEmail(EmailViewModel model)
        {
            if (!ModelState.IsValid)
                BadRequest();

            var message = new Message(model.EmailAddresses, model.Subject, model.Body);
            var result = _emailSender.SendEmail(message);
            
            return result ? Ok(result) : BadRequest(result);
        }

        [HttpPost]
        [Route("sendEmailAsync")]
        public async Task<ActionResult<bool>> SendEmailAsync(EmailViewModel model)
        {
            if (!ModelState.IsValid)
                BadRequest();

            var message = new Message(model.EmailAddresses, model.Subject, model.Body);
            var result = await _emailSender.SendEmailAsync(message);

            return result ? Ok(result) : BadRequest(result);
        }
    }
}
