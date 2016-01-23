using PebbleSharp.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DBus;
using Mono.BlueZ.DBus;
using org.freedesktop.DBus;
using System.IO;

namespace PebbleSharp.Net45
{
    public class PebbleNet45 : Pebble
    {
		private static Bus _system;
		//private Bus _session;
		public static Exception _startupException{ get; private set; }
		private static ManualResetEvent _started = new ManualResetEvent(false);
		private static Thread _dbusLoop;
		private static bool _run;

		const string Service = "org.bluez";
		private static ObjectPath AgentPath = new ObjectPath ("/agent");
		private static ObjectPath ProfilePath = new ObjectPath ("/profiles");
		private static ObjectPath BlueZPath = new ObjectPath ("/org/bluez");
		const string PebbleSerialUUID = "00000000-deca-fade-deca-deafdecacaff";

		private static ObjectManager ObjectManager;
		private static ProfileManager1 ProfileManager;
		private static PebbleProfile Profile;
		private static AgentManager1 AgentManager;
		private static PebbleAgent Agent;
		private static Adapter1 Adapter;
		private static Dictionary<ObjectPath,DiscoveredPebble> Pebbles;

		private static void Startup()
		{
			Pebbles = new Dictionary<ObjectPath, DiscoveredPebble> ();

			// Run a message loop for DBus on a new thread.
			_run = true;
			_dbusLoop = new Thread(DBusLoop);
			_dbusLoop.IsBackground = true;
			_dbusLoop.Start();
			_started.WaitOne(60 * 1000);
			_started.Close();
			if (_startupException != null) 
			{
				throw _startupException;
			}
			else 
			{
				System.Console.WriteLine ("Bus connected at " + _system.UniqueName);
			}
		}

		private static void DBusLoop()
		{
			try 
			{
				_system=Bus.System;
			} 
			catch (Exception ex) 
			{
				_startupException = ex;
				return;
			} 
			finally 
			{
				_started.Set();
			}

			while (_run) 
			{
				_system.Iterate();
			}
		}

		private static void Shutdown()
		{
			AgentManager.UnregisterAgent (AgentPath);
			ProfileManager.UnregisterProfile (ProfilePath);

			_run = false;
			try
			{
				_dbusLoop.Join(1000);
			}
			catch 
			{
				try 
				{
					_dbusLoop.Abort ();
				} 
				catch 
				{
				}
			}
		}

