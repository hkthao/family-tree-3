using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace backend.Web.Formatters;

public class HtmlInputFormatter : TextInputFormatter
{
    public HtmlInputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/html"));
        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }

    protected override bool CanReadType(Type type)
    {
        return type == typeof(string);
    }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
    {
        var httpContext = context.HttpContext;
        using var reader = new StreamReader(httpContext.Request.Body, encoding);

        try
        {
            var htmlContent = await reader.ReadToEndAsync();
            return await InputFormatterResult.SuccessAsync(htmlContent);
        }
        catch
        {
            return await InputFormatterResult.FailureAsync();
        }
    }
}
