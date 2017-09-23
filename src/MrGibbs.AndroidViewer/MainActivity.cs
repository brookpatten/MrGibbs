using System;

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
            this.Window.AddFlags(Android.Views.WindowManagerFlags.Fullscreen);
            this.Window.AddFlags(Android.Views.WindowManagerFlags.KeepScreenOn);

            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
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

        private void SetRow(int index,string name,string value)
        {
            TextView nameView;
            TextView valueView;

            if(index==0)
            {
                nameView = FindViewById<TextView>(Resource.Id.row1Name);
                valueView = FindViewById<TextView>(Resource.Id.row1Value);
            }
            else if (index == 1)
            {
                nameView = FindViewById<TextView>(Resource.Id.row2Name);
                valueView = FindViewById<TextView>(Resource.Id.row2Value);
            }
            else if (index == 2)
            {
                nameView = FindViewById<TextView>(Resource.Id.row3Name);
                valueView = FindViewById<TextView>(Resource.Id.row3Value);
            }
            else
            {
                throw new IndexOutOfRangeException();
            }

            nameView.Post(() => {
                nameView.Text = name;
            });
            valueView.Post(() =>
            {
                valueView.Text = value;
            });
        }

        private void SetRows(params Tuple<string,string>[] rows)
        {
            for(int i=0;i<rows.Length;i++)
            {
                SetRow(i, rows[i].Item1, rows[i].Item2);
            }
        }

        public void Update(StateLite state)
        {
            if(state==null)
            {
                var nothing = new Tuple<string, string>("ND", "000.0");
                SetRows(nothing, nothing, nothing);
                return;
            }
            if (!state.RaceStarted)
            {
                //start mode
                if (state.Countdown.HasValue)
                {
                    //countdown
                    SetRow(0, "Start", state.Countdown.Value.Minutes + ":" + state.Countdown.Value.Seconds.ToString("00"));
                }
                else
                {
                    SetRow(0, "Start", "");
                }

                //cog
                if (state.StateValues.ContainsKey(StateValue.CourseOverGroundDirection))
                {
                    SetRow(1, "cog", $"{state.StateValues[StateValue.CourseOverGroundDirection]:0.0}");
                }
                else
                {
                    SetRow(1, "cog", "?");
                }

                //speed
                if (state.StateValues.ContainsKey(StateValue.SpeedInKnots))
                {
                    SetRow(2, "kts", $"{state.StateValues[StateValue.SpeedInKnots]:0.0}");
                }
                else
                {
                    SetRow(2, "kts", "?");
                }
            }
            else
            {
                //race mode
                //cog
                if (state.StateValues.ContainsKey(StateValue.CourseOverGroundDirection))
                {
                    SetRow(0, "cog", $"{state.StateValues[StateValue.CourseOverGroundDirection]:0.0}");
                }
                else
                {
                    SetRow(0, "cog", "?");
                }

                //speed
                if (state.StateValues.ContainsKey(StateValue.SpeedInKnots))
                {
                    SetRow(1, "kts", $"{state.StateValues[StateValue.SpeedInKnots]:0.0}");
                }
                else
                {
                    SetRow(1, "kts", "?");
                }

                //tactical speed
                if (state.StateValues.ContainsKey(StateValue.VelocityMadeGoodOnCoursePercent))
                {
                    SetRow(2, "vmc%", $"{state.StateValues[StateValue.VelocityMadeGoodOnCoursePercent]:0.0}");
                }
                else if (state.StateValues.ContainsKey(StateValue.VelocityMadeGoodOnCourse))
                {
                    SetRow(2, "vmc", $"{state.StateValues[StateValue.VelocityMadeGoodOnCourse]:0.0}");
                }
                else if (state.StateValues.ContainsKey(StateValue.VelocityMadeGoodPercent))
                {
                    SetRow(2, "vmg%", $"{state.StateValues[StateValue.VelocityMadeGoodPercent]:0.0}");
                }
                else if(state.StateValues.ContainsKey(StateValue.VelocityMadeGood))
                {
                    SetRow(2, "vmg", $"{state.StateValues[StateValue.VelocityMadeGood]:0.0}");
                }
                else if (state.StateValues.ContainsKey(StateValue.CurrentTackCourseOverGroundDelta))
                {
                    SetRow(2, "tackΔ", $"{state.StateValues[StateValue.CurrentTackCourseOverGroundDelta]:0.0}");
                }
                else
                {
                    SetRow(2, "", "");
                }
            }

            //var imageView = this.FindViewById<ImageView>(Resource.Id.imageView);
            //imageView.Post(() =>
            //{
            //    var rect = new Rect();
            //    imageView.GetDrawingRect(rect);
            //    Bitmap tempBitmap = Bitmap.CreateBitmap(200, 200, Bitmap.Config.Rgb565);
                
            //    Canvas tempCanvas = new Canvas(tempBitmap);
            //    Paint paint = new Paint();
            //    paint.Color = Color.White;
            //    //Draw everything else you want into the canvas, in this example a rectangle with rounded edges
            //    tempCanvas.DrawRoundRect(new RectF(0, 0, 100, 100), 2, 2, paint);
                
            //    //Attach the canvas to the ImageView
            //    //imageView.SetImageDrawable(new BitmapDrawable(tempBitmap));

            //    imageView.SetImageBitmap(tempBitmap);
                
            //});
        }
    }
}

