namespace EmailSenderProject.Models
{
    public class EmailViewModel
    {
        public List<string> EmailAddresses { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class EmailWithAttachmentVM : EmailViewModel
    {
        public IFormFileCollection Attachments { get; set; }
    }
}