		public static IList<Pebble> Detect(string adapterName,bool doDiscovery)
		{
			Startup ();

			//these properties are defined by bluez in /doc/profile-api.txt
			//but it turns out the defaults work just fine
			var properties = new Dictionary<string,object> ();
			//properties ["AutoConnect"] = true;
			//properties ["Name"] = "Serial Port";
			//properties ["Service"] = pebbleSerialUUID;
			//properties ["Role"] = "client";
			//properties ["PSM"] = (ushort)1;
			//properties ["RequireAuthentication"] = false;
			//properties ["RequireAuthorization"] = false;
			//properties ["Channel"] = (ushort)0;

			//get a proxy for the profile manager so we can register our profile
			ProfileManager =  _system.GetObject<ProfileManager1> (Service, BlueZPath);
			//create and register our profile
			Profile = new PebbleProfile ();
			_system.Register (ProfilePath, Profile);
			ProfileManager.RegisterProfile (ProfilePath
				, PebbleSerialUUID
				, properties);
			Profile.NewConnectionAction=(path,stream,props)=>{
				if(Pebbles.ContainsKey(path))
				{
					Pebbles[path].FileDescriptor = stream;
				}
			};

			//get a copy of the object manager so we can browse the "tree" of bluetooth items
			PebbleNet45.ObjectManager = _system.GetObject<org.freedesktop.DBus.ObjectManager> (Service, ObjectPath.Root);
			//register these events so we can tell when things are added/removed (eg: discovery)
			PebbleNet45.ObjectManager .InterfacesAdded += (p, i) => {
				System.Console.WriteLine (p + " Discovered");
			};
			PebbleNet45.ObjectManager .InterfacesRemoved += (p, i) => {
				System.Console.WriteLine (p + " Lost");
			};

			//get the agent manager so we can register our agent
			AgentManager = _system.GetObject<AgentManager1> (Service, BlueZPath);
			Agent = new PebbleAgent();
			//register our agent and make it the default
			_system.Register (AgentPath, Agent);
			AgentManager.RegisterAgent (AgentPath, "KeyboardDisplay");
			AgentManager.RequestDefaultAgent (AgentPath);

			try
			{
				//get the bluetooth object tree
				var managedObjects = PebbleNet45.ObjectManager.GetManagedObjects();
				//find our adapter
				ObjectPath adapterPath = null;
				foreach (var obj in managedObjects.Keys) {
					if (managedObjects [obj].ContainsKey (typeof(Adapter1).DBusInterfaceName ())) {
						if (string.IsNullOrEmpty(adapterName) || obj.ToString ().EndsWith (adapterName)) {
							adapterPath = obj;
							break;
						}
					}
				}

				if (adapterPath == null) {
					throw new ArgumentException("Could not find bluetooth adapter");
				}

				//get a dbus proxy to the adapter
				Adapter = _system.GetObject<Adapter1> (Service, adapterPath);

				if(doDiscovery)
				{
					//scan for any new devices
					Adapter.StartDiscovery ();
					Thread.Sleep(5000);//totally arbitrary constant, the best kind
					//Thread.Sleep ((int)adapter.DiscoverableTimeout * 1000);

					//refresh the object graph to get any devices that were discovered
					//arguably we should do this in the objectmanager added/removed events and skip the full
					//refresh, but I'm lazy.
					managedObjects = PebbleNet45.ObjectManager.GetManagedObjects();
				}

				foreach (var obj in managedObjects.Keys) {
					if (obj.ToString ().StartsWith (adapterPath.ToString ())) {
						if (managedObjects [obj].ContainsKey (typeof(Device1).DBusInterfaceName ())) {

							var managedObject = managedObjects [obj];
							var name = (string)managedObject[typeof(Device1).DBusInterfaceName()]["Name"];

							if (name.StartsWith ("Pebble")) {
								System.Console.WriteLine ("Device " + name + " at " + obj);
								var device = _system.GetObject<Device1> (Service, obj);

								try{
									if (!device.Paired) {
										device.Pair ();
									}
									if (!device.Trusted) {
										device.Trusted=true;
									}

									Pebbles[obj]=new DiscoveredPebble(){Name=name,Device = device};

									device.ConnectProfile(PebbleSerialUUID);
								}
								catch(Exception ex)
								{
								}
							}
						}
					}
				}
				//wait for devices to connect
				Thread.Sleep(2000);

				var results = new List<Pebble>();
				foreach(var pebble in Pebbles.Keys)
				{
					if(Pebbles[pebble].FileDescriptor!=null)
					{
						Pebbles[pebble].FileDescriptor.SetBlocking();
						var stream = Pebbles[pebble].FileDescriptor.OpenAsStream(true);
						results.Add(new PebbleNet45(new PebbleBluetoothConnection(stream),Pebbles[pebble].Name));

					}
				}
				return results;
			}
			catch 
			{
				AgentManager.UnregisterAgent (AgentPath);
				ProfileManager.UnregisterProfile (ProfilePath);
				throw;
			}
		}

		private class DiscoveredPebble
		{
			public Device1 Device{ get; set; }
			public FileDescriptor FileDescriptor{get;set;}
			public string Name{get;set;}
		}

        private PebbleNet45( PebbleBluetoothConnection connection, string pebbleId )
            : base( connection, pebbleId )
        { }

