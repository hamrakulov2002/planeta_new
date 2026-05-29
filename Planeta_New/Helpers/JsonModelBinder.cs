using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Planeta_New.Helpers;

public class JsonModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
            throw new ArgumentNullException(nameof(bindingContext));

        // Получаем значение поля по его имени (в нашем случае "Attributes")
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;
        if (string.IsNullOrEmpty(value))
        {
            return Task.CompletedTask;
        }

        try
        {
            // Десериализуем JSON-строку прямо в целевой тип (List<ProductAttributeInputDto>)
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize(value, bindingContext.ModelType, options);
            
            bindingContext.Result = ModelBindingResult.Success(result);
        }
        catch (JsonException)
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Некорректный формат JSON для листов.");
        }

        return Task.CompletedTask;
    }
}