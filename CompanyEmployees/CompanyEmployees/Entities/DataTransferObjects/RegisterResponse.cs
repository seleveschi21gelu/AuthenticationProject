namespace CompanyEmployees.Entities.DataTransferObjects
{
    public class RegisterResponse
    {
        public bool IsSuccessfulRegistration { get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }
}
