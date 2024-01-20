namespace BookifyTest.Services
{
    public class EmailBodyBuilder : IEmailBodyBuilder
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailBodyBuilder(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public string GetEmailBody(string template, Dictionary<string, string> placehoders)
        {
            var filePath = $"{_webHostEnvironment.WebRootPath}/templates/{template}.html";
            StreamReader str = new(filePath);
            var bodyMessage = str.ReadToEnd();
            str.Close();

            foreach (var placehoder in placehoders)
            {
                bodyMessage = bodyMessage.Replace($"[{placehoder.Key}]", $"{placehoder.Value}");
            }


            return bodyMessage;
        }
    }
}
