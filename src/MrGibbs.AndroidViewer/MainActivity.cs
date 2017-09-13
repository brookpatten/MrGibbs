using Android.App;
using Android.Widget;
using Android.OS;
using Android.Util;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;

using MrGibbs.Models;

namespace MrGibbs.AndroidViewer
{
    [Activity(Label = "MrGibbs.AndroidViewer", MainLauncher = true, Icon = "@mipmap/icon",ScreenOrientation =Android.Content.PM.ScreenOrientation.Landscape)]
    public class MainActivity : Activity
    {
        //ViewSwitcher _switcher;
        DataServiceConnection _serviceConnection;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            this.RequestWindowFeature(Android.Views.WindowFeatures.NoTitle);
            //this.Window.AddFlags(Android.Views.WindowManagerFlags.Fullscreen);
            this.Window.AddFlags(Android.Views.WindowManagerFlags.KeepScreenOn);

            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            //_switcher = new ViewSwitcher(this);


            //IAttributeSet
            //var lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
            //_switcher.AddView(new RaceView(this,lp))

            // Get our button from the layout resource,
            // and attach an event to it
            //Button button = FindViewById<Button>(Resource.Id.myButton);

            //button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };

            

        }

        void DoBindService()
        {
            Intent serviceToStart = new Intent(this, typeof(DataService));
            BindService(serviceToStart, _serviceConnection, Bind.AutoCreate);
        }

        void DoUnBindService()
        {
            UnbindService(_serviceConnection);
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (_serviceConnection == null)
            {
                _serviceConnection = new DataServiceConnection(this);
            }
            DoBindService();
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (_serviceConnection.IsConnected)
            {
                //UpdateUiForBoundService();
            }
            else
            {
                //UpdateUiForUnboundService();
            }
        }

        public void Update(StateLite state)
        {
            var row1 = this.FindViewById<TextView>(Resource.Id.row1Value);
            row1.Post(() =>
            {
                if(state.StateValues.ContainsKey(StateValue.SpeedInKnots))
                {
                    row1.Text = $"{state.StateValues[StateValue.SpeedInKnots]:0.0}";
                }

                if(state.Countdown.HasValue && !state.RaceStarted)
                {
                    //start mode
                }
                else
                {
                    //race mode
                }
            });

            var imageView = this.FindViewById<ImageView>(Resource.Id.imageView);
            imageView.Post(() =>
            {
                var rect = new Rect();
                imageView.GetDrawingRect(rect);
                Bitmap tempBitmap = Bitmap.CreateBitmap(200, 200, Bitmap.Config.Rgb565);
                
                Canvas tempCanvas = new Canvas(tempBitmap);
                Paint paint = new Paint();
                paint.Color = Color.White;
                //Draw everything else you want into the canvas, in this example a rectangle with rounded edges
                tempCanvas.DrawRoundRect(new RectF(0, 0, 100, 100), 2, 2, paint);
                
                //Attach the canvas to the ImageView
                //imageView.SetImageDrawable(new BitmapDrawable(tempBitmap));

                imageView.SetImageBitmap(tempBitmap);
                
            });
        }
    }
}

