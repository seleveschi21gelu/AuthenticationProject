using EmailSenderProject.Interfaces;
using EmailSenderProject.Models;
using Microsoft.AspNetCore.Mvc;

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
        public ActionResult SendEmail(EmailViewModel model)
        {
            if (!ModelState.IsValid)
                BadRequest();

            var message = new Message(model.EmailAddresses, model.Subject, model.Body);
            var result = _emailSender.SendEmail(message);

            return result ? Ok() : NoContent();
        }
    }
}
