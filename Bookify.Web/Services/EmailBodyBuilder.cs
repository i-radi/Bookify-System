namespace Bookify.Web.Services
{
    public class EmailBodyBuilder : IEmailBodyBuilder
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailBodyBuilder(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public string GetEmailBody(string template, Dictionary<string, string> placeholders)
        {
            var filePath = $"{_webHostEnvironment.WebRootPath}/templates/{template}.html";
            StreamReader str = new(filePath);

            var templateContent = str.ReadToEnd();
            str.Close();

            foreach (var placeholder in placeholders)
                templateContent =
                    templateContent.Replace($"[{placeholder.Key}]", placeholder.Value);

            return templateContent;
        }
    }
}