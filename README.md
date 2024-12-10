# PDF Translation API  
  
This API allows users to translate PDF documents into a specified target language while maintaining the original layout and structure.  
  
## Architecture Overview  

![Alt Text](assets\architecture.png)

  
## Implementation Steps  
  
![Alt Text](assets\implementation_steps.png)
  
## Technologies Used  
  
- **OpenAI LLM GPT-4o** (version: 2024-08-06)  
- **.NET 7.0**  
  
## Value Adds  
  
- **Reusable and Extensible**  
- **Integration with EMR**  
  
## Note on Re-Generation  
  
Sometimes, re-generating the result may be necessary to select the best form.  
  
## Pre-requisites  
  
- Install .NET 7.0 or higher  
- Install VSCode  
- Install C# and Azure Extensions for VSCode  
- Ensure Azure Subscription and OpenAI  
  
## Getting Started  
  
1. **Clone the Repository**  
   ```bash  
   git clone https://github.com/yourusername/repository-name.git  
   ```
2. **Build the Solution**  
   ```bash  
   dotnet build  
   ```
3. **Build the Solution**  
   ```bash  
   dotnet run 
   ```  
4. **Accessing the Swagger UI**  
- Visit http://localhost:5000/swagger to upload PDFs and get translations.

### Steps to Get OpenAI API Key and Endpoint

Before proceeding, you need to obtain your **API Key** and **API Endpoint** from the Azure portal in order to configure your application.

1. **Sign in to Azure Portal**
   - Navigate to [Azure Portal](https://portal.azure.com/) and sign in with your credentials.

2. **Create or Access an OpenAI Resource**
   - In the search bar at the top, type **"OpenAI"** and select **Azure OpenAI Service**.
   - If you haven't already, click on **Create** to create a new OpenAI resource.
     - Choose the appropriate **Subscription**, **Resource Group**, and **Region**.
     - Under **Pricing tier**, select the one that fits your needs.
     - Complete the creation process.

3. **Get Your API Key**
   - Once your OpenAI resource is created, go to your OpenAI resource dashboard in the Azure portal.
   - In the left-hand menu, find and click on **Keys and Endpoint** under the **Resource Management** section.
   - Copy **Key 1** (or Key 2) and save it. This is your **API Key**.

4. **Get Your API Endpoint**
   - In the same **Keys and Endpoint** section, you will also see the **Endpoint** listed (e.g., `https://your-resource-name.openai.azure.com/`).
   - Copy the endpoint URL. This is your **API Endpoint**.

### Update Your `appsettings.json` and `appsettings.Development.json`

Once you have the **API Key** and **Endpoint**, you need to add these values to your `appsettings.json` or `appsettings.Development.json` file.

#### Example of `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AzureOpenAI": {
    "Endpoint": "https://your-resource-name.openai.azure.com/",
    "ApiKey": "your-api-key-here"
  }
}
```

## License
This project is licensed under the MIT License.

## Contact
For questions or comments, please contact [yaroslav@techiosoft.com].