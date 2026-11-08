namespace Hp2BaseMod.Extension;

public static partial class UiCellphoneAppExtension
{
    public static void ErrorMessage(this UiCellphoneApp app, int messageIndex)
    {
        if (app.cellphone.phoneErrorMsg != null 
            && messageIndex >= 0 
            && messageIndex < app.errorMessages.Length)
        {
            app.cellphone.phoneErrorMsg.ShowMessage(app.errorMessages[messageIndex]);
        }
    }
}
