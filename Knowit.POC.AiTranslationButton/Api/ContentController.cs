using System.Dynamic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Knowit.POC.AiTranslationButton.Api;

[ApiController]
[Route("api/ai")]
public class ContentController: ControllerBase
{
    private readonly IContentService _contentService;
    private readonly ILanguageService _languageService;

    public ContentController(IContentService contentService, ILanguageService languageService)
    {
        _contentService = contentService;
        _languageService = languageService;
    }

    [HttpGet("content-translate/{id:Guid}")]
    public async Task<ActionResult> TranslateContent(Guid id)
    {
        var translations = await GetContentToTranslate(id);
        if(translations.Count == 0)
        {
            return Ok(false);
        }

        var result = await AiClient.TranslateAsync(translations);
        var data = result.Choices.First().Message.TextContent;
        var json = JsonSerializer.Deserialize<IDictionary<string, IDictionary<string, string>>>(data);

        return Ok(UpdateContentWithTranslations(json, id));
    }

    [HttpGet("content-is-translatable/{id:Guid}")]
    public async Task<ActionResult> IsTranslatable(Guid id)
    {
        var cultures = (await _languageService.GetAllAsync()).Select(x => x.IsoCode).ToList();
        var result = new {IsTranslatable = false};

        if(cultures.Count < 2) // the site has at least one language with the default language
        {
            return Ok(result);
        }

        var content = _contentService.GetById(id);

        if(content is null)
        {
            return Ok(result);
        }

        var translations = await GetContentToTranslate(id);
        var isTranslatable = translations.Any(x => x.Value.Any(y => string.IsNullOrWhiteSpace(y.Value.ToString())));
        return Ok(new {IsTranslatable = isTranslatable});
    }

    private async Task<IDictionary<string, IDictionary<string, string>>> GetContentToTranslate(Guid id)
    {
        var translations = new Dictionary<string, IDictionary<string, string>>();

        var content = _contentService.GetById(id);
        if (content is null)
        {
            return translations;
        }

        var cultures = (await _languageService.GetAllAsync()).Select(x => x.IsoCode).ToList();

        foreach (var property in content.Properties.Where(x => x.PropertyType.VariesByCulture()))
        {
            foreach(var culture in cultures)
            {

                if (!translations.ContainsKey(culture))
                {
                    translations.Add(culture, new Dictionary<string, string>());
                    translations[culture].Add("Name", content.GetCultureName(culture) ?? "");
                }

                var value = content.GetValue<string>(property.Alias, culture) ?? "";
                if (value.Contains("{\"markup\":"))
                {
                    var json = JsonSerializer.Deserialize<Dictionary<string,object>>(value)!;
                    value = json["markup"].ToString();
                }

                translations[culture].Add(property.Alias, value!);
            }
        }

        return translations;
    }

    private bool UpdateContentWithTranslations(IDictionary<string, IDictionary<string, string>> translations,  Guid id)
    {
        var hasChanges = false;
        var content = _contentService.GetById(id);

        if (content is null)
        {
            return false;
        }

        var properties = content.Properties.Where(x => x.PropertyType.VariesByCulture()).ToList();

        foreach(var culture in translations.Keys)
        {
            var name = content.GetCultureName(culture);
            var translatedName = translations[culture]["Name"];
            if (name != translatedName)
            {
                hasChanges = true;
                content.SetCultureName(translatedName, culture);
            }

            foreach (var property in properties)
            {

                var value = content.GetValue<string>(property.Alias, culture) ?? "";
                if (string.IsNullOrWhiteSpace(value))
                {
                    var translatedValue = translations[culture][property.Alias];
                    if (value.Contains("{\"markup\":"))
                    {
                        var json = JsonSerializer.Deserialize<Dictionary<string,object>>(value)!;
                        json["markup"] = translatedValue;
                        value = JsonSerializer.Serialize(json);
                    }

                    if(translatedValue == value)
                    {
                        continue;
                    }

                    hasChanges = true;
                    content.SetValue(property.Alias, translatedValue, culture);
                }
            }
        }

        if (hasChanges)
        {
            _contentService.Save(content);
        }

        return hasChanges;

    }
}