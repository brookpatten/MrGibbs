using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Http;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.App;
using Newtonsoft.Json;

using MrGibbs.Models;

namespace MrGibbs.AndroidViewer
{
    [Service(Name ="io.mrgibbs.androidviewer.dataservice")]
    public class DataService : Service
    {
        static readonly string TAG = typeof(DataService).FullName;

        private IBinder _binder;

        private Thread _worker;
        private bool _run = false;
        private PowerManager.WakeLock _wakeLock;

        private StateLite _state;

        public override IBinder OnBind(Intent intent)
        {
            _binder = new DataBinder(this);
            return _binder;
        }
        
        public override void OnCreate()
        {
            base.OnCreate();
            _run = true;
            _worker = new Thread(new ThreadStart(() =>
            {
                while(_run)
                {
                    if(Tick!=null)
                    {
                        Tick(this, _state);
                    }
                    Update();
                    Thread.Sleep(1000);
                }
            }));
            _worker.Start();

            PowerManager pm = (PowerManager)GetSystemService(Context.PowerService);
            _wakeLock = pm.NewWakeLock(WakeLockFlags.Full, DataService.TAG);
            _wakeLock.Acquire();
            
        }

        private void Update()
        {
            try
            {
                //connect to wifi network
                //wait for ip
                //connect to gibbs
                //request data
                //push updated data to UI

                var client = new HttpClient();
                var uri = new Uri(string.Format("http://192.168.42.1:9000/api/v1/state", string.Empty));

                var task = client.GetAsync(uri);
                task.Wait();
                var response = task.Result;
                if (response.IsSuccessStatusCode)
                {
                    var readTask = response.Content.ReadAsStringAsync();
                    readTask.Wait();
                    var content = readTask.Result;
                    _state = JsonConvert.DeserializeObject<StateLite>(content);
                }
            }
            catch
            {
                _state = null;
            }
        }

        public delegate void TickHandler(object sender, StateLite state);
        public event TickHandler Tick;

        public override bool OnUnbind(Intent intent)
        {
            return base.OnUnbind(intent);
        }

        public override void OnDestroy()
        {
            _wakeLock.Release();
            _run = false;
            _worker.Join();
            _binder = null;
            Tick = null;
            base.OnDestroy();
        }
    }

    public class DataBinder : Binder
    {
        public DataService DataService { get; private set; }

        public DataBinder(DataService service)
        {
            this.DataService = service;
        }

    }

    public class DataServiceConnection : Java.Lang.Object, IServiceConnection
    {
        static readonly string TAG = typeof(DataServiceConnection).FullName;

        MainActivity mainActivity;
        public DataServiceConnection(MainActivity activity)
        {
            IsConnected = false;
            Binder = null;
            mainActivity = activity;
        }

        public bool IsConnected { get; private set; }
        public DataBinder Binder { get; private set; }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            Binder = service as DataBinder;
            IsConnected = this.Binder != null;

            
            if (IsConnected)
            {
                //mainActivity.UpdateUiForBoundService();
            }
            else
            {
                //mainActivity.UpdateUiForUnboundService();
            }

            //mainActivity.timestampMessageTextView.Text = message;
            Binder.DataService.Tick += DataService_Tick;

        }

        private void DataService_Tick(object sender, StateLite state)
        {
            mainActivity.Update(state);
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            IsConnected = false;
            Binder = null;
            //mainActivity.UpdateUiForUnboundService();
        }
    }
}