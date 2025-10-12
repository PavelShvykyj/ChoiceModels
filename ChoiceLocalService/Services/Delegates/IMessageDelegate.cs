namespace ChoiceLocalService.Services.Delegates;

public interface IMessageDelegate
{
    Task<bool> HandleAsync(string messageBody);
}

