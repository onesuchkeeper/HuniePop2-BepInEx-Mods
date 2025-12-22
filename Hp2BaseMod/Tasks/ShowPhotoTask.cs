using System;

namespace Hp2BaseMod.Tasks;

public class ShowPhotoTask
{
    private Action _onComplete;
    public ShowPhotoTask(RelativeId photoId, Action onComplete)
    {
        Game.Manager.Windows.WindowHideEvent += OnWindowHidden;
        Game.Manager.Windows.ShowWindow(ModInterface.State.UiWindowPhotos, false);
    }

    private void OnWindowHidden()
    {
        Game.Manager.Windows.WindowHideEvent -= OnWindowHidden;
        _onComplete?.Invoke();
        _onComplete = null;
    }
}