		private class PebbleAgent:Agent1
		{
			public PebbleAgent()
			{
			}
			public void Release()
			{
			}
			public string RequestPinCode(ObjectPath device)
			{
				return "1";
			}
			public void DisplayPinCode(ObjectPath device,string pinCode)
			{
			}
			public uint RequestPasskey(ObjectPath device)
			{
				return 1;
			}
			public void DisplayPasskey (ObjectPath device, uint passkey, ushort entered)
			{
			}
			public void RequestConfirmation(ObjectPath device,uint passkey)
			{
			}
			public void RequestAuthorization(ObjectPath device)
			{
			}
			public void AuthorizeService(ObjectPath device,string uuid)
			{
			}
			public void Cancel()
			{
			}
		}

		private class PebbleProfile:Profile1
		{
			private FileDescriptor _fileDescriptor;

			public Action<ObjectPath,FileDescriptor,IDictionary<string,object>> NewConnectionAction{get;set;}
			public Action<ObjectPath,FileDescriptor> RequestDisconnectionAction{ get; set; }
			public Action<FileDescriptor> ReleaseAction{ get; set; }

			public PebbleProfile ()
			{
			}

			public void Release ()
			{
				if (ReleaseAction != null) {
					ReleaseAction (_fileDescriptor);
				}
			}
			public void NewConnection (ObjectPath device, FileDescriptor fileDescriptor, IDictionary<string,object> properties)
			{
				_fileDescriptor = fileDescriptor;
				if (NewConnectionAction != null) {
					NewConnectionAction (device, _fileDescriptor, properties);
				}
			}
			public void RequestDisconnection (ObjectPath device)
			{
				if (RequestDisconnectionAction != null) {
					RequestDisconnectionAction (device, _fileDescriptor);
				} else {
					if (_fileDescriptor != null) {
						_fileDescriptor.Close ();
					}
				}
			}
		}

        private sealed class PebbleBluetoothConnection : IBluetoothConnection, IDisposable
        {
            private CancellationTokenSource _tokenSource;

            private Stream _networkStream;
            public event EventHandler<BytesReceivedEventArgs> DataReceived = delegate { };

			public PebbleBluetoothConnection( Stream Stream)
            {
				_networkStream=Stream;
                //_deviceInfo = deviceInfo;
            }

            ~PebbleBluetoothConnection()
            {
                Dispose( false );
            }

            public Task OpenAsync()
            {
                return Task.Run( () =>
                {
                    _tokenSource = new CancellationTokenSource();
                    //_client = new BluetoothClient();
                    //Console.WriteLine("Connecting BluetoothClient");
                    //_client.Connect( _deviceInfo.DeviceAddress, BluetoothService.SerialPort );
                    //Console.WriteLine("Getting network stream");
                    //_networkStream = _client.GetStream();
                    //Console.WriteLine("Checking for Data");
                    Task.Factory.StartNew( CheckForData, _tokenSource.Token, TaskCreationOptions.LongRunning,
                        TaskScheduler.Default );
                } );
            }

            public void Close()
            {
                if (_tokenSource != null)
                {
                    _tokenSource.Cancel();
                }
                
                //if ( _client.Connected )
                //{
                    //_client.Close();
                //}
            }

            public void Write( byte[] data )
            {
                if ( _networkStream.CanWrite )
                {
                    _networkStream.Write( data, 0, data.Length );
                }
            }

            public void Dispose()
            {
                Dispose( true );
                GC.SuppressFinalize( this );
            }

            private void Dispose( bool disposing )
            {
                if ( disposing )
                {
                    Close();
                }
            }

            private async void CheckForData()
            {
                try
                {
                    while ( true )
                    {
                        if ( _tokenSource.IsCancellationRequested )
                            return;

                        if ( _networkStream.CanRead /*&& _networkStream.DataAvailable*/ )
                        {
                            var buffer = new byte[256];
                            var numRead = _networkStream.Read( buffer, 0, buffer.Length );
                            Array.Resize( ref buffer, numRead );
                            DataReceived( this, new BytesReceivedEventArgs( buffer ) );
                        }

                        if ( _tokenSource.IsCancellationRequested )
                            return;
                        await Task.Delay( 10 );
                    }
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                }
            }
        }
    }
}