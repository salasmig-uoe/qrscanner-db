using System.Collections.Generic;
using QRScanner.ViewModel;

namespace QRScanner.Pages;

public partial class MainScanPage : ContentPage
{
    public MainScanPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
    void OnSwiped(object sender, SwipedEventArgs e)
    {
        var label_item = (Label)sender;

        switch (e.Direction)
        {
            case SwipeDirection.Left:
                break;
            case SwipeDirection.Right:

                Dispatcher.DispatchAsync(async () =>
                {
                bool result = await DisplayAlert("Do you want to delete the item ", label_item.Text, "OK", "Cancel");
                    if (result)
                    {
                        Dispatcher.DispatchAsync(async () =>
                        {
                            await DisplayAlert(label_item.Text, " deleted ", "OK");
                        });
                    }
                    else
                    {
                        Dispatcher.DispatchAsync(async () =>
                        {
                            await DisplayAlert("Operation Canceled", "", "OK");
                        });

                    }
                });
                break;
            case SwipeDirection.Up:
                break;
            case SwipeDirection.Down:
                break;
        }
    }
}