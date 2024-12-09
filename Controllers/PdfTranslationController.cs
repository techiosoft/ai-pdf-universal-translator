using Microsoft.AspNetCore.Mvc;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using PDFtoImage;
using OpenHtmlToPdf;
using PdfTranslationApi.Models;

namespace PdfTranslationApi.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class PdfTranslationController: ControllerBase {
		private readonly ILogger < PdfTranslationController > _logger;
		private readonly AzureOpenAIClient _client;
		public PdfTranslationController(ILogger < PdfTranslationController > logger, IConfiguration configuration) {
				_logger = logger;
				// Retrieve AzureOpenAI configuration values from appsettings.json
				var endpoint = configuration["AzureOpenAI:Endpoint"];
                var apiKey = configuration["AzureOpenAI:ApiKey"];
				_client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
			}
			[HttpPost("translate")]
		public async Task < IActionResult > TranslatePdf(IFormFile pdfFile, [FromForm] SupportedLanguage targetLanguage) {
			if(pdfFile == null || pdfFile.Length == 0) return BadRequest("Invalid PDF file.");
			string originalFileNameWithoutExtension = Path.GetFileNameWithoutExtension(pdfFile.FileName);
			string fileExtension = Path.GetExtension(pdfFile.FileName);
			string newFileName = $"{originalFileNameWithoutExtension}_translated_{targetLanguage}{fileExtension}";
			using(var inputPdfStream = new MemoryStream()) {
				await pdfFile.CopyToAsync(inputPdfStream);
				inputPdfStream.Position = 0;
				using(var outputPdfStream = new MemoryStream()) {
					await TranslatePdfDocument(inputPdfStream, outputPdfStream, targetLanguage.ToString());
					outputPdfStream.Position = 0;
					return File(outputPdfStream.ToArray(), "application/pdf", newFileName);
				}
			}
		}
		private async Task < string > TranslateImage(MemoryStream imageStream, string targetLanguage) {
			byte[] imageBytesArray = imageStream.ToArray();
			BinaryData imageBinaryData = new BinaryData(imageBytesArray);  
			var prompt = $@"  
			You are the helpful assistant that generates the full HTML based on provided image.

- Align form fields and their labels on the same line wherever possible.  
- Use `colspan` to merge cells when necessary, but ensure the overall table width is consistent.  
- Ensure that each row maintains the overall width by filling in any blank spaces with empty cells that have borders. 
- Identify the logo and place it in the recreated HTML in the same location. Use the following `img src`: 
 (https://raw.githubusercontent.com/yaroslav-techiosoft/test-images/refs/heads/main/registration-for-best-beginnings-is-easy-14545-figures-1.1.png).
- Ensure Footer is transferred to HTML
- Translate every word to {targetLanguage} in HTML output
- It is very sensitive and critical to retain every word in HTML output after translation
			";  
			var chatClient = _client.GetChatClient("gpt-4o");
			var options = new ChatCompletionOptions()
            {
                MaxOutputTokenCount = 16384,
				Temperature = (float?)0.7,
				TopP = (float?)0.95,
                FrequencyPenalty = 0,
                PresencePenalty = 0,
            };
			ChatCompletion completion = await chatClient.CompleteChatAsync(new List < ChatMessage > {
				new SystemChatMessage(prompt),
				//new UserChatMessage(ChatMessageContentPart.CreateImagePart(new Uri("https://github.com/Yaroslav-Morkvin-telus/test-image-health/blob/main/page-1.jpg?raw=true"), "auto"))
				new UserChatMessage(ChatMessageContentPart.CreateImagePart(imageBinaryData,"image/png", "auto"))
			}, options);
			return completion.Content[0].Text;
		}
		private async Task TranslatePdfDocument(Stream inputPdfStream, Stream outputPdfStream, string targetLanguage) {
			List < MemoryStream > imageStreams = ConvertPdfToImages(inputPdfStream);
			var translatedHtmlContents = new List < string > ();
			foreach(var imageStream in imageStreams) {
				string translatedHtml = await TranslateImage(imageStream, targetLanguage);
				int startIndex = translatedHtml.IndexOf('<');
				int endIndex = translatedHtml.LastIndexOf('>');
				if(startIndex != -1 && endIndex != -1 && startIndex < endIndex) {
					translatedHtml = translatedHtml.Substring(startIndex, endIndex - startIndex + 1);
				}
				translatedHtmlContents.Add(translatedHtml);
			}
			string fullHtmlContent = GenerateFullHtml(translatedHtmlContents);
			ConvertHtmlToPdf(fullHtmlContent, outputPdfStream);
		}
		private List < MemoryStream > ConvertPdfToImages(Stream pdfStream) {
			List < MemoryStream > imageStreams = new List < MemoryStream > ();
			byte[] pdfBytes = new byte[pdfStream.Length];
			pdfStream.Read(pdfBytes, 0, pdfBytes.Length);
			for(int i = 0; i < PDFtoImage.Conversion.GetPageCount(new MemoryStream(pdfBytes)); i++) {
				var imageStream = new MemoryStream();
				using(var tempPdfStream = new MemoryStream(pdfBytes)) {
					PDFtoImage.Conversion.SavePng(imageStream, tempPdfStream, i, leaveOpen: true, options: new(Dpi: 300));
				}
				imageStream.Position = 0;
				imageStreams.Add(imageStream);
			}
			return imageStreams;
		}
		private string GenerateFullHtml(List < string > translatedHtmlContents) {
			string htmlContent = "<html><body>";
			foreach(var htmlSnippet in translatedHtmlContents) {
				htmlContent += htmlSnippet;
			}
			htmlContent += "</body></html>";
			return htmlContent;
		}
		private void ConvertHtmlToPdf(string htmlContent, Stream outputPdfStream) {
			byte[] pdfBytes = Pdf.From(htmlContent).Content();
			outputPdfStream.Write(pdfBytes, 0, pdfBytes.Length);
			outputPdfStream.Position = 0;
		}
	}
}